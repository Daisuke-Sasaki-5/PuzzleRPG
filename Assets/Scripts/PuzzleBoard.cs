using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PuzzleBoard : MonoBehaviour
{
    [SerializeField] private PuzzlePiece piecePrefab;
    [SerializeField] private RectTransform piecesRoot;
    [SerializeField] private Sprite[] attributeSprites = new Sprite[5];
    [SerializeField, Range(1, 6)] private int width = 6;
    [SerializeField, Range(1, 6)] private int height = 6;

    private const float Spacing = 0f;
    private float cellSize = 100f;
    private Vector2 firstCenter;
    private PuzzlePiece[,] board;
    [SerializeField] private bool allowExchange = false;
    [SerializeField] private bool highlightSelection = false;
    private PuzzlePiece selected;
    private int activePointerId;
    private Vector2 previousPointer;
    private int selectedSibling;
    private PuzzlePiece[,] gestureStart;
    private bool pathChanged;

    public PuzzlePiece[,] Pieces => board;
    public bool IsResolving => GetComponent<PuzzleResolver>()?.IsBusy ?? false;
    public bool IsOperating => selected != null || IsResolving;

    public bool InputLocked
    {
        get;
        set;
    }

    private void Start()
    {
        Build();
    }

    private void Build()
    {
        if (piecePrefab == null || piecesRoot == null || attributeSprites == null || attributeSprites.Length != 5)
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

        if (board != null) return;
        width = 6;
        height = 6;
        ConfigureCells();
        board = new PuzzlePiece[width, height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                PuzzleAttribute type;
                do
                {
                    type = (PuzzleAttribute)Random.Range(0, 5);
                }
                while ((x >= 2 && board[x - 1, y].Type == type && board[x - 2, y].Type == type) || (y >= 2 && board[x, y - 1].Type == type && board[x, y - 2].Type == type));
                PuzzlePiece piece = Instantiate(piecePrefab, piecesRoot, false);
                piece.name = $"Piece_{x}_{y}";
                piece.Setup(x, y, type, attributeSprites[(int)type]);
                piece.Bind(this);
                RectTransform rect = piece.GetComponent<RectTransform>();
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
        PuzzlePiece first = board[0, 0];
        Debug.Log($"左上 {first.name} " + $"座標({first.X},{first.Y}) 属性{first.Type}", this);
    }


// マスの座標を、親の左上からの表示位置へ変換。
// 生成時だけでなく、交換・落下の移動先にも使う。
public Vector2 GetCellPosition(int x, int y)
    {
        float step = cellSize + Spacing;
        return new Vector2(
            firstCenter.x + x * step,
            -(firstCenter.y + y * step));
    }
    private void ConfigureCells()
    {
        cellSize = 100f;
        firstCenter = new Vector2(50f, 50f);
    }
    public void BeginGesture(PuzzlePiece piece, PointerEventData data)
    {
        if (!isActiveAndEnabled || board == null || IsOperating || InputLocked || HasEmptyCells() || piece == null || data.button != PointerEventData.InputButton.Left) return;
        if (!Inside(piece.X, piece.Y) || board[piece.X, piece.Y] != piece) return;
        if (!TryPointer(data, out Vector2 pointer)) return;
        gestureStart = (PuzzlePiece[,])board.Clone();
        pathChanged = false;
        selected = piece;
        activePointerId = data.pointerId;
        previousPointer = pointer;
        RectTransform rect = selected.GetComponent<RectTransform>();
        selectedSibling = rect.GetSiblingIndex();
        rect.SetAsLastSibling();
        selected.SetSelected(highlightSelection);
        rect.anchoredPosition = pointer;
        Debug.Log($"選択 ({piece.X},{piece.Y}) {piece.Type}", this);
    }

public void DragGesture(PointerEventData data)
    {
        if (!isActiveAndEnabled || selected == null || data.pointerId != activePointerId) return;
        if (!TryPointer(data, out Vector2 pointer)) return;
        if (allowExchange) FollowPath(previousPointer, pointer);
        previousPointer = pointer;
        selected.GetComponent<RectTransform>().anchoredPosition = pointer;
    }

    public void EndGesture(PointerEventData data)
    {
        if (selected == null || data.pointerId != activePointerId) return;
        DragGesture(data);
        bool commit = pathChanged;
        FinishGesture();
        ValidateBoard();
        if (commit)
        {
            InputLocked = true;
            if (!GetComponent<PuzzleResolver>().TryResolve()) Debug.LogError("Resolution could not start. Reset battle.", this);
        }
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

    // Focus loss/pause cancels and restores the entire uncommitted path.
    private void CancelGesture()
    {
        if (selected != null && gestureStart != null)
        {
            board = gestureStart;
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    board[x, y].SetCoordinates(x, y);
                    board[x, y].GetComponent<RectTransform>().anchoredPosition = GetCellPosition(x, y);
                }
        }
        FinishGesture();
    }

    private void OnDisable() { CancelGesture(); }
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

    // Returns pointer position in the left/top anchored UI coordinate system.
    private bool TryPointer(PointerEventData data, out Vector2 pointer)
    {
        pointer = default;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            piecesRoot, data.position, data.pressEventCamera, out Vector2 local)) return false;
        Rect area = piecesRoot.rect;
        float contentWidth = width * cellSize + (width - 1) * Spacing;
        float contentHeight = height * cellSize + (height - 1) * Spacing;
        pointer = new Vector2(
            Mathf.Clamp(local.x - area.xMin, 0f, contentWidth),
            -Mathf.Clamp(area.yMax - local.y, 0f, contentHeight));
        return true;
    }

    // Traverse the segment's grid boundaries in order, including fast events.
    // An exact corner crossing exchanges directly with the diagonal cell.
    private void FollowPath(Vector2 from, Vector2 to)
    {
        double dx = (double)to.x - from.x, dy = (double)from.y - to.y;
        int x = selected.X, y = selected.Y;
        int endX = Mathf.Clamp(Mathf.FloorToInt(to.x / cellSize), 0, width - 1);
        int endY = Mathf.Clamp(Mathf.FloorToInt(-to.y / cellSize), 0, height - 1);
        int sx = dx > 0 ? 1 : dx < 0 ? -1 : 0;
        int sy = dy > 0 ? 1 : dy < 0 ? -1 : 0;
        while (x != endX || y != endY)
        {
            // Compare crossing times by cross products, recomputed from the
            // original segment: no accumulated t error near an exact corner.
            if (x == endX) y += sy;
            else if (y == endY) x += sx;
            else
            {
                double distX = (((sx > 0 ? x + 1 : x) * (double)cellSize) - from.x) * sx;
                double distY = (((sy > 0 ? y + 1 : y) * (double)cellSize) + from.y) * sy;
                double crossX = distX * System.Math.Abs(dy);
                double crossY = distY * System.Math.Abs(dx);
                if (crossX == crossY) { x += sx; y += sy; }
                else if (crossX < crossY) x += sx;
                else y += sy;
            }
            if (!Inside(x, y) || !TrySwap(selected, board[x, y])) return;
        }
    }

    private bool TrySwap(PuzzlePiece a, PuzzlePiece b)
    {
        if (a == null || b == null || a == b) return false;
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
        // The held piece follows the pointer; only the displaced piece snaps.
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
        if (board == null) return;
        var seen = new HashSet<PuzzlePiece>();
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                PuzzlePiece piece = board[x, y];
                if (piece == null || !seen.Add(piece) || piece.X != x || piece.Y != y)
                {
                    Debug.LogError($"配列と石の座標が不一致 ({x},{y})", this);
                    return;
                }


                Vector2 pos = piece.GetComponent<RectTransform>().anchoredPosition;
                if (piece != selected && Vector2.Distance(pos, GetCellPosition(x, y)) > 0.01f)
                {
                    Debug.LogError($"表示位置が不一致 ({x},{y})", this);
                    return;
                }
            }
        }
    }
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

    public void ResetBoard()
    {
        GetComponent<PuzzleResolver>()?.CancelResolution();
        GetComponent<PuzzleResolver>()?.ResetResolutionState();
        CancelGesture();
        if (piecesRoot != null) foreach (Transform child in piecesRoot)
            {
                child.gameObject.SetActive(false);
                Destroy(child.gameObject);
            }
        board = null;
        InputLocked = false;
        Build();
    }

}
