// One view per enemy. Bind to the three existing short HP bars.
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class EnemyView : MonoBehaviour
{
    public Image portrait;
    public Slider hpBar;
    public TMP_Text caption;
    public void Show(EnemyData data,int currentHP,int turns)
    {
        if(portrait!=null)
        {
            if(data.portrait!=null) portrait.sprite=data.portrait;
            portrait.color=currentHP>0 ? Color.white : new Color(.35f,.35f,.35f,1);
            portrait.preserveAspect=true;
            portrait.raycastTarget=false;
        }
        if(hpBar!=null) { hpBar.maxValue=data.hp; hpBar.value=currentHP; }
        if(caption!=null) caption.text=$"{data.displayName} {currentHP}/{data.hp}  {turns}";
    }
}
