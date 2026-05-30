using Options;
using UnityEngine;

namespace Shop {
    public class CloudSettings : MonoBehaviour {
        [SerializeField] private Renderer cloudRenderer;
        private Material cloudMaterialInstance;
        private float originalModulationSpeed;
        private float originalEvolutionSpeed;
        private (float, float) originalScrollSpeeds;

        private void Awake() {
            cloudMaterialInstance = cloudRenderer.material;
            GameSettings.OnApplySettings += UpdateCloudAnimationSpeed;
            originalModulationSpeed = cloudMaterialInstance.GetFloat("_ModulationEvolutionSpeed");
            originalEvolutionSpeed = cloudMaterialInstance.GetFloat("_EvolutionSpeed");
            originalScrollSpeeds = (cloudMaterialInstance.GetFloat("_ScrollSpeedX"), cloudMaterialInstance.GetFloat("_ScrollSpeedY"));
            UpdateCloudAnimationSpeed();
        }

        private void UpdateCloudAnimationSpeed() {
            if (GameSettings.Accessibility.AnimateClouds) {
                cloudMaterialInstance.SetFloat("_ModulationEvolutionSpeed", originalModulationSpeed);
                cloudMaterialInstance.SetFloat("_EvolutionSpeed", originalEvolutionSpeed);
                cloudMaterialInstance.SetFloat("_ScrollSpeedX", originalScrollSpeeds.Item1);
                cloudMaterialInstance.SetFloat("_ScrollSpeedY", originalScrollSpeeds.Item2);
            }
            else {
                cloudMaterialInstance.SetFloat("_ModulationEvolutionSpeed", 0f);
                cloudMaterialInstance.SetFloat("_EvolutionSpeed", 0f);
                cloudMaterialInstance.SetFloat("_ScrollSpeedX", 0f);
                cloudMaterialInstance.SetFloat("_ScrollSpeedY", 0f);
            }
        }

        private void OnDestroy() {
            GameSettings.OnApplySettings -= UpdateCloudAnimationSpeed;
        }
    }
}