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
            0, 0, 0,
            0, 16, 0,
            0, 16, 0
        },
        new int[] //stage2
        {
            1, 0, 0,
            0, 1, 0,
            0, 0, 1
        },
        new int[] //stage3
        {
            1, 1, 1,
            0, 0, 0,
            1, 1, 1
        },
        new int[] //stage4
        {
            1, 1, 1,
            0, 0, 0,
            1, 1, 1
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

        // stageIndex �`�F�b�N
        if (stageIndex < 0 || stageIndex >= stages.Length)
        {
            Debug.LogError("���݂��Ȃ��X�e�[�W�ł�");
            return;
        }

        // �z���n��
        create.spriteIndices = stages[stageIndex];
        create.stage = stageIndex;

        // �Ֆʂ��Đ���
        create.CreateField();
    }
}
