using System;

namespace UnfairFlipsAPMod.Helpers
{
    // General Helper Class
    public static class ArchipelagoHelper
    {
        private static bool IsTrue(string str)
        {
            return str == "true" || str == "1";
        }

        public static bool IsConnectedAndEnabled =>
            UnfairFlipsAPMod.APClient?.IsConnected ?? false;

    }
}
