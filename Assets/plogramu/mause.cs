using UnityEngine;

public class mause : MonoBehaviour
{
    [SerializeField] private ParticleSystem _system;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Vector3 mouse;  // マウスの位置を保存する変数
    private Vector3 target; // オブジェクトのターゲット位置

    void Update()
    {
        // マウスのスクリーン座標を取得
        mouse = Input.mousePosition;

        // スクリーン座標をワールド座標に変換
        target = Camera.main.ScreenToWorldPoint(new Vector3(mouse.x, mouse.y, 10));

        // オブジェクトの位置をターゲットに更新
        this.transform.position = target;

        if (Input.GetMouseButtonDown(0))
        {
            _system.transform.position = target;
            Debug.Log(target);
           // _system.Play();
        }
    }
}
