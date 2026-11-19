using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class PuzzleBoard : MonoBehaviour
{
    [SerializeField] private PuzzlePiece piecePrefab;
    [SerializeField] private RectTransform piecesRoot;
    [SerializeField] private Sprite[] attributeSprites
        = new Sprite[5];
    [SerializeField, Range(1, 6)] private int width = 6;
    [SerializeField, Range(1, 6)] private int height = 6;
    [SerializeField] private float cellSize = 100f;
    [SerializeField] private float spacing = 0f;
    private PuzzlePiece[,] board;
    public PuzzlePiece[,] Pieces => board;
    public bool InputLocked { get; set; }
    public bool IsOperating => selected != null || IsResolving;
    public RectTransform PiecesRoot => piecesRoot;
    [SerializeField] private bool allowExchange = false;
    [SerializeField] private bool highlightSelection = false;
    private PuzzlePiece selected;
    private int activePointerId;
    private Vector2 previousPointer;
    private int selectedSibling;
    private PuzzlePiece[,] gestureStart;
    private bool pathChanged;
    private void Start()
    {
        if (board == null) Build();
    }
    private void Build()
    {
        if (piecePrefab == null || piecesRoot == null ||
            attributeSprites == null ||
            attributeSprites.Length != 5)
        {
            Debug.LogError("Prefab 親 Sprite5枚を確認", this);
            return;
        }
        for (int i = 0; i < attributeSprites.Length; i++)
        {
            if (attributeSprites[i] == null)
            {
                Debug.LogError("未設定のSpriteがあります", this);
                return;
            }
        }
        // Play開始時に一度生成する。
        if (piecesRoot.childCount != 0)
        {
            Debug.LogError("PuzzlePiecesの仮石を削除", this);
            return;
        }
        width = Mathf.Clamp(width, 1, 6);
        height = Mathf.Clamp(height, 1, 6);
        board = new PuzzlePiece[width, height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                PuzzleAttribute type;
                do { type = (PuzzleAttribute)Random.Range(0, attributeSprites.Length); }
                while ((x >= 2 && board[x-1,y].Type == type && board[x-2,y].Type == type) ||
                       (y >= 2 && board[x,y-1].Type == type && board[x,y-2].Type == type));
                PuzzlePiece piece = Instantiate(
                    piecePrefab, piecesRoot, false);
                piece.name = $"Piece_{x}_{y}";
                piece.Setup(x, y, type,
                    attributeSprites[(int)type]);
                piece.Bind(this);
                RectTransform rect =
                    piece.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.localScale = Vector3.one;
                rect.localRotation = Quaternion.identity;
                rect.sizeDelta = new Vector2(cellSize, cellSize);
                rect.anchoredPosition = GetCellPosition(x, y);
                board[x, y] = piece;
            }
        }
        ValidateBoard();
        PuzzlePiece first = board[0, 0];
        Debug.Log($"左上 {first.name} " +
            $"座標({first.X},{first.Y}) 属性{first.Type}", this);
    }
    // マスの座標を、親の左上からの表示位置へ変換。
    // 生成時だけでなく、交換・落下の移動先にも使う。
    public Vector2 GetCellPosition(int x, int y)
    {
        float step = cellSize + spacing;
        return new Vector2(cellSize / 2f + x * step,
            -(cellSize / 2f + y * step));
    }
    public void BeginGesture(PuzzlePiece piece,
        PointerEventData data)
    {
        if (data == null || piece == null || !isActiveAndEnabled || board == null ||
            InputLocked || IsOperating || HasEmptyCells() ||
            data.button != PointerEventData.InputButton.Left)
            return;
        if (!Inside(piece.X, piece.Y) ||
            board[piece.X, piece.Y] != piece) return;
        if (!TryGetPieceAt(data.position, data.pressEventCamera, out PuzzlePiece hit) ||
            hit != piece || !TryPointer(data, out Vector2 pointer)) return;
        GetComponent<PuzzleMatchPreview>()?.ClearHighlights();
        gestureStart = (PuzzlePiece[,])board.Clone();
        pathChanged = false;
        selected = piece;
        activePointerId = data.pointerId;
        previousPointer = pointer;
        RectTransform rect = selected.GetComponent<RectTransform>();
        selectedSibling = rect.GetSiblingIndex();
        rect.SetAsLastSibling();
        rect.anchoredPosition = pointer;
        selected.SetSelected(highlightSelection);
        Debug.Log($"選択 ({piece.X},{piece.Y}) {piece.Type}", this);
    }
    public void DragGesture(PointerEventData data)
    {
        if (data == null || !isActiveAndEnabled || selected == null ||
            data.pointerId != activePointerId || !TryPointer(data, out Vector2 pointer)) return;
        if (allowExchange) FollowPath(previousPointer, pointer);
        previousPointer = pointer;
        selected.GetComponent<RectTransform>().anchoredPosition = pointer;
    }
    public void EndGesture(PointerEventData data)
    {
        if (data == null || selected == null || data.pointerId != activePointerId) return;
        DragGesture(data);
        bool commit = pathChanged;
        FinishGesture();
        ValidateBoard();
        if (!commit) return;
        PuzzleResolver resolver = GetComponent<PuzzleResolver>();
        if (resolver != null) resolver.TryResolve();
        else GetComponent<PuzzleMatchPreview>()?.ShowMatches();
    }
    private void FinishGesture()
    {
        if (selected != null)
        {
            RectTransform rect = selected.GetComponent<RectTransform>();
            rect.anchoredPosition = GetCellPosition(selected.X, selected.Y);
            rect.SetSiblingIndex(selectedSibling);
            selected.SetSelected(false);
        }
        selected = null;
        activePointerId = 0;
        gestureStart = null;
        pathChanged = false;
    }
    private void CancelGesture()
    {
        if (selected != null && gestureStart != null)
        {
            board = gestureStart;
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                board[x,y].SetCoordinates(x,y);
                board[x,y].GetComponent<RectTransform>().anchoredPosition = GetCellPosition(x,y);
            }
        }
        FinishGesture();
    }
    private bool TryPointer(PointerEventData data, out Vector2 pointer)
    {
        pointer = default;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            piecesRoot, data.position, data.pressEventCamera, out Vector2 local)) return false;
        Rect area = piecesRoot.rect;
        float step = cellSize + spacing;
        // Display follows the pointer over the full playable rectangle (600x600).
        // Cell indices are clamped separately in FollowPath; Root padding is not a cell.
        pointer = new Vector2(
            Mathf.Clamp(local.x - area.xMin, 0f, (width - 1) * step + cellSize),
            -Mathf.Clamp(area.yMax - local.y, 0f, (height - 1) * step + cellSize));
        return true;
    }
    // Traverse actual boundary crossings in time order. Exact corner ties exchange directly with the diagonal cell.
    // Unlike fixed samples this cannot reorder two nearby crossings inside one sample.
    private void FollowPath(Vector2 from, Vector2 to)
    {
        double step = cellSize + spacing;
        double dx = to.x - from.x, dy = -to.y + from.y;
        int targetX = Mathf.Clamp(Mathf.FloorToInt(to.x / (float)step), 0, width - 1);
        int targetY = Mathf.Clamp(Mathf.FloorToInt(-to.y / (float)step), 0, height - 1);
        int sx = System.Math.Sign(dx), sy = System.Math.Sign(dy);
        while (selected.X != targetX || selected.Y != targetY)
        {
            int x = selected.X, y = selected.Y;
            double tx = x == targetX || sx == 0 ? double.PositiveInfinity :
                (((sx > 0 ? x + 1 : x) * step) - from.x) / dx;
            double ty = y == targetY || sy == 0 ? double.PositiveInfinity :
                (((sy > 0 ? y + 1 : y) * step) + from.y) / dy;
            if (double.IsPositiveInfinity(tx) && double.IsPositiveInfinity(ty)) return;
            if (System.Math.Abs(tx - ty) <= 1e-9) { x += sx; y += sy; }
            else if (tx < ty) x += sx;
            else y += sy;
            if (!Inside(x, y) || !TrySwap(selected, board[x, y])) return;
        }
    }
    private void OnDisable()
    {
        CancelGesture();
        GetComponent<PuzzleResolver>()?.InterruptResolution();
    }
    private void OnApplicationFocus(bool focused)
    {
        if (!focused) CancelGesture();
    }
    private void OnApplicationPause(bool paused)
    {
        if (paused) CancelGesture();
    }
    private bool Inside(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }
    private bool TryGetPieceAt(Vector2 screen, Camera camera,
        out PuzzlePiece piece)
    {
        piece = null;
        if (board == null || !RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                piecesRoot, screen, camera, out Vector2 local))
            return false;
        Rect area = piecesRoot.rect;
        if (!area.Contains(local)) return false;
        float left = local.x - area.xMin;
        float top = area.yMax - local.y;
        float step = cellSize + spacing;
        int x = Mathf.FloorToInt(left / step);
        int y = Mathf.FloorToInt(top / step);
        if (!Inside(x, y)) return false;
        // 石の間の隙間は選択対象にしない。
        if (left - x * step >= cellSize ||
            top - y * step >= cellSize) return false;
        piece = board[x, y];
        return piece != null;
    }
    private bool TrySwap(PuzzlePiece a, PuzzlePiece b)
    {
        if (board == null || a == null || b == null || a == b) return false;
        int ax = a.X, ay = a.Y;
        int bx = b.X, by = b.Y;
        if (!Inside(ax, ay) || !Inside(bx, by)) return false;
        if (board[ax, ay] != a || board[bx, by] != b) return false;
        int distance = Mathf.Max(Mathf.Abs(ax - bx), Mathf.Abs(ay - by));
        if (distance != 1) return false;
        pathChanged = true;
        board[ax, ay] = b;
        board[bx, by] = a;
        a.SetCoordinates(bx, by);
        b.SetCoordinates(ax, ay);
        if (a != selected)
            a.GetComponent<RectTransform>().anchoredPosition = GetCellPosition(bx, by);
        b.GetComponent<RectTransform>().anchoredPosition =
            GetCellPosition(ax, ay);
        Debug.Log($"交換 ({ax},{ay}) と ({bx},{by})", this);
        ValidateBoard();
        return true;
    }
    [ContextMenu("Validate Board")]
    private void ValidateBoard()
    {
        if (board == null)
        {
            Debug.LogWarning("Validate Board: board not generated", this);
            return;
        }
        if (board.GetLength(0) != width || board.GetLength(1) != height ||
            piecesRoot.childCount != width * height)
        {
            Debug.LogError("Validate Board: dimensions/child count mismatch", this);
            return;
        }
        var unique = new HashSet<PuzzlePiece>();
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                PuzzlePiece piece = board[x, y];
                if (piece == null || piece.X != x || piece.Y != y)
                {
                    Debug.LogError($"配列と石の座標が不一致 ({x},{y})", this);
                    return;
                }
                int type = (int)piece.Type;
                if (!unique.Add(piece) || piece.transform.parent != piecesRoot ||
                    piece.name != $"Piece_{x}_{y}" || type < 0 || type >= attributeSprites.Length ||
                    piece.GetComponent<Image>().sprite != attributeSprites[type])
                {
                    Debug.LogError($"Validate Board: identity/image mismatch ({x},{y})", this);
                    return;
                }
                Vector2 pos = piece.GetComponent<RectTransform>()
                    .anchoredPosition;
                if (piece != selected && Vector2.Distance(pos, GetCellPosition(x, y)) > 0.01f)
                {
                    Debug.LogError($"表示位置が不一致 ({x},{y})", this);
                    return;
                }
            }
        }
        Debug.Log($"Validate Board PASS: {width}x{height}, {unique.Count} unique pieces", this);
    }

public void ResetBoard()
{
    InputLocked = true;
    CancelGesture();
    GetComponent<PuzzleResolver>()?.ResetState();
    // Detach and deactivate first: delayed Destroy must not contaminate the new board.
    for (int i = piecesRoot.childCount - 1; i >= 0; i--)
    {
        Transform old = piecesRoot.GetChild(i);
        old.gameObject.SetActive(false);
        old.SetParent(null, false);
        Destroy(old.gameObject);
    }
    board = null;
    Build();
}
public bool IsResolving =>
    GetComponent<PuzzleResolver>()?.BlocksInput ?? false;
public bool HasEmptyCells()
{
    if (board == null) return true;
    foreach (PuzzlePiece piece in board)
        if (piece == null) return true;
    return false;
}
public int RemoveMarked(bool[,] marks)
{
    int removed = 0;
    for (int y = 0; y < height; y++)
    for (int x = 0; x < width; x++)
    {
        if (!marks[x, y] || board[x, y] == null) continue;
        PuzzlePiece piece = board[x, y];
        board[x, y] = null;
        piece.gameObject.SetActive(false);
        Destroy(piece.gameObject);
        removed++;
    }
    return removed;
}
public void CollapsePieces()
{
    for (int x = 0; x < width; x++)
    {
        int writeY = height - 1;
        for (int readY = height - 1; readY >= 0; readY--)
        {
            PuzzlePiece piece = board[x, readY];
            if (piece == null) continue;
            if (readY != writeY)
            {
                board[x, readY] = null;
                board[x, writeY] = piece;
            }
            piece.SetCoordinates(x, writeY);
            writeY--;
        }
    }
}
public void RefillPieces(bool fixedPattern)
{
    for (int y = 0; y < height; y++)
    for (int x = 0; x < width; x++)
    {
        if (board[x, y] != null) continue;
        int index = fixedPattern ? (x + 2 * y) % 5 :
            Random.Range(0, attributeSprites.Length);
        PuzzlePiece piece = Instantiate(piecePrefab, piecesRoot, false);
        piece.Setup(x, y, (PuzzleAttribute)index, attributeSprites[index]);
        piece.Bind(this);
        RectTransform rect = piece.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
        rect.sizeDelta = new Vector2(cellSize, cellSize);
        rect.anchoredPosition = GetCellPosition(x, y - height);
        board[x, y] = piece;
    }
}
// モデル更新後、各石を登録済みのマスの中心へ動かす。
public IEnumerator AnimateToCells(float duration)
{
    var rects = new List<RectTransform>();
    var starts = new List<Vector2>();
    var goals = new List<Vector2>();
    for (int y = 0; y < height; y++)
    for (int x = 0; x < width; x++)
    {
        if (board[x, y] == null) continue;
        RectTransform rect = board[x, y].GetComponent<RectTransform>();
        rects.Add(rect);
        starts.Add(rect.anchoredPosition);
        goals.Add(GetCellPosition(x, y));
    }
    float elapsed = 0f;
    while (elapsed < duration)
    {
        elapsed += Time.unscaledDeltaTime;
        float t = Mathf.Clamp01(elapsed / duration);
        for (int i = 0; i < rects.Count; i++)
            rects[i].anchoredPosition = Vector2.Lerp(starts[i], goals[i], t);
        yield return null;
    }
    for (int i = 0; i < rects.Count; i++)
        rects[i].anchoredPosition = goals[i];
}
[ContextMenu("Lesson 5/Apply Two Step Test")]
private void ApplyTwoStepTest()
{
    if (!Application.isPlaying || board == null || IsOperating || InputLocked ||
        width != 6 || height != 6 || HasEmptyCells()) return;
    int[,] types =
    {
        { 0, 1, 2, 3, 4, 0 },
        { 2, 3, 4, 0, 1, 2 },
        { 4, 0, 1, 2, 3, 4 },
        { 1, 2, 0, 4, 0, 1 },
        { 3, 4, 0, 1, 2, 3 },
        { 1, 1, 0, 3, 4, 0 }
    };
    // typesは表を読みやすくするため[y,x]。boardは[x,y]。
    for (int y = 0; y < height; y++)
    for (int x = 0; x < width; x++)
    {
        int index = types[y, x];
        board[x, y].Setup(x, y, (PuzzleAttribute)index,
            attributeSprites[index]);
    }
    GetComponent<PuzzleMatchPreview>()?.ShowMatches();
}
}
