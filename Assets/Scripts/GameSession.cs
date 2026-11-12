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
    public string[] initialCharacterIds = new[]
    {
        "fire", "water", "wood"
    };
    public EnemyData[] demoEnemies;
    public List<OwnedCharacterData> owned = new List<OwnedCharacterData>();
    public PartyData party = new PartyData();
    public int coins, stamina = 30;
    protected virtual void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        NewGame();
    }
    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
    public void NewGame()
    {
        owned.Clear();
        coins = 0;
        stamina = 30;
        party = new PartyData();
        var seen = new HashSet<string>();
        foreach (var c in characters)
        {
            if (c == null || string.IsNullOrEmpty(c.characterId) || !seen.Add(c.characterId) || (int)c.attribute < 0 || (int)c.attribute > 2 || c.rarity < 0 || c.rarity > 3 || c.fusionExperience < 0 || c.maxLevel < 2 || c.minHP < 1 || c.maxHP < c.minHP || c.minAttack < 0 || c.maxAttack < c.minAttack) throw new InvalidOperationException("Missing/duplicate CharacterData ID");
        }
        if (initialCharacterIds == null || initialCharacterIds.Length != 3) throw new InvalidOperationException("èâä˙èäéùIDÇÕ3êl");
        var initialSeen = new HashSet<string>();
        foreach (string id in initialCharacterIds)
        {
            if (string.IsNullOrEmpty(id) || !initialSeen.Add(id) || FindMaster(id) == null) throw new InvalidOperationException("èâä˙èäéùIDÇ™ïsê≥Ç≈Ç∑");
            AddCharacter(id);
        }
        for (int i = 0; i < 3; i++) party.memberIds[i] = owned[i].instanceId;
        party.leaderId = party.memberIds[0];
    }
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
        return FindMaster(c.characterId).minHP;
    }
    public int Attack(OwnedCharacterData c)
    {
        return FindMaster(c.characterId).minAttack;
    }
    public bool ValidParty()
    {
        if (party == null || party.memberIds == null || party.memberIds.Length != 3) return false;
        var seen = new HashSet<string>();
        foreach (var id in party.memberIds) if (FindOwned(id) == null || !seen.Add(id)) return false;
        return party.leaderId == party.memberIds[0];
    }
}

