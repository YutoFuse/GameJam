using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Collider2D))]
public class DragSpriteWithMouse : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private bool onlyDragWhenHit = true;
    [SerializeField] private SpriteRenderer spriteRenderer; // 未設定なら自動取得
    [SerializeField] private float paddingPixels = 0f;      // 余白（px）。少し内側にしたい時


    private Collider2D col;
    private bool dragging;
    private Vector3 grabOffset;

    private void Awake()
    {
        if (cam == null) cam = Camera.main;
        col = GetComponent<Collider2D>();

        if (cam == null)
            Debug.LogError("[DragSpriteWithMouse] Camera.main が見つかりません。MainCameraタグを確認。", this);

        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

    }

    private void Update()
    {
        if (cam == null) return;

        // --- 入力取得（新旧どちらでも） ---
        bool down, hold, up;
        Vector2 screenPos;

#if ENABLE_INPUT_SYSTEM
        if (Mouse.current == null)
        {
            // Input System が無効 or 初期化されてない
            return;
        }
        down = Mouse.current.leftButton.wasPressedThisFrame;
        hold = Mouse.current.leftButton.isPressed;
        up   = Mouse.current.leftButton.wasReleasedThisFrame;
        screenPos = Mouse.current.position.ReadValue();
#else
        down = Input.GetMouseButtonDown(0);
        hold = Input.GetMouseButton(0);
        up   = Input.GetMouseButtonUp(0);
        screenPos = Input.mousePosition;
#endif

        if (!dragging && down)
        {
            Vector3 mw = ScreenToWorldOnMyZ(screenPos);

            if (!onlyDragWhenHit || (col != null && col.OverlapPoint(mw)))
            {
                dragging = true;
                grabOffset = transform.position - mw;
            }
        }

        if (dragging && hold)
        {
            Vector3 mw = ScreenToWorldOnMyZ(screenPos);
            var targetPos = mw + grabOffset;
            transform.position = ClampToScreen(targetPos);  
      }

        if (dragging && up)
        {
            dragging = false;
        }
    }

    private Vector3 ClampToScreen(Vector3 worldPos)
    {
        if (spriteRenderer == null) return worldPos;

        // スプライトの半サイズ（ワールド単位）
        Vector3 ext = spriteRenderer.bounds.extents;

        // 画面の端（ワールド座標）を取得
        // zはカメラからオブジェクト平面までの距離が必要
        float dist = Mathf.Abs(cam.transform.position.z - transform.position.z);
        if (dist < 0.01f) dist = Mathf.Abs(cam.nearClipPlane + 1f);

        // paddingPixels をワールドに変換（縦方向基準）
        float padWorldY = 0f;
        float padWorldX = 0f;
        if (paddingPixels > 0f)
        {
            // 1pxがワールドでどれくらいか（その距離の平面で）
            Vector3 a = cam.ScreenToWorldPoint(new Vector3(0, 0, dist));
            Vector3 b = cam.ScreenToWorldPoint(new Vector3(1, 1, dist));
            Vector3 perPx = b - a;
            padWorldX = perPx.x * paddingPixels;
            padWorldY = perPx.y * paddingPixels;
        }

        Vector3 bottomLeft = cam.ScreenToWorldPoint(new Vector3(0, 0, dist));
        Vector3 topRight   = cam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, dist));

        float minX = bottomLeft.x + ext.x + padWorldX;
        float maxX = topRight.x   - ext.x - padWorldX;
        float minY = bottomLeft.y + ext.y + padWorldY;
        float maxY = topRight.y   - ext.y - padWorldY;

        worldPos.x = Mathf.Clamp(worldPos.x, minX, maxX);
        worldPos.y = Mathf.Clamp(worldPos.y, minY, maxY);
        worldPos.z = transform.position.z;
        return worldPos;
    }

    private Vector3 ScreenToWorldOnMyZ(Vector2 screen)
    {
        // dist=0になるのを避ける：最低でもnearClipより大きく
        float dist = Mathf.Abs(cam.transform.position.z - transform.position.z);
        if (dist < 0.01f) dist = Mathf.Abs(cam.nearClipPlane + 1f);

        Vector3 p = new Vector3(screen.x, screen.y, dist);
        Vector3 w = cam.ScreenToWorldPoint(p);
        w.z = transform.position.z;
        return w;
    }
}
