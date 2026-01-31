using UnityEngine;
using UnityEngine.UI;

public class field_create : MonoBehaviour
{
    public Sprite[] sprites;
    int[,] eria = new int[3, 3];
    GameObject[,] faces = new GameObject[3, 3];

    int eriaCount = 0;
    const int eriaWidth = 3;
    const int eriaHeight = 3;
    float cellsize = 1f;

    public GameObject background;
    public Transform parent;

    void Start()
    {
        Vector2 defaultpos = Vector2.zero;
        defaultpos.x = -(eriaWidth - 1) * cellsize / 2f;
        defaultpos.y = -(eriaHeight - 1) * cellsize / 2f;

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                Vector2 pos = defaultpos;
                pos.x += i * cellsize;
                pos.y += j * cellsize;

                GameObject obj = Instantiate(background);
                obj.transform.localPosition = pos;

                Debug.Log("OK");
                face view = obj.GetComponentInChildren<face>();

                //view.image.sprite = sprites[2];

                eria[i, j] = 1;
                eriaCount++;
                faces[i, j] = obj;
            }
        }

        Debug.Log("ƒ}ƒX–Ú " + eriaCount);
    }

    void Update()
    {
        int i = 1, j = 1;
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            faces[i, j].SetActive(false);
        }
    }
}