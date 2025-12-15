using System;

namespace UnfairFlipsAPMod.Helpers
{
    // Goal Helper Classs
    public static class ArchipelagoGoalManager
    {
        public static long GetGoalId()
        {
            string goalString = UnfairFlipsAPMod.APClient.GetSlotDataValue("goal");
            Int64.TryParse(goalString, out long goalId);

            return goalId;
        }

        public static void CheckAndCompleteGoal()
        {
            if (!ArchipelagoHelper.IsConnectedAndEnabled) return;

            long goalId = GetGoalId();

            switch (goalId)
            {
                default:
                    Log.Debug($"Unknown goal ID: {goalId}");
                    break;
            }
        }

        private static void CompleteGoal()
        {
            UnfairFlipsAPMod.APClient.GetSession().SetGoalAchieved();
        }
    }
}
