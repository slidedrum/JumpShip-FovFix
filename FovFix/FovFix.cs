using Il2CppKeepsake;
using MelonLoader;
using UnityEngine;
using HarmonyLib;

namespace FovFix
{
    public class FovFixMain : MelonMod
    {
        private static MelonPreferences_Category cfgCategory;
        private static MelonPreferences_Entry<float> cfgTargetFov;
        private static MelonPreferences_Entry<float> cfgTransitionDuration;
        private float initialFov = 65f;
        private float currentFov = 65f;
        private float transitionTime = 0f;
        private bool transitioning = false;

        public override void OnInitializeMelon()
        {
            cfgCategory = MelonPreferences.CreateCategory("FovFixSettings", "FOV Fix Settings");
            cfgTargetFov = cfgCategory.CreateEntry("TargetFOV", 105f, "Target FOV");
            cfgTransitionDuration = cfgCategory.CreateEntry("TransitionDuration", 1.0f, "Transition Duration (seconds)");
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            Camera cam = Camera.main;
            if (cam == null) return;

            if (!transitioning && Mathf.Approximately(cam.fieldOfView, initialFov))
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
                float newFov = Mathf.Lerp(initialFov, cfgTargetFov.Value, easedT);
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
