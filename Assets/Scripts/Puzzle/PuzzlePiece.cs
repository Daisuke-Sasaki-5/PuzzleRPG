using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
[RequireComponent(typeof(Image))]
public class PuzzlePiece : MonoBehaviour,
    IPointerDownHandler, IDragHandler,
    IPointerUpHandler, IEndDragHandler
{
    [SerializeField] private Image image;
    private PuzzleBoard owner;
    public int X { get; private set; }
    public int Y { get; private set; }
    public PuzzleAttribute Type { get; private set; }
    public void Setup(int x, int y,
        PuzzleAttribute type, Sprite sprite)
    {
        SetCoordinates(x, y);
        Type = type;
        if (image == null) image = GetComponent<Image>();
        image.sprite = sprite;
        image.color = Color.white;
        image.preserveAspect = true;
        image.raycastTarget = true;
    }
    public void Bind(PuzzleBoard board)
    {
        owner = board;
    }
    public void SetCoordinates(int x, int y)
    {
        X = x;
        Y = y;
        name = $"Piece_{x}_{y}";
    }
    public void SetSelected(bool selected)
    {
        if (image == null) image = GetComponent<Image>();
        image.color = selected
            ? new Color(0.7f, 1f, 1f, 1f) : Color.white;
    }
    public void OnPointerDown(PointerEventData data)
    {
        if (owner != null) owner.BeginGesture(this, data);
    }
    public void OnDrag(PointerEventData data)
    {
        if (owner != null) owner.DragGesture(data);
    }
    public void OnPointerUp(PointerEventData data)
    {
        if (owner != null) owner.EndGesture(data);
    }
    public void OnEndDrag(PointerEventData data)
    {
        if (owner != null) owner.EndGesture(data);
    }
    // Retain the previous lesson's public APIs; neither is used by gestures.
    public void SetAttribute(PuzzleAttribute type, Sprite sprite)
    {
        if (image == null) image = GetComponent<Image>();
        Type = type;
        image.sprite = sprite;
    }
    public void SetMatched(bool matched)
    {
        if (image == null) image = GetComponent<Image>();
        image.color = matched ? new Color(1f, .65f, .65f, 1f) : Color.white;
    }
}
