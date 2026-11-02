#if PLAY_GAMES && UNITY_ANDROID
using System;
using System.Collections.Generic;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using JellyDrop.Core.Online;
using UnityEngine;

namespace JellyDrop.Online
{
    /// <summary>
    /// Sign-in with the player's Google account through Google Play Games Services (Play Games plugin
    /// for Unity, v11+), plus a leaderboard for the highest level reached. Compiled only with the PLAY_GAMES
    /// define on Android (menu Stackville > Online: enable Google Play Games). Challenges still use the
    /// offline fallback until a game backend exists.
    /// </summary>
    public sealed class PlayGamesService : IOnlineService
    {
        private readonly OfflineService _fallback = new OfflineService();

        public bool IsSignedIn { get; private set; }
        public string PlayerId { get; private set; } = "local";
        public string PlayerName { get; private set; } = "Player";
        public bool CanSignIn => true;

        public PlayGamesService()
        {
            PlayGamesPlatform.Activate();
        }

        public void SignIn(Action<bool> onDone) =>
            PlayGamesPlatform.Instance.Authenticate(status => Finish(status, onDone));

        public void SignInWithUi(Action<bool> onDone) =>
            PlayGamesPlatform.Instance.ManuallyAuthenticate(status => Finish(status, onDone));

        private void Finish(SignInStatus status, Action<bool> onDone)
        {
            IsSignedIn = status == SignInStatus.Success;
            if (IsSignedIn)
            {
                PlayerId = PlayGamesPlatform.Instance.GetUserId();
                PlayerName = PlayGamesPlatform.Instance.GetUserDisplayName();
            }
            else
            {
                Debug.Log("[PlayGames] Sign-in: " + status);
            }
            onDone?.Invoke(IsSignedIn);
        }

        public void SaveProgress(string json, Action<bool> onDone) => _fallback.SaveProgress(json, onDone);

        public void SubmitLevelScore(LevelScore score, Action<bool> onDone)
        {
            if (!IsSignedIn) { onDone?.Invoke(false); return; }
            // One leaderboard: the highest level on the path.
            PlayGamesPlatform.Instance.ReportScore(score.level, OnlineIds.HighestLevelLeaderboard, ok => onDone?.Invoke(ok));
        }

        public void GetLevelLeaderboard(int level, int count, Action<List<LevelScore>> onDone) => _fallback.GetLevelLeaderboard(level, count, onDone);
        public void SendChallenge(string toPlayerId, int level, int myScore, int stakeCoins, Action<Challenge> onDone) => _fallback.SendChallenge(toPlayerId, level, myScore, stakeCoins, onDone);
        public void GetChallenges(Action<List<Challenge>> onDone) => _fallback.GetChallenges(onDone);
        public void AnswerChallenge(string challengeId, int myScore, Action<Challenge> onDone) => _fallback.AnswerChallenge(challengeId, myScore, onDone);
    }
}
#endif
