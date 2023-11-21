using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class GameController : MonoBehaviour
{
    // Game Vars
    [SerializeField] private int width;
    [SerializeField] private int height;

    private static GameController gameControl;

    public UnityEvent OnMoveComplete;

    private GameObject[,] allSquares;
    private GameObject[,] allPieces;

    private GameObject pieceOne = null;
    private GameObject pieceTwo = null;

    private bool isHorizontalMatch = false;
    private bool isVerticalMatch = false;

    [SerializeField] private List<GameObject> horizontalMatches = new List<GameObject>();
    [SerializeField] private List<GameObject> verticalMatches = new List<GameObject>();

    private Square originSquare;

    private int moveCompleteCount = 0;
    private bool movingPieces = false;

    // Init
    private void Awake()
    {
        gameControl = this;
        gameControl.OnMoveComplete.AddListener(MovementComplete);
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
    public void SetObjectArray(int width, int height, GameObject[,] objectArray, bool isSettingSquares)
    {
        if (isSettingSquares)
        {
            allSquares = new GameObject[width, height];
            allSquares = objectArray;
        }
        else
        {
            allPieces = new GameObject[width, height];
            allPieces = objectArray;
        }
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

    private void MovementComplete()
    {
        

        moveCompleteCount++;
        if(moveCompleteCount >= 2)
        {

            Debug.Log("MovementComplete()");

            moveCompleteCount = 0;


            // Turn off square highlight
            originSquare.SwitchSprite(false);

            // update piece array & calculate matches
            UpdateArrayPosition(pieceOne);
            UpdateArrayPosition(pieceTwo);



            CalculateMatches(pieceOne);
            CalculateMatches(pieceTwo);

            // finalize move
            // TODO: move this line to the end of the process
            ClearPieces();
        }
    }

    private void UpdateArrayPosition(GameObject piece)
    {
        int newX = (int)piece.transform.position.x;
        int newY = (int)piece.transform.position.y;

        allPieces[newX, newY] = piece;
    }

    private void CalculateMatches(GameObject piece)
    {
        Debug.Log("CalculateMatches()");

        Debug.Log("Calculating matches for piece: " + piece);

        int x = (int)piece.transform.position.x;
        int y = (int)piece.transform.position.y;

        List<GameObject> rightMatches = CheckMatches(x + 1, x + 2, y, y, piece);
        List<GameObject> leftMatches = CheckMatches(x - 2, x - 1, y, y, piece);
        List<GameObject> upMatches = CheckMatches(x, x, y + 1, y + 2, piece);
        List<GameObject> downMatches = CheckMatches(x, x, y - 2, y - 1, piece);

        List<GameObject> horizontalMatches = new List<GameObject> { piece };
        horizontalMatches.AddRange(rightMatches);
        horizontalMatches.AddRange(leftMatches);

        List<GameObject> verticalMatches = new List<GameObject> { piece };
        verticalMatches.AddRange(upMatches);
        verticalMatches.AddRange(downMatches);

        Debug.Log("horizontalMatches: " + horizontalMatches);
        Debug.Log("verticalMatches: " + verticalMatches);

        //EvaluateMatches(horizontalMatches);
        //EvaluateMatches(verticalMatches);
    }

    private List<GameObject> CheckMatches(int startX, int endX, int startY, int endY, GameObject originPiece)
    {
        List<GameObject> matches = new List<GameObject>();
        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                if (x >= 0 && x < width && y >= 0 && y < height)
                {
                    // get piece and check for match
                    if (originPiece.CompareTag(allPieces[x, y].tag))
                    {
                        matches.Add(allPieces[x,y]);
                    }
                }
            }
        }

        return matches;
    }

    private void EvaluateMatches(List<GameObject> matches)
    {
        Debug.Log("EvaluateMatches()");
        Debug.Log("matches: " + matches);

        bool isMatch = false;
        switch (matches.Count) {
            case 3:
                // basic match
                //Debug.Log("Match 3!");

                // add points 
                // clear matched list
                isMatch = true;
                break;

            case 4:
                // match 4 - give bonus
                //Debug.Log("Match 4!!");
                isMatch = true;
                break;

            case 5:
                // match 5 - mega bonus!
                //Debug.Log("Match 5!!!");
                isMatch = true;
                break;

            default:
                // no match
                //Debug.Log("NO MATCH -- UNDO MOVE");


                break;
        }

        if (isMatch)
            DestroyMatchedPieces(matches);
        else
            // no matches, now what?
            matches.Clear();
    }

    private void DestroyMatchedPieces(List<GameObject> matches)
    {
        foreach (GameObject piece in matches)
        {
            Destroy(allPieces[(int)piece.transform.position.x, (int)piece.transform.position.y]);
        }
    }

    private void ClearPieces()
    {
        pieceOne = null;
        pieceTwo = null;
        originSquare = null;
    }
}
