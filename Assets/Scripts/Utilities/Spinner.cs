using UnityEngine;

/// <summary>
/// Rotates the attached GameObject continuously.
/// </summary>
public class Spinner : MonoBehaviour
{
    [Tooltip("Degrees rotated per second on each local axis.")]
    [SerializeField] private Vector3 degreesPerSecond = new Vector3(0f, 90f, 0f);

    private void Update()
    {
        transform.Rotate(degreesPerSecond * Time.deltaTime, Space.Self);
    }
}
