using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ActionManager : MonoBehaviour
{
    public static ActionManager Inst;

    public GameObject startUI;
    public GameObject gameUI;

    public InputField usernameField;
    [SerializeField]
    private Button[] actionButtons;

    [SerializeField]
    private Text[] queueShowing;
    private QueueElement[] queue;

    public static GameObject chosenObject;
    public static int action;

    private void Awake()
    {
        if (Inst == null)
        {
            Inst = this;
            action = 1;
            return;
        }
        if (Inst == this)
        {
            Debug.Log("Instznce already exists");
            Destroy(this);
        }
    }

    /// <summary> Обнуляет как смену платформ, так и толчки персонажей/ходьбу нашего персонажа</summary>
    public void СlearActionWithPlat()
    {
        Vector2 pos;
        if (chosenObject.CompareTag("Player"))
        {
            pos = chosenObject.GetComponent<Person>().position;
        }
        else { pos = chosenObject.GetComponent<PlatformController>().posOnF; }
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                FIeld.Inst[pos.y + i, pos.x + j].isChangable = false;
                FIeld.Inst[pos.y + i, pos.x + j].isStepOn = 0;
            }
        }
    }

    public void GetAction(int buttonNumb)
    {
        chosenObject = null;
        action = buttonNumb;
    }

    public void ConnectToServer()
    {
        startUI.SetActive(false);
        gameUI.SetActive(true);
        usernameField.interactable = false;
        SceneManager.LoadScene(1);
        Client.Inst.ConnectToServer();
    }

    public void GetQueue(int _playersCount, Packet _packet)
    {
        queue = new QueueElement[_playersCount];
        for (int i = 0; i < _playersCount; i++)
        {
            if (i == 0)
            {
                queue[i] = new QueueElement(null, _packet.ReadInt().ToString());
                if (_playersCount == 1)
                {
                    queue[i].next = queue[i];
                }
                continue;
            }
            if (i == _playersCount - 1)
            {
                queue[i] = new QueueElement(queue[0], _packet.ReadInt().ToString());
                queue[i - 1].next = queue[i];
                continue;
            }
            queue[i] = new QueueElement(null, _packet.ReadInt().ToString());
            queue[i - 1].next = queue[i];
        }
        ShowQueue();
    }

    public void MoveQueue()
    {
        for (int i = 0; i < queue.Length; i++)
        {
            queue[i] = queue[i].next;
        }
        ShowQueue();
    }

    public void ShowQueue()
    {
        queueShowing[0].text = queue[0].value;
        queueShowing[1].text = queue[0].next.value;
        queueShowing[2].text = queue[0].next.next.value;
        if (queueShowing[1].text == Client.Inst.myId.ToString())
        {
            for (int i = 0; i < actionButtons.Length; i++)
            {
                if (i == 2)
                {
                    actionButtons[i].interactable = Client.Inst.player.pickedUpArtefact;
                    continue;
                }
                actionButtons[i].interactable = true;
            }
            return;
        }
        for (int i = 0; i < actionButtons.Length; i++)
        {
            actionButtons[i].interactable = false;
        }
    }
}
