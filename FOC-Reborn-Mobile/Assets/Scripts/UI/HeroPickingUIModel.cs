/* === 01. SYSTEM / ENGINE CONFIG, CONSTANTS & SCHEMA === */
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FOC.UI_UX
{
    public enum PlayerPickState
    {
        Picking,
        LockedIn,
        Disconnected
    }

    [System.Serializable]
    public class PlayerSlotData
    {
        public int playerIndex; // 0 to 9 (e.g. Player1 to Player5 on Team 1)
        public string playerName;
        public string avatarIconPath;
        public PlayerPickState state;
        public string selectedHeroId;
        public bool isLocalPlayer;
        public bool isBot;
    }

    public enum HeroFilterCategory
    {
        All,
        Strength,
        Agility,
        Intelligence
    }

    [System.Serializable]
    public struct PickPacket
    {
        public int playerIndex;
        public string heroId;
        public bool isConfirmed;
        public long timestamp;
    }
}
