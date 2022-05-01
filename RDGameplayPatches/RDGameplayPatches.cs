using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Emit;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace RDGameplayPatches
{
    [BepInPlugin("com.rhythmdr.gameplaypatches", "Rhythm Doctor Gameplay Patches", "1.9.0")]
    [BepInProcess("Rhythm Doctor.exe")]
    public class RDGameplayPatches : BaseUnityPlugin
    {
        private const string assetsPath = "BepInEx/plugins/RDGameplayPatches/Assets/";

        private static ConfigEntry<VeryHardMode> configVeryHardMode;
        private static ConfigEntry<bool> configAccurateReleaseMargins;
        private static ConfigEntry<bool> configCountOffsetOnRelease;
        private static ConfigEntry<bool> configAntiCheeseHolds;
        private static ConfigEntry<bool> configFixAutoHitMisses;
        private static ConfigEntry<bool> configFixHoldPseudos;
        private static ConfigEntry<bool> configRankColorOnSpeedChange;
        private static ConfigEntry<bool> configChangeRankButtonPerDifficulty;
        private static ConfigEntry<bool> configPlayerOnlyMsOffset;

        private enum VeryHardMode
        {
            None,
            P1,
            P2,
            Both
        }

        private void Awake()
        {
            configVeryHardMode = Config.Bind("Difficulty", "VeryHardMode", VeryHardMode.None,
                "Sets the player(s) in which Very Hard difficulty is enabled. Not affected by the difficulty setting in Rhythm Doctor when enabled.");

            configAccurateReleaseMargins = Config.Bind("Holds", "AccurateReleaseMargins", false,
                "Changes the hold release margins to better reflect the player difficulty, including Very Hard.");

            configCountOffsetOnRelease = Config.Bind("Holds", "CountOffsetOnRelease", true,
                "Shows the millisecond offset and counts the number of offset frames on hold releases.");

            configAntiCheeseHolds = Config.Bind("Holds", "AntiCheeseHolds", true,
                "Prevents you from cheesing levels by abusing a hold's auto-hits.");

            configFixAutoHitMisses = Config.Bind("Holds", "FixAutoHitMisses", true,
                "Fixes hold auto-hits from sometimes missing.");

            configFixHoldPseudos = Config.Bind("Holds", "FixHoldPseudos", true,
                "Always auto-hits beats that happen at the end of a hold, fixing the hold pseudo-hit issue. Recommended with FixAutoHitMisses enabled.");

            configRankColorOnSpeedChange = Config.Bind("HUD", "RankColorOnSpeedChange", true,
                "Changes the color of the rank text depending on the level's speed (blue on chill speed, red on chili speed).");

            configChangeRankButtonPerDifficulty = Config.Bind("HUD", "ChangeRankButtonPerDifficulty", true,
                "Changes the player's button in the rank screen depending on the difficulty.");

            configPlayerOnlyMsOffset = Config.Bind("HUD", "PlayerOnlyMsOffset", false,
                "Changes the status sign behavior to only show player hit offsets when Numerical Hit Judgement is enabled.");

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
                Harmony.CreateAndPatchAll(typeof(AccurateReleaseMargins));

            if (configAntiCheeseHolds.Value ||
                configFixAutoHitMisses.Value ||
                configFixHoldPseudos.Value)
                Harmony.CreateAndPatchAll(typeof(HoldAutoHitPatch));

            if (configCountOffsetOnRelease.Value)
                Harmony.CreateAndPatchAll(typeof(CountOffsetOnRelease));

            if (configRankColorOnSpeedChange.Value)
                Harmony.CreateAndPatchAll(typeof(RankColorOnSpeedChange));

            if (configChangeRankButtonPerDifficulty.Value)
                Harmony.CreateAndPatchAll(typeof(ChangeRankButtonPerDifficulty));

            if (configPlayerOnlyMsOffset.Value) 
                Harmony.CreateAndPatchAll(typeof(PlayerOnlyMsOffset));

            Logger.LogInfo("Plugin enabled!");
        }

        private void OnDestroy()
        {
            Harmony.UnpatchAll();
        }
        
        public static class VeryHard
        {
            private static readonly bool isP1VeryHard = configVeryHardMode.Value == VeryHardMode.P1 ||
                                                        configVeryHardMode.Value == VeryHardMode.Both;

            private static readonly bool isP2VeryHard = configVeryHardMode.Value == VeryHardMode.P2 ||
                                                        configVeryHardMode.Value == VeryHardMode.Both;

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
                    __instance.quad.size = new Vector2(8f, __instance.quad.size.y);
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
                Label breakLabel;
                var codeMatcher = new CodeMatcher(instructions, il);

                codeMatcher
                    .MatchForward(false,
                        new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(PauseMenuMode), "CheckCJKText")))
                    .CreateLabelAt(codeMatcher.Pos - 2, out breakLabel)
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

            // Never make unmissable releases miss
            [HarmonyPatch(typeof(scrPlayerbox), "releaseOffsetType", MethodType.Getter)]
            public static void Postfix(scrRowEntities ___ent, ref OffsetType __result)
            {
                var player = ___ent.row.playerProp.GetCurrentPlayer();
                if ((player == RDPlayer.P1 ? scnGame.p1DefibMode : scnGame.p2DefibMode) == DefibMode.Unmissable)
                    __result = OffsetType.Perfect;
            }
        }

        public static class CountOffsetOnRelease
        {
            // Copied over from scrPlayerBox.Pulse()
            [HarmonyPrefix]
            [HarmonyPatch(typeof(scrPlayerbox), "SpaceBarReleased")]
            public static bool Prefix(RDPlayer player, bool cpuTriggered, scrPlayerbox __instance, double ___beatReleaseTime)
            {
                if (player != __instance.player || (!__instance.beatBeingHeld && !cpuTriggered)) return true;

                var audioPos = __instance.conductor.audioPos;
                var timeOffset = (float)(audioPos - ___beatReleaseTime);

                if (GC.showAbsoluteOffsets)
                {
                    var offsetFrames = Mathf.RoundToInt(timeOffset * 60);

                    if (RDC.auto || Mathf.Abs(offsetFrames) <= 1)
                        offsetFrames = 0;

                    if (__instance.player != RDPlayer.CPU)
                        __instance.game.mistakesManager.AddAbsoluteMistake(__instance.player, offsetFrames);
                }

                if (!(configPlayerOnlyMsOffset.Value && cpuTriggered) && GC.d_showMarginsNumerically)
                {
                    var timeOffsetInMilliseconds = timeOffset * 1000f;
                    var offsetMs = timeOffsetInMilliseconds.ToString("N3");

                    if (timeOffsetInMilliseconds >= 0)
                        offsetMs = "+" + offsetMs;

                    HUD.status = "[ " + offsetMs + " " + RDString.Get("editor.unit.ms") + " ]";
                }

                return true;
            }
        }

        public static class HoldAutoHitPatch
        {
            private static readonly double[] lastHoldReleaseTime = { 0, 0 };
            private static readonly double[] lastPerfectReleaseTime = { 0, 0 };

            [HarmonyPrefix]
            [HarmonyPatch(typeof(Beat), "LateUpdate")]
            public static bool Prefix(Beat __instance)
            {
                if (RDC.auto) return true;

                var player = __instance.row.playerProp.GetCurrentPlayer();
                var isPlayerHolding = false;
                var audioPos = __instance.conductor.audioPos;

                foreach (var row in __instance.game.rows)
                {
                    if (row.playerBox == null || player != row.playerProp.GetCurrentPlayer() || !row.playerBox.beatBeingHeld) continue;

                    isPlayerHolding = true;

                    var beatReleaseTime = row.playerBox.beatReleaseTime;
                    
                    if (beatReleaseTime > lastHoldReleaseTime[(int)player])
                        lastHoldReleaseTime[(int)player] = beatReleaseTime;
                    
                    if (row.playerBox.releaseOffsetType == OffsetType.Perfect && beatReleaseTime > lastPerfectReleaseTime[(int)player])
                        lastPerfectReleaseTime[(int)player] = beatReleaseTime;

                    break;
                }

                if (player != RDPlayer.CPU && (isPlayerHolding || (configFixHoldPseudos.Value && __instance.inputTime <= lastPerfectReleaseTime[(int)player])))
                {
                    var isHeldClap = __instance.isHeldClap;
                    var emuState = RDInput.emuStates[(int)player];

                    if (!isHeldClap && audioPos >= __instance.inputTime &&
                        !(configAntiCheeseHolds.Value && __instance.inputTime > lastHoldReleaseTime[(int)player]))
                    {
                        if (configFixAutoHitMisses.Value)
                        {
                            __instance.row.playerBox.Pulse((float)(audioPos - __instance.inputTime), __instance, true);
                            RDBase.Vfx.FlashBorderFeedback(true);
                            __instance.Create8thBeat();
                        }
                        else
                        {
                            emuState.SetKey(RDInput.PlayerEmuKey.Down);
                            Timer.Add(delegate { emuState.SetKey(RDInput.PlayerEmuKey.IsUp); }, 0.2f);
                        }
                    }

                    if (configAntiCheeseHolds.Value && isHeldClap && audioPos >= __instance.releaseTime + 0.40000000596046448)
                        emuState.SetKey(RDInput.PlayerEmuKey.Up);
                }

                if (__instance.dead) __instance.DestroyBeat(false, false);

                return false;
            }

            // Set all auto-hit frame offsets to 0
            [HarmonyTranspiler]
            [HarmonyPatch(typeof(scrPlayerbox), "Pulse")]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
            {
                var codeMatcher = new CodeMatcher(instructions, il);

                if (configFixAutoHitMisses.Value)
                {
                    Label label;
                    codeMatcher
                        .MatchForward(false,
                            new CodeMatch(OpCodes.Ldsfld, AccessTools.Field(typeof(GC), "showAbsoluteOffsets")))
                        .Advance(7)
                        .CreateLabelAt(codeMatcher.Pos + 2, out label)
                        .InsertAndAdvance(
                            new CodeInstruction(OpCodes.Ldarg_3),
                            new CodeInstruction(OpCodes.Brtrue, label));
                }

                return codeMatcher.InstructionEnumeration();
            }
        }

        public static class RankColorOnSpeedChange
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(HUD), "ShowAndSaveRank")]
            public static void Postfix(HUD __instance)
            {
                var levelType = __instance.game.currentLevel.levelType;
                if (levelType == LevelType.Boss || levelType == LevelType.Challenge) return;

                if (RDTime.speed > 1f) __instance.rank.color = new Color(0.93f, 0.44f, 0.44f);
                if (RDTime.speed < 1f) __instance.rank.color = new Color(0.44f, 0.85f, 0.93f);
            }
        }

        public static class ChangeRankButtonPerDifficulty
        {
            // Hand images are editable in the Assets/ folder
            [HarmonyPrefix]
            [HarmonyPatch(typeof(HUD), "ShowSmallHand")]
            public static bool Prefix(ref Image ___smallHand)
            {
                if (___smallHand.gameObject.activeSelf) return true;

                var hardSmallHand1 = new Texture2D(47, 24);
                hardSmallHand1.LoadImage(File.ReadAllBytes(assetsPath + "hard-small-hand-1.png"));
                hardSmallHand1.filterMode = FilterMode.Point;

                var hardSmallHand2 = new Texture2D(47, 24);
                hardSmallHand2.LoadImage(File.ReadAllBytes(assetsPath + "hard-small-hand-2.png"));
                hardSmallHand2.filterMode = FilterMode.Point;

                var easySmallHand1 = new Texture2D(47, 24);
                easySmallHand1.LoadImage(File.ReadAllBytes(assetsPath + "easy-small-hand-1.png"));
                easySmallHand1.filterMode = FilterMode.Point;

                var easySmallHand2 = new Texture2D(47, 24);
                easySmallHand2.LoadImage(File.ReadAllBytes(assetsPath + "easy-small-hand-2.png"));
                easySmallHand2.filterMode = FilterMode.Point;

                var rect = new Rect(0.0f, 0.0f, 47, 24);
                var vector = new Vector2(0.5f, 0.5f);

                var hardSmallHandSprite = Sprite.Create(hardSmallHand1, rect, vector);
                var easySmallHandSprite = Sprite.Create(easySmallHand1, rect, vector);
                Sprite[] hardSmallHandSprites = { hardSmallHandSprite, Sprite.Create(hardSmallHand2, rect, vector) };
                Sprite[] easySmallHandSprites = { easySmallHandSprite, Sprite.Create(easySmallHand2, rect, vector) };

                var spriteAnimationComponent = ___smallHand.GetComponent<SpriteAnimation>();
                var imageComponent = ___smallHand.GetComponent<Image>();

                if (scnGame.p1DefibMode > DefibMode.Normal)
                {
                    imageComponent.sprite = hardSmallHandSprite;
                    spriteAnimationComponent.currentAnimationData.sprites = hardSmallHandSprites;
                }

                if (scnGame.p1DefibMode < DefibMode.Normal)
                {
                    imageComponent.sprite = easySmallHandSprite;
                    spriteAnimationComponent.currentAnimationData.sprites = easySmallHandSprites;
                }

                return true;
            }
        }

        public static class PlayerOnlyMsOffset
        {
            [HarmonyTranspiler]
            [HarmonyPatch(typeof(scrPlayerbox), "Pulse")]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
            {
                Label label;
                var codeMatcher = new CodeMatcher(instructions, il);
                
                return codeMatcher
                    .End()
                    .CreateLabelAt(codeMatcher.Pos -1, out label)
                    .MatchBack(false,
                        new CodeMatch(OpCodes.Ldsfld, AccessTools.Field(typeof(GC), "d_showMarginsNumerically")))
                    .InsertAndAdvance(
                        new CodeInstruction(OpCodes.Ldarg_3),
                        new CodeInstruction(OpCodes.Brtrue, label))
                    .InstructionEnumeration();
            }
        }
    }
}