using System.Collections;
using TMPro;
using UnityEngine;
[RequireComponent(typeof(PuzzleBoard))]
public class PuzzleResolver:MonoBehaviour
{
    [SerializeField] bool refillEnabled=false,repeatMatches=false,useFixedRefill=false;
    [SerializeField,Min(0f)] float fallDuration=0f,phaseWait=.25f;
    [SerializeField] TMP_Text comboText;
    PuzzleBoard owner;
    public bool IsBusy
    {
        get;
        private set;
    }
    public int TotalCombos
    {
        get;
        private set;
    }
    public int TotalCleared
    {
        get;
        private set;
    }
    public int ClearRounds
    {
        get;
        private set;
    }
    public int ExtraChains=>Mathf.Max(0,ClearRounds-1);
    void Awake()
    {
        owner=GetComponent<PuzzleBoard>();
    }
    void Start()
    {
        RefreshLabel();
    }
    void RefreshLabel()
    {
        if(comboText!=null)comboText.text=$"{TotalCombos} Combo";
    }
    [ContextMenu("Resolve Now")]
 public void TryResolve()
    {
        if(IsBusy||owner.Pieces==null||owner.IsOperating||owner.HasEmptyCells())return;
        TotalCombos=TotalCleared=ClearRounds=0;
        RefreshLabel();
        IsBusy=true;
        StartCoroutine(ResolveRoutine());
    }
    IEnumerator ResolveRoutine()
    {
        const int maxRounds=20;
        while(true)
        {
            bool[,] marks=PuzzleMatcher.Find(owner.Pieces,true);
            int count=PuzzleMatcher.Count(marks);
            if(count==0)break;
            int groups=PuzzleComboCounter.Count(owner.Pieces,marks);
            TotalCombos+=groups;
            RefreshLabel();
            Debug.Log($"第{ClearRounds+1}回：対象{count}個 / {groups}組");
            if(phaseWait>0)yield return new WaitForSecondsRealtime(phaseWait);
            TotalCleared+=owner.RemoveMarked(marks);
            ClearRounds++;
            owner.CollapsePieces();
            yield return owner.AnimateToCells(fallDuration);
            if(!refillEnabled)break;
            owner.RefillPieces(useFixedRefill);
            yield return owner.AnimateToCells(fallDuration);
            owner.ValidateBoard();
            if(!repeatMatches)break;
            if(ClearRounds>=maxRounds)
            {
                int remain=PuzzleMatcher.Count(PuzzleMatcher.Find(owner.Pieces,true));
                if(remain>0)Debug.LogWarning($"{maxRounds}消去回で中断：残り{remain}個");
                break;
            }
        }
        RefreshLabel();
        Debug.Log($"消去{TotalCleared}個 / {TotalCombos} Combo / 消去{ClearRounds}回 / 追加連鎖{ExtraChains}回");
        IsBusy=false;
    }
}
