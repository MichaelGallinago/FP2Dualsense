﻿using MelonLoader;
using UnityEngine;

namespace FP2Dualsense.MichaelGallinago
{
    public static class BuildInfo
    {
        public const string Name = "FP2Rebalance"; // Name of the Mod.  (MUST BE SET)

        public const string Description =
            "Mod that changes the balance of the game."; // Description for the Mod.  (Set as null if none)

        public const string Author = "Michael Gallinago"; // Author of the Mod.  (MUST BE SET)
        public const string Company = null; // Company that made the Mod.  (Set as null if none)
        public const string Version = "1.1.5.0"; // Version of the Mod.  (MUST BE SET)
        public const string DownloadLink = "https://github.com/MichaelGallinago/FreedomPlanet2Rebalance"; // Download Link for the Mod.  (Set as null if none)
    }

    public class FP2Dualsense : MelonMod
    {
        public static GameObject goFP2Dualsense;

        [System.Obsolete]
        public override void OnApplicationStart() // Runs after Game Initialization.
        {
            MelonLogger.Msg("FP2Rebalance is active.");
            MelonPreferences.Load();
            MelonPreferences.Save();
        }

        /*
        public override void OnSceneWasLoaded(int buildindex, string sceneName) // Runs when a Scene has Loaded and is passed the Scene's Build Index and Name.
        {
            if (goFP2Dualsense == null)
            {
                goFP2Dualsense = new GameObject();
                GameObject.DontDestroyOnLoad(goFP2Dualsense);
                var mtc = goFP2Dualsense.AddComponent<FP2DualsenseBehaviour>();
            }
        }
        */
    }
}