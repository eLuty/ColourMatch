using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

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
    public GameObject[,] allPieces;

    // Move System Vars
    //public UnityEvent OnMoveComplete;

    public bool piecesMoving = false;

    private bool processingTurn = false;

    private GameObject pieceOne = null;
    private GameObject pieceTwo = null;

    private Square originSquare;

    public List<GameObject> movingPieces = new List<GameObject>();

    private int undoCount = 0;

    // Match System Vars
    //private List<GameObject> matchedPieces = new List<GameObject>();

    private bool undoMove = false;

    private int undoMoveCount = 0;

    // Scoring variables
    public int currentScore = 0;
    [SerializeField] private int basicMatchScore;



    // Init
    private void Awake()
    {
        // SLOPPY - DI this
        gameControl = this;
        //gameControl.OnMoveComplete.AddListener(MovementComplete);

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
            EndTurn();
        }

        if(movingPieces.Count == 0 && piecesMoving)
        {
            // all pieces are done moving, remove matches.
            piecesMoving = false;

            StartCoroutine(RemoveMatches());
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

    // Add game pieces to the board
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

    // Check to make sure no more than 2 pieces of the same type get placed next to one another
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

    // Handles when a game piece is selected. Allocates the proper handling.
    public void PieceSelected(GameObject piece)
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
            EndTurn();
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
            originSquare.SwitchSprite(false);
            originSquare = null;

            piecesMoving = true;

            MovePiece(pieceOne, pieceTwo.transform.position);
            MovePiece(pieceTwo, pieceOne.transform.position);
        }
        else
        {
            GameObject newOriginPiece = pieceTwo;
            originSquare.SwitchSprite(false);

            // Reset turn
            EndTurn();
            PieceSelected(newOriginPiece);
        }
    }

    private void MovePiece(GameObject piece, Vector2 destination)
    {
        movingPieces.Add(piece);
        piece.GetComponent<Piece>().StartMove(destination);
    }

    public void MovementComplete(GameObject piece)
    {
        UpdateArrayPosition(piece);
        CheckForMatch(piece);

        //if (!undoMove)
        //{
        //    CheckForMatch(piece);
        //}
        //else
        //{
        //    //undoMoveCount++;
        //    //if(undoMoveCount >= 2)
        //    //{
        //    //    undoMove = false;
        //    //    undoMoveCount = 0;

        //    //    // End turn
        //    //    EndTurn();
        //    //}
        //}
    }

    private void UpdateArrayPosition(GameObject piece)
    {
        int newX = (int)piece.transform.position.x;
        int newY = (int)piece.transform.position.y;

        allPieces[newX, newY] = piece;
    }

    private void CheckForMatch(GameObject originPiece)
    {

        List < GameObject > horizontalMatches = GetHorizontalMatches(originPiece);
        bool isHorizontalMatch = SetMatches(horizontalMatches);

        List<GameObject> verticalMatches = GetVerticalMatches(originPiece);
        bool isVerticalMatch = SetMatches(verticalMatches);

        movingPieces.Remove(originPiece);

        //    List<GameObject> horizontalMatches = GetHorizontalMatches(originPiece);
        //    bool isHorizontalMatch = SetMatches(horizontalMatches);

        //    List<GameObject> verticalMatches = GetVerticalMatches(originPiece);
        //    bool isVerticalMatch = SetMatches(verticalMatches);

        //    movingPieces.Remove(originPiece);

        //if (isHorizontalMatch || isVerticalMatch)
        //{
        //    //RemoveMatches();

        //    // there is a match!

        //}
        //else
        //{
        //    //if (originPiece == pieceOne || originPiece == pieceTwo)
        //    //{
        //    //    // undo move
        //    //    UndoMove();
        //    //}
        //    //else
        //    //{
        //    //    // end turn
        //    //    EndTurn();
        //    //}
        //}
    }

    private List<GameObject> GetHorizontalMatches(GameObject originPiece) 
    {
        List<GameObject> horizontalMatches = new List<GameObject> { originPiece };
        int originX = (int)originPiece.transform.position.x;
        int originY = (int)originPiece.transform.position.y;

        // check right
        int maxRight = originX + 2;
        for(int x = originX + 1; x <= maxRight; x++)
        {
            if (x < _width && allPieces[x, originY] != null && originPiece.CompareTag(allPieces[x, originY].tag))
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
            if (x >= 0 && allPieces[x, originY] != null && originPiece.CompareTag(allPieces[x, originY].tag))
                horizontalMatches.Add(allPieces[x, originY]);
            else
                break;
        }

        return horizontalMatches;
    }

    private List<GameObject> GetVerticalMatches(GameObject originPiece)
    {
        List<GameObject> verticalMatches = new List<GameObject> { originPiece };
        int originX = (int)originPiece.transform.position.x;
        int originY = (int)originPiece.transform.position.y;

        // check up
        int maxUp = originY + 2;
        for (int y = originY + 1; y <= maxUp; y++)
        {
            if (y < _height && allPieces[originX, y] != null && originPiece.CompareTag(allPieces[originX, y].tag))
                verticalMatches.Add(allPieces[originX, y]);
            else
                break;
        }

        // check down
        int maxDown = originY - 2;
        for (int y = originY - 1; y >= maxDown; y--)
        {
            if (y >= 0 && allPieces[originX, y] != null && originPiece.CompareTag(allPieces[originX, y].tag))
                verticalMatches.Add(allPieces[originX, y]);
            else
                break;
        }

        return verticalMatches;
    }

    private bool SetMatches(List<GameObject> matches)
    {
        if (matches.Count >= 3)
        {
            SetScore(matches.Count);
            foreach (GameObject piece in matches)
            {
                piece.GetComponent<Piece>().isMatched = true;
            }
            return true;
        }
        else { return false; }
    }

    private void SetScore(int matchCount)
    {
        int points = 0;

        switch (matchCount) {
            case 3:
                // basic score
                points = basicMatchScore;
                break;

            case 4:
                // basic score + 50
                points = basicMatchScore + 50;
                break;

            case 5:
                // basic score + 100
                points = basicMatchScore + 100;
                break;

            default:
                Debug.Log("score count outside the norm. hMatch count: " + matchCount);
                break;
        }

        currentScore += points;
    }

    private void HandleMatches()
    {
        Debug.Log("HandleMatches called. movingPieces.Count: " + movingPieces.Count);

        if (movingPieces.Count == 0)
        {
            Debug.Log("Remove matches now.");
            //RemoveMatches();
            StartCoroutine(RemoveMatches());
        }
    }

    private void UndoMove()
    {
        undoCount++;
        if(undoCount >= 2)
        {
            undoCount = 0;

            MovePiece(pieceOne, pieceTwo.transform.position);
            MovePiece(pieceTwo, pieceOne.transform.position);
        }
    }

    private IEnumerator RemoveMatches()
    {
        Debug.Log("Removing...");
        yield return new WaitForSeconds(1.5f);

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                if (allPieces[x, y] != null && allPieces[x, y].GetComponent<Piece>().isMatched)
                {
                    Destroy(allPieces[x, y]);
                    allPieces[x, y] = null;
                }
            }
        }

        //StartCoroutine(CheckForNulls());
        CheckForNulls();
    }

    private void CheckForNulls()
    {
        Debug.Log("Remove complete. Check for nulls...");

        int nullSpaces = 0;
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                if (allPieces[x, y] == null)
                {
                    nullSpaces++;
                }
                else if (nullSpaces > 0)
                {
                    // move the next piece down nullSpaces 
                    GameObject piece = allPieces[x, y];
                    Vector2 destination = new Vector2(piece.transform.position.x, piece.transform.position.y - nullSpaces);
                    //MovePiece(piece, destination);

                    Debug.Log("Move down: " + piece.name);
                }
            }

            // reset null space counter after each column
            nullSpaces = 0;
        }
        //yield return new WaitForSeconds(.8f);
        //EndTurn();
    }

    private void EndTurn()
    {

        Debug.Log("end turn!");

        pieceOne = null;
        pieceTwo = null;
        originSquare = null;
    }
}
