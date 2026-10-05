using UnityEngine;

public class BallController : MonoBehaviour
{
    [SerializeField] private float speed = 15f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        LaunchBall();
    }

    private void FixedUpdate()
    {
        // Mantém a velocidade sempre cravada no valor definido,
        // mas respeita a direção do quique calculada pela física
        if (rb.linearVelocity != Vector2.zero)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * speed;
        }
    }

    private void LaunchBall()
    {
        float horizontalDirection = GameManager.Instance.GetServeDirection() ? 1f : -1f;

        // Ângulo inicial de lançamento
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