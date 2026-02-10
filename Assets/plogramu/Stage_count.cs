using UnityEngine;

public class Stage_count : MonoBehaviour
{
    int count=0;
    [SerializeField] private UnityEngine.UI.Image targetImage;
    public Sprite[] stage_count;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       count= deta_retu.stage_now;
        targetImage.sprite = stage_count[count+1];
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
