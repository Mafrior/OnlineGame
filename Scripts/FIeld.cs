using UnityEngine;

public class FIeld : MonoBehaviour
{
    [HideInInspector]
    public static int Count => field.Length;
    private static GameObject[][] field;
    public GameObject platformPrefab;
    public static FIeld Inst;

    public PlatformController this[float y, float x]
    {
        get
        {
            return field[(int)y][(int)x].GetComponent<PlatformController>();
        }
        set
        {
            field[(int)y][(int)x] = value.gameObject;
        }
    }

    private void Awake()
    {
        if (Inst != null)
        {
            Debug.LogError("Instance может быть только один");
            return;
        }
        Inst = this;
    }

    public void InitializeField(int _count, Packet _packet)
    {
        field = new GameObject[_count][];
        for (int i = 0; i < field.Length; i++)
        {
            field[i] = new GameObject[_count];
            for (int j = 0; j < field[i].Length; j++)
            { 
                GameObject asObject = Instantiate(platformPrefab, 
                    new Vector3(-1 * 2f * (_count / 2 - 0.5f) + (j * platformPrefab.transform.localScale.x * 2.65f),
                    -1 * 2f * (_count / 2 - 0.5f) + (i * platformPrefab.transform.localScale.x * 2.65f), 0), Quaternion.identity);

                PlatformController component = asObject.GetComponent<PlatformController>();

                component.isWater = _packet.ReadBool();
                component.rotation = _packet.ReadString();
                component.posOnF = new Vector2(j, i);

                field[i][j] = asObject;
                field[i][j].transform.SetParent(gameObject.transform);
            }
        }
    }

    /// <summary> Ипользуя выбранную платформу и платформу на которую нажали, меняет их местами как в игре, так и в FIeld </summary>
    public static void SwapPlatform(PlatformController soOne, PlatformController soTwo)
    {
        GameObject save = Instantiate(soTwo.gameObject, Inst.gameObject.transform);
        save.transform.position = soOne.transform.position;
        soOne.transform.position = soTwo.transform.position;
        field[(int)soTwo.posOnF.y][(int)soTwo.posOnF.x] = soOne.gameObject;
        field[(int)soOne.posOnF.y][(int)soOne.posOnF.x] = save;
        save.GetComponent<PlatformController>().posOnF = soOne.posOnF;
        soOne.posOnF = soTwo.posOnF;
        save.GetComponent<PlatformController>().SpriteActivation();
        Destroy(soTwo.gameObject);
    }

    public static void DestroyWall(Vector2 pos)
    {
        Inst[pos.y, pos.x].wall.gameObject.GetComponent<SpriteRenderer>().sprite = null;
        Inst[pos.y, pos.x].wall.wallDestroied = true;
        Inst[pos.y, pos.x].rotation = "";
    }

    public static void MovePlatform(Vector2 posOnF)
    {
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (j == 0)
                {
                    if (i == 0) continue;
                    if((posOnF.y != 1 && i == -1) || (posOnF.y != Count-2 && i == 1))
                    {
                        Inst[posOnF.y + i, posOnF.x].isChangable = Inst[posOnF.y + i, posOnF.x].isWater;
                        if((posOnF.x != 1 && i == -1) || (posOnF.x != Count - 2 && i == 1))
                        {
                            Inst[posOnF.y, posOnF.x + i].isChangable = Inst[posOnF.y, posOnF.x + i].isWater;
                            Inst[posOnF.y + i, posOnF.x + i].isChangable = Inst[posOnF.y + i, posOnF.x].isChangable && Inst[posOnF.y, posOnF.x + i].isChangable && Inst[posOnF.y + i, posOnF.x + i].isWater;
                        }
                    }
                }
                if(i == 0)
                {
                    if (j == 0) continue;
                    if((posOnF.x != 1 && j == -1) || (posOnF.x != Count - 2 && j == 1))
                    {
                        Inst[posOnF.y, posOnF.x + j].isChangable = Inst[posOnF.y, posOnF.x + j].isWater;
                        if((posOnF.y != 1 && j == 1) || (posOnF.y != Count - 2 && j == -1))
                        {
                            Inst[posOnF.y + -1 * j, posOnF.x].isChangable = Inst[posOnF.y + -1 * j, posOnF.x].isWater;
                            Inst[posOnF.y + -1 * j, posOnF.x + j].isChangable = Inst[posOnF.y + -1 * j, posOnF.x].isChangable && Inst[posOnF.y, posOnF.x + j].isChangable && Inst[posOnF.y + -1 * j, posOnF.x + j].isWater;
                        }
                    }
                }
            }
        }
    }
}
