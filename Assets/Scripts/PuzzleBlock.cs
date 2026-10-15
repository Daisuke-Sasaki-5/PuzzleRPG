using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// ブロックの属性
public enum PuzzleAttribute
{
    Fire,  // 火
    Water, // 水
    Wood,  // 木
    Light, // 光
    Dark   // 闇
}

public class PuzzleBlock : MonoBehaviour, IBeginDragHandler,IDragHandler, IEndDragHandler
{
    // ブロックの種類
    public PuzzleAttribute Type { get; private set; }

    // ブロックの画像
    [SerializeField] private Image blockImage;

    // 盤面上の座標
    public int X { get; private set; }
    public int Y { get; private set; }

    // この石が所属する盤面
    private PuzzleBoard puzzleBoard;

    // 自分のRectTransform
    private RectTransform rectTransform;

    // ドラッグ開始時の表示
    private Vector2 startPosition;

    // ドラッグ開始時のポインター位置
    private Vector2 startPointerPostion;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    /// <summary>
    /// 石の初期情報を設定する
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="type"></param>
    /// <param name="sprite"></param>
    public void Setup(int x, int y, PuzzleAttribute type, Sprite sprite)
    {
        X = x;
        Y = y;
        Type = type;
        blockImage.sprite = sprite;
    }

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
    public void SetPosition(int x, int y)
    {
        X = x; Y = y;
    }

    /// <summary>
    /// 所属する盤面を設定する
    /// </summary>
    /// <param name="board"></param>
    public void SetBoard(PuzzleBoard board)
    {
        puzzleBoard = board;
    }

    /// <summary>
    /// ドラッグ開始
    /// </summary>
    /// <param name="eventData"></param>
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (puzzleBoard == null) { return; }

        startPosition = rectTransform.anchoredPosition;

        // 画面上のポインター位置をUI座標に変換する
        RectTransformUtility.ScreenPointToLocalPointInRectangle(puzzleBoard.PiecesRoot, eventData.position, eventData.pressEventCamera, out startPointerPostion);
    }

    /// <summary>
    /// ドラッグ中
    /// </summary>
    /// <param name="eventData"></param>
    public void OnDrag(PointerEventData eventData)
    {
        if (puzzleBoard == null) { return; }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(puzzleBoard.PiecesRoot, eventData.position, eventData.pressEventCamera, out Vector2 currentPointerPosition);

        // 指移動量を求める
        Vector2 delta = currentPointerPosition - startPointerPostion;

        // 石の指の動きに合わせて移動する
        rectTransform.anchoredPosition = startPointerPostion + delta;
    }

    /// <summary>
    /// ドラッグ終了
    /// </summary>
    /// <param name="eventData"></param>
    public void OnEndDrag(PointerEventData eventData)
    {
        if (puzzleBoard == null) { return; }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(puzzleBoard.PiecesRoot, eventData.position, eventData.pressEventCamera, out Vector2 endPointerPosition);

        Vector2 delta = endPointerPosition - startPointerPostion;

        // 盤面に交換処理を依頼する
        bool swapped = puzzleBoard.TrySwap(this, delta);

        // 交換できなかった場合は元の位置に戻す
        if(!swapped)
        {
            rectTransform.anchoredPosition = startPointerPostion;
        }
    }
}
