using UnityEngine;

/// <summary>
/// Gives a fountain water surface a gentle expanding and contracting ripple.
/// </summary>
public class FountainWaterRipple : MonoBehaviour
{
    [Min(0f)] public float rippleAmount = 0.025f;
    [Min(0f)] public float rippleSpeed = 1.5f;

    private Vector3 startingScale;

    private void Start()
    {
        startingScale = transform.localScale;
    }

    private void Update()
    {
        float ripple = 1f + Mathf.Sin(Time.time * rippleSpeed) * rippleAmount;
        transform.localScale = new Vector3(startingScale.x * ripple, startingScale.y, startingScale.z * ripple);
    }
}
