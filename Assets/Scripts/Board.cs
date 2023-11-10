using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    // Game board vars
    [SerializeField] private int wdith;
    [SerializeField] private int height;

    [SerializeField] private GameObject squarePrefab;

    //Game control vars
    [SerializeField] private GameController gameControl;
    
    [SerializeField] public GameObject[,] allPieces;
    private GameObject[,] allSquares;

    // Game pieces vars
    [SerializeField] private GameObject[] pieces;


    // Start is called before the first frame update
    void Start()
    {
        allSquares = new GameObject[wdith, height];
        allPieces = new GameObject[wdith, height];
        CreateBoard();
        CreatePieces();
    }

    private void CreateBoard()
    {
       for(int x = 0; x < wdith; x++)
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
    }

    private void CreatePieces()
    {
        for(int x = 0;x < wdith; x++)
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
                pieceComponent.currentSquare = allSquares[x, y];

                allPieces[x, y] = piece;
            }
        }
    }
}
