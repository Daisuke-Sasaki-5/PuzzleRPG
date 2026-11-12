using System;
public static class BattleMath
{
    public static float CountMultiplier(int count) => count < 3 ? 0f : (count + 2) / 5f;
    public static int Damage(int count, int attack) => count < 3 ? 0 : (int)((long)attack * (count + 2) / 5);
    public static int Heal(int count, int maxHP) => count < 3 ? 0 : (int)((long)maxHP * (count + 2) / 25);
    public static int Dark(int count, int partyAttack) => DarkDamage(count, partyAttack, 1);
    private static bool Advantage(PuzzleAttribute a, PuzzleAttribute e) => (a == PuzzleAttribute.Fire && e == PuzzleAttribute.Wood) || (a == PuzzleAttribute.Water && e == PuzzleAttribute.Fire) || (a == PuzzleAttribute.Wood && e == PuzzleAttribute.Water);
    public static float Affinity(PuzzleAttribute a, PuzzleAttribute e) => Advantage(a, e) ? 1.2f : 1f;
    public static float ChainBonus(int chain) => (Math.Max(1, chain) + 3) / 4f;
    public static int NormalDamage(int count, int attack, PuzzleAttribute a, PuzzleAttribute e, int chain) => count < 3 ? 0 : (int)((long)attack * (count + 2) * (Advantage(a, e) ? 6 : 5) * (Math.Max(1, chain) + 3) / 100);
    public static int DarkDamage(int count, int partyAttack, int chain) => count < 3 ? 0 : (int)((long)partyAttack * (1 + Math.Min(5, count - 3)) * (Math.Max(1, chain) + 3) / 80);
}
