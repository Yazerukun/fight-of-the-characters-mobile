/* === 06. CORE GAMEPLAY, BUSINESS & TRANSACTION LOGIC === */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FOC.HeroSystems;
using FOC.UI_UX;

namespace FOC.OfflineMode
{
    public class OfflinePickController : MonoBehaviour
    {
        [Header("Bot Pick Timings")]
        [SerializeField] private float minBotReactionDelay = 2.0f;
        [SerializeField] private float maxBotReactionDelay = 6.0f;

        private HashSet<string> _claimedHeroIds = new HashSet<string>();

        public void RegisterClaimedHero(string heroId)
        {
            if (!string.IsNullOrEmpty(heroId))
            {
                _claimedHeroIds.Add(heroId);
            }
        }

        public bool IsHeroAvailable(string heroId)
        {
            return !_claimedHeroIds.Contains(heroId);
        }

        public HeroData SelectBotHero(List<HeroData> availablePool)
        {
            var validChoices = new List<HeroData>();
            for (int i = 0; i < availablePool.Count; i++)
            {
                if (IsHeroAvailable(availablePool[i].heroId))
                {
                    validChoices.Add(availablePool[i]);
                }
            }

            if (validChoices.Count > 0)
            {
                int idx = Random.Range(0, validChoices.Count);
                var picked = validChoices[idx];
                RegisterClaimedHero(picked.heroId);
                return picked;
            }

            return null;
        }
    }
}
