using UnityEngine;

public class Draggable : MonoBehaviour, IDraggable
{
    [Header("Drag Settings")]
    [SerializeField] private bool canDrag = true;
    [SerializeField] private float dragAlpha = 0.8f;

    [Header("Sprites")]
    public Sprite originalSprite;
    public Sprite replacementSprite;
    public Sprite SauceSprite;

    // 인터페이스 구현
    public bool CanDrag => canDrag && Time.timeScale > 0;
    public string ItemName => name.Replace("(Clone)", "").Trim();

    
    public bool isDraggable
    {
        get => canDrag;
        set => canDrag = value;
    }

    public Vector3 originalPosition { get; private set; }
    public Vector3 originalScale { get; private set; }

    private SpriteRenderer spriteRenderer;
    private Camera mainCamera;
    private bool isDragging;
    private Vector3 offset;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainCamera = Camera.main;
        originalPosition = transform.position;
        originalScale = transform.localScale;
    }

    private void Start()
    {
        SetTransparency(0f);
    }

    private void OnMouseDown()
    {
        if (CanDrag)
        {
            OnDragStart();
        }
    }

    private void OnMouseDrag()
    {
        if (isDragging)
        {
            UpdateDragPosition();
        }
    }

    private void OnMouseUp()
    {
        if (isDragging)
        {
            OnDragEnd();
        }
    }

    public void OnDragStart()
    {
        if (!CanDrag) return;

        isDragging = true;
        offset = transform.position - GetMouseWorldPosition();
        SetTransparency(dragAlpha);

        if (originalSprite != null)
        {
            spriteRenderer.sprite = originalSprite;
        }
    }

    public void OnDragEnd()
    {
        isDragging = false;

        bool wasDropped = TryDropOnValidArea();

        if (!wasDropped)
        {
            transform.position = originalPosition;
            SetTransparency(0f);
        }
    }

    private void UpdateDragPosition()
    {
        Vector3 targetPosition = GetMouseWorldPosition() + offset;
        transform.position = Vector3.Lerp(transform.position, targetPosition, 0.2f);
    }

    private bool TryDropOnValidArea()
    {
        var hit = Physics2D.OverlapPoint(transform.position);
        if (hit != null && hit.CompareTag("DropArea"))
        {
            var droppable = hit.GetComponent<IDroppable>();
            if (droppable != null && droppable.CanAcceptDrop(this))
            {
                droppable.OnItemDropped(this);
                return true;
            }
        }
        return false;
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = mainCamera.WorldToScreenPoint(transform.position).z;
        return mainCamera.ScreenToWorldPoint(mousePos);
    }

    private void SetTransparency(float alpha)
    {
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }
    }
}