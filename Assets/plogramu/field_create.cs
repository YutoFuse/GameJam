using UnityEngine;

/// <summary>
/// マス（盤面）を生成し、各マスに顔スプライトを割り当てるクラス
/// ・マス数は width × height
/// ・配置順は 左下 → 右 → 上
/// ・表示する顔は spriteIndices で自由に指定
/// ・eye / kuti は spriteIndex から自動計算
/// </summary>
public class field_create : MonoBehaviour
{
    // ================================
    // 顔スプライト一覧
    // sprites[0] ～ sprites[16]（17種類）
    // ================================
    public Sprite[] sprites;

    // ================================
    // グリッド設定（将来変更可能）
    // ================================
    [Header("Grid Size")]
    public int width = 4;        // 横マス数
    public int height = 4;       // 縦マス数
    public float cellSize = 1f;  // マス間隔

    // ================================
    // Prefab設定
    // ================================
    [Header("Prefabs")]
    public GameObject background; // マスのPrefab
    public Transform parent;      // 親オブジェクト

    // ================================
    // 各マスに表示する spriteIndex
    // 配置順：左下 → 右 → 上
    // ================================
    [Header("Sprite Index Per Cell")]
    public int[] spriteIndices =
    {
        0, 0, 1, 1,
        1, 4, 0, 10,
        3, 2, 9, 7,
        12, 12, 9, 5
    };

    // ================================
    // 初期化処理
    // ================================
    void Start()
    {
        // マスの総数
        int total = width * height;
        Debug.Log(total);

        // spriteIndices の数チェック
        Debug.Log(spriteIndices.Length);
        if (spriteIndices.Length != total)
        {
            Debug.LogError(
                $"spriteIndices の数({spriteIndices.Length})が " +
                $"マス数({total})と一致していません"
            );
            return;
        }

        // 左下基準の原点位置を計算
        Vector2 origin;
        origin.x = -(width - 1) * cellSize / 2f;
        origin.y = -(height - 1) * cellSize / 2f;

        // spriteIndices 用のインデックス
        int index = 0;

        // ================================
        // マス生成ループ
        // ================================

        // y方向（下 → 上）
        for (int y = 0; y < height; y++)
        {
            // x方向（左 → 右）
            for (int x = 0; x < width; x++)
            {
                // マス位置を計算
                Vector2 pos = origin;
                pos.x += x * cellSize;
                pos.y += y * cellSize;

                // マスPrefabを生成
                GameObject obj = Instantiate(background, parent);
                obj.transform.localPosition = pos;

                // face コンポーネント取得
                face img = obj.GetComponentInChildren<face>();

                // このマスに対応する spriteIndex
                int spriteIndex = spriteIndices[index];

                // spriteIndex の範囲チェック
                if (spriteIndex < 0 || spriteIndex >= sprites.Length)
                {
                    Debug.LogError($"無効な spriteIndex: {spriteIndex}");
                    continue;
                }

                // スプライト設定
                img.tekusutya.sprite = sprites[spriteIndex];

                // spriteIndex から eye / kuti を自動計算
                CalcEyeKuti(spriteIndex, out img.eye, out img.kuti);

                // 次のマスへ
                index++;
            }
        }

        Debug.Log($"マス生成完了：{total}マス index " + index);
    }

    // ================================
    // spriteIndex → eye / kuti 変換
    // ================================
    /// <summary>
    /// spriteIndex から eye と kuti を自動計算する
    ///
    /// 対応表：
    /// eye=0,kuti=0 → sprite[0]
    /// eye=0,kuti=1 → sprite[1]
    /// eye=0,kuti=2 → sprite[2]
    /// eye=0,kuti=3 → sprite[3]
    /// eye=1,kuti=0 → sprite[4]
    /// ...
    /// eye=3,kuti=3 → sprite[15]
    /// </summary>
    void CalcEyeKuti(int spriteIndex, out int eye, out int kuti)
    {
        // 1行あたりの口の種類数
        int kutiCount = 4;

        // eye は行番号
        eye = spriteIndex / kutiCount;

        // kuti は列番号
        kuti = spriteIndex % kutiCount;
    }
}