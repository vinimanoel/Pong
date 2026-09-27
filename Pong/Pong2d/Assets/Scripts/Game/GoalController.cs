using UnityEngine;

public class GoalController : MonoBehaviour
{
    [SerializeField] private bool givesPointToTeamA;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ball"))
        {
            if (givesPointToTeamA)
            {
                GameManager.Instance.AddPointToTeamA();
            }
            else
            {
                GameManager.Instance.AddPointToTeamB();
            }

            BallController ball = collision.GetComponent<BallController>();

            if (ball != null)
            {
                ball.ResetBall();
            }
        }
    }
}