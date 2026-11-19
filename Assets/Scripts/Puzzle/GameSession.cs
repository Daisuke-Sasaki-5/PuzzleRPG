// シーンをまたぐ所持・編成・進行データの窓口です。
// 第8以降は既存Instanceを優先し、各シーンの複製を破棄します。
using System;
using System.Collections.Generic;
using UnityEngine;
public class GameSession : MonoBehaviour
{
    public static GameSession Instance
    {
        get;
        private set;
    }
    public CharacterData[] characters;
    // IDs used by the lesson-07 character assets in this repository.
    public string[] initialCharacterIds = new[]
    {
        "lesson07-01", "lesson07-02", "lesson07-03"
    };
    public EnemyData[] demoEnemies;
    public List<OwnedCharacterData> owned = new List<OwnedCharacterData>();
    public PartyData party = new PartyData();
    public int coins, stamina = 30;
    public int rewardBaseCoins = 100;
    public string[] rewardCharacterIds = { "lesson07-01", "lesson07-02", "lesson07-03" };
    [NonSerialized] public BattleReceipt receipt;
    [NonSerialized] public readonly Dictionary<string, BattleReceipt> receipts = new Dictionary<string, BattleReceipt>();
    protected virtual void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        NewGame();
    }
    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
    // 教材の初期所持と編成を作成。マスターID/属性やステージ定義も検証します。
    public void NewGame()
    {
        owned.Clear();
        receipts.Clear();
        receipt = null;
        coins = 0;
        stamina = 30;
        party = new PartyData();
        var seen = new HashSet<string>();
        foreach (var c in characters)
        {
            if (c == null || string.IsNullOrEmpty(c.characterId) || !seen.Add(c.characterId) || (int)c.attribute < 0 || (int)c.attribute > 2 || c.rarity < 0 || c.rarity > 3 || c.fusionExperience < 0 || c.maxLevel < 2 || c.minHP < 1 || c.maxHP < c.minHP || c.minAttack < 0 || c.maxAttack < c.minAttack)
                throw new InvalidOperationException("Missing/duplicate CharacterData ID");
            AddCharacter(c.characterId);
        }
        if (owned.Count < 3) throw new InvalidOperationException("Need at least three characters");
        for (int i = 0; i < 3; i++) party.memberIds[i] = owned[i].instanceId;
        party.leaderId = party.memberIds[0];
    }
    // 同種キャラでも新しい所持個体IDを発行し、個体重複を防ぎます。
    public OwnedCharacterData AddCharacter(string id)
    {
        if (FindMaster(id) == null) throw new ArgumentException("Unknown character ID");
        var item = new OwnedCharacterData
        {
            characterId = id,
            instanceId = Guid.NewGuid().ToString("N")
        };
        owned.Add(item);
        return item;
    }
    public CharacterData FindMaster(string id) => Array.Find(characters, c => c != null && c.characterId == id);
    public OwnedCharacterData FindOwned(string id) => owned.Find(c => c.instanceId == id);
    public int MaxHP(OwnedCharacterData c)
    {
        var m = FindMaster(c.characterId);
        return BattleMath.Growth(m.minHP, m.maxHP, Math.Min(c.level, m.maxLevel), m.maxLevel) + Math.Max(0, c.level - m.maxLevel) * (m.maxHP - m.minHP) / (m.maxLevel - 1);
    }
    public int Attack(OwnedCharacterData c)
    {
        var m = FindMaster(c.characterId);
        return BattleMath.Growth(m.minAttack, m.maxAttack, Math.Min(c.level, m.maxLevel), m.maxLevel) + Math.Max(0, c.level - m.maxLevel) * (m.maxAttack - m.minAttack) / (m.maxLevel - 1);
    }
    // 3枠すべてが所持個体を参照し、同じ個体が重複していないことを確認します。
    public bool ValidParty()
    {
        if (party == null || party.memberIds == null || party.memberIds.Length != 3) return false;
        var seen = new HashSet<string>();
        foreach (var id in party.memberIds) if (FindOwned(id) == null || !seen.Add(id)) return false;
        return party.leaderId == party.memberIds[0];
    }
}