using UnityEngine;

[RequireComponent(typeof(PuzzleBoard))]
public class PuzzleMatchPreview : MonoBehaviour
{ 
    public enum TestPattern
    {
        None,
        Horizontal3,
        Vertical3,
        Separate3,
        Horizontal4,
        Horizontal5,
        Horizontal6,
        LShape,
        TShape,
        Edge2,
        RowWrap
    }

    [Header("判定設定")]
    [SerializeField] private bool includeVertical = false;

    [Header("固定テスト")]
    [SerializeField] private TestPattern testPattern = TestPattern.Horizontal3;

    [Header("属性画像")]
    [SerializeField] private Sprite[] attributeSprites = new Sprite[5];

    private PuzzleBoard puzzleBoard;

    private void Awake()
    {
        puzzleBoard = GetComponent<PuzzleBoard>();
    }
    
    [ContextMenu("Show Matches")]
    public void ShowMatches()
    {
        if (puzzleBoard == null ||
            puzzleBoard.Pieces == null)
        {
            Debug.LogWarning(
                "盤面が生成されていません");
            return;
        }
        ClearHighlights();
        PuzzleBlock[,] board = puzzleBoard.Pieces;
        bool[,] marks = PuzzleMatcher.Find(board, includeVertical);
        int width = board.GetLength(0);
        int height = board.GetLength(1);

        //マッチした石を色で表示
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (!marks[x, y])
                {
                    continue;
                }
                if (board[x, y] == null)
                {
                    continue;
                }
                board[x, y].SetMatched(true);
            }
        }
        int count = PuzzleMatcher.Count(marks);
        Debug.Log($"マッチ対象数：{count}個");
    }

    public void ClearHighlights()
    {
        if(puzzleBoard == null || puzzleBoard.Pieces == null)
        {
            return;
        }

        PuzzleBlock[,] board = puzzleBoard.Pieces;
        foreach(PuzzleBlock piece  in board)
        {
            if(piece != null)
            {
                piece.SetMatched(false);
            }
        }
    }

    [ContextMenu("Apply test pattern")]
    public void ApplyPattern()
    {
        if(puzzleBoard == null || puzzleBoard.Pieces == null) { return; }

        if(attributeSprites ==  null || attributeSprites.Length != 5) { return; }

        for(int i = 0; i < attributeSprites.Length; i++)
        {
            if(attributeSprites[i] == null)
            {
                return;
            }
        }

        PuzzleBlock[,] board = puzzleBoard.Pieces;
        int width = board.GetLength(0);
        int height = board.GetLength(1);

        if(width != 6 || height != 6) {  return; }

        for(int y = 0; y < height; y++)
        {
            for(int x = 0; x < width; x++)
            {
                int index = (x +  y * 2) % 5;

            }
        }

        switch (testPattern)
        {
            case TestPattern.None:
                break;
            case TestPattern.Horizontal3:
                SetHorizontal(1, 2, 3);
                break;
            case TestPattern.Vertical3:
                SetVertical(2, 1, 3);
                break;
            case TestPattern.Separate3:
                SetHorizontal(1, 1, 3);
                SetVertical(4, 2, 3);
                break;
            case TestPattern.Horizontal4:
                SetHorizontal(1, 2, 4);
                break;
            case TestPattern.Horizontal5:
                SetHorizontal(0, 2, 5);
                break;
            case TestPattern.Horizontal6:
                SetHorizontal(0, 2, 6);
                break;
            case TestPattern.LShape:
                SetHorizontal(1, 1, 3);
                SetVertical(1, 1, 3);
                break;
            case TestPattern.TShape:
                SetHorizontal(1, 1, 3);
                SetVertical(2, 1, 3);
                break;
            case TestPattern.Edge2:
                SetPiece(4, 0, PuzzleAttribute.Fire);
                SetPiece(5, 0, PuzzleAttribute.Fire);
                break;
            case TestPattern.RowWrap:
                SetPiece(4, 0, PuzzleAttribute.Fire);
                SetPiece(5, 0, PuzzleAttribute.Fire);
                SetPiece(0, 1, PuzzleAttribute.Fire);
                break;
        }

        ShowMatches();
    }

    private void SetPiece(int x, int y, PuzzleAttribute type)
    {
        PuzzleBlock[,] board = puzzleBoard.Pieces;
        if (board[x,y] == null) {  return; }
        board[x, y].SetAttribute(type, attributeSprites[(int)type]);
    }

    private void SetHorizontal(int startX, int y, int length)
    {
        for (int i = 0; i < length; i++)
        {
            SetPiece(startX + i, y, PuzzleAttribute.Fire);
        }
    }

    private void SetVertical(int x, int startY, int length)
    {
        for(int i = 0; i < length; i++)
        {
            SetPiece(x, startY + i, PuzzleAttribute.Fire);
        }
    }
}
