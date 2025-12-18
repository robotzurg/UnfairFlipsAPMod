using HarmonyLib;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace UnfairFlipsAPMod
{
    public static class ArchipelagoConstants
    {
        // ========== DATA CONSTANTS =======================================
        public const float MaxHeadsChance = 0.9f;
        
        public const float MinFlipTime = 0.2f;
        public const float MaxFlipTime = 5f;
        
        public const float MaxComboMultiplier = 5;
        public const float MinComboMultiplier = 2;

        // ========== LOCATION CONSTANTS (what you check in-game) ==========
        // These are the location IDs that get sent when you accomplish something

        // ========== HELPER METHODS ==========

    }
}