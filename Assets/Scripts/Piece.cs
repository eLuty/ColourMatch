using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    // Game vars
    public GameController gameControl;

    public GameObject gamePiece;

    // Position & movement vars
    public int x;
    public int y;

    private Vector2 homePosition;
    private Vector2 destinationPosition;

    private bool isMoving;

    // Matched vars
    public bool isMatched = false;
    
    private void Start()
    {
        gamePiece = this.gameObject;
        SetPositionData(gamePiece.transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        if(isMoving)
        {
            if (Vector2.Distance(gamePiece.transform.position, destinationPosition) > 0.01f)
            {
                Vector2 direction = (destinationPosition - homePosition).normalized;
                transform.Translate(direction * 4 * Time.deltaTime);
            }
            else
            {
                FinishMove();
            }
        }

        if (isMatched)
        {
            transform.Rotate(0, 0.5f, 0);
        }
    }

    // Sets the position data for the game piece
    private void SetPositionData(Vector2 newPosition)
    {
        homePosition = newPosition;
        x = (int)newPosition.x;
        y = (int)newPosition.y;
    }

    // Registers mouse click on game piece, passes the selection to the game controller
    private void OnMouseDown()
    {
        gameControl.PieceSelected(gamePiece);
    }

    // Initiates the movement process for the game piece
    public void StartMove(Vector2 destination)
    {
        destinationPosition = destination;
        isMoving = true;
    }

    // Finishes the move piece process
    private void FinishMove()
    {
        isMoving = false;

        // Directly set the position so everything lines up neatly
        gamePiece.transform.position = destinationPosition;
        SetPositionData(destinationPosition);

        // Let the game controller know movement is complete
        //gameControl.OnMoveComplete.Invoke();
        gameControl.MovementComplete(gamePiece);
    }

}
