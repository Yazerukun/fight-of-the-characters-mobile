/* === 02. SECURITY, CRYPTO & ANTI-CHEAT UTILITIES === */
using System;
using System.Collections.Generic;
using UnityEngine;
using FOC.UI_UX;

namespace FOC.OnlineMode
{
    public class OnlinePickSynchronizer : MonoBehaviour
    {
        public delegate void OnPlayerPickUpdated(int playerIndex, string heroId, bool isConfirmed);
        public event OnPlayerPickUpdated PlayerPickUpdated;

        private readonly Dictionary<int, PickPacket> _confirmedPicks = new Dictionary<int, PickPacket>();
        private readonly HashSet<string> _lockedHeroIds = new HashSet<string>();

        public bool TrySubmitPick(int playerIndex, string heroId, bool confirm)
        {
            // Defensive anti-race condition guard
            if (_confirmedPicks.TryGetValue(playerIndex, out var existing) && existing.isConfirmed)
            {
                Debug.LogWarning($"[OnlinePickSynchronizer] Player {playerIndex} already locked in!");
                return false;
            }

            if (confirm && _lockedHeroIds.Contains(heroId))
            {
                Debug.LogWarning($"[OnlinePickSynchronizer] Hero {heroId} was already locked by another player!");
                return false;
            }

            var packet = new PickPacket
            {
                playerIndex = playerIndex,
                heroId = heroId,
                isConfirmed = confirm,
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            _confirmedPicks[playerIndex] = packet;
            if (confirm)
            {
                _lockedHeroIds.Add(heroId);
            }

            PlayerPickUpdated?.Invoke(playerIndex, heroId, confirm);
            return true;
        }

        public bool IsHeroLocked(string heroId)
        {
            return _lockedHeroIds.Contains(heroId);
        }
    }
}
