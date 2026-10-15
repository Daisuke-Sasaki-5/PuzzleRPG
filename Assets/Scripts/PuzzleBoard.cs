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
                targetY++;
            }
            else
            {
                targetY--;
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
    }

    //public PuzzleBlock GetPiece(int x, int y)
    //{
    //    if(board == null)
    //    {
    //        return null;
    //    }
    //    if(x < 0 || x >= width || y < 0 || y >= height)
    //    {
    //        return null;
    //    }

    //}
}
