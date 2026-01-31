using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class field_create : MonoBehaviour
{
    int erasize =3;
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

                eria[i, j] = 1;
                

                faces[i, j] = obj;
                face img = faces[i, j].GetComponentInChildren<face>();
                PullArrowIndicator arrow = faces[i, j].GetComponentInChildren<PullArrowIndicator>();

                if (eriaCount % 2 == 0) { img.tekusutya.sprite = sprites[0]; arrow.sprite_number = 0; }
                else { img.tekusutya.sprite = sprites[1]; arrow.sprite_number = 1; }

                eriaCount++;
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
        face img = faces[i, j].GetComponentInChildren<face>();
        img.tekusutya.sprite = sprites[eriaCount];
    }
}