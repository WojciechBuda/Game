using JellyDrop.Core;
using JellyDrop.Core.City;
using JellyDrop.Visuals;
using UnityEngine;
using UnityEngine.UI;

namespace JellyDrop.Interface
{
    /// <summary>Row of pills showing coins and the three building supplies.</summary>
    public sealed class ResourceBar : MonoBehaviour
    {
        private Text _coins, _wood, _brick, _glass;

        public static ResourceBar Create(Transform parent, Vector2 anchor, Vector2 position)
        {
            var rt = UIKit.Node("Resources", parent).At(anchor, position, new Vector2(800, 96));
            var bar = rt.gameObject.AddComponent<ResourceBar>();
            bar.Build(rt);
            return bar;
        }

        private void Build(RectTransform rt)
        {
            _coins = Pill(rt, SpriteFactory.Coin(), 0f, 250f);
            _wood = Pill(rt, CitySprites.SupplyIcon(Supply.Wood), 262f, 172f);
            _brick = Pill(rt, CitySprites.SupplyIcon(Supply.Brick), 446f, 172f);
            _glass = Pill(rt, CitySprites.SupplyIcon(Supply.Glass), 630f, 172f);
        }

        private static Text Pill(RectTransform parent, Sprite icon, float x, float width)
        {
            var t = UIKit.CurrencyPill(parent, icon, ColorUtil.Ink, out var pill, width);
            pill.anchorMin = pill.anchorMax = new Vector2(0f, 0.5f);
            pill.pivot = new Vector2(0f, 0.5f);
            pill.anchoredPosition = new Vector2(x, 0f);
            t.Fit(22);
            return t;
        }

        public void Refresh(App app)
        {
            _coins.text = Loc.Number(app.Economy.Coins);
            _wood.text = Loc.Number(app.City.Amount(Supply.Wood));
            _brick.text = Loc.Number(app.City.Amount(Supply.Brick));
            _glass.text = Loc.Number(app.City.Amount(Supply.Glass));
        }
    }

    /// <summary>Three stars, filled up to a count. Used on the level path, level intro and results.</summary>
    public sealed class StarRow : MonoBehaviour
    {
        private readonly Image[] _stars = new Image[3];

        public static StarRow Create(Transform parent, float starSize, float spacing)
        {
            var rt = UIKit.Node("Stars", parent);
            rt.sizeDelta = new Vector2(starSize * 3 + spacing * 2, starSize);
            var row = rt.gameObject.AddComponent<StarRow>();
            for (int i = 0; i < 3; i++)
            {
                var img = UIKit.Image(rt, SpriteFactory.Star(), Color.white, false, "Star");
                // Middle star sits a little higher, like most level maps.
                img.rectTransform.At(UIKit.Center, new Vector2((i - 1) * (starSize + spacing), i == 1 ? starSize * 0.18f : 0f), new Vector2(starSize, starSize));
                row._stars[i] = img;
            }
            row.Set(0);
            return row;
        }

        public void Set(int count)
        {
            for (int i = 0; i < 3; i++)
                _stars[i].color = i < count ? ColorUtil.Gold : new Color(0.17f, 0.18f, 0.36f, 0.15f);
        }

        /// <summary>Fills the stars one by one with a pop (results screen).</summary>
        public void Animate(int count, System.Action<int> onStar = null)
        {
            Set(0);
            for (int i = 0; i < count; i++)
            {
                int index = i;
                JellyDrop.Util.Tweener.Delay(0.35f + i * 0.35f, () =>
                {
                    if (this == null) return;
                    _stars[index].color = ColorUtil.Gold;
                    UIKit.Pop(_stars[index].transform, 0.2f, 0.4f);
                    onStar?.Invoke(index);
                }, this);
            }
        }
    }
}
