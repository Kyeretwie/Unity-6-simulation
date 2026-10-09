using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BouncingBall : MonoBehaviour
{
    [Tooltip("Upward impulse applied whenever the ball lands.")]
    public float jumpForce = 7f;

    private Rigidbody ballRigidbody;

    private void Awake()
    {
        ballRigidbody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        Jump();
    }

    private void OnCollisionEnter(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                Jump();
                break;
            }
        }
    }

    private void Jump()
    {
        ballRigidbody.linearVelocity = new Vector3(
            ballRigidbody.linearVelocity.x,
            0f,
            ballRigidbody.linearVelocity.z);

        ballRigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
}
