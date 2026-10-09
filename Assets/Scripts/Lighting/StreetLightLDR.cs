using UnityEngine;

/// <summary>
/// Switches assigned street lights on at night and off during the day.
/// Night begins when the main directional light is disabled or is dimmer than
/// the configured threshold.
/// </summary>
public class StreetLightLDR : MonoBehaviour
{
    [Header("Day / Night Sensor")]
    [Tooltip("Assign the scene's main Directional Light here.")]
    [SerializeField] private Light mainDirectionalLight;

    [Tooltip("The scene is treated as night when the directional light is below this intensity.")]
    [Min(0f)]
    [SerializeField] private float nightIntensityThreshold = 0.05f;

    [Tooltip("Use this when a day-cycle script rotates the sun instead of switching it off.")]
    [SerializeField] private bool detectSunBelowHorizon = true;

    [Header("Road Lights")]
    [Tooltip("Assign the Street_lights parent object. Every Point Light and Spot Light beneath it will be controlled.")]
    [SerializeField] private Transform streetLightsRoot;

    [Tooltip("Optional: assign road lights here only if they are outside the Street_lights parent.")]
    [SerializeField] private Light[] additionalStreetLights;

    [Header("Status")]
    [SerializeField, Tooltip("Read-only status shown while the game is running.")]
    private bool isNight;

    private bool hasAppliedState;

    private void Start()
    {
        UpdateStreetLights();
    }

    private void Update()
    {
        UpdateStreetLights();
    }

    private void UpdateStreetLights()
    {
        if (mainDirectionalLight == null)
        {
            return;
        }

        bool sunIsBelowHorizon = detectSunBelowHorizon && mainDirectionalLight.transform.forward.y >= 0f;
        bool shouldBeNight = !mainDirectionalLight.gameObject.activeInHierarchy ||
                             !mainDirectionalLight.enabled ||
                             mainDirectionalLight.intensity <= nightIntensityThreshold ||
                             sunIsBelowHorizon;

        if (hasAppliedState && shouldBeNight == isNight)
        {
            return;
        }

        isNight = shouldBeNight;
        hasAppliedState = true;

        if (streetLightsRoot != null)
        {
            SetLights(streetLightsRoot.GetComponentsInChildren<Light>(true));
        }

        SetLights(additionalStreetLights);
    }

    private void SetLights(Light[] lights)
    {
        foreach (Light streetLight in lights)
        {
            if (streetLight != null && streetLight != mainDirectionalLight)
            {
                streetLight.enabled = isNight;
            }
        }
    }
}
