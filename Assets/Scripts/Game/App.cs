using System;
using JellyDrop.Audio;
using JellyDrop.Core;
using JellyDrop.Core.Economy;
using JellyDrop.GameInput;
using JellyDrop.Gameplay;
using JellyDrop.Interface;
using JellyDrop.Monetization;
using JellyDrop.Services;
using JellyDrop.Visuals;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JellyDrop
{
    /// <summary>
    /// Composition root. Creates the camera, UI, services and screens, and owns the main flows
    /// (start game, game over, shop, ads, purchases). Everything is built from code, so the
    /// project runs from any empty scene; see <see cref="Bootstrap"/>.
    /// </summary>
    public sealed class App : MonoBehaviour
    {
        public static App Instance { get; private set; }

        public GameConfig GameConfig { get; private set; }
        public EconomyService Economy { get; private set; }
        public SaveService Save { get; private set; }
        public AudioManager Audio { get; private set; }
        public IAdService Ads { get; private set; }
        public IIapService Iap { get; private set; }
        public GameController Game { get; private set; }
        public Camera Camera { get; private set; }
        public ScreenStack Screens { get; } = new ScreenStack();
        public RectTransform UiRoot { get; private set; }

        public HomeScreen Home { get; private set; }
        public HudScreen Hud { get; private set; }
        public PauseScreen Pause { get; private set; }
        public GameOverScreen GameOver { get; private set; }
        public ShopScreen Shop { get; private set; }
        public MissionsScreen Missions { get; private set; }
        public SettingsScreen Settings { get; private set; }
        public Popup Popup { get; private set; }
        public FakeAdOverlay AdOverlay { get; private set; }
        public TutorialOverlay Tutorial { get; private set; }
        public LanguagePicker LanguagePicker { get; private set; }

        public GameMode LastMode { get; private set; } = GameMode.Classic;

        private BackgroundView _background;
        private bool _adShowing;
        private float _lastAspect;

        // ================================================================== setup

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Screen.orientation = ScreenOrientation.Portrait;
#if ENABLE_LEGACY_INPUT_MANAGER
            Input.multiTouchEnabled = false;
#endif

            GameConfig = new GameConfig();
            Save = new SaveService();
            Economy = new EconomyService(Save.Load(), Catalog.CreateDefault(), new EconomyConfig(), new SystemClock());
            Economy.Changed += OnEconomyChanged;
            Haptics.Enabled = Economy.Data.hapticsOn;
            ApplyLanguage();

            Audio = new GameObject("Audio").AddComponent<AudioManager>();
            Audio.transform.SetParent(transform, false);
            Audio.ApplySettings(Economy.Data.musicOn, Economy.Data.sfxOn);
            UIKit.ClickSound = () => Audio.Play(Sfx.Button, 0.8f);

            SetupCamera();
            SetupUi();

            Game = new GameObject("Game").AddComponent<GameController>();
            Game.transform.SetParent(transform, false);
            Game.Init(Camera, Audio, GameConfig);
            Game.GameEnded += OnGameEnded;
            Game.FloatingText += (text, world, color, scale) => Hud.SpawnFloatingText(text, world, color, scale);
            Game.StateChanged += () => { if (Hud.IsVisible) Hud.Refresh(); };

            ApplyCosmetics();
            Game.LayoutCamera();
            _background.FitToCamera();
            _lastAspect = Camera.aspect;

            SetupMonetization();
        }

        private void Start()
        {
            Screens.Show(Home);
            Audio.PlayThemeMusic(Economy.EquippedTheme);
            // Daily reward greets the player once per day.
            if (Economy.CanClaimStreak)
                JellyDrop.Util.Tweener.Delay(0.6f, () => { if (Home.IsVisible && !Screens.HasModal) Popup.ShowDailyReward(); }, this);
        }

        private void SetupCamera()
        {
            Camera = Camera.main;
            if (Camera == null)
            {
                var go = new GameObject("Main Camera") { tag = "MainCamera" };
                Camera = go.AddComponent<Camera>();
                go.AddComponent<AudioListener>();
            }
            Camera.orthographic = true;
            Camera.clearFlags = CameraClearFlags.SolidColor;
            Camera.backgroundColor = ColorUtil.Night;
            Camera.nearClipPlane = 0.1f;
            Camera.farClipPlane = 100f;
            Camera.transform.position = new Vector3(0, 0, -10);
            Camera.gameObject.AddComponent<CameraShake>();

            _background = new GameObject("Background").AddComponent<BackgroundView>();
            _background.transform.SetParent(transform, false);
            _background.Build(Camera);
        }

        private void SetupUi()
        {
            if (EventSystem.current == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
                es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
                es.AddComponent<StandaloneInputModule>();
#endif
                DontDestroyOnLoad(es);
            }

            var canvasGo = new GameObject("UI", typeof(RectTransform));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = UIKit.Reference;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            // Narrow phones: match width. Wider screens (tablets): match height. The 1080x1920 layout always fits.
            scaler.matchWidthOrHeight = (float)Screen.width / Mathf.Max(1, Screen.height) > UIKit.Reference.x / UIKit.Reference.y ? 1f : 0f;
            canvasGo.AddComponent<GraphicRaycaster>();

            _canvas = canvasGo.transform;
            BuildScreens();
        }

        private Transform _canvas;

        /// <summary>Creates every screen. Called again after a language change, since screens build their text once.</summary>
        private void BuildScreens()
        {
            UiRoot = UIKit.Node("Safe", _canvas);
            UiRoot.Fill();
            UiRoot.gameObject.AddComponent<SafeAreaFitter>();

            Home = AddScreen<HomeScreen>("Home");
            Hud = AddScreen<HudScreen>("Hud");
            GameOver = AddScreen<GameOverScreen>("GameOver");
            Shop = AddScreen<ShopScreen>("Shop");
            Missions = AddScreen<MissionsScreen>("Missions");
            Settings = AddScreen<SettingsScreen>("Settings");
            Pause = AddScreen<PauseScreen>("Pause");
            Tutorial = AddScreen<TutorialOverlay>("Tutorial");
            Popup = AddScreen<Popup>("Popup");
            LanguagePicker = AddScreen<LanguagePicker>("Language");
            AdOverlay = AddScreen<FakeAdOverlay>("FakeAd");
        }

        // ================================================================== language

        /// <summary>The phone's language, mapped to the closest supported one (English if unsupported).</summary>
        public Language DeviceLanguage
        {
            get
            {
                switch (Application.systemLanguage)
                {
                    case SystemLanguage.Polish: return Language.Polish;
                    case SystemLanguage.German: return Language.German;
                    case SystemLanguage.Spanish: return Language.Spanish;
                    case SystemLanguage.French: return Language.French;
                    case SystemLanguage.Portuguese: return Language.Portuguese;
                    default: return Language.English;
                }
            }
        }

        private void ApplyLanguage() => Loc.Current = Economy.ChosenLanguage ?? DeviceLanguage;

        /// <summary>Switches language (null = follow the phone) and rebuilds the UI in it.</summary>
        public void SetLanguage(Language? language)
        {
            Economy.SetLanguage(language);
            Save.Save(Economy.Data);
            var before = Loc.Current;
            ApplyLanguage();
            if (Loc.Current != before)
            {
                Game.Leave();
                Screens.Reset();
                var old = UiRoot;
                BuildScreens();
                Destroy(old.gameObject);
            }
            Audio.Play(Sfx.Pop);
            Screens.Show(Settings);
        }

        private T AddScreen<T>(string name) where T : UIScreen
        {
            var rt = UIKit.Node(name, UiRoot);
            var screen = rt.gameObject.AddComponent<T>();
            screen.Setup(this);
            return screen;
        }

        private void SetupMonetization()
        {
#if ADMOB && !UNITY_EDITOR
            Ads = new AdMobAdService();
#else
            Ads = new FakeAdService((rewarded, onClosed) => AdOverlay.Open(rewarded, onClosed));
#endif
            Ads.Initialize(() => Debug.Log("[Ads] Ready"));

#if UNITY_PURCHASING && !UNITY_EDITOR
            var iap = new UnityIapService();
            iap.PremiumRestored += () => { if (!Economy.IsPremium) Economy.GrantPremium(); };
            Iap = iap;
#else
            Iap = new FakeIapService();
#endif
            Iap.Initialize(owned =>
            {
                if (owned && !Economy.IsPremium)
                {
                    Economy.GrantPremium();
                    ApplyCosmetics();
                }
            });
        }

        // ================================================================== flows

        public void StartGame(GameMode mode)
        {
            LastMode = mode;
            int seed = mode == GameMode.Daily ? Economy.DailyChallengeSeed : 0;
            var difficulty = Economy.DifficultyFor(mode);
            var config = DifficultySettings.For(difficulty).Apply(GameConfig);
            Screens.Show(Hud);
            Game.StartGame(mode, seed, config);
            Hud.OnGameStarted();

            if (!Economy.Data.tutorialSeen)
            {
                Game.Pause();
                Screens.PushModal(Tutorial);
            }
        }

        public void PauseGame()
        {
            if (!Game.IsPlaying) return;
            Game.Pause();
            Screens.PushModal(Pause);
        }

        public void ResumeGame()
        {
            Screens.PopModal(Pause);
            Game.Resume();
        }

        public void RestartGame() => StartGame(LastMode);

        /// <summary>Ends the run from the pause menu; coins earned so far are still paid out.</summary>
        public void QuitGame()
        {
            Screens.PopModal(Pause);
            Game.Quit();
            Game.Resume();
        }

        public void GoHome()
        {
            Game.Leave();
            Screens.Show(Home);
        }

        public void OpenShop(ShopTab tab = ShopTab.Skins)
        {
            Shop.SelectTab(tab);
            Screens.Show(Shop);
        }

        public void OpenMissions() => Screens.Show(Missions);
        public void OpenSettings() => Screens.Show(Settings);

        private void OnGameEnded(GameResult result)
        {
            var rewards = Economy.ApplyGameResult(result);
            Save.Save(Economy.Data);

            void ShowResults() => Screens.Show(GameOver.With(result, rewards));

            if (Economy.ShouldShowInterstitial())
            {
                Save.Save(Economy.Data);
                ShowAd(false, _ => ShowResults());
            }
            else
            {
                ShowResults();
            }
        }

        /// <summary>Shows a rewarded ad (max 2 per day). Coins are granted only if it was watched to the end.</summary>
        public void WatchAdForCoins(Action<int> onDone = null)
        {
            if (!Economy.CanWatchRewardedAd)
            {
                Popup.ShowMessage(Loc.T("ads.done_title"), Loc.T("ads.done_body", Economy.Config.MaxRewardedAdsPerDay), SpriteFactory.Coin());
                onDone?.Invoke(0);
                return;
            }
            ShowAd(true, completed =>
            {
                int coins = completed ? Economy.GrantRewardedAdReward() : 0;
                if (coins > 0)
                {
                    Audio.Play(Sfx.Purchase);
                    Popup.ShowReward($"+{coins}", Loc.T("ads.thanks"), SpriteFactory.Coin());
                }
                onDone?.Invoke(coins);
            });
        }

        private void ShowAd(bool rewarded, Action<bool> onClosed)
        {
            if (_adShowing) { onClosed(false); return; }
            _adShowing = true;
            Audio.Duck(0f);
            void Done(bool ok)
            {
                _adShowing = false;
                Audio.Duck(1f);
                onClosed(ok);
            }
            if (rewarded) Ads.ShowRewarded(Done);
            else Ads.ShowInterstitial(() => Done(false));
        }

        public void BuyPremium()
        {
            if (Economy.IsPremium) return;
            Iap.BuyPremium((success, error) =>
            {
                if (success)
                {
                    Economy.GrantPremium();
                    ApplyCosmetics();
                    Audio.Play(Sfx.Win);
                    Popup.ShowReward(Loc.T("premium.bought_title"), Loc.T("premium.bought_body"), SpriteFactory.Star());
                }
                else if (!string.IsNullOrEmpty(error))
                {
                    Popup.ShowMessage(Loc.T("iap.failed"), error, null);
                }
            });
        }

        public void RestorePurchases()
        {
            Iap.RestorePurchases(owned =>
            {
                if (owned)
                {
                    if (!Economy.IsPremium) Economy.GrantPremium();
                    ApplyCosmetics();
                    Popup.ShowMessage(Loc.T("iap.restored_title"), Loc.T("iap.restored_body"), SpriteFactory.Star());
                }
                else
                {
                    Popup.ShowMessage(Loc.T("iap.nothing_title"), Loc.T("iap.nothing_body"), null);
                }
            });
        }

        /// <summary>Applies the equipped skin, theme and avatar everywhere.</summary>
        public void ApplyCosmetics()
        {
            Game.ApplyCosmetics(Economy.EquippedSkin, Economy.EquippedAvatar, Economy.EquippedTheme);
            _background.SetTheme(Economy.EquippedTheme);
            Camera.backgroundColor = ColorUtil.Hex(Economy.EquippedTheme.BackgroundTop);
            Audio.PlayThemeMusic(Economy.EquippedTheme);
        }

        public void SetMusic(bool on)
        {
            Economy.SetMusic(on);
            Audio.ApplySettings(on, Economy.Data.sfxOn);
            if (on) Audio.PlayThemeMusic(Economy.EquippedTheme);
        }

        public void SetSfx(bool on)
        {
            Economy.SetSfx(on);
            Audio.ApplySettings(Economy.Data.musicOn, on);
        }

        public void SetHaptics(bool on)
        {
            Economy.SetHaptics(on);
            Haptics.Enabled = on;
        }

        // ================================================================== loop

        private void OnEconomyChanged()
        {
            Save.MarkDirty();
            Screens.RefreshAll();
        }

        private void Update()
        {
            Save.SaveIfDirty(Economy.Data);

            if (!Mathf.Approximately(Camera.aspect, _lastAspect))
            {
                _lastAspect = Camera.aspect;
                Game.LayoutCamera();
                _background.FitToCamera();
            }

            if (PointerInput.BackPressed() && !_adShowing) Screens.HandleBack();
        }

        private void OnApplicationPause(bool paused)
        {
            if (!paused) return;
            if (Game != null && Game.IsPlaying && !Game.IsPaused && !_adShowing) PauseGame();
            Save?.Save(Economy.Data);
        }

        private void OnApplicationQuit() => Save?.Save(Economy.Data);
    }

    /// <summary>Starts the game automatically in whatever scene is loaded (no scene setup needed).</summary>
    public static class Bootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            if (App.Instance != null) return;
            new GameObject("JellyDrop").AddComponent<App>();
        }
    }
}
