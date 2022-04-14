using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace RDGameplayPatches
{
    [BepInPlugin("com.rhythmdr.gameplaypatches", "Rhythm Doctor Gameplay Patches", "1.3.3")]
    [BepInProcess("Rhythm Doctor.exe")]
    public class RDGameplayPatches : BaseUnityPlugin
    {
        private static ConfigEntry<VeryHardMode> configVeryHardMode;
        private ConfigEntry<bool> configAccurateReleaseMargins;
        private ConfigEntry<bool> configPersistentP1AndP2Positions;
        private ConfigEntry<bool> configCountOffsetOnRelease;

        private enum VeryHardMode
        {
            None,
            P1,
            P2,
            Both,
        }
        void Awake()
        {
            configVeryHardMode = Config.Bind("Difficulty", "VeryHardMode", VeryHardMode.None,
                "Sets the player(s) in which Very Hard difficulty is enabled. Not affected by the difficulty setting in Rhythm Doctor when enabled.");

            configAccurateReleaseMargins = Config.Bind("Holds", "AccurateRelaseMargins", false,
                "Changes the hold release margins to better reflect the player difficulty, including Very Hard.");

            configCountOffsetOnRelease = Config.Bind("Holds", "CountOffsetOnRelease", true,
                "Shows the millisecond offset and counts the number of offset frames on hold releases.");

            configPersistentP1AndP2Positions = Config.Bind("2P", "PersistentP1AndP2Positions", true,
                "Reverts back to old game behavior and makes P1 and P2 positions persistent between level restarts.");

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

            if (configAccurateReleaseMargins.Value)
            {
                Harmony.CreateAndPatchAll(typeof(AccurateReleaseMargins));
            }

            if (configPersistentP1AndP2Positions.Value)
            {
                Harmony.CreateAndPatchAll(typeof(PersistentP1AndP2Positions));
            }

            if (configCountOffsetOnRelease.Value)
            {
                Harmony.CreateAndPatchAll(typeof(CountOffsetOnRelease));
            }

            Logger.LogInfo("Plugin enabled!");
        }

        void OnDestroy()
        {
            Harmony.UnpatchAll();
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

            [HarmonyPrefix]
            [HarmonyPatch(typeof(RDHitStrip), "SetWidth")]
            public static bool Prefix(RDHitStrip __instance)
            {
                __instance.quad.size = new Vector2(8f, __instance.quad.size.y);
                return false;
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

            [HarmonyPatch(typeof(scrPlayerbox), "releaseOffsetType", MethodType.Getter)]
            public static void Postfix(scrRowEntities ___ent, ref OffsetType __result)
            {
                RDPlayer currentPlayer = ___ent.row.playerProp.GetCurrentPlayer();
                if ((currentPlayer == RDPlayer.P2 ? scnGame.p2DefibMode : scnGame.p1DefibMode) == DefibMode.Unmissable)
                {
                    __result = OffsetType.Perfect;
                }
            }
        }

        public static class CountOffsetOnRelease
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(scrPlayerbox), "SpaceBarReleased")]
            public static bool Prefix(RDPlayer player, bool cpuTriggered, scrPlayerbox __instance, double ___beatReleaseTime, scrRowEntities ___ent, bool ___beatBeingHeld)
            {
                RDPlayer currentPlayer = ___ent.row.playerProp.GetCurrentPlayer();

                if ((player != currentPlayer) || (!___beatBeingHeld && !cpuTriggered))
                {
                    return true;
                }

                double audioPos = __instance.conductor.audioPos;
                float timeOffset = (float) (audioPos - ___beatReleaseTime);
                
                if (GC.showAbsoluteOffsets)
                {
                    int offsetFrames = Mathf.RoundToInt(timeOffset * 60);
                    if (RDC.auto || Mathf.Abs(offsetFrames) <= 1)
                    {
                        offsetFrames = 0;
                    }
                    if (currentPlayer != RDPlayer.CPU)
                    {
                        __instance.game.mistakesManager.AddAbsoluteMistake(currentPlayer, offsetFrames);
                    }
                }

                if (GC.d_showMarginsNumerically)
                {
                    float timeOffsetInMilliseconds = timeOffset * 1000f;
                    string offsetMs = timeOffsetInMilliseconds.ToString("N3");
                    if (timeOffsetInMilliseconds >= 0)
                    {
                        offsetMs = "+" + offsetMs;
                    }
                    HUD.status = "[ " + offsetMs + " " + RDString.Get("editor.unit.ms") + " ]";
                }

                return true;
            }
        }

        public static class PersistentP1AndP2Positions
        {
            [HarmonyPatch(typeof(scnGame), "Start")]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                return new CodeMatcher(instructions)
                    .MatchForward(false,
                        new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(RDInput), "SwapP1AndP2Controls")))
                    .SetOpcodeAndAdvance(OpCodes.Nop)
                    .InstructionEnumeration();
            }
        }
    }
}
