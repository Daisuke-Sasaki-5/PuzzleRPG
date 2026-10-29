using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent (typeof(PuzzleBoard))]
public class PuzzleResolver : MonoBehaviour
{
    [Header("段階設定")]
    [SerializeField] private bool refillEnabled = false;
    [SerializeField] private bool repeartMatches = false;

    [Header("固定テスト")]
    [SerializeField] private bool useFixedRefill = false;

    [Header("時間設定")]
    [SerializeField, Min(0f)] private float fallDuration = 0f;

    [SerializeField, Min(0f)] private float phaseWaite = 0.25f;

    [Header("UI")]
    [SerializeField] private TMP_Text comboText;

    private PuzzleBoard owner;
    private Coroutine resolveCoroutine;

    // 解決中かどうか
    public bool IsBusy {  get; private set; }

    // 今回の操作で発生したコンボ数
    public int TotalCombos {  get; private set; }

    // 実際に消去した石の数
    public int TotalCleared {  get; private set; }

    // 消去処理を実行した回数
    public int ClearRounds {  get; private set; }

    // 初回以降の追加連鎖回数
    public int ExtraChains => Mathf.Max(0, ClearRounds - 1);

    private void Awake()
    {
        owner = GetComponent<PuzzleBoard>();
    }

    private void Start()
    {
        
    }

    [ContextMenu("Resolve Now")]
    public void TryResolve()
    {
        if (IsBusy) { return; }
        if(owner == null || owner.Pieces == null) { return; }

        TotalCombos = 0;
        TotalCleared = 0;
        ClearRounds = 0;

        RefreshLabel();

        // コルーチン開始
        IsBusy = true;
    }

    private IEnumerator ResolveRoutine()
    {
        const int maxClearRounds = 20;

        while (true)
        {
            bool[,] marks = PuzzleMatcher.Find(owner.Pieces, true);

            int count = PuzzleMatcher.Count(marks);

            if (count == 0)
            {
                break;
            }

            int combos = PuzzleComboCounter.Count(owner.Pieces, marks);

            TotalCombos += combos;
            RefreshLabel();

            Debug.Log($"第{ClearRounds + 1}回消去:" + $"対象{count}個/{combos}組");

            if(phaseWaite > 0f)
            {
                yield return new WaitForSecondsRealtime(phaseWaite);
            }

            
        }
    }

    /// <summary>
    /// コンボ数をUIに表示する
    /// </summary>
    private void RefreshLabel()
    {
        if(comboText  != null)
        {
            comboText.text = $"{TotalCombos} Combo";
        }
    }
}
