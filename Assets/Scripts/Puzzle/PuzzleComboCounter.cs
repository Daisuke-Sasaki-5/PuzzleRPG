using System.Collections.Generic;
using UnityEngine;
public static class PuzzleComboCounter
{
    // 印の付いた同属性のマスを上下左右でまとめる。
    public static int Count(PuzzlePiece[,] board, bool[,] marks)
    {
        int width = board.GetLength(0);
        int height = board.GetLength(1);
        bool[,] visited = new bool[width, height];
        int groups = 0;
        var queue = new Queue<Vector2Int>();
        int[] dx = { 1, -1, 0, 0 };
        int[] dy = { 0, 0, 1, -1 };
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            if (!marks[x, y] || visited[x, y] ||
                board[x, y] == null) continue;
            groups++;
            PuzzleAttribute type = board[x, y].Type;
            visited[x, y] = true;
            queue.Enqueue(new Vector2Int(x, y));
            while (queue.Count > 0)
            {
                Vector2Int cell = queue.Dequeue();
                for (int i = 0; i < 4; i++)
                {
                    int nx = cell.x + dx[i];
                    int ny = cell.y + dy[i];
                    if (nx < 0 || nx >= width ||
                        ny < 0 || ny >= height) continue;
                    if (!marks[nx, ny] || visited[nx, ny] ||
                        board[nx, ny] == null ||
                        board[nx, ny].Type != type) continue;
                    visited[nx, ny] = true;
                    queue.Enqueue(new Vector2Int(nx, ny));
                }
            }
        }
        return groups;
    }
}
