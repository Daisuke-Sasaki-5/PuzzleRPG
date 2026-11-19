using UnityEngine;
using UnityEngine.UI;
public enum MatchTestPattern
{
    None, Horizontal3, Vertical3, Horizontal4, Horizontal5, Horizontal6,
    LShape, TShape, Edge2, RowWrap, Separate3
}
[RequireComponent(typeof(PuzzleBoard))]
public class PuzzleMatchPreview : MonoBehaviour
{
    [SerializeField] private bool includeVertical = false;
    [SerializeField] private MatchTestPattern testPattern;
    [SerializeField] private Sprite[] attributeSprites = new Sprite[5];
    private PuzzleBoard owner;
    private void Awake() { owner = GetComponent<PuzzleBoard>(); }
    public void ClearHighlights()
    {
        if (owner == null) owner = GetComponent<PuzzleBoard>();
        PuzzlePiece[,] board = owner.Pieces;
        if (board == null) return;
        foreach (PuzzlePiece piece in board)
            if (piece != null)
                piece.GetComponent<Image>().color = Color.white;
    }
    [ContextMenu("Show Matches")]
    public void ShowMatches()
    {
        if (owner == null) owner = GetComponent<PuzzleBoard>();
        if (!Application.isPlaying || owner.Pieces == null ||
            owner.IsOperating) return;
        PuzzlePiece[,] board = owner.Pieces;
        bool[,] marks = PuzzleMatcher.Find(board, includeVertical);
        ClearHighlights();
        for (int y = 0; y < board.GetLength(1); y++)
            for (int x = 0; x < board.GetLength(0); x++)
                if (marks[x, y] && board[x, y] != null)
                {
                    board[x, y].GetComponent<Image>().color =
                        new Color(1f, 0.65f, 0.65f, 1f);
                    Debug.Log($"対象 ({x},{y}) {board[x,y].Type}", this);
                }
        Debug.Log($"対象の石 {PuzzleMatcher.Count(marks)}個", this);
    }
    [ContextMenu("Apply Test Pattern")]
    private void ApplyTestPattern()
    {
        if (owner == null) owner = GetComponent<PuzzleBoard>();
        PuzzlePiece[,] board = owner.Pieces;
        if (!Application.isPlaying || board == null || owner.IsOperating || owner.InputLocked)
            return;
        if (board.GetLength(0) != 6 || board.GetLength(1) != 6)
        {
            Debug.LogWarning("テスト盤面は6行6列に戻してください", this);
            return;
        }
        if (attributeSprites == null || attributeSprites.Length != 5)
            return;
        for (int i = 0; i < 5; i++)
            if (attributeSprites[i] == null) return;
        for (int y = 0; y < 6; y++)
            for (int x = 0; x < 6; x++)
                if (board[x, y] == null) return;
        // 火を含まない、縦横とも3個並ばない下地。
        for (int y = 0; y < 6; y++)
            for (int x = 0; x < 6; x++)
            {
                int index = (x + y * 2) % 4 + 1;
                board[x, y].Setup(x, y, (PuzzleAttribute)index,
                    attributeSprites[index]);
            }
        Vector2Int[] cells;
        switch (testPattern)
        {
            case MatchTestPattern.Horizontal3:
                cells = new[] { P(1,2), P(2,2), P(3,2) }; break;
            case MatchTestPattern.Vertical3:
                cells = new[] { P(2,1), P(2,2), P(2,3) }; break;
            case MatchTestPattern.Horizontal4:
                cells = new[] { P(1,2), P(2,2), P(3,2), P(4,2) }; break;
            case MatchTestPattern.Horizontal5:
                cells = new[] { P(0,2), P(1,2), P(2,2),
                    P(3,2), P(4,2) }; break;
            case MatchTestPattern.Horizontal6:
                cells = new[] { P(0,2), P(1,2), P(2,2),
                    P(3,2), P(4,2), P(5,2) }; break;
            case MatchTestPattern.LShape:
                cells = new[] { P(1,1), P(2,1), P(3,1),
                    P(1,2), P(1,3) }; break;
            case MatchTestPattern.TShape:
                cells = new[] { P(1,1), P(2,1), P(3,1),
                    P(2,2), P(2,3) }; break;
            case MatchTestPattern.Edge2:
                cells = new[] { P(4,0), P(5,0) }; break;
            case MatchTestPattern.RowWrap:
                cells = new[] { P(4,0), P(5,0), P(0,1) }; break;
            case MatchTestPattern.Separate3:
                cells = new[] { P(0,1), P(1,1), P(2,1),
                    P(4,2), P(4,3), P(4,4) }; break;
            default:
                cells = new Vector2Int[0]; break;
        }
        foreach (Vector2Int cell in cells)
            board[cell.x, cell.y].Setup(cell.x, cell.y,
                PuzzleAttribute.Fire, attributeSprites[0]);
        ShowMatches();
    }
    private static Vector2Int P(int x, int y)
    {
        return new Vector2Int(x, y);
    }
}
