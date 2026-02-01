using UnityEngine;

public class deta_retu : MonoBehaviour
{
    public int[][] stages =
    {
        //チュートリアル用で
        new int[] //stage1の変数
        {
            16, 16, 16,
            16, 1, 1,
            16, 16, 16
        },
        new int[] //stage2の変数
        {
            16, 16, 16,
            16, 1, 2,
            16, 16, 16
        },
        new int[] //stage3の変数
        {
           16, 16, 16,
            12, 14, 12,
            16, 16, 16
        },
        new int[] //stage4の変数
        {
           16, 16, 16,
            16, 16, 16,
            16, 16, 16
        }

    };
    private void Start()
    {
        int stage = 2;
        deta_shuuto(stage-1);//stage
    }

    public void deta_shuuto(int stageIndex)
    {
        field_create create =
            GameObject.Find("field_Maneger")
            .GetComponent<field_create>();

        // stageIndex チェック
        if (stageIndex < 0 || stageIndex >= stages.Length)
        {
            Debug.LogError("存在しないステージです");
            return;
        }

        // 配列を渡す
        create.spriteIndices = stages[stageIndex];

        // 盤面を再生成
        create.CreateField();
    }
}
