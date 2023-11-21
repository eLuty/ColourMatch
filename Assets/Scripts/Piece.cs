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
    
    private void Start()
    {
        SetPositionData(gamePiece.transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        if (isMoving)
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
    }

    private void SetPositionData(Vector2 newPosition)
    {
        homePosition = newPosition;
        x = (int)newPosition.x;
        y = (int)newPosition.y;
    }

    private void OnMouseDown()
    {
        gameControl.PieceSelected(gamePiece);
    }

    public void StartMove(Vector2 destination)
    {
        destinationPosition = destination;
        isMoving = true;
    }

    private void FinishMove()
    {
        isMoving = false;

        gamePiece.transform.position = destinationPosition;
        SetPositionData(destinationPosition);
         
        gameControl.OnMoveComplete.Invoke();
    }

}
