using UnityEngine;

public class field_create : MonoBehaviour
{
    int[][] eria = new int[3][];
    int eriaCount;
    const int eriaWidth = 0;
    const int eriaHeight = 0;
    public GameObject background;
    public Transform parent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Vector2 defaultpos = new Vector2(0.0f, 0.0f);
        defaultpos.x=-(eriaWidth/2);
        defaultpos.y=-(eriaHeight/2);

        for (int i = 0; i < 3; i++)
        {
            for(int j = 0; j < 3; j++)
            {

                Vector2 pos = new Vector2(0.0f, 0.0f);
                pos.x += i;
                pos.y += j;
                GameObject obj;
                obj = Instantiate(background, parent);
                obj.transform.position = pos;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
