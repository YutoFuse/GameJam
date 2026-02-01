using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonTitle : MonoBehaviour
{
    public void GameOsu()
    {
        SceneManager.LoadScene("min_stage");
        AudioManager.instance.PlaySE(AudioManager.instance.ActionSE);

    }
    
    public void slect()
    {
        SceneManager.LoadScene("SelectScene");
    }

    public void tyutoriaru()
    {
        SceneManager.LoadScene("TitleScene");
    }

    public void GoNextStageClickButton()
    {
        //SceneManager.LoadScene("Game");
    }
}
