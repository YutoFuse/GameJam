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
            16, 16, 16,
            0, 0, 16,
            16, 16, 16
        },
        new int[] //stage2
        {
            16, 16, 16,
            16, 1, 3,
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
            3, 3, 3,
            3, 1, 3,
            3, 3, 3
        },

        new int[] //stage5
        {
            16, 5, 14,
            16, 16, 13,
            16, 16, 16

        }
        ,
        new int[] //stage6
        {
             4, 5, 16,
            16, 5, 4,
            16, 5, 16

        }
        ,
        new int[] //stage7
        {


           16, 8, 16,
            1, 5, 1,
            16,5, 16
        }
        ,
        new int[] //stage8
        {
           4, 16, 4,
            12, 16, 14,
            4, 5, 4
        },
        new int[] //stage9
        {
            3, 0, 1,
            1, 16, 3,
            5, 1, 3
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

        // stageIndex ?øΩE?øΩ`?øΩE?øΩF?øΩE?øΩb?øΩE?øΩN
        if (stageIndex < 0 || stageIndex >= stages.Length)
        {
            Debug.LogError("?øΩE?øΩ?øΩE?øΩ?øΩE?øΩ›ÇÔøΩ?øΩE?øΩ»ÇÔøΩ?øΩE?øΩX?øΩE?øΩe?øΩE?øΩ[?øΩE?øΩW?øΩE?øΩ≈ÇÔøΩ");
            return;
        }

        // ?øΩE?øΩz?øΩE?øΩ?øΩE?øΩ?øΩE?øΩn?øΩE?øΩ?øΩE?øΩ
        create.spriteIndices = stages[stageIndex];

        // ?øΩE?øΩ’ñ ÇÔøΩ?øΩE?øΩƒêÔøΩ?øΩE?øΩ?øΩE?øΩ
        create.CreateField();
    }
}
