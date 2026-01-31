using UnityEngine;

/// <summary>
/// マス（盤面）を生成し、各マスに顔スプライトを割り当てるクラス
/// ・Inspectorで設定するもの
///   - sprites
///   - background
///   - parent
/// ・マス数や配置、spriteIndex はコード内で管理
/// ・配置順：左下 → 右 → 上
/// </summary>
public class field_create : MonoBehaviour
{
    // ================================
    // Inspectorから設定するもの
    // ================================
    [Header("Sprites (0～16)")]
    public Sprite[] sprites;          // 顔スプライト（17種類）

    [Header("Prefab")]
    public GameObject background;     // マスPrefab

    [Header("Parent")]
    public Transform parent;          // 親オブジェクト

    // ================================
    // グリッド設定（コード内）
    // ================================
    const int width = 3;
    const int height = 3;
    const float cellSize = 1f;

    // ================================
    // 各マスに表示する spriteIndex
    // 配置順：左下 → 右 → 上
    // ================================
    int[] spriteIndices =
    {
        0, 0, 1,
        1, 1, 1,
        0, 0, 0
    };

    // ================================
    // 初期化処理
    // ================================
    void Start()
    {
        // ----------------
        // 事前チェック
        // ----------------
        if (sprites == null || sprites.Length < 17)
        {
            Debug.LogError("sprites に 17 枚以上の Sprite を設定してください");
            return;
        }

        if (background == null)
        {
            Debug.LogError("background が設定されていません");
            return;
        }

        if (parent == null)
        {
            Debug.LogError("parent が設定されていません");
            return;
        }

        // ----------------
        // マス数チェック
        // ----------------
        int total = width * height;

        if (spriteIndices.Length != total)
        {
            Debug.LogError(
                $"spriteIndices({spriteIndices.Length}) と マス数({total})が一致しません"
            );
            return;
        }

        // ----------------
        // 左下基準の原点
        // ----------------
        Vector2 origin;
        origin.x = -(width - 1) * cellSize / 2f;
        origin.y = -(height - 1) * cellSize / 2f;

        int index = 0;

        // ================================
        // マス生成
        // ================================
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // マス位置計算
                Vector2 pos = origin;
                pos.x += x * cellSize;
                pos.y += y * cellSize;

                // マス生成
                GameObject obj = Instantiate(background, parent);
                obj.transform.localPosition = pos;

                // face コンポーネント取得
                face img = obj.GetComponentInChildren<face>();

                // spriteIndex 取得
                int spriteIndex = spriteIndices[index];

                // 範囲チェック
                if (spriteIndex < 0 || spriteIndex >= sprites.Length)
                {
                    Debug.LogError($"無効な spriteIndex: {spriteIndex}");
                    index++;
                    continue;
                }

                // スプライト設定
                img.tekusutya.sprite = sprites[spriteIndex];

                // eye / kuti 自動計算
                CalcEyeKuti(spriteIndex, out img.eye, out img.kuti);

                index++;
            }
        }

        Debug.Log($"生成完了：{total}マス");
    }

    // ================================
    // spriteIndex → eye / kuti 変換
    // ================================
    void CalcEyeKuti(int spriteIndex, out int eye, out int kuti)
    {
        const int kutiCount = 4;
        eye = spriteIndex / kutiCount;
        kuti = spriteIndex % kutiCount;
    }
}


/*using UnityEngine;

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
    public int width = 3;        // 横マス数
    public int height = 3;       // 縦マス数
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
}*/