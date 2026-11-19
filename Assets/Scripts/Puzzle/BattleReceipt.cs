using System;
[Serializable]
public class BattleReceipt
{
    public string battleId;
    public bool won;
    public int coins, experience;
    public string[] characterIds = Array.Empty<string>();
    public string[] instanceIds = Array.Empty<string>();
}