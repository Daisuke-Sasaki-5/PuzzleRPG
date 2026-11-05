public static class PuzzleMatcher {
 public static bool[,] Find(PuzzlePiece[,] b,bool vertical){int w=b.GetLength(0),h=b.GetLength(1);bool[,] m=new bool[w,h];
 for(int y=0;y<h;y++){int x=0;while(x<w){if(b[x,y]==null){x++;continue;}int s=x;var t=b[x,y].Type;x++;while(x<w&&b[x,y]!=null&&b[x,y].Type==t)x++;if(x-s>=3)for(int i=s;i<x;i++)m[i,y]=true;}}
 if(vertical)for(int x=0;x<w;x++){int y=0;while(y<h){if(b[x,y]==null){y++;continue;}int s=y;var t=b[x,y].Type;y++;while(y<h&&b[x,y]!=null&&b[x,y].Type==t)y++;if(y-s>=3)for(int i=s;i<y;i++)m[x,i]=true;}}
 return m;}
 public static int Count(bool[,] marks){int n=0;foreach(bool m in marks)if(m)n++;return n;}
}
