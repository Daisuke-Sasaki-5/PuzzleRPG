using TMPro;
using UnityEngine;
public class MyPageController : MonoBehaviour
{
    public TMP_Text statusText;
    private void Start()
    {
        if (statusText != null)
        {
            if (GameSession.Instance == null)
            {
                statusText.text = "GameSession bootstrap missing";
            }
            else
            {
                statusText.text =
                    $"MyPage\nCoins {GameSession.Instance.coins}  Owned {GameSession.Instance.owned.Count}";
            }
        }
    }
    public void StartBattle()
    {
        if (GameSession.Instance == null || !
            GameSession.Instance.ValidParty())
        {
            return;
        }
        CourseFlow.Battle();
    }

}