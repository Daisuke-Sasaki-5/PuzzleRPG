using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(PuzzleBoard))]
public class PuzzleResolver : MonoBehaviour
{
    [SerializeField] private bool refillEnabled = true;
    [SerializeField] private bool repeatMatches = true;
    [SerializeField] private bool useFixedRefill = false;
    [SerializeField] private float phaseWait = .25f;
    [SerializeField] private float fallDuration = .12f;
    [SerializeField] private TMP_Text comboText;
    private PuzzleBoard owner;
    private int generation;
    public bool HasFailed
    {
        get;
        private set;
    }
    public string LastFailure
    {

        get;
        private set;
    }
    public event Action<PuzzleResult> Resolved;
    public event Action<string> Failed;
    private void Awake()
    {
        owner = GetComponent<PuzzleBoard>();
    }
    public void ResolveNow()
    {
        TryResolve();
    }

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

    public bool TryResolve()
    {
        if (owner == null) owner = GetComponent<PuzzleBoard>();
        if (!Application.isPlaying || !isActiveAndEnabled || !owner.isActiveAndEnabled || IsBusy || HasFailed || owner.Pieces == null || owner.IsOperating || owner.HasEmptyCells()) return false;
        if (Resolved == null || Resolved.GetInvocationList().Length != 1)
        {
            Fail("Exactly one Battle result consumer is required. Reset after reconnecting.");
            return false;
        }
        IsBusy = true;
        owner.InputLocked = true;
        TotalCombos = ClearRounds = TotalCleared = 0;

        StartCoroutine(GuardRoutine(ResolveRoutine(), ++generation));
        return true;
    }
    private IEnumerator ResolveRoutine()
    {
        var result = new PuzzleResult();
        if (!refillEnabled || !repeatMatches)
        {
            Fail("Lesson6+ requires refillEnabled and repeatMatches. Reset after fixing Inspector.");
            yield break;
        }
        while (true)
        {
            bool[,] marks = PuzzleMatcher.Find(owner.Pieces);
            if (PuzzleMatcher.Count(marks) == 0) break;
            if (result.ClearRounds >= 20)
            {

                Fail("Safety cap: unfinished chain. No result/reward/turn was committed. Reset battle.");
                yield break;
            }
            var wave = new PuzzleWave
            {
                Chain = result.ClearRounds + 1,
                Combos = PuzzleComboCounter.Count(owner.Pieces, marks)
            };
            for (int y = 0; y < 6; y++) for (int x = 0; x < 6; x++) if (marks[x, y]) wave.Counts[(int)owner.Pieces[x, y].Type]++;
            result.Waves.Add(wave);
            result.TotalCombos += wave.Combos;
            owner.RemoveMarked(marks);
            TotalCombos = result.TotalCombos;
            ClearRounds = result.ClearRounds;
            TotalCleared = result.TotalCleared;
            if (comboText != null) comboText.text = $"{TotalCombos} Combo / {ClearRounds} Chain";
            yield return new WaitForSecondsRealtime(Mathf.Max(0, phaseWait));
            owner.CollapsePieces();
            yield return owner.AnimateToCells(Mathf.Max(0, fallDuration));
            owner.RefillPieces(useFixedRefill);
            yield return owner.AnimateToCells(Mathf.Max(0, fallDuration));
        }
        IsBusy = false;
        if (Resolved == null || Resolved.GetInvocationList().Length != 1)
        {
            Fail("Result consumer was lost or duplicated. Reset battle.");
            yield break;
        }
        Resolved.Invoke(result);
    }

    private IEnumerator GuardRoutine(IEnumerator root, int stamp)
    {
        var stack = new Stack<IEnumerator>();
        stack.Push(root);
        while (stack.Count > 0 && !HasFailed && stamp == generation)
        {
            IEnumerator top = stack.Peek();
            object current = null;
            bool moved = false;
            Exception error = null;
            try
            {
                moved = top.MoveNext();
                if (moved) current = top.Current;
            }
            catch
        (Exception ex)
            {
                error = ex;
            }
            if (stamp != generation) yield break;
            if (error != null)
            {
                Fail("Resolution exception: " + error.Message);
                yield break;
            }
            if (!moved)
            {
                stack.Pop();
                continue;
            }
            if (current is IEnumerator child) stack.Push(child);
            else
                yield return current;
        }
    }
    private void Fail(string message)
    {
        if (HasFailed) return;
        HasFailed = true;
        LastFailure = message;
        IsBusy = false;
        if (owner != null) owner.InputLocked = true;
        Debug.LogError(message, this);
        if (Failed != null) foreach (Action<string> handler in Failed.GetInvocationList())
            {
                try
                {
                    handler(message);
                }
                catch
        (Exception ex)
                {
                    Debug.LogException(ex, this);
                }
            }
    }
    public void CancelResolution()
    {
        generation++;
        bool unfinished = IsBusy;
        StopAllCoroutines();
        IsBusy = false;
        if (owner != null) owner.InputLocked = true;
        if (unfinished) Fail("Resolution interrupted. No partial result applied. Reset battle.");
    }
    public void ResetResolutionState()
    {
        HasFailed = false;
        LastFailure = null;
    }
    private void OnDisable()
    {
        CancelResolution();
    }

}
