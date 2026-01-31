using UnityEngine;
using UnityEngine.UI;

public class ScroolTitle : MonoBehaviour
{
    [SerializeField] private RawImage _image;
    [SerializeField] private float _dx, _dy;

    void Update()
    {
        // 現在のUV Rectを取得し、新しい座標を計算して適用する
        _image.uvRect = new Rect(_image.uvRect.position + new Vector2(_dx, _dy) * Time.deltaTime, _image.uvRect.size);
    }
}