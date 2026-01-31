using Unity.VisualScripting;
using UnityEngine;

public class deta_retu : MonoBehaviour
{
    int stage_now = 0;
    public int[][] stages =
    {
        new int[] //stage1�̕ϐ�
        {
            0, 0, 1,
            1, 1, 1,
            0, 0, 1
        },
        new int[] //stage2�̕ϐ�
        {
            1, 0, 0,
            0, 1, 0,
            0, 0, 1
        },
        new int[] //stage3�̕ϐ�
        {
            1, 1, 1,
            0, 0, 0,
            1, 1, 1
        },
        new int[] //stage4�̕ϐ�
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

        // stageIndex �`�F�b�N
        if (stageIndex < 0 || stageIndex >= stages.Length)
        {
            Debug.LogError("���݂��Ȃ��X�e�[�W�ł�");
            return;
        }

        // �z���n��
        create.spriteIndices = stages[stageIndex];

        // �Ֆʂ��Đ���
        create.CreateField();
    }
}
