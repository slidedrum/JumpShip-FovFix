using Il2CppKeepsake;
using MelonLoader;
using UnityEngine;
using HarmonyLib;
using Il2CppGoPickerTree;

namespace FovFix
{
    public class FovFixMain : MelonMod
    {
        private static MelonPreferences_Category cfgCategory;
        private static MelonPreferences_Entry<float> cfgTargetFov;
        private static MelonPreferences_Entry<float> cfgTransitionDuration;
        private float currentFov = 0f;
        private float transitionTime = 0f;
        private bool transitioning = false;
        private CameraManager realCameraManager;

        public override void OnInitializeMelon()
        {
            cfgCategory = MelonPreferences.CreateCategory("FovFixSettings", "FOV Fix Settings");
            cfgTargetFov = cfgCategory.CreateEntry("TargetFOV", 105f, "Target FOV");
            cfgTransitionDuration = cfgCategory.CreateEntry("TransitionDuration", 1.0f, "Transition Duration (seconds)");
        }
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            base.OnSceneWasLoaded(buildIndex, sceneName);
            updateCameraManager();
        }
        public void updateCameraManager()
        {
            var cameraManagers = Resources.FindObjectsOfTypeAll<CameraManager>();
            foreach (CameraManager cameraManager in cameraManagers)
            {
                if (cameraManager.CurrentDefaultFov != 0f)
                {
                    realCameraManager = cameraManager;
                }
            }
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            Camera cam = Camera.main;
            if (cam == null) return;
            if (realCameraManager == null) { updateCameraManager(); }
            if (!transitioning && Mathf.Approximately(cam.fieldOfView, realCameraManager.CurrentDefaultFov))
            {
                currentFov = cam.fieldOfView;
                transitionTime = 0f;
                transitioning = true;
            }

            if (transitioning)
            {
                if (!Mathf.Approximately(cam.fieldOfView, currentFov))
                {
                    transitioning = false;
                    return;
                }

                transitionTime += Time.deltaTime;
                float t = Mathf.Clamp01(transitionTime / cfgTransitionDuration.Value);
                float easedT = t * t;
                float newFov = Mathf.Lerp(realCameraManager.CurrentDefaultFov, cfgTargetFov.Value, easedT);
                cam.fieldOfView = newFov;
                currentFov = newFov;

                if (t >= 1f)
                {
                    transitioning = false;
                }
            }
        }
    }
}
