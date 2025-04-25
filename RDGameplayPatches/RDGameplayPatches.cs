using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace RDGameplayPatches
{
    [BepInPlugin("com.rhythmdr.gameplaypatches", "Rhythm Doctor Gameplay Patches", "1.14.0")]
    [BepInProcess("Rhythm Doctor.exe")]
    public class RDGameplayPatches : BaseUnityPlugin
    {
        private static ConfigEntry<VeryHardMode> configVeryHardMode;
        private static ConfigEntry<bool> configAccurateReleaseMargins;
        private static ConfigEntry<bool> configCountOffsetOnRelease;
        private static ConfigEntry<bool> configLegacyHitJudgment;
        private static ConfigEntry<float> configStatusSignTransparency;

        private enum VeryHardMode { None, P1, P2, Both }

        private void Awake()
        {
            configVeryHardMode = Config.Bind("Hits", "VeryHardMode", VeryHardMode.None,
                "Sets the player(s) in which Very Hard difficulty is enabled. Not affected by the difficulty setting in Rhythm Doctor when enabled.");

            configAccurateReleaseMargins = Config.Bind("Holds", "AccurateReleaseMargins", false,
                "Changes the hold release margins to better reflect the player difficulty, including Very Hard.");

            configCountOffsetOnRelease = Config.Bind("Holds", "CountOffsetOnRelease", true,
                "Shows the millisecond offset and counts the number of offset frames on hold releases.");
            
            configLegacyHitJudgment = Config.Bind("HUD", "LegacyHitJudgment", false,
                "Reverts back to old behavior and rounds the ms offset in the hit judgment sign to 3 decimal points.");

            configStatusSignTransparency = Config.Bind("HUD", "StatusSignTransparency", 1.0f,
                new ConfigDescription("Sets the transparency of the status sign.", new AcceptableValueRange<float>(0f, 1f)));

            if (configVeryHardMode.Value != VeryHardMode.None)
                Harmony.CreateAndPatchAll(typeof(VeryHard));

            if (configAccurateReleaseMargins.Value)
                Harmony.CreateAndPatchAll(typeof(AccurateReleaseMargins));

            if (configCountOffsetOnRelease.Value)
                Harmony.CreateAndPatchAll(typeof(CountOffsetOnRelease));

            if (configLegacyHitJudgment.Value)
                Harmony.CreateAndPatchAll(typeof(LegacyHitJudgment));

            if (configStatusSignTransparency.Value != 1f)
                Harmony.CreateAndPatchAll(typeof(TransparentStatusSign));

            Logger.LogInfo("Plugin enabled!");
        }

        private void OnDestroy()
        {
            Harmony.UnpatchAll();
        }

        public static class VeryHard
        {
            private static readonly bool isP1VeryHard = configVeryHardMode.Value is VeryHardMode.P1 or VeryHardMode.Both;
            private static readonly bool isP2VeryHard = configVeryHardMode.Value is VeryHardMode.P2 or VeryHardMode.Both;

            // Change player hit margins to 25 ms (threshold before a 2 frame offset)
            [HarmonyPostfix]
            [HarmonyPatch(typeof(scnGame), "GetHitMargin")]
            public static void Postfix(RDPlayer player, ref float __result)
            {
                if (player == RDPlayer.P1 ? isP1VeryHard : isP2VeryHard)
                    __result = 0.025f;
            }

            // Make the hit strip width thinner
            [HarmonyPatch(typeof(RDHitStrip), "Setup")]
            public static void Postfix(RDPlayer player, RDHitStrip __instance)
            {
                if (player == RDPlayer.P1 ? isP1VeryHard : isP2VeryHard)
                    __instance.width = 8f;
            }

            // Force a hard button for each Very Hard player
            [HarmonyPatch(typeof(scnGame), "Awake")]
            public static void Postfix()
            {
                if (isP1VeryHard) scnGame.p1DefibMode = DefibMode.Hard;
                if (isP2VeryHard) scnGame.p2DefibMode = DefibMode.Hard;
            }

            // Prevent the difficulty setting from changing the Very Hard player's button or hit strip width 
            [HarmonyTranspiler]
            [HarmonyPatch(typeof(PauseModeContentArrows), "ChangeContentValue")]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
            {
                var codeMatcher = new CodeMatcher(instructions, il);

                codeMatcher
                    .MatchForward(false,
                        new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(PauseMenuMode), "CheckCJKText")))
                    .CreateLabelAt(codeMatcher.Pos - 2, out var breakLabel)
                    .MatchBack(false,
                        new CodeMatch(ci => ci.LoadsConstant(PauseContentName.DefibrillatorP1)))
                    .Advance(3);

                if (isP1VeryHard) 
                    codeMatcher.InsertAndAdvance(
                        new CodeInstruction(OpCodes.Ldloc_S, 6),
                        new CodeInstruction(OpCodes.Brtrue_S, breakLabel));

                if (isP2VeryHard)
                    codeMatcher.InsertAndAdvance(
                        new CodeInstruction(OpCodes.Ldloc_S, 6),
                        new CodeInstruction(OpCodes.Brfalse_S, breakLabel));

                return codeMatcher.InstructionEnumeration();
            }
        }

        public static class AccurateReleaseMargins
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(scnGame), "GetReleaseMargin")]
            public static void Postfix(RDPlayer player, ref float __result)
            {
                __result = scnGame.GetHitMargin(player);
            }
        }

        public static class CountOffsetOnRelease
        {
            // Copied over from scrPlayerBox.Pulse()
            [HarmonyPrefix]
            [HarmonyPatch(typeof(scrPlayerbox), "SpaceBarReleased")]
            public static bool Prefix(RDPlayer player, bool cpuTriggered, scrPlayerbox __instance)
            {
                if (player != __instance.player || (!__instance.currentHoldBeat && !cpuTriggered)) return true;

                var audioPos = __instance.conductor.audioPos;
                var timeOffset = (float)(audioPos - __instance.currentHoldBeat.releaseTime);

                if (GC.showAbsoluteOffsets)
                {
                    var offsetFrames = Mathf.RoundToInt(timeOffset * 60);

                    if (RDBase.debugSettings.Auto || Mathf.Abs(offsetFrames) <= 1)
                        offsetFrames = 0;

                    if (__instance.player != RDPlayer.CPU)
                        __instance.game.mistakesManager.AddAbsoluteMistake(__instance.player, offsetFrames);
                }

                if (GC.d_showMarginsNumerically && !cpuTriggered)
                {
                    var timeOffsetInMilliseconds = timeOffset * 1000f;
                    var offsetMs = configLegacyHitJudgment.Value ? timeOffsetInMilliseconds.ToString("N3") : ((int) timeOffsetInMilliseconds).ToString();

                    if (timeOffsetInMilliseconds >= 0)
                        offsetMs = "+" + offsetMs;

                    HUD.status = "[ " + offsetMs + " " + RDString.Get("editor.unit.ms") + " ]";
                }

                return true;
            }
            
            [HarmonyTranspiler]
            [HarmonyPatch(typeof(scrPlayerbox), "SpaceBarReleased")]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
            {
                return new CodeMatcher(instructions, il)
                    .End()
                    .MatchBack(false, new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(HUD), "set_status")))
                    .Advance(1)
                    .CreateLabel(out var skipLabel)
                    .MatchBack(false, new CodeMatch(OpCodes.Ldc_I4_5))
                    .Insert(new CodeInstruction(OpCodes.Br, skipLabel))
                    .MatchBack(false, new CodeMatch(OpCodes.Ble))
                    .SetOperandAndAdvance(skipLabel)
                    .InstructionEnumeration();
            }
        }

        public static class LegacyHitJudgment
        {
            public static float msOffset;

            [HarmonyTranspiler]
            [HarmonyPatch(typeof(scrPlayerbox), "Pulse")]
            [HarmonyPatch(typeof(scrPlayerbox), "SpaceBarReleased")]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                return new CodeMatcher(instructions)
                    .End()
                    .MatchBack(false, new CodeMatch(OpCodes.Conv_I4))
                    // Skips converting the ms offset float to an int and stores it in msOffset instead
                    .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Stsfld, AccessTools.Field(typeof(LegacyHitJudgment), nameof(msOffset))))
                    .RemoveInstruction()
                    // Rounds to the nearest 3 decimals
                    .SetAndAdvance(OpCodes.Ldsflda, AccessTools.Field(typeof(LegacyHitJudgment), nameof(msOffset)))
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Ldstr, "N3"))
                    .SetAndAdvance(OpCodes.Call, AccessTools.Method(typeof(float), "ToString", [typeof(string)]))
                    // Fixes an edge case when the ms offset is in the -0.xxx range
                    .Advance(1)
                    .SetAndAdvance(OpCodes.Ldsfld, AccessTools.Field(typeof(LegacyHitJudgment), nameof(msOffset)))
                    .SetAndAdvance(OpCodes.Ldc_R4, 0f)
                    .SetOpcodeAndAdvance(OpCodes.Blt_Un)
                    .InstructionEnumeration();
            }
        }

        public class TransparentStatusSign
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(LEDSign), "Awake")]
            public static void AwakePostfix(LEDSign __instance)
            {
                var images = __instance.gameObject.GetComponentsInChildren<Image>(true);
                
                foreach (var image in images)
                    image.color = new Color(1f, 1f, 1f, 1f);
            }

            // Only change the transparency of the status sign while in a level (after pressing to start)
            [HarmonyPostfix]
            [HarmonyPatch(typeof(scnGame), "StartTheGame")]
            public static IEnumerator Wrapper(IEnumerator result, scnGame __instance)
            {
                while (result.MoveNext())
                    yield return result.Current;
                
                var images = __instance.hud.statusText.gameObject.GetComponentsInChildren<Image>(true);
                
                foreach (var image in images)
                    image.color = new Color(1f, 1f, 1f, configStatusSignTransparency.Value);
            }
        }
    }
}