using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class field_create : MonoBehaviour
{
    int fast = 0;
    int erasize = 3;
    public Sprite[] sprites;
    int[] eria;
    int[,] eria_eye = new int[3, 3];
    int[,] eria_kuti = new int[3, 3];
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

                GameObject obj = Instantiate(background, parent);
                obj.transform.localPosition = pos;


                faces[i, j] = obj;
                face img = faces[i, j].GetComponentInChildren<face>();

                if (eriaCount % 2 == 0) { img.tekusutya.sprite = sprites[0]; img.eye = 0; img.kuti = 0; }
                else { img.tekusutya.sprite = sprites[1]; img.eye = 1; img.kuti = 1; }

                eriaCount++;
            }
        }

        Debug.Log("ƒ}ƒX–Ú " + eriaCount);
    }
}

    /*public void number()
    {
            {
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        GameObject obj = faces[0, 0];
                        PullArrowIndicator arrow = faces[i, j].GetComponentInChildren<PullArrowIndicator>();
                    if (arrow == null)
                    {
                        Debug.LogError($"PullArrowIndicator ‚ªŒ©‚Â‚©‚è‚Ü‚¹‚ñ [{i},{j}]", faces[i, j]);
                        continue;
                    }
                    arrow.sprite_number_eye = eria_eye[i, j];
                        arrow.sprite_number_kuti = eria_kuti[i, j];
                        Debug.Log("OK" + eria_eye[i, j] + eria_kuti[i, j]);
                    }
                }
            }
        }
}*/