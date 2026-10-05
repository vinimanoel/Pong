using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int teamAScore = 0;
    public int teamBScore = 0;

    [Header("Configurações do Jogo")]
    [SerializeField] private int maxScore = 10; // Pontuação para vencer o jogo

    [Header("Referências")]
    [SerializeField] private ScoreUI scoreUI;
    [SerializeField] private BallController ballController; // Arraste a bola aqui no Inspector

    private bool serveToRight = false;
    private bool isGameOver = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Garante que o tempo do jogo está normal ao iniciar
        Time.timeScale = 1f;
    }

    public void AddPointToTeamA()
    {
        if (isGameOver) return;

        teamAScore++;
        serveToRight = !serveToRight;

        scoreUI.UpdateScore(teamAScore, teamBScore);

        // Verifica se o Time A venceu
        if (teamAScore >= maxScore)
        {
            EndGame("Time A");
        }
        else if (ballController != null)
        {
            ballController.ResetBall();
        }
    }

    public void AddPointToTeamB()
    {
        if (isGameOver) return;

        teamBScore++;
        serveToRight = !serveToRight;

        scoreUI.UpdateScore(teamAScore, teamBScore);

        // Verifica se o Time B venceu
        if (teamBScore >= maxScore)
        {
            EndGame("Time B");
        }
        else if (ballController != null)
        {
            ballController.ResetBall();
        }
    }

    private void EndGame(string winner)
    {
        isGameOver = true;
        Debug.Log("FIM DE JOGO! Vencedor: " + winner);

        // Atualiza a tela com o texto do vencedor
        if (scoreUI != null)
        {
            scoreUI.ShowWinner(winner.ToUpper() + " VENCEU!");
        }

        // Pausa a física e o movimento de toda a cena
        Time.timeScale = 0f;
    }

    public bool GetServeDirection()
    {
        return serveToRight;
    }
}