// キャラクターのマスターデータ。同じ種類を複数所有してもこの定義は共有します。
using UnityEngine;
[CreateAssetMenu(menuName="Course/Character")]
public class CharacterData : ScriptableObject
{
    public Sprite portrait;
    public string characterId;
    public string displayName;
    public PuzzleAttribute attribute;
    [Range(0,3)] public int rarity;
    public int minHP=250,maxHP=500,minAttack=80,maxAttack=200,maxLevel=20;
    public int fusionExperience=40;
}
