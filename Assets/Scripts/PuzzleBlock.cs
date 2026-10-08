using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

// ブロックの属性
public enum PuzzleAttribute
{
    Fire,  // 火
    Water, // 水
    Wood,  // 木
    Light, // 光
    Dark   // 闇
}

public class PuzzleBlock : MonoBehaviour
{
    // ブロックの種類
    public PuzzleAttribute Type {  get; private set; }

    // ブロックの画像
    [SerializeField] private Image blockImage;

    // 盤面上の座標
    public int X { get; private set; }
    public int Y { get; private set; }

    /// <summary>
    /// ブロックの初期化関数
    /// </summary>
    /// <param name="type">ブロックの種類</param>
    /// <param name="x">盤面上のX座標</param>
    /// <param name="y">盤面上のY座標</param>
    /// <param name="sprite">プロック画像</param>
    public void Initislizer(PuzzleAttribute type, int x, int y, Sprite sprite)
    {
        Type = type;
        X = x;
        Y = y;
        blockImage.sprite = sprite;
    }

    /// <summary>
    /// 座標を変更する
    /// </summary>
    /// <param name="x">盤面上のX座標</param>
    /// <param name="y">盤面上のY座標</param>
    public void SetPosition(int  x, int y)
    {
        X = x; Y = y;
    }
}
