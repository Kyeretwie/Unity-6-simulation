using UnityEngine;

/// <summary>
/// Plays fountain particle streams only while the Main Camera is inside the
/// fountain's trigger area.
/// </summary>
[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Rigidbody))]
public class FountainProximityParticles : MonoBehaviour
{
    [SerializeField] private ParticleSystem[] waterParticles;
    [Min(1f), SerializeField] private float triggerRadius = 12f;
    [SerializeField] private bool stopParticlesWhenCameraLeaves = true;

    private SphereCollider proximityTrigger;
    private Rigidbody triggerRigidbody;
    private int cameraInsideCount;

    public void Configure(ParticleSystem[] particles, float radius)
    {
        waterParticles = particles;
        triggerRadius = radius;
    }

    private void Start()
    {
        proximityTrigger = GetComponent<SphereCollider>();
        proximityTrigger.isTrigger = true;
        proximityTrigger.radius = triggerRadius;

        triggerRigidbody = GetComponent<Rigidbody>();
        triggerRigidbody.isKinematic = true;
        triggerRigidbody.useGravity = false;

        if (waterParticles == null || waterParticles.Length == 0)
        {
            waterParticles = GetComponentsInChildren<ParticleSystem>(true);
        }

        AddCameraTriggerCollider();
        StopWaterParticles();
    }

    private void AddCameraTriggerCollider()
    {
        Camera playerCamera = Camera.main;
        if (playerCamera == null || playerCamera.GetComponent<Collider>() != null) return;

        SphereCollider cameraCollider = playerCamera.gameObject.AddComponent<SphereCollider>();
        cameraCollider.radius = 0.25f;
        cameraCollider.isTrigger = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsMainCamera(other)) return;

        cameraInsideCount++;
        PlayWaterParticles();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsMainCamera(other)) return;

        cameraInsideCount = Mathf.Max(0, cameraInsideCount - 1);
        if (stopParticlesWhenCameraLeaves && cameraInsideCount == 0)
        {
            StopWaterParticles();
        }
    }

    private static bool IsMainCamera(Collider other)
    {
        Camera camera = other.GetComponent<Camera>();
        return camera != null && camera == Camera.main;
    }

    private void PlayWaterParticles()
    {
        foreach (ParticleSystem waterParticle in waterParticles)
        {
            if (waterParticle != null) waterParticle.Play();
        }
    }

    private void StopWaterParticles()
    {
        foreach (ParticleSystem waterParticle in waterParticles)
        {
            if (waterParticle != null)
            {
                waterParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
    }
}
