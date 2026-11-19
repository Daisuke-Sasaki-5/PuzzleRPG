using UnityEngine;
public class ResultIconsBinder : MonoBehaviour
{
    public RewardIconsView icons;
    private void Start()
    {
        if (icons != null)
        {
            icons.Show(GameSession.Instance.receipt);
        }
    }
}