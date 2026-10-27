using System.Collections.Generic;

namespace JellyDrop.Core.Economy
{
    public enum ItemCategory { Skin, Theme, Avatar }

    public enum Rarity { Common, Rare, Epic, Premium }

    /// <summary>How a block skin is drawn. The Unity layer turns this into textures.</summary>
    public enum BlockStyle { Jelly, Gummy, Sprinkles, Gem, Pixel, Neon, Gold }

    /// <summary>Which procedural music style plays with a theme.</summary>
    public enum MusicStyle { Bubbly, Mellow, Funky, Space, Dreamy, Arcade }

    public sealed class ShopItem
    {
        public string Id;
        public string Name;
        public ItemCategory Category;
        public Rarity Rarity;
        /// <summary>0 means free / owned from the start.</summary>
        public int CoinPrice;
        /// <summary>Alternative price in skin shards (from the lucky wheel); 0 = not sold for shards.</summary>
        public int ShardPrice;
        /// <summary>Granted with the Premium purchase; cannot be bought with coins.</summary>
        public bool PremiumOnly;

        // Skin data
        public BlockStyle BlockStyle;
        /// <summary>Seven piece colours (I, O, T, S, Z, J, L) as hex "#RRGGBB".</summary>
        public string[] Palette;

        // Theme data
        public string BackgroundTop;
        public string BackgroundBottom;
        public string Accent;
        public MusicStyle Music;
        public int MusicBpm;
        /// <summary>Draws a glowing synthwave grid at the bottom of the background.</summary>
        public bool Grid;

        // Avatar data
        public string BodyColor;
        /// <summary>0 round, 1 drop, 2 square, 3 star, 4 crown.</summary>
        public int AvatarShape;

        public bool IsFree => CoinPrice == 0 && ShardPrice == 0 && !PremiumOnly;
    }

    public sealed class PowerUpOffer
    {
        public string Id;
        public Core.PowerUpType Type;
        public string Name;
        public string Description;
        public int Quantity;
        public int CoinPrice;
    }

    /// <summary>Everything that can be bought. Kept in code so prices can be tuned and later overridden remotely.</summary>
    public sealed class Catalog
    {
        public readonly List<ShopItem> Items = new List<ShopItem>();
        public readonly List<PowerUpOffer> PowerUpOffers = new List<PowerUpOffer>();

        private readonly Dictionary<string, ShopItem> _byId = new Dictionary<string, ShopItem>();

        public ShopItem Get(string id) => id != null && _byId.TryGetValue(id, out var item) ? item : null;

        public IEnumerable<ShopItem> InCategory(ItemCategory category)
        {
            foreach (var item in Items) if (item.Category == category) yield return item;
        }

        public string DefaultId(ItemCategory category)
        {
            foreach (var item in Items) if (item.Category == category && item.IsFree) return item.Id;
            return null;
        }

        public void Add(ShopItem item)
        {
            Items.Add(item);
            _byId[item.Id] = item;
        }

        // ------------------------------------------------------------------ default content

        public static Catalog CreateDefault()
        {
            var c = new Catalog();

            // Skins -----------------------------------------------------------
            // The first free item of each category is the default for new players.
            c.Add(Skin("skin_soft", "Soft Arcade", Rarity.Common, 0, 0, BlockStyle.Jelly,
                "#5CD3F5", "#FFD25A", "#B48CFF", "#6FE0A0", "#FF7A8A", "#6E9BFF", "#FFA65C"));
            c.Add(Skin("skin_neon", "Neon Arcade", Rarity.Common, 250, 0, BlockStyle.Neon,
                "#00F0FF", "#FFF200", "#FF00F5", "#39FF14", "#FF2E63", "#4D7CFF", "#FF8C00"));
            c.Add(Skin("skin_jelly", "Jelly Classic", Rarity.Common, 250, 0, BlockStyle.Jelly,
                "#3FD0FF", "#FFD23F", "#C86BFA", "#5BE37D", "#FF5C7A", "#4D7CFF", "#FF9F43"));
            c.Add(Skin("skin_gummy", "Gummy Buddies", Rarity.Common, 300, 0, BlockStyle.Gummy,
                "#7FE7FF", "#FFE066", "#E29BFF", "#8CF29E", "#FF8FA3", "#8AA8FF", "#FFBE7A"));
            c.Add(Skin("skin_pixel", "Pixel Pop", Rarity.Common, 400, 0, BlockStyle.Pixel,
                "#00E5FF", "#FFEA00", "#D500F9", "#00E676", "#FF1744", "#2979FF", "#FF9100"));
            c.Add(Skin("skin_sprinkles", "Sprinkle Donut", Rarity.Rare, 800, 0, BlockStyle.Sprinkles,
                "#9EE8FF", "#FFF1A8", "#F3B6FF", "#B9F7C4", "#FFB3C1", "#B3C6FF", "#FFD3A8"));
            c.Add(Skin("skin_gems", "Gem Drops", Rarity.Rare, 1200, 0, BlockStyle.Gem,
                "#1EC8FF", "#FFC400", "#A64DFF", "#21D07A", "#FF3D6E", "#3A5BFF", "#FF7B1C"));
            c.Add(Skin("skin_plasma", "Vapor Plasma", Rarity.Epic, 2500, 10, BlockStyle.Neon,
                "#01CDFE", "#FFFB96", "#B967FF", "#05FFA1", "#FF71CE", "#7A5CFF", "#FF9E6E"));
            c.Add(Premium(Skin("skin_gold", "Royal Gold", Rarity.Premium, 0, 0, BlockStyle.Gold,
                "#FFE27A", "#FFD447", "#F5C542", "#FFE9A3", "#F2B632", "#FFDB6E", "#E8A93A")));

            // Themes (background + music) ----------------------------------------
            c.Add(Theme("theme_daydream", "Daydream", Rarity.Common, 0, "#EAF0FF", "#FDE9F3", "#FF6F91", MusicStyle.Bubbly, 124));
            c.Add(Theme("theme_arcade", "Neon Arcade", Rarity.Common, 400, "#0B1033", "#2A0B4F", "#FF2E88", MusicStyle.Arcade, 132, grid: true));
            c.Add(Theme("theme_cyber", "Cyber Grid", Rarity.Rare, 1200, "#001426", "#003A4D", "#00E5FF", MusicStyle.Space, 140, grid: true));
            c.Add(Theme("theme_candy", "Candy Land", Rarity.Common, 500, "#FFB6D9", "#B38CFF", "#FF5FA2", MusicStyle.Bubbly, 128));
            c.Add(Theme("theme_mint", "Mint Meadow", Rarity.Common, 500, "#B5FFE1", "#5CC8FF", "#2ED8A3", MusicStyle.Mellow, 112));
            c.Add(Theme("theme_cotton", "Cotton Cloud", Rarity.Common, 600, "#FFFFFF", "#FFC6E7", "#FF8AC5", MusicStyle.Dreamy, 104));
            c.Add(Theme("theme_soda", "Sunset Soda", Rarity.Rare, 900, "#FFD27F", "#FF6F91", "#FF9671", MusicStyle.Funky, 120));
            c.Add(Theme("theme_galaxy", "Bubblegum Galaxy", Rarity.Epic, 1800, "#3A1C71", "#D76D77", "#FFAF7B", MusicStyle.Space, 136));

            // Avatars ------------------------------------------------------------
            c.Add(Avatar("avatar_bloop", "Bloop", Rarity.Common, 0, "#FF7EB6", 0));
            c.Add(Avatar("avatar_gumdrop", "Gumdrop", Rarity.Common, 250, "#7AE582", 1));
            c.Add(Avatar("avatar_marsh", "Marsh", Rarity.Common, 350, "#F1F1F1", 2));
            c.Add(Avatar("avatar_berry", "Berry", Rarity.Rare, 700, "#9B5DE5", 0));
            c.Add(Avatar("avatar_choco", "Choco", Rarity.Rare, 900, "#8D5524", 2));
            c.Add(Avatar("avatar_star", "Twinkle", Rarity.Epic, 2000, "#FFD60A", 3, shards: 8));
            c.Add(Premium(Avatar("avatar_king", "King Bloop", Rarity.Premium, 0, "#FF7EB6", 4)));

            // Power-up packs -----------------------------------------------------
            c.PowerUpOffers.Add(new PowerUpOffer { Id = "pu_bomb3", Type = PowerUpType.Bomb, Name = "Jelly Bomb", Description = "Next piece is a bomb that blasts a 3x3 area", Quantity = 3, CoinPrice = 150 });
            c.PowerUpOffers.Add(new PowerUpOffer { Id = "pu_zap3", Type = PowerUpType.LineZap, Name = "Line Zap", Description = "Zaps away the bottom row", Quantity = 3, CoinPrice = 120 });
            c.PowerUpOffers.Add(new PowerUpOffer { Id = "pu_slow3", Type = PowerUpType.SlowMo, Name = "Slow-Mo", Description = "Pieces fall slower for 15 seconds", Quantity = 3, CoinPrice = 100 });
            c.PowerUpOffers.Add(new PowerUpOffer { Id = "pu_swap3", Type = PowerUpType.Swap, Name = "Stick Swap", Description = "Turns the current piece into a long stick", Quantity = 3, CoinPrice = 80 });

            return c;
        }

        private static ShopItem Skin(string id, string name, Rarity rarity, int coins, int shards, BlockStyle style, params string[] palette) =>
            new ShopItem { Id = id, Name = name, Category = ItemCategory.Skin, Rarity = rarity, CoinPrice = coins, ShardPrice = shards, BlockStyle = style, Palette = palette };

        private static ShopItem Theme(string id, string name, Rarity rarity, int coins, string top, string bottom, string accent, MusicStyle music, int bpm, bool grid = false) =>
            new ShopItem { Id = id, Name = name, Category = ItemCategory.Theme, Rarity = rarity, CoinPrice = coins, BackgroundTop = top, BackgroundBottom = bottom, Accent = accent, Music = music, MusicBpm = bpm, Grid = grid };

        private static ShopItem Avatar(string id, string name, Rarity rarity, int coins, string body, int shape, int shards = 0) =>
            new ShopItem { Id = id, Name = name, Category = ItemCategory.Avatar, Rarity = rarity, CoinPrice = coins, ShardPrice = shards, BodyColor = body, AvatarShape = shape };

        private static ShopItem Premium(ShopItem item)
        {
            item.PremiumOnly = true;
            return item;
        }
    }
}
