using UnityEngine;

public class GoalController : MonoBehaviour
{
    [SerializeField] private bool givesPointToTeamA;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Ball"))
        {
            return;
        }

        if (GameManager.Instance == null)
        {
            return;
        }

        if (givesPointToTeamA)
        {
            GameManager.Instance.AddPointToTeamA();
        }
        else
        {
            GameManager.Instance.AddPointToTeamB();
        }

        // GameManager resets the ball after updating the score.
        // Do not reset it a second time here.
    }
}
