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
            Add("common.leave_title", "Back to the main menu?", "Wrócić do menu głównego?", "Zurück zum Hauptmenü?", "¿Volver al menú principal?", "Retour au menu principal ?", "Voltar ao menu principal?");
            Add("common.leave_body", "This game will end and its progress will be lost.", "Ta gra się zakończy, a jej postęp przepadnie.", "Dieses Spiel endet und der Fortschritt geht verloren.", "Esta partida terminará y se perderá su progreso.", "Cette partie se termine et sa progression sera perdue.", "Este jogo vai terminar e o progresso perde-se.");
            Add("common.leave", "MENU", "MENU", "MENÜ", "MENÚ", "MENU", "MENU");
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
            Add("home.quit_title", "Leave Stackville?", "Wyjść ze Stackville?", "Stackville verlassen?", "¿Salir de Stackville?", "Quitter Stackville ?", "Sair do Stackville?");
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
            Add("item.skin_soft", "Soft Arcade", "Miękki automat", "Soft-Arcade", "Arcade suave", "Arcade douce", "Fliperama suave");
            Add("item.theme_dusk", "Dusk", "Zmierzch", "Dämmerung", "Atardecer", "Crépuscule", "Crepúsculo");
            Add("item.theme_daydream", "Daydream", "Marzenie", "Tagtraum", "Ensueño", "Rêverie", "Devaneio");
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
            Same("music.Chill", "Lo-fi");
            Add("music.Future", "Future House", "Future House", "Future House", "Future House", "Future House", "Future House");

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
            Add("wheel.title", "LUCKY WHEEL", "KOŁO FORTUNY", "GLÜCKSRAD", "RUEDA DE LA SUERTE", "ROUE DE LA CHANCE", "RODA DA SORTE");
            Add("wheel.free_now", "Free spin ready!", "Darmowy obrót gotowy!", "Gratis-Dreh bereit!", "¡Giro gratis listo!", "Tour gratuit prêt !", "Giro grátis pronto!");
            Add("wheel.spin_for", "SPIN  {0}", "KRĘĆ  {0}", "DREHEN  {0}", "GIRAR  {0}", "TOURNER  {0}", "GIRAR  {0}");
            Add("wheel.paid_left", "Spins for coins left today: {0}", "Obroty za monety na dziś: {0}", "Drehs für Münzen heute: {0}", "Giros por monedas hoy: {0}", "Tours payants aujourd'hui : {0}", "Giros por moedas hoje: {0}");
            Add("wheel.paid_none", "No more spins today - come back tomorrow!", "Na dziś koniec obrotów - wróć jutro!", "Heute keine Drehs mehr - komm morgen wieder!", "No hay más giros hoy, ¡vuelve mañana!", "Plus de tours aujourd'hui, reviens demain !", "Sem mais giros hoje - volta amanhã!");
            Add("wheel.booster_active", "Coins x2 for the next {0} games", "Monety x2 przez {0} następne gry", "Münzen x2 für die nächsten {0} Spiele", "Monedas x2 en las próximas {0} partidas", "Pièces x2 pour les {0} prochaines parties", "Moedas x2 nos próximos {0} jogos");
            Add("wheel.booster", "Coins x2 for your next {0} games!", "Monety x2 w {0} następnych grach!", "Münzen x2 für deine nächsten {0} Spiele!", "¡Monedas x2 en tus próximas {0} partidas!", "Pièces x2 pour tes {0} prochaines parties !", "Moedas x2 nos próximos {0} jogos!");
            Add("wheel.booster_line", "Wheel booster x2", "Booster z koła x2", "Glücksrad-Booster x2", "Potenciador de la rueda x2", "Bonus de la roue x2", "Bónus da roda x2");
            Add("wheel.jackpot", "JACKPOT! The big prize is yours!", "JACKPOT! Główna nagroda jest twoja!", "JACKPOT! Der Hauptpreis gehört dir!", "¡PREMIO GORDO!", "JACKPOT ! Le gros lot est à toi !", "JACKPOT! O grande prémio é teu!");
            Add("wheel.supplies", "Supplies for your city!", "Surowce dla twojego miasta!", "Material für deine Stadt!", "¡Materiales para tu ciudad!", "Des ressources pour ta ville !", "Materiais para a tua cidade!");
            Add("wheel.supplies_big", "+{0} / +{1} / +{2}", "+{0} / +{1} / +{2}", "+{0} / +{1} / +{2}", "+{0} / +{1} / +{2}", "+{0} / +{1} / +{2}", "+{0} / +{1} / +{2}");
            Add("wheel.pet_energy", "Energy for your pet!", "Energia dla pupila!", "Energie für dein Tier!", "¡Energía para tu mascota!", "De l'énergie pour ton compagnon !", "Energia para o teu bichinho!");
            Add("wheel.kind.Coins", "{0} coins", "{0} monet", "{0} Münzen", "{0} monedas", "{0} pièces", "{0} moedas");
            Add("wheel.kind.Jackpot", "Jackpot: {0} coins", "Jackpot: {0} monet", "Jackpot: {0} Münzen", "Premio gordo: {0} monedas", "Jackpot : {0} pièces", "Jackpot: {0} moedas");
            Add("wheel.kind.PowerUp", "{0} power-up(s)", "{0} wzmocnienie(a)", "{0} Power-up(s)", "{0} potenciador(es)", "{0} bonus", "{0} power-up(s)");
            Add("wheel.kind.Shard", "{0} shard", "{0} kryształ", "{0} Splitter", "{0} fragmento", "{0} éclat", "{0} fragmento");
            Add("wheel.kind.Supplies", "Supplies (wood, bricks, glass)", "Surowce (drewno, cegły, szkło)", "Material (Holz, Ziegel, Glas)", "Materiales (madera, ladrillos, vidrio)", "Ressources (bois, briques, verre)", "Materiais (madeira, tijolos, vidro)");
            Add("wheel.kind.RunTicket", "Block Run bonus (x2 coins)", "Bieg bonusowy Block Run (x2)", "Block-Run-Bonus (x2)", "Carrera bonus (x2)", "Course bonus (x2)", "Corrida bónus (x2)");
            Add("wheel.kind.PetEnergy", "+{0} pet energy", "+{0} energii pupila", "+{0} Tier-Energie", "+{0} energía de mascota", "+{0} énergie du compagnon", "+{0} energia do bichinho");
            Add("wheel.kind.CoinBooster", "Coins x2 for {0} games", "Monety x2 na {0} gry", "Münzen x2 für {0} Spiele", "Monedas x2 durante {0} partidas", "Pièces x2 pendant {0} parties", "Moedas x2 durante {0} jogos");
            Add("wheel.odds_note", "A free spin after every game and once a day. Extra spins cost {0} coins (max {1} a day). Coins can't be bought with real money.", "Darmowy obrót po każdej grze i raz dziennie. Dodatkowe obroty kosztują {0} monet (maks. {1} dziennie). Monet nie da się kupić za prawdziwe pieniądze.", "Ein Gratis-Dreh nach jedem Spiel und einmal täglich. Extra-Drehs kosten {0} Münzen (max. {1} pro Tag). Münzen kann man nicht mit echtem Geld kaufen.", "Un giro gratis tras cada partida y una vez al día. Los giros extra cuestan {0} monedas (máx. {1} al día). Las monedas no se compran con dinero real.", "Un tour gratuit après chaque partie et une fois par jour. Les tours en plus coûtent {0} pièces (max {1} par jour). Les pièces ne s'achètent pas avec de l'argent réel.", "Um giro grátis após cada jogo e uma vez por dia. Giros extra custam {0} moedas (máx. {1} por dia). As moedas não se compram com dinheiro real.");
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
            Add("shop.thanks", "Thank you for supporting Stackville!", "Dziękujemy za wsparcie Stackville!", "Danke, dass du Stackville unterstützt!",
                "¡Gracias por apoyar Stackville!", "Merci de soutenir Stackville !", "Obrigado por apoiar o Stackville!");
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
            Add("tut.tap_d", "tap a column: drop there\ntap the piece: rotate", "stuknij kolumnę: klocek spada tam\nstuknij klocek: obrót",
                "Spalte tippen: dort fallen lassen\nStein tippen: drehen", "toca una columna: cae ahí\ntoca la pieza: girar",
                "tape une colonne : la pièce y tombe\ntape la pièce : tourner", "toque uma coluna: cai ali\ntoque a peça: girar");
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

            // ---------------------------------------------------------------- hub
            Add("home.levels", "LEVELS", "POZIOMY", "LEVELS", "NIVELES", "NIVEAUX", "NÍVEIS");
            Add("home.play_level", "PLAY LEVEL {0}", "GRAJ: POZIOM {0}", "LEVEL {0} SPIELEN", "JUGAR NIVEL {0}", "JOUER NIVEAU {0}", "JOGAR NÍVEL {0}");
            Add("home.city", "CITY", "MIASTO", "STADT", "CIUDAD", "VILLE", "CIDADE");
            Same("home.arcade", "ARCADE");
            Add("arcade.info", "Endless play. Coins from every game help your city grow.", "Gra bez końca. Monety z każdej gry pomagają rozwijać miasto.",
                "Endlos spielen. Münzen aus jedem Spiel lassen deine Stadt wachsen.", "Juego sin fin. Las monedas de cada partida hacen crecer tu ciudad.",
                "Jeu sans fin. Les pièces de chaque partie font grandir ta ville.", "Jogo sem fim. As moedas de cada partida fazem sua cidade crescer.");
            Add("common.score", "SCORE", "WYNIK", "PUNKTE", "PUNTOS", "SCORE", "PONTOS");

            // ---------------------------------------------------------------- level path
            Add("level.title", "LEVEL {0}", "POZIOM {0}", "LEVEL {0}", "NIVEL {0}", "NIVEAU {0}", "NÍVEL {0}");
            Add("level.goal", "Score {0} points", "Zdobądź {0} pkt", "Erziele {0} Punkte", "Consigue {0} puntos", "Marque {0} points", "Faça {0} pontos");
            Add("level.pieces", "You have {0} pieces", "Masz klocków: {0}", "Du hast {0} Steine", "Tienes {0} piezas", "Tu as {0} pièces", "Você tem {0} peças");
            Add("level.boost_title", "BOOST LEVEL {0}", "POZIOM BOOST {0}", "BOOST-LEVEL {0}", "NIVEL BOOST {0}", "NIVEAU BOOST {0}", "NÍVEL BOOST {0}");
            Add("level.boost_info",
                "{0} seconds. Points x2 and more golden pieces. Score as much as you can: big rewards for your city!",
                "{0} sekund. Punkty x2 i więcej złotych klocków. Zdobądź jak najwięcej, by zgarnąć duże nagrody dla miasta!",
                "{0} Sekunden. Punkte x2 und mehr goldene Steine. Hol so viel du kannst: große Belohnungen für deine Stadt!",
                "{0} segundos. Puntos x2 y más piezas doradas. ¡Consigue todo lo que puedas: grandes premios para tu ciudad!",
                "{0} secondes. Points x2 et plus de pièces dorées. Marque un maximum : grosses récompenses pour ta ville !",
                "{0} segundos. Pontos x2 e mais peças douradas. Faça o máximo que puder: grandes prêmios para sua cidade!");
            Same("level.boost", "BOOST");
            Add("level.stars", "Stars", "Gwiazdki", "Sterne", "Estrellas", "Étoiles", "Estrelas");
            Add("level.rewards", "Rewards", "Nagrody", "Belohnungen", "Premios", "Récompenses", "Prêmios");
            Add("level.locked", "Finish level {0} first", "Najpierw ukończ poziom {0}", "Schaffe zuerst Level {0}", "Primero completa el nivel {0}",
                "Termine d'abord le niveau {0}", "Conclua primeiro o nível {0}");
            Add("level.complete", "LEVEL COMPLETE!", "POZIOM ZALICZONY!", "LEVEL GESCHAFFT!", "¡NIVEL COMPLETADO!", "NIVEAU RÉUSSI !", "NÍVEL CONCLUÍDO!");
            Add("level.failed", "SO CLOSE!", "PRAWIE!", "KNAPP!", "¡CASI!", "PRESQUE !", "QUASE!");
            Add("level.short", "{0} more points needed", "Zabrakło {0} pkt", "Es fehlten {0} Punkte", "Faltaron {0} puntos", "Il manquait {0} points", "Faltaram {0} pontos");
            Add("level.boost_done", "BOOST OVER!", "KONIEC BOOSTU!", "BOOST VORBEI!", "¡FIN DEL BOOST!", "BOOST TERMINÉ !", "FIM DO BOOST!");
            Add("level.next", "NEXT LEVEL", "DALEJ", "WEITER", "SIGUIENTE", "SUIVANT", "PRÓXIMO");
            Add("level.retry", "RETRY", "JESZCZE RAZ", "NOCHMAL", "REINTENTAR", "RÉESSAYER", "TENTAR DE NOVO");
            Add("level.goal_reached", "GOAL!", "CEL!", "ZIEL!", "¡META!", "OBJECTIF !", "META!");
            Add("level.first_clear", "First clear bonus!", "Bonus za pierwsze przejście!", "Bonus für den ersten Sieg!", "¡Bonus de primera victoria!",
                "Bonus de première victoire !", "Bônus da primeira vitória!");
            Add("hud.pieces_left", "PIECES {0}", "KLOCKI {0}", "STEINE {0}", "PIEZAS {0}", "PIÈCES {0}", "PEÇAS {0}");
            Add("hud.time_left", "TIME {0}", "CZAS {0}", "ZEIT {0}", "TIEMPO {0}", "TEMPS {0}", "TEMPO {0}");

            // ---------------------------------------------------------------- city
            Add("city.level", "CITY LEVEL {0}", "MIASTO: POZIOM {0}", "STADTSTUFE {0}", "CIUDAD NIVEL {0}", "VILLE NIVEAU {0}", "CIDADE NÍVEL {0}");
            Add("city.level_short", "LV {0}", "POZ. {0}", "ST. {0}", "NV {0}", "NIV. {0}", "NV {0}");
            Add("city.population", "Population {0} / {1}", "Mieszkańcy {0} / {1}", "Einwohner {0} / {1}", "Población {0} / {1}", "Population {0} / {1}", "População {0} / {1}");
            Add("city.population_max", "Population {0}", "Mieszkańcy {0}", "Einwohner {0}", "Población {0}", "Population {0}", "População {0}");
            Add("city.bonus", "+{0}% coins from games", "+{0}% monet z gier", "+{0} % Münzen aus Spielen", "+{0}% monedas de las partidas",
                "+{0} % de pièces des parties", "+{0}% de moedas das partidas");
            Add("city.bonus_short", "+{0}% coins", "+{0}% monet", "+{0} % Münzen", "+{0}% monedas", "+{0} % pièces", "+{0}% moedas");
            Add("city.collect", "COLLECT", "ZBIERZ", "EINSAMMELN", "RECOGER", "RÉCOLTER", "COLETAR");
            Add("city.nothing", "Nothing to collect yet", "Na razie nic do zebrania", "Noch nichts zum Einsammeln", "Aún no hay nada que recoger",
                "Rien à récolter pour l'instant", "Nada para coletar ainda");
            Add("city.collected", "Collected!", "Zebrano!", "Eingesammelt!", "¡Recogido!", "Récolté !", "Coletado!");
            Add("city.market", "MARKET", "TARG", "MARKT", "MERCADO", "MARCHÉ", "MERCADO");
            Add("city.decor", "DECOR", "OZDOBY", "DEKO", "DECORACIÓN", "DÉCO", "DECORAÇÃO");
            Add("city.decor_info", "Decorations make the town prettier and add a coin bonus after every game.", "Ozdoby upiększają miasto i dają bonus monet po każdej grze.", "Deko macht die Stadt schöner und gibt nach jedem Spiel einen Münzbonus.", "La decoración embellece el pueblo y da un bonus de monedas tras cada partida.", "Les décorations embellissent la ville et donnent un bonus de pièces après chaque partie.", "A decoração deixa a cidade mais bonita e dá bónus de moedas após cada jogo.");
            Add("city.decor_spots", "Free spots: {0} of {1}", "Wolne miejsca: {0} z {1}", "Freie Plätze: {0} von {1}", "Huecos libres: {0} de {1}", "Places libres : {0} sur {1}", "Lugares livres: {0} de {1}");
            Add("city.decor_full", "No free spots left. The island grows with your town, or buy a new island for more room!", "Brak wolnych miejsc. Wyspa rośnie razem z miastem albo kup nową wyspę!", "Keine freien Plätze. Die Insel wächst mit der Stadt, oder kauf eine neue Insel!", "No quedan huecos. La isla crece con el pueblo, ¡o compra una isla nueva!", "Plus de place. L'île grandit avec la ville, ou achète une nouvelle île !", "Sem lugares livres. A ilha cresce com a cidade, ou compra uma ilha nova!");
            Add("city.buy", "BUY", "KUP", "KAUFEN", "COMPRAR", "ACHETER", "COMPRAR");
            Add("city.remove", "REMOVE", "USUŃ", "ENTFERNEN", "QUITAR", "RETIRER", "REMOVER");
            Add("city.owned_count", "Placed: {0}", "Postawione: {0}", "Aufgestellt: {0}", "Colocadas: {0}", "Posées : {0}", "Colocadas: {0}");
            Add("city.decor.flowers", "Flower bed", "Klomb", "Blumenbeet", "Parterre", "Parterre de fleurs", "Canteiro");
            Add("city.decor.lamp", "Street lamp", "Latarnia", "Laterne", "Farola", "Lampadaire", "Candeeiro");
            Add("city.decor.bench", "Bench", "Ławka", "Bank", "Banco", "Banc", "Banco");
            Add("city.decor.topiary", "Topiary", "Ozdobny krzew", "Formschnitt", "Arbusto podado", "Topiaire", "Arbusto podado");
            Add("city.decor.pond", "Lily pond", "Oczko wodne", "Seerosenteich", "Estanque", "Bassin", "Lago");
            Add("city.decor.fountain", "Fountain", "Fontanna", "Brunnen", "Fuente", "Fontaine", "Fonte");
            Add("city.decor.statue", "Golden statue", "Złoty pomnik", "Goldstatue", "Estatua dorada", "Statue dorée", "Estátua dourada");
            Add("city.decor.gazebo", "Gazebo", "Altana", "Pavillon", "Cenador", "Kiosque", "Coreto");
            Add("city.island", "NEW ISLAND", "NOWA WYSPA", "NEUE INSEL", "ISLA NUEVA", "NOUVELLE ÎLE", "ILHA NOVA");
            Add("city.island_info", "More room: {0} new plots and {1} decoration spots, joined to the town by a bridge.", "Więcej miejsca: {0} nowych działek i {1} miejsca na ozdoby, połączone z miastem mostem.", "Mehr Platz: {0} neue Grundstücke und {1} Deko-Plätze, mit einer Brücke zur Stadt.", "Más espacio: {0} parcelas nuevas y {1} huecos para decoración, unidos al pueblo por un puente.", "Plus de place : {0} terrains et {1} places de déco, reliés à la ville par un pont.", "Mais espaço: {0} terrenos novos e {1} lugares para decoração, ligados à cidade por uma ponte.");
            Add("city.island_owned", "Islands bought: {0} of {1}", "Kupione wyspy: {0} z {1}", "Gekaufte Inseln: {0} von {1}", "Islas compradas: {0} de {1}", "Îles achetées : {0} sur {1}", "Ilhas compradas: {0} de {1}");
            Add("city.island_level", "Needs town level {0}", "Wymaga poziomu miasta {0}", "Braucht Stadtstufe {0}", "Requiere nivel de pueblo {0}", "Nécessite le niveau de ville {0}", "Requer nível da cidade {0}");
            Add("city.island_all", "You own every island!", "Masz już wszystkie wyspy!", "Dir gehören alle Inseln!", "¡Ya tienes todas las islas!", "Tu possèdes toutes les îles !", "Já tens todas as ilhas!");
            Add("city.island_bought", "Welcome to your new island!", "Witaj na nowej wyspie!", "Willkommen auf deiner neuen Insel!", "¡Bienvenido a tu isla nueva!", "Bienvenue sur ta nouvelle île !", "Bem-vindo à tua ilha nova!");
            Add("city.demolish", "DEMOLISH", "WYBURZ", "ABREISSEN", "DERRIBAR", "DÉMOLIR", "DEMOLIR");
            Add("city.demolish_q", "Demolish the {0}? The plot will be free again and you get {1} coins back.", "Wyburzyć: {0}? Działka znów będzie wolna, a Ty odzyskasz {1} monet.", "{0} abreißen? Das Grundstück wird frei und du bekommst {1} Münzen zurück.", "¿Derribar {0}? La parcela quedará libre y recuperas {1} monedas.", "Démolir : {0} ? Le terrain sera libre et tu récupères {1} pièces.", "Demolir {0}? O terreno fica livre e recebes {1} moedas.");
            Add("city.demolished", "Plot cleared! +{0} coins", "Działka wolna! +{0} monet", "Grundstück frei! +{0} Münzen", "¡Parcela libre! +{0} monedas", "Terrain libéré ! +{0} pièces", "Terreno livre! +{0} moedas");
            Add("city.bonus_each", "+{0}% coins", "+{0}% monet", "+{0} % Münzen", "+{0}% monedas", "+{0} % pièces", "+{0}% moedas");
            Add("city.empty_plot", "Empty plot", "Wolna działka", "Freies Grundstück", "Parcela libre", "Terrain libre", "Terreno livre");
            Add("city.locked_plot", "Opens at city level {0}", "Od poziomu miasta {0}", "Ab Stadtstufe {0}", "Se abre en nivel {0}", "S'ouvre au niveau {0}", "Abre no nível {0}");
            Add("city.build", "BUILD", "BUDUJ", "BAUEN", "CONSTRUIR", "CONSTRUIRE", "CONSTRUIR");
            Add("city.upgrade", "UPGRADE", "ULEPSZ", "AUSBAUEN", "MEJORAR", "AMÉLIORER", "MELHORAR");
            Add("city.max", "MAX LEVEL", "MAKS. POZIOM", "MAX. STUFE", "NIVEL MÁX.", "NIVEAU MAX", "NÍVEL MÁX.");
            Add("city.choose", "Choose a building", "Wybierz budynek", "Gebäude wählen", "Elige un edificio", "Choisis un bâtiment", "Escolha um prédio");
            Add("city.needs_level", "City level {0}", "Poziom miasta {0}", "Stadtstufe {0}", "Nivel de ciudad {0}", "Niveau de ville {0}", "Nível da cidade {0}");
            Add("city.produces", "Makes {0} per hour", "Produkuje {0} na godzinę", "Macht {0} pro Stunde", "Produce {0} por hora", "Produit {0} par heure", "Produz {0} por hora");
            Add("city.people", "+{0} people", "+{0} mieszk.", "+{0} Einwohner", "+{0} habitantes", "+{0} habitants", "+{0} moradores");
            Add("city.built", "{0} built!", "Zbudowano: {0}!", "{0} gebaut!", "¡Construido: {0}!", "Construit : {0} !", "Construído: {0}!");
            Add("city.upgraded", "{0} is now level {1}!", "{0}: poziom {1}!", "{0} ist jetzt Stufe {1}!", "¡{0} ahora es nivel {1}!", "{0} passe au niveau {1} !", "{0} agora é nível {1}!");
            Add("city.not_enough_coins", "Not enough coins", "Za mało monet", "Nicht genug Münzen", "No tienes suficientes monedas", "Pas assez de pièces", "Moedas insuficientes");
            Add("city.not_enough_supplies",
                "Not enough supplies. Buy them at the market or win them in Boost levels!",
                "Za mało materiałów. Kup je na targu albo wygraj w poziomach Boost!",
                "Nicht genug Material. Kauf es auf dem Markt oder gewinne es in Boost-Levels!",
                "No tienes suficientes materiales. ¡Cómpralos en el mercado o gánalos en niveles Boost!",
                "Pas assez de matériaux. Achète-les au marché ou gagne-les dans les niveaux Boost !",
                "Materiais insuficientes. Compre no mercado ou ganhe nos níveis Boost!");
            Add("city.level_up", "CITY LEVEL {0}!", "MIASTO: POZIOM {0}!", "STADTSTUFE {0}!", "¡CIUDAD NIVEL {0}!", "VILLE NIVEAU {0} !", "CIDADE NÍVEL {0}!");
            Add("city.level_up_body", "New plots and buildings are open.", "Nowe działki i budynki są dostępne.", "Neue Grundstücke und Gebäude sind frei.",
                "Hay nuevas parcelas y edificios.", "De nouveaux terrains et bâtiments sont ouverts.", "Novos terrenos e prédios liberados.");
            Add("city.no_builder", "All {0} builders are busy. Wait for one to finish or speed it up.", "Wszyscy budowniczowie ({0}) są zajęci. Poczekaj albo przyspiesz budowę.",
                "Alle {0} Bauarbeiter sind beschäftigt. Warte oder beschleunige den Bau.", "Los {0} constructores están ocupados. Espera o acelera una obra.",
                "Les {0} ouvriers sont occupés. Attends ou accélère un chantier.", "Os {0} construtores estão ocupados. Espere ou acelere uma obra.");
            Add("city.busy", "This building is already under construction.", "Ten budynek jest już w budowie.", "Dieses Gebäude wird schon gebaut.",
                "Este edificio ya está en obras.", "Ce bâtiment est déjà en travaux.", "Este prédio já está em obras.");
            Add("city.started", "Construction started!", "Budowa ruszyła!", "Bau gestartet!", "¡Obra iniciada!", "Travaux lancés !", "Obra iniciada!");
            Add("city.ready_in", "Ready in {0}", "Gotowe za {0}", "Fertig in {0}", "Listo en {0}", "Prêt dans {0}", "Pronto em {0}");
            Add("city.build_time", "Build time: {0}", "Czas budowy: {0}", "Bauzeit: {0}", "Tiempo de obra: {0}", "Durée des travaux : {0}", "Tempo de obra: {0}");
            Add("city.building_to", "Building level {0}...", "Budowa poziomu {0}...", "Baue Stufe {0}...", "Construyendo nivel {0}...", "Construction du niveau {0}...", "Construindo nível {0}...");
            Add("city.speed_up", "FINISH NOW  {0}", "SKOŃCZ TERAZ  {0}", "SOFORT FERTIG  {0}", "TERMINAR YA  {0}", "TERMINER  {0}", "TERMINAR JÁ  {0}");
            Add("city.level_of", "LEVEL {0} / {1}", "POZIOM {0} / {1}", "STUFE {0} / {1}", "NIVEL {0} / {1}", "NIVEAU {0} / {1}", "NÍVEL {0} / {1}");
            Add("city.builders", "Builders {0} / {1}", "Budowniczowie {0} / {1}", "Bauarbeiter {0} / {1}", "Constructores {0} / {1}", "Ouvriers {0} / {1}", "Construtores {0} / {1}");
            // Settings & privacy
            Add("settings.sound_section", "SOUND", "DŹWIĘK", "TON", "SONIDO", "SON", "SOM");
            Add("settings.general_section", "GENERAL", "OGÓLNE", "ALLGEMEIN", "GENERAL", "GÉNÉRAL", "GERAL");
            Add("settings.account_section", "ACCOUNT", "KONTO", "KONTO", "CUENTA", "COMPTE", "CONTA");
            Add("settings.privacy_section", "PRIVACY", "PRYWATNOŚĆ", "DATENSCHUTZ", "PRIVACIDAD", "CONFIDENTIALITÉ", "PRIVACIDADE");
            Add("settings.help_section", "HELP", "POMOC", "HILFE", "AYUDA", "AIDE", "AJUDA");
            Add("settings.fps.0", "Smoothness: max", "Płynność: maks.", "Flüssigkeit: max", "Fluidez: máx.", "Fluidité : max", "Fluidez: máx.");
            Add("settings.fps.1", "Smoothness: 60 FPS", "Płynność: 60 FPS", "Flüssigkeit: 60 FPS", "Fluidez: 60 FPS", "Fluidité : 60 FPS", "Fluidez: 60 FPS");
            Add("settings.fps.2", "Battery saver: 30 FPS", "Oszczędzanie baterii: 30 FPS", "Akku sparen: 30 FPS", "Ahorro de batería: 30 FPS", "Économie de batterie : 30 FPS", "Poupança de bateria: 30 FPS");
            Add("settings.contact", "Contact support", "Kontakt z pomocą", "Support kontaktieren", "Contactar con soporte", "Contacter le support", "Contactar o suporte");
            Add("privacy.intro", "Welcome to Stackville! Please take a moment to read how the game uses data.", "Witaj w Stackville! Poświęć chwilę, żeby przeczytać, jak gra korzysta z danych.", "Willkommen in Stackville! Lies kurz, wie das Spiel Daten nutzt.", "¡Bienvenido a Stackville! Lee cómo usa el juego los datos.", "Bienvenue dans Stackville ! Prends un instant pour lire comment le jeu utilise les données.", "Bem-vindo ao Stackville! Lê como o jogo usa os dados.");
            Add("privacy.online", "OPEN ONLINE", "OTWÓRZ ONLINE", "ONLINE ÖFFNEN", "ABRIR ONLINE", "OUVRIR EN LIGNE", "ABRIR ONLINE");
            Add("privacy.accept", "OK, GOT IT", "OK, ROZUMIEM", "OK, VERSTANDEN", "VALE, ENTENDIDO", "OK, COMPRIS", "OK, PERCEBI");
            Add("privacy.english_only", "The full policy is available in English and Polish.", "Pełna polityka jest dostępna po angielsku i po polsku.", "Die vollständige Richtlinie gibt es auf Englisch und Polnisch.", "La política completa está en inglés y polaco.", "La politique complète est disponible en anglais et en polonais.", "A política completa está disponível em inglês e polaco.");
            Add("privacy.delete", "Delete my data", "Usuń moje dane", "Meine Daten löschen", "Borrar mis datos", "Supprimer mes données", "Apagar os meus dados");
            Add("privacy.delete_body", "This permanently deletes all your progress, coins, city, pet and settings on this device. Purchases can be restored later. This cannot be undone.", "To trwale usunie cały postęp, monety, miasto, pupila i ustawienia na tym urządzeniu. Zakupy można później przywrócić. Tego nie da się cofnąć.", "Dadurch werden Fortschritt, Münzen, Stadt, Tier und Einstellungen auf diesem Gerät dauerhaft gelöscht. Käufe können wiederhergestellt werden. Das kann nicht rückgängig gemacht werden.", "Se borrarán para siempre tu progreso, monedas, ciudad, mascota y ajustes en este dispositivo. Las compras se pueden restaurar. No se puede deshacer.", "Cela supprime définitivement ta progression, tes pièces, ta ville, ton compagnon et tes réglages sur cet appareil. Les achats peuvent être restaurés. Action irréversible.", "Isto apaga para sempre o progresso, moedas, cidade, bichinho e definições neste dispositivo. As compras podem ser restauradas. Não pode ser desfeito.");
            Add("privacy.delete_confirm", "DELETE", "USUŃ", "LÖSCHEN", "BORRAR", "SUPPRIMER", "APAGAR");
            Add("privacy.deleted_title", "Data deleted", "Dane usunięte", "Daten gelöscht", "Datos borrados", "Données supprimées", "Dados apagados");
            Add("privacy.deleted_body", "Everything was deleted. The game will now close and start fresh next time.", "Wszystko zostało usunięte. Gra się teraz zamknie i następnym razem zacznie od nowa.", "Alles wurde gelöscht. Das Spiel wird jetzt beendet und startet beim nächsten Mal neu.", "Se ha borrado todo. El juego se cerrará y empezará de cero la próxima vez.", "Tout a été supprimé. Le jeu va se fermer et repartira de zéro.", "Tudo foi apagado. O jogo vai fechar e recomeçar da próxima vez.");

            Add("city.look.1", "Look: simple", "Wygląd: prosty", "Aussehen: schlicht", "Aspecto: sencillo", "Style : simple", "Aspeto: simples");
            Add("city.look.2", "Look: cosy", "Wygląd: przytulny", "Aussehen: gemütlich", "Aspecto: acogedor", "Style : douillet", "Aspeto: acolhedor");
            Add("city.look.3", "Look: elegant", "Wygląd: elegancki", "Aussehen: elegant", "Aspecto: elegante", "Style : élégant", "Aspeto: elegante");
            Add("city.look.4", "Look: grand", "Wygląd: okazały", "Aussehen: prachtvoll", "Aspecto: majestuoso", "Style : grandiose", "Aspeto: grandioso");
            Add("city.next_look", "{0} from level {1}", "{0} od poziomu {1}", "{0} ab Stufe {1}", "{0} desde el nivel {1}", "{0} dès le niveau {1}", "{0} a partir do nível {1}");
            // Islands & forest
            Add("island.city", "VILLAGE", "MIASTO", "DORF", "PUEBLO", "VILLAGE", "VILA");
            Add("island.forest", "FOREST", "LAS", "WALD", "BOSQUE", "FORÊT", "FLORESTA");
            Add("island.garden", "GARDEN", "OGRÓD", "GARTEN", "HUERTO", "JARDIN", "HORTA");
            Add("island.soon", "The garden island is coming soon!", "Wyspa ogród już wkrótce!", "Die Garteninsel kommt bald!", "¡La isla huerto llega pronto!", "L'île jardin arrive bientôt !", "A ilha horta vem aí!");
            Add("forest.title", "Forest · level {0}", "Las · poziom {0}", "Wald · Stufe {0}", "Bosque · nivel {0}", "Forêt · niveau {0}", "Floresta · nível {0}");
            Add("forest.xp", "Forest XP {0} / {1}", "Doświadczenie lasu {0} / {1}", "Wald-EP {0} / {1}", "PX del bosque {0} / {1}", "XP forêt {0} / {1}", "XP da floresta {0} / {1}");
            Add("forest.max", "The whole forest is yours!", "Cały las jest twój!", "Der ganze Wald gehört dir!", "¡Todo el bosque es tuyo!", "Toute la forêt est à toi !", "A floresta toda é tua!");
            Add("forest.goods", "Mushrooms {0} · golden {1} · berries {2} · honey {3} · wood {4}", "Grzyby {0} · złote {1} · jagody {2} · miód {3} · drewno {4}", "Pilze {0} · golden {1} · Beeren {2} · Honig {3} · Holz {4}", "Setas {0} · doradas {1} · bayas {2} · miel {3} · madera {4}", "Champignons {0} · dorés {1} · baies {2} · miel {3} · bois {4}", "Cogumelos {0} · dourados {1} · bagas {2} · mel {3} · madeira {4}");
            Add("forest.ready", "{0} mushrooms waiting - tap them! Tap trees to chop, bushes for berries.", "{0} grzybów czeka - stuknij je! Stukaj drzewa, by ściąć, krzaki - po jagody.", "{0} Pilze warten - tippe sie an! Bäume fällen, Büsche pflücken.", "{0} setas esperan, ¡tócalas! Toca árboles para talar y arbustos para bayas.", "{0} champignons t'attendent ! Touche les arbres pour couper, les buissons pour les baies.", "{0} cogumelos à espera - toca neles! Toca árvores para cortar e arbustos para bagas.");
            Add("forest.wait", "Mushrooms are growing back... chop trees or pick berries meanwhile.", "Grzyby odrastają... w tym czasie zetnij drzewo albo zbierz jagody.", "Die Pilze wachsen nach... fälle solange Bäume oder pflücke Beeren.", "Las setas vuelven a crecer... mientras, tala árboles o coge bayas.", "Les champignons repoussent... coupe du bois ou cueille des baies en attendant.", "Os cogumelos estão a crescer... entretanto corta árvores ou apanha bagas.");
            Add("forest.intro", "Welcome to the forest! Pick mushrooms, chop trees and gather berries. Use them everywhere: feed your pet, sell them at the market, buy an extra life in Block Run or a spring start in the tower. Build forest houses to grow more.", "Witaj w lesie! Zbieraj grzyby, ścinaj drzewa i zbieraj jagody. Używaj ich wszędzie: nakarm pupila, sprzedaj na targu, kup dodatkowe życie w Block Run albo start ze sprężyny w wieży. Buduj leśne domki, żeby rosło więcej.", "Willkommen im Wald! Sammle Pilze, fälle Bäume, pflücke Beeren. Nutze sie überall: Tier füttern, auf dem Markt verkaufen, Extra-Leben im Block Run oder Feder-Start im Turm. Baue Waldhäuser für mehr.", "¡Bienvenido al bosque! Recoge setas, tala árboles y coge bayas. Úsalas en todo: alimenta a tu mascota, véndelas, compra una vida extra en Block Run o un salto inicial en la torre.", "Bienvenue dans la forêt ! Cueille des champignons, coupe du bois et des baies. Utilise-les partout : nourris ton compagnon, vends au marché, une vie en plus dans Block Run ou un départ ressort dans la tour.", "Bem-vindo à floresta! Apanha cogumelos, corta árvores e apanha bagas. Usa-os em todo o lado: alimenta o bichinho, vende no mercado, compra uma vida extra no Block Run ou um salto inicial na torre.");
            Add("forest.level_up", "The forest grew! New areas and buildings may be open.", "Las urósł! Mogą być dostępne nowe tereny i budynki.", "Der Wald ist gewachsen! Neue Gebiete und Gebäude könnten frei sein.", "¡El bosque creció! Puede haber nuevas zonas y edificios.", "La forêt a grandi ! De nouvelles zones et maisons sont peut-être ouvertes.", "A floresta cresceu! Pode haver novas zonas e edifícios.");
            Add("forest.fox_chip", "A fox stole a basket! Catch it", "Lis ukradł koszyk! Złap go", "Ein Fuchs hat einen Korb geklaut!", "¡Un zorro robó una cesta! Atrápalo", "Un renard a volé un panier !", "Uma raposa roubou um cesto! Apanha-a");
            Add("forest.fox_title", "The fox", "Lis", "Der Fuchs", "El zorro", "Le renard", "A raposa");
            Add("forest.fox_do", "Fox caught {0}/{1}", "Złapano lisa {0}/{1}", "Fuchs {0}/{1}", "Zorro {0}/{1}", "Renard {0}/{1}", "Raposa {0}/{1}");
            Add("forest.fox_caught", "You got the basket of mushrooms back!", "Odzyskałeś koszyk grzybów!", "Du hast den Pilzkorb zurück!", "¡Recuperaste la cesta de setas!", "Tu as récupéré le panier de champignons !", "Recuperaste o cesto de cogumelos!");
            Add("forest.fox_escaped", "It got away... it will be back!", "Uciekł... ale wróci!", "Er ist entkommen... er kommt wieder!", "Se escapó... ¡volverá!", "Il s'est enfui... il reviendra !", "Fugiu... mas vai voltar!");
            Add("forest.build", "BUILD", "ZBUDUJ", "BAUEN", "CONSTRUIR", "CONSTRUIRE", "CONSTRUIR");
            Add("forest.upgrade", "UPGRADE", "ULEPSZ", "AUSBAUEN", "MEJORAR", "AMÉLIORER", "MELHORAR");
            Add("forest.cost", "Cost: {0} coins · {1} wood · {2} mushrooms", "Koszt: {0} monet · {1} drewna · {2} grzybów", "Kosten: {0} Münzen · {1} Holz · {2} Pilze", "Coste: {0} monedas · {1} madera · {2} setas", "Coût : {0} pièces · {1} bois · {2} champignons", "Custo: {0} moedas · {1} madeira · {2} cogumelos");
            Add("forest.unlock_at", "Opens at forest level {0}", "Dostępne od poziomu lasu {0}", "Ab Waldstufe {0}", "Disponible en el nivel {0} del bosque", "Disponible au niveau de forêt {0}", "Disponível no nível {0} da floresta");
            Add("forest.maxed", "Fully upgraded!", "W pełni ulepszone!", "Voll ausgebaut!", "¡Mejorado al máximo!", "Amélioré au maximum !", "Totalmente melhorado!");
            Add("forest.missing.NotEnoughCoins", "Not enough coins", "Za mało monet", "Nicht genug Münzen", "Faltan monedas", "Pas assez de pièces", "Faltam moedas");
            Add("forest.missing.NotEnoughWood", "Not enough wood - chop some trees", "Za mało drewna - zetnij drzewa", "Nicht genug Holz - fälle Bäume", "Falta madera: tala árboles", "Pas assez de bois - coupe des arbres", "Falta madeira - corta árvores");
            Add("forest.missing.NotEnoughMushrooms", "Not enough mushrooms", "Za mało grzybów", "Nicht genug Pilze", "Faltan setas", "Pas assez de champignons", "Faltam cogumelos");
            Add("forest.missing.MaxLevel", "Fully upgraded!", "W pełni ulepszone!", "Voll ausgebaut!", "¡Al máximo!", "Au maximum !", "No máximo!");
            Add("forest.collect_honey", "COLLECT HONEY ({0})", "ZBIERZ MIÓD ({0})", "HONIG ERNTEN ({0})", "RECOGER MIEL ({0})", "RÉCOLTER LE MIEL ({0})", "APANHAR MEL ({0})");
            Add("forest.brew", "BREW A POWER-UP ({0})", "UWARZ WZMOCNIENIE ({0})", "POWER-UP BRAUEN ({0})", "PREPARAR POTENCIADOR ({0})", "PRÉPARER UN BONUS ({0})", "PREPARAR POWER-UP ({0})");
            Add("forest.brewed", "The herbalist brewed a power-up!", "Zielarka uwarzyła wzmocnienie!", "Die Kräuterfrau hat ein Power-up gebraut!", "¡La herbolaria preparó un potenciador!", "L'herboriste a préparé un bonus !", "A herbanária preparou um power-up!");
            Add("forest.b.MushroomHut", "Mushroom hut", "Chatka grzybiarza", "Pilzhütte", "Cabaña setera", "Cabane aux champignons", "Cabana dos cogumelos");
            Add("forest.b.LumberCamp", "Lumber camp", "Obóz drwali", "Holzfällerlager", "Campamento leñador", "Camp de bûcherons", "Acampamento lenhador");
            Add("forest.b.Apiary", "Apiary", "Pasieka", "Imkerei", "Colmenar", "Rucher", "Apiário");
            Add("forest.b.Watchtower", "Watchtower", "Wieża obserwacyjna", "Wachturm", "Torre vigía", "Tour de guet", "Torre de vigia");
            Add("forest.b.Herbalist", "Herbalist", "Zielarka", "Kräuterfrau", "Herbolaria", "Herboriste", "Herbanária");
            Add("forest.b.MushroomHut.desc", "One more mushroom spot in every area and mushrooms grow back faster.", "Dodatkowe miejsce na grzyby w każdej części lasu i szybsze odrastanie.", "Ein Pilzplatz mehr pro Gebiet, Pilze wachsen schneller nach.", "Un sitio más para setas en cada zona y crecen más rápido.", "Un emplacement de plus par zone et les champignons repoussent plus vite.", "Mais um sítio de cogumelos por zona e crescem mais depressa.");
            Add("forest.b.LumberCamp.desc", "More wood from every tree, and trees grow back faster.", "Więcej drewna z każdego drzewa i szybsze odrastanie drzew.", "Mehr Holz pro Baum, Bäume wachsen schneller nach.", "Más madera por árbol y crecen más rápido.", "Plus de bois par arbre, ils repoussent plus vite.", "Mais madeira por árvore e crescem mais depressa.");
            Add("forest.b.Apiary.desc", "Bees make honey all day: collect it for your pet or the market.", "Pszczoły robią miód cały dzień: zbierz go dla pupila albo na targ.", "Bienen machen den ganzen Tag Honig: für dein Tier oder den Markt.", "Las abejas hacen miel todo el día: para tu mascota o el mercado.", "Les abeilles font du miel toute la journée : pour ton compagnon ou le marché.", "As abelhas fazem mel o dia todo: para o bichinho ou o mercado.");
            Add("forest.b.Watchtower.desc", "Spot more golden mushrooms (worth a lot at the market).", "Wypatrzysz więcej złotych grzybów (dużo warte na targu).", "Mehr goldene Pilze entdecken (viel wert auf dem Markt).", "Encuentra más setas doradas (valen mucho).", "Repère plus de champignons dorés (ils valent cher).", "Encontra mais cogumelos dourados (valem muito).");
            Add("forest.b.Herbalist.desc", "Brews power-ups from mushrooms. Each level makes it cheaper.", "Warzy wzmocnienia z grzybów. Każdy poziom obniża koszt.", "Braut Power-ups aus Pilzen. Jede Stufe macht es billiger.", "Prepara potenciadores con setas. Cada nivel lo abarata.", "Prépare des bonus avec des champignons. Chaque niveau baisse le prix.", "Prepara power-ups com cogumelos. Cada nível fica mais barato.");

            // Garden
            Add("garden.title", "Garden · level {0}", "Ogród · poziom {0}", "Garten · Stufe {0}", "Huerto · nivel {0}", "Jardin · niveau {0}", "Horta · nível {0}");
            Add("garden.goods", "Carrots {0} · tomatoes {1} · strawberries {2} · pumpkins {3} · sunflowers {4} · eggs {5} · compost {6}", "Marchew {0} · pomidory {1} · truskawki {2} · dynie {3} · słoneczniki {4} · jajka {5} · kompost {6}", "Karotten {0} · Tomaten {1} · Erdbeeren {2} · Kürbisse {3} · Sonnenblumen {4} · Eier {5} · Kompost {6}", "Zanahorias {0} · tomates {1} · fresas {2} · calabazas {3} · girasoles {4} · huevos {5} · abono {6}", "Carottes {0} · tomates {1} · fraises {2} · citrouilles {3} · tournesols {4} · œufs {5} · compost {6}", "Cenouras {0} · tomates {1} · morangos {2} · abóboras {3} · girassóis {4} · ovos {5} · composto {6}");
            Add("garden.intro", "Welcome to the garden! Tap a bed to plant seeds. Crops grow in real time - water them to speed up. Harvest, cook tasty dishes in the kitchen or sell at the stall.", "Witaj w ogrodzie! Stuknij grządkę, żeby posadzić nasiona. Rośliny rosną w czasie rzeczywistym - podlej je, żeby rosły szybciej. Zbieraj plony, gotuj w kuchni albo sprzedawaj na straganie.", "Willkommen im Garten! Tippe ein Beet an, um zu säen. Pflanzen wachsen in Echtzeit - gieße sie für mehr Tempo. Ernte, koche in der Küche oder verkaufe am Stand.", "¡Bienvenido al huerto! Toca un bancal para sembrar. Crecen en tiempo real; riégalas para acelerar. Cosecha, cocina o vende en el puesto.", "Bienvenue au jardin ! Touche un carré pour semer. Les plantes poussent en temps réel - arrose-les pour accélérer. Récolte, cuisine ou vends au stand.", "Bem-vindo à horta! Toca num canteiro para semear. Crescem em tempo real - rega para acelerar. Colhe, cozinha ou vende na banca.");
            Add("garden.ripe", "{0} beds ready to harvest - tap them!", "{0} grządek gotowych do zbioru - stuknij je!", "{0} Beete erntereif - tippe sie an!", "{0} bancales listos, ¡tócalos!", "{0} carrés prêts à récolter !", "{0} canteiros prontos - toca neles!");
            Add("garden.empty", "{0} empty beds - tap one to plant.", "{0} pustych grządek - stuknij, żeby posadzić.", "{0} leere Beete - tippe zum Säen.", "{0} bancales vacíos: toca para sembrar.", "{0} carrés vides - touche pour semer.", "{0} canteiros vazios - toca para semear.");
            Add("garden.growing", "Everything is growing... water the beds to speed them up.", "Wszystko rośnie... podlej grządki, żeby rosły szybciej.", "Alles wächst... gieße die Beete.", "Todo crece... riega para acelerar.", "Tout pousse... arrose pour accélérer.", "Tudo a crescer... rega para acelerar.");
            Add("garden.level_up", "The garden grew! More beds and new seeds may be open.", "Ogród urósł! Mogą być nowe grządki i nasiona.", "Der Garten ist gewachsen! Mehr Beete und neue Samen.", "¡El huerto creció! Más bancales y semillas.", "Le jardin a grandi ! Plus de carrés et de graines.", "A horta cresceu! Mais canteiros e sementes.");
            Add("garden.bed", "Garden bed", "Grządka", "Beet", "Bancal", "Carré", "Canteiro");
            Add("garden.choose_seed", "What would you like to plant?", "Co chcesz posadzić?", "Was möchtest du pflanzen?", "¿Qué quieres plantar?", "Que veux-tu planter ?", "O que queres plantar?");
            Add("garden.has_compost", "Compost ready: {0} (the next planting gets one extra crop)", "Kompost: {0} (następne sadzenie da dodatkowy plon)", "Kompost: {0} (nächste Pflanzung +1 Ernte)", "Abono: {0} (la próxima siembra da una más)", "Compost : {0} (la prochaine plantation donne un en plus)", "Composto: {0} (a próxima dá mais um)");
            Add("garden.plant", "{0} · {1} coins · {2}", "{0} · {1} monet · {2}", "{0} · {1} Münzen · {2}", "{0} · {1} monedas · {2}", "{0} · {1} pièces · {2}", "{0} · {1} moedas · {2}");
            Add("garden.crop_locked", "{0} - garden level {1}", "{0} - poziom ogrodu {1}", "{0} - Gartenstufe {1}", "{0}: nivel {1}", "{0} - niveau {1}", "{0} - nível {1}");
            Add("garden.water", "WATER", "PODLEJ", "GIESSEN", "REGAR", "ARROSER", "REGAR");
            Add("garden.watered", "WATERED", "PODLANE", "GEGOSSEN", "REGADO", "ARROSÉ", "REGADO");
            Add("garden.water_all", "WATER ALL BEDS", "PODLEJ WSZYSTKO", "ALLE GIESSEN", "REGAR TODO", "TOUT ARROSER", "REGAR TUDO");
            Add("garden.growing_info", "{0} ready in {1}", "{0} gotowe za {1}", "{0} fertig in {1}", "{0} listo en {1}", "{0} prêt dans {1}", "{0} pronto em {1}");
            Add("garden.composted", "Composted: one extra crop", "Z kompostem: dodatkowy plon", "Mit Kompost: +1 Ernte", "Con abono: una más", "Avec compost : un en plus", "Com composto: mais um");
            Add("garden.kitchen", "KITCHEN", "KUCHNIA", "KÜCHE", "COCINA", "CUISINE", "COZINHA");
            Add("garden.kitchen_info", "Cook dishes from your crops, mushrooms, eggs and honey. Each one gives a bonus!", "Gotuj dania z plonów, grzybów, jajek i miodu. Każde daje bonus!", "Koche aus Ernte, Pilzen, Eiern und Honig. Jedes Gericht gibt einen Bonus!", "Cocina con tu cosecha, setas, huevos y miel. ¡Cada plato da un bonus!", "Cuisine avec tes récoltes, champignons, œufs et miel. Chaque plat donne un bonus !", "Cozinha com colheitas, cogumelos, ovos e mel. Cada prato dá um bónus!");
            Add("garden.sell", "SELL HARVEST", "SPRZEDAJ PLONY", "ERNTE VERKAUFEN", "VENDER COSECHA", "VENDRE LA RÉCOLTE", "VENDER COLHEITA");
            Add("garden.sold", "Sold at the garden stall!", "Sprzedane na straganie!", "Am Gartenstand verkauft!", "¡Vendido en el puesto!", "Vendu au stand !", "Vendido na banca!");
            Add("garden.nothing_to_sell", "Nothing to sell yet - harvest some crops first.", "Na razie nie ma co sprzedać - najpierw zbierz plony.", "Noch nichts zu verkaufen - ernte zuerst.", "Nada que vender aún: cosecha primero.", "Rien à vendre - récolte d'abord.", "Nada para vender - colhe primeiro.");
            Add("garden.cost", "Cost: {0} coins · {1} wood · {2} bricks", "Koszt: {0} monet · {1} drewna · {2} cegieł", "Kosten: {0} Münzen · {1} Holz · {2} Ziegel", "Coste: {0} monedas · {1} madera · {2} ladrillos", "Coût : {0} pièces · {1} bois · {2} briques", "Custo: {0} moedas · {1} madeira · {2} tijolos");
            Add("garden.unlock_at", "Opens at garden level {0}", "Dostępne od poziomu ogrodu {0}", "Ab Gartenstufe {0}", "Disponible en el nivel {0} del huerto", "Disponible au niveau de jardin {0}", "Disponível no nível {0} da horta");
            Add("garden.collect_eggs", "COLLECT EGGS ({0})", "ZBIERZ JAJKA ({0})", "EIER SAMMELN ({0})", "RECOGER HUEVOS ({0})", "RAMASSER LES ŒUFS ({0})", "APANHAR OVOS ({0})");
            Add("garden.make_compost", "MAKE COMPOST ({0} mushrooms)", "ZRÓB KOMPOST ({0} grzyby)", "KOMPOST MACHEN ({0} Pilze)", "HACER ABONO ({0} setas)", "FAIRE DU COMPOST ({0} champignons)", "FAZER COMPOSTO ({0} cogumelos)");
            Add("garden.crows_chip", "Crows in the garden! Chase them", "Wrony w ogrodzie! Przegoń je", "Krähen im Garten! Verjage sie", "¡Cuervos en el huerto! Échalos", "Des corbeaux au jardin ! Chasse-les", "Corvos na horta! Afugenta-os");
            Add("garden.crows_title", "Crows", "Wrony", "Krähen", "Cuervos", "Corbeaux", "Corvos");
            Add("garden.crows_do", "Crows chased {0}/{1}", "Przegonione wrony {0}/{1}", "Verjagt {0}/{1}", "Ahuyentados {0}/{1}", "Chassés {0}/{1}", "Afugentados {0}/{1}");
            Add("garden.crows_chased", "The farmer thanks you: coins and {0} eggs!", "Rolnik dziękuje: monety i {0} jajka!", "Der Bauer dankt: Münzen und {0} Eier!", "El granjero te da monedas y {0} huevos", "Le fermier te remercie : pièces et {0} œufs !", "O agricultor agradece: moedas e {0} ovos!");
            Add("garden.crows_won", "The crows pecked a bed - it needs a bit more time.", "Wrony podziobały grządkę - potrzebuje trochę więcej czasu.", "Die Krähen haben ein Beet angepickt - es braucht etwas länger.", "Los cuervos picotearon un bancal: tardará un poco más.", "Les corbeaux ont picoré un carré - il lui faut un peu plus de temps.", "Os corvos bicaram um canteiro - precisa de mais tempo.");
            Add("garden.crop.Carrot", "Carrot", "Marchew", "Karotte", "Zanahoria", "Carotte", "Cenoura");
            Add("garden.crop.Tomato", "Tomato", "Pomidor", "Tomate", "Tomate", "Tomate", "Tomate");
            Add("garden.crop.Strawberry", "Strawberry", "Truskawka", "Erdbeere", "Fresa", "Fraise", "Morango");
            Add("garden.crop.Pumpkin", "Pumpkin", "Dynia", "Kürbis", "Calabaza", "Citrouille", "Abóbora");
            Add("garden.crop.Sunflower", "Sunflower", "Słonecznik", "Sonnenblume", "Girasol", "Tournesol", "Girassol");
            Add("garden.crop_short.Carrot", "carrots", "marchewki", "Karotten", "zanahorias", "carottes", "cenouras");
            Add("garden.crop_short.Tomato", "tomatoes", "pomidory", "Tomaten", "tomates", "tomates", "tomates");
            Add("garden.crop_short.Strawberry", "strawberries", "truskawki", "Erdbeeren", "fresas", "fraises", "morangos");
            Add("garden.crop_short.Pumpkin", "pumpkin", "dynia", "Kürbis", "calabaza", "citrouille", "abóbora");
            Add("garden.crop_short.Sunflower", "sunflowers", "słoneczniki", "Sonnenblumen", "girasoles", "tournesols", "girassóis");
            Add("garden.ing.mushroom", "mushroom", "grzyb", "Pilz", "seta", "champignon", "cogumelo");
            Add("garden.ing.egg", "eggs", "jajka", "Eier", "huevos", "œufs", "ovos");
            Add("garden.ing.honey", "honey", "miód", "Honig", "miel", "miel", "mel");
            Add("garden.dish.veggie_soup", "Veggie soup", "Zupa jarzynowa", "Gemüsesuppe", "Sopa de verduras", "Soupe de légumes", "Sopa de legumes");
            Add("garden.dish.omelette", "Omelette", "Omlet", "Omelett", "Tortilla", "Omelette", "Omelete");
            Add("garden.dish.strawberry_cake", "Strawberry cake", "Tort truskawkowy", "Erdbeertorte", "Tarta de fresa", "Gâteau aux fraises", "Bolo de morango");
            Add("garden.dish.pumpkin_pie", "Pumpkin pie", "Placek dyniowy", "Kürbiskuchen", "Pastel de calabaza", "Tarte à la citrouille", "Tarte de abóbora");
            Add("garden.dish.sunflower_bread", "Sunflower bread", "Chleb słonecznikowy", "Sonnenblumenbrot", "Pan de girasol", "Pain aux tournesols", "Pão de girassol");
            Add("garden.dish.veggie_soup.bonus", "Your pet is full and happy!", "Pupil najedzony i szczęśliwy!", "Dein Tier ist satt und froh!", "¡Tu mascota está llena y feliz!", "Ton compagnon est repu et content !", "O bichinho está cheio e feliz!");
            Add("garden.dish.omelette.bonus", "Your pet's energy is full!", "Energia pupila na maksa!", "Die Energie deines Tiers ist voll!", "¡Energía de la mascota al máximo!", "L'énergie de ton compagnon est au max !", "Energia do bichinho no máximo!");
            Add("garden.dish.strawberry_cake.bonus", "Coins x2 for your next 2 games!", "Monety x2 w 2 następnych grach!", "Münzen x2 für die nächsten 2 Spiele!", "¡Monedas x2 en las próximas 2 partidas!", "Pièces x2 pour les 2 prochaines parties !", "Moedas x2 nos próximos 2 jogos!");
            Add("garden.dish.pumpkin_pie.bonus", "A Block Run bonus and a power-up!", "Bieg bonusowy i wzmocnienie!", "Ein Block-Run-Bonus und ein Power-up!", "¡Una carrera bonus y un potenciador!", "Une course bonus et un bonus !", "Uma corrida bónus e um power-up!");
            Add("garden.dish.sunflower_bread.bonus", "Wood, bricks and glass for your city!", "Drewno, cegły i szkło dla miasta!", "Holz, Ziegel und Glas für deine Stadt!", "¡Madera, ladrillos y vidrio!", "Du bois, des briques et du verre !", "Madeira, tijolos e vidro!");
            Add("garden.b.Greenhouse", "Greenhouse", "Szklarnia", "Gewächshaus", "Invernadero", "Serre", "Estufa");
            Add("garden.b.Well", "Well", "Studnia", "Brunnen", "Pozo", "Puits", "Poço");
            Add("garden.b.Compost", "Compost bin", "Kompostownik", "Komposter", "Compostera", "Composteur", "Compostor");
            Add("garden.b.Coop", "Chicken coop", "Kurnik", "Hühnerstall", "Gallinero", "Poulailler", "Galinheiro");
            Add("garden.b.Greenhouse.desc", "Every crop grows faster (20% per level).", "Wszystko rośnie szybciej (20% na poziom).", "Alles wächst schneller (20 % pro Stufe).", "Todo crece más rápido (20% por nivel).", "Tout pousse plus vite (20 % par niveau).", "Tudo cresce mais depressa (20% por nível).");
            Add("garden.b.Well.desc", "Water every bed at once.", "Podlewa wszystkie grządki naraz.", "Gießt alle Beete auf einmal.", "Riega todos los bancales a la vez.", "Arrose tous les carrés d'un coup.", "Rega todos os canteiros de uma vez.");
            Add("garden.b.Compost.desc", "Turns mushrooms into compost: one extra crop per planting.", "Zamienia grzyby w kompost: dodatkowy plon z grządki.", "Macht aus Pilzen Kompost: +1 Ernte pro Beet.", "Convierte setas en abono: una cosecha más.", "Transforme les champignons en compost : une récolte en plus.", "Transforma cogumelos em composto: mais uma colheita.");
            Add("garden.b.Coop.desc", "Chickens lay eggs all day for the kitchen.", "Kury znoszą jajka przez cały dzień - do kuchni.", "Hühner legen den ganzen Tag Eier für die Küche.", "Las gallinas ponen huevos para la cocina.", "Les poules pondent des œufs pour la cuisine.", "As galinhas põem ovos para a cozinha.");

            // Mushrooms
            Add("mission.Mushrooms", "Mushroom hunt!", "Grzybobranie!", "Pilzsuche!", "¡A por setas!", "Cueillette de champignons !", "Apanha de cogumelos!");
            Add("mission.Mushrooms.do", "Picked {0}/{1}", "Zebrane {0}/{1}", "Gesammelt {0}/{1}", "Recogidas {0}/{1}", "Cueillis {0}/{1}", "Apanhados {0}/{1}");
            Add("mission.Mushrooms.body", "Mushrooms pop up around the village for a moment. Pick {0} in {1} seconds!", "Grzyby wyrastają na chwilę przy domach. Zbierz {0} w {1} sekund!", "Pilze tauchen kurz im Dorf auf. Sammle {0} in {1} Sekunden!", "Aparecen setas por el pueblo un momento. ¡Recoge {0} en {1} segundos!", "Des champignons poussent un instant dans le village. Cueilles-en {0} en {1} secondes !", "Surgem cogumelos pela vila por um momento. Apanha {0} em {1} segundos!");
            Add("fx.mushroom", "+{0} mushroom!", "+{0} grzyb!", "+{0} Pilz!", "¡+{0} seta!", "+{0} champignon !", "+{0} cogumelo!");
            Add("mush.picked", "Mushrooms picked: {0}", "Zebrane grzyby: {0}", "Gesammelte Pilze: {0}", "Setas recogidas: {0}", "Champignons cueillis : {0}", "Cogumelos apanhados: {0}");
            Add("mush.count", "{0} mushrooms", "{0} grzybów", "{0} Pilze", "{0} setas", "{0} champignons", "{0} cogumelos");
            Add("mush.extra_life", "+1 LIFE ({0})", "+1 ŻYCIE ({0})", "+1 LEBEN ({0})", "+1 VIDA ({0})", "+1 VIE ({0})", "+1 VIDA ({0})");
            Add("mush.spring_start", "SPRING START ({0})", "START ZE SPRĘŻYNY ({0})", "FEDER-START ({0})", "SALTO INICIAL ({0})", "DÉPART RESSORT ({0})", "SALTO INICIAL ({0})");
            Add("mush.not_enough", "Not enough goods - visit the forest!", "Za mało - zajrzyj do lasu!", "Nicht genug - geh in den Wald!", "No tienes suficiente: ¡ve al bosque!", "Pas assez - va dans la forêt !", "Não chega - vai à floresta!");
            Add("market.sell", "Sell goods", "Sprzedaj towary", "Waren verkaufen", "Vender productos", "Vendre des produits", "Vender produtos");
            Add("pet.item.mushroom_soup", "Mushroom soup", "Zupa grzybowa", "Pilzsuppe", "Sopa de setas", "Soupe aux champignons", "Sopa de cogumelos");
            Add("pet.item.berry_bowl", "Berry bowl", "Miska jagód", "Beerenschale", "Cuenco de bayas", "Bol de baies", "Taça de bagas");
            Add("pet.item.honey_toast", "Honey toast", "Tost z miodem", "Honigtoast", "Tostada con miel", "Tartine au miel", "Torrada com mel");

            // Tower climb
            Add("tower.title", "TOWER", "WIEŻA", "TURM", "TORRE", "TOUR", "TORRE");
            Add("tower.endless", "Endless tower", "Nieskończona wieża", "Endloser Turm", "Torre infinita", "Tour infinie", "Torre infinita");
            Add("tower.towers", "Towers", "Wieże", "Türme", "Torres", "Tours", "Torres");
            Add("tower.mode.Classic", "CLASSIC", "KLASYK", "KLASSIK", "CLÁSICO", "CLASSIQUE", "CLÁSSICO");
            Add("tower.mode.Speed", "SPEED", "SZYBKI", "TEMPO", "RÁPIDO", "VITESSE", "RÁPIDO");
            Add("tower.mode.Ice", "ICE", "LÓD", "EIS", "HIELO", "GLACE", "GELO");
            Add("tower.modes_info", "Speed: the tower rises fast. Ice: every floor is slippery.", "Szybki: wieża pędzi w górę. Lód: każde piętro jest śliskie.", "Tempo: der Turm steigt schnell. Eis: jede Etage ist glatt.", "Rápido: la torre sube deprisa. Hielo: todos los pisos resbalan.", "Vitesse : la tour monte vite. Glace : chaque étage glisse.", "Rápido: a torre sobe depressa. Gelo: todos os pisos escorregam.");
            Add("tower.scenery.Ice", "Ice", "Lód", "Eis", "Hielo", "Glace", "Gelo");
            Add("tower.scenery.Candy", "Candy", "Cukierki", "Bonbon", "Dulces", "Bonbons", "Doces");
            Add("tower.scenery.Jungle", "Jungle", "Dżungla", "Dschungel", "Selva", "Jungle", "Selva");
            Add("tower.scenery.Space", "Space", "Kosmos", "Weltall", "Espacio", "Espace", "Espaço");
            Add("tower.scenery.Lava", "Lava", "Lawa", "Lava", "Lava", "Lave", "Lava");
            Add("tower.scenery.Forest", "Forest", "Las", "Wald", "Bosque", "Forêt", "Floresta");
            Add("tower.loot.Coin", "{0} coins", "{0} monet", "{0} Münzen", "{0} monedas", "{0} pièces", "{0} moedas");
            Add("tower.loot.Energy", "{0} energy for your pet", "{0} energii dla pupila", "{0} Energie fürs Tier", "{0} energía para tu mascota", "{0} énergie pour ton compagnon", "{0} energia para o bichinho");
            Add("tower.best", "Best: floor {0}", "Rekord: piętro {0}", "Rekord: Etage {0}", "Récord: piso {0}", "Record : étage {0}", "Recorde: piso {0}");
            Add("tower.name", "{0}. {1} tower", "{0}. Wieża: {1}", "{0}. {1}-Turm", "{0}. Torre {1}", "{0}. Tour {1}", "{0}. Torre {1}");
            Add("tower.floors", "{0} floors", "{0} pięter", "{0} Etagen", "{0} pisos", "{0} étages", "{0} pisos");
            Add("tower.locked", "LOCKED", "ZABLOKOWANA", "GESPERRT", "BLOQUEADA", "VERROUILLÉE", "BLOQUEADA");
            Add("tower.climb", "CLIMB +{0}", "WSPINAJ +{0}", "KLETTERN +{0}", "SUBIR +{0}", "GRIMPER +{0}", "SUBIR +{0}");
            Add("tower.replay", "AGAIN +{0}", "PONOWNIE +{0}", "NOCHMAL +{0}", "OTRA VEZ +{0}", "ENCORE +{0}", "OUTRA VEZ +{0}");
            Add("tower.hz.none", "No obstacles - warm up!", "Bez przeszkód - rozgrzewka!", "Keine Hindernisse - zum Aufwärmen!", "Sin obstáculos: ¡calienta!", "Sans obstacles : échauffement !", "Sem obstáculos: aquece!");
            Add("tower.hz.ice", "icy floors", "śliskie piętra", "glatte Etagen", "pisos de hielo", "étages glacés", "pisos de gelo");
            Add("tower.hz.moving", "moving floors", "ruchome piętra", "bewegliche Etagen", "pisos móviles", "étages mobiles", "pisos móveis");
            Add("tower.hz.crumble", "crumbling floors", "kruszące się piętra", "bröckelnde Etagen", "pisos que se rompen", "étages fragiles", "pisos que se partem");
            Add("tower.hz.spring", "springs", "sprężyny", "Federn", "muelles", "ressorts", "molas");
            Add("tower.hz.fast", "fast rise", "szybkie tempo", "schneller Anstieg", "subida rápida", "montée rapide", "subida rápida");
            Add("tower.hz.narrow", "narrow floors", "wąskie piętra", "schmale Etagen", "pisos estrechos", "étages étroits", "pisos estreitos");
            Add("tower.hint", "Slide your finger left / right to run, tap to jump, double tap to double jump. Tip: steer with one thumb and tap with the other. Run fast to jump higher!", "Przesuwaj palec w lewo / prawo, żeby biec, stuknij, żeby skoczyć, stuknij dwa razy, żeby zrobić podwójny skok. Rada: steruj jednym kciukiem, a stukaj drugim. Im szybciej biegniesz, tym wyżej skaczesz!", "Wische nach links / rechts zum Laufen, tippe zum Springen, doppelt tippen für den Doppelsprung. Tipp: mit einem Daumen lenken, mit dem anderen tippen. Je schneller, desto höher!", "Desliza el dedo para correr y toca para saltar y toca dos veces para el doble salto. Consejo: dirige con un pulgar y toca con el otro. ¡Cuanto más rápido, más alto!", "Glisse le doigt pour courir, tape pour sauter, tape deux fois pour un double saut. Astuce : dirige avec un pouce, tape avec l'autre. Plus vite = plus haut !", "Desliza o dedo para correr, toca para saltar, toca duas vezes para o salto duplo. Dica: dirige com um polegar e toca com o outro. Mais rápido = mais alto!");
            Add("tower.floor", "Floor {0}", "Piętro {0}", "Etage {0}", "Piso {0}", "Étage {0}", "Piso {0}");
            Add("tower.floor_of", "Floor {0} / {1}", "Piętro {0} / {1}", "Etage {0} / {1}", "Piso {0} / {1}", "Étage {0} / {1}", "Piso {0} / {1}");
            Add("tower.combo", "COMBO {0}", "COMBO {0}", "KOMBO {0}", "COMBO {0}", "COMBO {0}", "COMBO {0}");
            Add("tower.combo_end", "Combo {0} floors! +{1}", "Combo {0} pięter! +{1}", "Kombo {0} Etagen! +{1}", "¡Combo de {0} pisos! +{1}", "Combo {0} étages ! +{1}", "Combo de {0} pisos! +{1}");
            Add("tower.top", "TOP", "SZCZYT", "GIPFEL", "CIMA", "SOMMET", "TOPO");
            Add("tower.again", "CLIMB AGAIN", "JESZCZE RAZ", "NOCHMAL", "OTRA VEZ", "ENCORE", "OUTRA VEZ");
            Add("tower.won", "Top reached!", "Szczyt zdobyty!", "Gipfel erreicht!", "¡Cima alcanzada!", "Sommet atteint !", "Topo alcançado!");
            Add("tower.fell", "You fell!", "Spadłeś!", "Abgestürzt!", "¡Te caíste!", "Tu es tombé !", "Caíste!");
            Add("tower.record", "New record!", "Nowy rekord!", "Neuer Rekord!", "¡Nuevo récord!", "Nouveau record !", "Novo recorde!");
            Add("tower.result", "Floor {0} · best combo {1} · score {2}", "Piętro {0} · najlepsze combo {1} · wynik {2}", "Etage {0} · bestes Kombo {1} · Punkte {2}", "Piso {0} · mejor combo {1} · puntos {2}", "Étage {0} · meilleur combo {1} · score {2}", "Piso {0} · melhor combo {1} · pontos {2}");
            Add("tower.result_tower", "Floor {0} of {1}", "Piętro {0} z {1}", "Etage {0} von {1}", "Piso {0} de {1}", "Étage {0} sur {1}", "Piso {0} de {1}");
            Add("tower.unlocked_next", "Next tower unlocked!", "Następna wieża odblokowana!", "Nächster Turm freigeschaltet!", "¡Siguiente torre desbloqueada!", "Tour suivante débloquée !", "Próxima torre desbloqueada!");
            Add("tower.try_again", "So close - try again!", "Blisko - spróbuj jeszcze raz!", "Knapp - versuch es nochmal!", "¡Casi! Inténtalo otra vez", "Presque - réessaie !", "Quase - tenta outra vez!");
            Add("tower.arcade_info", "Towers {0}/{1} · best floor {2}", "Wieże {0}/{1} · rekord {2}", "Türme {0}/{1} · Rekord {2}", "Torres {0}/{1} · récord {2}", "Tours {0}/{1} · record {2}", "Torres {0}/{1} · recorde {2}");

            // Pet care
            Add("pet.title", "MY PET", "MÓJ PUPIL", "MEIN TIER", "MI MASCOTA", "MON COMPAGNON", "MEU BICHINHO");
            Add("pet.feed", "FEED", "KARM", "FÜTTERN", "COMIDA", "NOURRIR", "COMIDA");
            Add("pet.wash", "WASH", "UMYJ", "WASCHEN", "BAÑAR", "LAVER", "LAVAR");
            Add("pet.sleep", "SLEEP", "ŚPIJ", "SCHLAFEN", "DORMIR", "DODO", "DORMIR");
            Add("pet.wake", "WAKE UP", "POBUDKA", "AUFWECKEN", "DESPERTAR", "RÉVEIL", "ACORDAR");
            Add("pet.gift", "GIFTS", "PREZENTY", "GESCHENKE", "REGALOS", "CADEAUX", "PRENDAS");
            Add("pet.dress", "DRESS", "UBIERZ", "ANZIEHEN", "VESTIR", "HABILLER", "VESTIR");
            Add("pet.heal", "MEDICINE", "LEK", "MEDIZIN", "MEDICINA", "REMÈDE", "REMÉDIO");
            Add("pet.play", "PLAY", "GRAJ", "SPIELEN", "JUGAR", "JOUER", "JOGAR");
            Add("pet.hint", "Tap your pet to stroke it. Playing levels together is fun but tiring!", "Stuknij pupila, żeby go pogłaskać. Wspólna gra to frajda, ale męczy!", "Tippe dein Tier an, um es zu streicheln. Zusammen spielen macht Spaß, aber müde!", "Toca a tu mascota para acariciarla. ¡Jugar juntos divierte pero cansa!", "Touche ton compagnon pour le caresser. Jouer ensemble amuse mais fatigue !", "Toca no bichinho para lhe fazer festas. Jogar juntos diverte mas cansa!");
            Add("pet.bonus", "Happy pet: +{0}% coins from levels", "Szczęśliwy pupil: +{0}% monet z poziomów", "Glückliches Tier: +{0}% Münzen", "Mascota feliz: +{0}% monedas", "Compagnon heureux : +{0}% de pièces", "Bichinho feliz: +{0}% moedas");
            Add("pet.bonus_off", "Keep it happy for +{0}% coins", "Dbaj o niego, a dostaniesz +{0}% monet", "Halte es glücklich für +{0}% Münzen", "Hazla feliz para +{0}% monedas", "Rends-le heureux pour +{0}% de pièces", "Mantém-no feliz para +{0}% moedas");
            Add("pet.full", "I'm full!", "Jestem najedzony!", "Ich bin satt!", "¡Estoy lleno!", "J'ai plus faim !", "Estou cheio!");
            Add("pet.already_clean", "Already squeaky clean!", "Już lśni czystością!", "Schon blitzsauber!", "¡Ya está limpísimo!", "Déjà tout propre !", "Já está a brilhar!");
            Add("pet.not_sleepy", "Not sleepy yet!", "Jeszcze nie chce spać!", "Noch nicht müde!", "¡Aún no tiene sueño!", "Pas encore sommeil !", "Ainda sem sono!");
            Add("pet.shh", "Shh... sleeping", "Ciii... śpi", "Pssst... schläft", "Shh... duerme", "Chut... il dort", "Shh... a dormir");
            Add("pet.mood.Happy", "Happy and full of energy!", "Szczęśliwy i pełen energii!", "Glücklich und voller Energie!", "¡Feliz y lleno de energía!", "Heureux et plein d'énergie !", "Feliz e cheio de energia!");
            Add("pet.mood.Okay", "Feeling okay", "Czuje się nieźle", "Geht so", "Está bien", "Ça va", "Está bem");
            Add("pet.mood.Hungry", "So hungry...", "Strasznie głodny...", "So hungrig...", "Tiene mucha hambre...", "Trop faim...", "Cheio de fome...");
            Add("pet.mood.Tired", "Very tired...", "Bardzo zmęczony...", "Sehr müde...", "Muy cansado...", "Très fatigué...", "Muito cansado...");
            Add("pet.mood.Bored", "Bored... play with me!", "Nudzi się... pobaw się ze mną!", "Langweilig... spiel mit mir!", "Aburrido... ¡juega conmigo!", "Il s'ennuie... joue avec moi !", "Aborrecido... brinca comigo!");
            Add("pet.mood.Dirty", "Needs a bath!", "Potrzebuje kąpieli!", "Braucht ein Bad!", "¡Necesita un baño!", "Il faut un bain !", "Precisa de banho!");
            Add("pet.mood.Lonely", "Missed you! Feels neglected", "Tęsknił! Czuje się zaniedbany", "Hat dich vermisst! Fühlt sich vernachlässigt", "¡Te echó de menos! Se siente olvidado", "Tu lui as manqué ! Il se sent délaissé", "Teve saudades! Sente-se esquecido");
            Add("pet.mood.Sick", "Sick! Give it medicine", "Chory! Daj mu lekarstwo", "Krank! Gib ihm Medizin", "¡Enfermo! Dale medicina", "Malade ! Donne-lui un remède", "Doente! Dá-lhe o remédio");
            Add("pet.mood.Sleeping", "Sleeping sweetly", "Słodko śpi", "Schläft süß", "Duerme tranquilo", "Dort paisiblement", "A dormir docemente");
            Add("pet.need.Food", "food", "jedzenie", "Essen", "comida", "repas", "comida");
            Add("pet.need.Energy", "energy", "energia", "Energie", "energía", "énergie", "energia");
            Add("pet.need.Fun", "fun", "zabawa", "Spaß", "diversión", "fun", "diversão");
            Add("pet.need.Clean", "clean", "czystość", "Sauber", "limpieza", "propreté", "limpeza");
            Add("pet.item.apple", "Apple", "Jabłko", "Apfel", "Manzana", "Pomme", "Maçã");
            Add("pet.item.sandwich", "Sandwich", "Kanapka", "Sandwich", "Sándwich", "Sandwich", "Sanduíche");
            Add("pet.item.smoothie", "Smoothie", "Koktajl", "Smoothie", "Batido", "Smoothie", "Batido");
            Add("pet.item.burger", "Burger", "Burger", "Burger", "Hamburguesa", "Burger", "Hambúrguer");
            Add("pet.item.cake", "Cake", "Ciasto", "Kuchen", "Pastel", "Gâteau", "Bolo");
            Add("pet.item.ball", "Ball", "Piłka", "Ball", "Pelota", "Ballon", "Bola");
            Add("pet.item.teddy", "Teddy bear", "Miś", "Teddybär", "Osito", "Nounours", "Ursinho");
            Add("pet.item.rocket", "Toy rocket", "Rakieta", "Spielzeugrakete", "Cohete", "Fusée", "Foguetão");

            // Village missions
            Add("mission.Chicken", "Runaway chicken!", "Uciekająca kura!", "Entlaufenes Huhn!", "¡Gallina fugitiva!", "Poule en fuite !", "Galinha fugitiva!");
            Add("mission.Thief", "Stop the thief!", "Złap złodzieja!", "Haltet den Dieb!", "¡Atrapa al ladrón!", "Arrêtez le voleur !", "Pega o ladrão!");
            Add("mission.Balloons", "Balloon festival", "Festiwal balonów", "Ballonfest", "Festival de globos", "Fête des ballons", "Festival de balões");
            Add("mission.Cat", "Hide and seek cat", "Kot w chowanego", "Versteck-Katze", "Gato escondido", "Chat caché", "Gato escondido");
            Add("mission.Chicken.do", "Caught {0}/{1}", "Złapano {0}/{1}", "Gefangen {0}/{1}", "Atrapada {0}/{1}", "Attrapée {0}/{1}", "Apanhada {0}/{1}");
            Add("mission.Thief.do", "Tackles {0}/{1}", "Złapania {0}/{1}", "Gestoppt {0}/{1}", "Placajes {0}/{1}", "Plaquages {0}/{1}", "Agarrões {0}/{1}");
            Add("mission.Balloons.do", "Popped {0}/{1}", "Przebite {0}/{1}", "Geplatzt {0}/{1}", "Reventados {0}/{1}", "Éclatés {0}/{1}", "Rebentados {0}/{1}");
            Add("mission.Cat.do", "Found {0}/{1}", "Znaleziono {0}/{1}", "Gefunden {0}/{1}", "Encontrado {0}/{1}", "Trouvé {0}/{1}", "Encontrado {0}/{1}");
            Add("mission.Chicken.body", "A chicken escaped from the farm! Tap it {0} times in {1} seconds to catch it.", "Kura uciekła z farmy! Stuknij ją {0} razy w {1} sekund, żeby ją złapać.", "Ein Huhn ist entlaufen! Tippe es {0}-mal in {1} Sekunden an.", "¡Se escapó una gallina! Tócala {0} veces en {1} segundos.", "Une poule s'est échappée ! Touche-la {0} fois en {1} secondes.", "Uma galinha fugiu! Toca nela {0} vezes em {1} segundos.");
            Add("mission.Thief.body", "A thief grabbed a sack of coins! Tap him {0} times in {1} seconds before he gets away.", "Złodziej porwał worek monet! Stuknij go {0} razy w {1} sekund, zanim ucieknie.", "Ein Dieb hat einen Münzsack! Tippe ihn {0}-mal in {1} Sekunden an.", "¡Un ladrón robó monedas! Tócalo {0} veces en {1} segundos.", "Un voleur a pris des pièces ! Touche-le {0} fois en {1} secondes.", "Um ladrão roubou moedas! Toca nele {0} vezes em {1} segundos.");
            Add("mission.Balloons.body", "Balloons are floating over the village. Pop {0} of them in {1} seconds!", "Nad wioską lecą balony. Przebij {0} w {1} sekund!", "Ballons schweben über dem Dorf. Lass {0} in {1} Sekunden platzen!", "Hay globos sobre el pueblo. ¡Revienta {0} en {1} segundos!", "Des ballons flottent sur le village. Éclates-en {0} en {1} secondes !", "Há balões sobre a vila. Rebenta {0} em {1} segundos!");
            Add("mission.Cat.body", "A cat is hiding next to the buildings and keeps moving. Find it {0} times in {1} seconds!", "Kot chowa się przy budynkach i ciągle zmienia miejsce. Znajdź go {0} razy w {1} sekund!", "Eine Katze versteckt sich bei den Häusern. Finde sie {0}-mal in {1} Sekunden!", "Un gato se esconde junto a los edificios. ¡Encuéntralo {0} veces en {1} segundos!", "Un chat se cache près des bâtiments. Trouve-le {0} fois en {1} secondes !", "Um gato esconde-se junto aos edifícios. Encontra-o {0} vezes em {1} segundos!");
            Add("mission.offer_chip", "Event: {0}", "Wydarzenie: {0}", "Ereignis: {0}", "Evento: {0}", "Événement : {0}", "Evento: {0}");
            Add("mission.reward", "Reward: {0}", "Nagroda: {0}", "Belohnung: {0}", "Premio: {0}", "Récompense : {0}", "Prémio: {0}");
            Add("mission.coins", "{0} coins", "{0} monet", "{0} Münzen", "{0} monedas", "{0} pièces", "{0} moedas");
            Add("mission.supply", "{0} {1}", "{0} {1}", "{0} {1}", "{0} {1}", "{0} {1}", "{0} {1}");
            Add("mission.start", "LET'S GO", "DO DZIEŁA", "LOS GEHT'S", "¡VAMOS!", "C'EST PARTI", "VAMOS");
            Add("mission.time", "{0} s left", "Zostało {0} s", "Noch {0} s", "Quedan {0} s", "Encore {0} s", "Faltam {0} s");
            Add("mission.success", "{0} - done!", "{0} - gotowe!", "{0} - geschafft!", "{0}: ¡hecho!", "{0} : réussi !", "{0} - feito!");
            Add("mission.fail", "Out of time! Another event will come soon.", "Koniec czasu! Wkrótce pojawi się kolejne wydarzenie.", "Zeit abgelaufen! Bald gibt es ein neues Ereignis.", "¡Se acabó el tiempo! Pronto habrá otro evento.", "Temps écoulé ! Un autre événement arrive bientôt.", "Acabou o tempo! Outro evento vem já.");
            Add("city.full", "FULL", "PEŁNE", "VOLL", "LLENO", "PLEIN", "CHEIO");
            Add("city.intro",
                "Win levels to earn coins and supplies, then build and upgrade your city. Buildings keep producing while you're away.",
                "Wygrywaj poziomy, zdobywaj monety i materiały, a potem buduj i rozwijaj miasto. Budynki produkują, nawet gdy nie grasz.",
                "Gewinne Levels für Münzen und Material, dann baue deine Stadt aus. Gebäude produzieren auch, wenn du nicht spielst.",
                "Gana niveles para conseguir monedas y materiales, y luego construye tu ciudad. Los edificios producen aunque no juegues.",
                "Gagne des niveaux pour obtenir pièces et matériaux, puis construis ta ville. Les bâtiments produisent même quand tu ne joues pas.",
                "Vença níveis para ganhar moedas e materiais, depois construa sua cidade. Os prédios produzem mesmo quando você não joga.");
            Add("market.info", "Buy building supplies with coins.", "Kupuj materiały budowlane za monety.", "Kaufe Baumaterial mit Münzen.",
                "Compra materiales con monedas.", "Achète des matériaux avec des pièces.", "Compre materiais com moedas.");
            Add("supply.Wood", "Wood", "Drewno", "Holz", "Madera", "Bois", "Madeira");
            Add("supply.Brick", "Bricks", "Cegły", "Ziegel", "Ladrillos", "Briques", "Tijolos");
            Add("supply.Glass", "Glass", "Szkło", "Glas", "Vidrio", "Verre", "Vidro");
            Add("building.house", "House", "Dom", "Haus", "Casa", "Maison", "Casa");
            Add("building.sawmill", "Sawmill", "Tartak", "Sägewerk", "Aserradero", "Scierie", "Serraria");
            Add("building.bakery", "Bakery", "Piekarnia", "Bäckerei", "Panadería", "Boulangerie", "Padaria");
            Add("building.brickworks", "Brickworks", "Cegielnia", "Ziegelei", "Ladrillera", "Briqueterie", "Olaria");
            Add("building.park", "Park", "Park", "Park", "Parque", "Parc", "Parque");
            Add("building.windmill", "Windmill", "Wiatrak", "Windmühle", "Molino", "Moulin", "Moinho");
            Add("building.glassworks", "Glassworks", "Huta szkła", "Glashütte", "Vidriería", "Verrerie", "Vidraria");
            Add("building.market", "Market Hall", "Hala targowa", "Markthalle", "Mercado", "Halle", "Mercadão");
            Add("building.tower", "Tower", "Wieża", "Turm", "Torre", "Tour", "Torre");
            Add("building.castle", "Castle", "Zamek", "Schloss", "Castillo", "Château", "Castelo");

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

            // ---------------------------------------------------------------- player level & season pass
            Add("item.skin_bubble", "Bubble Pop", "Bańkowy pop", "Blubber-Pop", "Pop de burbujas", "Bulles pop", "Pop de bolhas");
            Add("item.skin_champion", "Champion", "Mistrz", "Champion", "Campeón", "Champion", "Campeão");
            Add("item.theme_aurora", "Aurora", "Zorza", "Polarlicht", "Aurora", "Aurore", "Aurora");
            Add("item.theme_bloom", "Spring Bloom", "Wiosenny rozkwit", "Frühlingsblüte", "Flor de primavera", "Floraison", "Flor da primavera");
            Add("item.skin_blossom", "Blossom", "Kwiat wiśni", "Kirschblüte", "Cerezo", "Fleur de cerisier", "Flor de cerejeira");
            Add("item.theme_splash", "Summer Splash", "Letni plusk", "Sommerplatsch", "Chapuzón de verano", "Plouf d'été", "Mergulho de verão");
            Add("item.skin_tropical", "Tropical", "Tropiki", "Tropisch", "Tropical", "Tropical", "Tropical");
            Add("item.theme_harvest", "Autumn Arcade", "Jesienny automat", "Herbst-Arcade", "Arcade de otoño", "Arcade d'automne", "Arcade de outono");
            Add("item.skin_maple", "Maple", "Klon", "Ahorn", "Arce", "Érable", "Bordo");
            Add("item.theme_frost", "Winter Glow", "Zimowy blask", "Winterglanz", "Brillo de invierno", "Lueur d'hiver", "Brilho de inverno");
            Add("item.skin_ice", "Ice Crystal", "Lodowy kryształ", "Eiskristall", "Cristal de hielo", "Cristal de glace", "Cristal de gelo");

            Add("player.level", "LEVEL {0}", "POZIOM {0}", "STUFE {0}", "NIVEL {0}", "NIVEAU {0}", "NÍVEL {0}");
            Add("player.lv", "LV", "POZ", "ST", "NV", "NIV", "NV");
            Add("player.level_up", "LEVEL {0}!", "POZIOM {0}!", "STUFE {0}!", "¡NIVEL {0}!", "NIVEAU {0} !", "NÍVEL {0}!");
            Add("player.info",
                "{0} / {1} XP to the next level.\nYour level gives +{2}% coins from every game (+1% every 5 levels).",
                "{0} / {1} XP do następnego poziomu.\nTwój poziom daje +{2}% monet z każdej gry (+1% co 5 poziomów).",
                "{0} / {1} XP bis zur nächsten Stufe.\nDeine Stufe gibt +{2} % Münzen pro Spiel (+1 % alle 5 Stufen).",
                "{0} / {1} XP para el siguiente nivel.\nTu nivel da +{2}% de monedas en cada partida (+1% cada 5 niveles).",
                "{0} / {1} XP jusqu'au niveau suivant.\nTon niveau donne +{2} % de pièces par partie (+1 % tous les 5 niveaux).",
                "{0} / {1} XP para o próximo nível.\nSeu nível dá +{2}% de moedas em cada partida (+1% a cada 5 níveis).");
            Add("player.next_item", "Level {0} unlocks: {1}", "Poziom {0} odblokowuje: {1}", "Stufe {0} schaltet frei: {1}",
                "El nivel {0} desbloquea: {1}", "Le niveau {0} débloque : {1}", "O nível {0} desbloqueia: {1}");
            Add("player.gain", "+{0} XP  ·  +{0} season points", "+{0} XP  ·  +{0} pkt sezonu", "+{0} XP  ·  +{0} Saisonpunkte",
                "+{0} XP  ·  +{0} puntos de temporada", "+{0} XP  ·  +{0} points de saison", "+{0} XP  ·  +{0} pontos da temporada");
            Add("go.bonus", "city & level +{0}%", "miasto i poziom +{0}%", "Stadt & Stufe +{0} %", "ciudad y nivel +{0}%", "ville & niveau +{0} %", "cidade e nível +{0}%");

            Add("season.home", "SEASON", "SEZON", "SAISON", "TEMPORADA", "SAISON", "TEMPORADA");
            Add("season.title", "SEASON {0}", "SEZON {0}", "SAISON {0}", "TEMPORADA {0}", "SAISON {0}", "TEMPORADA {0}");
            Add("season.name.0", "Spring Bloom", "Wiosenny rozkwit", "Frühlingsblüte", "Flor de primavera", "Floraison", "Flor da primavera");
            Add("season.name.1", "Summer Splash", "Letni plusk", "Sommerplatsch", "Chapuzón de verano", "Plouf d'été", "Mergulho de verão");
            Add("season.name.2", "Autumn Arcade", "Jesienny automat", "Herbst-Arcade", "Arcade de otoño", "Arcade d'automne", "Arcade de outono");
            Add("season.name.3", "Winter Glow", "Zimowy blask", "Winterglanz", "Brillo de invierno", "Lueur d'hiver", "Brilho de inverno");
            Add("season.ends", "Ends in {0}d {1}h", "Koniec za {0} d {1} h", "Endet in {0} T {1} Std.", "Termina en {0} d {1} h", "Fin dans {0} j {1} h", "Termina em {0} d {1} h");
            Add("season.tier", "TIER {0}", "STOPIEŃ {0}", "STUFE {0}", "NIVEL {0}", "PALIER {0}", "NÍVEL {0}");
            Add("season.free", "FREE", "DARMOWE", "GRATIS", "GRATIS", "GRATUIT", "GRÁTIS");
            Add("season.pass", "PASS", "PRZEPUSTKA", "PASS", "PASE", "PASS", "PASSE");
            Add("season.buy", "GET PASS {0}", "KUP PRZEPUSTKĘ {0}", "PASS HOLEN {0}", "CONSEGUIR PASE {0}", "OBTENIR LE PASS {0}", "PEGAR PASSE {0}");
            Add("season.active", "PASS ACTIVE", "PRZEPUSTKA AKTYWNA", "PASS AKTIV", "PASE ACTIVO", "PASS ACTIF", "PASSE ATIVO");
            Add("season.buy_title", "Season Pass", "Przepustka sezonowa", "Saisonpass", "Pase de temporada", "Pass de saison", "Passe da temporada");
            Add("season.buy_body",
                "Unlock the pass track for this season: an exclusive skin and avatar, 2,000 bonus coins, shards and building supplies. Tiers you already reached are unlocked at once.",
                "Odblokuj ścieżkę przepustki w tym sezonie: ekskluzywną skórkę i awatara, 2000 monet, kryształy i materiały budowlane. Osiągnięte już stopnie odblokują się od razu.",
                "Schalte die Pass-Leiste dieser Saison frei: exklusiver Skin und Avatar, 2.000 Bonusmünzen, Splitter und Baumaterial. Bereits erreichte Stufen werden sofort freigeschaltet.",
                "Desbloquea el pase de esta temporada: aspecto y avatar exclusivos, 2000 monedas extra, fragmentos y materiales. Los niveles ya alcanzados se desbloquean al momento.",
                "Débloque le pass de cette saison : skin et avatar exclusifs, 2 000 pièces bonus, éclats et matériaux. Les paliers déjà atteints sont débloqués tout de suite.",
                "Desbloqueie o passe desta temporada: visual e avatar exclusivos, 2.000 moedas extras, fragmentos e materiais. Os níveis já alcançados são liberados na hora.");
            Add("season.bought", "Pass unlocked!", "Przepustka odblokowana!", "Pass freigeschaltet!", "¡Pase desbloqueado!", "Pass débloqué !", "Passe liberado!");
            Add("season.claim_all", "CLAIM ALL", "ODBIERZ WSZYSTKO", "ALLES ABHOLEN", "RECOGER TODO", "TOUT RÉCUPÉRER", "PEGAR TUDO");
            Add("season.claimed", "Rewards claimed!", "Nagrody odebrane!", "Belohnungen abgeholt!", "¡Premios recogidos!", "Récompenses récupérées !", "Prêmios recebidos!");
            Add("season.reach", "Reach tier {0} to claim this.", "Osiągnij stopień {0}, by to odebrać.", "Erreiche Stufe {0}, um das abzuholen.",
                "Alcanza el nivel {0} para recogerlo.", "Atteins le palier {0} pour le récupérer.", "Alcance o nível {0} para pegar isso.");
            Add("season.how", "Earn season points by playing games, winning levels and finishing missions.",
                "Zdobywaj punkty sezonu, grając, wygrywając poziomy i wykonując misje.",
                "Sammle Saisonpunkte durch Spiele, gewonnene Levels und Missionen.",
                "Gana puntos de temporada jugando, superando niveles y completando misiones.",
                "Gagne des points de saison en jouant, en réussissant des niveaux et en finissant des missions.",
                "Ganhe pontos da temporada jogando, vencendo níveis e completando missões.");
            Add("season.new", "NEW SEASON!", "NOWY SEZON!", "NEUE SAISON!", "¡NUEVA TEMPORADA!", "NOUVELLE SAISON !", "NOVA TEMPORADA!");
            Add("season.new_body", "Rewards you hadn't claimed last season were added for you:\n{0}",
                "Nieodebrane nagrody z poprzedniego sezonu zostały ci dodane:\n{0}",
                "Nicht abgeholte Belohnungen der letzten Saison wurden dir gutgeschrieben:\n{0}",
                "Te añadimos los premios que no recogiste la temporada pasada:\n{0}",
                "Les récompenses non récupérées de la saison passée ont été ajoutées :\n{0}",
                "Os prêmios que você não pegou na última temporada foram adicionados:\n{0}");
            Add("shop.level_reward", "LEVEL {0}", "POZIOM {0}", "STUFE {0}", "NIVEL {0}", "NIVEAU {0}", "NÍVEL {0}");
            Add("shop.season_reward", "SEASON", "SEZON", "SAISON", "TEMPORADA", "SAISON", "TEMPORADA");
            Add("shop.reward_body", "This item can't be bought. It's a reward for reaching player level {0}.",
                "Tego nie da się kupić. To nagroda za osiągnięcie poziomu gracza {0}.",
                "Das kann man nicht kaufen. Es ist eine Belohnung für Spielerstufe {0}.",
                "No se puede comprar. Es un premio por llegar al nivel de jugador {0}.",
                "Impossible à acheter. C'est une récompense du niveau de joueur {0}.",
                "Não dá para comprar. É um prêmio por chegar ao nível de jogador {0}.");
            Add("shop.season_body", "This item can't be bought. It comes back in its season's pass.",
                "Tego nie da się kupić. Wraca w przepustce swojego sezonu.",
                "Das kann man nicht kaufen. Es kommt im Pass seiner Saison zurück.",
                "No se puede comprar. Vuelve en el pase de su temporada.",
                "Impossible à acheter. Il revient dans le pass de sa saison.",
                "Não dá para comprar. Ele volta no passe da sua temporada.");
            // ---------------------------------------------------------------- Google account
            Add("account.title", "Google account", "Konto Google", "Google-Konto", "Cuenta de Google", "Compte Google", "Conta Google");
            Add("account.sign_in", "SIGN IN WITH GOOGLE", "ZALOGUJ PRZEZ GOOGLE", "MIT GOOGLE ANMELDEN", "INICIAR SESIÓN CON GOOGLE", "CONNEXION AVEC GOOGLE", "ENTRAR COM GOOGLE");
            Add("account.google_name", "Google: {0}", "Google: {0}", "Google: {0}", "Google: {0}", "Google : {0}", "Google: {0}");
            Add("account.signed_in", "Signed in as {0}.", "Zalogowano jako {0}.", "Angemeldet als {0}.", "Sesión iniciada como {0}.", "Connecté en tant que {0}.", "Conectado como {0}.");
            Add("account.welcome", "Welcome!", "Witaj!", "Willkommen!", "¡Bienvenido!", "Bienvenue !", "Bem-vindo!");
            Add("account.failed", "Sign-in didn't work. Check your connection and try again.", "Logowanie się nie udało. Sprawdź połączenie i spróbuj ponownie.",
                "Anmeldung fehlgeschlagen. Prüfe deine Verbindung und versuche es erneut.", "No se pudo iniciar sesión. Revisa tu conexión e inténtalo de nuevo.",
                "La connexion a échoué. Vérifie ta connexion et réessaie.", "Não foi possível entrar. Verifique sua conexão e tente de novo.");
            Add("account.unavailable", "Google sign-in works in the Android app once Google Play Games is set up.",
                "Logowanie Google działa w aplikacji na Androida po skonfigurowaniu Google Play Games.",
                "Die Google-Anmeldung funktioniert in der Android-App, sobald Google Play Games eingerichtet ist.",
                "El inicio de sesión con Google funciona en la app de Android cuando Google Play Games está configurado.",
                "La connexion Google fonctionne dans l'appli Android une fois Google Play Jeux configuré.",
                "O login com Google funciona no app Android depois de configurar o Google Play Games.");

            // ---------------------------------------------------------------- Block Run bonus
            Same("run.title", "BLOCK RUN");
            Add("run.life_lost", "Ouch! Lives left: {0}", "Auć! Zostało żyć: {0}", "Autsch! Noch {0} Leben", "¡Ay! Vidas: {0}", "Aïe ! Vies restantes : {0}", "Ai! Vidas: {0}");
            Add("run.hint", "Swipe left / right to change lanes\nup to jump, down to slide", "Przesuń w lewo / prawo, by zmienić tor\nw górę: skok, w dół: ślizg",
                "Wische links / rechts für die Spur\nhoch: springen, runter: rutschen", "Desliza izq. / der. para cambiar de carril\narriba: saltar, abajo: deslizar",
                "Glisse à gauche / droite pour changer de voie\nhaut : sauter, bas : glisser", "Deslize esq. / dir. para trocar de faixa\ncima: pular, baixo: deslizar");
            Add("run.paused", "PAUSED\ntap pause to continue", "PAUZA\nstuknij pauzę, by grać dalej", "PAUSE\nPause tippen zum Weiterspielen",
                "PAUSA\ntoca pausa para seguir", "PAUSE\ntape pause pour continuer", "PAUSA\ntoque pausa para continuar");
            Add("run.over", "RUN OVER!", "KONIEC BIEGU!", "LAUF VORBEI!", "¡FIN DE LA CARRERA!", "COURSE TERMINÉE !", "FIM DA CORRIDA!");
            Add("run.result", "{0} m  ·  {1} coins picked up", "{0} m  ·  zebrane monety: {1}", "{0} m  ·  {1} Münzen eingesammelt",
                "{0} m  ·  {1} monedas recogidas", "{0} m  ·  {1} pièces ramassées", "{0} m  ·  {1} moedas coletadas");
            Add("run.record", "New record!", "Nowy rekord!", "Neuer Rekord!", "¡Nuevo récord!", "Nouveau record !", "Novo recorde!");
            Add("run.best", "Best: {0} m", "Rekord: {0} m", "Bestwert: {0} m", "Récord: {0} m", "Record : {0} m", "Recorde: {0} m");
            Add("run.again", "RUN AGAIN ({0})", "BIEGNIJ ZNOWU ({0})", "NOCHMAL ({0})", "OTRA VEZ ({0})", "ENCORE ({0})", "DE NOVO ({0})");
            Add("run.no_tickets", "NO RUNS LEFT", "BRAK BIEGÓW", "KEINE LÄUFE MEHR", "SIN CARRERAS", "PLUS DE COURSES", "SEM CORRIDAS");
            Add("run.no_tickets_body", "One free run every day. Win a Boost level for an extra run!",
                "Jeden darmowy bieg dziennie. Wygraj poziom Boost, by dostać dodatkowy!",
                "Ein Gratis-Lauf pro Tag. Gewinne ein Boost-Level für einen Extra-Lauf!",
                "Una carrera gratis al día. ¡Gana un nivel Boost para una extra!",
                "Une course gratuite par jour. Gagne un niveau Boost pour une course en plus !",
                "Uma corrida grátis por dia. Vença um nível Boost para ganhar outra!");
            Add("run.info", "3D bonus run: collect coins!  Best {0} m", "Bonusowy bieg 3D: zbieraj monety!  Rekord {0} m",
                "3D-Bonuslauf: sammle Münzen!  Bestwert {0} m", "Carrera bonus 3D: ¡recoge monedas!  Récord {0} m",
                "Course bonus 3D : ramasse des pièces !  Record {0} m", "Corrida bônus 3D: pegue moedas!  Recorde {0} m");
            Add("run.again_free", "RUN AGAIN", "BIEGNIJ ZNOWU", "NOCHMAL", "OTRA VEZ", "ENCORE", "DE NOVO");
            Add("run.again_bonus", "RUN AGAIN  x2 ({0})", "BIEGNIJ ZNOWU  x2 ({0})", "NOCHMAL  x2 ({0})", "OTRA VEZ  x2 ({0})", "ENCORE  x2 ({0})", "DE NOVO  x2 ({0})");
            Add("run.info_bonus", "Bonus runs with x2 coins: {0}", "Biegi bonusowe z x2 monet: {0}", "Bonusläufe mit x2 Münzen: {0}", "Carreras bonus con x2 monedas: {0}", "Courses bonus à x2 pièces : {0}", "Corridas bónus com x2 moedas: {0}");
            Add("run.doubled", "Bonus run: coins x2!", "Bieg bonusowy: monety x2!", "Bonuslauf: Münzen x2!", "Carrera bonus: ¡monedas x2!", "Course bonus : pièces x2 !", "Corrida bónus: moedas x2!");
            Add("run.back", "BACK", "WRÓĆ", "ZURÜCK", "VOLVER", "RETOUR", "VOLTAR");
            Add("run.ticket_won", "+1 Block Run bonus (x2 coins)!", "+1 bieg bonusowy Block Run (x2 monet)!", "+1 Block-Run-Bonus (x2 Münzen)!", "¡+1 carrera bonus (x2 monedas)!", "+1 course bonus (x2 pièces) !", "+1 corrida bónus (x2 moedas)!");

            // ---------------------------------------------------------------- wardrobe
            Add("home.style", "STYLE", "STYL", "STYLE", "ESTILO", "STYLE", "ESTILO");
            Add("wardrobe.title", "STYLE", "STYL", "STYLE", "ESTILO", "STYLE", "ESTILO");
            Add("wardrobe.colors", "COLOURS", "KOLORY", "FARBEN", "COLORES", "COULEURS", "CORES");
            Add("wardrobe.hats", "HATS", "CZAPKI", "HÜTE", "GORROS", "CHAPEAUX", "CHAPÉUS");
            Add("wardrobe.glasses", "GLASSES", "OKULARY", "BRILLEN", "GAFAS", "LUNETTES", "ÓCULOS");
            Add("wardrobe.wear", "WEAR", "ZAŁÓŻ", "TRAGEN", "PONER", "PORTER", "USAR");
            Add("wardrobe.worn", "ON", "ZAŁOŻONE", "AN", "PUESTO", "PORTÉ", "USANDO");
            Add("wardrobe.original", "Original", "Oryginalny", "Original", "Original", "Original", "Original");
            Add("wardrobe.color_coral", "Coral", "Koralowy", "Koralle", "Coral", "Corail", "Coral");
            Add("wardrobe.color_mint", "Mint", "Miętowy", "Minze", "Menta", "Menthe", "Menta");
            Add("wardrobe.color_sky", "Sky", "Błękitny", "Himmel", "Cielo", "Ciel", "Céu");
            Add("wardrobe.color_lilac", "Lilac", "Liliowy", "Flieder", "Lila", "Lilas", "Lilás");
            Add("wardrobe.color_lemon", "Lemon", "Cytrynowy", "Zitrone", "Limón", "Citron", "Limão");
            Add("wardrobe.color_midnight", "Midnight", "Północ", "Mitternacht", "Medianoche", "Minuit", "Meia-noite");
            Add("wardrobe.color_gold", "Gold", "Złoty", "Gold", "Oro", "Or", "Ouro");
            Add("wardrobe.hat_cap", "Cap", "Czapka z daszkiem", "Kappe", "Gorra", "Casquette", "Boné");
            Add("wardrobe.hat_beanie", "Beanie", "Czapka zimowa", "Mütze", "Gorro", "Bonnet", "Gorro");
            Add("wardrobe.hat_party", "Party hat", "Czapeczka imprezowa", "Partyhut", "Gorro de fiesta", "Chapeau de fête", "Chapéu de festa");
            Add("wardrobe.hat_headphones", "Headphones", "Słuchawki", "Kopfhörer", "Auriculares", "Casque", "Fones");
            Add("wardrobe.hat_cowboy", "Cowboy hat", "Kapelusz kowbojski", "Cowboyhut", "Sombrero vaquero", "Chapeau de cowboy", "Chapéu de caubói");
            Add("wardrobe.hat_crown", "Crown", "Korona", "Krone", "Corona", "Couronne", "Coroa");
            Add("wardrobe.glasses_sun", "Sunglasses", "Okulary przeciwsłoneczne", "Sonnenbrille", "Gafas de sol", "Lunettes de soleil", "Óculos de sol");
            Add("wardrobe.glasses_round", "Round glasses", "Okrągłe okulary", "Runde Brille", "Gafas redondas", "Lunettes rondes", "Óculos redondos");
            Add("wardrobe.glasses_star", "Star glasses", "Okulary gwiazdki", "Sternbrille", "Gafas de estrella", "Lunettes étoiles", "Óculos de estrela");
            Add("wardrobe.glasses_visor", "Visor", "Wizjer", "Visier", "Visor", "Visière", "Viseira");
            Add("reward.shards", "{0} shards", "{0} kryształów", "{0} Splitter", "{0} fragmentos", "{0} éclats", "{0} fragmentos");
        }
    }
}
