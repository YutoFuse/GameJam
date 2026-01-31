using UnityEngine;

public class face : MonoBehaviour
{
    public SpriteRenderer tekusutya;

    // 既にある前提（あなたのPullArrowIndicatorが参照している）
    public int eye;
    public int kuti;

    [Header("Mask State")]
    public bool maskEye;
    public bool maskMouth;

    [Header("Mask Visual Refs (optional)")]
    public Transform eyeSlot;    // Square内のEyeSlot
    public Transform mouthSlot;  // Square内のMouthSlot
    public GameObject eyeMaskObj;
    public GameObject mouthMaskObj;

    public void SetSprite(Sprite sprite)
    {
        tekusutya.sprite = sprite;
        tekusutya.enabled = (sprite != null);
    }

    public void AttachMask(SlotType type, GameObject maskObj)
    {
        if (type == SlotType.Eye)
        {
            maskEye = true;
            eyeMaskObj = maskObj;
            if (eyeSlot != null) maskObj.transform.position = eyeSlot.position;
            if (eyeSlot != null) maskObj.transform.SetParent(eyeSlot, true);
        }
        else
        {
            maskMouth = true;
            mouthMaskObj = maskObj;
            if (mouthSlot != null) maskObj.transform.position = mouthSlot.position;
            if (mouthSlot != null) maskObj.transform.SetParent(mouthSlot, true);
        }
    }

    public void ClearMasks(bool destroyVisual = true)
    {
        maskEye = false;
        maskMouth = false;

        if (destroyVisual)
        {
            if (eyeMaskObj != null) Destroy(eyeMaskObj);
            if (mouthMaskObj != null) Destroy(mouthMaskObj);
        }

        eyeMaskObj = null;
        mouthMaskObj = null;
    }
}

public enum SlotType { Eye, Mouth }
