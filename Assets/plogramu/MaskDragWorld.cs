using UnityEngine;
using UnityEngine.InputSystem;

public class MaskDragWorld : MonoBehaviour
{
    private bool dragging;
    private Camera cam;
    private MaskSnapper snapper;

    void Awake()
    {
        cam = Camera.main;
        snapper = GetComponent<MaskSnapper>();
    }

    void Update()
    {
        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            // ここはあなたの「掴んだ判定」があるならそれを使う
            dragging = true;
        }

        if (dragging && Mouse.current.leftButton.isPressed)
        {
            // ここはあなたのドラッグ移動処理
        }

        if (dragging && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            dragging = false;
            Debug.Log("[MaskDragWorld] Released -> TrySnap");
            if (snapper != null) snapper.TrySnap();
            else Debug.Log("[MaskDragWorld] snapper is NULL");
        }

    }
}
