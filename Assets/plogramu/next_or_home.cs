using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;

public class next_or_home : MonoBehaviour
{
    public int stage;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void next_stage()
    {
        deta_retu.stage_now++;
        if (deta_retu.stage_now > 7)
        {
            SceneManager.LoadScene("TitleScene");//次のステージを宣言 
        }
        else  SceneManager.LoadScene("field_kari 1");//次のステージを宣言
        
    }

    public void go_home()
    {
        deta_retu.stage_now = 0;
        SceneManager.LoadScene("TitleScene");//ここでホーム画面を宣言
    }
}
