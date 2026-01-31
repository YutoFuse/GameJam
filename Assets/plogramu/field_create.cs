using Unity.VisualScripting;
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

                eria[i, j] = 1;
                eriaCount++;

                faces[i, j] = obj;
                face img = faces[i, j].GetComponentInChildren<face>();

                if (img == null)
                {
                    Debug.LogError("face ‚ªæ“¾‚Å‚«‚Ä‚¢‚Ü‚¹‚ñ");
                    continue;
                }

                if (img.tekusutya == null)
                {
                    Debug.LogError("tekusutya ‚ª Inspector ‚Åİ’è‚³‚ê‚Ä‚¢‚Ü‚¹‚ñ");
                    continue;
                }
                
                img.tekusutya.sprite= sprites[eriaCount];

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