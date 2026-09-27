using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int teamAScore = 0;
    public int teamBScore = 0;

    [SerializeField] private ScoreUI scoreUI;

    private void Awake()
    {
        Instance = this;
    }

    public void AddPointToTeamA()
    {
        teamAScore++;
        
        serveToRight = !serveToRight;

        scoreUI.UpdateScore(teamAScore, teamBScore);

        Debug.Log("Time A: " + teamAScore);
    }

    public void AddPointToTeamB()
    {
        teamBScore++;
        
        serveToRight = !serveToRight;

        scoreUI.UpdateScore(teamAScore, teamBScore);

        Debug.Log("Time B: " + teamBScore);
    }
    private bool serveToRight = false;

    public bool GetServeDirection()
    {
        return serveToRight;
    }
}