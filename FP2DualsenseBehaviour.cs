using UnityEngine;
using HarmonyLib;
using DualSenseAPI;
using DualSenseAPI.State;
using System;
using System.Collections.Generic;
using System.Linq;
using MelonLoader;

namespace FP2Dualsense.MichaelGallinago
{
    public class FP2DualsenseBehaviour : MelonMod
    {
        public override void OnSceneWasUnloaded(int buildIndex, string sceneName)
        {
            var dualsense = DSSupport.DS;
            dualsense.EndPolling();
            DSSupport.ResetToDefaultState(dualsense);
        }

        public override void OnApplicationQuit()
        {
            var dualsense = DSSupport.DS;
            dualsense.EndPolling();
            DSSupport.ResetToDefaultState(dualsense);
            dualsense.Release();
        }
    }

    class Patcher
    {
        public static FPPlayer GetPlayer => FPStage.currentStage.GetPlayerInstance_FPPlayer();
    }

    class DSSupport
    {
        public static DualSense DS { get; set; }
        static void Main(string[] args)
        {
            DualSense[] available = DualSense.EnumerateControllers().Cast<DualSense>().ToArray();
            DS = available.Length == 0 ? null : available[0];
        }

        public static void SetInitialProperties(DualSense ds)
        {
            ds.JoystickDeadZone = 0.1f;
            ds.OutputState = new DualSenseOutputState()
            {
                LightbarBehavior = LightbarBehavior.CustomColor,
                R2Effect = new TriggerEffect.Vibrate(20, 1, 1, 1),
                L2Effect = new TriggerEffect.Section(0, 0.5f)
            };
        }

        public static void ResetToDefaultState(DualSense ds)
        {
            ds.OutputState.LightbarBehavior = LightbarBehavior.PulseBlue;
            ds.OutputState.PlayerLed = PlayerLed.None;
            ds.OutputState.R2Effect = TriggerEffect.Default;
            ds.OutputState.L2Effect = TriggerEffect.Default;
            ds.OutputState.MicLed = MicLed.Off;
            ds.ReadWriteOnce();
        }

        public static LightbarColor ColorWheel(int position)
        {
            int r = 0, g = 0, b = 0;
            switch (position / 128)
            {
                case 0:
                    r = 127 - position % 128;   //Red down
                    g = position % 128;      // Green up
                    b = 0;                  //blue off
                    break;
                case 1:
                    g = 127 - position % 128;  //green down
                    b = position % 128;      //blue up
                    r = 0;                  //red off
                    break;
                case 2:
                    b = 127 - position % 128;  //blue down
                    r = position % 128;      //red up
                    g = 0;                  //green off
                    break;
            }
            return new LightbarColor(r / 255f, g / 255f, b / 255f);
        }
    }

    [HarmonyPatch(typeof(FPPlayer), "Start")]
    public class Patch_Start
    {
        static void Postfix()
        {
            var fpPlayer = Patcher.GetPlayer;
            var dualsense = DSSupport.DS;

            dualsense.Acquire();
            DualSenseInputState prevState = dualsense.InputState;
            int wheelPos = 0;

            DSSupport.SetInitialProperties(dualsense);
            DualSenseInputState dsInput;
            DualSenseOutputState dsOutput;
        }
    }

    [HarmonyPatch(typeof(FPPlayer), "Action_NeeraRune")]
    public class Patch_Action_NeeraRune
    {
        static void Postfix()
        {
            var fpPlayer = Patcher.GetPlayer;
            if (fpPlayer.characterID == FPCharacterID.NEERA && fpPlayer.energy >= 100f && fpPlayer.powerupTimer == 30f)
            {
                if (FPStage.prevGameSpeed == 1f)
                {
                    FPStage.SetGameSpeed(0.7f);
                }
                fpPlayer.energyRecoverRateCurrent = -0.3f;
            }
        }
    }

    [HarmonyPatch(typeof(FPSaveManager), "AddCrystal")]
    public class Patch_AddCrystal
    {
        static void Postfix(FPPlayer targetPlayer)
        {
            if (targetPlayer.characterID == FPCharacterID.NEERA && targetPlayer.energyRecoverRateCurrent < 0f)
            {
                targetPlayer.energy += targetPlayer.energyRecoverRate * 45f;
            }
        }
    }

    [HarmonyPatch(typeof(FPPlayer), "Energy_Restore")]
    public class Patch_Energy_Restore
    {
        static bool Prefix(float amount)
        {
            var fpPlayer = Patcher.GetPlayer;
            if (amount >= 0f)
            {
                if (fpPlayer.energy < 100f)
                {
                    fpPlayer.energy = Mathf.Min(fpPlayer.energy + amount + (fpPlayer.powerupTimer > 0f ? 10f : 0f), 100f);
                    if (fpPlayer.energy >= 100f)
                    {
                        fpPlayer.Effect_Regen_Sparkle();
                        return false;
                    }
                }
            }
            else
            {
                fpPlayer.energy = Mathf.Max(fpPlayer.energy + amount, 0f);
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(FPPlayer), "Action_NeeraEnergyReset")]
    public class Patch_Action_NeeraEnergyReset
    {
        static bool Prefix(bool zero = true)
        {
            var fpPlayer = Patcher.GetPlayer;
            if (zero)
            {
                fpPlayer.Energy_Restore(-50f);
            }
            if (!zero || fpPlayer.energy <= 0f)
            {
                if (FPStage.prevGameSpeed == 0.7f)
                {
                    FPStage.SetGameSpeed(1f);
                }
                fpPlayer.energyRecoverRateCurrent = fpPlayer.energyRecoverRate / 4f;
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(FPPlayer), "Action_Neera_GroundMoves")]
    public class Patch_Action_Neera_GroundMoves
    {
        private static float savedEnergy;

        static void Prefix()
        {
            var fpPlayer = Patcher.GetPlayer;
            savedEnergy = fpPlayer.energy;
            fpPlayer.energy += 50f;
        }

        static void Postfix()
        {
            var fpPlayer = Patcher.GetPlayer;
            fpPlayer.energy = fpPlayer.energy == 0 ? savedEnergy - 50f : savedEnergy;
        }
    }

    [HarmonyPatch(typeof(FPPlayer), "Action_Neera_AirMoves")]
    public class Patch_Action_Neera_AirMoves
    {
        private static float savedEnergy;

        static void Prefix()
        {
            var fpPlayer = Patcher.GetPlayer;
            savedEnergy = fpPlayer.energy;
            fpPlayer.energy += 50f;
        }

        static void Postfix()
        {
            var fpPlayer = Patcher.GetPlayer;
            fpPlayer.energy = fpPlayer.energy == 0 ? savedEnergy - 50f : savedEnergy;

            if ((fpPlayer.powerupTimer > 0f || fpPlayer.energyRecoverRateCurrent < 0f) && fpPlayer.state == new FPObjectState(fpPlayer.State_Neera_AttackForward))
            {
                fpPlayer.velocity.y = Mathf.Max(5f, fpPlayer.velocity.y + 2f);
            }
        }
    }

    [HarmonyPatch(typeof(FPPlayer), "State_Neera_AttackNeutral")]
    public class Patch_State_Neera_AttackNeutral
    {
        static void Postfix()
        {
            var fpPlayer = Patcher.GetPlayer;
            if (!fpPlayer.onGround && fpPlayer.state == new FPObjectState(fpPlayer.State_Neera_AttackForward) && (fpPlayer.powerupTimer > 0f || fpPlayer.energyRecoverRateCurrent < 0f))
            {
                fpPlayer.jumpAbilityFlag = false;
            }
        }
    }

    [HarmonyPatch(typeof(FPPlayer), "State_Neera_AttackForward")]
    public class Patch_State_Neera_AttackForward
    {
        static void Postfix()
        {
            var fpPlayer = Patcher.GetPlayer;
            if (fpPlayer.currentAnimation == "AirAttackDown" && fpPlayer.input.attackPress)
            {
                fpPlayer.velocity.y = -12f;
            }
        }
    }

    [HarmonyPatch(typeof(FPPlayer), "Action_Hurt")]
    public class Patch_Action_Hurt
    {
        static void Postfix()
        {
            var fpPlayer = Patcher.GetPlayer;
            if (fpPlayer.guardEffectFlag == true && fpPlayer.characterID == FPCharacterID.NEERA)
            {
                fpPlayer.Energy_Restore((fpPlayer.energyRecoverRateCurrent < 0f) ? 40f : 25f);
            }
        }
    }

    [HarmonyPatch(typeof(FPPlayer), "State_Lilac_DragonBoostPt2")]
    public class Patch_State_Lilac_DragonBoostPt2
    {
        static void Postfix()
        {
            var fpPlayer = Patcher.GetPlayer;
            if (fpPlayer.currentAnimation == "Wings_Loop" && fpPlayer.state == new FPObjectState(fpPlayer.State_Lilac_DragonBoostPt2))
            {
                if (fpPlayer.powerupTimer <= 0f)
                {
                    fpPlayer.health = FPCommon.RoundToQuantumWithinErrorThreshold(fpPlayer.health - FPStage.deltaTime / 70f, 0.5f);
                }

                var speed = Mathf.Sqrt(fpPlayer.velocity.x * fpPlayer.velocity.x + fpPlayer.velocity.y * fpPlayer.velocity.y);
                if (fpPlayer.input.leftPress)
                {
                    fpPlayer.angle = 180f;
                }
                if (fpPlayer.input.rightPress)
                {
                    fpPlayer.angle = 0f;
                }
                if (fpPlayer.input.downPress)
                {
                    fpPlayer.angle = 90f;
                }
                if (fpPlayer.input.upPress)
                {
                    fpPlayer.angle = 270f;
                }

                if (fpPlayer.state != new FPObjectState(fpPlayer.State_Lilac_DragonBoostPt2))
                {
                    if (FPStage.prevGameSpeed == 0.6f)
                    {
                        FPStage.SetGameSpeed(1f);
                    }
                }
                else
                {
                    if (FPStage.prevGameSpeed == 1f)
                    {
                        FPStage.SetGameSpeed(0.6f);
                    }
                }

                fpPlayer.prevVelocity = fpPlayer.velocity;
                fpPlayer.velocity.x = Mathf.Sin(0.017453292f * (fpPlayer.angle + 90f)) * speed;
                fpPlayer.velocity.y = Mathf.Cos(0.017453292f * (fpPlayer.angle + 90f)) * speed;
            }
        }
    }

    [HarmonyPatch(typeof(ItemFuel), "CollisionCheck")]
    public class Patch_ItemFuel
    {
        static void Postfix()
        {
            FPBaseObject fpbaseObject = null;
            while (FPStage.ForEach(FPPlayer.classID, ref fpbaseObject))
            {
                FPPlayer fpPlayer = (FPPlayer)fpbaseObject;
                if (fpPlayer.characterID == FPCharacterID.LILAC && !fpPlayer.hasSpecialItem && fpPlayer.powerupTimer >= 600f)
                {
                    fpPlayer.Action_Jump();
                    fpPlayer.hasSpecialItem = true;
                    fpPlayer.powerupTimer = 0f;
                    fpPlayer.flashTime = 0f;
                }
            }
        }
    }

    [HarmonyPatch(typeof(FPPlayer), "State_Carol_JumpDiscWarp")]
    public class Patch_State_Carol_JumpDiscWarp
    {
        private static Vector2 savedVelocity;

        static void Prefix()
        {
            var fpPlayer = Patcher.GetPlayer;
            savedVelocity = fpPlayer.velocity;
        }

        static void Postfix()
        {
            var fpPlayer = Patcher.GetPlayer;
            if (fpPlayer.genericTimer <= 0f && fpPlayer.guardTime <= 0f && fpPlayer.input.guardHold)
            {
                fpPlayer.velocity = savedVelocity * 1.2f;
                fpPlayer.hitStun = 0f;

                if (Mathf.Abs(fpPlayer.velocity.x) > 12f)
                {
                    fpPlayer.SetPlayerAnimation("GuardAirFast", 0f, 0f, false, true);
                }
                else
                {
                    fpPlayer.SetPlayerAnimation("GuardAir", 0f, 0f, false, true);
                }
                fpPlayer.animator.SetSpeed(Mathf.Max(1f, 0.7f + Mathf.Abs(fpPlayer.velocity.x * 0.05f)));
                fpPlayer.childAnimator.SetSpeed(Mathf.Max(1f, 0.7f + Mathf.Abs(fpPlayer.velocity.x * 0.05f)));
                fpPlayer.Action_Guard(0f);
                fpPlayer.Action_ShadowGuard();
                GuardFlash guardFlash = (GuardFlash)FPStage.CreateStageObject(GuardFlash.classID, fpPlayer.position.x, fpPlayer.position.y);
                guardFlash.parentObject = fpPlayer;
                fpPlayer.Action_StopSound();
                FPAudio.PlaySfx(15);
            }
        }
    }

    [HarmonyPatch(typeof(FPPlayer), "ApplyGroundForces")]
    public class Patch_ApplyGroundForces
    {
        static void Postfix()
        {
            var fpPlayer = Patcher.GetPlayer;
            if (fpPlayer.state == new FPObjectState(fpPlayer.State_Carol_Roll))
            {
                fpPlayer.groundVel *= 1f - Mathf.Sin(fpPlayer.groundAngle * 0.017453292f) / 32f * ((fpPlayer.groundVel > 0f) ? 1f : -1f) * FPStage.deltaTime;
            }
            else if (fpPlayer.characterID == FPCharacterID.BIKECAROL && fpPlayer.currentAnimation == "Crouching")
            {
                var acceleration = 1f - Mathf.Sin(fpPlayer.groundAngle * 0.017453292f) / 32f * ((fpPlayer.groundVel > 0f) ? 1f : -1f) * FPStage.deltaTime;
                fpPlayer.groundVel *= acceleration * (acceleration > 1f ? 1.004f: 0.998f);
            }
        }
    }

    [HarmonyPatch(typeof(FPPlayer), "State_CrushKO")]
    public class Patch_State_CrushKO
    {
        static void Prefix()
        {
            var fpPlayer = Patcher.GetPlayer;
            if (fpPlayer.characterID == FPCharacterID.LILAC)
            {
                fpPlayer.hasSpecialItem = false;
            }
        }
    }

    [HarmonyPatch(typeof(FPPlayer), "State_KO")]
    public class Patch_State_KO
    {
        static void Prefix()
        {
            var fpPlayer = Patcher.GetPlayer;
            if (fpPlayer.characterID == FPCharacterID.LILAC)
            {
                fpPlayer.hasSpecialItem = false;
            }
        }
    }

    /*
    [HarmonyPatch(typeof(ClassName), "MethodName")]
    public class Patch_MethodName
    {
        static bool Prefix()
        {
        }
        static void Postfix()
        {
        }
    }
    */
}