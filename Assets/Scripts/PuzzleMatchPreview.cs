using UnityEngine;

[RequireComponent (typeof(PuzzleBoard))]
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

    [Header("îªíËê›íË")]
    [SerializeField] private bool includeVertical = false;

    [Header("å≈íËÉeÉXÉg")]
    [SerializeField] private TestPattern testPattern = TestPattern.Horizontal3;

    [Header("ëÆê´âÊëú")]
    [SerializeField] private Sprite[] attributeSprites = new Sprite[5];

    private PuzzleBoard puzzleBoard;

    private void Awake()
    {
        puzzleBoard = GetComponent<PuzzleBoard>();
    }
    
    [ContextMenu("Show Matches")]
    public void ShowMatches()
    {
        if(puzzleBoard == null || puzzleBoard.Pieces == null)
        {
            return;
        }
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
            case TestPattern.None: break;
            case TestPattern.Horizontal3:
            case TestPattern.Vertical3:
            case TestPattern.Separate3:
            case TestPattern.Horizontal4:
            case TestPattern.Horizontal5:
            case TestPattern.Horizontal6:
            case TestPattern.LShape:
            case TestPattern.TShape:
            case TestPattern.Edge2:
            case TestPattern.RowWrap:
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
