using UnityEngine;
using UnityEngine.UI;
public class RewardIconsView : MonoBehaviour
{
    public RectTransform content;
    public Image iconPrefab;
    public void Show(BattleReceipt receipt)
    {
        foreach (Transform child in content)
        {
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }
        if (receipt == null || !receipt.won)
        {
            return;
        }
        int i = 0;
        foreach (string id in receipt.characterIds)
        {
            var icon = Instantiate(iconPrefab, content, false);
            icon.gameObject.SetActive(true);
            icon.sprite = GameSession.Instance.FindMaster(id).portrait;
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            var rect = icon.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.sizeDelta = new Vector2(96, 96);
            rect.anchoredPosition = new Vector2(i * 104, 0);
            i++;
        }
    }
}