using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameController : MonoBehaviour
{
    // Core Game Vars
    private static GameController gameControl;

    [SerializeField] private int _width;
    [SerializeField] private int _height;

    [SerializeField] private Board gameBoard;

    [SerializeField] private GameObject squarePrefab;
    [SerializeField] private GameObject[] pieces;

    private GameObject[,] allSquares;
    private GameObject[,] allPieces;

    // Move System Vars
    public UnityEvent OnMoveComplete;

    private GameObject pieceOne = null;
    private GameObject pieceTwo = null;

    private Square originSquare;

    private int moveCompleteCount = 0;
    private bool movingPieces = false;

    // Match System Vars
    private List<GameObject> matchedPieces = new List<GameObject>();

    private bool undoMove = false;

    // Init
    private void Awake()
    {

        // SLOPPY
        gameControl = this;
        gameControl.OnMoveComplete.AddListener(MovementComplete);

        // Set the board
        allSquares = new GameObject[_width, _height];
        allPieces = new GameObject[_width, _height];
        CreateBoard();
        CreateGamePieces();
    }

    // Update is called once per frame
    void Update()
    {
        // Debug controls

        if (Input.GetKeyDown("space"))
        {
            ResetTurnVars();
        }
    }

    // Game board set up
    private void CreateBoard()
    {
        for(int x = 0; x < _width; x++)
        {
            for(int y = 0; y < _height; y++)
            {
                Vector2 spawnPosition = new Vector2(x, y);
                GameObject newSquare = Instantiate(squarePrefab, spawnPosition, Quaternion.identity) as GameObject;
                newSquare.transform.parent = gameBoard.transform;
                newSquare.name = "Square (" + x + ", " + y + ")";

                allSquares[x, y] = newSquare;
            }
        }
    }

    private void CreateGamePieces()
    {
        for(int x = 0; x < _width; x++)
        {
            for(int y = 0; y <_height; y++)
            {
                int pieceToUse = UnityEngine.Random.Range(0, pieces.Length);
                while(CheckPiece(x, y, pieces[pieceToUse]))
                {
                    pieceToUse = UnityEngine.Random.Range(0, pieces.Length);
                }

                Vector2 spawnPosition = new Vector2(x, y);
                GameObject newPiece = Instantiate(pieces[pieceToUse], spawnPosition, Quaternion.identity) as GameObject;
                newPiece.transform.parent = gameBoard.transform;
                newPiece.GetComponent<Piece>().gameControl = this;
                newPiece.name = pieces[pieceToUse].name + "(" + x + ", " + y + ")";

                allPieces[x, y] = newPiece;
            }
        }
    }

    private void SpawnGameObject()
    {

    }

    private bool CheckPiece(int x, int y, GameObject piece)
    {
        if (x <= 1 && y > 1)
        {
            // check down only
            if (allPieces[x, y - 1].CompareTag(piece.tag) && allPieces[x, y - 2].CompareTag(piece.tag))
                return true;
            else
                return false;
        }
        else if (x > 1 && y <= 1)
        {
            // check left only
            if (allPieces[x - 1, y].CompareTag(piece.tag) && allPieces[x - 2, y].CompareTag(piece.tag))
                return true;
            else
                return false;
        }
        else if (x > 1 && y > 1)
        {
            // check left and down
            if (allPieces[x - 1, y].CompareTag(piece.tag) && allPieces[x - 2, y].CompareTag(piece.tag) ||
                allPieces[x, y - 1].CompareTag(piece.tag) && allPieces[x, y - 2].CompareTag(piece.tag))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }













    // Sets the allSquares 2D array with data from the Board GameObject.
    //public void SetObjectArray(int width, int height, GameObject[,] objectArray, bool isSettingSquares)
    //{
    //    if (isSettingSquares)
    //    {
    //        allSquares = new GameObject[width, height];
    //        allSquares = objectArray;
    //    }
    //    else
    //    {
    //        allPieces = new GameObject[width, height];
    //        allPieces = objectArray;
    //    }
    //}

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
        movingPieces = true;

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
            moveCompleteCount = 0;

            // Turn off square highlight
            originSquare.SwitchSprite(false);

            // Update piece array positions
            UpdateArrayPosition(pieceOne);
            UpdateArrayPosition(pieceTwo);

            if (!undoMove)
            {
                // Check for matches & finalize the turn
                bool isPieceOneMatched = CheckPieceMatches(pieceOne);
                bool isPieceTwoMatched = CheckPieceMatches(pieceTwo);

                FinalizeTurn(isPieceOneMatched, isPieceTwoMatched);
            }
            else
            {
                undoMove = false;
                ResetTurnVars();

            }
        }
    }

    private void UpdateArrayPosition(GameObject piece)
    {
        int newX = (int)piece.transform.position.x;
        int newY = (int)piece.transform.position.y;

        allPieces[newX, newY] = piece;
    }

    private bool CheckPieceMatches(GameObject originPiece)
    {
        List<GameObject> horizontalMatches = CheckHorizontalMatches(originPiece);
        List<GameObject> verticalMatches = CheckVerticalMatches(originPiece);

        bool isHorizontalMatch = EvaluateMatches(horizontalMatches);
        bool isVerticalMatch = EvaluateMatches(verticalMatches);

        if(isHorizontalMatch || isVerticalMatch)
        {
            matchedPieces.Add(originPiece);
            return true;
        }
        else { return false; }
    }

    private List<GameObject> CheckHorizontalMatches(GameObject originPiece) 
    {
        List<GameObject> horizontalMatches = new List<GameObject>();
        int originX = (int)originPiece.transform.position.x;
        int originY = (int)originPiece.transform.position.y;

        // check right
        int maxRight = originX + 2;
        for(int x = originX + 1; x <= maxRight; x++)
        {
            if (x < _width && originPiece.CompareTag(allPieces[x, originY].tag))
            {
                horizontalMatches.Add(allPieces[x, originY]);
            }
            else
                break;
        }

        // check left
        int maxLeft = originX - 2;
        for (int x = originX - 1; x >= maxLeft; x--)
        {
            if (x >= 0 && originPiece.CompareTag(allPieces[x, originY].tag))
                horizontalMatches.Add(allPieces[x, originY]);
            else
                break;
        }

        return horizontalMatches;
    }

    private List<GameObject> CheckVerticalMatches(GameObject originPiece)
    {
        List<GameObject> verticalMatches = new List<GameObject>();
        int originX = (int)originPiece.transform.position.x;
        int originY = (int)originPiece.transform.position.y;

        // check up
        int maxUp = originY + 2;
        for (int y = originY + 1; y <= maxUp; y++)
        {
            if (y < _height && originPiece.CompareTag(allPieces[originX, y].tag))
                verticalMatches.Add(allPieces[originX, y]);
            else
                break;
        }

        // check down
        int maxDown = originY - 2;
        for (int y = originY - 1; y >= maxDown; y--)
        {
            if (y >= 0 && originPiece.CompareTag(allPieces[originX, y].tag))
                verticalMatches.Add(allPieces[originX, y]);
            else
                break;
        }

        return verticalMatches;
    }

    private bool EvaluateMatches(List<GameObject> matches)
    {
        // Remember to include origin piece in count.
        if(matches.Count + 1 >= 3)
        {
            matchedPieces.AddRange(matches);
            return true;
        }
        else { return false; }
    }

    private void FinalizeTurn(bool pieceOneMatch, bool pieceTwoMatch)
    {
        if (pieceOneMatch || pieceTwoMatch)
        {
            DestroyMatchedPieces();
            //ResetTurnVars();
        }
        else
        {
            // Undo move
            undoMove = true;
            MovePieces(pieceOne, pieceTwo);
        }

        // I don't know about this sequence... revise this later
        //ResetTurnVars();
    }

    private void DestroyMatchedPieces()
    {
        foreach (GameObject piece in matchedPieces)
        {
            Destroy(allPieces[(int)piece.transform.position.x, (int)piece.transform.position.y]);
        }

        StartCoroutine(CheckForNulls());
    }

    //private void EndTurn()
    //{
    //    ResetTurnVars();

        
    //}

    private void ClearPieces()
    {
        pieceOne = null;
        pieceTwo = null;
        originSquare = null;
    }

    // Method used for testing/debugging only
    private void ResetTurnVars()
    {
        movingPieces = false;

        pieceOne = null;
        pieceTwo = null;
        originSquare = null;

        //isPieceOneMatched = false;
        //isPieceTwoMatched = false;

        matchedPieces.Clear();
    }

    private IEnumerator CheckForNulls()
    {
        Debug.Log("Checking for nulls...");

        int nullSpaces = 0;
        for(int x = 0; x < _width; x++)
        {
            for(int y = 0; y < _height; y++)
            {

                if (allPieces[x, y] == null)
                {
                    nullSpaces++;
                }
            }
        }
        yield return new WaitForSeconds(.4f);

        Debug.Log("Null spaces: " + nullSpaces);
    }
}
