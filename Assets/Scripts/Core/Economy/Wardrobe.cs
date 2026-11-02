using System.Collections.Generic;

namespace JellyDrop.Core.Economy
{
    /// <summary>What a wardrobe item changes on the avatar.</summary>
    public enum WardrobeSlot { Color = 0, Hat = 1, Glasses = 2 }

    public sealed class WardrobeItem
    {
        public string Id;
        public WardrobeSlot Slot;
        public int CoinPrice;
        /// <summary>Body colour for colour items (hex), main colour for hats and glasses.</summary>
        public string Color;
        /// <summary>Which drawing to use for hats and glasses (see SpriteFactory).</summary>
        public string Style;
    }

    /// <summary>How the player's avatar is dressed. Empty fields mean "none" / the avatar's own colour.</summary>
    public struct AvatarLook
    {
        public string BodyColor;
        public string Hat;
        public string HatColor;
        public string Glasses;
        public string GlassesColor;

        public static readonly AvatarLook None = new AvatarLook();

        public bool IsEmpty => string.IsNullOrEmpty(BodyColor) && string.IsNullOrEmpty(Hat) && string.IsNullOrEmpty(Glasses);

        /// <summary>Short stable key for sprite caching.</summary>
        public string Key => $"{BodyColor}|{Hat}{HatColor}|{Glasses}{GlassesColor}";
    }

    /// <summary>Avatar customisation bought with coins: body colours, hats and glasses.</summary>
    public static class Wardrobe
    {
        public static readonly List<WardrobeItem> All = new List<WardrobeItem>
        {
            // Body colours
            new WardrobeItem { Id = "color_coral", Slot = WardrobeSlot.Color, CoinPrice = 150, Color = "#FF8A5C" },
            new WardrobeItem { Id = "color_mint", Slot = WardrobeSlot.Color, CoinPrice = 150, Color = "#3DDBA8" },
            new WardrobeItem { Id = "color_sky", Slot = WardrobeSlot.Color, CoinPrice = 150, Color = "#4FB8FF" },
            new WardrobeItem { Id = "color_lilac", Slot = WardrobeSlot.Color, CoinPrice = 200, Color = "#B48CFF" },
            new WardrobeItem { Id = "color_lemon", Slot = WardrobeSlot.Color, CoinPrice = 200, Color = "#FFD84D" },
            new WardrobeItem { Id = "color_midnight", Slot = WardrobeSlot.Color, CoinPrice = 500, Color = "#3B3F7A" },
            new WardrobeItem { Id = "color_gold", Slot = WardrobeSlot.Color, CoinPrice = 900, Color = "#FFC93C" },

            // Hats
            new WardrobeItem { Id = "hat_cap", Slot = WardrobeSlot.Hat, CoinPrice = 300, Color = "#FF5A6E", Style = "cap" },
            new WardrobeItem { Id = "hat_beanie", Slot = WardrobeSlot.Hat, CoinPrice = 300, Color = "#3FB6FF", Style = "beanie" },
            new WardrobeItem { Id = "hat_party", Slot = WardrobeSlot.Hat, CoinPrice = 250, Color = "#B48CFF", Style = "party" },
            new WardrobeItem { Id = "hat_headphones", Slot = WardrobeSlot.Hat, CoinPrice = 550, Color = "#2B2F5E", Style = "headphones" },
            new WardrobeItem { Id = "hat_cowboy", Slot = WardrobeSlot.Hat, CoinPrice = 700, Color = "#C98552", Style = "cowboy" },
            new WardrobeItem { Id = "hat_crown", Slot = WardrobeSlot.Hat, CoinPrice = 1500, Color = "#FFC93C", Style = "crown" },

            // Glasses
            new WardrobeItem { Id = "glasses_sun", Slot = WardrobeSlot.Glasses, CoinPrice = 400, Color = "#1F1A33", Style = "sun" },
            new WardrobeItem { Id = "glasses_round", Slot = WardrobeSlot.Glasses, CoinPrice = 250, Color = "#2A1440", Style = "round" },
            new WardrobeItem { Id = "glasses_star", Slot = WardrobeSlot.Glasses, CoinPrice = 600, Color = "#FF6F91", Style = "star" },
            new WardrobeItem { Id = "glasses_visor", Slot = WardrobeSlot.Glasses, CoinPrice = 800, Color = "#3DE0FF", Style = "visor" },
        };

        public static WardrobeItem Get(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            foreach (var w in All) if (w.Id == id) return w;
            return null;
        }

        public static IEnumerable<WardrobeItem> InSlot(WardrobeSlot slot)
        {
            foreach (var w in All) if (w.Slot == slot) yield return w;
        }
    }
}
