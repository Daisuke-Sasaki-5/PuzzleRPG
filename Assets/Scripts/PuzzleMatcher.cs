using UnityEngine;

public static class PuzzleMatcher
{
    /// <summary>
    /// 3個以上マッチしたマスを調べる関数
    /// </summary>
    /// <param name="board"></param>
    /// <param name="includeVertical"></param>
    /// <returns></returns>
    public static bool[,] Find(PuzzleBlock[,] board, bool includeVertical)
    {
        int width = board.GetLength(0);
        int height = board.GetLength(1);

        // マッチした配列を記録する配列
        bool[,] marks = new bool[width, height];
        for(int y = 0; y < height; y++)
        {
            int x = 0;
            while(x < width)
            {
                if(board[x, y] == null)
                {
                    x++; continue;
                }
                int start = x;
                PuzzleAttribute type = board[x, y].Type;
                x++;

                while(x < width && board[x,y] != null && board[x,y].Type == type)
                {
                    x++;
                }

                int length = x - start;

                // 3個以上なら対象マスにしるしをつける
                if(length >= 3)
                {
                    for(int i = start; i < x;  i++)
                    {
                        marks[i, y] = true;
                    }
                }
            }
        }
        if(!includeVertical)
        {
            return marks;
        }
        // 縦方向チェック
        for(int x = 0; x < width; x++)
        {
            int y = 0;
            while (y < height)
            {
                // 空のマスは飛ばす
                if (board[x, y] == null)
                {
                    y++; continue;
                }
                // 同じ属性が続く場合の開始位置
                int start = y;
                PuzzleAttribute type = board[x, y].Type;
                y++;

                // 同じ属性が続く場合は下にすすむ
                while (y < height && board[x, y] != null && board[x, y].Type == type)
                {
                    y++;
                }
                // 同じ属性が続いた場合の個数
                int length = y - start;
                // ３個以上なら対象マスにしるしをつける
                if (length >= 3)
                {
                    for (int i = start; i < y; i++)
                    {
                        marks[x, i] = true;
                    }
                }
            }
        }
        return marks;
    }
    
    /// <summary>
    /// マッチしたマスの個数を数える
    /// </summary>
    /// <param name="marks"></param>
    /// <returns></returns>
    public static int Count(bool[,] marks)
    {
        int count = 0;

        foreach(bool marked in marks)
        {
            if(marked)
            {
                count++;
            }
        }

        return count;
    }
}
