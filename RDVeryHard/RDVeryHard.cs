using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace RDVeryHard
{
    [BepInPlugin("com.rhythmdr.veryhard", "Very Hard Difficulty for Rhythm Doctor", "1.0.0")]
    [BepInProcess("Rhythm Doctor.exe")]
    public class RDVeryHard : BaseUnityPlugin
    {
        private ConfigEntry<bool> configEnableVeryHard;
        void Awake()
        {
            configEnableVeryHard = Config.Bind("General", "EnableVeryHard", true,
                "Enables Very Hard difficulty. Not affected by the difficulty setting in Rhythm Doctor when enabled.");

            if (configEnableVeryHard.Value)
            {
                Harmony.CreateAndPatchAll(typeof(EnableVeryHard));
            }

            Logger.LogMessage("Plugin enabled!");
        }
    }

    public static class EnableVeryHard
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(scnGame), "GetHitMargin")]
        public static bool Prefix(ref float __result)
        {
            __result = 0.025f;
            return false;
        }
    }
}
