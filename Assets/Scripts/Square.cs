using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Square : MonoBehaviour
{

    [SerializeField] private Sprite[] sprites;
    //[SerializeField] private bool pieceSelected = false;

    public void SwitchSprite(bool pieceSelected)
    {
        if (pieceSelected)
        {
            this.GetComponentInParent<SpriteRenderer>().sprite = sprites[1];
        }
        else
        {
            this.GetComponentInParent<SpriteRenderer>().sprite = sprites[0];
        }

    }
}
