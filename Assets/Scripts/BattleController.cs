using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class BattleController : MonoBehaviour
{
    public PuzzleBoard board;
    public TMP_Text statusText;
    public Slider partyHPBar;
    public EnemyView[] enemyViews = new EnemyView[3];
    public Image[] partyPortraits = new Image[3];
    public Button[] skillButtons = new Button[3];
    public TMP_Text[] skillSPLabels = new TMP_Text[3];
    public bool IsPresenting { get; private set; }
    private BattleAttackMotion motion;
    private int presentationGeneration;
    private readonly HashSet<PuzzleResult> applied = new
        HashSet<PuzzleResult>();

    public string FailureMessage
    {
        get;
        private set;
    }
    private PuzzleResolver resolver;
    public GameSession session;
    public event Action<bool> Finished;
    public bool Ended
    {
        get;
        private set;
    }
    public bool InBattle
    {
        get;
        private set;
    }
    public int[] SP
    {
        get;
        private set;
    }
    = new int[3];
    public int PartyHP
    {
        get;
        private set;
    }
    public int PartyMaxHP
    {
        get;
        private set;
    }
    public int Turns
    {
        get;
        private set;
    }
    public string BattleId
    {
        get;
        private set;
    }
    private EnemyData[] enemies;
    private int[] enemyHP, timers;
    private int[] attributeAttack = new int[3];
    private OwnedCharacterData[] members = new OwnedCharacterData[3];

    private sealed class TurnCue
    {
        public int Attribute, Enemy, PartyHP;
        public bool Healing;
        public int[] Targets, Amounts, EnemyHP, Timers;
    }
    private Image EnemyPortrait(int index)
    {
        return enemyViews != null && index >= 0 && index < enemyViews.Length &&
            enemyViews[index] != null ? enemyViews[index].portrait : null;
    }
    private Image[] PartyAttackers(int attribute)
    {
        var images = new List<Image>();
        for (int i = 0; i < members.Length; i++)
            if ((attribute == 4 ||
                (int)session.FindMaster(members[i].characterId).attribute ==
                    attribute) &&
                partyPortraits != null && i < partyPortraits.Length &&
                partyPortraits[i] != null)
                images.Add(partyPortraits[i]);
        return images.ToArray();
    }
    private IEnumerator PresentCue(TurnCue cue)
    {
        bool enemyAttack = cue.Enemy >= 0;
        var attackers = cue.Healing ? Array.Empty<Image>() :
            enemyAttack ? new[] { EnemyPortrait(cue.Enemy) } :
            PartyAttackers(cue.Attribute);
        var victims = enemyAttack || cue.Healing ?
            (partyPortraits ?? Array.Empty<Image>()) :
            Array.ConvertAll(cue.Targets, EnemyPortrait);
        var anchors = enemyAttack || cue.Healing ?
            new[] { partyHPBar != null ? (RectTransform)partyHPBar.transform :
            null } :
            Array.ConvertAll(victims, image => image != null ?
                image.rectTransform : null);
        yield return motion.Play(attackers, victims, anchors, cue.Amounts,
            enemyAttack, cue.Healing, () =>
            {
                if (partyHPBar != null) partyHPBar.value = cue.PartyHP;
                for (int i = 0; i < enemies.Length; i++)
                    if (enemyViews != null && i < enemyViews.Length &&
                    enemyViews[i] != null)
                        enemyViews[i].Show(enemies[i], cue.EnemyHP[i],
                        cue.Timers[i]);
            });
    }
    private IEnumerator PresentTurn(List<TurnCue> cues, PuzzleResult result,
        int[] nextEnemyHP, int[] nextTimers, int nextPartyHP, int token)
    {
        var stack = new Stack<IEnumerator>();
        foreach (var cue in cues)
        {
            stack.Push(PresentCue(cue));
            while (stack.Count > 0)
            {
                if (token != presentationGeneration || !isActiveAndEnabled)
                    yield break;
                if (board == null || !board.isActiveAndEnabled || resolver ==
                    null ||
                    !resolver.isActiveAndEnabled || resolver.HasFailed)
                {
                    OnFailed("Attack presentation interrupted.");
                    yield break;
                }
                bool moved = false;
                object current = null;
                Exception error = null;
                try
                {
                    moved = stack.Peek().MoveNext();
                    if (moved) current = stack.Peek().Current;
                }
                catch (Exception ex) { error = ex; }
                if (error != null)
                {
                    OnFailed("Attack presentation failed: " + error.Message);
                    Debug.LogWarning(error, this);
                    yield break;
                }
                if (!moved)
                {
                    (stack.Pop() as IDisposable)?.Dispose();
                    continue;
                }
                if (current is IEnumerator child) stack.Push(child);
                else yield return current;
            }
        }
        if (token != presentationGeneration || !isActiveAndEnabled) yield
            break;
        if (board == null || !board.isActiveAndEnabled || resolver == null ||
            !resolver.isActiveAndEnabled || resolver.HasFailed)
        {
            OnFailed("Attack presentation interrupted.");
            yield break;
        }
        motion.Restore();
        motion = null;
        IsPresenting = false;
        applied.Add(result);
        enemyHP = nextEnemyHP;
        timers = nextTimers;
        PartyHP = nextPartyHP;
        Turns++;
        for (int i = 0; i < 3; i++) SP[i] = Mathf.Min(100, SP[i] + 20);
        if (Target() < 0) End(true);
        else if (PartyHP == 0) End(false);
        else board.InputLocked = false;
        Refresh();
    }
    private void CancelPresentation()
    {
        presentationGeneration++;
        StopAllCoroutines();
        motion?.Restore();
        motion = null;
        IsPresenting = false;
    }

    private void OnEnable()
    {
        if (!Application.isPlaying) return;
        if (board == null || board.GetComponent<PuzzleResolver>() == null)
        {
            Debug.LogError("Assign BattleController.board and add PuzzleResolver.", this);
            enabled = false;
            return;
        }
        resolver = board.GetComponent<PuzzleResolver>();
        resolver.Resolved += Apply;
        resolver.Failed += OnFailed;

    }
    private void OnDisable()
    {
        bool interrupted = IsPresenting;
        CancelPresentation();
        if (interrupted)
        {
            FailureMessage = "処理を停止しました。リセットしてください。";
            if (enemies != null) Refresh();
        }
        if (resolver != null)
        {
            resolver.Resolved -= Apply;
            resolver.Failed -= OnFailed;
            if (resolver.IsBusy) resolver.CancelResolution();
        }
        resolver = null;
        if (board != null) board.InputLocked = true;
    }
private void Start()
    {
        BeginBattle(session.demoEnemies);
    }

    private void Update()
    {
        for (int i = 0; i < 3; i++) if (skillButtons != null && i < skillButtons.Length && skillButtons[i] != null) skillButtons[i].interactable = InBattle && !Ended && board != null && !board.IsOperating && !board.InputLocked && SP[i] >= 100;
    }


    public void ResetBattle()
    {
        BeginBattle(enemies ?? session.demoEnemies);
    }
    private void Apply(PuzzleResult result)
    {
        if (!isActiveAndEnabled || !InBattle || Ended || IsPresenting ||
            FailureMessage != null || resolver == null ||
                !resolver.isActiveAndEnabled ||
            !board.isActiveAndEnabled || resolver.HasFailed) return;
        if (applied.Contains(result)) return;
        board.InputLocked = true;
        if (result == null || result.Waves == null)
            throw new InvalidOperationException("Missing result");
        int[] nextEnemyHP = (int[])enemyHP.Clone();
        int[] nextTimers = (int[])timers.Clone();
        int nextPartyHP = PartyHP;
        var cues = new List<TurnCue>();
        int NextTarget()
        {
            for (int i = 0; i < nextEnemyHP.Length; i++)
                if (nextEnemyHP[i] > 0) return i;
            return -1;
        }
        void AddCue(int attribute, int enemy, int[] targets, int[] amounts,
            bool healing = false)
        {
            cues.Add(new TurnCue
            {
                Attribute = attribute,
                Enemy = enemy,
                Targets = targets,
                Amounts = amounts,
                Healing = healing,
                EnemyHP = (int[])nextEnemyHP.Clone(),
                Timers = (int[])nextTimers.Clone(),
                PartyHP = nextPartyHP
            });
        }
        foreach (var wave in result.Waves)
        {
            if (wave == null || wave.Counts == null ||
                wave.Counts.Length != 5 || wave.Chain < 1)
                throw new InvalidOperationException("Invalid wave");
            foreach (int count in wave.Counts)
                if (count < 0) throw new
                    InvalidOperationException("Negative count");
            for (int a = 0; a < 3; a++)
            {
                int target = NextTarget();
                if (target < 0) break;
                int damage = BattleMath.NormalDamage(wave.Counts[a],
                    attributeAttack[a], (PuzzleAttribute)a,
                    enemies[target].attribute, wave.Chain);
                int dealt = Mathf.Min(nextEnemyHP[target], damage);
                nextEnemyHP[target] = Mathf.Max(0, nextEnemyHP[target] -
                    damage);
                if (damage > 0) AddCue(a, -1, new[] { target }, new[] { dealt
                });
            }
            int dark = BattleMath.DarkDamage(wave.Counts[4],
                attributeAttack[0] + attributeAttack[1] + attributeAttack[2],
                    wave.Chain);
            var targets = new List<int>();
            var amounts = new List<int>();
            for (int i = 0; i < nextEnemyHP.Length; i++)
                if (nextEnemyHP[i] > 0 && dark > 0)
                {
                    targets.Add(i);
                    amounts.Add(Mathf.Min(nextEnemyHP[i], dark));
                    nextEnemyHP[i] = Mathf.Max(0, nextEnemyHP[i] - dark);
                }
            if (targets.Count > 0)
                AddCue(4, -1, targets.ToArray(), amounts.ToArray());
            int beforeHeal = nextPartyHP;
            nextPartyHP = Mathf.Min(PartyMaxHP,
                nextPartyHP + BattleMath.Heal(wave.Counts[3], PartyMaxHP));
            if (nextPartyHP > beforeHeal)
                AddCue(3, -1, Array.Empty<int>(), new[] { nextPartyHP -
                beforeHeal }, true);
        }
        if (NextTarget() >= 0)
            for (int i = 0; i < enemies.Length; i++)
                if (nextEnemyHP[i] > 0 && --nextTimers[i] <= 0)
                {
                    bool canShowAttack = nextPartyHP > 0;
                    int dealt = Mathf.Min(nextPartyHP, enemies[i].attack);
                    nextPartyHP = Mathf.Max(0, nextPartyHP -
                        enemies[i].attack);
                    nextTimers[i] = enemies[i].attackTurns;
                    if (canShowAttack)
                        AddCue(-1, i, Array.Empty<int>(), new[] { dealt });
                }
        IsPresenting = true;
        motion = new BattleAttackMotion(statusText != null ? statusText.font :
            null);
        StartCoroutine(PresentTurn(cues, result, nextEnemyHP, nextTimers,
            nextPartyHP, ++presentationGeneration));
    }
private void OnFailed(string message)
    {
        CancelPresentation();
        FailureMessage = message;
        if (board != null) board.InputLocked = true;
        if (enemies != null) Refresh();
        else if (statusText != null) statusText.text = "STOP: " + message;

    }
    private void Refresh()
    {
        if (partyHPBar != null)
        {
            partyHPBar.maxValue = PartyMaxHP;
            partyHPBar.value = PartyHP;
        }
        for (int i = 0; i < enemies.Length; i++) if (enemyViews != null && i < enemyViews.Length && enemyViews[i] != null) enemyViews[i].Show(enemies[i], enemyHP[i], timers[i]);
        for (int i = 0; i < 3; i++) if (partyPortraits != null && i < partyPortraits.Length && partyPortraits[i] != null)
            {
                var portrait = session.FindMaster(members[i].characterId).portrait;
                if (portrait != null) partyPortraits[i].sprite = portrait;
                partyPortraits[i].preserveAspect = true;
            }
        for (int i = 0; i < 3; i++) if (skillSPLabels != null && i < skillSPLabels.Length && skillSPLabels[i] != null) skillSPLabels[i].text = $"SP {SP[i]}%";
        if (statusText == null) return;
        string text = $"Turn {Turns}  HP {PartyHP}/{PartyMaxHP}\nSP {SP[0]} / {SP[1]} / {SP[2]}\n";
        for (int i = 0; i < enemies.Length; i++) text += $"{enemies[i].displayName}: {enemyHP[i]} ({timers[i]})  ";
        statusText.text = text + (Ended ? (PartyHP > 0 ? " WIN" : " LOSE") : "") + (string.IsNullOrEmpty(FailureMessage) ? "" : "\nSTOP: " + FailureMessage);
    }

public void BeginBattle(EnemyData[] list)
    {
        if (!session.ValidParty() || list == null || list.Length != 3) throw
            new
            InvalidOperationException("Party/enemies invalid");
        foreach (var e in list) if (e == null || e.hp <= 0 || e.attackTurns < 1
            || e.attack
            < 0 || e.experience < 0) throw new
                InvalidOperationException("Invalid enemy");
        CancelPresentation();
        board.ResetBoard();
        applied.Clear();
        FailureMessage = null;
        BattleId = Guid.NewGuid().ToString("N");
        Ended = false;
        InBattle = true;
        Turns = 0;
        PartyMaxHP = 0;
        SP = new int[3];
        attributeAttack = new int[3];
        for (int i = 0; i < 3; i++)
        {
            members[i] = session.FindOwned(session.party.memberIds[i]);
            var master = session.FindMaster(members[i].characterId);
            PartyMaxHP += session.MaxHP(members[i]);
            attributeAttack[(int)master.attribute] +=
                session.Attack(members[i]);
        }
        PartyHP = PartyMaxHP;
        enemies = (EnemyData[])list.Clone();
        enemyHP = new int[list.Length];
        timers = new int[list.Length];
        for (int i = 0; i < list.Length; i++)
        {
            enemyHP[i] = list[i].hp;
            timers[i] = list[i].attackTurns;
        }
        board.InputLocked = false;
        Refresh();
    }
    private int Target()
    {
        for (int i = 0; i < enemyHP.Length; i++) if (enemyHP[i] > 0) return i;
        return -1;
    }

    public void UseSkill(int index)
    {
        if (!InBattle || Ended || board.IsOperating || board.InputLocked || index < 0 || index >= 3 || SP[index] < 100) return;
        int target = Target();
        if (target < 0) return;
        var m = session.FindMaster(members[index].characterId);
        int damage = (int)((long)session.Attack(members[index]) * (6 + m.rarity) * (BattleMath.Affinity(m.attribute, enemies[target].attribute) > 1f ? 6 : 5) / 25);
        SP[index] = 0;
        enemyHP[target] = Mathf.Max(0, enemyHP[target] - damage);
        if (Target() < 0) End(true);
        Refresh();
    }
    private void End(bool won)
    {
        if (Ended) return;
        Ended = true;
        InBattle = false;
        board.InputLocked = true;
        Finished?.Invoke(won);
    }

}
