using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Collider2D))]
public class DragSpriteWithMouse : MonoBehaviour
{
    [Header("Refs")]
    public Camera cam;
    public bool onlyDragWhenHit = true;
    public SpriteRenderer spriteRenderer;
    public float paddingPixels = 0f;

    [Header("Snap")]
    [SerializeField] private MaskSnapper snapper; // 未設定なら自動取得

    [Header("Render (Bring to front while dragging)")]
    [SerializeField] private int dragSortingOrder = 100;   // ドラッグ中は前に
    private int originalSortingOrder;
    private int originalSortingLayerID;

    private Collider2D col;
    private bool dragging;
    private Vector3 grabOffset;

    void Awake()
    {
        if (cam == null) cam = Camera.main;
        col = GetComponent<Collider2D>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (snapper == null)
            snapper = GetComponent<MaskSnapper>(); // 同じGOに付いてる想定

        if (spriteRenderer != null)
        {
            originalSortingOrder = spriteRenderer.sortingOrder;
            originalSortingLayerID = spriteRenderer.sortingLayerID;
        }
    }

    void Update()
    {
        var pressed = GetMousePressedThisFrame();
        var held = GetMouseHeld();
        var released = GetMouseReleasedThisFrame();

        if (!dragging && pressed)
        {
            Vector3 mw = GetMouseWorld();
            if (!onlyDragWhenHit || HitTest(mw))
            {
                dragging = true;
                grabOffset = transform.position - mw;

                // ドラッグ中は前に出す（裏に回る対策）
                BringToFrontWhileDragging();
            }
        }

        if (dragging && held)
        {
            Vector3 mw = GetMouseWorld();
            transform.position = mw + grabOffset;
        }

        if (dragging && released)
        {
            dragging = false;

            // 元の描画順へ戻す（吸着したらMaskSnapper側で上書きしてもOK）
            RestoreSorting();

            // ★ ここが重要：離した瞬間にスナップを呼ぶ
            Debug.Log("[DragSpriteWithMouse] Released -> TrySnap", this);
            if (snapper != null) snapper.TrySnap();
            else Debug.LogWarning("[DragSpriteWithMouse] snapper is NULL", this);
        }
    }

    private bool HitTest(Vector3 mouseWorld)
    {
        if (col == null) return false;

        // Collider2DのOverlapPointで判定（最も確実）
        if (col.OverlapPoint(mouseWorld)) return true;

        // paddingPixels が欲しい場合：SpriteRenderer.boundsで少し広げて判定
        if (spriteRenderer != null && paddingPixels > 0f && cam != null)
        {
            float worldPerPixel = cam.orthographic
                ? (cam.orthographicSize * 2f) / Screen.height
                : 0f;

            float padWorld = paddingPixels * worldPerPixel;
            var b = spriteRenderer.bounds;
            b.Expand(new Vector3(padWorld, padWorld, 0f));
            return b.Contains(mouseWorld);
        }

        return false;
    }

    private void BringToFrontWhileDragging()
    {
        if (spriteRenderer == null) return;
        spriteRenderer.sortingOrder = dragSortingOrder;
    }

    private void RestoreSorting()
    {
        if (spriteRenderer == null) return;
        spriteRenderer.sortingLayerID = originalSortingLayerID;
        spriteRenderer.sortingOrder = originalSortingOrder;
    }

    private Vector3 GetMouseWorld()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            Vector2 screen = Mouse.current.position.ReadValue();
            var p = new Vector3(screen.x, screen.y, Mathf.Abs(cam.transform.position.z));
            var w = cam.ScreenToWorldPoint(p);
            w.z = transform.position.z;
            return w;
        }
#endif
        // 旧Input fallback
        var p2 = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Mathf.Abs(cam.transform.position.z));
        var w2 = cam.ScreenToWorldPoint(p2);
        w2.z = transform.position.z;
        return w2;
    }

    private bool GetMousePressedThisFrame()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
#else
        return Input.GetMouseButtonDown(0);
#endif
    }

    private bool GetMouseHeld()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null && Mouse.current.leftButton.isPressed;
#else
        return Input.GetMouseButton(0);
#endif
    }

    private bool GetMouseReleasedThisFrame()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame;
#else
        return Input.GetMouseButtonUp(0);
#endif
    }
}
