using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QueueElement
{
    public QueueElement next;
    public string value;

    public QueueElement(QueueElement _next, string _value)
    {
        next = _next;
        value = _value;
    }
}
