# Stackville na iPhone'a (instalka testowa, bez Maca)

Na Windowsie nie zbudujesz gry na iOS, więc robi to za Ciebie GitHub: na swoim Macu w chmurze buduje **niepodpisany plik `.ipa`**. Na iPhone'a wgrywasz go z Windowsa programem **Sideloadly** i darmowym Apple ID.

**Ograniczenia instalki testowej:**
- Z darmowym Apple ID gra działa **7 dni**. Potem wgrywasz ją jeszcze raz tym samym plikiem i postęp zostaje.
- Reklamy i zakupy są w wersji testowej (atrapy), bo budowa w chmurze nie ma wtyczek AdMob i IAP. Do sprawdzenia rozgrywki to wystarczy.
- Do App Store potrzebne jest płatne konto Apple Developer (99 USD rocznie). Opis jest na dole.

---

## 1. Licencja Unity na GitHubie (jednorazowo)

GitHub musi uruchomić Unity na Twojej licencji (darmowa Personal wystarczy):

1. Na komputerze, na którym masz Unity, znajdź plik licencji:
   `C:\ProgramData\Unity\Unity_lic.ulf`
   (`ProgramData` jest ukryty: wklej tę ścieżkę w pasek adresu Eksploratora).
2. Otwórz go w Notatniku i skopiuj **całą** zawartość.
3. Na GitHubie: repo `WojciechBuda/Game` → **Settings → Secrets and variables → Actions → New repository secret**. Dodaj trzy sekrety:
   - `UNITY_LICENSE`: wklejona zawartość pliku `.ulf`,
   - `UNITY_EMAIL`: e-mail Twojego konta Unity,
   - `UNITY_PASSWORD`: hasło do konta Unity.

Te same sekrety naprawią też automatyczne budowanie APK na Androida, które teraz kończy się błędem właśnie z ich braku.

## 2. Zbuduj `.ipa`

1. Repo → zakładka **Actions** → po lewej **iOS test build** → **Run workflow** → **Run workflow**.
2. Poczekaj 40–90 minut. Pierwszy raz trwa najdłużej.
3. Gdy będzie zielony ptaszek, wejdź w **Releases** (prawa strona strony repo) → **Stackville iOS test build N** → pobierz **`Stackville-unsigned.ipa`**.

Jeśli zadanie skończy się na czerwono, otwórz je, skopiuj ostatnie linie z błędem i wklej mi. Najczęstsza przyczyna przy pierwszym razie: brak gotowego obrazu Unity 6000.6.7f1 w game-ci. Wtedy ustawię wersję, która jest dostępna.

## 3. Zainstaluj na iPhonie przez Sideloadly (Windows)

1. Zainstaluj **iTunes** i **iCloud** w wersjach ze strony Apple (nie ze Sklepu Microsoft). Sideloadly ich potrzebuje.
2. Pobierz **Sideloadly** z https://sideloadly.io i zainstaluj.
3. Podłącz iPhone'a kablem USB. Na telefonie kliknij **Ufaj temu komputerowi**.
4. W Sideloadly:
   - przeciągnij `Stackville-unsigned.ipa`,
   - wpisz swój Apple ID,
   - kliknij **Start** i podaj hasło do Apple ID (lub hasło do aplikacji, jeśli masz dwuetapowe logowanie).
5. Na iPhonie: **Ustawienia → Ogólne → VPN i zarządzanie urządzeniem** → wybierz swój Apple ID → **Ufaj**.
6. Na iOS 16 i nowszych: **Ustawienia → Prywatność i ochrona → Tryb dewelopera** → włącz i uruchom telefon ponownie.
7. Otwórz Stackville.

Po 7 dniach gra przestanie się otwierać. Powtórz krok 4 z tym samym plikiem.

---

## Później: App Store

Kod jest już przygotowany:
- iPhone, iOS 15+,
- tekst zgody na śledzenie (ATT),
- plik prywatności,
- ID AdMob w Info.plist.

Do wydania w App Store potrzebujesz:

1. **Apple Developer Program** (developer.apple.com, 99 USD rocznie). Twój **Team ID** wpisz w `Assets/Scripts/Editor/ReleaseTools.cs` (`AppleTeamId`).
2. **AdMob → Aplikacje → Dodaj aplikację → iOS.**
   - Utwórz dwie jednostki reklamowe: z nagrodą i pełnoekranową.
   - Wyślij mi App ID i ID jednostek. Wpiszę je w miejsce testowych.
   - W **Prywatność i wiadomości** utwórz też komunikat **IDFA (iOS)**, żeby zgoda na śledzenie pokazywała się przed reklamami.
3. **App Store Connect:**
   - nowa aplikacja z identyfikatorem pakietu `com.stackville`,
   - produkty `stackville_premium` (Non-Consumable) i `stackville_season_pass` (Consumable),
   - etykiety prywatności,
   - zrzuty ekranu 6,9" (1320 × 2868).
4. **Wysyłka do Apple**: na Macu przez Xcode (Archive → Distribute) albo przez GitHub. Workflow trzeba wtedy rozbudować o podpisywanie i wysyłkę do TestFlight, co zrobię, gdy będziesz miał konto Apple.
