using System.Collections.Generic;

namespace JellyDrop.Core.Economy
{
    public enum WheelRewardKind
    {
        Coins, PowerUp, Shard,
        /// <summary>Wood, bricks and glass for the city.</summary>
        Supplies,
        /// <summary>A Block Run bonus ticket (that run pays double).</summary>
        RunTicket,
        /// <summary>Energy for the pet (paid out by the app, the pet lives outside the economy).</summary>
        PetEnergy,
        /// <summary>Coins x2 for the next few games.</summary>
        CoinBooster,
        /// <summary>The big prize.</summary>
        Jackpot,
    }

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

    /// <summary>
    /// The lucky wheel: a free spin after every game and once a day from the home screen, plus a few spins a day
    /// for coins (earned in the game - coins are never sold for real money). Every slice wins something and the
    /// exact chances are shown in the game.
    /// </summary>
    public sealed class LuckyWheel
    {
        public readonly List<WheelSegment> Segments;

        public LuckyWheel(List<WheelSegment> segments) { Segments = segments; }

        public static LuckyWheel CreateDefault() => new LuckyWheel(new List<WheelSegment>
        {
            new WheelSegment { Kind = WheelRewardKind.Coins,       Amount = 25,   Weight = 20, Label = "25",   Color = "#FF6F91" },
            new WheelSegment { Kind = WheelRewardKind.PowerUp,     Amount = 1,    Weight = 12, Label = "x1",   Color = "#3FB6FF" },
            new WheelSegment { Kind = WheelRewardKind.Coins,       Amount = 50,   Weight = 15, Label = "50",   Color = "#FFC93C" },
            new WheelSegment { Kind = WheelRewardKind.Supplies,    Amount = 3,    Weight = 10, Label = "x3",   Color = "#C98552" },
            new WheelSegment { Kind = WheelRewardKind.Coins,       Amount = 100,  Weight = 9,  Label = "100",  Color = "#8E7CFF" },
            new WheelSegment { Kind = WheelRewardKind.RunTicket,   Amount = 1,    Weight = 6,  Label = "x1",   Color = "#FF4FA3" },
            new WheelSegment { Kind = WheelRewardKind.Shard,       Amount = 1,    Weight = 5,  Label = "x1",   Color = "#2ED3A0" },
            new WheelSegment { Kind = WheelRewardKind.PetEnergy,   Amount = 40,   Weight = 6,  Label = "+40",  Color = "#9BDB4D" },
            new WheelSegment { Kind = WheelRewardKind.Coins,       Amount = 250,  Weight = 4,  Label = "250",  Color = "#FF9F43" },
            new WheelSegment { Kind = WheelRewardKind.CoinBooster, Amount = 3,    Weight = 5,  Label = "x2",   Color = "#3DE0FF" },
            new WheelSegment { Kind = WheelRewardKind.PowerUp,     Amount = 2,    Weight = 3,  Label = "x2",   Color = "#5A8CFF" },
            new WheelSegment { Kind = WheelRewardKind.Jackpot,     Amount = 1000, Weight = 2,  Label = "1000", Color = "#FF5A6E" },
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
