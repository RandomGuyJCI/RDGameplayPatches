using System;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace RDVeryHard
{
    [BepInPlugin("com.rhythmdr.veryhard", "Very Hard Difficulty for Rhythm Doctor", "1.1.0")]
    [BepInProcess("Rhythm Doctor.exe")]
    public class RDVeryHard : BaseUnityPlugin
    {
        private static ConfigEntry<VeryHardMode> configVeryHardMode;

        private enum VeryHardMode
        {
            None,
            P1,
            P2,
            Both,
        }
        void Awake()
        {
            configVeryHardMode = Config.Bind("General", "VeryHardMode", VeryHardMode.Both,
                "Sets the player(s) in which Very Hard difficulty is enabled. Not affected by the difficulty setting in Rhythm Doctor when enabled.");

            switch (configVeryHardMode.Value)
            {
                case VeryHardMode.P1:
                case VeryHardMode.P2:
                case VeryHardMode.Both:
                    Harmony.CreateAndPatchAll(typeof(VeryHard));
                    break;
                case VeryHardMode.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Logger.LogMessage("Plugin enabled!");
        }

        public static class VeryHard
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(scnGame), "GetHitMargin")]
            public static void Postfix(RDPlayer player, ref float __result)
            {
                if ((configVeryHardMode.Value == VeryHardMode.Both) ||
                    (player == RDPlayer.P1 && configVeryHardMode.Value == VeryHardMode.P1) ||
                    (player == RDPlayer.P2 && configVeryHardMode.Value == VeryHardMode.P2))
                {
                    __result = 0.025f;
                }
            }
        }
    }
}
