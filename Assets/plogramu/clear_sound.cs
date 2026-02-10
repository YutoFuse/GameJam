using UnityEngine;

public class clear_sound : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AudioManager.instance.PlaySE(AudioManager.instance.sykiinSE);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
