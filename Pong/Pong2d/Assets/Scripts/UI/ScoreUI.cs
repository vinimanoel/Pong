using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreTextA;
    [SerializeField] private TextMeshProUGUI scoreTextB;

    public void UpdateScore(int scoreA, int scoreB)
    {
        scoreTextA.text = scoreA.ToString();
        scoreTextB.text = scoreB.ToString();
    }

}