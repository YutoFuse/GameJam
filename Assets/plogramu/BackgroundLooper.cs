using UnityEngine;

public class BackgroundLooper : MonoBehaviour
{
    [Header("Assign Sprites (per stage)")]
    [SerializeField] private Sprite spriteA;
    [SerializeField] private Sprite spriteB;

    [Header("SpriteRenderers")]
    [SerializeField] private SpriteRenderer srA;
    [SerializeField] private SpriteRenderer srB;

    [Header("Scroll")]
    [SerializeField] private float speed = 2f;   // 左へ流れる速度（ワールド単位/秒）
    [SerializeField] private float y = 0f;       // 背景のY固定
    [SerializeField] private float z = 10f;      // 背景のZ固定（カメラより奥/手前調整）

    [Header("Camera")]
    [SerializeField] private Camera cam;         // nullならMain Camera

    [Tooltip("1px隙間が気になる時にON（Orthographic向け）")]
    [SerializeField] private bool snapToPixel = false;

    private float widthA;

    private void Awake()
    {
        if (cam == null) cam = Camera.main;

        // 参照が未設定なら子から自動取得（A/Bの名前が違ってもOK）
        if (srA == null || srB == null)
        {
            var srs = GetComponentsInChildren<SpriteRenderer>();
            if (srs.Length >= 2)
            {
                srA = srs[0];
                srB = srs[1];
            }
        }

        ApplySprites();
    }

    /// ステージ開始時などに呼べば差し替え可能
    public void SetStageBackground(Sprite a, Sprite b)
    {
        spriteA = a;
        spriteB = b;
        ApplySprites();
    }

    private void ApplySprites()
    {
        if (srA == null || srB == null) return;
        if (spriteA == null || spriteB == null) return;

        srA.sprite = spriteA;
        srB.sprite = spriteB;

        // Aの幅（ワールド）を基準にBを右へ置く
        widthA = srA.bounds.size.x;

        srA.transform.position = new Vector3(0f, y, z);
        srB.transform.position = new Vector3(widthA, y, z);
    }

    private void Update()
    {
        if (srA == null || srB == null) return;
        if (srA.sprite == null || srB.sprite == null) return;

        float dx = speed * Time.deltaTime;
        srA.transform.position += Vector3.left * dx;
        srB.transform.position += Vector3.left * dx;

        if (snapToPixel) SnapToPixel(srA.transform, srB.transform);

        // カメラ左端（ワールド）を求める：背景のZ平面で計算
        float dist = Mathf.Abs(cam.transform.position.z - z);
        if (dist < 0.01f) dist = 10f;

        float camLeft = cam.ScreenToWorldPoint(new Vector3(0, 0, dist)).x;

        RecycleIfOut(srA, srB, camLeft);
        RecycleIfOut(srB, srA, camLeft);
    }

    private void RecycleIfOut(SpriteRenderer me, SpriteRenderer other, float camLeft)
    {
        // meの右端がカメラ左端より左に行ったら＝完全に画面外
        float myRightEdge = me.transform.position.x + me.bounds.extents.x;

        if (myRightEdge < camLeft)
        {
            float otherRightEdge = other.transform.position.x + other.bounds.extents.x;

            // otherの右端のさらに右へ、ピッタリ繋ぐ
            Vector3 p = me.transform.position;
            p.x = otherRightEdge + me.bounds.extents.x;
            me.transform.position = p;
        }
    }

    private void SnapToPixel(Transform a, Transform b)
    {
        if (cam == null || !cam.orthographic) return;

        // 1pxあたりのワールド長（縦基準）
        float worldPerPixel = (cam.orthographicSize * 2f) / Screen.height;

        a.position = new Vector3(
            Mathf.Round(a.position.x / worldPerPixel) * worldPerPixel,
            a.position.y, a.position.z
        );

        b.position = new Vector3(
            Mathf.Round(b.position.x / worldPerPixel) * worldPerPixel,
            b.position.y, b.position.z
        );
    }
}
