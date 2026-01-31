using Unity.VisualScripting;
using UnityEngine;

public class deta_retu : MonoBehaviour
{
    int stage_now = 0;
    public int[][] stages =
    {
        new int[] //stage1の変数
        {
            0, 0, 1,
            1, 1, 1,
            0, 0, 1
        },
        new int[] //stage2の変数
        {
            1, 0, 0,
            0, 1, 0,
            0, 0, 1
        },
        new int[] //stage3の変数
        {
            1, 1, 1,
            0, 0, 0,
            1, 1, 1
        },
        new int[] //stage4の変数
        {
            1, 1, 1,
            0, 0, 0,
            1, 1, 1
        }

    };
    private void Start()
    {
        int stage = 1;
        stage_now = stage;
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
