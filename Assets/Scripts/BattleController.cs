using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;
using NUnit.Framework.Internal.Commands;

//[RequireComponent (typeof(PuzzleBoard), typeof(PuzzleResolver))]
public class BattleController : MonoBehaviour
{
    public PuzzleBoard board;
    public TMP_Text statusText;
    public Slider partyHPBar;
    // public EnemyView[];
    public Image[] partyPortrats = new Image[3];
    public Button[] skillButtons = new Button[3];
    public TMP_Text[] skillSPLabels = new TMP_Text[3];
    private string failure;
    // private readonly System.Collections.Generic.HashSet<PuzzleResult> applied =
    private PuzzleResolver resolver;
    // public GameSession seesion;
    public event Action<bool> Finished;
    public bool Ended {  get; private set; }
    public bool InBattle {  get; private set; }
    public int[] SP {  get; private set; } = new int[3];
    public int[] PartyHP { get; private set; }
    //public int PartyMaxHP {  get; private set; }
    public int PartyMaxHP = 1000, enemyMaxHP = 500, attack = 100;
    private int hp, enemyHp;

    public int Turns {  get; private set; }
    public string BattledId {  get; private set; }

    // private BattleSetup battleSetup;
    private EnemyDate[] enemis;
    private int[] enemyHP, timers;
    private int[] attributeAttack = new int[3];
    // private OwnedCharcterData[] members = new OwnedCharcterData[3];

    private void OnEnable()
    {
        if(board == null || board.GetComponent<PuzzleResolver>() == null)
        {
            Debug.LogError("bord‚à‚µ‚­‚ÍPuzzleResolver‚ªƒAƒTƒCƒ“‚³‚ê‚Ä‚Ü‚¹‚ñ", this);
            enabled = false;
            return;
        }
        resolver = board.GetComponent<PuzzleResolver>();
        // resolver.Resolved += Apply;
        // resolver.Failed += OnFailed;
    }

    private void OnDisable()
    {
        if(board != null)
        {
            if(resolver != null)
            {
                // resolver.Resolved -= Apply;
                // resolver.Failed -= OnFailed;
                // resolver.InterrupResolution();
            }
            // board.InputLocked = true;
        }
    }

    protected virtual void Start()
    {
        // session = GameSession.Instance;
    }

    private void Apply(PuzzleResult result)
    {
        
    }
}
