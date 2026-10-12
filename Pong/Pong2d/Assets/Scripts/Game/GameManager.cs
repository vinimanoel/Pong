using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int teamAScore;
    public int teamBScore;

    [Header("Match")]
    [SerializeField] private int maxScore = 15;

    [Header("References")]
    [SerializeField] private ScoreUI scoreUI;
    [SerializeField] private BallController ballController;

    private bool isGameOver;
    private bool serveToRight;

    private void Awake()
    {
        Instance = this;

        if (scoreUI == null)
        {
            scoreUI = FindFirstObjectByType<ScoreUI>();
        }

        if (ballController == null)
        {
            ballController = FindFirstObjectByType<BallController>();
        }
    }

    private void Start()
    {
        teamAScore = 0;
        teamBScore = 0;
        isGameOver = false;
        Time.timeScale = 0f;

        if (scoreUI != null)
        {
            scoreUI.UpdateScore(teamAScore, teamBScore);
        }
    }

    public void AddPointToTeamA()
    {
        if (isGameOver)
        {
            return;
        }

        teamAScore++;
        serveToRight = false;
        UpdateScore();

        if (teamAScore >= Mathf.Max(1, maxScore))
        {
            EndGame("TIME A");
        }
        else if (ballController != null)
        {
            ballController.ResetBall();
        }
    }

    public void AddPointToTeamB()
    {
        if (isGameOver)
        {
            return;
        }

        teamBScore++;
        serveToRight = true;
        UpdateScore();

        if (teamBScore >= Mathf.Max(1, maxScore))
        {
            EndGame("TIME B");
        }
        else if (ballController != null)
        {
            ballController.ResetBall();
        }
    }

    public void BeginNetworkGame()
    {
        teamAScore = 0;
        teamBScore = 0;
        isGameOver = false;
        serveToRight = false;
        UpdateScore();

        if (ballController != null &&
            ballController.TryGetComponent(out Rigidbody2D ballBody) &&
            ballBody.simulated)
        {
            ballController.ResetBall();
        }

        if (scoreUI != null)
        {
            scoreUI.ShowGame();
        }

        Time.timeScale = 1f;
    }

    public void ApplyNetworkScore(int scoreA, int scoreB)
    {
        bool teamAScored = scoreA > teamAScore;
        bool teamBScored = scoreB > teamBScore;

        teamAScore = scoreA;
        teamBScore = scoreB;
        UpdateScore();

        int scoreLimit = Mathf.Max(1, maxScore);
        if (isGameOver || scoreA >= scoreLimit || scoreB >= scoreLimit)
        {
            return;
        }

        if (teamAScored)
        {
            serveToRight = false;
            if (ballController != null)
            {
                ballController.ResetBall();
            }
        }
        else if (teamBScored)
        {
            serveToRight = true;
            if (ballController != null)
            {
                ballController.ResetBall();
            }
        }
    }
    public void EndGameFromNetwork(string winner, int scoreA, int scoreB)
    {
        teamAScore = scoreA;
        teamBScore = scoreB;
        isGameOver = true;
        UpdateScore();

        if (scoreUI != null)
        {
            scoreUI.ShowWinner(winner, teamAScore, teamBScore);
        }

        Time.timeScale = 0f;
    }

    private void EndGame(string winner)
    {
        isGameOver = true;
        Debug.Log("FIM DE JOGO! Vencedor: " + winner);

        if (scoreUI != null)
        {
            scoreUI.ShowWinner(winner, teamAScore, teamBScore);
        }

        if (PongUdpServer.Instance != null)
        {
            PongUdpServer.Instance.BroadcastGameOver(
                winner == "TIME A" ? "A" : "B",
                teamAScore,
                teamBScore
            );
        }

        Time.timeScale = 0f;
    }

    private void UpdateScore()
    {
        if (scoreUI != null)
        {
            scoreUI.UpdateScore(teamAScore, teamBScore);
        }
    }

    public bool GetServeDirection()
    {
        return serveToRight;
    }
}

