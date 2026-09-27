using UnityEngine;

public class BallController : MonoBehaviour
{
    [SerializeField] private float speed = 5f;

    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        LaunchBall();
    }

    private void LaunchBall()
    {
        float horizontalDirection = GameManager.Instance.GetServeDirection() ? 1f : -1f;

        Vector2 direction = new Vector2(horizontalDirection, 0.5f).normalized;

        rb.linearVelocity = direction * speed;
    }

    public void ResetBall()
    {
        rb.linearVelocity = Vector2.zero;

        transform.position = Vector3.zero;

        LaunchBall();
    }
}