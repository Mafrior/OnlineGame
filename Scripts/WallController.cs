using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallController : MonoBehaviour
{
    [SerializeField]
    private Sprite[] wallSprites;
    [SerializeField]
    private Sprite[] destroingWalls;
    public int index;

    public bool wallDestroied;

    public void SetSprite(bool isEnd = false)
    {
        if (GetComponentInParent<PlatformController>().isWater || wallDestroied)
        {
            return;
        }
        if (!isEnd)
        {
            GetComponent<SpriteRenderer>().sprite = destroingWalls[index];
            return;
        }
        GetComponent<SpriteRenderer>().sprite = wallSprites[index];
    }
}
