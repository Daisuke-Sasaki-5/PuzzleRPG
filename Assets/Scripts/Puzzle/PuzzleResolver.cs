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
    [SerializeField] private float fallDuration = .18f;
    [SerializeField] private TMP_Text comboText;
    private PuzzleBoard owner;
    private int generation;
    public bool IsBusy { get; private set; }
    public bool NeedsRestart { get; private set; }
    // Compatibility with the current lesson slides; both names share one state.
    public bool HasFailed
    {
        get
        {
            return NeedsRestart;
        }
    }

    public void CancelResolution()
    {
        InterruptResolution();
    }

    public bool BlocksInput => IsBusy || NeedsRestart;
    public int TotalCombos { get; private set; }
    public int ClearRounds { get; private set; }
    public int TotalCleared { get; private set; }
    public event Action<PuzzleResult> Resolved;
    public event Action<string> Failed;
    private void Awake() { owner = GetComponent<PuzzleBoard>(); }
    [ContextMenu("Resolve Now")]
    public void ResolveNow() { TryResolve(); }
    public bool TryResolve()
    {
        if (owner == null) owner = GetComponent<PuzzleBoard>();
        if (!Application.isPlaying || !isActiveAndEnabled || !owner.isActiveAndEnabled ||
            owner.Pieces == null || owner.IsOperating || owner.InputLocked) return false;
        if (owner.HasEmptyCells()) { Fail("Incomplete board. Reset battle."); return false; }
        IsBusy = true;
        owner.InputLocked = true;
        TotalCombos = ClearRounds = TotalCleared = 0;
        RefreshLabel();
        if (!refillEnabled || !repeatMatches || Resolved == null || Resolved.GetInvocationList().Length != 1)
        {
            Fail("Lesson 6 requires refill/repeat and exactly one result consumer. Reset battle.");
            return false;
        }
        StartCoroutine(RunSafely(ResolveRoutine(), ++generation));
        return true;
    }
    // Unity does not propagate exceptions in nested yielded enumerators to the caller.
    // Drive the whole iterator stack here so failures cannot silently unlock a partial board.
    private IEnumerator RunSafely(IEnumerator routine, int token)
    {
        var stack = new Stack<IEnumerator>();
        stack.Push(routine);
        while (stack.Count > 0 && token == generation && IsBusy)
        {
            object current = null;
            bool moved = false;
            Exception error = null;
            try { moved = stack.Peek().MoveNext(); if (moved) current = stack.Peek().Current; }
            catch (Exception ex) { error = ex; }
            if (error != null) { Fail("Resolution exception: " + error.Message + ". Reset battle."); yield break; }
            if (!moved) { (stack.Pop() as IDisposable)?.Dispose(); continue; }
            if (current is IEnumerator child) stack.Push(child);
            else yield return current;
        }
    }
    private IEnumerator ResolveRoutine()
    {
        var result = new PuzzleResult();
        GetComponent<PuzzleMatchPreview>()?.ClearHighlights();
        while (true)
        {
            bool[,] marks = PuzzleMatcher.Find(owner.Pieces, true);
            int count = PuzzleMatcher.Count(marks);
            if (count == 0) break;
            if (ClearRounds >= 20) { Fail("20-wave cap with remaining matches. Reset battle."); yield break; }
            var wave = new PuzzleWave
            {
                Chain = result.ClearRounds + 1,
                Combos = PuzzleComboCounter.Count(owner.Pieces, marks)
            };
            for (int y = 0; y < owner.Pieces.GetLength(1); y++)
                for (int x = 0; x < owner.Pieces.GetLength(0); x++)
                    if (marks[x, y]) wave.Counts[(int)owner.Pieces[x, y].Type]++;
            result.Waves.Add(wave);
            result.TotalCombos += wave.Combos;
            owner.RemoveMarked(marks);
            TotalCombos = result.TotalCombos;
            ClearRounds = result.ClearRounds;
            TotalCleared = result.TotalCleared;
            RefreshLabel();
            Debug.Log($"Wave {wave.Chain}: Counts=[{string.Join(",", wave.Counts)}], Combos={wave.Combos}", this);
            yield return new WaitForSecondsRealtime(Mathf.Max(0f, phaseWait));
            owner.CollapsePieces();
            yield return owner.AnimateToCells(Mathf.Max(0f, fallDuration));
            owner.RefillPieces(useFixedRefill);
            yield return owner.AnimateToCells(Mathf.Max(0f, fallDuration));
            yield return new WaitForSecondsRealtime(Mathf.Max(0f, phaseWait));
        }
        if (owner.HasEmptyCells()) { Fail("Incomplete final board. Reset battle."); yield break; }
        if (Resolved == null || Resolved.GetInvocationList().Length != 1) { Fail("Result consumer missing or duplicated. Reset battle."); yield break; }
        IsBusy = false;
        Debug.Log($"Resolved once: cleared={TotalCleared}, combo={TotalCombos}, waves={ClearRounds}", this);
        Resolved.Invoke(result);
    }
    private void RefreshLabel() { if (comboText != null) comboText.text = $"{TotalCombos} Combo"; }
    private void Fail(string message)
    {
        IsBusy = false;
        NeedsRestart = true;
        if (owner != null) owner.InputLocked = true;
        Debug.LogWarning(message, this);
        if (Failed != null) foreach (Action<string> handler in Failed.GetInvocationList())
            { try { handler(message); } catch (Exception ex) { Debug.LogException(ex, this); } }
    }
    public void InterruptResolution()
    {
        if (!IsBusy) return;
        generation++;
        StopAllCoroutines();
        Fail("Resolution interrupted. Reset battle.");
    }
    public void ResetState()
    {
        generation++;
        StopAllCoroutines();
        IsBusy = false;
        NeedsRestart = false;
        TotalCombos = ClearRounds = TotalCleared = 0;
        RefreshLabel();
    }
    private void OnDisable() { InterruptResolution(); }
}