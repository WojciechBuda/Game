# Stackville: wydanie w Google Play, krok po kroku

W tym folderze jest wszystko, czego potrzebujesz do Play Console. Teksty możesz kopiować z plików, a grafiki wgrywać prosto z `Store/graphics`.

| Co | Plik | Wymaganie Google |
|---|---|---|
| Ikona aplikacji | `Store/graphics/icon-512.png` | 512 × 512 PNG, do 1 MB |
| Grafika promocyjna (PL) | `Store/graphics/feature-graphic-pl-PL.png` | 1024 × 500, bez przezroczystości |
| Grafika promocyjna (EN) | `Store/graphics/feature-graphic-en-US.png` | j.w. |
| Zrzuty ekranu | `Store/screenshots/framed/<język>/NN.png` | 2–8 szt., 9:16, min. 1080 px (generujesz w kroku 6) |
| Tytuł | `fastlane/metadata/android/<język>/title.txt` | do 30 znaków |
| Krótki opis | `.../short_description.txt` | do 80 znaków |
| Pełny opis | `.../full_description.txt` | do 4000 znaków |
| Informacje o wersji | `.../changelogs/1.txt` | do 500 znaków |
| Polityka prywatności | `privacy-policy.html`, online: https://wojciechbuda.github.io/Game/privacy-policy.html | publiczny URL |
| Regulamin (opcjonalny) | `terms.html`, online: https://wojciechbuda.github.io/Game/terms.html | — |
| app-ads.txt | `Store/app-ads.txt` | hostowany w głównym katalogu Twojej strony |

Dane aplikacji:

- **Nazwa pakietu:** `com.wojciechbuda.stackville`. Po pierwszym wgraniu nie da się jej już zmienić.
- **Wersja:** 1.0.0, kod wersji 1 (przy każdym kolejnym wgraniu musi być wyższy).
- **Kontakt:** wojciech.buda6@gmail.com. Ten adres jest publiczny w sklepie; jeśli wolisz inny, zmień go też w `Tools/PrivacyPolicy/policy.py`.

---

## 1. Konto dewelopera (jednorazowo)

1. Wejdź na https://play.google.com/console i załóż konto **Osobiste**. Opłata wynosi jednorazowo 25 USD.
2. Przejdź weryfikację tożsamości (dowód osobisty) i potwierdź numer telefonu. Trwa to od kilku dni do ok. 2 tygodni.
3. **Ważne dla nowych kont osobistych:** zanim opublikujesz grę dla wszystkich, musisz przeprowadzić **test zamknięty z co najmniej 12 testerami przez 14 dni bez przerwy**. Dopiero potem pojawi się przycisk „Złóż wniosek o dostęp do wersji produkcyjnej”. Zacznij zbierać testerów (rodzina, znajomi, grupy testerów) już teraz.

## 2. Przygotuj grę w Unity

1. **Stackville → Setup Project** ustawia między innymi: Target API 36, IL2CPP, ARM64, minSdk 26 i budowanie .aab.
2. **Klucz podpisu (keystore):** Player Settings → Android → Publishing Settings → Keystore Manager → Create new.
   - Zapisz plik `.keystore` i hasła w bezpiecznym miejscu, **nie w repozytorium**. To Twój „upload key”.
   - Gdy go zgubisz, reset jest możliwy, ale trwa.
3. **Prawdziwe reklamy AdMob:**
   - Na https://admob.google.com dodaj aplikację (Android, „nie jest jeszcze w sklepie”).
   - Utwórz 2 jednostki reklamowe: **Z nagrodą** oraz **Pełnoekranowa**.
   - Wpisz ich ID w `Assets/Scripts/Game/Monetization/MonetizationIds.cs` (sekcja Android) w miejsce testowych `ca-app-pub-3940256099942544/...`.
   - App ID AdMob (`ca-app-pub-XXXX~YYYY`) wpisz w Assets → Google Mobile Ads → Settings.
   - Po zaimportowaniu Google Mobile Ads kliknij *Stackville → Ads: enable AdMob* (symbol `ADMOB`).
   - W AdMob → Prywatność i wiadomości utwórz **komunikat RODO (GDPR)** dla Europy. Gra już go wyświetla przez UMP przy starcie.
   - Ustaw w AdMob → Blokowanie → Kategorie wrażliwe maks. ocenę treści **T (Teen)**. Kod też to wymusza.
4. **Zakupy w aplikacji:** zainstaluj pakiet In-App Purchasing i kliknij *Stackville → IAP: enable Unity IAP* (symbol `UNITY_PURCHASING`). Same produkty utworzysz w kroku 5.
5. **Google Play Games (opcjonalnie):**
   - Play Console → Konfiguracja Gier Play: utwórz projekt i dane logowania OAuth.
   - Podaj SHA-1 z Play Console → Integralność aplikacji → Podpisywanie aplikacji, a potem utwórz tabelę wyników.
   - ID wpisz w `Assets/Scripts/Game/Online/OnlineIds.cs` i kliknij *Stackville → Online: enable Google Play Games*.
   - Jeśli nie chcesz tego teraz, zostaw `PLAY_GAMES` wyłączone. Wtedy w Bezpieczeństwie danych pomiń wiersz „Identyfikatory użytkownika”.
6. Zostaw wyłączone **Unity Analytics** (Edit → Project Settings → Services). Gra ich nie potrzebuje, a polityka prywatności nie opisuje dodatkowej analityki.
7. **Stackville → Release → 1. Check release settings** pokaże, co jeszcze jest do poprawienia.
8. **Stackville → Release → 3. Build release .aab** utworzy plik `Builds/Stackville-1.0.0-1.aab`.
   - Przed każdą kolejną wersją kliknij **2. Bump version code (+1)**.

## 3. Utwórz aplikację w Play Console

Kliknij Utwórz aplikację i podaj:

- **Nazwa:** Stackville: Klocki i Miasto
- **Język domyślny:** polski (pl-PL)
- **Aplikacja czy gra:** Gra
- **Bezpłatna czy płatna:** Bezpłatna (później nie da się zmienić na płatną).
- Zaznacz oświadczenia i zapisz.

## 4. „Treść aplikacji” (App content): gotowe odpowiedzi

### Polityka prywatności
`https://wojciechbuda.github.io/Game/privacy-policy.html`

Najpierw włącz GitHub Pages (krok 9) i sprawdź, czy strona się otwiera.

### Dostęp do aplikacji
**Wszystkie funkcje są dostępne bez specjalnego dostępu.** Gra nie wymaga logowania.

### Reklamy
**Tak, aplikacja zawiera reklamy.**

### Identyfikator wyświetlania reklam (Advertising ID)
- **Tak**, aplikacja korzysta z identyfikatora wyświetlania reklam.
- Cel: **Reklamy lub marketing**.
- SDK AdMob samo dodaje uprawnienie `AD_ID`.

### Ocena treści (kwestionariusz IARC)
- E-mail: wojciech.buda6@gmail.com
- Kategoria: **Gra**.
- Odpowiedzi:
  - Przemoc / krew / strach / seks / wulgaryzmy / narkotyki / alkohol: **Nie**.
  - Hazard prawdziwy lub symulowany (kasyno, ruletka o żetony do kupienia): **Nie**.
    - Koło fortuny daje tylko przedmioty w grze.
    - Kręci się za darmo (raz dziennie) albo za monety zarobione w grze.
    - Monet nie da się kupić za prawdziwe pieniądze, a nagród nie da się wymienić na pieniądze.
  - Czy użytkownicy mogą się komunikować / udostępniać treści: **Nie**.
  - Czy aplikacja udostępnia lokalizację użytkownika innym użytkownikom: **Nie**.
  - Zakupy cyfrowe: **Tak** (Premium, przepustka sezonowa).
  - Losowe przedmioty za prawdziwe pieniądze (loot box): **Nie**.
- Spodziewany wynik: PEGI 3 / ESRB Everyone (z dopiskiem „Zakupy w grze”).

### Docelowa grupa odbiorców
- Grupy wiekowe: **13–15, 16–17, 18+**. **Nie** zaznaczaj grup poniżej 13 lat.
  - Zaznaczenie młodszych dzieci wymaga programu „Projektowane dla rodzin” i certyfikowanych reklam, których ta gra nie ma.
- „Czy aplikacja może nieumyślnie przyciągać dzieci?”: **Nie**, z uzasadnieniem:
  > Gra logiczna z klockami i budowaniem miasta, skierowana do nastolatków i dorosłych; opis, grafiki i polityka prywatności wskazują wiek 13+.

  Jeśli Google mimo to uzna inaczej, dostosuj się do jego uwag.

### Bezpieczeństwo danych (Data safety)

**Pytania ogólne:**
- Czy aplikacja zbiera lub udostępnia dane wymienionych typów? **Tak**
- Czy wszystkie dane są szyfrowane podczas przesyłania? **Tak**
- Czy użytkownik może poprosić o usunięcie danych? **Tak**
  - W grze: Ustawienia → „Usuń moje dane” (usuwa cały zapis z telefonu).
  - Alternatywnie e-mail: wojciech.buda6@gmail.com.
  - Jeśli formularz poprosi o URL usuwania danych: `https://wojciechbuda.github.io/Game/privacy-policy.html#pl`. Polityka ma sekcję „Twoje prawa” z instrukcją.

**Typy danych do zaznaczenia:**

| Kategoria → typ | Zbierane | Udostępniane | Opcjonalne? | Cele | Skąd |
|---|---|---|---|---|---|
| Lokalizacja → **Przybliżona lokalizacja** | Tak | Tak | Nie (wymagane) | Reklamy lub marketing; Analityka; Zapobieganie oszustwom, bezpieczeństwo i zgodność | AdMob (adres IP) |
| Aktywność w aplikacjach → **Interakcje z aplikacją** | Tak | Tak | Nie | Reklamy lub marketing; Analityka | AdMob |
| Informacje o aplikacji i wydajności → **Dzienniki awarii** i **Diagnostyka** | Tak | Tak | Nie | Analityka; Zapobieganie oszustwom | AdMob |
| Identyfikatory urządzenia lub inne → **Identyfikatory urządzenia lub inne** | Tak | Tak | Nie | Reklamy lub marketing; Analityka; Zapobieganie oszustwom | AdMob (identyfikator reklamowy) |
| Informacje finansowe → **Historia zakupów** | Tak | Nie | Tak | Funkcje aplikacji | Google Play Billing |
| Informacje osobiste → **Identyfikatory użytkownika** | Tak | Nie | Tak | Funkcje aplikacji; Zarządzanie kontem | Gry Play: nick i wyniki. Tylko jeśli włączysz `PLAY_GAMES` |
| Aktywność w aplikacjach → **Inne treści generowane przez użytkownika** | Nie | — | — | — | Postęp gry jest tylko na telefonie |

- „Przetwarzane efemerycznie”: **Nie**, dla wszystkich typów.
- Pozostałych kategorii (zdjęcia, kontakty, wiadomości, zdrowie itd.) nie zaznaczaj.

Źródła mapowania: dokumentacja Google [AdMob – Data disclosure](https://developers.google.com/admob/android/privacy/play-data-disclosure) oraz [Play Games Services – Data safety](https://developer.android.com/games/pgs/data-safety).

### Pozostałe deklaracje
Wszystkie poniższe: **Nie / nie dotyczy**.

- Aplikacja rządowa
- Funkcje finansowe
- Aplikacja zdrowotna
- Aplikacja z wiadomościami
- Śledzenie kontaktów COVID
- Uprawnienia wrażliwe: gra używa tylko Internetu i wibracji, a to nie są uprawnienia wymagające deklaracji

## 5. Produkty w aplikacji

Ścieżka: Play Console → Zarabianie → Produkty → Produkty w aplikacji. Opcja ta pojawi się dopiero **po wgraniu pierwszego .aab** (krok 7) z biblioteką płatności, czyli z włączonym `UNITY_PURCHASING`.

| ID produktu (musi być identyczne) | Nazwa | Typ w grze | Sugerowana cena |
|---|---|---|---|
| `stackville_premium` | Stackville Premium | jednorazowy | 9,99 zł |
| `stackville_season_pass` | Przepustka sezonowa | jednorazowy (consumable) | 14,99 zł |

**Opisy produktów:**

- **Premium:**
  - PL: „Zero reklam na zawsze, +50% monet po każdej grze, skórka Królewskie złoto i kumpel Król Bloop.”
  - EN: „No ads ever, +50% coins after every game, Royal Gold skin and King Bloop buddy.”
- **Przepustka sezonowa:**
  - PL: „Odblokowuje nagrody premium bieżącego sezonu.”
  - EN: „Unlocks the premium rewards of the current season.”

Ustaw status **Aktywny**.

## 6. Grafiki i zrzuty ekranu

1. W Unity w zakładce **Game** dodaj rozdzielczość **1080 × 1920**: menu rozdzielczości → „+”.
2. Uruchom Play i otwórz kolejne ekrany w tej kolejności (pasuje do podpisów):
   1. gra z klockami
   2. mapa poziomów
   3. miasto 3D
   4. las lub ogród
   5. wieża
   6. bieg 3D
   7. pupil
   8. garderoba
3. Na każdym ekranie naciśnij **Ctrl+Shift+K**, czyli *Stackville → Release → Capture store screenshot*. Pliki trafią do `Store/screenshots/raw/01.png, 02.png…`.
4. W terminalu w folderze repo uruchom:
   ```
   pip install pillow
   python Tools/StoreAssets/make_store_assets.py
   ```
   - Powstaną zrzuty z podpisami w `Store/screenshots/framed/pl-PL/` i `.../en-US/`.
   - Odświeżą się też ikona i grafiki promocyjne.
   - Podpisy zmienisz w `CAPTIONS` w tym skrypcie.

## 7. Wpis w sklepie i test zamknięty

1. **Rozwój → Główna strona sklepu:** wklej teksty z `fastlane/metadata/android/pl-PL/`, wgraj ikonę, grafikę i zrzuty.
   - Potem **Zarządzaj tłumaczeniami → angielski (en-US)** i wklej teksty z `en-US/`.
2. **Ustawienia sklepu:**
   - Kategoria: **Gry → Logiczne** (Puzzle).
   - Tagi: Klocki, Budowanie miast, Casual.
   - E-mail: wojciech.buda6@gmail.com.
   - Strona: `https://wojciechbuda.github.io/Game/` albo strona z kroku 9.
3. **Kraje:** wszystkie albo wybrane.
4. **Testowanie → Test zamknięty → Utwórz ścieżkę:**
   - Dodaj listę testerów (adresy Gmail, min. 12 osób).
   - Wgraj `.aab`, a jako informacje o wersji wklej `changelogs/1.txt`.
   - Wyślij do sprawdzenia. Pierwsza recenzja Google trwa zwykle od kilku dni do tygodnia.
5. Testerzy muszą **dołączyć przez link z Play Console i mieć grę zainstalowaną przez 14 dni**. Poproś ich, żeby grali i zgłaszali błędy.
   - W tym czasie wgrywaj poprawki: Bump version code → Build → nowa wersja w teście zamkniętym.
6. Po 14 dniach: **Panel → Złóż wniosek o dostęp do produkcji**.
   - Google pyta, jak przebiegał test i co poprawiłeś; odpowiedz konkretnie.
   - Po akceptacji: **Produkcja → Utwórz wersję**. Najlepiej zacznij od wdrożenia etapowego, np. 20%.

## 8. Podpisywanie przez Google (Play App Signing)

Przy pierwszym wgraniu Play Console zaproponuje **Podpisywanie aplikacji przez Google Play**: zaakceptuj.

- Google trzyma klucz aplikacji, a Ty podpisujesz tylko kluczem przesyłania (Twój keystore).
- SHA-1 do Gier Play i AdMob bierz z Integralność aplikacji → Podpisywanie aplikacji, sekcja „Certyfikat klucza podpisywania aplikacji”.

## 9. Strony internetowe (GitHub Pages)

1. **Polityka i regulamin:** repo `WojciechBuda/Game` → Settings → Pages → Source: *Deploy from a branch* → `main` / `(root)` → Save.
   - Po ok. minucie zadziałają:
     - https://wojciechbuda.github.io/Game/privacy-policy.html
     - https://wojciechbuda.github.io/Game/terms.html
   - Repo musi pozostać **publiczne**. Gdy zechcesz je ukryć, przenieś te 2 pliki HTML do osobnego publicznego repo.
2. **app-ads.txt** (zalecany przez AdMob; bez niego część reklam może nie być wyświetlana):
   1. Utwórz nowe publiczne repo o nazwie dokładnie `wojciechbuda.github.io`.
   2. Wrzuć do niego `Store/app-ads.txt`, z ID wydawcy AdMob zamiast `pub-0000000000000000`, oraz prosty `index.html`.
   3. Plik będzie pod https://wojciechbuda.github.io/app-ads.txt.
   4. W Play Console jako **Witryna** podaj `https://wojciechbuda.github.io`.
   5. AdMob sprawdzi plik w ciągu doby.
3. Przy zmianie polityki edytuj `Tools/PrivacyPolicy/policy.py` (zmień `DATE` i `VERSION`) i uruchom go. Zaktualizuje i stronę, i tekst w grze.

## 10. Lista kontrolna przed wysłaniem

- [ ] *Check release settings* nie pokazuje błędów (target API 36, ARM64, keystore, ADMOB, UNITY_PURCHASING, brak testowych ID reklam).
- [ ] Gra zainstalowana z .aab (np. przez test wewnętrzny) działa na prawdziwym telefonie:
  - komunikat zgody RODO
  - reklama z nagrodą
  - zakup testowy: dodaj swój Gmail w Ustawienia → Testowanie licencji
  - przywracanie zakupów
- [ ] Polityka prywatności otwiera się pod podanym URL i w grze (Ustawienia → Prywatność).
- [ ] Wypełnione: Treść aplikacji (wszystkie zielone), wpis w sklepie PL + EN, kategoria, kontakt.
- [ ] Produkty `stackville_premium` i `stackville_season_pass` aktywne.
- [ ] 12+ testerów w teście zamkniętym przez 14 dni.
- [ ] Zapisana kopia keystore i haseł w dwóch miejscach.

## Czego unikać (częste powody odrzucenia)

- Nazw cudzych gier i marek w tytule, opisie i słowach kluczowych (np. Tetris, Subway Surfers, Icy Tower, Pou). Teksty w tym folderze są od nich wolne.
- Zrzutów, które nie pokazują prawdziwej gry, albo obietnic typu „nr 1”, „najlepsza gra”.
- Testowych ID reklam w wersji produkcyjnej oraz klikania we własne reklamy (AdMob może zablokować konto). Do testów dodaj swój telefon jako urządzenie testowe w AdMob.
- Polityki prywatności niezgodnej z deklaracją Bezpieczeństwa danych. Obecne teksty są ze sobą spójne.
