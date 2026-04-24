  using UnityEngine;
  using Random = System.Random;

  namespace Minigames.CoinTilt {
      public class LightingVariant : MonoBehaviour {
          [Header("References")]
          [SerializeField] private Light directionalLight;

          [Header("General Settings")] [SerializeField]
          private bool IsEnabled = false;

          [Header("Daytime Settings")]
          [SerializeField] private Color daytimeFilter = new Color(1f, 0.957f, 0.839f);
          [SerializeField] private float daytimeTemperature = 8000f;
          [SerializeField] private float daytimeIntensity = 2.27f;
          [SerializeField] private Vector3 daytimeRotation = new(50f, -30f, 0f);

          [Header("Sunset Settings")]
          [SerializeField] private Color sunsetFilter = new Color(1f, 0.957f, 0.839f);
          [SerializeField] private float sunsetTemperature = 2500f;
          [SerializeField] private float sunsetIntensity = 1.8f;
          [SerializeField] private Vector3 sunsetRotation = new(10f, -30f, 0f);

          private bool isSunset;
          private static readonly Random rng = new();

          private void Awake() {
              if (IsEnabled) isSunset = rng.NextDouble() < 0.5;
              else isSunset = false;
              ApplyLighting(isSunset);
          }

          private void ApplyLighting(bool isSunset) {
              directionalLight.useColorTemperature = true;
              if (isSunset) {
                  directionalLight.color = sunsetFilter;
                  directionalLight.colorTemperature = sunsetTemperature;
                  directionalLight.intensity = sunsetIntensity;
                  directionalLight.transform.eulerAngles = sunsetRotation;
              } else {
                  directionalLight.color = daytimeFilter;
                  directionalLight.colorTemperature = daytimeTemperature;
                  directionalLight.intensity = daytimeIntensity;
                  directionalLight.transform.eulerAngles = daytimeRotation;
              }
          }

          public void ToggleLighting() {
              isSunset = !isSunset;
              ApplyLighting(isSunset);
          }
      }
  }
