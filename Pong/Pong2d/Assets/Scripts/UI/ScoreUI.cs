using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement; // Necessário para recarregar a cena

public class ScoreUI : MonoBehaviour
{
    [Header("Placar")]
    [SerializeField] private TextMeshProUGUI scoreTextA;
    [SerializeField] private TextMeshProUGUI scoreTextB;

    [Header("Painel de Vitória")]
    [SerializeField] private GameObject victoryPanel; // Referência ao Painel (Fundo + Botão)
    [SerializeField] private TextMeshProUGUI winnerText; // Texto de quem ganhou

    private void Start()
    {
        // Garante que o painel comece desligado (invisível) quando o jogo inicia
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }
    }

    public void UpdateScore(int scoreA, int scoreB)
    {
        scoreTextA.text = scoreA.ToString();
        scoreTextB.text = scoreB.ToString();
    }

    public void ShowWinner(string message)
    {
        if (victoryPanel != null && winnerText != null)
        {
            winnerText.text = message; // Muda o texto para "TIME A VENCEU"
            victoryPanel.SetActive(true); // Liga o painel na tela
        }
    }

    // Função que será chamada quando o jogador clicar no botão de "Jogar Novamente"
    public void RestartGame()
    {
        Time.timeScale = 1f; // Volta o tempo ao normal antes de recarregar
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Recarrega a cena atual
    }
}