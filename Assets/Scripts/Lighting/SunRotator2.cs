using UnityEngine;

public class SunRotator2 : MonoBehaviour
{
    public float dayLength = 60f;

    private void Update()
    {
        float angle = (Time.time / dayLength) * 360f;
        transform.rotation = Quaternion.Euler(angle, -30f, 0f);
    }
}
