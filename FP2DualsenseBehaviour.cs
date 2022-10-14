using UnityEngine;
using HarmonyLib;
using System;
using System.Linq;
using DualSenseSharp;
using MelonLoader;

namespace FP2Dualsense.MichaelGallinago
{
    public class DSSupport : MelonMod
    {
        private DeviceContext Context;
        private bool Connected = false;

        public override void OnUpdate()
        {
            if (Connected)
            {
                MelonLogger.Msg("Dualsense is active.");
                DS5OutputState outState = new DS5OutputState();
                outState.leftRumble = 10;
                outState.rightRumble = 10;
                DS5.SetDeviceOutputState(Context, outState);
            }
            else
            {
                DeviceEnumInfo[] devices = DS5.EnumDevices();
                if (devices.Length < 1) return;
                DS5.InitDeviceContext(devices[0], ref Context);
                Connected = true;
            }
        }

        public override void OnSceneWasUnloaded(int buildIndex, string sceneName)
        {

        }

        public override void OnApplicationQuit()
        {
            DS5.FreeDeviceContext(Context);
        }
    }

    class Patcher
    {
        public static FPPlayer GetPlayer => FPStage.currentStage.GetPlayerInstance_FPPlayer();
    }
}