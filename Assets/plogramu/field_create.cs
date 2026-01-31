using UnityEngine;

public class field_create : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite[] sprites;

    public GameObject background;
    public Transform parent;

    const int width = 3;
    const int height = 3;
    const float cellSize = 1f;

    [HideInInspector]
    public int[] spriteIndices;

    // -------------------------
    // 外部から呼ばれる生成関数
    // -------------------------


    public void CreateField()
    {
        // 既存マス削除
        foreach (Transform child in parent)
        {
            Destroy(child.gameObject);
        }

        int total = width * height;
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

                face img = obj.GetComponentInChildren<face>();

                int spriteIndex = spriteIndices[index];

                img.tekusutya.sprite = sprites[spriteIndex];
                CalcEyeKuti(spriteIndex, out img.eye, out img.kuti);

                index++;
            }
        }
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
}