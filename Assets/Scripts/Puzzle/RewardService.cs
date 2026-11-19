using System;
using System.Collections.Generic;
public static class RewardService
{
    private static readonly Random random = new Random();
    public static BattleReceipt Commit(GameSession s, string battleId, bool won, EnemyData[] enemies = null)
    {
        if (s == null || string.IsNullOrEmpty(battleId)) throw new ArgumentException("Session/battle ID missing");
        if (s.receipts.TryGetValue(battleId, out var existing)) return existing;
        var receipt = new BattleReceipt { battleId = battleId, won = won };
        var additions = new List<OwnedCharacterData>();
        if (won)
        {
            if (s.rewardBaseCoins < 0 || s.rewardCharacterIds == null || s.rewardCharacterIds.Length == 0)
                throw new InvalidOperationException("Invalid reward configuration");
            foreach (var id in s.rewardCharacterIds) if (s.FindMaster(id) == null) throw new InvalidOperationException("Unknown reward candidate");
            enemies = enemies ?? s.demoEnemies;
            if (enemies == null) throw new InvalidOperationException("Missing enemies");
            checked { foreach (var e in enemies) { if (e == null || e.experience < 0) throw new InvalidOperationException("Invalid enemy EXP"); receipt.experience += e.experience; } }
            int count = random.Next(1, 6);
            receipt.characterIds = new string[count]; receipt.instanceIds = new string[count];
            for (int i = 0; i < count; i++)
            {
                string id = s.rewardCharacterIds[random.Next(s.rewardCharacterIds.Length)];
                var item = new OwnedCharacterData { characterId = id, instanceId = Guid.NewGuid().ToString("N") };
                additions.Add(item); receipt.characterIds[i] = id; receipt.instanceIds[i] = item.instanceId;
                checked { receipt.coins += (int)Math.Floor(s.rewardBaseCoins * (.8 + random.NextDouble() * .4)); }
            }
        }
        // Validate and stage first: failed calculation must not leave partially granted items.
        int total = checked(s.coins + receipt.coins);
        s.owned.AddRange(additions); s.coins = total;
        s.receipts.Add(battleId, receipt); s.receipt = receipt;
        return receipt;
    }
}