using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public float speed = 5f;
    public float sensitivity = 100f;
    [Range(-89f, 0f)] public float minLookAngle = -80f;
    [Range(0f, 89f)] public float maxLookAngle = 80f;

    private float yaw;
    private float pitch;
    private bool inputLocked;
    private Vector3 startingPosition;
    private Quaternion startingRotation;
    private bool hasStartingPose;

    public bool HasStartingPose => hasStartingPose;
    public Vector3 StartingPosition => startingPosition;
    public Quaternion StartingRotation => startingRotation;

    private void Start()
    {
        startingPosition = transform.position;
        startingRotation = transform.rotation;
        hasStartingPose = true;

        Vector3 startingEulerAngles = transform.localEulerAngles;
        yaw = startingEulerAngles.y;
        pitch = startingEulerAngles.x > 180f ? startingEulerAngles.x - 360f : startingEulerAngles.x;
    }

    private void Update()
    {
        if (inputLocked)
        {
            return;
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        float horizontal = 0f;
        float vertical = 0f;

        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) horizontal -= 1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) horizontal += 1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) vertical -= 1f;
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) vertical += 1f;

        // Keep keyboard movement on the ground even when the camera is looking up or down.
        Vector3 groundForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        Vector3 groundRight = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;
        Vector3 movement = (groundRight * horizontal + groundForward * vertical).normalized;
        transform.position += movement * speed * Time.deltaTime;

        if (Mouse.current != null)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            yaw += mouseDelta.x * sensitivity * 0.01f;
            pitch -= mouseDelta.y * sensitivity * 0.01f;
            pitch = Mathf.Clamp(pitch, minLookAngle, maxLookAngle);

            transform.localRotation = Quaternion.Euler(pitch, yaw, 0f);
        }
    }

    public void SetInputLocked(bool locked)
    {
        inputLocked = locked;
    }

    public void SetViewRotation(Quaternion rotation)
    {
        transform.rotation = rotation;
        Vector3 currentRotation = transform.localEulerAngles;
        yaw = currentRotation.y;
        pitch = currentRotation.x > 180f ? currentRotation.x - 360f : currentRotation.x;
    }
}
