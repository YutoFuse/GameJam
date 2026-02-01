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

    [SerializeField] private ParticleSystem _system;

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
    [SerializeField] private float deadZone = 0.3f;
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
                face fromFace = ownerRoot.GetComponentInChildren<face>(true);
                int now_eye=fromFace.eye;
                int now_kuti=fromFace.kuti;
                Debug.Log("eye" + now_eye + "kuti" + now_kuti);
                if (now_eye != 999 && now_kuti != 999)
                {

                    dragging = true;
                    Show(true);
                    UpdateArrow(mouseWorld);
                }
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
            float dragDist = (releaseWorld - center).magnitude;
            if (dragDist < deadZone)
            {
                // deadZone未満なら何も起こさない
                dragging = false;
                HideArrowHard();
                OnReleased?.Invoke(this, dir);
                OnReleasedWithTarget?.Invoke(this, dir, null);
                return;
            }

            // 矢印の先端（tipPos）で判定する
            Collider2D pointed = Physics2D.OverlapPoint(tipPos, targetLayer);
            if (pointed != null && !IsOwnerCollider(pointed))
            {
                LastPointedCollider = pointed;
            }
            else
            {
                LastPointedCollider = null;
            }

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
            // faceコンポーネントでfromFaceと同じものは除外
            var ownerRoot = this.ownerRoot != null ? this.ownerRoot : transform.root;
            face fromFace = ownerRoot.GetComponentInChildren<face>(true);
            face toFace = c.GetComponentInParent<face>(true);
            if (fromFace != null && toFace != null && fromFace == toFace) continue;
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

        var board = field_create.Instance;
        if (board == null) return;

        Transform fromRoot = this.ownerRoot != null ? this.ownerRoot : transform.root;
        var fromCoord = fromRoot.GetComponent<GridCoord>();
        if (fromCoord == null) return;

        // targetのセルRoot（Square）側から座標を取る
        var targetCoord = target.GetComponentInParent<GridCoord>();
        if (targetCoord == null) return;

        Vector2Int d = dir switch
        {
            DragDirection.Up => Vector2Int.up,
            DragDirection.Down => Vector2Int.down,
            DragDirection.Left => Vector2Int.left,
            DragDirection.Right => Vector2Int.right,
            _ => Vector2Int.zero
        };

        board.TryMergeAndShift(
            new Vector2Int(fromCoord.x, fromCoord.y),
            new Vector2Int(targetCoord.x, targetCoord.y),
            d
        );
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
