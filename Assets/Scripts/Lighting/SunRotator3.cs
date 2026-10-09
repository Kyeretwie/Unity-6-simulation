using UnityEngine;

public class SunRotator3 : MonoBehaviour
{
    public Light sun;
    public float dayLength = 60f;

    private void Update()
    {
        float angle = (Time.time / dayLength) * 360f;
        transform.rotation = Quaternion.Euler(angle, -30f, 0f);

        float intensity = Mathf.Clamp01(Vector3.Dot(transform.forward, Vector3.down));
        sun.intensity = intensity;
    }
}
