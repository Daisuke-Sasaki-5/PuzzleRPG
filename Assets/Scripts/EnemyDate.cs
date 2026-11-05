using UnityEngine;

[CreateAssetMenu(menuName = "Course/Enemy")]
public class EnemyDate : ScriptableObject
{
    public Sprite portrait;
    public string enemyId, displayName;
    public PuzzleAttribute attribute;
    public int hp = 400, attack = 80, attackTurns = 2, experience = 20, level = 1;
}
