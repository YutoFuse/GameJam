using UnityEngine;
using UnityEngine.UI; // UIを操作するために必要
using System.Collections.Generic;
using UnityEngine.SceneManagement; // リストを使うために必要

public class Scwne : MonoBehaviour
{
    public Image displayImage;       // 表示用のImageコンポーネント
    public List<Sprite> sprites;    // チュートリアル画像のリスト
    private int currentIndex = 0;
    private int maxmaisuu=0;// 現在何枚目か

    void Update()
    {
        // 左クリック（または画面タップ）を検知
        if (Input.GetMouseButtonUp(0))
        {
            maxmaisuu++;
            if(maxmaisuu==3)
            {
                SceneManager.LoadScene("TitleScene");
            }
            AdvanceTutorial();
        }
    }

    void AdvanceTutorial()
    {
        currentIndex++;

        // 次の画像があるかチェック
        if (currentIndex < sprites.Count)
        {
            // 次の画像を表示
            displayImage.sprite = sprites[currentIndex];
        }
        else
        {
            // 全て表示し終わったらImageオブジェクトを非表示にする
            displayImage.gameObject.SetActive(false);

            // スクリプト自体も不要なら無効化する
            this.enabled = false;
        }
    }
}