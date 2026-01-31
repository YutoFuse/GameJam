using UnityEngine;

public class field_create : MonoBehaviour
{
    int[,] eria = new int[3,3];
    GameObject[,] face=new GameObject[3,3];
    int eriaCount=0;
    const int eriaWidth = 3;
    const int eriaHeight = 3;
    float cellsize=100f;
    public GameObject background;
    public Transform parent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Vector2 defaultpos = new Vector2(0.0f, 0.0f);
        defaultpos.x=-(eriaWidth-1)*cellsize/2f;
        defaultpos.y=-(eriaHeight - 1) * cellsize / 2f;

        for (int i = 0; i < 3; i++)
        {
            for(int j = 0; j < 3; j++)
            {

                Vector2 pos = defaultpos;
                pos.x += i*cellsize;
                pos.y += j*cellsize;
                GameObject obj;
                obj = Instantiate(background,parent);
                obj.transform.localPosition = pos;
                eria[i,j] = 1;
                eriaCount++;
                face[i, j] = obj;
            }
        }
        Debug.Log("ƒ}ƒX–Ú"+eriaCount);
    }

    // Update is called once per frame
    void Update()
    {
        int i = 1,j = 1;
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Debug.Log("OK");
            face[i,j].SetActive(false);
        }
    }
}
