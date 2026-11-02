using JellyDrop.Core;
using JellyDrop.Core.Economy;
using JellyDrop.Visuals;
using UnityEngine;

namespace JellyDrop.Interface
{
    /// <summary>The "mushroom power" switches (extra life in Block Run, spring start in the tower).</summary>
    public static class MushroomUi
    {
        public static CandyButton Toggle(RectTransform parent, Vector2 anchor, Vector2 pos, Vector2 size, System.Action onClick)
        {
            var b = UIKit.Button(parent, "", ColorUtil.Muted, onClick, size, 28, IslandSprites.Mushroom());
            b.GetComponent<RectTransform>().At(anchor, pos, size);
            return b;
        }

        public static void Refresh(CandyButton b, bool on, int mushrooms, string labelKey)
        {
            bool afford = mushrooms >= EconomyService.MushroomBoostCost;
            b.SetLabel(Loc.T(labelKey, EconomyService.MushroomBoostCost) + "  " + Loc.T(on && afford ? "common.on" : "common.off") + "  (" + mushrooms + ")");
            b.SetColor(on && afford ? ColorUtil.Hex("#E0483E") : ColorUtil.Muted);
        }
    }
}
