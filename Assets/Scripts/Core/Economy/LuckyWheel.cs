using System.Collections.Generic;

namespace JellyDrop.Core.Economy
{
    public enum WheelRewardKind { Coins, PowerUp, Shard }

    public sealed class WheelSegment
    {
        public WheelRewardKind Kind;
        public int Amount;
        public int Weight;
        public string Label;
        /// <summary>Hex colour of the slice.</summary>
        public string Color;
    }

    public sealed class WheelResult
    {
        public int SegmentIndex;
        public WheelSegment Segment;
        public PowerUpType PowerUp;
    }

    /// <summary>The free end-of-game lucky wheel. Never sold for real money.</summary>
    public sealed class LuckyWheel
    {
        public readonly List<WheelSegment> Segments;

        public LuckyWheel(List<WheelSegment> segments) { Segments = segments; }

        public static LuckyWheel CreateDefault() => new LuckyWheel(new List<WheelSegment>
        {
            new WheelSegment { Kind = WheelRewardKind.Coins,   Amount = 10,  Weight = 28, Label = "10",     Color = "#FF6F91" },
            new WheelSegment { Kind = WheelRewardKind.Coins,   Amount = 25,  Weight = 24, Label = "25",     Color = "#FFC93C" },
            new WheelSegment { Kind = WheelRewardKind.PowerUp, Amount = 1,   Weight = 12, Label = "POWER",  Color = "#3FB6FF" },
            new WheelSegment { Kind = WheelRewardKind.Coins,   Amount = 50,  Weight = 16, Label = "50",     Color = "#8E7CFF" },
            new WheelSegment { Kind = WheelRewardKind.Shard,   Amount = 1,   Weight = 6,  Label = "SHARD",  Color = "#2ED3A0" },
            new WheelSegment { Kind = WheelRewardKind.Coins,   Amount = 100, Weight = 10, Label = "100",    Color = "#FF9F43" },
            new WheelSegment { Kind = WheelRewardKind.Coins,   Amount = 15,  Weight = 2,  Label = "15",     Color = "#9BDB4D" },
            new WheelSegment { Kind = WheelRewardKind.Coins,   Amount = 500, Weight = 2,  Label = "500!",   Color = "#FF5A6E" },
        });

        public WheelResult Spin(IRandom rng)
        {
            int total = 0;
            foreach (var s in Segments) total += s.Weight;
            int roll = rng.Range(0, total);
            for (int i = 0; i < Segments.Count; i++)
            {
                roll -= Segments[i].Weight;
                if (roll < 0)
                {
                    return new WheelResult
                    {
                        SegmentIndex = i,
                        Segment = Segments[i],
                        PowerUp = (PowerUpType)rng.Range(0, 4)
                    };
                }
            }
            return new WheelResult { SegmentIndex = 0, Segment = Segments[0] };
        }

        /// <summary>Probability of each slice, for showing drop rates transparently in the UI.</summary>
        public double ProbabilityOf(int index)
        {
            int total = 0;
            foreach (var s in Segments) total += s.Weight;
            return (double)Segments[index].Weight / total;
        }
    }
}
