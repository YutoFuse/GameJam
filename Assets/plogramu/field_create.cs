using UnityEngine;
using UnityEngine.SceneManagement;

public class field_create : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite[] sprites;

    [Header("Prefabs")]
    public GameObject face_image;
    public GameObject background;

    [Header("Parents")]
    public Transform parent;   // 顔マス
    public Transform parent2;  // 背景マス

    const int width = 3;
    const int height = 3;
    const float cellSize = 1f;

    int total;
    [HideInInspector] public int[] spriteIndices;

    // -----------------------------
    // フィールド生成
    // -----------------------------
    public void CreateField()
    {
        // MaskStockUI 初期化
        MaskStockUI musk = null;
        GameObject stock = GameObject.Find("MaskImage");
        if (stock != null)
        {
            musk = stock.GetComponent<MaskStockUI>();
            musk.stock = 3;
            musk.RefreshCountUI();
        }

        // 既存マス削除
        ClearChildren(parent);
        ClearChildren(parent2);

        total = width * height;

        if (spriteIndices == null || spriteIndices.Length != total)
        {
            Debug.LogError("spriteIndices が不正です");
            return;
        }

        Vector2 origin = new Vector2(-(width - 1) * cellSize / 2f, -(height - 1) * cellSize / 2f);
        int index = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2 pos = origin + new Vector2(x * cellSize, y * cellSize);

                // マス生成
                GameObject obj = Instantiate(face_image, parent);
             //   GameObject back = Instantiate(background, parent2);

                obj.transform.localPosition = pos;
               // back.transform.localPosition = pos;

                // face コンポーネント取得
                face img = obj.GetComponentInChildren<face>(true);
                if (img == null)
                {
                    Debug.LogError("face が見つかりません。Prefab構造を確認してください", obj);
                    index++;
                    continue;
                }

                int spriteIndex = spriteIndices[index];

                // スプライト設定
                img.tekusutya.sprite = sprites[spriteIndex];
                img.tekusutya.enabled = true;
                img.tekusutya.color = (spriteIndex == 16) ? new Color(0f, 0f, 0f, 0.5f) : Color.white;

                // 目口設定
                CalcEyeKuti(spriteIndex, out img.eye, out img.kuti);

                // 黒マスはMaskSlot無効化
                var slots = obj.GetComponentsInChildren<MaskSlotTrigger>(true);
                foreach (var slot in slots)
                {
                    var col = slot.GetComponent<Collider2D>();
                    if (col != null)
                        col.enabled = (spriteIndex != 16); // 黒マスは Collider 無効

                    slot.gameObject.layer = (spriteIndex == 16) ?
                        LayerMask.NameToLayer("Default") :
                        LayerMask.NameToLayer("MaskSlot");
                }

                index++;
            }
        }
    }

    // -----------------------------
    // 親の子をすべて破棄
    // -----------------------------
    private void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
        }
    }

    // -----------------------------
    // 目口計算
    // -----------------------------
    private void CalcEyeKuti(int spriteIndex, out int eye, out int kuti)
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

    // -----------------------------
    // ステージクリア
    // -----------------------------
    public void stick()
    {
        total--;
        if (total <= 1)
        {
            Invoke(nameof(CLEAR), 1.0f);
        }
    }

    private void CLEAR()
    {
        SceneManager.LoadScene("GameClearScene");
    }
}
