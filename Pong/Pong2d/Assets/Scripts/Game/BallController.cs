using UnityEngine;

public class BallController : MonoBehaviour
{
    [SerializeField] private float speed = 15f;

    private Rigidbody2D rb;
    private Vector2 currentDirection;
    private bool isBallActive = false;

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
        // Atualiza e força o movimento a cada frame de física
        if (isBallActive)
        {
            rb.linearVelocity = currentDirection * speed;
        }
    }

    private void LaunchBall()
    {
        float horizontalDirection = GameManager.Instance.GetServeDirection() ? 1f : -1f;

        // Guarda a direção inicial
        currentDirection = new Vector2(horizontalDirection, 0.5f).normalized;
        isBallActive = true;
    }

    public void ResetBall()
    {
        isBallActive = false;
        rb.linearVelocity = Vector2.zero;
        transform.position = Vector3.zero;

        LaunchBall();
    }
}