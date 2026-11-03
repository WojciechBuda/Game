# Source of the Stackville privacy policy. Run `python3 policy.py` to regenerate:
#   - privacy-policy.html (repo root, for GitHub Pages / the store listing)
#   - Assets/Scripts/Game/Interface/PrivacyPolicyText.cs (shown inside the game)
# Edit CONTACT and DEVELOPER below before publishing. No personal names: the game is published as "Stackville".
import html, os

DEVELOPER = {"en": "the Stackville team", "pl": "zespół Stackville"}
CONTACT = "stackville.info@gmail.com"
DATE = {"en": "3 November 2026", "pl": "3 listopada 2026"}
VERSION = 1
# Neutral web address (GitHub organisation "stackville", repo "stackville.github.io"); see Store/README.md step 9.
SITE = "https://stackville.github.io"

EN = [
("Who we are", [
 f"Stackville is a game made by {DEVELOPER['en']} (\"we\", \"us\"). This policy explains what data the game uses, why, and the choices you have. Questions: {CONTACT}."]),
("In short", [
 "You can play without an account. Your progress is saved on your phone, not on our servers - we don't run any servers.",
 "The free version shows ads from Google AdMob. Google may use your advertising ID to show and measure ads. Where the law requires it, you are asked for consent first and can change your choice in Settings at any time.",
 "Signing in with Google is optional. We never sell your data."]),
("Data saved on your device", [
 "Your progress (levels, scores, coins, shards, supplies, city, forest and garden islands, mushrooms, pet, wardrobe, towers, lucky wheel, season pass) and your settings (volume, vibration, language) are stored only on your device.",
 "This data is not sent to us. It is removed when you uninstall the game or tap Settings > Delete my data."]),
("Google sign-in (optional)", [
 "If you choose to sign in with Google Play Games, Google shares your Play Games player ID, display name and profile picture with the game. We use them only to sign you in and, in future versions, for leaderboards and challenges with friends.",
 "You can sign out or disconnect the game at any time in the Google Play Games app. Google's privacy policy applies: https://policies.google.com/privacy"]),
("Advertising (free version)", [
 "The free version shows ads served by Google AdMob. To show ads, limit how often you see them, measure them and prevent fraud, Google may collect your device's advertising ID, IP address, device and app information, and how you interact with ads.",
 "If you are in the EEA, the UK or Switzerland, Google's consent form asks whether ads may be personalised. You can change your choice in Settings > Privacy choices. You can also reset or delete your advertising ID in your phone's settings.",
 "Ads in the game are limited to content rated suitable for teenagers. Buying Premium removes ads.",
 "How Google uses data: https://policies.google.com/technologies/partner-sites"]),
("In-app purchases", [
 "Purchases are handled by Google Play (or the Apple App Store). We only receive a confirmation that the purchase was made, so the game can unlock it. We never see your payment details."]),
("Game engine", [
 "Stackville is built with Unity. The Unity engine may collect basic technical information (device model, operating system, crashes) to keep the game working. See https://unity.com/legal/game-player-and-app-user-privacy-policy"]),
("What we don't collect", [
 "We don't ask for your name, e-mail or age, and we don't access your location, contacts, photos, camera or microphone. There is no chat between players. We don't sell or rent personal data."]),
("Permissions", [
 "Internet - for ads, sign-in and purchases. Vibration - for haptic feedback (can be turned off in Settings)."]),
("Children", [
 "Stackville is not directed at children under 13 and we don't knowingly collect personal data from them. Ads are limited to teen-suitable content, and personalised ads are only shown with consent where the law requires it. If you are a parent and believe your child has shared data with us, contact us and we will help remove it."]),
("Your rights", [
 "Depending on where you live (for example under the GDPR), you may have the right to access, correct or delete your data and to object to its use. Most data never leaves your device, so you can delete it yourself with Settings > Delete my data. For data held by Google, use Google's tools (https://myaccount.google.com) or contact us and we will help."]),
("How long data is kept", [
 "Data on your device is kept until you delete it or uninstall the game. Google and Unity keep data according to their own policies."]),
("Changes", [
 "If this policy changes, we will update the date below and show the new version in the game."]),
("Contact", [
 f"Stackville - {CONTACT}"]),
]

PL = [
("Kim jesteśmy", [
 f"Stackville to gra stworzona przez {DEVELOPER['pl']} (\"my\"). Ta polityka wyjaśnia, jakich danych używa gra, po co i jakie masz wybory. Pytania: {CONTACT}."]),
("W skrócie", [
 "Możesz grać bez zakładania konta. Postęp zapisuje się na Twoim telefonie, a nie na naszych serwerach - nie prowadzimy żadnych serwerów.",
 "Darmowa wersja wyświetla reklamy Google AdMob. Google może używać Twojego identyfikatora reklamowego do wyświetlania i mierzenia reklam. Tam, gdzie wymaga tego prawo, najpierw prosimy o zgodę, którą możesz zmienić w Ustawieniach w dowolnej chwili.",
 "Logowanie kontem Google jest dobrowolne. Nigdy nie sprzedajemy Twoich danych."]),
("Dane zapisane na urządzeniu", [
 "Twój postęp (poziomy, wyniki, monety, odłamki, surowce, miasto, wyspy las i ogród, grzyby, pupil, garderoba, wieże, koło fortuny, przepustka sezonowa) i ustawienia (głośność, wibracje, język) są przechowywane wyłącznie na Twoim urządzeniu.",
 "Te dane nie są do nas wysyłane. Usuwasz je, odinstalowując grę lub wybierając Ustawienia > Usuń moje dane."]),
("Logowanie Google (dobrowolne)", [
 "Jeśli zalogujesz się przez Google Play Games, Google przekaże grze Twój identyfikator gracza, nazwę i zdjęcie profilowe. Używamy ich tylko do logowania, a w przyszłych wersjach do rankingów i wyzwań ze znajomymi.",
 "W każdej chwili możesz się wylogować lub odłączyć grę w aplikacji Google Play Games. Obowiązuje polityka prywatności Google: https://policies.google.com/privacy"]),
("Reklamy (wersja darmowa)", [
 "Darmowa wersja wyświetla reklamy Google AdMob. Aby wyświetlać reklamy, ograniczać ich częstotliwość, mierzyć je i zapobiegać oszustwom, Google może zbierać identyfikator reklamowy urządzenia, adres IP, informacje o urządzeniu i aplikacji oraz to, jak korzystasz z reklam.",
 "Jeśli jesteś w EOG, Wielkiej Brytanii lub Szwajcarii, formularz zgody Google zapyta, czy reklamy mogą być spersonalizowane. Wybór zmienisz w Ustawienia > Wybory prywatności. Identyfikator reklamowy możesz też zresetować lub usunąć w ustawieniach telefonu.",
 "Reklamy w grze są ograniczone do treści odpowiednich dla nastolatków. Zakup Premium usuwa reklamy.",
 "Jak Google używa danych: https://policies.google.com/technologies/partner-sites"]),
("Zakupy w aplikacji", [
 "Płatności obsługuje Google Play (lub Apple App Store). Otrzymujemy tylko potwierdzenie zakupu, żeby gra mogła go odblokować. Nigdy nie widzimy danych płatniczych."]),
("Silnik gry", [
 "Stackville powstał w Unity. Silnik Unity może zbierać podstawowe informacje techniczne (model urządzenia, system, awarie), żeby gra działała poprawnie. Zobacz https://unity.com/legal/game-player-and-app-user-privacy-policy"]),
("Czego nie zbieramy", [
 "Nie pytamy o imię, e-mail ani wiek i nie mamy dostępu do lokalizacji, kontaktów, zdjęć, aparatu ani mikrofonu. W grze nie ma czatu między graczami. Nie sprzedajemy ani nie wynajmujemy danych osobowych."]),
("Uprawnienia", [
 "Internet - do reklam, logowania i zakupów. Wibracje - do wibracji w grze (można je wyłączyć w Ustawieniach)."]),
("Dzieci", [
 "Stackville nie jest skierowany do dzieci poniżej 13 lat i świadomie nie zbieramy od nich danych osobowych. Reklamy są ograniczone do treści odpowiednich dla nastolatków, a reklamy spersonalizowane pojawiają się tylko za zgodą tam, gdzie wymaga tego prawo. Jeśli jesteś rodzicem i uważasz, że Twoje dziecko przekazało nam dane, napisz do nas - pomożemy je usunąć."]),
("Twoje prawa", [
 "W zależności od miejsca zamieszkania (np. na podstawie RODO) możesz mieć prawo dostępu do danych, ich poprawienia lub usunięcia oraz sprzeciwu wobec ich wykorzystania. Większość danych nigdy nie opuszcza Twojego urządzenia, więc możesz je usunąć sam: Ustawienia > Usuń moje dane. W sprawie danych przechowywanych przez Google skorzystaj z narzędzi Google (https://myaccount.google.com) albo napisz do nas - pomożemy."]),
("Jak długo przechowujemy dane", [
 "Dane na urządzeniu są przechowywane, dopóki ich nie usuniesz lub nie odinstalujesz gry. Google i Unity przechowują dane zgodnie z własnymi zasadami."]),
("Zmiany", [
 "Jeśli polityka się zmieni, zaktualizujemy datę poniżej i pokażemy nową wersję w grze."]),
("Kontakt", [
 f"Stackville - {CONTACT}"]),
]

root = os.path.normpath(os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", ".."))

def cs(s): return s.replace("\\", "\\\\").replace("\"", "\\\"")

def cs_sections(name, secs):
    out = [f"        public static readonly string[][] {name} =", "        {"]
    for title, paras in secs:
        out.append("            new[] { " + ", ".join(f"\"{cs(x)}\"" for x in [title] + paras) + " },")
    out.append("        };")
    return "\n".join(out)

code = f'''// Generated by Tools/PrivacyPolicy/policy.py - edit the script, not this file.
namespace JellyDrop.Interface
{{
    /// <summary>The privacy policy shown in the game (English and Polish; other languages show English).</summary>
    public static class PrivacyPolicyText
    {{
        public const int Version = {VERSION};
        public const string Contact = "{cs(CONTACT)}";
        public const string DateEn = "{DATE['en']}";
        public const string DatePl = "{DATE['pl']}";

        /// <summary>Each entry: title, then paragraphs.</summary>
{cs_sections("English", EN)}

{cs_sections("Polish", PL)}
    }}
}}
'''
open(os.path.join(root, "Assets/Scripts/Game/Interface/PrivacyPolicyText.cs"), "w", encoding="utf-8").write(code)

def linkify(t):
    import re
    t = html.escape(t)
    return re.sub(r"(https://[^\s<]+)", r'<a href="\1">\1</a>', t)

def sec_html(secs):
    return "\n".join(f"<h2>{html.escape(t)}</h2>\n" + "\n".join(f"<p>{linkify(p)}</p>" for p in ps) for t, ps in secs)

page = f'''<!doctype html>
<html lang="en">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>Stackville - Privacy Policy</title>
<style>
:root {{ --bg:#f7f6fb; --ink:#1d1b33; --dim:#5d5a7a; --accent:#ff8a2a; --card:#ffffff; }}
@media (prefers-color-scheme: dark) {{ :root {{ --bg:#14152e; --ink:#eceaf8; --dim:#a7a4c6; --card:#1e2042; }} }}
body {{ margin:0; background:var(--bg); color:var(--ink); font:16px/1.6 system-ui,-apple-system,"Segoe UI",Roboto,sans-serif; }}
main {{ max-width:760px; margin:0 auto; padding:32px 16px 64px; }}
h1 {{ font-size:2rem; margin:0 0 4px; }}
h1 span {{ color:var(--accent); }}
h2 {{ font-size:1.15rem; margin:28px 0 6px; }}
p {{ margin:0 0 10px; }}
.date {{ color:var(--dim); margin-bottom:24px; }}
nav a {{ margin-right:16px; }}
a {{ color:var(--accent); word-break:break-word; }}
section {{ background:var(--card); border-radius:16px; padding:8px 20px 16px; margin-top:24px; }}
</style>
</head>
<body>
<main>
<h1><span>Stackville</span> Privacy Policy</h1>
<nav><a href="#en">English</a><a href="#pl">Polski</a></nav>
<section id="en">
<p class="date">Effective: {DATE['en']}</p>
{sec_html(EN)}
</section>
<section id="pl" lang="pl">
<h1 style="font-size:1.5rem;margin-top:16px">Polityka prywatności</h1>
<p class="date">Obowiązuje od: {DATE['pl']}</p>
{sec_html(PL)}
</section>
</main>
</body>
</html>
'''
open(os.path.join(root, "privacy-policy.html"), "w", encoding="utf-8").write(page)

# Site/ is everything for the neutral website (upload the folder's files to the stackville.github.io repo).
site = os.path.join(root, "Site")
os.makedirs(site, exist_ok=True)
open(os.path.join(site, "privacy-policy.html"), "w", encoding="utf-8").write(page)
import shutil
shutil.copyfile(os.path.join(root, "terms.html"), os.path.join(site, "terms.html"))
index = f"""<!doctype html>
<html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1">
<title>Stackville</title>
<style>body{{margin:0;background:#14152e;color:#eceaf8;font:17px/1.6 system-ui,sans-serif}}main{{max-width:640px;margin:0 auto;padding:48px 16px}}
h1{{font-size:2.4rem;margin:0;color:#ff8a2a}}a{{color:#ffc93c}}</style></head>
<body><main><h1>Stackville</h1>
<p>Stack blocks, build a town, pick mushrooms in the forest and raise a pet. A free game for Android.</p>
<p>Układaj klocki, buduj miasto, zbieraj grzyby w lesie i dbaj o pupila. Darmowa gra na Androida.</p>
<p><a href="privacy-policy.html">Privacy Policy / Polityka prywatności</a><br><a href="terms.html">Terms of Use / Regulamin</a></p>
<p>Contact / Kontakt: <a href="mailto:{CONTACT}">{CONTACT}</a></p>
</main></body></html>
"""
open(os.path.join(site, "index.html"), "w", encoding="utf-8").write(index)
ads = os.path.join(site, "app-ads.txt")
if not os.path.exists(ads):
    shutil.copyfile(os.path.join(root, "Store", "app-ads.txt"), ads)
print("written")
