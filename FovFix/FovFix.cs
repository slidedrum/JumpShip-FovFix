using HarmonyLib;
using Il2Cpp;
using Il2CppGoPickerTree;
using Il2CppKeepsake;
using MelonLoader;
using System;
using System.Runtime;
using UnityEngine;


namespace FovFix
{
    public class FovFixMain : MelonMod
    {
        private static MelonPreferences_Category cfgCategory;
        private static MelonPreferences_Entry<float> cfgTargetFov;
        private CameraManager realCameraManager;
        private CameraSettings cameraSettings_FirstPersonPlayer;
        private float timeSinceLastCheck;
        private float checkInterval = 1f;
        Camera cam;

        public override void OnInitializeMelon()
        {
            HarmonyInstance.PatchAll();
            cfgCategory = MelonPreferences.CreateCategory("FovFixSettings", "FOV Fix Settings");
            cfgTargetFov = cfgCategory.CreateEntry("TargetFOV", 105f, "Target FOV");
        }
        public void updateCameraManager()
        {
            var cameraManagers = Resources.FindObjectsOfTypeAll<CameraManager>();
            foreach (CameraManager cameraManager in cameraManagers)
            {
                try
                {
                    if (cameraManager.CurrentDefaultFov != 0f)
                    {
                        realCameraManager = cameraManager;
                    }
                }
                catch (Exception) { }
            }
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (cameraSettings_FirstPersonPlayer == null)
            {
                timeSinceLastCheck += Time.deltaTime;

                if (timeSinceLastCheck >= checkInterval)
                {
                    timeSinceLastCheck = 0f;
                    MelonLogger.Msg($"Looking for CameraSettings_FirstPersonPlayer");
                    // Try to find the object in the scene
                    var allSettings = Resources.FindObjectsOfTypeAll<CameraSettings>();
                    foreach (var settings in allSettings)
                    {
                        MelonLogger.Msg($"Saw: {settings.name}");
                        if (settings.name == "CameraSettings_FirstPersonPlayer")
                        {
                            MelonLogger.Msg($"Found: {settings.name}");
                            cameraSettings_FirstPersonPlayer = settings;
                        }
                    }
                }
            }
            else
            {
                if (cam == null) cam = Camera.main;
                if (cam == null) return;
                if (realCameraManager == null) { updateCameraManager(); }
                if (realCameraManager == null) return;
                if (realCameraManager.m_TargetFov == cameraSettings_FirstPersonPlayer.m_Fov.Value)
                {
                    realCameraManager.m_TargetFov = cfgTargetFov.Value;
                }
                if (cam.fieldOfView == cameraSettings_FirstPersonPlayer.m_Fov.Value)
                {
                    cam.fieldOfView = cfgTargetFov.Value;
                }
            }
        }
    }
}
