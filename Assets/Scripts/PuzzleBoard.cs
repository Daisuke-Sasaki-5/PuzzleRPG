using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using UnityEngine;

public class PuzzleBoard : MonoBehaviour
{
    [Header("Prefabと生成先")]
    // 生成する石のPrefab
    [SerializeField] private GameObject PuzzlePicePrefab;
    // 石を配置する親オブジェクト
    [SerializeField] private RectTransform PiceRoot;

    [Header("属性画像")]
    // 石のSprite（火、水、木、光、闇の順番）
    [SerializeField] private Sprite[] blockSprite = new Sprite[5];

    [Header("石の配置設定")]
    // 石1個の大きさ
    [SerializeField] private float cellSize = 100.0f;
    // 石同士の間隔
    [SerializeField] private float spacing = 0f;
    // 左上の石の中心座標を保持
    private Vector2 firstCenter;
    // 盤面を管理する２次元配列
    private PuzzleBlock[,] board;
    // 石同士の間隔を参照するためのプロパティ
    public float Spacing => spacing;

    [Header("盤面サイズ")]
    // 横方向のマス数
    [SerializeField, Range(1, 6)] private int width = 6; 
    // 縦方向のマス数
    [SerializeField, Range(1, 6)] private int height = 6;

    [Header("操作設定")]
    // この距離以上ドラッグしたら交換を試みる
    [SerializeField] private float dragThreshold = 30.0f;

    // PuzzleBlockから参照する
    public RectTransform PiecesRoot => PiceRoot;

    public PuzzleBlock[,] Pieces => board;
    private PuzzleMatchPreview matchPreview;

    private PuzzleResolver resolver;
    public bool IsResolving => resolver != null && resolver.IsBusy;

    private bool isOperating;

    public bool IsOperating => isOperating || IsResolving;

    private void Start()
    {
        Build();
    }

    /// <summary>
    /// 石の大きさと最初の石の中心位置を設定する
    /// </summary>
    private void ConfigureCells()
    {
        firstCenter = new Vector2(cellSize / 2f, cellSize / 2f);
    }

    /// <summary>
    /// 指定した座標の位置を求める
    /// </summary>
    /// <param name="x">盤面上のX座標</param>
    /// <param name="y">盤面上のY座標</param>
    /// <returns></returns>
    private Vector2 GetCellPosition(int x, int y)
    {
        // 石の大きさと間隔を合計する
        float step = cellSize + spacing;

        // UIのY座標は下方向がマイナス
        return new Vector2(firstCenter.x + x * step,-(firstCenter.y + y * step));
    }

    /// <summary>
    /// パズル盤面を生成する
    /// </summary>
    private void Build()
    {
        if (PuzzlePicePrefab == null || PiceRoot == null)
        {
            Debug.LogError("Piece PrefabまたはPieceRootが未設定");
            return;
        }

        if (blockSprite == null || blockSprite.Length != 5)
        {
            Debug.LogError("blockSpriteに５種類の画像を設定してください");
            return;
        }

        for(int i =  0; i < blockSprite.Length; i++)
        {
            if(blockSprite[i] == null)
            {
                Debug.LogError($"blockSpriteの{i}番が未設定です");
                return;
            }
        }

        // 石の配置情報を準備する
        ConfigureCells();

        board = new PuzzleBlock[width,height];

        for(int y  = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                PuzzleAttribute type = (PuzzleAttribute)Random.Range(0,blockSprite.Length);
                GameObject obj = Instantiate(PuzzlePicePrefab, PiceRoot, false);
                PuzzleBlock piece =  obj.GetComponent<PuzzleBlock>();
                //Hierarchyで確認しやすい名前をつける
                piece.name = $"Piece_{x}_{y}";
                piece.Setup(x, y, type, blockSprite[(int)type]);

                // 所属する盤面を登録
                piece.SetBoard(this);

                RectTransform rect = piece.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.localScale = Vector3.one;
                rect.localRotation = Quaternion.identity;

                rect.sizeDelta = new Vector2(cellSize, cellSize);
                rect.anchoredPosition = GetCellPosition(x,y);
                board[x, y] = piece;
            }
        }
    }

    public bool TrySwap(PuzzleBlock piece, Vector2 delta)
    {
        // ドラッグ距離が短すぎる場合は交換しない
        if(delta.magnitude < dragThreshold)
        {
            return false;
        }

        int targetX = piece.X;
        int targetY = piece.Y;

        // 移動量が大きい方向を優先
        if(Mathf.Abs(delta.x) >= Mathf.Abs(delta.y))
        {
            // 横方向の移動
            if(delta.x > 0f)
            {
                targetX++;
            }
            else
            {
                targetX--;
            }
        }
        else
        {
            // 縦方向の移動
            if(delta.y > 0f)
            {
                targetY--;
            }
            else
            {
                targetY++;
            }
        }

        // 盤面の外には移動できない
        if(targetX < 0 || targetX >= width || targetY < 0 || targetY >= height) { return false; }

        // 交換先の石を取得する
        PuzzleBlock targetPiece = board[targetX, targetY];
        if(targetPiece == null)
        {
            return false;
        }

        // 石を交換する
        SwapPiece(piece, targetPiece);
        return true;
    }

    private void SwapPiece(PuzzleBlock pieceA, PuzzleBlock pieceB)
    {
        // 交換前の座標を保存する
        int ax = pieceA.X;
        int ay = pieceA.Y;
        int bx = pieceB.X;
        int by = pieceB.Y;

        // 2次元配列の参照を入れ替える
        board[ax, ay] = pieceB;
        board[bx, by] = pieceA;

        // 石自身の座標情報を更新する
        pieceA.SetPosition(bx , by);
        pieceB.SetPosition(ax , ay);

        // RectTransformを取得する
        RectTransform rectA = pieceA.GetComponent<RectTransform>();
        RectTransform rectB = pieceB.GetComponent<RectTransform>();

        // 表示位置を交換後のマスに合わせる
        rectA.anchoredPosition = GetCellPosition(bx , by);
        rectB.anchoredPosition = GetCellPosition(ax , ay);

        // Hierarcy上の名前も更新する
        pieceA.name = $"Piece_{bx}_{by}";
        pieceB.name = $"Piece_{ax}_{ay}";

        Debug.Log($"石を交換しました : ({ax},{ay} <=> ({bx},{by}");

        //CheckMatches();
    }

    public PuzzleBlock GetPiece(int x, int y)
    {
        if (board == null)
        {
            return null;
        }
        if (x < 0 || x >= width ||
            y < 0 || y >= height)
        {
            return null;
        }
        return board[x, y];
    }

    /// <summary>
    /// 操作開始時に前回の判定を消す
    /// </summary>
    public bool BeginOperation()
    {
        if(isOperating || HasEmptyCells())
        {
            return false;
        }
        isOperating = true;

        if(matchPreview != null)
        {
            matchPreview.ClearHighlights();
        }
        return true;
    }

    /// <summary>
    /// 操作終了時に盤面を判定する
    /// </summary>
    public void EndOperation()
    {
        if (!isOperating) { return; }

        isOperating = false;
        if(resolver != null)
        {
            resolver.TryResolve(); 
        }
        else if(matchPreview != null)
        {
            matchPreview.ShowMatches();
        }
    }

    /// <summary>
    /// 石のTransformを設定する
    /// </summary>
    /// <param name="piece"></param>
    /// <returns></returns>
    private RectTransform configurePiceRect(PuzzleBlock piece)
    {
        RectTransform rect = piece.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
        rect.sizeDelta = new Vector2(cellSize, cellSize);
        return rect;
    }

    /// <summary>
    /// 空きマスが存在するか確認
    /// </summary>
    /// <returns></returns>
    public bool HasEmptyCells()
    {
        if(board == null)
        {
            return false;
        }
        foreach (PuzzleBlock piece in board)
        {
            if(piece == null)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 石を削除
    /// </summary>
    /// <param name="marks"></param>
    /// <returns></returns>
    public int RemoveMarked(bool[,] marks)
    {
        int removed = 0;
        for(int y = 0; y < height;  y++)
        {
            for(int x = 0; x < width; x++)
            {
                if(!marks[x, y] || board[x,y] == null) { continue; }
                PuzzleBlock piece = board[x,y];
                // 配列から参照を外す
                board[x,y] = null;

                piece.gameObject.SetActive(false);
                Destroy(piece.gameObject);
                removed++;
            }
        }
        return removed;
    }

    /// <summary>
    /// 残った石を下方向につめる
    /// </summary>
    public void CollapsePieces()
    {
        for(int x = 0; x < height; x++)
        {
            int writeY = height - 1;
            // 下から上へ調べる
            for(int readY = height - 1; readY >= 0; readY--)
            {
                PuzzleBlock piece = board[x, readY];
                if(piece == null) { continue;}
                if(readY != writeY)
                {
                    board[x,readY] = null;
                    board[x,writeY] = piece;
                }

                piece.SetCoordinates(x, writeY);
                writeY--;
            }
        }
    }

    public void RefillPiece(bool useFixedRefill)
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (board[x, y] != null) { continue; }
                int index;

                if (useFixedRefill)
                {
                    // 固定テスト用
                    index = (x + 2 * y) % 5;
                }
                else
                {
                    index = Random.Range(0, blockSprite.Length);
                }
                PuzzleBlock piece = Instantiate(PuzzlePicePrefab, PiceRoot, false);

                piece.Setup(x, y, (PuzzleAttribute)index, blockSprite[index]);

                piece.SetBoard(this);
                RectTransform rect = configurePiceRect(piece);

                rect.anchoredPosition = GetCellPosition(x, y - height);
                board[x, y] = piece;
            }
        }
    }

    public IEnumerator AnimateToCells(float duration)
    {
        List<RectTransform> rects = new List<RectTransform> ();
        List<Vector2> starts = new List<Vector2> ();
        List<Vector2> target = new List<Vector2> ();

        for(int y = 0; y < height;y++)
        {
            for(int x = 0; x < width;x++)
            {
                PuzzleBlock piece = board[x, y];
                if(piece == null) { continue; }
                RectTransform rect = piece.GetComponent<RectTransform>();

                rects.Add(rect);
                starts.Add(rect.anchoredPosition);
                target.Add(GetCellPosition(x, y));
            }
        }

        float elapsed = 0f;
        if(duration > 0f)
        {
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01 (elapsed / duration);

                for(int i = 0; i < rects.Count; i++)
                {
                    rects[i].anchoredPosition = Vector2.Lerp(starts[i], target[i], t);
                }
                yield return null;
            }
        }

        for(int i =0; i < rects.Count;i++)
        {
            rects[i].anchoredPosition = target[i];
        }
    }

    public bool ValidateBoard()
    {
        if(board == null) {  return false; }
        bool valid = true;

        HashSet<PuzzleBlock> found = new HashSet<PuzzleBlock>();

        for(int y = 0; y < height; y++)
        {
            for(int x = 0; x < width; x++)
            {
                PuzzleBlock piece = board[x, y];
                if(piece == null) { valid = false; continue; }
                
                if(!found.Add(piece))
                {
                    valid = false;
                }

                if(piece.X != x || piece.Y != y)
                {
                    valid |= false;
                }

                Vector2 position = piece.GetComponent<RectTransform>().anchoredPosition;

                if (Vector2.Distance(position, GetCellPosition(x,y)) > 0.1f)
                {
                    valid = false;
                }
            }
        }
        return valid;
    }

    //private void CheckHorizintalMatches()
    //{
    //    // 各行を上から順番に調べる
    //    for(int y = 0; y < height; y++)
    //    {
    //        // 横方向に３個並んでるか確認
    //        for (int x = 0; x < width - 2; x++)
    //        {
    //            PuzzleBlock piece1 = board[x, y];
    //            PuzzleBlock piece2 = board[x + 1, y];
    //            PuzzleBlock piece3 = board[x + 2, y];

    //            // 3個とも同じ属性か調べる
    //            if(piece1.Type == piece2.Type &&  piece2.Type == piece3.Type)
    //            {
    //                Debug.Log($"横マッチ:({x},{y}) 属性 : {piece1.Type}");
    //            }
    //        }
    //    }
    //}

    //private void CheckVerticalMatches()
    //{
    //    // 各列を左から順に調べる
    //    for(int x = 0; x < width; x++)
    //    {
    //        // 縦方向に３個以上並んでいるか確認
    //        for( int y = 0; y < height - 2; y++)
    //        {
    //            PuzzleBlock piece1 = board[x, y];
    //            PuzzleBlock piece2 = board[x, y + 1];
    //            PuzzleBlock piece3 = board[x, y + 2];

    //            // 3個とも同じ属性か調べる
    //            if (piece1.Type == piece2.Type && piece2.Type == piece3.Type)
    //            {
    //                Debug.Log($"縦マッチ:({x},{y}) 属性 : {piece1.Type}");
    //            }
    //        }
    //    }
    //}

    //private void CheckMatches()
    //{
    //    Debug.Log("マッチ判定開始");
    //    CheckHorizintalMatches();
    //    CheckVerticalMatches();
    //    Debug.Log("マッチ判定終了");
    //}
}
