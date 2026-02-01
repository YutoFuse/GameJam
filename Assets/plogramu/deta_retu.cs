using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;

public class deta_retu : MonoBehaviour
{
    public static int stage_now = 0;
    public int[][] stages =
    {
        new int[] //stage1
        {
            1, 0, 0,
            0, 16, 0,
            0, 16, 0
        },
        new int[] //stage2
        {
            16, 16, 16,
            16, 1, 2,
            16, 16, 16
        },
        new int[] //stage3
        {
           16, 16, 16,
            12, 14, 12,
            16, 16, 16
        },
        new int[] //stage4
        {
           16, 16, 16,
            16, 4, 16,
            16, 16, 16
        },
        new int[] //stage5
        {
           16, 16, 16,
            16, 5, 16,
            16, 16, 16
        },
        new int[] //stage6
        {
           16, 16, 16,
            16, 6, 16,
            16, 16, 16
        },
        new int[] //stage7
        {
           16, 16, 16,
            16, 7, 16,
            16, 16, 16
        },
        new int[] //stage8
        {
           16, 16, 16,
            16, 8, 16,
            16, 16, 16
        }
    };
    private void Start()
    {
        deta_shuuto(stage_now);//stage
    }

    public void Reset()
    {
        deta_shuuto(stage_now);
    }


    public void deta_shuuto(int stageIndex)
    {
        field_create create =
            GameObject.Find("field_Maneger")
            .GetComponent<field_create>();

        // stageIndex ・ｽ`・ｽF・ｽb・ｽN
        if (stageIndex < 0 || stageIndex >= stages.Length)
        {
            Debug.LogError("・ｽ・ｽ・ｽﾝゑｿｽ・ｽﾈゑｿｽ・ｽX・ｽe・ｽ[・ｽW・ｽﾅゑｿｽ");
            return;
        }

        // ・ｽz・ｽ・ｽ・ｽn・ｽ・ｽ
        create.spriteIndices = stages[stageIndex];

        // ・ｽﾕ面ゑｿｽ・ｽﾄ撰ｿｽ・ｽ・ｽ
        create.CreateField();
    }
}
