using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PullArrowIndicator : MonoBehaviour
{
    public enum DragDirection { None, Up, Down, Left, Right }

    [System.Serializable]
    public class PullReleasedEvent : UnityEvent<PullArrowIndicator, DragDirection> { }

    [System.Serializable]
    public class PullReleasedTargetEvent : UnityEvent<PullArrowIndicator, DragDirection, Collider2D> { }

    [Header("Owner (Target)")]
    [SerializeField] private Transform owner;              // セルの中心（通常は親）
    [SerializeField] private Collider2D ownerCollider;     // セルのCollider2D（クリック開始判定）

    [Tooltip("このTransform配下にあるCollider/SpriteRendererを“自分”として扱う（セルRoot推奨）")]
    [SerializeField] private Transform ownerRoot;          // セルのルート（未設定ならownerを使う）

    [Header("Arrow Refs")]
    [SerializeField] private Transform arrowSprite;
    [SerializeField] private SpriteRenderer arrowRenderer;

    [Header("Arrow Tuning")]
    [SerializeField] private float maxLength = 1.5f;
    [SerializeField] private float minLength = 0.0f;
    [SerializeField] private float baseThickness = 0.25f;
    [SerializeField] private float angleOffsetDeg = 180f;
    [SerializeField] private bool invertDirection = false;
    [SerializeField] private float pivotTipLocalX = 0.5f;

    [Header("Direction Decide")]
    [SerializeField] private float deadZone = 0.15f;
    [SerializeField] private float axisConeDeg = 30f;

    [Header("Target Detect (Neighbor only)")]
    [SerializeField] private float cellStep = 1.0f;
    [SerializeField] private LayerMask targetLayer = ~0;
    [SerializeField] private float rayRadius = 0.05f;

    [Header("Result Effect")]
    [SerializeField] private Color targetHitColor = Color.yellow;

    [Tooltip("true: 消すとき Destroy / false: 非表示＆Collider無効化")]
    [SerializeField] private bool destroyOwner = false;

    [Tooltip("true: ownerRootを丸ごとSetActive(false)（消し残りが絶対出ない）")]
    [SerializeField] private bool deactivateOwnerRoot = true;

    [Header("Events")]
    public PullReleasedEvent OnReleased;
    public PullReleasedTargetEvent OnReleasedWithTarget;

    private Camera cam;
    private bool dragging;

    private float spriteWorldLengthX = 1f;
    private Vector3 center;
    private Vector3 tipPos;

    public Collider2D LastPointedCollider { get; private set; }
    public GameObject LastPointedObject => LastPointedCollider != null ? LastPointedCollider.gameObject : null;

    public int eye=999;
    public int kuti = 999;
    int target_number_eye=999;
    int target_number_kuti = 999;
    void Awake()
    {
        cam = Camera.main;

        if (arrowSprite == null)
        {
            Debug.LogWarning("[PullArrowIndicator] arrowSprite が未設定です", this);
            enabled = false;
            return;
        }

        if (owner == null) owner = transform.parent;
        if (ownerRoot == null) ownerRoot = owner;

        if (ownerCollider == null && owner != null)
            ownerCollider = owner.GetComponent<Collider2D>();

        if (arrowRenderer == null)
            arrowRenderer = arrowSprite.GetComponent<SpriteRenderer>();

        if (arrowRenderer != null && arrowRenderer.sprite != null)
            spriteWorldLengthX = arrowRenderer.sprite.bounds.size.x;

        HideArrowHard();
    }


    void OnDisable()
    {
        // 途中で無効化されても矢印が残らない保険
        HideArrowHard();
    }

    void Update()
    {
        if (Mouse.current == null) return;
        if (arrowSprite == null) return;

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

        if (dragging && Mouse.current.leftButton.isPressed)
        {
            UpdateArrow(GetMouseWorld());
        }

        if (dragging && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            Vector3 releaseWorld = GetMouseWorld();
            UpdateArrow(releaseWorld);

            DragDirection dir = DecideDirection(releaseWorld);

            center = (owner != null) ? owner.position : transform.position;
            LastPointedCollider = DetectNeighbor(dir);

            dragging = false;
            HideArrowHard();

            OnReleased?.Invoke(this, dir);
            OnReleasedWithTarget?.Invoke(this, dir, LastPointedCollider);

            ApplyEffect(dir, LastPointedCollider);
        }
    }

    private void UpdateArrow(Vector3 mouseWorld)
    {
        center = (owner != null) ? owner.position : transform.position;

        Vector3 v = mouseWorld - center;
        float rawDist = v.magnitude;

        // 見た目も deadZone 未満はほぼ出さない（残像対策にもなる）
        if (rawDist < deadZone)
        {
            tipPos = center;
            arrowSprite.localScale = new Vector3(0f, baseThickness, 1f);
            return;
        }

        Vector3 dir = (rawDist < 0.0001f) ? Vector3.right : v / rawDist;
        if (invertDirection) dir = -dir;

        float dist = Mathf.Clamp(rawDist, minLength, maxLength);
        tipPos = center + dir * dist;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + angleOffsetDeg;
        arrowSprite.rotation = Quaternion.Euler(0, 0, angle);

        float sx = (spriteWorldLengthX > 0.0001f) ? (dist / spriteWorldLengthX) : dist;
        arrowSprite.localScale = new Vector3(sx, baseThickness, 1f);

        float tipOffsetWorld = spriteWorldLengthX * sx * pivotTipLocalX;
        arrowSprite.position = tipPos - dir * tipOffsetWorld;
    }

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

    private Collider2D DetectNeighbor(DragDirection dir)
    {
        Vector2 d = dir switch
        {
            DragDirection.Up => Vector2.up,
            DragDirection.Down => Vector2.down,
            DragDirection.Left => Vector2.left,
            DragDirection.Right => Vector2.right,
            _ => Vector2.zero
        };

        if (d == Vector2.zero) return null;
        if (cellStep <= 0.0001f) return null;

        Vector3 origin = center;

        // 自分の外に押し出す（自分を最初に拾うのを防ぐ）
        if (ownerCollider != null)
        {
            var b = ownerCollider.bounds;
            float push = Mathf.Max(b.extents.x, b.extents.y) + rayRadius + 0.01f;
            origin = center + (Vector3)d * push;
        }
        else
        {
            origin = center + (Vector3)d * (rayRadius + 0.01f);
        }

        RaycastHit2D[] hits = Physics2D.CircleCastAll(origin, rayRadius, d, cellStep, targetLayer);
        if (hits == null || hits.Length == 0) return null;

        for (int i = 0; i < hits.Length; i++)
        {
            var c = hits[i].collider;
            if (c == null) continue;
            if (IsOwnerCollider(c)) continue;
            return c;
        }

        return null;
    }

    private bool IsOwnerCollider(Collider2D c)
    {
        if (c == null) return false;

        if (ownerCollider != null && c == ownerCollider) return true;

        if (ownerRoot != null)
        {
            if (c.transform == ownerRoot) return true;
            if (c.transform.IsChildOf(ownerRoot)) return true;
            if (ownerRoot.IsChildOf(c.transform)) return true;
        }

        if (owner != null)
        {
            if (c.transform == owner) return true;
            if (c.transform.IsChildOf(owner)) return true;
        }

        return false;
    }

    private void ApplyEffect(DragDirection dir, Collider2D target)
    {
        if (dir == DragDirection.None) return;
        if (target == null) return;

        Transform ownerRoot = this.ownerRoot != null ? this.ownerRoot : transform.root;
        Transform targetRoot = target.transform;

        face fromFace = ownerRoot.GetComponentInChildren<face>(true);
        face toFace   = targetRoot.GetComponentInChildren<face>(true);

        if (fromFace == null || toFace == null) return;
        if (fromFace.tekusutya == null || toFace.tekusutya == null) return;

        // ★ マスク考慮の一致判定
        bool eyeOK   = (fromFace.eye  == toFace.eye)  || fromFace.maskEye   || toFace.maskEye;
        bool mouthOK = (fromFace.kuti == toFace.kuti) || fromFace.maskMouth || toFace.maskMouth;

        if (!eyeOK || !mouthOK) return;

        // 成功：見た目移動（指したほうが残る）
        toFace.tekusutya.sprite = fromFace.tekusutya.sprite;
        toFace.tekusutya.enabled = (toFace.tekusutya.sprite != null);

        // 数値も移動
        toFace.eye  = fromFace.eye;
        toFace.kuti = fromFace.kuti;

        // ★ 成功したらマスクは外れる（仕様）
        toFace.ClearMasks(true); // 見た目も消す
        fromFace.ClearMasks(true);

        // 元を消す（持ったほうが消える）
        if (deactivateOwnerRoot && ownerRoot != null)
        {
            ownerRoot.gameObject.SetActive(false);
            return;
        }

        fromFace.tekusutya.sprite = null;
        fromFace.tekusutya.enabled = false;
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

    // 「Show(false)だけだと1フレーム残る」みたいな事故の保険
    private void HideArrowHard()
    {
        dragging = false;
        if (arrowSprite == null) return;
        arrowSprite.gameObject.SetActive(false);
        arrowSprite.localScale = new Vector3(0f, baseThickness, 1f);
    }
}
