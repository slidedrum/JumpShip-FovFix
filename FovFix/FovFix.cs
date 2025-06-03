
using Il2CppInterop.Runtime;
using Il2CppKeepsake;
using Il2CppTMPro;
using MelonLoader;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


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
        private GameObject someSlider;
        Camera cam;
        private bool setLabelText;
        private HSSlider hsslider;

        public GameObject FOVSlider { get; private set; }

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
                catch (System.Exception) { }
            }
        }
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            base.OnSceneWasLoaded(buildIndex, sceneName);
            setLabelText = false;
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (cameraSettings_FirstPersonPlayer == null || someSlider == null || setLabelText == false)
            {
                timeSinceLastCheck += Time.deltaTime;

                if (timeSinceLastCheck >= checkInterval)
                {
                    timeSinceLastCheck = 0f;
                    if (cameraSettings_FirstPersonPlayer == null)
                    {
                        var allSettings = Resources.FindObjectsOfTypeAll<CameraSettings>();
                        foreach (var settings in allSettings)
                        {
                            if (settings.name == "CameraSettings_FirstPersonPlayer")
                            {
                                cameraSettings_FirstPersonPlayer = settings;
                            }
                        }
                    }
                    if (someSlider == null)
                    {
                        someSlider = FindInactiveGameObject("LOD Bias");
                        if (someSlider != null)
                        {
                            FOVSlider = UnityEngine.Object.Instantiate(someSlider, someSlider.transform.parent, worldPositionStays: false);
                            hsslider = FOVSlider.GetComponent<HSSlider>();
                            hsslider.name = "MahFOV";
                            hsslider.maxValue = 120;
                            hsslider.minValue = 50;
                            hsslider.value = cfgTargetFov.Value;
                            FOVSlider.transform.SetSiblingIndex(9);
                            hsslider.onValueChanged.RemoveAllListeners();
                        }
                    }
                }
            }
            if (cameraSettings_FirstPersonPlayer != null)
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
            if (FOVSlider != null && FOVSlider.activeInHierarchy)
            {

                hsslider.m_LabelText.SetText("Field of View", true);
                if (!setLabelText)
                {
                    try
                    {
                        UnityAction<float> il2cppUnityAction = DelegateSupport.ConvertDelegate<UnityAction<float>>(
                            new Action<float>(onFOVsliderMoved)
                        );
                        hsslider.onValueChanged.AddListener(il2cppUnityAction);
                        setLabelText = true;
                    }
                    catch { }
                }

            }
        }
        private void onFOVsliderMoved(float value)
        {
            cfgTargetFov.Value = value;
            MelonPreferences.Save();
            if (cam != null)
            {
                cam.fieldOfView = value;
            }
        }
        private GameObject FindInactiveGameObject(string name)
        {
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

            foreach (GameObject obj in allObjects)
            {
                if (obj.name == name)
                {
                    // Optional: Filter out prefabs
                    if (obj.hideFlags == HideFlags.None && obj.scene.IsValid())
                        return obj;
                }
            }

            return null;
        }
    }
}
