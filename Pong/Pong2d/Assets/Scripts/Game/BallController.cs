using System.Collections;
using UnityEngine;
[RequireComponent(typeof(Rigidbody2D))]
public class BallController : MonoBehaviour
{
    [SerializeField] private float speed = 15f;
    [SerializeField] private float servePause = 0.75f;

    private Rigidbody2D rb;
    private Vector2 lastDirection = Vector2.right;
    private bool launched;
    private Vector2 centerPosition;
    private Coroutine serveCoroutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        centerPosition = rb.position;
    }

    private void Start()
    {
        LaunchBall();
    }

    private void FixedUpdate()
    {
        if (!launched || rb == null || !rb.simulated)
        {
            return;
        }

        Vector2 velocity = rb.linearVelocity;
        if (velocity.sqrMagnitude > 0.0001f)
        {
            lastDirection = velocity.normalized;
        }

        // Keep the configured speed and recover if a collision ever stops the ball.
        rb.linearVelocity = lastDirection * speed;
    }

    private void LaunchBall()
    {
        if (rb == null)
        {
            return;
        }

        float horizontalDirection =
            GameManager.Instance != null &&
            GameManager.Instance.GetServeDirection()
                ? 1f
                : -1f;

        lastDirection = new Vector2(horizontalDirection, 0.5f).normalized;
        launched = true;
        rb.angularVelocity = 0f;
        rb.linearVelocity = lastDirection * speed;
    }

    public void ResetBall()
    {
        if (rb == null)
        {
            return;
        }

        if (serveCoroutine != null)
        {
            StopCoroutine(serveCoroutine);
        }

        launched = false;
        rb.position = centerPosition;
        transform.position = new Vector3(centerPosition.x, centerPosition.y, transform.position.z);
        rb.angularVelocity = 0f;
        rb.linearVelocity = Vector2.zero;
        serveCoroutine = StartCoroutine(LaunchAfterServePause());
    }

    private IEnumerator LaunchAfterServePause()
    {
        yield return new WaitForSecondsRealtime(servePause);
        serveCoroutine = null;

        if (rb != null && rb.simulated)
        {
            LaunchBall();
        }
    }
}
