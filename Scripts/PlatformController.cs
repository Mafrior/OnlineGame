using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PlatformController : MonoBehaviour
{
    
    public bool isWater;
    public string rotation;
    public int isStepOn;
    public Vector2 posOnF;
    public bool isChangable;

    /// <summary> -1 - Нельзя наступить, 0 - просто платформа, 1 - можно наступить
    private int check;

    delegate void doAction();
    Dictionary<int, doAction> actions;

    [SerializeField]
    private ParticleSystem shadowLeft;
    [SerializeField]
    private ParticleSystem shadowRight;

    [SerializeField]
    private Sprite[] sprites;
    public WallController wall;

    private void Start()
    {
        wall = transform.GetChild(0).GetComponent<WallController>();
        if (!isWater)
        {
            wall.index = rotation == "right" ? 0 : rotation == "up" ? 1 : rotation == "left" ? 2 : 3;
            wall.SetSprite(true);
        }
        actions = new Dictionary<int, doAction>()
        {
            {1, MovePerson },
            {2, Jab },
            {3, DestroyWall}
        };
        if (!isWater)
        {
            GetComponent<SpriteRenderer>().sprite = sprites[1];
        }
    }

    private void Update()
    {
        if (isStepOn == check) { return; }
        check = isStepOn;
        SpriteActivation();
    }

    public void OnMouseUp()
    {
        actions[ActionManager.action]();
    }

    private void OnMouseEnter()
    {
        if (isChangable)
        {
            GetComponent<SpriteRenderer>().sprite = sprites[5];
            return;
        }
        if (!isWater && isStepOn == 1)
        {
            GetComponent<SpriteRenderer>().sprite = sprites[4];
            return;
        }
        if (ActionManager.action == 3)
        {
            wall.SetSprite();
        }
    }

    private void OnMouseExit()
    {
        SpriteActivation();
    }

    public void SpriteActivation()
    {
        if (isWater && isStepOn != 1) { GetComponent<SpriteRenderer>().sprite = null; return; }
        if (isWater && isStepOn == 1) { GetComponent<SpriteRenderer>().sprite = sprites[3]; return; }
        GetComponent<SpriteRenderer>().sprite = sprites[isStepOn + 1];
        wall.SetSprite(true);
    }

    void DestroyWall()
    {
        ActionManager.Inst.GetAction(1);
        ClientSend.WallDestroy(posOnF);
    }

    void Jab()
    {
        GameObject cho = ActionManager.chosenObject;
        if (cho?.tag == "Player")
        {
            MovePerson();
            ActionManager.Inst.GetAction(1);
            return;
        }
        if (cho?.tag == "Platform")
        {
            if (isChangable)
            {
                ActionManager.Inst.СlearActionWithPlat();
                ClientSend.PlatformPull(cho.GetComponent<PlatformController>().posOnF, posOnF);
                ActionManager.Inst.GetAction(1);
            }
            return;
        }
        FIeld.MovePlatform(posOnF);
        ActionManager.chosenObject = gameObject;
    }

    void MovePerson()
    {
        if (isStepOn == 1)
        {
            ClientSend.MovePlayer(posOnF, PlayersManager.Inst.players.FirstOrDefault(x => x.Value == ActionManager.chosenObject).Key); // Пересылаем передвижение игрока на сервер, которое обработается в PlayersManager
            ActionManager.Inst.СlearActionWithPlat(); // Обнуляем спрайты
            ActionManager.chosenObject.GetComponent<Person>().isReadyToStep = false; // Сменяем флашок, чтобы не нажимать два раза
            Instantiate(shadowLeft, transform);
            Instantiate(shadowRight, transform);
        }
    }
}
