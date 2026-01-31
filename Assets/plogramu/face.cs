using UnityEngine;
using UnityEngine.UI;

public class face : MonoBehaviour
{
    public SpriteRenderer tekusutya;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetSprite(Sprite sprite)
    {
        Debug.Log("OK");
        tekusutya.sprite = sprite;
    }
}
