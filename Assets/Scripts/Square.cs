using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Square : MonoBehaviour
{

    [SerializeField] private Sprite[] sprites;

    private bool pieceSelected = false;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SwitchSprite()
    {
        if (!pieceSelected)
        {
            this.GetComponentInParent<SpriteRenderer>().sprite = sprites[1];
            pieceSelected = true;
        }
        else
        {
            this.GetComponentInParent<SpriteRenderer>().sprite = sprites[0];
            pieceSelected = false;
        }

    }
}
