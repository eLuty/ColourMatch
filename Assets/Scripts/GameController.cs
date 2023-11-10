using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private GameObject pieceOne = null;
    [SerializeField] private GameObject pieceTwo = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            DeselectPieceOne();
            pieceTwo = null;

            Debug.Log("cleared piece vars!");
        }
    }

    public void PieceSelected(GameObject piece)
    {
        if (pieceOne == null)
        {
            SelectPieceOne(piece);
        }
        else if (pieceOne != null && pieceTwo == null)
        {
            pieceTwo = piece;
            CheckLegalMove();

        }
        else if (piece == pieceOne)
        {
            DeselectPieceOne();
        }
    }

    private void SelectPieceOne(GameObject piece)
    {
        GameObject startSquare = piece.GetComponent<Piece>().currentSquare;
        startSquare.GetComponent<Square>().SwitchSprite();
        pieceOne = piece;
    }

    private void DeselectPieceOne()
    {
        GameObject startSquare = pieceOne.GetComponent<Piece>().currentSquare;
        startSquare.GetComponent<Square>().SwitchSprite();
        pieceOne = null;

    }

    private void CheckLegalMove()
    {
        Vector2 pieceOnePos = pieceOne.transform.position;
        Vector2 pieceTwoPos = pieceTwo.transform.position;

        float xDiff = Math.Abs(pieceOnePos.x - pieceTwoPos.x);
        float yDiff = Math.Abs(pieceOnePos.y - pieceTwoPos.y);

        if(xDiff == 1 && yDiff == 0 || xDiff == 0 && yDiff == 1)
        {
            Debug.Log("DO MOVE!!!");
        }
        else
        {
            DeselectPieceOne();
            SelectPieceOne(pieceTwo);
            pieceTwo = null;

        }
    }

    private void MovePieces()
    {

    }
}
