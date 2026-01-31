using UnityEngine;

// public enum SlotType { Eye, Mouth }

public class MaskSlotTrigger : MonoBehaviour
{
    public SlotType type;

    [Tooltip("未設定なら親から自動取得")]
    public face ownerFace;

    [Tooltip("未設定なら自分のTransformを使用（吸着位置）")]
    public Transform snapPoint;

    void Reset()
    {
        AutoAssign();
    }

    void Awake()
    {
        AutoAssign();
    }

    private void AutoAssign()
    {
        if (ownerFace == null)
            ownerFace = GetComponentInParent<face>(true);

        if (snapPoint == null)
            snapPoint = transform;

        if (ownerFace != null)
        {
            if (type == SlotType.Eye && ownerFace.eyeSlot == null)
                ownerFace.eyeSlot = snapPoint;
            else if (type == SlotType.Mouth && ownerFace.mouthSlot == null)
                ownerFace.mouthSlot = snapPoint;
        }   

        if (ownerFace == null)
            Debug.LogWarning($"[MaskSlotTrigger] ownerFace が親から見つかりません: {name}", this);
    }
}
