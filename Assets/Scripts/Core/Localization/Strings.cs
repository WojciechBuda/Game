namespace JellyDrop.Core
{
    /// <summary>
    /// All game text. Column order in every row: English, Polish, German, Spanish, French, Portuguese.
    /// Keep placeholders ({0}, {1}...) identical across a row: a unit test checks this.
    /// </summary>
    public static partial class Loc
    {
        private static void Build()
        {
            // ---------------------------------------------------------------- common
            Same("common.ok", "OK");
            Add("common.cancel", "CANCEL", "ANULUJ", "ABBRECHEN", "CANCELAR", "ANNULER", "CANCELAR");
            Add("common.yay", "YAY!", "HURA!", "JUHU!", "¡BIEN!", "YOUPI !", "OBA!");
            Add("common.awesome", "AWESOME!", "SUPER!", "KLASSE!", "¡GENIAL!", "GÉNIAL !", "DEMAIS!");
            Add("common.done", "DONE", "GOTOWE", "FERTIG", "HECHO", "FAIT", "FEITO");
            Add("common.close", "CLOSE", "ZAMKNIJ", "SCHLIESSEN", "CERRAR", "FERMER", "FECHAR");
            Add("common.claim", "CLAIM {0}", "ODBIERZ {0}", "ABHOLEN {0}", "RECLAMAR {0}", "RÉCUPÉRER {0}", "RESGATAR {0}");
            Add("common.tomorrow", "TOMORROW", "JUTRO", "MORGEN", "MAÑANA", "DEMAIN", "AMANHÃ");
            Add("common.on", "ON", "WŁ.", "AN", "SÍ", "OUI", "SIM");
            Add("common.off", "OFF", "WYŁ.", "AUS", "NO", "NON", "NÃO");
            Add("common.restore", "Restore purchases", "Przywróć zakupy", "Käufe wiederherstellen", "Restaurar compras", "Restaurer les achats", "Restaurar compras");

            // Plural units (see Loc.PluralKey). Polish uses one / few / many; the others one / many.
            Add("unit.coin.one", "coin", "moneta", "Münze", "moneda", "pièce", "moeda");
            Add("unit.coin.few", "coins", "monety", "Münzen", "monedas", "pièces", "moedas");
            Add("unit.coin.many", "coins", "monet", "Münzen", "monedas", "pièces", "moedas");

            // ---------------------------------------------------------------- home
            Add("home.play", "PLAY", "GRAJ", "SPIELEN", "JUGAR", "JOUER", "JOGAR");
            Same("home.sprint", "SPRINT 40");
            Add("home.daily", "DAILY", "DZIENNE", "TÄGLICH", "DIARIO", "DÉFI DU JOUR", "DIÁRIO");
            Add("home.shop", "SHOP", "SKLEP", "SHOP", "TIENDA", "BOUTIQUE", "LOJA");
            Add("home.missions", "MISSIONS", "MISJE", "MISSIONEN", "MISIONES", "MISSIONS", "MISSÕES");
            Add("home.noads", "NO ADS", "BEZ REKLAM", "OHNE WERBUNG", "SIN ANUNCIOS", "SANS PUB", "SEM ANÚNCIOS");
            Add("home.best", "BEST ON {0}  {1}", "REKORD ({0})  {1}", "REKORD ({0})  {1}", "RÉCORD ({0})  {1}", "RECORD ({0})  {1}", "RECORDE ({0})  {1}");
            Add("home.pick", "Pick a difficulty and tap PLAY!", "Wybierz poziom i naciśnij GRAJ!", "Wähle die Schwierigkeit und tippe auf SPIELEN!",
                "¡Elige la dificultad y pulsa JUGAR!", "Choisis la difficulté et appuie sur JOUER !", "Escolha a dificuldade e toque em JOGAR!");
            Add("home.daily_today", "today: {0}", "dziś: {0}", "heute: {0}", "hoy: {0}", "aujourd'hui : {0}", "hoje: {0}");
            Add("home.daily_info", "same pieces for all · normal", "te same klocki dla wszystkich · normalny", "gleiche Steine für alle · normal",
                "mismas piezas para todos · normal", "mêmes pièces pour tous · normal", "mesmas peças para todos · normal");
            Add("home.free_coins", "FREE +{0} COINS  ({1}/{2})", "DARMOWE MONETY +{0}  ({1}/{2})", "GRATIS +{0} MÜNZEN  ({1}/{2})",
                "+{0} MONEDAS GRATIS  ({1}/{2})", "+{0} PIÈCES GRATUITES  ({1}/{2})", "+{0} MOEDAS GRÁTIS  ({1}/{2})");
            Add("home.free_coins_tomorrow", "FREE COINS - BACK TOMORROW", "DARMOWE MONETY - WRÓĆ JUTRO", "GRATIS-MÜNZEN - MORGEN WIEDER",
                "MONEDAS GRATIS - VUELVE MAÑANA", "PIÈCES GRATUITES - REVIENS DEMAIN", "MOEDAS GRÁTIS - VOLTE AMANHÃ");
            Add("home.quit_title", "Leave Jelly Drop?", "Wyjść z Jelly Drop?", "Jelly Drop verlassen?", "¿Salir de Jelly Drop?", "Quitter Jelly Drop ?", "Sair do Jelly Drop?");
            Add("home.quit_body", "See you soon!", "Do zobaczenia!", "Bis bald!", "¡Hasta pronto!", "À bientôt !", "Até logo!");
            Add("home.quit", "QUIT", "WYJDŹ", "BEENDEN", "SALIR", "QUITTER", "SAIR");

            // ---------------------------------------------------------------- difficulty
            Add("difficulty.easy", "EASY", "ŁATWY", "LEICHT", "FÁCIL", "FACILE", "FÁCIL");
            Add("difficulty.normal", "NORMAL", "NORMALNY", "NORMAL", "NORMAL", "NORMAL", "NORMAL");
            Add("difficulty.hard", "HARD", "TRUDNY", "SCHWER", "DIFÍCIL", "DIFFICILE", "DIFÍCIL");
            Add("difficulty.insane", "INSANE", "SZALONY", "IRRE", "LOCO", "INFERNAL", "INSANO");
            Add("difficulty.easy.desc",
                "Slow and relaxed. Extra previews, more power-ups.",
                "Wolno i spokojnie. Więcej podglądów i wzmocnień.",
                "Langsam und entspannt. Mehr Vorschau, mehr Power-ups.",
                "Lento y relajado. Más vistas previas y potenciadores.",
                "Lent et détendu. Plus d'aperçus et de bonus.",
                "Lento e tranquilo. Mais prévias e power-ups.");
            Add("difficulty.normal.desc", "The classic experience.", "Klasyczna rozgrywka.", "Das klassische Spielerlebnis.",
                "La experiencia clásica.", "L'expérience classique.", "A experiência clássica.");
            Add("difficulty.hard.desc",
                "Fast start, junk rows, less time to slide. x1.5 coins.",
                "Szybki start, śmieciowe rzędy, mniej czasu. x1,5 monet.",
                "Schneller Start, Schrottreihen, weniger Zeit. x1,5 Münzen.",
                "Inicio rápido, filas basura, menos tiempo. x1,5 monedas.",
                "Départ rapide, lignes de débris, moins de temps. x1,5 pièces.",
                "Início rápido, linhas de lixo, menos tempo. x1,5 moedas.");
            Add("difficulty.insane.desc",
                "No ghost, one preview, rising junk, no power-ups. x2.5 coins!",
                "Bez cienia, jeden podgląd, rosnące śmieci, bez wzmocnień. x2,5 monet!",
                "Kein Schatten, eine Vorschau, steigender Schrott, keine Power-ups. x2,5 Münzen!",
                "Sin sombra, una vista previa, basura que sube, sin potenciadores. ¡x2,5 monedas!",
                "Pas d'ombre, un aperçu, débris montants, pas de bonus. x2,5 pièces !",
                "Sem sombra, uma prévia, lixo subindo, sem power-ups. x2,5 moedas!");

            // ---------------------------------------------------------------- in-game HUD
            Add("hud.hold", "HOLD", "SCHOWEK", "HALTEN", "GUARDAR", "RÉSERVE", "GUARDAR");
            Add("hud.next", "NEXT", "NASTĘPNY", "NÄCHSTER", "SIGUIENTE", "SUIVANT", "PRÓXIMA");
            Add("hud.lines_left", "{0} LINES LEFT", "ZOSTAŁO LINII: {0}", "NOCH {0} REIHEN", "QUEDAN {0} LÍNEAS", "ENCORE {0} LIGNES", "FALTAM {0} LINHAS");
            Add("hud.level_lines", "LEVEL {0}  ·  {1} LINES", "POZIOM {0}  ·  LINIE: {1}", "LEVEL {0}  ·  {1} REIHEN",
                "NIVEL {0}  ·  {1} LÍNEAS", "NIVEAU {0}  ·  {1} LIGNES", "NÍVEL {0}  ·  {1} LINHAS");
            Add("hud.no_powerups_on", "no power-ups on {0}", "brak wzmocnień na poziomie {0}", "keine Power-ups auf {0}",
                "sin potenciadores en {0}", "pas de bonus en {0}", "sem power-ups no {0}");
            Add("hud.powerups_left", "power-ups this game: {0}", "wzmocnienia w tej grze: {0}", "Power-ups in diesem Spiel: {0}",
                "potenciadores esta partida: {0}", "bonus pour cette partie : {0}", "power-ups nesta partida: {0}");
            Add("hud.powerups_none", "no more power-ups this game", "koniec wzmocnień w tej grze", "keine Power-ups mehr in diesem Spiel",
                "no quedan potenciadores", "plus de bonus pour cette partie", "acabaram os power-ups");
            Add("hud.slowmo", "SLOW-MO {0}s", "SPOWOLNIENIE {0}s", "ZEITLUPE {0}s", "CÁMARA LENTA {0}s", "RALENTI {0}s", "CÂMERA LENTA {0}s");
            Add("hud.junk_in", "JUNK IN {0}", "ŚMIECI ZA {0}", "SCHROTT IN {0}", "BASURA EN {0}", "DÉBRIS DANS {0}", "LIXO EM {0}");
            Add("hud.out_title", "Out of power-ups", "Brak wzmocnień", "Keine Power-ups mehr", "Sin potenciadores", "Plus de bonus", "Sem power-ups");
            Add("hud.out_body",
                "Buy more in the Shop with coins,\nor win them on the lucky wheel!",
                "Kup więcej w sklepie za monety\nalbo wygraj je na kole fortuny!",
                "Kauf mehr im Shop mit Münzen\noder gewinne sie am Glücksrad!",
                "¡Compra más en la tienda con monedas\no gánalos en la rueda de la suerte!",
                "Achète-en dans la boutique avec des pièces\nou gagne-les à la roue de la chance !",
                "Compre mais na loja com moedas\nou ganhe na roleta da sorte!");

            // ---------------------------------------------------------------- floating texts
            Add("fx.nice", "Nice!", "Nieźle!", "Nett!", "¡Bien!", "Joli !", "Legal!");
            Add("fx.sweet", "Sweet!", "Słodko!", "Süß!", "¡Dulce!", "Trop bien !", "Doce!");
            Add("fx.delicious", "Delicious!", "Pycha!", "Lecker!", "¡Delicioso!", "Délicieux !", "Delicioso!");
            Add("fx.blast", "JELLY BLAST!", "EKSPLOZJA JELLY!", "JELLY-EXPLOSION!", "¡EXPLOSIÓN JELLY!", "EXPLOSION JELLY !", "EXPLOSÃO JELLY!");
            Add("fx.b2b", "BACK-TO-BACK\n{0}", "SERIA!\n{0}", "HINTEREINANDER!\n{0}", "¡SEGUIDAS!\n{0}", "À LA SUITE !\n{0}", "EM SEQUÊNCIA!\n{0}");
            Add("fx.combo", "COMBO x{0}", "KOMBO x{0}", "KOMBO x{0}", "COMBO x{0}", "COMBO x{0}", "COMBO x{0}");
            Add("fx.golden_piece", "Golden piece!", "Złoty klocek!", "Goldener Stein!", "¡Pieza dorada!", "Pièce dorée !", "Peça dourada!");
            Add("fx.golden_clear", "GOLDEN x{0}!\n+{1}", "ZŁOTO x{0}!\n+{1}", "GOLD x{0}!\n+{1}", "¡ORO x{0}!\n+{1}", "OR x{0} !\n+{1}", "OURO x{0}!\n+{1}");
            Add("fx.rainbow", "RAINBOW!\n+{0}", "TĘCZA!\n+{0}", "REGENBOGEN!\n+{0}", "¡ARCOÍRIS!\n+{0}", "ARC-EN-CIEL !\n+{0}", "ARCO-ÍRIS!\n+{0}");
            Add("fx.jackpot", "JACKPOT!!!\n+{0}", "JACKPOT!!!\n+{0}", "JACKPOT!!!\n+{0}", "¡¡¡PREMIO GORDO!!!\n+{0}", "JACKPOT !!!\n+{0}", "JACKPOT!!!\n+{0}");
            Add("fx.level", "LEVEL {0}!", "POZIOM {0}!", "LEVEL {0}!", "¡NIVEL {0}!", "NIVEAU {0} !", "NÍVEL {0}!");
            Add("fx.bomb_ready", "BOMB READY!", "BOMBA GOTOWA!", "BOMBE BEREIT!", "¡BOMBA LISTA!", "BOMBE PRÊTE !", "BOMBA PRONTA!");
            Add("fx.zap", "ZAP!", "ZAP!", "ZAP!", "¡ZAP!", "ZAP !", "ZAP!");
            Add("fx.slowmo", "SLOW-MO!", "SPOWOLNIENIE!", "ZEITLUPE!", "¡CÁMARA LENTA!", "RALENTI !", "CÂMERA LENTA!");
            Add("fx.stick", "STICK!", "KRESKA!", "STAB!", "¡PALO!", "BÂTON !", "PALITO!");
            Add("fx.junk", "JUNK RISING!", "ŚMIECI ROSNĄ!", "SCHROTT STEIGT!", "¡SUBE LA BASURA!", "LES DÉBRIS MONTENT !", "O LIXO ESTÁ SUBINDO!");

            // ---------------------------------------------------------------- power-ups
            Add("powerup.Bomb.name", "Jelly Bomb", "Żelkowa bomba", "Gelee-Bombe", "Bomba de gelatina", "Bombe de gelée", "Bomba de geleia");
            Add("powerup.Bomb.short", "BOMB", "BOMBA", "BOMBE", "BOMBA", "BOMBE", "BOMBA");
            Add("powerup.Bomb.desc",
                "Next piece is a bomb that blasts a 3x3 area",
                "Następny klocek to bomba niszcząca obszar 3x3",
                "Der nächste Stein ist eine Bombe, die 3x3 Felder sprengt",
                "La siguiente pieza es una bomba que destruye un área de 3x3",
                "La prochaine pièce est une bombe qui détruit une zone 3x3",
                "A próxima peça é uma bomba que destrói uma área 3x3");
            Add("powerup.LineZap.name", "Line Zap", "Zap linii", "Reihen-Zapper", "Rayo de línea", "Zap de ligne", "Raio de linha");
            Same("powerup.LineZap.short", "ZAP");
            Add("powerup.LineZap.desc", "Zaps away the bottom row", "Usuwa dolny rząd", "Entfernt die unterste Reihe",
                "Elimina la fila inferior", "Efface la ligne du bas", "Elimina a linha de baixo");
            Add("powerup.SlowMo.name", "Slow-Mo", "Spowolnienie", "Zeitlupe", "Cámara lenta", "Ralenti", "Câmera lenta");
            Add("powerup.SlowMo.short", "SLOW-MO", "SPOWOLNIENIE", "ZEITLUPE", "CÁMARA LENTA", "RALENTI", "CÂMERA LENTA");
            Add("powerup.SlowMo.desc", "Pieces fall slower for 15 seconds", "Klocki spadają wolniej przez 15 sekund",
                "Steine fallen 15 Sekunden lang langsamer", "Las piezas caen más lento durante 15 segundos",
                "Les pièces tombent plus lentement pendant 15 secondes", "As peças caem mais devagar por 15 segundos");
            Add("powerup.Swap.name", "Stick Swap", "Zamiana na kreskę", "Stab-Tausch", "Cambio a palo", "Échange bâton", "Troca por palito");
            Add("powerup.Swap.short", "STICK", "KRESKA", "STAB", "PALO", "BÂTON", "PALITO");
            Add("powerup.Swap.desc", "Turns the current piece into a long stick", "Zmienia obecny klocek w długą kreskę",
                "Verwandelt den aktuellen Stein in einen langen Stab", "Convierte la pieza actual en un palo largo",
                "Transforme la pièce actuelle en long bâton", "Transforma a peça atual em um palito longo");

            // ---------------------------------------------------------------- shop items (character names stay as in the catalog)
            Add("item.skin_neon", "Neon Arcade", "Neonowy automat", "Neon-Arcade", "Arcade neón", "Arcade néon", "Fliperama neon");
            Add("item.skin_jelly", "Jelly Classic", "Klasyczne żelki", "Gelee Klassik", "Gelatina clásica", "Gelée classique", "Geleia clássica");
            Add("item.skin_gummy", "Gummy Buddies", "Żelkowi kumple", "Gummi-Freunde", "Amigos de goma", "Copains gommeux", "Amigos de goma");
            Add("item.skin_pixel", "Pixel Pop", "Pikselowy pop", "Pixel-Pop", "Pop de píxeles", "Pixel Pop", "Pop de pixels");
            Add("item.skin_sprinkles", "Sprinkle Donut", "Pączek z posypką", "Streusel-Donut", "Dónut con virutas", "Donut aux vermicelles", "Donut granulado");
            Add("item.skin_gems", "Gem Drops", "Klejnoty", "Edelsteine", "Gemas", "Gemmes", "Gemas");
            Add("item.skin_plasma", "Vapor Plasma", "Plazma Vapor", "Vapor-Plasma", "Plasma vapor", "Plasma vapeur", "Plasma vapor");
            Add("item.skin_gold", "Royal Gold", "Królewskie złoto", "Königsgold", "Oro real", "Or royal", "Ouro real");
            Add("item.theme_arcade", "Neon Arcade", "Neonowy automat", "Neon-Arcade", "Arcade neón", "Arcade néon", "Fliperama neon");
            Add("item.theme_cyber", "Cyber Grid", "Cybersiatka", "Cyber-Gitter", "Cuadrícula cíber", "Grille cyber", "Grade cyber");
            Add("item.theme_candy", "Candy Land", "Kraina słodyczy", "Süßigkeitenland", "Tierra de dulces", "Pays des bonbons", "Terra dos doces");
            Add("item.theme_mint", "Mint Meadow", "Miętowa łąka", "Minzwiese", "Prado de menta", "Prairie menthe", "Prado de menta");
            Add("item.theme_cotton", "Cotton Cloud", "Watowa chmurka", "Zuckerwatte-Wolke", "Nube de algodón", "Nuage de coton", "Nuvem de algodão");
            Add("item.theme_soda", "Sunset Soda", "Oranżada o zmierzchu", "Abendrot-Limo", "Refresco al atardecer", "Soda du couchant", "Refri do pôr do sol");
            Add("item.theme_galaxy", "Bubblegum Galaxy", "Gumowa galaktyka", "Kaugummi-Galaxie", "Galaxia de chicle", "Galaxie chewing-gum", "Galáxia de chiclete");
            Add("item.avatar_king", "King Bloop", "Król Bloop", "König Bloop", "Rey Bloop", "Roi Bloop", "Rei Bloop");

            Add("rarity.Common", "COMMON", "ZWYKŁY", "NORMAL", "COMÚN", "COMMUN", "COMUM");
            Add("rarity.Rare", "RARE", "RZADKI", "SELTEN", "RARO", "RARE", "RARO");
            Add("rarity.Epic", "EPIC", "EPICKI", "EPISCH", "ÉPICO", "ÉPIQUE", "ÉPICO");
            Same("rarity.Premium", "PREMIUM");

            Add("music.Bubbly", "Bubbly", "Bąbelkowa", "Sprudelnd", "Burbujeante", "Pétillant", "Borbulhante");
            Add("music.Mellow", "Mellow", "Łagodna", "Sanft", "Suave", "Doux", "Suave");
            Same("music.Funky", "Funky");
            Add("music.Space", "Space", "Kosmiczna", "Weltraum", "Espacial", "Spatial", "Espacial");
            Add("music.Dreamy", "Dreamy", "Marzycielska", "Verträumt", "Soñadora", "Rêveur", "Sonhadora");
            Same("music.Arcade", "Arcade");

            // ---------------------------------------------------------------- missions ({0} = target)
            Add("mission.ClearLines", "Clear {0} lines", "Wyczyść linie: {0}", "Räume {0} Reihen ab", "Completa {0} líneas", "Efface {0} lignes", "Limpe {0} linhas");
            Add("mission.ClearQuads", "Clear 4 lines at once {0} times", "Wyczyść 4 linie naraz (razy: {0})", "Räume {0}-mal 4 Reihen auf einmal ab",
                "Completa 4 líneas a la vez {0} veces", "Efface 4 lignes d'un coup {0} fois", "Limpe 4 linhas de uma vez {0} vezes");
            Add("mission.PlayGames", "Play {0} games", "Rozegraj gry: {0}", "Spiele {0} Runden", "Juega {0} partidas", "Joue {0} parties", "Jogue {0} partidas");
            Add("mission.ReachScore", "Score {0} in one game", "Zdobądź {0} pkt w jednej grze", "Erziele {0} Punkte in einem Spiel",
                "Consigue {0} puntos en una partida", "Marque {0} points en une partie", "Faça {0} pontos em uma partida");
            Add("mission.CollectCoins", "Collect {0} coins while playing", "Zbierz monety w grze: {0}", "Sammle {0} Münzen beim Spielen",
                "Recoge {0} monedas jugando", "Ramasse {0} pièces en jouant", "Colete {0} moedas jogando");
            Add("mission.UsePowerUps", "Use {0} power-ups", "Użyj wzmocnień: {0}", "Nutze {0} Power-ups", "Usa {0} potenciadores", "Utilise {0} bonus", "Use {0} power-ups");
            Add("mission.GoldenClears", "Clear lines with a golden piece: {0}", "Wyczyść linie złotym klockiem: {0}", "Reihen mit goldenem Stein abräumen: {0}",
                "Completa líneas con una pieza dorada: {0}", "Efface des lignes avec une pièce dorée : {0}", "Limpe linhas com uma peça dourada: {0}");
            Add("mission.MaxCombo", "Make a {0}x combo", "Zrób kombo x{0}", "Schaffe eine {0}x-Kombo", "Haz un combo x{0}", "Fais un combo x{0}", "Faça um combo x{0}");

            // ---------------------------------------------------------------- game over
            Add("go.game_over", "GAME OVER", "KONIEC GRY", "SPIEL VORBEI", "FIN DEL JUEGO", "PARTIE TERMINÉE", "FIM DE JOGO");
            Add("go.sprint_clear", "SPRINT CLEAR!", "SPRINT ZALICZONY!", "SPRINT GESCHAFFT!", "¡SPRINT COMPLETADO!", "SPRINT RÉUSSI !", "SPRINT CONCLUÍDO!");
            Add("go.daily_done", "DAILY DONE!", "DZIENNE ZALICZONE!", "TAGESSPIEL FERTIG!", "¡DIARIO HECHO!", "DÉFI DU JOUR FAIT !", "DIÁRIO CONCLUÍDO!");
            Add("go.new_best", "NEW BEST!", "NOWY REKORD!", "NEUER REKORD!", "¡NUEVO RÉCORD!", "NOUVEAU RECORD !", "NOVO RECORDE!");
            Add("go.spin", "SPIN!", "KRĘĆ!", "DREHEN!", "¡GIRAR!", "TOURNER !", "GIRAR!");
            Add("go.free_spin", "free spin\nevery game!", "darmowy obrót\npo każdej grze!", "Gratis-Dreh\nnach jedem Spiel!",
                "¡giro gratis\ncada partida!", "tour gratuit\nà chaque partie !", "giro grátis\na cada partida!");
            Add("go.odds", "odds", "szanse", "Chancen", "probabilidad", "chances", "chances");
            Add("go.home", "HOME", "MENU", "MENÜ", "INICIO", "ACCUEIL", "INÍCIO");
            Add("go.again", "AGAIN", "JESZCZE", "NOCHMAL", "OTRA VEZ", "REJOUER", "DE NOVO");
            Add("go.best_time", "{0}  ·  best time {1}  ·  {2} lines", "{0}  ·  najlepszy czas {1}  ·  linie: {2}", "{0}  ·  Bestzeit {1}  ·  {2} Reihen",
                "{0}  ·  mejor tiempo {1}  ·  {2} líneas", "{0}  ·  meilleur temps {1}  ·  {2} lignes", "{0}  ·  melhor tempo {1}  ·  {2} linhas");
            Add("go.lines_only", "{0}  ·  {1} lines", "{0}  ·  linie: {1}", "{0}  ·  {1} Reihen", "{0}  ·  {1} líneas", "{0}  ·  {1} lignes", "{0}  ·  {1} linhas");
            Add("go.summary", "{0}  ·  best {1}  ·  {2} lines  ·  level {3}", "{0}  ·  rekord {1}  ·  linie: {2}  ·  poziom {3}",
                "{0}  ·  Rekord {1}  ·  {2} Reihen  ·  Level {3}", "{0}  ·  récord {1}  ·  {2} líneas  ·  nivel {3}",
                "{0}  ·  record {1}  ·  {2} lignes  ·  niveau {3}", "{0}  ·  recorde {1}  ·  {2} linhas  ·  nível {3}");
            Add("go.score_bonus", "score bonus", "bonus za wynik", "Punktebonus", "bonus por puntos", "bonus de score", "bônus de pontos");
            Add("go.collected", "collected", "zebrane", "gesammelt", "recogidas", "ramassées", "coletadas");
            Add("go.new_best_bonus", "new best", "nowy rekord", "neuer Rekord", "nuevo récord", "nouveau record", "novo recorde");
            Add("go.sprint_bonus", "sprint clear", "sprint zaliczony", "Sprint geschafft", "sprint completado", "sprint réussi", "sprint concluído");
            Add("go.premium_bonus", "premium x{0}", "premium x{0}", "Premium x{0}", "premium x{0}", "premium x{0}", "premium x{0}");
            Add("go.mission_done", "Mission complete!  {0}\nclaim it in MISSIONS", "Misja wykonana!  {0}\nodbierz w MISJACH",
                "Mission erfüllt!  {0}\nhol sie dir unter MISSIONEN", "¡Misión cumplida!  {0}\nreclámala en MISIONES",
                "Mission accomplie !  {0}\nrécupère-la dans MISSIONS", "Missão cumprida!  {0}\nresgate em MISSÕES");
            Add("go.lucky_spin", "Lucky spin!", "Szczęśliwy obrót!", "Glücksdreh!", "¡Giro de la suerte!", "Tour chanceux !", "Giro da sorte!");
            Add("go.shard_body", "Collect shards to unlock epic items!", "Zbieraj kryształy, by odblokować epickie przedmioty!",
                "Sammle Splitter für epische Items!", "¡Reúne fragmentos para desbloquear objetos épicos!",
                "Collecte des éclats pour débloquer des objets épiques !", "Junte fragmentos para liberar itens épicos!");
            Add("go.free_powerup", "A free power-up!", "Darmowe wzmocnienie!", "Ein Gratis-Power-up!", "¡Un potenciador gratis!", "Un bonus gratuit !", "Um power-up grátis!");
            Add("go.odds_title", "Wheel odds", "Szanse na kole", "Glücksrad-Chancen", "Probabilidades de la rueda", "Chances de la roue", "Chances da roleta");
            Add("go.odds_shard", "1 shard", "1 kryształ", "1 Splitter", "1 fragmento", "1 éclat", "1 fragmento");
            Add("go.odds_powerup", "1 power-up", "1 wzmocnienie", "1 Power-up", "1 potenciador", "1 bonus", "1 power-up");
            Add("wheel.power", "POWER", "MOC", "POWER", "PODER", "BONUS", "PODER");
            Add("wheel.shard", "SHARD", "KRYSZTAŁ", "SPLITTER", "FRAGM.", "ÉCLAT", "FRAGM.");

            // ---------------------------------------------------------------- shop
            Add("shop.tab.skins", "SKINS", "SKÓRKI", "SKINS", "ASPECTOS", "SKINS", "VISUAIS");
            Add("shop.tab.themes", "THEMES", "MOTYWY", "THEMEN", "TEMAS", "THÈMES", "TEMAS");
            Add("shop.tab.buddies", "BUDDIES", "KUMPLE", "FREUNDE", "AMIGOS", "COPAINS", "AMIGOS");
            Add("shop.tab.power", "POWER", "MOC", "POWER", "PODER", "BONUS", "PODER");
            Same("shop.tab.premium", "PREMIUM");
            Add("shop.equipped", "EQUIPPED", "ZAŁOŻONE", "AUSGERÜSTET", "EQUIPADO", "ÉQUIPÉ", "EQUIPADO");
            Add("shop.equip", "EQUIP", "ZAŁÓŻ", "AUSRÜSTEN", "EQUIPAR", "ÉQUIPER", "EQUIPAR");
            Same("shop.premium_only", "PREMIUM");
            Add("shop.unlocked", "UNLOCKED!", "ODBLOKOWANE!", "FREIGESCHALTET!", "¡DESBLOQUEADO!", "DÉBLOQUÉ !", "DESBLOQUEADO!");
            Add("shop.is_yours", "{0} is yours!", "Masz: {0}!", "{0} gehört dir!", "¡{0} es tuyo!", "{0} est à toi !", "{0} é seu!");
            Add("shop.need_shards_title", "Need more shards", "Potrzeba więcej kryształów", "Mehr Splitter nötig", "Necesitas más fragmentos", "Il faut plus d'éclats", "Precisa de mais fragmentos");
            Add("shop.need_shards_body", "Win shards on the lucky wheel after each game!", "Wygrywaj kryształy na kole fortuny po każdej grze!",
                "Gewinne Splitter am Glücksrad nach jedem Spiel!", "¡Gana fragmentos en la rueda tras cada partida!",
                "Gagne des éclats à la roue après chaque partie !", "Ganhe fragmentos na roleta após cada partida!");
            Add("shop.almost", "Almost there!", "Prawie!", "Fast geschafft!", "¡Casi!", "Presque !", "Quase lá!");
            Add("shop.need_coins_ad", "You need {0} more.\nWatch a short video for +{1}?", "Potrzebujesz jeszcze: {0}.\nObejrzeć krótki film za +{1}?",
                "Dir fehlen {0}.\nKurzes Video für +{1} ansehen?", "Te faltan {0}.\n¿Ver un vídeo corto por +{1}?",
                "Il te manque {0}.\nRegarder une courte vidéo pour +{1} ?", "Faltam {0}.\nAssistir a um vídeo curto por +{1}?");
            Add("shop.need_coins_play", "You need {0} more.\nPlay a few games and spin the wheel!", "Potrzebujesz jeszcze: {0}.\nZagraj kilka razy i zakręć kołem!",
                "Dir fehlen {0}.\nSpiel ein paar Runden und dreh am Rad!", "Te faltan {0}.\n¡Juega unas partidas y gira la rueda!",
                "Il te manque {0}.\nJoue quelques parties et tourne la roue !", "Faltam {0}.\nJogue algumas partidas e gire a roleta!");
            Add("shop.watch", "WATCH", "OBEJRZYJ", "ANSEHEN", "VER", "REGARDER", "ASSISTIR");
            Add("shop.you_have", "you have {0}", "masz: {0}", "du hast {0}", "tienes {0}", "tu en as {0}", "você tem {0}");
            Add("shop.you_are_premium", "YOU ARE PREMIUM", "MASZ PREMIUM", "DU BIST PREMIUM", "ERES PREMIUM", "TU ES PREMIUM", "VOCÊ É PREMIUM");
            Add("shop.go_premium", "GO PREMIUM", "KUP PREMIUM", "HOL DIR PREMIUM", "HAZTE PREMIUM", "PASSE PREMIUM", "SEJA PREMIUM");
            Add("shop.perk1", "No ads. Ever.", "Zero reklam. Na zawsze.", "Keine Werbung. Nie mehr.", "Sin anuncios. Nunca.", "Zéro pub. Pour toujours.", "Sem anúncios. Nunca.");
            Add("shop.perk2", "+50% coins after every game", "+50% monet po każdej grze", "+50 % Münzen nach jedem Spiel",
                "+50 % de monedas tras cada partida", "+50 % de pièces après chaque partie", "+50% de moedas após cada partida");
            Add("shop.perk3", "Royal Gold block skin", "Skórka Królewskie złoto", "Stein-Skin Königsgold", "Aspecto Oro real", "Skin Or royal", "Visual Ouro real");
            Add("shop.perk4", "King Bloop buddy", "Kumpel Król Bloop", "Freund König Bloop", "Amigo Rey Bloop", "Copain Roi Bloop", "Amigo Rei Bloop");
            Add("shop.perk5", "Free bonus videos still available", "Darmowe filmy z bonusem nadal dostępne", "Gratis-Bonusvideos weiterhin verfügbar",
                "Los vídeos con premio siguen disponibles", "Les vidéos bonus restent disponibles", "Vídeos de bônus continuam disponíveis");
            Add("shop.thanks", "Thank you for supporting Jelly Drop!", "Dziękujemy za wsparcie Jelly Drop!", "Danke, dass du Jelly Drop unterstützt!",
                "¡Gracias por apoyar Jelly Drop!", "Merci de soutenir Jelly Drop !", "Obrigado por apoiar o Jelly Drop!");
            Add("shop.buy", "BUY  {0}", "KUP  {0}", "KAUFEN  {0}", "COMPRAR  {0}", "ACHETER  {0}", "COMPRAR  {0}");
            Add("shop.one_time", "one-time purchase", "jednorazowy zakup", "einmaliger Kauf", "compra única", "achat unique", "compra única");

            // ---------------------------------------------------------------- missions screen & daily gift
            Add("missions.daily_gift", "DAILY GIFT", "CODZIENNY PREZENT", "TAGESGESCHENK", "REGALO DIARIO", "CADEAU DU JOUR", "PRESENTE DIÁRIO");
            Add("missions.today", "TODAY'S MISSIONS", "DZISIEJSZE MISJE", "HEUTIGE MISSIONEN", "MISIONES DE HOY", "MISSIONS DU JOUR", "MISSÕES DE HOJE");
            Add("missions.complete", "Mission complete!", "Misja wykonana!", "Mission erfüllt!", "¡Misión cumplida!", "Mission accomplie !", "Missão cumprida!");
            Add("missions.free_coins", "FREE COINS", "DARMOWE MONETY", "GRATIS-MÜNZEN", "MONEDAS GRATIS", "PIÈCES GRATUITES", "MOEDAS GRÁTIS");
            Add("missions.watch_left", "Watch a short video: {0} left today", "Obejrzyj krótki film: na dziś zostało {0}", "Kurzes Video ansehen: heute noch {0}",
                "Mira un vídeo corto: quedan {0} hoy", "Regarde une courte vidéo : encore {0} aujourd'hui", "Assista a um vídeo curto: restam {0} hoje");
            Add("missions.back_tomorrow", "Back tomorrow!", "Wróć jutro!", "Morgen wieder!", "¡Vuelve mañana!", "Reviens demain !", "Volte amanhã!");
            Add("popup.day", "DAY {0}", "DZIEŃ {0}", "TAG {0}", "DÍA {0}", "JOUR {0}", "DIA {0}");
            Add("popup.come_back", "Come back every day for bigger gifts!", "Wracaj codziennie po większe prezenty!", "Komm jeden Tag wieder für größere Geschenke!",
                "¡Vuelve cada día para regalos mayores!", "Reviens chaque jour pour de plus gros cadeaux !", "Volte todo dia para presentes maiores!");
            Add("popup.gift_collected", "Daily gift collected!", "Prezent odebrany!", "Tagesgeschenk abgeholt!", "¡Regalo diario recogido!", "Cadeau du jour récupéré !", "Presente diário resgatado!");

            // ---------------------------------------------------------------- pause
            Add("pause.title", "PAUSED", "PAUZA", "PAUSE", "PAUSA", "PAUSE", "PAUSADO");
            Add("pause.resume", "RESUME", "WZNÓW", "WEITER", "CONTINUAR", "REPRENDRE", "CONTINUAR");
            Add("pause.restart", "RESTART", "OD NOWA", "NEUSTART", "REINICIAR", "RECOMMENCER", "REINICIAR");
            Add("pause.end", "END GAME", "ZAKOŃCZ GRĘ", "SPIEL BEENDEN", "TERMINAR", "TERMINER", "ENCERRAR");
            Add("pause.hint", "Ending early still pays the coins you collected.", "Po zakończeniu gry dostaniesz zebrane monety.",
                "Auch bei vorzeitigem Ende behältst du deine Münzen.", "Si terminas antes, conservas las monedas recogidas.",
                "En arrêtant plus tôt, tu gardes tes pièces.", "Encerrando antes, você mantém as moedas coletadas.");

            // ---------------------------------------------------------------- tutorial
            Add("tut.title", "HOW TO PLAY", "JAK GRAĆ", "SO WIRD GESPIELT", "CÓMO JUGAR", "COMMENT JOUER", "COMO JOGAR");
            Add("tut.tap", "TAP", "STUKNIJ", "TIPPEN", "TOCA", "TAPE", "TOQUE");
            Add("tut.tap_d", "rotate the piece", "obróć klocek", "Stein drehen", "girar la pieza", "tourner la pièce", "girar a peça");
            Add("tut.swipe", "SWIPE SIDEWAYS", "PRZESUŃ W BOK", "ZUR SEITE WISCHEN", "DESLIZA AL LADO", "GLISSE SUR LE CÔTÉ", "DESLIZE AO LADO");
            Add("tut.swipe_d", "move left / right", "ruch w lewo / prawo", "nach links / rechts", "mover izq. / der.", "gauche / droite", "mover esq. / dir.");
            Add("tut.drag", "DRAG DOWN", "PRZECIĄGNIJ W DÓŁ", "NACH UNTEN ZIEHEN", "ARRASTRA ABAJO", "GLISSE VERS LE BAS", "ARRASTE PARA BAIXO");
            Add("tut.drag_d", "fall faster", "szybsze spadanie", "schneller fallen", "caer más rápido", "tomber plus vite", "cair mais rápido");
            Add("tut.flick_down", "FLICK DOWN", "SZARPNIJ W DÓŁ", "NACH UNTEN SCHNIPPEN", "DESLIZA RÁPIDO ABAJO", "LANCE VERS LE BAS", "PUXE RÁPIDO P/ BAIXO");
            Add("tut.flick_down_d", "drop instantly", "upuść od razu", "sofort fallen lassen", "soltar al instante", "lâcher direct", "soltar na hora");
            Add("tut.flick_up", "FLICK UP", "SZARPNIJ W GÓRĘ", "NACH OBEN SCHNIPPEN", "DESLIZA RÁPIDO ARRIBA", "LANCE VERS LE HAUT", "PUXE RÁPIDO P/ CIMA");
            Add("tut.flick_up_d", "hold for later", "zachowaj na później", "für später halten", "guardar para luego", "garder pour plus tard", "guardar para depois");
            Add("tut.tip", "Clear lines to earn coins.\nGolden pieces pay x5!", "Czyść linie, by zdobywać monety.\nZłote klocki dają x5!",
                "Räume Reihen ab für Münzen.\nGoldene Steine zahlen x5!", "Completa líneas para ganar monedas.\n¡Las piezas doradas dan x5!",
                "Efface des lignes pour gagner des pièces.\nLes pièces dorées rapportent x5 !", "Limpe linhas para ganhar moedas.\nPeças douradas pagam x5!");
            Add("tut.go", "LET'S GO!", "JAZDA!", "LOS GEHT'S!", "¡VAMOS!", "C'EST PARTI !", "BORA!");

            // ---------------------------------------------------------------- settings
            Add("settings.title", "SETTINGS", "USTAWIENIA", "EINSTELLUNGEN", "AJUSTES", "PARAMÈTRES", "AJUSTES");
            Add("settings.music", "MUSIC", "MUZYKA", "MUSIK", "MÚSICA", "MUSIQUE", "MÚSICA");
            Add("settings.sound", "SOUNDS", "DŹWIĘKI", "SOUNDS", "SONIDOS", "SONS", "SONS");
            Add("settings.vibration", "VIBRATION", "WIBRACJE", "VIBRATION", "VIBRACIÓN", "VIBRATION", "VIBRAÇÃO");
            Add("settings.language", "LANGUAGE", "JĘZYK", "SPRACHE", "IDIOMA", "LANGUE", "IDIOMA");
            Add("settings.auto", "Automatic ({0})", "Automatycznie ({0})", "Automatisch ({0})", "Automático ({0})", "Automatique ({0})", "Automático ({0})");
            Add("settings.how", "How to play", "Jak grać", "So wird gespielt", "Cómo jugar", "Comment jouer", "Como jogar");
            Add("settings.privacy_choices", "Privacy choices", "Ustawienia prywatności", "Datenschutz-Optionen", "Opciones de privacidad", "Choix de confidentialité", "Opções de privacidade");
            Add("settings.privacy_policy", "Privacy policy", "Polityka prywatności", "Datenschutzerklärung", "Política de privacidad", "Politique de confidentialité", "Política de privacidade");

            // ---------------------------------------------------------------- ads & purchases
            Add("ads.done_title", "All done for today!", "To wszystko na dziś!", "Für heute erledigt!", "¡Listo por hoy!", "C'est tout pour aujourd'hui !", "Tudo pronto por hoje!");
            Add("ads.done_body", "You've watched today's {0} bonus videos.\nCome back tomorrow for more free coins!",
                "Dzisiejsze filmy z bonusem już obejrzane ({0}).\nWróć jutro po więcej darmowych monet!",
                "Du hast die {0} Bonusvideos von heute gesehen.\nKomm morgen für mehr Gratis-Münzen wieder!",
                "Ya viste los {0} vídeos con premio de hoy.\n¡Vuelve mañana por más monedas gratis!",
                "Tu as vu les {0} vidéos bonus du jour.\nReviens demain pour plus de pièces gratuites !",
                "Você já viu os {0} vídeos de bônus de hoje.\nVolte amanhã para mais moedas grátis!");
            Add("ads.thanks", "Thanks for watching!", "Dzięki za obejrzenie!", "Danke fürs Zuschauen!", "¡Gracias por mirar!", "Merci d'avoir regardé !", "Obrigado por assistir!");
            Same("premium.bought_title", "PREMIUM!");
            Add("premium.bought_body", "No more ads, +50% coins every game,\nand your royal goodies are unlocked!",
                "Koniec reklam, +50% monet w każdej grze\ni odblokowane królewskie nagrody!",
                "Keine Werbung mehr, +50 % Münzen pro Spiel\nund deine königlichen Extras sind frei!",
                "¡Sin anuncios, +50 % de monedas por partida\ny tus regalos reales desbloqueados!",
                "Plus de pub, +50 % de pièces par partie\net tes cadeaux royaux sont débloqués !",
                "Sem anúncios, +50% de moedas por partida\ne seus presentes reais liberados!");
            Add("iap.failed", "Purchase failed", "Zakup nieudany", "Kauf fehlgeschlagen", "Compra fallida", "Échec de l'achat", "Falha na compra");
            Add("iap.unavailable", "Store not available. Check your connection.", "Sklep niedostępny. Sprawdź połączenie.",
                "Store nicht verfügbar. Prüfe deine Verbindung.", "Tienda no disponible. Revisa tu conexión.",
                "Boutique indisponible. Vérifie ta connexion.", "Loja indisponível. Verifique sua conexão.");
            Add("iap.restored_title", "Restored!", "Przywrócono!", "Wiederhergestellt!", "¡Restaurado!", "Restauré !", "Restaurado!");
            Add("iap.restored_body", "Your Premium purchase is active again.", "Twój zakup Premium znów działa.", "Dein Premium-Kauf ist wieder aktiv.",
                "Tu compra Premium vuelve a estar activa.", "Ton achat Premium est de nouveau actif.", "Sua compra Premium está ativa de novo.");
            Add("iap.nothing_title", "Nothing to restore", "Nic do przywrócenia", "Nichts wiederherzustellen", "Nada que restaurar", "Rien à restaurer", "Nada para restaurar");
            Add("iap.nothing_body", "No previous purchases were found for this account.", "Nie znaleziono wcześniejszych zakupów na tym koncie.",
                "Für dieses Konto wurden keine Käufe gefunden.", "No se encontraron compras previas en esta cuenta.",
                "Aucun achat précédent trouvé pour ce compte.", "Nenhuma compra anterior encontrada nesta conta.");
        }
    }
}
