using System.Collections.Generic;
using UnityEngine;
public static class PuzzleComboCounter
{
    public static int Count(PuzzlePiece[,] b,bool[,] marks)
    {
        int w=b.GetLength(0),h=b.GetLength(1),combos=0;
        bool[,] visited=new bool[w,h];
        int[] dx=
        {
            1,-1,0,0
        }
        ,dy=
        {
            0,0,1,-1
        }
        ;
        for(int y=0;y<h;y++)for(int x=0;x<w;x++)
        {
            if(!marks[x,y]||visited[x,y]||b[x,y]==null)continue;
            combos++;
            var type=b[x,y].Type;
            var q=new Queue<Vector2Int>();
            q.Enqueue(new Vector2Int(x,y));
            visited[x,y]=true;
            while(q.Count>0)
            {
                var p=q.Dequeue();
                for(int k=0;k<4;k++)
                {
                    int nx=p.x+dx[k],ny=p.y+dy[k];
                    if(nx<0||ny<0||nx>=w||ny>=h||!marks[nx,ny]||visited[nx,ny]||b[nx,ny]==null||b[nx,ny].Type!=type)continue;
                    visited[nx,ny]=true;
                    q.Enqueue(new Vector2Int(nx,ny));
                }
            }
        }
        return combos;
    }
}
