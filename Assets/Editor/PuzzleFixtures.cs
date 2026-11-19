#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
public static class PuzzleFixtures
{
    [MenuItem("Course/Fixtures/No match board (Play)")]
    public static void NoMatch() { Apply(false); }
    [MenuItem("Course/Fixtures/One fire match board (Play)")]
    public static void OneMatch() { Apply(true); }
    private static void Apply(bool oneMatch)
    {
        var b=Object.FindFirstObjectByType<PuzzleBoard>();
        if(!Application.isPlaying || b==null || b.Pieces==null || b.IsOperating || b.InputLocked || b.HasEmptyCells()) {
            Debug.LogWarning("Run an active unlocked battle before applying fixture.");return;
        }
        var so=new SerializedObject(b);var sprites=so.FindProperty("attributeSprites");
        for(int y=0;y<6;y++) for(int x=0;x<6;x++) {
            int a=(x+2*y)%5;
            if(oneMatch && y==0 && x<3) a=x==2?1:0;
            if(oneMatch && x==2 && y==1) a=0;
            b.Pieces[x,y].Setup(x,y,(PuzzleAttribute)a,(Sprite)sprites.GetArrayElementAtIndex(a).objectReferenceValue);
        }
        Debug.Log(oneMatch?"Hold (2,0), move to (2,1), release: first wave 3 Fire. v1.3 continuous exchange.":"Tap: 0 turns. Move (0,0) to (1,0) and release: no-match turn once.");
    }
}
#endif
