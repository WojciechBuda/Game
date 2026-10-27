using System;
using System.Collections.Generic;

namespace JellyDrop.Core.Online
{
    /// <summary>A score another player set on a level, used for leaderboards and challenges.</summary>
    [Serializable]
    public sealed class LevelScore
    {
        public string playerId;
        public string playerName;
        public int level;
        public int score;
        public int stars;
    }

    /// <summary>
    /// An asynchronous duel: both players play the same level with the same piece order (LevelDef.Seed),
    /// and the higher score wins the stake.
    /// </summary>
    [Serializable]
    public sealed class Challenge
    {
        public string id;
        public string fromPlayerId;
        public string fromPlayerName;
        public string toPlayerId;
        public int level;
        public int fromScore;
        /// <summary>-1 until the opponent has played.</summary>
        public int toScore = -1;
        public int stakeCoins;
        public long createdUnix;
    }

    /// <summary>
    /// Everything online, behind one interface so the game runs fully offline today and a backend
    /// (Unity Gaming Services, PlayFab, Nakama or your own ASP.NET Core API) can be plugged in later.
    /// The game already plays levels with fixed seeds, so scores from different players are comparable.
    /// </summary>
    public interface IOnlineService
    {
        bool IsSignedIn { get; }
        string PlayerId { get; }
        string PlayerName { get; }

        void SignIn(Action<bool> onDone);

        /// <summary>Uploads the city and progress so they survive a phone change.</summary>
        void SaveProgress(string json, Action<bool> onDone);

        void SubmitLevelScore(LevelScore score, Action<bool> onDone);
        void GetLevelLeaderboard(int level, int count, Action<List<LevelScore>> onDone);

        void SendChallenge(string toPlayerId, int level, int myScore, int stakeCoins, Action<Challenge> onDone);
        void GetChallenges(Action<List<Challenge>> onDone);
        void AnswerChallenge(string challengeId, int myScore, Action<Challenge> onDone);
    }

    /// <summary>Default implementation until a backend exists: everything works offline and lists are empty.</summary>
    public sealed class OfflineService : IOnlineService
    {
        public bool IsSignedIn => false;
        public string PlayerId => "local";
        public string PlayerName => "Player";

        public void SignIn(Action<bool> onDone) => onDone?.Invoke(false);
        public void SaveProgress(string json, Action<bool> onDone) => onDone?.Invoke(false);
        public void SubmitLevelScore(LevelScore score, Action<bool> onDone) => onDone?.Invoke(false);
        public void GetLevelLeaderboard(int level, int count, Action<List<LevelScore>> onDone) => onDone?.Invoke(new List<LevelScore>());
        public void SendChallenge(string toPlayerId, int level, int myScore, int stakeCoins, Action<Challenge> onDone) => onDone?.Invoke(null);
        public void GetChallenges(Action<List<Challenge>> onDone) => onDone?.Invoke(new List<Challenge>());
        public void AnswerChallenge(string challengeId, int myScore, Action<Challenge> onDone) => onDone?.Invoke(null);
    }
}
