using UnityEngine;

public class big_and_small : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
     float time, changeSpeed;
     bool enlarge;

    void Start()
    {
        enlarge = true;
    }

    void Update()
    {
        changeSpeed = Time.deltaTime * 0.3f;

        if (time < 0)
        {
            enlarge = true;
        }
        if (time > 0.7f)
        {
            enlarge = false;
        }

        if (enlarge == true)
        {
            time += Time.deltaTime;
            transform.localScale += new Vector3(changeSpeed, changeSpeed, changeSpeed);
        }
        else
        {
            time -= Time.deltaTime;
            transform.localScale -= new Vector3(changeSpeed, changeSpeed, changeSpeed);
        }
    }
}
