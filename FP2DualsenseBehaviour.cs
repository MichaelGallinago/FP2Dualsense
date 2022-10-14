using UnityEngine;
using HarmonyLib;
using DualSenseAPI;
using DualSenseAPI.State;
using System;
using System.Linq;
using MelonLoader;

namespace FP2Dualsense.MichaelGallinago
{
    public class DSSupport : MelonMod
    {
        public static DualSense DS { get; set; }
        public override void OnUpdate()
        {
            DualSense[] available = DualSense.EnumerateControllers().Cast<DualSense>().ToArray();
            DS = available.Length == 0 ? null : available[0];

            var dualsense = DS;

            dualsense.Acquire();

            SetInitialProperties(dualsense);
            dualsense.BeginPolling(1);

            DualSenseInputState dsInput = dualsense.InputState;
            DualSenseOutputState dsOutput = dualsense.OutputState;

            dsOutput.LeftRumble = Math.Abs(dsInput.LeftAnalogStick.Y);
            dsOutput.RightRumble = Math.Abs(dsInput.RightAnalogStick.Y);

            dsOutput.LightbarColor = new LightbarColor(1f, 0f, 0f);
        }

        public override void OnSceneWasUnloaded(int buildIndex, string sceneName)
        {
            var dualsense = DS;
            dualsense.EndPolling();
            DSSupport.ResetToDefaultState(dualsense);
        }

        public override void OnApplicationQuit()
        {
            var dualsense = DS;
            dualsense.EndPolling();
            DSSupport.ResetToDefaultState(dualsense);
            dualsense.Release();
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

    class Patcher
    {
        public static FPPlayer GetPlayer => FPStage.currentStage.GetPlayerInstance_FPPlayer();
    }

    /*
    [HarmonyPatch(typeof(FPPlayer), "Start")]
    public class Patch_Start
    {
        static void Postfix()
        {
        }
    }
    */
    /*
    [HarmonyPatch(typeof(FPPlayer), "Update")]
    public class Patch_Update
    {
        static void Postfix()
        {
            DualSense[] available = DualSense.EnumerateControllers().Cast<DualSense>().ToArray();
            var dualsense = available.Length == 0 ? null : available[0];

            var fpPlayer = Patcher.GetPlayer;
            dualsense.Acquire();

            SetInitialProperties(dualsense);
            DualSenseInputState dsInput = dualsense.ReadWriteOnce();
            DualSenseOutputState dsOutput = dualsense.OutputState;

            dsOutput.LeftRumble = Math.Abs(dsInput.LeftAnalogStick.Y);
            dsOutput.RightRumble = Math.Abs(dsInput.RightAnalogStick.Y);

            dsOutput.LightbarColor = new LightbarColor(1f, 0f, 0f);
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