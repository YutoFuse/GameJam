using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class field_create : MonoBehaviour
{
    public static field_create Instance { get; private set; }

    public int stage;

    [Header("Sprites")]
    public Sprite[] sprites;

    [Header("Prefabs")]
    public GameObject background; // Square prefab
    public Transform parent;

    const int width = 3;
    const int height = 3;
    const float cellSize = 1f;

    [Header("Blank")]
    [SerializeField] private int blankSpriteIndex = 16; // 黒(空白)のindex

    int total;
    [HideInInspector] public int[] spriteIndices;

    // 盤面参照
    private GameObject[,] cellGO = new GameObject[width, height];
    private face[,] faceGrid = new face[width, height];

    private void Awake()
    {
        Instance = this;
    }

    // -------------------------
    // 生成
    // -------------------------
    public void CreateField()
    {
        // 既存マス削除
        foreach (Transform child in parent)
            Destroy(child.gameObject);

        total = width * height;

        MaskStockUI mask;
        GameObject stock = GameObject.Find("MaskImage");
        mask = stock.GetComponent<MaskStockUI>();
        mask.reset_mask();

        if (spriteIndices == null || spriteIndices.Length != total)
        {
            Debug.LogError("spriteIndices が不正です");
            return;
        }

        // 念のため参照配列を初期化
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            cellGO[x, y] = null;
            faceGrid[x, y] = null;
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

                // 座標（デバッグや後処理用）
                var coord = obj.GetComponent<GridCoord>();
                if (coord == null) coord = obj.AddComponent<GridCoord>();
                coord.x = x; coord.y = y;

                // face
                face f = obj.GetComponentInChildren<face>(true);
                if (f == null || f.tekusutya == null)
                {
                    Debug.LogError($"face/tekusutya が見つかりません x={x} y={y}", obj);
                    index++;
                    continue;
                }

                cellGO[x, y] = obj;
                faceGrid[x, y] = f;

                int spriteIndex = spriteIndices[index];
                if (spriteIndices[index] == 16) { total--; }
                SetCellBySpriteIndex(x, y, spriteIndex);

                index++;
            }
        }

        // 生成直後に空白埋めの統一（sprite=null事故防止）
        NormalizeAllBlanks();
    }

    // -------------------------
    // 空白・参照
    // -------------------------
    public bool InBounds(int x, int y) => (0 <= x && x < width && 0 <= y && y < height);

    public face GetFace(int x, int y) => InBounds(x, y) ? faceGrid[x, y] : null;

    public bool IsBlank(int x, int y)
    {
        var f = GetFace(x, y);
        if (f == null || f.tekusutya == null) return true;
        return (f.eye == 999 && f.kuti == 999);
    }

    public void SetBlank(int x, int y)
    {
        SetCellBySpriteIndex(x, y, blankSpriteIndex);
    }

    // 「顔がない状態」を必ず黒で埋める（復活・null防止）
    public void NormalizeAllBlanks()
    {
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            var f = GetFace(x, y);
            if (f == null || f.tekusutya == null) continue;

            // ロジックが空白なら黒で統一
            if (f.eye == 999 && f.kuti == 999)
            {
                f.tekusutya.sprite = sprites[blankSpriteIndex];
                f.tekusutya.enabled = true;
            }
            // 見た目がnull/無効（事故）→ 空白に落とす
            else if (f.tekusutya.sprite == null || f.tekusutya.enabled == false)
            {
                SetBlank(x, y);
            }
        }
    }

    // spriteIndex から目口を設定して必ず表示
    public void SetCellBySpriteIndex(int x, int y, int spriteIndex)
    {
        var f = GetFace(x, y);
        if (f == null || f.tekusutya == null) return;

        spriteIndex = Mathf.Clamp(spriteIndex, 0, sprites.Length - 1);

        f.tekusutya.sprite = sprites[spriteIndex];
        f.tekusutya.enabled = true;

        CalcEyeKuti(spriteIndex, out f.eye, out f.kuti);

        // 空白ならマスク状態もリセット（空白にマスク残るバグ防止）
        if (f.eye == 999 && f.kuti == 999)
        {
            f.maskEye = false;
            f.maskMouth = false;

            // face に ClearMasks がある前提なら外す（無ければ消してOK）
            try { f.ClearMasks(true); } catch { }
        }
    }

    // -------------------------
    // 連結塊（to起点）を取る：空白は含めない
    // -------------------------
    public List<Vector2Int> GetConnectedComponentFrom(int startX, int startY)
    {
        var result = new List<Vector2Int>();
        if (!InBounds(startX, startY)) return result;
        if (IsBlank(startX, startY)) return result;

        var visited = new bool[width, height];
        var q = new Queue<Vector2Int>();

        q.Enqueue(new Vector2Int(startX, startY));
        visited[startX, startY] = true;

        while (q.Count > 0)
        {
            var p = q.Dequeue();
            result.Add(p);

            TryPush(p.x + 1, p.y);
            TryPush(p.x - 1, p.y);
            TryPush(p.x, p.y + 1);
            TryPush(p.x, p.y - 1);
        }

        return result;

        void TryPush(int nx, int ny)
        {
            if (!InBounds(nx, ny)) return;
            if (visited[nx, ny]) return;
            if (IsBlank(nx, ny)) return; // 空白は塊に含めない

            visited[nx, ny] = true;
            q.Enqueue(new Vector2Int(nx, ny));
        }
    }

    // -------------------------
    // 合体 + 押し出し（仕様ど真ん中）
    // -------------------------
    public void TryMergeAndShift(Vector2Int from, Vector2Int to, Vector2Int dir)
    {
        if (dir == Vector2Int.zero) return;
        if (!InBounds(from.x, from.y) || !InBounds(to.x, to.y)) return;

        var fromFace = GetFace(from.x, from.y);
        var toFace   = GetFace(to.x, to.y);
        if (fromFace == null || toFace == null) return;

        // 空白相手は禁止（空白へ矢印で消える事故防止）
        if (IsBlank(from.x, from.y)) return;
        if (IsBlank(to.x, to.y)) return;

        // ★「元々 owner(from) に繋がっていた集合」を合体前に取る（押し出し用）
        var originalComp = GetConnectedComponentFrom(from.x, from.y);

        // -------------------------
        // (1) 合体可否（マスク考慮）
        // -------------------------
        bool eyeMasked   = fromFace.maskEye   || toFace.maskEye;
        bool mouthMasked = fromFace.maskMouth || toFace.maskMouth;

        bool eyeOK   = eyeMasked   || (fromFace.eye  == toFace.eye);
        bool mouthOK = mouthMasked || (fromFace.kuti == toFace.kuti);

        if (!eyeOK || !mouthOK) return;

        // -------------------------
        // (2) 成功：あなたのルール
        //     「掴んだ方(from)が消える」「離した場所(to)は残る」
        // -------------------------

        // マスクは成功時に消費するなら外す（必要に応じて）
        if (toFace.maskEye || toFace.maskMouth)   { try { toFace.ClearMasks(true); } catch { } }
        if (fromFace.maskEye || fromFace.maskMouth){ try { fromFace.ClearMasks(true); } catch { } }

        // ★toFace は絶対に変更しない！（これがルールの核）
        // 掴んだ方だけ空白化
        SetBlank(from.x, from.y);

        // -------------------------
        // (3) 孤立防止のシフト（アンカー=to）
        //     ※あなたが既に入れている「孤立だけ動かす」方式を使う
        // -------------------------
        if (!IsAllNonBlankConnected())
        {
            ShiftComponentOneStep_Anchored(originalComp, dir, anchor: to);
        }
        
        NormalizeAllBlanks();

        // もし「消したら残数減らす」ならここ
        
    }



    /// <summary>
    /// 連結成分のうち anchor を固定して、dir 方向に「空白がある場合のみ」1マス詰める
    /// ・移動先が「今空白」か「このステップで空いた(vacated)」だけOK
    /// ・compSet.Contains(to) を空き扱いにしない（バグの原因）
    /// </summary>
    private void ShiftComponentOneStep_Anchored(List<Vector2Int> component, Vector2Int dir, Vector2Int anchor)
    {
        if (component == null || component.Count == 0) return;

        var comp = new List<Vector2Int>(component);
        comp.Remove(anchor); // アンカー固定
        comp.RemoveAll(p => IsBlank(p.x, p.y)); // 念のため

        if (comp.Count == 0) return;

        // 前から順に処理（vacated を使って連鎖的に詰める）
        comp.Sort((a, b) =>
        {
            if (dir == Vector2Int.up)    return b.y.CompareTo(a.y); // 上へ：上から
            if (dir == Vector2Int.down)  return a.y.CompareTo(b.y); // 下へ：下から
            if (dir == Vector2Int.right) return b.x.CompareTo(a.x); // 右へ：右から
            return a.x.CompareTo(b.x);                             // 左へ：左から
        });

        var vacated = new HashSet<Vector2Int>();
        var moves = new List<(Vector2Int from, Vector2Int to)>();

        foreach (var p in comp)
        {
            var t = p + dir;
            if (!InBounds(t.x, t.y)) continue;
            if (t == anchor) continue;

            bool destBlankNow = IsBlank(t.x, t.y);
            bool destVacated  = vacated.Contains(t);

            // ★「今空白」または「このステップで空いた」だけ許可
            if (destBlankNow || destVacated)
            {
                moves.Add((p, t));
                vacated.Add(p); // p は空く
            }
        }

        if (moves.Count == 0) return;

        // スナップショット
        var snap = new List<CellData>(moves.Count);
        foreach (var m in moves)
        {
            var f = GetFace(m.from.x, m.from.y);
            snap.Add(new CellData
            {
                sprite = f.tekusutya.sprite,
                eye = f.eye,
                mouth = f.kuti,
                maskEye = f.maskEye,
                maskMouth = f.maskMouth
            });
        }

        // from を空白化（複製防止）
        foreach (var m in moves)
            SetBlank(m.from.x, m.from.y);

        // to へ書き込み
        for (int i = 0; i < moves.Count; i++)
        {
            var dst = GetFace(moves[i].to.x, moves[i].to.y);
            dst.tekusutya.sprite = snap[i].sprite;
            dst.tekusutya.enabled = true;
            dst.eye = snap[i].eye;
            dst.kuti = snap[i].mouth;
            dst.maskEye = snap[i].maskEye;
            dst.maskMouth = snap[i].maskMouth;
        }
    }

    private struct CellData
    {
        public Sprite sprite;
        public int eye;
        public int mouth;
        public bool maskEye;
        public bool maskMouth;
    }

    // -------------------------
    // クリア判定（そのまま）
    // -------------------------
    // -----------------------------
    // ステージクリア
    // -----------------------------
    public void stick()
    {
        AudioManager.instance.PlaySE(AudioManager.instance.ActionSE);
        

        total--;
        Debug.Log("total" + total);
        if (total <= 1)
        {
            Invoke(nameof(CLEAR), 1.0f);
        }
    }

    private void CLEAR()
    {
        SceneManager.LoadScene("GameClearScene");
    }

    // -------------------------
    // spriteIndex → eye/kuti
    // -------------------------
    void CalcEyeKuti(int spriteIndex, out int eye, out int kuti)
    {
        if (spriteIndex == blankSpriteIndex)
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

    private void ShiftOnlyIsolatedTowardAnchor(List<Vector2Int> originalComp, Vector2Int dir, Vector2Int anchor)
    {
        stick();

        if (originalComp == null || originalComp.Count == 0) return;

        // 「元々owner(from)に繋がってた集合」
        var compSet = new HashSet<Vector2Int>(originalComp);

        // ループで孤立が解消するまで最大9回
        for (int iter = 0; iter < width * height; iter++)
        {
            // 現在アンカーに繋がってる集合
            var connectedNow = new HashSet<Vector2Int>(GetConnectedComponentFrom(anchor.x, anchor.y));

            // compSet のうち「今アンカーに繋がってない」ものだけ動かす対象
            var candidates = new List<Vector2Int>();
            foreach (var p in compSet)
            {
                if (!InBounds(p.x, p.y)) continue;
                if (p == anchor) continue;
                if (IsBlank(p.x, p.y)) continue;

                if (!connectedNow.Contains(p))
                    candidates.Add(p);
            }

            if (candidates.Count == 0) break; // もう孤立は無い

            // candidates を 1マスだけ動かす（空白がある時のみ / 連鎖はvacatedでOK）
            var movedPairs = ShiftSubsetOneStep(candidates, dir, anchor);

            if (movedPairs.Count == 0) break; // 動けない＝これ以上直せない

            // compSet を更新（位置が変わった分）
            foreach (var m in movedPairs)
            {
                compSet.Remove(m.from);
                compSet.Add(m.to);
            }
        }
    }

    private List<(Vector2Int from, Vector2Int to)> ShiftSubsetOneStep(List<Vector2Int> subset, Vector2Int dir, Vector2Int anchor)
    {
        var moved = new List<(Vector2Int from, Vector2Int to)>();
        if (subset == null || subset.Count == 0) return moved;

        // 方向に応じて「詰める順番」を整える
        subset.Sort((a, b) =>
        {
            if (dir == Vector2Int.up)    return b.y.CompareTo(a.y); // 上へ：上から
            if (dir == Vector2Int.down)  return a.y.CompareTo(b.y); // 下へ：下から
            if (dir == Vector2Int.right) return b.x.CompareTo(a.x); // 右へ：右から
            return a.x.CompareTo(b.x);                             // 左へ：左から
        });

        var vacated = new HashSet<Vector2Int>();

        // move候補を作る（今空白 or このステップで空く）
        foreach (var p in subset)
        {
            var t = p + dir;
            if (!InBounds(t.x, t.y)) continue;
            if (t == anchor) continue;

            bool destBlankNow = IsBlank(t.x, t.y);
            bool destVacated  = vacated.Contains(t);

            if (destBlankNow || destVacated)
            {
                moved.Add((p, t));
                vacated.Add(p);
            }
        }

        if (moved.Count == 0) return moved;

        // スナップショット
        var snap = new List<CellData>(moved.Count);
        foreach (var m in moved)
        {
            var f = GetFace(m.from.x, m.from.y);
            snap.Add(new CellData
            {
                sprite = f.tekusutya.sprite,
                eye = f.eye,
                mouth = f.kuti,
                maskEye = f.maskEye,
                maskMouth = f.maskMouth
            });
        }

        // from を先に空白へ（複製防止）
        foreach (var m in moved)
            SetBlank(m.from.x, m.from.y);

        // to へ書き込み
        for (int i = 0; i < moved.Count; i++)
        {
            var dst = GetFace(moved[i].to.x, moved[i].to.y);
            dst.tekusutya.sprite = snap[i].sprite;
            dst.tekusutya.enabled = true;
            dst.eye = snap[i].eye;
            dst.kuti = snap[i].mouth;
            dst.maskEye = snap[i].maskEye;
            dst.maskMouth = snap[i].maskMouth;
        }

        return moved;
    }
    // 盤面上の「空白じゃないマス」を全部集める
    private List<Vector2Int> GetAllNonBlankCells()
    {
        var list = new List<Vector2Int>();
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            if (!IsBlank(x, y)) list.Add(new Vector2Int(x, y));
        }
        return list;
    }

    // 空白以外が「全部ひとつの連結成分」になっているか？
    private bool IsAllNonBlankConnected()
    {
        var all = GetAllNonBlankCells();
        if (all.Count <= 1) return true;

        // どれか1つから BFS
        var start = all[0];
        var visited = new HashSet<Vector2Int>();
        var q = new Queue<Vector2Int>();
        q.Enqueue(start);
        visited.Add(start);

        while (q.Count > 0)
        {
            var p = q.Dequeue();
            Try(p + Vector2Int.up);
            Try(p + Vector2Int.down);
            Try(p + Vector2Int.left);
            Try(p + Vector2Int.right);
        }

        return visited.Count == all.Count;

        void Try(Vector2Int n)
        {
            if (!InBounds(n.x, n.y)) return;
            if (IsBlank(n.x, n.y)) return;
            if (visited.Add(n)) q.Enqueue(n);
        }
    }

}
