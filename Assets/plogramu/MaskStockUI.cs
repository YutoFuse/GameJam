using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MaskStockUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Stock")]
    [SerializeField] private int stock = 3;

    [Tooltip("0,1,2,3...の順に入れる（Assets/image/Numbers のSprite）")]
    [SerializeField] private Sprite[] numberSprites; // index=残数

    [SerializeField] private Image countImage; // 数字を表示するUI Image

    [Header("Spawn")]
    [SerializeField] private GameObject maskPrefab;   // ワールドに出すSpriteのPrefab
    [SerializeField] private Transform worldParent;   // 生成先（未設定ならシーン直下）
    [SerializeField] private Camera worldCamera;      // nullならMainCamera

    [Header("Drag Plane")]
    [Tooltip("ワールドのマスクを置くZ（2Dなら0でOK。カメラが-10なら0が見える）")]
    [SerializeField] private float worldZ = 0f;

    private GameObject currentMask;
    private bool dragging;

    private void Awake()
    {
        if (worldCamera == null) worldCamera = Camera.main;
        RefreshCountUI();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (stock <= 0) return;
        if (maskPrefab == null) return;

        dragging = true;

        // 生成
        currentMask = Instantiate(maskPrefab, worldParent);
        // 初期位置
        SetMaskPositionToPointer(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!dragging) return;
        if (currentMask == null) return;

        SetMaskPositionToPointer(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log($"[MaskStockUI] OnEndDrag called dragging={dragging} currentMask={(currentMask ? currentMask.name : "NULL")}");

        var snap = currentMask.GetComponent<MaskSnapper>();
        if (snap != null) snap.TrySnap();

        if (!dragging) return;

        dragging = false;

        if (currentMask != null)
        {
            // とりあえず「離したら在庫-1」仕様（成功/失敗判定は後で）
            stock = Mathf.Max(0, stock - 1);
            RefreshCountUI();

            // 在庫0なら掴めないようにする（任意）
            if (stock == 0)
            {
                // このUIをクリック不可にしたいならRaycastを切る
                var img = GetComponent<Image>();
                if (img != null) img.raycastTarget = false;
            }

            // ここでは currentMask は残す（場に置かれる）
            currentMask = null;
        }
    }

    private void SetMaskPositionToPointer(PointerEventData eventData)
    {
        if (worldCamera == null) return;

        Vector3 screen = eventData.position;

        // ScreenToWorldPointはzに「カメラからの距離」が必要なので、worldZに合わせて距離を作る
        float dist = Mathf.Abs(worldCamera.transform.position.z - worldZ);
        Vector3 w = worldCamera.ScreenToWorldPoint(new Vector3(screen.x, screen.y, dist));
        w.z = worldZ;

        currentMask.transform.position = w;
    }

    private void RefreshCountUI()
    {
        if (countImage == null) return;
        if (numberSprites == null || numberSprites.Length == 0) return;

        int idx = Mathf.Clamp(stock, 0, numberSprites.Length - 1);
        countImage.sprite = numberSprites[idx];

        // 0の時は非表示にしたいならここで調整
        // countImage.enabled = stock > 0;
    }
}
