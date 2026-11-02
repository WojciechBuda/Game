# Stackville

A soft, bright arcade falling-block puzzle game for Android and iOS, built with Unity (C#). You play arcade levels to earn coins and building supplies, then grow your own city with them.

The game includes:
- **Gameplay:** the full falling-block game with guideline rotation (SRS wall kicks), 7-bag randomizer, hold, ghost piece, combos and back-to-back bonuses.
- **Level path:** a winding map of levels. Each level asks for an exact target score within a limited number of pieces, and gives 1–3 stars. Every 10th level is a **Boost level**: 90 seconds, points ×2, more lucky pieces, and score as much as you can for big rewards (lots of coins plus wood, bricks and glass).
- **City builder:** coins buy supplies at the market (wood, bricks, glass). Supplies plus coins build and upgrade 10 kinds of buildings on up to 12 plots. Houses, bakeries and windmills make coins over time; the sawmill, brickworks and glassworks make supplies; parks, the tower and the castle add a % bonus to coins from every game. Population raises the city level, which opens new plots and buildings. Buildings store up to 8 hours of production, so players come back to collect.
- **3D village:** the city is a low-poly 3D island (in the spirit of Ikariam / Grepolis). Buildings rise with scaffolding when built or upgraded, the windmill turns, villagers walk the paths and floating icons show what is ready to collect. Drag to pan, pinch to zoom, tap a plot.
- **Block Run (bonus):** a 3D three-lane runner starring your avatar block (swipe to change lanes, jump and slide). Collect coins. Always free to play, with 3 lives; bonus tickets (one a day, plus one for every Boost level won) make a run pay double coins.
- **Village missions:** every 20–45 minutes something happens in the 3D village: a runaway chicken, a thief with a sack of coins, a balloon festival or a hide-and-seek cat. Tap it in time for coins and supplies (bigger cities pay more).
- **Build times:** construction takes real time and grows with every level (seconds at first, up to 12 hours); 2 builders work at once and FINISH NOW speeds a build up for coins. The island grows as new rows of plots unlock (30 plots, city level up to 25, buildings up to level 10).
- **Pet care (like Pou):** your avatar is also a pet with four needs (food, energy, fun, cleanliness) that drop in real time, even while the game is closed. Ignore it and it gets sad, lonely and finally sick (medicine cures it). Feed it, wash it, put it to bed, give presents, dress it up or stroke it. A happy pet gives +5% coins from levels. Tap the mascot on the home screen.
- **Tower (like Icy Tower):** jump from floor to floor up a tower; run faster to jump higher and bounce off the walls for big combos. Endless tower in three modes (Classic, Speed, Ice) and five sceneries (ice, candy, jungle, space, lava), plus 12 towers with a floor limit (20–100 floors) and obstacles: icy, moving and crumbling floors, springs, narrow floors and a fast rise. Each tower unlocks the next.
- **Wardrobe:** dress up your avatar with coins: body colours, hats (cap, beanie, party hat, headphones, cowboy hat, crown) and glasses.
- **Controls:** tap a column to drop the piece there, tap the piece to rotate; swipes still work.
- **Player level:** XP from every game, won level and mission. Each level-up pays coins (plus shards and supplies on milestones), unlocks exclusive items at levels 5, 10, 20, 30 and 50, and adds +1% coins from games every 5 levels (up to +20%). It never resets.
- **Season pass:** a new 28-day season with 30 tiers. Every tier has a free reward and a pass reward (coins, shards, power-ups, supplies). Each season has its own exclusive theme (free track), avatar and skin (pass track), rotating spring, summer, autumn and winter sets. The pass is a one-time purchase per season; unclaimed rewards are paid out automatically when a season ends.
- **Arcade modes:** Classic (endless, with difficulty), Sprint 40 and a Daily Challenge (the same pieces for everyone each day, always on Normal).
- **Difficulty levels:** Easy, Normal, Hard and Insane, picked on the home screen. Best scores are kept separately for each.
- **Lucky shots:** golden pieces (×5 coins), coin blocks, a random Rainbow bonus, a rare Jackpot, and a free lucky wheel after every game.
- **Economy:** coins and skin shards, a shop (block skins, themes with music, avatars, power-ups), daily missions, and a 7-day login streak.
- **Power-ups:** Bomb, Line Zap, Slow-Mo, Stick Swap. Max 2 per game, bought with coins.
- **Monetization:**
  - Free version: interstitial ads every 3rd game, never mid-game.
  - Rewarded video: +100 coins, capped at 2 per day.
  - Premium: a one-time purchase that removes ads and adds +50% coins plus exclusive items.
- **Languages:** English, Polish, German, Spanish, French and Portuguese. The game starts in the phone's language and can be switched in Settings.
- **Look and sound:** modern dusk style: deep indigo backgrounds, dark glass cards, Poppins font, springy animations and haptics. Music is generated lo-fi and future-house loops (the old chiptune and synthwave styles remain on a few shop themes).
- **Art and audio:** all graphics, music and sound effects are generated in code at startup, so there are no asset files to manage.

Previews of the generated art are in `Docs/`.

---

## 1. Get the game on your Android phone

Every push to `main` builds an installable APK in GitHub Actions and publishes it under **Releases**. The build runs Unity in the cloud, so it needs your Unity licence once.

**One-time setup (about 10 minutes):**
1. Install **Unity Hub** on your PC, sign in with your Unity account and activate a free licence: Hub → Settings (gear) → Licenses → **Add** → **Get a free personal license**.
2. Find the licence file it created:
   - Windows: `C:\ProgramData\Unity\Unity_lic.ulf`
   - macOS: `/Library/Application Support/Unity/Unity_lic.ulf`
3. In this GitHub repo: **Settings → Secrets and variables → Actions → New repository secret**. Add three secrets:
   - `UNITY_LICENSE`: the whole content of `Unity_lic.ulf` (open it in Notepad, copy everything).
   - `UNITY_EMAIL`: the email of your Unity account.
   - `UNITY_PASSWORD`: its password.
4. Go to **Actions → Android APK → Run workflow** (or push any commit). The first build takes roughly 20–40 minutes; later builds are faster thanks to caching.

**Install on the phone:**
1. On the phone, open this repo's **Releases** page and download `Stackville.apk` from the newest "Stackville test build".
2. Open the file. Android asks you to allow installing unknown apps for your browser: allow it, then tap **Install**.
3. Updates: install a newer build over the old one. Progress is kept.

The APK is signed with Unity's debug key: fine for testing, not for the Play Store (that needs an `.aab` signed with your own upload key).

## 2. Open the project in Unity

1. Install **Unity 6000.6.7f1** through Unity Hub, with these modules:
   - **Android Build Support** (includes OpenJDK and the Android SDK/NDK).
   - **iOS Build Support**. iOS builds need a Mac with Xcode.
2. Clone this repo. In Unity Hub choose **Add → Add project from disk** and select the repo folder. The first import takes a few minutes.
3. Run the menu **Stackville → Setup Project**. It does the following:
   - Sets the app name, bundle id (`com.wojciechbuda.stackville`) and portrait orientation.
   - Configures Android for IL2CPP + ARM64 and App Bundle (.aab) output, which Google Play requires.
   - Creates `Assets/Scenes/Main.unity` and adds it to the build.
   - Generates the app icon at `Assets/Art/AppIcon.png`.
4. Open `Assets/Scenes/Main.unity` and press **Play**. The game builds itself from code; the scene itself contains nothing.
5. To build an APK locally instead of in GitHub: **File → Build Profiles → Android → Switch Platform**, untick "Build App Bundle", then **Build**, or run the menu-free command `Unity -batchmode -quit -projectPath . -executeMethod JellyDrop.EditorTools.CiBuild.BuildAndroid`.

> The bundle id `com.wojciechbuda.stackville` is set in `Assets/Scripts/Editor/JellyDropSetup.cs`. Change it before your first store upload if you want a different one: it can't change after you publish.

### Difficulty levels

| | Easy | Normal | Hard | Insane |
|---|---|---|---|---|
| Starting speed level | 1 (slower curve) | 1 | 5 | 10 |
| Time to slide a landed piece | 0.8 s | 0.5 s | 0.4 s | 0.3 s |
| Next-piece previews | 3 | 3 | 2 | 1 |
| Ghost piece | yes | yes | yes | no |
| Junk rows at the start | 0 | 0 | 4 | 6 |
| Rising junk row | no | no | no | every 12 s |
| Power-ups per game | 4 | 2 | 1 | 0 |
| Score and coins | x0.75 | x1 | x1.5 | x2.5 |
| Lucky-shot chances | x1.2 | x1 | x1 | x0.8 |

All values live in `Core/Difficulty.cs`. The Daily Challenge always uses Normal, so everyone plays the same game.

### Languages

The game ships in English, Polish, German, Spanish, French and Portuguese.

- On first launch it follows the phone's language (English when the phone uses another language).
- **Settings → Language** lets the player pick one, or go back to "Automatic". The choice is saved.
- Every piece of text is in `Core/Localization/Strings.cs`, one row per text with all six languages side by side. Fix a translation there.
- Numbers use each language's separators (1,250 / 12 345 / 12.345), and coin counts use proper plurals ("2 monety", "5 monet").
- If you add a font in `Assets/Resources/Fonts/MainFont.ttf`, make sure it includes Polish and other accented letters (ą ę ł ż ñ ç ã...). Fredoka and Baloo 2 do.

### Controls

| Action | Touch | Keyboard (editor) |
|---|---|---|
| Rotate | tap | ↑ / X (Z = counter-clockwise) |
| Move | drag sideways | ← → |
| Soft drop | drag down | ↓ |
| Hard drop | quick flick down | Space |
| Hold | flick up | C / Shift |
| Pause | pause button / Android back | Esc / P |

Tower: slide a finger left / right to run, tap to jump (or steer with one thumb and tap with the other). Keyboard: ← → and Space / ↑.

## 3. Project structure

```
Assets/
  Scripts/
    Core/        Pure C# rules: board, pieces, scoring, lucky events, economy.
                 No Unity references, fully unit-tested.
      Economy/   Catalog (shop items & prices), PlayerData (save), EconomyService,
                 Missions, LuckyWheel
      Levels/    LevelPath (targets, piece limits, boost levels, rewards), ProgressService
      City/      CityCatalog (buildings, costs, production), CityService
      Loyalty/   Player level and season pass (points, tiers, rewards, season calendar)
      Runner/    Block Run rules (lanes, obstacles, coins), unit-tested
      Pet/       Pet needs, moods, sickness and care actions
      Tower/     Tower climb physics, endless modes, towers with a floor limit, demo bot
      Online/    IOnlineService + OfflineService (ready for leaderboards and challenges)
    Game/
      App.cs     Composition root and main flows (start game, game over, ads, purchases)
      Gameplay/  GameController (game feel), BoardView, effects, mascot, background
      Interface/ UI built in code: UIKit (candy buttons), screens and popups
      Audio/     Chiptune synth, procedural music per theme, sound effects
      Visuals/   Procedural sprite drawing (blocks, icons, avatars, app icon)
      City3D/    Low-poly mesh builder, building models and the 3D village
      Runner3D/  The 3D Block Run view
      Input/     Touch gestures + keyboard (new Input System or legacy, both work)
      Monetization/ Ad/IAP interfaces, fake services, AdMob & Unity IAP implementations
      Services/  Save (PlayerPrefs JSON + backup), haptics
    Editor/      "Stackville" menu: project setup, icon, SDK switches, reset progress
  Tests/EditMode/ NUnit tests (Window → General → Test Runner)
Tools/CoreTests/  Runs the same tests outside Unity: `dotnet test`
Packages/, ProjectSettings/  Unity project files (Unity 6000.6.7f1)
.github/workflows/android.yml  Cloud build: tests + installable APK on every push
```

### Tuning numbers

| What | Where |
|---|---|
| Speed, scoring, coins per line, power-up limits | `Core/GameConfig.cs` |
| Difficulty levels (speed, junk rows, previews, multipliers) | `Core/Difficulty.cs` |
| UI colour palette (neon colours, panel and text colours) | `ColorUtil` in `Game/Visuals/PixelCanvas.cs` |
| Lucky chances (golden 1/50, rainbow 8%, jackpot 0.4%) | `LuckyConfig` in `Core/GameConfig.cs` |
| Starting coins, ad reward (100), ads per day (2), interstitial frequency (3), streak rewards, Premium multiplier | `Core/Economy/EconomyService.cs` (`EconomyConfig`) |
| Shop items, prices, colours, themes, music style | `Core/Economy/Catalog.cs` |
| Missions | `Core/Economy/Missions.cs` |
| All game text and translations | `Core/Localization/Strings.cs` |
| Wheel slices and odds | `Core/Economy/LuckyWheel.cs` |
| Level targets, piece limits, boost every 10 levels, level rewards | `Core/Levels/LevelPath.cs` |
| Buildings, costs, production, supply prices, city levels | `Core/City/CityCatalog.cs` |
| XP curve, level rewards, season length, tiers, points per tier, tier rewards, points per game/mission | `Core/Loyalty/Loyalty.cs` |

All of these are plain classes, so you can later override them from Unity Remote Config or Firebase Remote Config without shipping an update.

## 4. Ads (AdMob)

Until this is set up, the game shows **fake test ads**, so the whole reward flow works in the editor.

1. Create an AdMob account and add an Android app and an iOS app. Create a **Rewarded** ad unit and an **Interstitial** ad unit for each.
2. Import the **Google Mobile Ads Unity plugin** from its GitHub releases (`.unitypackage`). Then run **Assets → External Dependency Manager → Android Resolver → Force Resolve**.
3. Open **Assets → Google Mobile Ads → Settings** and enter your AdMob **App IDs**.
4. In `Game/Monetization/MonetizationIds.cs`, replace the ad unit IDs. The ones in the file are Google's test IDs, which are safe to use while developing.
5. Run the menu **Stackville → Ads: enable AdMob**. This adds the `ADMOB` define.
6. In the AdMob console, create a GDPR consent message under **Privacy & messaging**. The game already shows Google's consent form (UMP) to EU users, and offers "Privacy choices" in Settings.

## 5. In-app purchase (Premium)

1. Install **In-App Purchasing** from Window → Package Manager → Unity Registry. The code targets version **4.12.x**.
2. Create these products in both stores (Google Play Console and App Store Connect):
   - `jellydrop_premium`: **non-consumable** (no ads + Premium pack).
   - `jellydrop_season_pass`: **consumable** (the season pass, bought again each season).
3. Run the menu **Stackville → IAP: enable Unity IAP**. This adds the `UNITY_PURCHASING` define.
4. Test it:
   - **Android:** use license testers in the Play Console, with an internal-testing build.
   - **iOS:** use a Sandbox account.
5. Before launch, add server-side receipt validation (see the TODO in `UnityIapService.cs`).

Restore Purchases is in **Settings** and on the **Premium** shop tab. Apple requires it.

## 5b. Google sign-in (Google Play Games)

The game signs players in with their Google account through Google Play Games Services (Android). Until it is set up, the game runs offline and the Settings button explains that sign-in is not available yet.

1. In **Play Console**: your app > **Grow > Play Games Services > Setup and management > Configuration**. Create a Play Games Services project.
2. Add an **Android credential**: package `com.wojciechbuda.stackville` and the SHA-1 of your signing keys (Play Console > Setup > App signing shows both the upload key and the app signing key; add both).
3. Under **Leaderboards**, create "Highest level" (integer, larger is better) and put its ID in `Game/Online/OnlineIds.cs`.
4. Add your Google account under **Testers** while the game is unpublished.
5. Download the **Play Games plugin for Unity** (`GooglePlayGamesPlugin-x.y.z.unitypackage`) from github.com/playgameservices/play-games-plugin-for-unity and import it.
6. **Window > Google Play Games > Setup > Android setup**: paste the resources XML from Play Console (Configuration > Get resources).
7. Run the menu **Stackville > Online: enable Google Play Games**. This adds the `PLAY_GAMES` define.

Players are signed in silently at start; **Settings > Sign in with Google** signs in on demand. For iOS, add Sign in with Apple later (Apple requires it whenever another sign-in is offered).

## 5c. Privacy policy

Both stores require a public privacy policy. It lives in `Tools/PrivacyPolicy/policy.py` (English + Polish), which generates:
- `privacy-policy.html` in the repo root (the public page for the store listing),
- `Assets/Scripts/Game/Interface/PrivacyPolicyText.cs` (shown in the game: on first launch and in Settings > Privacy policy).

To change the contact e-mail or the text, edit `policy.py` and run `python3 Tools/PrivacyPolicy/policy.py`. Then publish the page with GitHub Pages (repo Settings > Pages > Deploy from a branch > `main` / root). The address, `https://wojciechbuda.github.io/Game/privacy-policy.html`, is already set in `MonetizationIds.PrivacyPolicyUrl`; paste it into the Play Console (App content > Privacy policy) too. If you change what the game collects, update the policy and raise `VERSION` so players see it again.

## 6. Publishing checklist

- [ ] Final name. Don't use "Tetris" or "-tris" in the name, icon or store text: The Tetris Company enforces its trademark.
- [ ] Bundle id, company name and version set in `JellyDropSetup.cs`.
- [ ] Privacy policy hosted, with its URL set in `MonetizationIds.PrivacyPolicyUrl` and in both store listings.
- [ ] Google Play:
  - Developer account ($25 one-time).
  - **Data safety** form (the ads SDK collects the device ad ID).
  - Content rating questionnaire.
  - Target audience: 13+ is recommended, to avoid the Families ad policy.
  - New personal accounts must run a **closed test with 12+ testers for 14 days** before production.
- [ ] Apple:
  - Developer Program ($99/year).
  - **App Privacy** labels.
  - App Tracking Transparency prompt text (`NSUserTrackingUsageDescription`) if you use personalised ads.
- [ ] Screenshots, feature graphic, short trailer.
- [ ] Real AdMob IDs (not the test ones), plus `ADMOB` and `UNITY_PURCHASING` defines enabled.

## 7. Nice next steps

- **Font:** drop a rounded font into `Assets/Resources/Fonts/MainFont.ttf` and it is picked up automatically. Fredoka and Baloo 2 from Google Fonts are free (OFL licence). Without it, Unity's built-in font is used.
- **Multiplayer and challenges** (the groundwork is in place):
  1. Pick a backend: Unity Gaming Services (Authentication, Cloud Save, Leaderboards, Cloud Code), PlayFab, Nakama, or your own ASP.NET Core API.
  2. Implement `IOnlineService` (`Core/Online/IOnlineService.cs`) and set it in `App.Awake` instead of `OfflineService`. Scores are already submitted after every won level.
  3. **Async challenges:** every level has a fixed seed (`LevelDef.Seed`), so two players get exactly the same pieces. Player A plays a level and sends a challenge with a coin stake; player B plays the same level; the higher score wins. The `Challenge` model and the send/answer calls are already defined.
  4. Later: real-time duels (both boards side by side, cleared lines send junk rows to the opponent; the game already supports junk rows), weekly leaderboards per level, and friends visiting each other's cities.
  5. Validate scores on the server (replay the inputs with the same seed, using the pure C# `GameSession`) to stop cheating.
- **Analytics:** track retention, ad views and the coin economy.
- **More languages:** add a value to the `Language` enum and one column to every row in `Core/Localization/Strings.cs`. The tests fail if any translation is missing or has different placeholders.
- **Haptics:** a native plugin for short taps. Built-in vibration is only used for big moments.
- **Art and music:** replace the procedural sprites or music with commissioned art and tracks whenever you like (see `SpriteFactory` and `MusicComposer`).

## Running the tests

- In Unity: **Window → General → Test Runner → EditMode → Run All**.
- Without Unity: `cd Tools/CoreTests && dotnet test`. This needs the .NET 8 SDK.
