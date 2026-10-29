using System.Collections.Generic;
using UnityEngine;

public static class PuzzleComboCounter
{
    public static int Count(PuzzleBlock[,] board, bool[,] marks)
    {
        int width = board.GetLength(0);
        int height = board.GetLength(1);

        bool[,] visited = new bool[width, height];
        int combos = 0;

        // è„â∫ç∂âEÇÃíTçıï˚å¸
        int[] dx = { 1, -1, 0, 0 };
        int[] dy = { 0, 0, 1, -1 };

        for(int y = 0; y < height; y++)
        {
            for(int x = 0; x < width; x++)
            {
                if (!marks[x,y] || visited[x,y] || board[x,y] == null)
                {
                    continue;
                }
                // êVÇµÇ¢ëgÇî≠å©
                combos++;
                PuzzleAttribute type = board[x,y].Type;
                Queue<Vector2Int> queue = new Queue<Vector2Int>();

                queue.Enqueue(new Vector2Int(x,y));
                visited[x,y] = true;
                while (queue.Count > 0)
                {
                    Vector2Int current = queue.Dequeue();
                    for(int i = 0; i < 4 ; i++)
                    {
                        int nx = current.x + dx[i];
                        int ny = current.y + dy[i];

                        if(nx < 0 || nx >= width || ny < 0 || ny >= height)
                        {
                            continue;
                        }
                        if (!marks[nx,ny] || visited[nx,ny] ||  board[nx,ny] == null)
                        {
                            continue;
                        }
                        if(board[nx,ny].Type != type)
                        {
                            continue;
                        }

                        visited[nx,ny] = true;
                        queue.Enqueue(new Vector2Int(nx, ny));
                    }
                }
            }
        }
        return combos;
    }
}
