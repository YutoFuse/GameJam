using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PullArrowIndicator : MonoBehaviour
{
    public enum DragDirection { None, Up, Down, Left, Right }

    [System.Serializable]
    public class PullReleasedEvent : UnityEvent<PullArrowIndicator, DragDirection> { }

    [Header("Owner (Target)")]
    [SerializeField] private Transform owner;              // 矢印の中心（通常は親）
    [SerializeField] private Collider2D ownerCollider;     // クリック開始判定（親のCollider）

    [Header("Arrow Refs")]
    [SerializeField] private Transform arrowSprite;        // 子の矢印Transform
    [SerializeField] private SpriteRenderer arrowRenderer; // 未設定なら自動取得

    [Header("Arrow Tuning")]
    [SerializeField] private float maxLength = 1.5f;
    [SerializeField] private float minLength = 0.0f;
    [SerializeField] private float baseThickness = 0.25f;
    [SerializeField] private float angleOffsetDeg = 180f;  // 左向き矢印なら180が合うことが多い
    [SerializeField] private bool invertDirection = false;

    [Tooltip("先端がスプライトのどの位置か。Pivot中心なら0.5、左端Pivotなら1.0、右端Pivotなら0.0")]
    [SerializeField] private float pivotTipLocalX = 0.5f;

    [Header("Direction Decide")]
    [Tooltip("これ未満のドラッグは None 扱い（誤爆防止）")]
    [SerializeField] private float deadZone = 0.15f;

    [Tooltip("軸(上下左右)に対してこの角度以内なら採用。小さいほど斜めに厳しくなる")]
    [SerializeField] private float axisConeDeg = 30f;

    [Header("Events")]
    public PullReleasedEvent OnReleased; // (source, direction)

    private Camera cam;
    private bool dragging;

    private float spriteWorldLengthX = 1f;
    private Vector3 center;
    private Vector3 tipPos;

    void Awake()
    {
        cam = Camera.main;

        if (arrowSprite == null)
        {
            Debug.LogWarning("[PullArrowIndicator] arrowSprite が未設定です", this);
            return;
        }

        // owner未設定なら親（Targetの子として使う前提）
        if (owner == null) owner = transform.parent;

        // ownerCollider未設定なら owner から取る
        if (ownerCollider == null && owner != null)
            ownerCollider = owner.GetComponent<Collider2D>();

        if (arrowRenderer == null)
            arrowRenderer = arrowSprite.GetComponent<SpriteRenderer>();

        if (arrowRenderer != null && arrowRenderer.sprite != null)
            spriteWorldLengthX = arrowRenderer.sprite.bounds.size.x;

        Show(false);
    }

    void Update()
    {
        if (Mouse.current == null) return;
        if (arrowSprite == null) return;

        // 押した：ownerCollider内だけ開始
        if (!dragging && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector3 mouseWorld = GetMouseWorld();
            if (ownerCollider != null && ownerCollider.OverlapPoint(mouseWorld))
            {
                dragging = true;
                Show(true);
                UpdateArrow(mouseWorld);
            }
        }

        // 押しっぱなし：矢印更新
        if (dragging && Mouse.current.leftButton.isPressed)
        {
            UpdateArrow(GetMouseWorld());
        }

        // 離した：directionだけ返す
        if (dragging && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            Vector3 releaseWorld = GetMouseWorld();
            DragDirection dir = DecideDirection(releaseWorld);

            dragging = false;
            Show(false);

            OnReleased?.Invoke(this, dir);
        }
    }

    // ====== 矢印の見た目更新 ======
    private void UpdateArrow(Vector3 mouseWorld)
    {
        center = (owner != null) ? owner.position : transform.position;

        Vector3 v = mouseWorld - center;
        Vector3 dir = (v.sqrMagnitude < 0.0001f) ? Vector3.right : v.normalized;
        if (invertDirection) dir = -dir;

        float dist = Mathf.Clamp(v.magnitude, minLength, maxLength);

        tipPos = center + dir * dist;

        // 回転
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + angleOffsetDeg;
        arrowSprite.rotation = Quaternion.Euler(0, 0, angle);

        // X方向に伸ばす（Yは固定）
        float sx = (spriteWorldLengthX > 0.0001f) ? (dist / spriteWorldLengthX) : dist;
        arrowSprite.localScale = new Vector3(sx, baseThickness, 1f);

        // 先端を tipPos に合わせる（ピボット位置補正）
        float tipOffsetWorld = spriteWorldLengthX * sx * pivotTipLocalX;
        arrowSprite.position = tipPos - dir * tipOffsetWorld;
    }

    // ====== 方向決定（勝手に斜めを左右上下にしない） ======
    private DragDirection DecideDirection(Vector3 releaseWorld)
    {
        center = (owner != null) ? owner.position : transform.position;
        Vector2 v = (Vector2)(releaseWorld - center);

        float mag = v.magnitude;
        if (mag < deadZone) return DragDirection.None;

        float ax = Mathf.Abs(v.x);
        float ay = Mathf.Abs(v.y);

        bool horizontal = ax >= ay;

        float degFromAxis = horizontal
            ? Mathf.Atan2(ay, ax) * Mathf.Rad2Deg
            : Mathf.Atan2(ax, ay) * Mathf.Rad2Deg;

        if (degFromAxis > axisConeDeg) return DragDirection.None;

        if (horizontal)
            return (v.x >= 0f) ? DragDirection.Right : DragDirection.Left;
        else
            return (v.y >= 0f) ? DragDirection.Up : DragDirection.Down;
    }

    private Vector3 GetMouseWorld()
    {
        Vector2 screen = Mouse.current.position.ReadValue();
        Vector3 p = new Vector3(screen.x, screen.y, Mathf.Abs(cam.transform.position.z));
        Vector3 w = cam.ScreenToWorldPoint(p);
        w.z = transform.position.z;
        return w;
    }

    private void Show(bool on)
    {
        if (arrowSprite != null) arrowSprite.gameObject.SetActive(on);
    }
}
