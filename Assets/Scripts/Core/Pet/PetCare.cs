using System;
using JellyDrop.Core.Economy;

namespace JellyDrop.Core.Pet
{
    public enum PetNeed { Food = 0, Energy = 1, Fun = 2, Clean = 3 }

    /// <summary>How the pet feels right now, most urgent first.</summary>
    public enum PetMood { Happy, Okay, Hungry, Tired, Bored, Dirty, Lonely, Sick, Sleeping }

    public enum PetResult { Ok, NotEnoughCoins, Full, AlreadyClean, Sleeping, NotSick, NotSleepy, Unknown }

    /// <summary>Something the pet can eat or play with, bought with coins.</summary>
    public sealed class PetItem
    {
        public string Id;
        public int Coins;
        public float Food, Energy, Fun, Clean;
        public bool IsGift;

        public PetItem(string id, int coins, float food = 0, float energy = 0, float fun = 0, float clean = 0, bool gift = false)
        {
            Id = id; Coins = coins; Food = food; Energy = energy; Fun = fun; Clean = clean; IsGift = gift;
        }
    }

    public static class PetCatalog
    {
        public static readonly PetItem[] Foods =
        {
            new PetItem("apple", 8, food: 18),
            new PetItem("sandwich", 15, food: 32),
            new PetItem("smoothie", 20, food: 18, energy: 15),
            new PetItem("burger", 25, food: 50, clean: -8),
            new PetItem("cake", 35, food: 28, fun: 15, clean: -4),
        };

        public static readonly PetItem[] Gifts =
        {
            new PetItem("ball", 30, fun: 35, energy: -5, gift: true),
            new PetItem("teddy", 60, fun: 70, gift: true),
            new PetItem("rocket", 120, fun: 100, gift: true),
        };

        public const int MedicineCost = 60;

        public static PetItem Get(string id)
        {
            foreach (var f in Foods) if (f.Id == id) return f;
            foreach (var g in Gifts) if (g.Id == id) return g;
            return null;
        }
    }

    /// <summary>
    /// The avatar as a pet. Its needs (food, energy, fun, cleanliness) drop in real time, also while the game
    /// is closed. Ignored for too long it gets sad, then sick; medicine cures it. Feeding, washing, gifts,
    /// stroking, new clothes and playing levels keep it happy. A happy pet gives a small coin bonus.
    /// Everything is cosmetic/in-game: it never blocks playing the game.
    /// </summary>
    public sealed class PetService
    {
        // Points lost per hour while awake / asleep (negative = gained).
        public static readonly float[] AwakeDecay = { 5f, 4f, 5f, 3f };
        public static readonly float[] SleepDecay = { 2.5f, -30f, 0f, 1f };
        public const float SickThreshold = 15f;
        public const long LonelyAfterSeconds = 24 * 3600;
        public const long SickAfterSeconds = 72 * 3600;
        public const float StrokeCap = 70f;
        public const int DressBonusesPerDay = 3;
        public const int HappyCoinBonus = 5;

        private readonly EconomyService _economy;
        private PlayerData Data => _economy.Data;
        private long Now => _economy.Clock.UnixSeconds;
        private bool _ticking;

        public event Action Changed;

        public PetService(EconomyService economy)
        {
            _economy = economy;
            if (Data.petUpdated == 0)
            {
                Data.petFood = Data.petEnergy = Data.petFun = Data.petClean = 80f;
                Data.petUpdated = Now;
                Data.petLastCare = Now;
            }
        }

        // ------------------------------------------------------------------ state

        public float Food { get { Tick(); return Data.petFood; } }
        public float Energy { get { Tick(); return Data.petEnergy; } }
        public float Fun { get { Tick(); return Data.petFun; } }
        public float Clean { get { Tick(); return Data.petClean; } }
        public bool IsSleeping { get { Tick(); return Data.petSleeping; } }
        public bool IsSick { get { Tick(); return Data.petSick; } }
        public bool IsLonely => Now - Data.petLastCare >= LonelyAfterSeconds;

        public float Need(PetNeed need)
        {
            switch (need)
            {
                case PetNeed.Food: return Food;
                case PetNeed.Energy: return Energy;
                case PetNeed.Fun: return Fun;
                default: return Clean;
            }
        }

        /// <summary>Average of all needs, 0..100.</summary>
        public float Wellbeing => (Food + Energy + Fun + Clean) / 4f;

        public PetMood Mood
        {
            get
            {
                Tick();
                if (Data.petSleeping) return PetMood.Sleeping;
                if (Data.petSick) return PetMood.Sick;
                if (IsLonely) return PetMood.Lonely;
                // The lowest need wins if it is low.
                var lowest = PetNeed.Food;
                float min = Data.petFood;
                if (Data.petEnergy < min) { min = Data.petEnergy; lowest = PetNeed.Energy; }
                if (Data.petFun < min) { min = Data.petFun; lowest = PetNeed.Fun; }
                if (Data.petClean < min) { min = Data.petClean; lowest = PetNeed.Clean; }
                if (min < 30f)
                {
                    switch (lowest)
                    {
                        case PetNeed.Food: return PetMood.Hungry;
                        case PetNeed.Energy: return PetMood.Tired;
                        case PetNeed.Fun: return PetMood.Bored;
                        default: return PetMood.Dirty;
                    }
                }
                return Wellbeing >= 65f ? PetMood.Happy : PetMood.Okay;
            }
        }

        /// <summary>Extra coins from levels while the pet is happy.</summary>
        public int CoinBonusPercent => Mood == PetMood.Happy ? HappyCoinBonus : 0;

        /// <summary>True when something needs doing (for a badge on the home screen).</summary>
        public bool NeedsAttention
        {
            get
            {
                var m = Mood;
                return m != PetMood.Happy && m != PetMood.Okay && m != PetMood.Sleeping;
            }
        }

        // ------------------------------------------------------------------ simulation

        /// <summary>Advances the needs to now. Called automatically by every getter and action.</summary>
        public void Tick()
        {
            if (_ticking) return;
            _ticking = true;
            try
            {
                long now = Now;
                long elapsed = now - Data.petUpdated;
                if (elapsed <= 0) return;
                // Simulate in 10-minute steps (capped at two weeks) so waking up and getting sick happen in order.
                long remaining = Math.Min(elapsed, 14L * 24 * 3600);
                long t = Data.petUpdated;
                while (remaining > 0)
                {
                    long step = Math.Min(600, remaining);
                    Step(step / 3600f, t + step);
                    remaining -= step;
                    t += step;
                }
                Data.petUpdated = now;
            }
            finally { _ticking = false; }
        }

        private void Step(float hours, long at)
        {
            var decay = Data.petSleeping ? SleepDecay : AwakeDecay;
            float sickMul = Data.petSick ? 1.5f : 1f;
            Data.petFood = Clamp(Data.petFood - decay[0] * hours * sickMul);
            float energyCap = Data.petSick ? 50f : 100f;
            float energy = Data.petEnergy - decay[1] * hours * (decay[1] > 0 ? sickMul : 1f);
            // Resting can't go past the cap (a sick pet never feels fully rested).
            if (decay[1] < 0) energy = Math.Min(energy, Math.Max(Data.petEnergy, energyCap));
            Data.petEnergy = Clamp(energy);
            Data.petFun = Clamp(Data.petFun - decay[2] * hours * sickMul);
            Data.petClean = Clamp(Data.petClean - decay[3] * hours);

            // Wakes up by itself when rested.
            if (Data.petSleeping && Data.petEnergy >= energyCap - 0.01f) Data.petSleeping = false;

            if (!Data.petSick)
            {
                int empty = 0;
                if (Data.petFood <= 0f) empty++;
                if (Data.petEnergy <= 0f) empty++;
                if (Data.petFun <= 0f) empty++;
                if (Data.petClean <= 0f) empty++;
                float avg = (Data.petFood + Data.petEnergy + Data.petFun + Data.petClean) / 4f;
                if (avg < SickThreshold || empty >= 2 || at - Data.petLastCare >= SickAfterSeconds)
                    Data.petSick = true;
            }
        }

        private static float Clamp(float v) => v < 0f ? 0f : v > 100f ? 100f : v;

        private void Cared()
        {
            Data.petLastCare = Now;
            Data.petCareTotal++;
            _economy.NotifyChanged();
            Changed?.Invoke();
        }

        private void Apply(PetItem item)
        {
            Data.petFood = Clamp(Data.petFood + item.Food);
            Data.petEnergy = Clamp(Data.petEnergy + item.Energy);
            Data.petFun = Clamp(Data.petFun + item.Fun);
            Data.petClean = Clamp(Data.petClean + item.Clean);
        }

        // ------------------------------------------------------------------ actions

        public PetResult Feed(string foodId)
        {
            Tick();
            var item = PetCatalog.Get(foodId);
            if (item == null || item.IsGift) return PetResult.Unknown;
            if (Data.petSleeping) return PetResult.Sleeping;
            if (Data.petFood >= 95f) return PetResult.Full;
            if (!_economy.TrySpendCoins(item.Coins)) return PetResult.NotEnoughCoins;
            Apply(item);
            Cared();
            return PetResult.Ok;
        }

        /// <summary>A present (the "reward"): lots of fun.</summary>
        public PetResult Gift(string giftId)
        {
            Tick();
            var item = PetCatalog.Get(giftId);
            if (item == null || !item.IsGift) return PetResult.Unknown;
            if (Data.petSleeping) return PetResult.Sleeping;
            if (!_economy.TrySpendCoins(item.Coins)) return PetResult.NotEnoughCoins;
            Apply(item);
            Cared();
            return PetResult.Ok;
        }

        /// <summary>Bath time: free, cleans fully and is a little bit of fun.</summary>
        public PetResult Wash()
        {
            Tick();
            if (Data.petSleeping) return PetResult.Sleeping;
            if (Data.petClean >= 95f) return PetResult.AlreadyClean;
            Data.petClean = 100f;
            Data.petFun = Clamp(Data.petFun + 5f);
            Cared();
            return PetResult.Ok;
        }

        /// <summary>Stroking the pet: a little fun, free, up to a cap (so it can't replace playing).</summary>
        public PetResult Stroke()
        {
            Tick();
            if (Data.petSleeping) return PetResult.Sleeping;
            if (Data.petFun < StrokeCap) Data.petFun = Math.Min(StrokeCap, Data.petFun + 4f);
            Cared();
            return PetResult.Ok;
        }

        public PetResult Sleep()
        {
            Tick();
            if (Data.petSleeping) return PetResult.Ok;
            if (Data.petEnergy >= 90f) return PetResult.NotSleepy;
            Data.petSleeping = true;
            Cared();
            return PetResult.Ok;
        }

        public PetResult Wake()
        {
            Tick();
            Data.petSleeping = false;
            _economy.NotifyChanged();
            Changed?.Invoke();
            return PetResult.Ok;
        }

        public PetResult Heal()
        {
            Tick();
            if (!Data.petSick) return PetResult.NotSick;
            if (!_economy.TrySpendCoins(PetCatalog.MedicineCost)) return PetResult.NotEnoughCoins;
            Data.petSick = false;
            Data.petFood = Math.Max(Data.petFood, 40f);
            Data.petEnergy = Math.Max(Data.petEnergy, 40f);
            Data.petFun = Math.Max(Data.petFun, 40f);
            Data.petClean = Math.Max(Data.petClean, 40f);
            Cared();
            return PetResult.Ok;
        }

        /// <summary>New clothes cheer it up (a few times a day).</summary>
        public void OnDressed()
        {
            Tick();
            if (Data.petDressDay != _economy.Today) { Data.petDressDay = _economy.Today; Data.petDressCount = 0; }
            if (Data.petDressCount >= DressBonusesPerDay) return;
            Data.petDressCount++;
            Data.petFun = Clamp(Data.petFun + 8f);
            Cared();
        }

        /// <summary>Playing a level (or a mini game) together: fun, but tiring and makes it hungry.</summary>
        public void OnGamePlayed()
        {
            Tick();
            if (Data.petSleeping) Data.petSleeping = false;
            Data.petFun = Clamp(Data.petFun + 10f);
            Data.petEnergy = Clamp(Data.petEnergy - 5f);
            Data.petFood = Clamp(Data.petFood - 3f);
            Cared();
        }
    }
}
