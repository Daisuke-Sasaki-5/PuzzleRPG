using System;
using System.Collections.Generic;

[Serializable]
public sealed class PuzzleWave
{
    public int[] Counts = new int[5];
    public int Chain;
    public int Combos;
}

[Serializable]
public sealed class PuzzleResult
{
    public List<PuzzleWave> Waves = new List<PuzzleWave>();
    public int TotalCombos;
    public int ClearRounds => Waves.Count;
    public int TotalCleared
    {
        get
        {
            int n = 0;
            foreach (var w in Waves)
            {
                foreach (int c in w.Counts)
                {
                    n += c;
                }
            }
            return n;
        }
    }
}
