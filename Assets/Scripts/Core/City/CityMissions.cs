using System;
using JellyDrop.Core.Economy;

namespace JellyDrop.Core.City
{
    /// <summary>Little random events in the village that the player plays by tapping in the 3D city.</summary>
    public enum CityMissionType
    {
        /// <summary>A runaway chicken darts along the paths: tap it 3 times.</summary>
        Chicken = 0,
        /// <summary>A thief with a sack of coins runs off: tap him 3 times.</summary>
        Thief = 1,
        /// <summary>Balloons float up from the plots: pop 6.</summary>
        Balloons = 2,
        /// <summary>A cat hides next to buildings and keeps moving: find it twice.</summary>
        Cat = 3,
    }

    public sealed class CityMissionReward
    {
        public int Coins, Wood, Brick, Glass;
    }

    /// <summary>
    /// Offers a random village mission every so often (20–45 minutes). An offer waits up to 2 hours; once
    /// started it has a short time limit. Success pays coins (more for bigger cities) and sometimes supplies.
    /// </summary>
    public sealed class CityMissionService
    {
        public const long OfferSeconds = 2 * 3600;
        public const int TypeCount = 4;

        private readonly EconomyService _economy;
        private readonly CityService _city;
        private PlayerData Data => _economy.Data;
        private long Now => _economy.Clock.UnixSeconds;
        private readonly Random _rng;

        public CityMissionService(EconomyService economy, CityService city, int seed = 0)
        {
            _economy = economy;
            _city = city;
            _rng = new Random(seed == 0 ? Environment.TickCount : seed);
            // First mission comes soon for new players.
            if (Data.cityMissionNext == 0) Data.cityMissionNext = Now + 60;
        }

        /// <summary>The waiting offer, or null. New offers appear when their time comes.</summary>
        public CityMissionType? Offer
        {
            get
            {
                if (Data.cityMissionType >= 0 && Now > Data.cityMissionOfferEnds)
                {
                    // Expired: try again a bit later.
                    Data.cityMissionType = -1;
                    Data.cityMissionNext = Now + 15 * 60;
                }
                if (Data.cityMissionType < 0 && Now >= Data.cityMissionNext && _city.Population > 0)
                {
                    Data.cityMissionType = _rng.Next(TypeCount);
                    Data.cityMissionOfferEnds = Now + OfferSeconds;
                    _economy.NotifyChanged();
                }
                return Data.cityMissionType >= 0 ? (CityMissionType?)Data.cityMissionType : null;
            }
        }

        public int MissionsDone => Data.cityMissionsDone;

        public static float TimeLimit(CityMissionType t) =>
            t == CityMissionType.Chicken ? 20f : t == CityMissionType.Thief ? 25f : t == CityMissionType.Balloons ? 20f : 18f;

        public static int TapsNeeded(CityMissionType t) =>
            t == CityMissionType.Balloons ? 6 : t == CityMissionType.Cat ? 2 : 3;

        public CityMissionReward RewardFor(CityMissionType t)
        {
            int level = _city.CityLevel;
            var r = new CityMissionReward { Coins = 40 + 20 * level + (t == CityMissionType.Thief ? 30 : 0) };
            // Every other mission also brings supplies; the thief always drops some.
            if (t == CityMissionType.Thief || Data.cityMissionsDone % 2 == 1)
            {
                r.Wood = 2 + level / 3;
                r.Brick = 1 + level / 4;
                r.Glass = level >= 4 ? 1 + level / 6 : 0;
            }
            return r;
        }

        /// <summary>Ends the current mission. On success pays the reward and returns it; null otherwise.</summary>
        public CityMissionReward Finish(bool success)
        {
            if (Data.cityMissionType < 0) return null;
            var type = (CityMissionType)Data.cityMissionType;
            CityMissionReward reward = null;
            if (success)
            {
                reward = RewardFor(type);
                Data.coins += reward.Coins;
                Data.totalCoinsEarned += reward.Coins;
                Data.wood += reward.Wood;
                Data.brick += reward.Brick;
                Data.glass += reward.Glass;
                Data.cityMissionsDone++;
                Data.cityMissionNext = Now + 20 * 60 + _rng.Next(25 * 60);
            }
            else
            {
                Data.cityMissionNext = Now + 10 * 60;
            }
            Data.cityMissionType = -1;
            _economy.NotifyChanged();
            return reward;
        }
    }
}
