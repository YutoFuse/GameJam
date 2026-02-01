using UnityEngine;
using UnityEngine.SceneManagement;

public class field_create : MonoBehaviour
{
    public int stage;

    [Header("Sprites")]
    public Sprite[] sprites;

    public GameObject background;
    public Transform parent;

    const int width = 3;
    const int height = 3;
    const float cellSize = 1f;

    public int total;
    [HideInInspector] public int[] spriteIndices;

    public void CreateField()
    {
        // 既存マス削除
        foreach (Transform child in parent)
        {
            Destroy(child.gameObject);
        }

        total = width * height;

        if (spriteIndices == null || spriteIndices.Length != total)
        {
            Debug.LogError("spriteIndices が不正です");
            return;
        }

        Vector2 origin;
        origin.x = -(width - 1) * cellSize / 2f;
        origin.y = -(height - 1) * cellSize / 2f;

        int index = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2 pos = origin + new Vector2(x * cellSize, y * cellSize);

                GameObject obj = Instantiate(background, parent);
                obj.transform.localPosition = pos;

                // ★ true を付ける：非アクティブな子にも対応
                face img = obj.GetComponentInChildren<face>(true);
                if (img == null)
                {
                    Debug.LogError("[field_create] face が見つかりません。backgroundプレハブ構造を確認してください", obj);
                    index++;
                    continue;
                }

                int spriteIndex = spriteIndices[index];

                // 範囲チェック
                if (sprites == null || spriteIndex < 0 || spriteIndex >= sprites.Length)
                {
                    Debug.LogError($"[field_create] spriteIndexが範囲外: {spriteIndex}", obj);
                    index++;
                    continue;
                }

                // 見た目
                img.tekusutya.sprite = sprites[spriteIndex];
                img.tekusutya.enabled = true; // 念のため

                // 目口パラメータ
                CalcEyeKuti(spriteIndex, out img.eye, out img.kuti);

                // ★ ここが肝：index16(黒マス)はマスクスロット無効化
                if (spriteIndex == 16)
                {
                    DisableMaskSlots(obj);
                }
                else
                {
                    EnableMaskSlots(obj); // もし前のステージで無効化されてた場合の保険
                }

                index++;
            }
        }
    }

    public void stick()
    {
        total--;
        Debug.Log(total);
        if (total == 1)
        {
            Invoke(nameof(CLEAR), 1.0f);
        }
    }

    void CLEAR()
    {
        SceneManager.LoadScene("GameClearScene");
    }

    void CalcEyeKuti(int spriteIndex, out int eye, out int kuti)
    {
        if (spriteIndex == 16)
        {
            eye = 999;
            kuti = 999;
        }
        else
        {
            const int kutiCount = 4;
            eye = spriteIndex / kutiCount;
            kuti = spriteIndex % kutiCount;
        }
    }

    // -------------------------
    // ★追加：黒マスはマスク吸着不可にする
    // -------------------------
    private void DisableMaskSlots(GameObject cellRoot)
    {
        var slots = cellRoot.GetComponentsInChildren<MaskSlotTrigger>(true);
        for (int i = 0; i < slots.Length; i++)
        {
            // スロット側のColliderを切る（OverlapCircleに拾われなくなる）
            var col = slots[i].GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            // ついでに Layer を MaskSlot 以外へ（保険）
            slots[i].gameObject.layer = LayerMask.NameToLayer("Default");
        }
    }

    // （任意）次のステージ生成時に元へ戻す用
    private void EnableMaskSlots(GameObject cellRoot)
    {
        var slots = cellRoot.GetComponentsInChildren<MaskSlotTrigger>(true);
        for (int i = 0; i < slots.Length; i++)
        {
            var col = slots[i].GetComponent<Collider2D>();
            if (col != null) col.enabled = true;

            // MaskSlotレイヤーを使ってるなら戻す
            int maskSlotLayer = LayerMask.NameToLayer("MaskSlot");
            if (maskSlotLayer >= 0) slots[i].gameObject.layer = maskSlotLayer;
        }
    }
}
