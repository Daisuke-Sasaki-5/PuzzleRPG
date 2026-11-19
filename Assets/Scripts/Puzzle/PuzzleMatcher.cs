public static class PuzzleMatcher
{
    public static bool[,] Find(PuzzlePiece[,] board,
        bool includeVertical = true)
    {
        if (board == null) return new bool[0, 0];
        int width = board.GetLength(0);
        int height = board.GetLength(1);
        bool[,] marks = new bool[width, height];
        for (int y = 0; y < height; y++)
        {
            int x = 0;
            while (x < width)
            {
                if (board[x, y] == null) { x++; continue; }
                int start = x;
                PuzzleAttribute type = board[x, y].Type;
                x++;
                while (x < width && board[x, y] != null &&
                    board[x, y].Type == type) x++;
                int length = x - start;
                if (length >= 3)
                    for (int i = start; i < x; i++)
                        marks[i, y] = true;
            }
        }
        if (!includeVertical) return marks;
        for (int x = 0; x < width; x++)
        {
            int y = 0;
            while (y < height)
            {
                if (board[x, y] == null) { y++; continue; }
                int start = y;
                PuzzleAttribute type = board[x, y].Type;
                y++;
                while (y < height && board[x, y] != null &&
                    board[x, y].Type == type) y++;
                int length = y - start;
                if (length >= 3)
                    for (int i = start; i < y; i++)
                        marks[x, i] = true;
            }
        }
        return marks;
    }
    public static int Count(bool[,] marks)
    {
        int count = 0;
        foreach (bool marked in marks)
            if (marked) count++;
        return count;
    }
}
