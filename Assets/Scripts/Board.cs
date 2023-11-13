using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    // Game board vars
    [SerializeField] private int width;
    [SerializeField] private int height;

    [SerializeField] private GameObject squarePrefab;

    //Game control vars
    [SerializeField] private GameController gameControl;
    
    private GameObject[,] allPieces;
    private GameObject[,] allSquares;

    // Game pieces vars
    [SerializeField] private GameObject[] pieces;

    void Start()
    {
        allSquares = new GameObject[width, height];
        allPieces = new GameObject[width, height];
        CreateBoard();
        CreatePieces();
    }

    private void CreateBoard()
    {
       for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                Vector2 tempPosition = new Vector2(x, y); 
                GameObject newSquare = Instantiate(squarePrefab, tempPosition, Quaternion.identity) as GameObject;
                newSquare.transform.parent = this.transform;
                newSquare.name = "Square (" + x + ", " + y + ")";

                allSquares[x, y] = newSquare;
            }
        }

        gameControl.SetSquares(width, height, allSquares);
    }

    private void CreatePieces()
    {
        for(int x = 0; x < width; x++)
        {
            for(int y=0; y < height; y++)
            {
                int pieceToUse = Random.Range(0, pieces.Length);                
                Vector2 tempPosition = new Vector2(x, y);

                GameObject piece = Instantiate(pieces[pieceToUse], tempPosition, Quaternion.identity) as GameObject;
                piece.transform.parent = this.transform;
                piece.name = pieces[pieceToUse].name + "(" + piece.transform.position.x + ", " + piece.transform.position.y + ")";

                Piece pieceComponent = piece.GetComponent<Piece>();
                pieceComponent.gameControl = gameControl;
                pieceComponent.gamePiece = piece;

                // is this needed still?
                //pieceComponent.currentSquare = allSquares[x, y].GetComponent<Square>();

                allPieces[x, y] = piece;
            }
        }
    }
}
