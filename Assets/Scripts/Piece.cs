using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    // Game vars
    public GameController gameControl;

    public GameObject gamePiece;
    public GameObject currentSquare;

    // Position vars
    [SerializeField] private Vector2 homePosition;
    private Vector2 destinationPosition;
    
    // Start is called before the first frame update
    void Start()
    {
        homePosition = gamePiece.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        gameControl.PieceSelected(gamePiece);
    }

    public void SetMoveCoordinates(Vector2 destination)
    {

    }
}
