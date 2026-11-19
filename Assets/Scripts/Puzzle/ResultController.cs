using TMPro;
using UnityEngine;
public class ResultController : MonoBehaviour
{
    public TMP_Text resultText;
    private void Start()
    {
        BattleReceipt r;
        if (GameSession.Instance != null)
        {
            r = GameSession.Instance.receipt;
        }
        else
        {
            r = null;
        }
        if (resultText == null)
        {
            return;
        }
        if (r == null)
        {
            resultText.text = "No battle result. Return to MyPage.";
        }
        else
        {
            string displayValue3;
            if (r.won)
            {
                displayValue3 = "VICTORY";
            }
            else
            {
                displayValue3 = "DEFEAT";
            }
            resultText.text =
                $"{(displayValue3)}\nCoins +{r.coins}  EXP {r.experience} (growth begins lesson11)\nDrops: {string.Join(", ", r.characterIds)}";
        }
    }
    public void Back()
    {
        CourseFlow.MyPage();
    }
}