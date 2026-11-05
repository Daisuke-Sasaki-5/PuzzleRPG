using UnityEngine;
[RequireComponent(typeof(PuzzleBoard))]
public class PuzzleMatchPreview:MonoBehaviour
{
    public enum TestPattern
    {
        None,Horizontal3,Vertical3,Separate3,Horizontal4,Horizontal5,Horizontal6,LShape,TShape,Edge2,RowWrap
    }
    [SerializeField] bool includeVertical=true;
    [SerializeField] TestPattern testPattern=TestPattern.Horizontal3;
    [SerializeField] Sprite[] attributeSprites=new Sprite[5];
    PuzzleBoard owner;
    void Awake()
    {
        owner=GetComponent<PuzzleBoard>();
    }
    public void ClearHighlights()
    {
        if(owner.Pieces==null)return;
        foreach(var p in owner.Pieces)if(p!=null)p.SetMatched(false);
    }
    [ContextMenu("Show Matches")]
 public void ShowMatches()
    {
        if(owner.Pieces==null||owner.IsOperating)return;
        ClearHighlights();
        var b=owner.Pieces;
        var m=PuzzleMatcher.Find(b,includeVertical);
        for(int y=0;y<b.GetLength(1);y++)for(int x=0;x<b.GetLength(0);x++)if(m[x,y]&&b[x,y]!=null)
        {
            b[x,y].SetMatched(true);
            Debug.Log($"対象({x},{y}) {b[x,y].Type}");
        }
        Debug.Log($"対象数：{PuzzleMatcher.Count(m)}");
    }
    void Set(int x,int y,PuzzleAttribute type)
    {
        owner.Pieces[x,y].SetAttribute(type,attributeSprites[(int)type]);
    }
    void H(int x,int y,int len)
    {
        for(int i=0;i<len;i++)Set(x+i,y,PuzzleAttribute.Fire);
    }
    void V(int x,int y,int len)
    {
        for(int i=0;i<len;i++)Set(x,y+i,PuzzleAttribute.Fire);
    }
    [ContextMenu("Apply Test Pattern")]
 public void ApplyTestPattern()
    {
        if(owner.Pieces==null||owner.IsOperating||owner.HasEmptyCells())return;
        if(attributeSprites==null||attributeSprites.Length!=5)return;
        foreach(var s in attributeSprites)if(s==null)return;
        if(owner.Pieces.GetLength(0)!=6||owner.Pieces.GetLength(1)!=6)return;
        for(int y=0;y<6;y++)for(int x=0;x<6;x++)Set(x,y,(PuzzleAttribute)((x+2*y)%4+1));
        switch(testPattern)
        {
            case TestPattern.Horizontal3:H(1,2,3);
            break;
            case TestPattern.Vertical3:V(2,1,3);
            break;
            case TestPattern.Separate3:H(1,1,3);
            V(4,2,3);
            break;
            case TestPattern.Horizontal4:H(1,2,4);
            break;
            case TestPattern.Horizontal5:H(0,2,5);
            break;
            case TestPattern.Horizontal6:H(0,2,6);
            break;
            case TestPattern.LShape:H(1,1,3);
            V(1,1,3);
            break;
            case TestPattern.TShape:H(1,1,3);
            V(2,1,3);
            break;
            case TestPattern.Edge2:Set(4,0,PuzzleAttribute.Fire);
            Set(5,0,PuzzleAttribute.Fire);
            break;
            case TestPattern.RowWrap:Set(4,0,PuzzleAttribute.Fire);
            Set(5,0,PuzzleAttribute.Fire);
            Set(0,1,PuzzleAttribute.Fire);
            break;
        }
        ShowMatches();
    }
}
