using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameController : MonoBehaviour
{
    private static GameController gameControl;

    public UnityEvent OnMoveComplete;

    private GameObject[,] allSquares;

    private GameObject pieceOne = null;
    private GameObject pieceTwo = null;

    private Square originSquare;

    private int moveCompleteCount = 0;
    private bool movingPieces = false;

    // Init
    private void Awake()
    {
        gameControl = this;
        gameControl.OnMoveComplete.AddListener(MoveComplete);
    }

    // Update is called once per frame
    void Update()
    {
        // Debug controls
        if (Input.GetKeyDown("space"))
        {
            ClearPieces();
        }
    }

    // Sets the allSquares 2D array with data from the Board GameObject.
    public void SetSquares(int width, int height, GameObject[,] squareArray)
    {
        allSquares = new GameObject[width, height];
        allSquares = squareArray;
    }

    // Handles when a game piece is selected. Allocates the proper handling.
    public void PieceSelected(GameObject piece)
    {
        if (!movingPieces)
        {
            if (pieceOne == null)
            {
                // First piece selected.
                int originX = piece.GetComponent<Piece>().x;
                int originY = piece.GetComponent<Piece>().y;
                originSquare = allSquares[originX, originY].GetComponent<Square>();
                originSquare.SwitchSprite(true);

                pieceOne = piece;
            }
            else if (piece != pieceOne && pieceTwo == null)
            {
                // Second piece selected - check if it's a good selection.
                pieceTwo = piece;
                CheckLegalMove();

            }
            else if (piece == pieceOne)
            {
                // Origin piece was selected again - deselect.
                originSquare.SwitchSprite(false);
                ClearPieces();
            }
        }
    }

    // Checkes if the selected pieces are a good selection for a move. If not, the second piece becomes the new pieceOne and the piece selection resets.
    private void CheckLegalMove()
    {
        Vector2 pieceOnePos = pieceOne.transform.position;
        Vector2 pieceTwoPos = pieceTwo.transform.position;

        float xDiff = Math.Abs(pieceOnePos.x - pieceTwoPos.x);
        float yDiff = Math.Abs(pieceOnePos.y - pieceTwoPos.y);

        if(xDiff == 1 && yDiff == 0 || xDiff == 0 && yDiff == 1)
        {
            MovePieces(pieceOne, pieceTwo);
        }
        else
        {
            GameObject newOriginPiece = pieceTwo;
            originSquare.SwitchSprite(false);
            ClearPieces();
            PieceSelected(newOriginPiece);
        }
    }

    private void MovePieces(GameObject pieceOne, GameObject pieceTwo)
    {
        // Set vars for piece one move - reminder: it's moving to pieceTwo location
        Vector2 destinationOne = pieceTwo.transform.position;

        // Set vars for piece two move
        Vector2 destinationTwo = pieceOne.transform.position;

        //Start Moves
        pieceOne.GetComponent<Piece>().StartMove(destinationOne);
        pieceTwo.GetComponent<Piece>().StartMove(destinationTwo);
    }

    private void MoveComplete()
    {
        moveCompleteCount++;
        if(moveCompleteCount >= 2)
        {
            originSquare.SwitchSprite(false);

            moveCompleteCount = 0;
            ClearPieces();
        }
    }

    private void ClearPieces()
    {
        pieceOne = null;
        pieceTwo = null;

        originSquare = null;
    }
}
