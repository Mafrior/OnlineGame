using UnityEngine;
using System.Collections.Generic;

public class Person : MonoBehaviour
{
    public Vector2 position;
    public bool isReadyToStep { get; set; }
    delegate void doAction(params int[] a);
    Dictionary<int, doAction> actions;
    public bool pickedUpArtefact;

    private void Awake()
    {
        actions = new Dictionary<int, doAction>()
        {
            { 1, Step},
            { 2, Step},
            { 3, null},
            { 4, GiveAnArtefact}
        };
    }

    private void OnMouseDown()
    {
        ActionManager.chosenObject = gameObject;
        actions[ActionManager.action](ActionManager.action);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.transform.CompareTag("Artifact") && !pickedUpArtefact)
        {
            other.transform.SetParent(transform);
            pickedUpArtefact = true;
        }
    }

    void Step(params int[] a)
    {
        if (a[0] == 2 || a[0] == 1 && this == Client.Inst.player)
        {
            if (!isReadyToStep)
            {
                AllCheck();
                isReadyToStep = true;
                return;
            }
            ActionManager.Inst.СlearActionWithPlat();
            isReadyToStep = false;
        }
    }

    void GiveAnArtefact(params int[] a)
    {
        if (Client.Inst.player != this && !pickedUpArtefact)
        {
            ClientSend.GiveArtifactTo(position);
        }
    }

    void AllCheck()
    {
        PlatformController leftDown = FIeld.Inst[position.y - 1, position.x - 1];
        PlatformController midDown = FIeld.Inst[position.y - 1, position.x];
        PlatformController rightDown = FIeld.Inst[position.y - 1, position.x + 1];
        PlatformController left = FIeld.Inst[position.y, position.x - 1];
        PlatformController right = FIeld.Inst[position.y, position.x + 1];
        PlatformController leftUp = FIeld.Inst[position.y + 1, position.x - 1];
        PlatformController midUp = FIeld.Inst[position.y + 1, position.x];
        PlatformController rightUp = FIeld.Inst[position.y + 1, position.x + 1];
        PlatformController thisPlat = FIeld.Inst[position.y, position.x];

        if (midDown.rotation == "up" && !midDown.isWater || thisPlat.rotation == "down")
        {
            leftDown.isStepOn = -1;
            midDown.isStepOn = -1;
            rightDown.isStepOn = -1;
        }
        else
        {
            midDown.isStepOn = 1;
            if ((leftDown.rotation == "right" || leftDown.rotation == "up") && !leftDown.isWater || left.rotation == "down" || midDown.rotation == "left")
            {
                leftDown.isStepOn = -1;
            }
            else { leftDown.isStepOn = 1; }
            if ((rightDown.rotation == "left" || rightDown.rotation == "up") && !rightDown.isWater || right.rotation == "down" || midDown.rotation == "right")
            {
                rightDown.isStepOn = -1;
            }
            else { rightDown.isStepOn = 1; }
        }

        if (midUp.rotation == "down" && !midUp.isWater || thisPlat.rotation == "up")
        {
            leftUp.isStepOn = -1;
            midUp.isStepOn = -1;
            rightUp.isStepOn = -1;
        }
        else
        {
            midUp.isStepOn = 1;
            if ((leftUp.rotation == "right" || leftUp.rotation == "down") && !leftUp.isWater || left.rotation == "up" || midUp.rotation == "left")
            {
                leftUp.isStepOn = -1;
            }
            else { leftUp.isStepOn = 1; }
            if ((rightUp.rotation == "left" || rightUp.rotation == "down") && !leftDown.isWater || right.rotation == "up" || midUp.rotation == "right")
            {
                rightUp.isStepOn = -1;
            }
            else { rightDown.isStepOn = 1; }
        }

        if (left.rotation == "right" && !left.isWater || thisPlat.rotation == "left")
        {
            leftDown.isStepOn = -1;
            left.isStepOn = -1;
            leftUp.isStepOn = -1;
        }
        else { left.isStepOn = 1; }

        if (right.rotation == "left" && !right.isWater || thisPlat.rotation == "right")
        {
            rightDown.isStepOn = -1;
            right.isStepOn = -1;
            rightUp.isStepOn = -1;
        }
        else { right.isStepOn = 1; }
    }
}
