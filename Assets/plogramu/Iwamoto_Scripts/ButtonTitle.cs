using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonTitle : MonoBehaviour
{
    public void GameOsu()
    {
        SceneManager.LoadScene("field_kari");
    }
    
    public void Osu()
    {
        SceneManager.LoadScene("SelectScene");
    }

    public void GoHomeClickButton()
    {
        SceneManager.LoadScene("TitleScene");
    }

    public void GoNextStageClickButton()
    {
        //SceneManager.LoadScene("Game");
    }
}
