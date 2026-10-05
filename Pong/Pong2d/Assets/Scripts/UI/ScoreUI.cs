using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreTextA;
    [SerializeField] private TextMeshProUGUI scoreTextB;
    [SerializeField] private TextMeshProUGUI winnerText; // <--- Novo campo para a mensagem de vitória

    private void Start()
    {
        // Esconde o texto de vitória no início do jogo
        if (winnerText != null)
        {
            winnerText.gameObject.SetActive(false);
        }
    }

    public void UpdateScore(int scoreA, int scoreB)
    {
        scoreTextA.text = scoreA.ToString();
        scoreTextB.text = scoreB.ToString();
    }

    public void ShowWinner(string message)
    {
        if (winnerText != null)
        {
            winnerText.text = message;
            winnerText.gameObject.SetActive(true); // Exibe o texto quando o jogo termina
        }
    }
}