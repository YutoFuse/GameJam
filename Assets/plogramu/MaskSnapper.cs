using UnityEngine;

public class MaskSnapper : MonoBehaviour
{
    [SerializeField] private LayerMask slotLayer;
    [SerializeField] private float searchRadius = 0.8f;

    public bool snapped;
    public face snappedFace;
    public SlotType snappedType;

     public void TrySnap()
    {
        Debug.Log("[MaskSnapper] TrySnap called", this);

        // ★追加：Overlapの件数を見る
        var hits = Physics2D.OverlapCircleAll(transform.position, searchRadius, slotLayer);
        Debug.Log($"[MaskSnapper] hits={hits.Length} pos={transform.position} r={searchRadius} layer={slotLayer.value}", this);

        if (snapped) return;
        if (hits == null || hits.Length == 0) return;

        MaskSlotTrigger best = null;
        float bestDist = float.MaxValue;

        foreach (var h in hits)
        {
            // ★重要：Colliderに直で付いてないことが多いので Parent も見る
            var slot = h.GetComponent<MaskSlotTrigger>();
            if (slot == null) slot = h.GetComponentInParent<MaskSlotTrigger>(); // 追加

            if (slot == null)
            {
                Debug.Log($"[MaskSnapper] hit {h.name} BUT no MaskSlotTrigger", h);
                continue;
            }

            if (slot.ownerFace == null)
            {
                Debug.Log($"[MaskSnapper] slot found but ownerFace is NULL : {slot.name}", slot);
                continue;
            }

            // 既に埋まってるスロットは弾く
            if (slot.type == SlotType.Eye && slot.ownerFace.maskEye) continue;
            if (slot.type == SlotType.Mouth && slot.ownerFace.maskMouth) continue;

            float d = Vector2.Distance(transform.position, slot.transform.position);
            Debug.Log($"[MaskSnapper] candidate {slot.name} dist={d}", slot);

            if (d < bestDist)
            {
                bestDist = d;
                best = slot;
            }
        }

        if (best == null)
        {
            Debug.Log("[MaskSnapper] best is NULL (no valid slot)", this);
            return;
        }

        // ★ピタッ：まず位置を合わせる（AttachMaskが未完成でも視覚的に分かる）
        transform.position = best.transform.position;

        snapped = true;
        snappedFace = best.ownerFace;
        snappedType = best.type;

        snappedFace.AttachMask(snappedType, gameObject);
    }

}
