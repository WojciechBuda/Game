# Jelly Drop

A neon-arcade falling-block puzzle game for Android and iOS, built with Unity (C#).

The game includes:
- **Gameplay:** the full falling-block game with guideline rotation (SRS wall kicks), 7-bag randomizer, hold, ghost piece, combos and back-to-back bonuses.
- **Modes:** Classic, Sprint 40 and a Daily Challenge (the same pieces for everyone each day, always on Normal).
- **Difficulty levels:** Easy, Normal, Hard and Insane, picked on the home screen. Best scores are kept separately for each.
- **Lucky shots:** golden pieces (×5 coins), coin blocks, a random Rainbow bonus, a rare Jackpot, and a free lucky wheel after every game.
- **Economy:** coins and skin shards, a shop (block skins, themes with music, avatars, power-ups), daily missions, and a 7-day login streak.
- **Power-ups:** Bomb, Line Zap, Slow-Mo, Stick Swap. Max 2 per game, bought with coins.
- **Monetization:**
  - Free version: interstitial ads every 3rd game, never mid-game.
  - Rewarded video: +100 coins, capped at 2 per day.
  - Premium: a one-time purchase that removes ads and adds +50% coins plus exclusive items.
- **Languages:** English, Polish, German, Spanish, French and Portuguese. The game starts in the phone's language and can be switched in Settings.
- **Look:** neon arcade style: deep navy backgrounds, glowing pink/cyan/lime borders, a synthwave grid and a synthwave soundtrack. The older candy look is still in the shop as the Candy Land theme and Jelly Classic skin.
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
1. On the phone, open this repo's **Releases** page and download `JellyDrop.apk` from the newest "Jelly Drop test build".
2. Open the file. Android asks you to allow installing unknown apps for your browser: allow it, then tap **Install**.
3. Updates: install a newer build over the old one. Progress is kept.

The APK is signed with Unity's debug key: fine for testing, not for the Play Store (that needs an `.aab` signed with your own upload key).

## 2. Open the project in Unity

1. Install **Unity 6000.0.58f2** (Unity 6 LTS, includes the September 2025 security fix) through Unity Hub, with these modules:
   - **Android Build Support** (includes OpenJDK and the Android SDK/NDK).
   - **iOS Build Support**. iOS builds need a Mac with Xcode.
2. Clone this repo. In Unity Hub choose **Add → Add project from disk** and select the repo folder. The first import takes a few minutes.
3. Run the menu **Jelly Drop → Setup Project**. It does the following:
   - Sets the app name, bundle id (`com.wojciechbuda.jellydrop`) and portrait orientation.
   - Configures Android for IL2CPP + ARM64 and App Bundle (.aab) output, which Google Play requires.
   - Creates `Assets/Scenes/Main.unity` and adds it to the build.
   - Generates the app icon at `Assets/Art/AppIcon.png`.
4. Open `Assets/Scenes/Main.unity` and press **Play**. The game builds itself from code; the scene itself contains nothing.
5. To build an APK locally instead of in GitHub: **File → Build Profiles → Android → Switch Platform**, untick "Build App Bundle", then **Build**, or run the menu-free command `Unity -batchmode -quit -projectPath . -executeMethod JellyDrop.EditorTools.CiBuild.BuildAndroid`.

> The bundle id `com.wojciechbuda.jellydrop` is set in `Assets/Scripts/Editor/JellyDropSetup.cs`. Change it before your first store upload if you want a different one: it can't change after you publish.

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

## 3. Project structure

```
Assets/
  Scripts/
    Core/        Pure C# rules: board, pieces, scoring, lucky events, economy.
                 No Unity references, fully unit-tested.
      Economy/   Catalog (shop items & prices), PlayerData (save), EconomyService,
                 Missions, LuckyWheel
    Game/
      App.cs     Composition root and main flows (start game, game over, ads, purchases)
      Gameplay/  GameController (game feel), BoardView, effects, mascot, background
      Interface/ UI built in code: UIKit (candy buttons), screens and popups
      Audio/     Chiptune synth, procedural music per theme, sound effects
      Visuals/   Procedural sprite drawing (blocks, icons, avatars)
      Input/     Touch gestures + keyboard (new Input System or legacy, both work)
      Monetization/ Ad/IAP interfaces, fake services, AdMob & Unity IAP implementations
      Services/  Save (PlayerPrefs JSON + backup), haptics
    Editor/      "Jelly Drop" menu: project setup, icon, SDK switches, reset progress
  Tests/EditMode/ NUnit tests (Window → General → Test Runner)
Tools/CoreTests/  Runs the same tests outside Unity: `dotnet test`
Packages/, ProjectSettings/  Unity project files (Unity 6000.0.58f2)
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

All of these are plain classes, so you can later override them from Unity Remote Config or Firebase Remote Config without shipping an update.

## 4. Ads (AdMob)

Until this is set up, the game shows **fake test ads**, so the whole reward flow works in the editor.

1. Create an AdMob account and add an Android app and an iOS app. Create a **Rewarded** ad unit and an **Interstitial** ad unit for each.
2. Import the **Google Mobile Ads Unity plugin** from its GitHub releases (`.unitypackage`). Then run **Assets → External Dependency Manager → Android Resolver → Force Resolve**.
3. Open **Assets → Google Mobile Ads → Settings** and enter your AdMob **App IDs**.
4. In `Game/Monetization/MonetizationIds.cs`, replace the ad unit IDs. The ones in the file are Google's test IDs, which are safe to use while developing.
5. Run the menu **Jelly Drop → Ads: enable AdMob**. This adds the `ADMOB` define.
6. In the AdMob console, create a GDPR consent message under **Privacy & messaging**. The game already shows Google's consent form (UMP) to EU users, and offers "Privacy choices" in Settings.

## 5. In-app purchase (Premium)

1. Install **In-App Purchasing** from Window → Package Manager → Unity Registry. The code targets version **4.12.x**.
2. Create a **non-consumable** product with ID `jellydrop_premium` in both stores:
   - Google Play Console
   - App Store Connect
3. Run the menu **Jelly Drop → IAP: enable Unity IAP**. This adds the `UNITY_PURCHASING` define.
4. Test it:
   - **Android:** use license testers in the Play Console, with an internal-testing build.
   - **iOS:** use a Sandbox account.
5. Before launch, add server-side receipt validation (see the TODO in `UnityIapService.cs`).

Restore Purchases is in **Settings** and on the **Premium** shop tab. Apple requires it.

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
- **Online features:** Unity Cloud Save or Firebase for cross-device progress, and leaderboards for the Daily Challenge.
- **Analytics:** track retention, ad views and the coin economy.
- **More languages:** add a value to the `Language` enum and one column to every row in `Core/Localization/Strings.cs`. The tests fail if any translation is missing or has different placeholders.
- **Haptics:** a native plugin for short taps. Built-in vibration is only used for big moments.
- **Art and music:** replace the procedural sprites or music with commissioned art and tracks whenever you like (see `SpriteFactory` and `MusicComposer`).

## Running the tests

- In Unity: **Window → General → Test Runner → EditMode → Run All**.
- Without Unity: `cd Tools/CoreTests && dotnet test`. This needs the .NET 8 SDK.
