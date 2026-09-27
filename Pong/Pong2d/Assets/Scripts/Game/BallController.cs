using UnityEngine;

public class BallController : MonoBehaviour
{
    [SerializeField] private float speed = 5f;

    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        Vector2 direction = new Vector2(1f, 0.5f).normalized;

        rb.linearVelocity = direction * speed;
    }
}