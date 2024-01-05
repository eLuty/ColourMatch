using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class GameController : MonoBehaviour
{
    // Core Game Vars
    [SerializeField] private int _width;
    [SerializeField] private int _height;

    [SerializeField] private Board gameBoard;

    [SerializeField] private GameObject squarePrefab;
    [SerializeField] private GameObject[] pieces;

    private GameObject[,] allSquares;
    private GameObject[,] allPieces;

    // Move System Vars
    private bool piecesMoving = false;

    [SerializeField] private GameObject pieceOne = null;
    [SerializeField] private GameObject pieceTwo = null;

    private Square originSquare;

    private List<GameObject> movingPieces = new List<GameObject>();
    private List<GameObject> piecesToCheck = new List<GameObject>();

    // Scoring variables
    public int currentScore = 0;
    [SerializeField] private int basicMatchScore;

    private void Start()
    {
        allSquares = new GameObject[_width, _height];
        allPieces = new GameObject[_width, _height];

        ObjectPool.SharedInstance.InitPool(_height, _width);
        InitGame();
    }

    private void InitGame()
    {
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
    }

    private void LateUpdate()
    {
        if (movingPieces.Count == 0 && piecesMoving)
        {
            // all pieces are done moving, remove matches.
            piecesMoving = false;
            CheckForMatches();
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

                GameObject newPiece = SpawnGamePiece(x, y, pieceToUse);
                newPiece.name = newPiece.name + "(" + x + ", " + y + ")";
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

    private GameObject SpawnGamePiece(int spawnX, int spawnY, int pieceToUse)
    {
        Vector2 spawnPosition = new Vector2(spawnX, spawnY);
        GameObject newPiece = ObjectPool.SharedInstance.GetPooledGamePiece(pieces[pieceToUse]);
        
        // Set position & positional data
        newPiece.transform.position = spawnPosition;
        newPiece.GetComponent<Piece>().SetPositionData(spawnPosition);

        // Set game controller and activate
        newPiece.GetComponent<Piece>().gameControl = this;
        newPiece.SetActive(true);

        return newPiece;
    }

    // Handles when a game piece is selected. Allocates the proper handling.
    public void PieceSelected(GameObject piece)
    {
        if (pieceOne == null)
        {
            // First piece selected - piece movement snaps to grid coords. Transform position will correlate with 2D array pos.
            int originX = (int)piece.transform.position.x;
            int originY = (int)piece.transform.position.y;
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

    // Move a piece. Add the piece to movement tracking list.
    private void MovePiece(GameObject piece, Vector2 destination)
    {
        if(!piecesMoving) { piecesMoving = true; }

        movingPieces.Add(piece);
        piece.GetComponent<Piece>().StartMove(destination);
    }

    public void MovementComplete(GameObject piece)
    {
        UpdateArrayPosition(piece);

        movingPieces.Remove(piece);
        piecesToCheck.Add(piece);
    }

    private void UpdateArrayPosition(GameObject piece)
    {
        int newX = (int)piece.transform.position.x;
        int newY = (int)piece.transform.position.y;

        allPieces[newX, newY] = piece;
    }

    // Checks if any of the moved pieces have a match. If a match exists, it is flagged on the piece.
    private void CheckForMatches()
    {
        bool isMatch = false;

        // cycle through moved pieces & look for a horizontal/vertical match
        for (int i = 0; i < piecesToCheck.Count; i++)
        {
            GameObject checkPiece = piecesToCheck[i];
            bool hMatch = GetHorizontalMatches(checkPiece);
            bool vMatch = GetVerticalMatches(checkPiece);

            // set flag
            if (!isMatch && hMatch || vMatch)
                isMatch = true;
        }

        piecesToCheck.Clear();

        if (isMatch)
        {
            if (pieceOne != null && pieceTwo != null)
            {
                pieceOne = null;
                pieceTwo = null;
            }

            StartCoroutine(RemoveMatches());
        }
        else if (!isMatch && pieceOne != null && pieceTwo != null)
        {
            UndoMove();
        }
        else
        {
            EndTurn();
        }
    }

    private bool GetHorizontalMatches(GameObject originPiece)
    {
        List<GameObject> horizontalMatches = new List<GameObject> { originPiece };
        int originX = (int)originPiece.transform.position.x;
        int originY = (int)originPiece.transform.position.y;

        // check right
        int maxRight = originX + 2;
        for (int x = originX + 1; x <= maxRight; x++)
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

        if (horizontalMatches.Count >= 3)
        {
            SetMatches(horizontalMatches);
            return true;
        }
        else { return false; }
    }

    private bool GetVerticalMatches(GameObject originPiece)
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

        if (verticalMatches.Count >= 3)
        {
            SetMatches(verticalMatches);
            return true;
        }
        else { return false; }
    }

    private void SetMatches(List<GameObject> matches)
    {
        SetScore(matches.Count);
        foreach (GameObject piece in matches)
        {
            // Cycle through each matched game piece and set the matched flag
            piece.GetComponent<Piece>().SetPieceMatchStatus(true);
        }
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

    private void UndoMove()
    {

        MovePiece(pieceOne, pieceTwo.transform.position);
        MovePiece(pieceTwo, pieceOne.transform.position);

        pieceOne = null;
        pieceTwo = null;
    }

    private IEnumerator RemoveMatches()
    {
        yield return new WaitForSeconds(0.8f);

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                if (allPieces[x, y] != null && allPieces[x, y].GetComponent<Piece>().isMatched)
                {
                    allPieces[x, y].GetComponent<Piece>().SetPieceMatchStatus(false);
                    allPieces[x, y].SetActive(false);
                    allPieces[x, y] = null;
                }
            }
        }

        UpdateBoard();
    }

    private void UpdateBoard()
    {
        int nullSpaces = 0;
        List<GameObject> refillList = new List<GameObject>();
        for (int  x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                if (allPieces[x, y] == null)
                {
                    // null space - spawn & track refill
                    int pieceToUse = UnityEngine.Random.Range(0, pieces.Length);
                    int spawnHeight = _height + nullSpaces;

                    GameObject refillPiece = SpawnGamePiece(x, spawnHeight, pieceToUse);
                    refillPiece.name = refillPiece.name +"(" + x + ", " + y + ")";
                    refillList.Add(refillPiece);

                    nullSpaces++;

                    // check if refills need to be shuffled down
                    AddRefillsToBoard(y, nullSpaces, refillList);
                }
                else if (nullSpaces > 0)
                {
                    // shuffle down column piece
                    GameObject shufflePiece = allPieces[x, y];
                    ShuffleDown(shufflePiece, nullSpaces);
                    allPieces[x, y] = null;

                    // check if refills need to be shuffled down
                    AddRefillsToBoard(y, nullSpaces, refillList);
                }
            }

            nullSpaces = 0;
            refillList.Clear();
        }
    }

    private void AddRefillsToBoard(int y, int nullCount, List<GameObject> refills)
    {
        if (y == _height - 1 && nullCount > 0)
        {
            for(int i = 0; i < refills.Count; i++)
            {
                GameObject refillPiece = refills[i];
                ShuffleDown(refillPiece, nullCount);
            }
        }
    }

    private void ShuffleDown(GameObject shufflePiece, int shuffleSpaces)
    {
        Vector2 shuffleDestination = new Vector2(shufflePiece.transform.position.x, 
                                                    shufflePiece.transform.position.y - shuffleSpaces);
        MovePiece(shufflePiece, shuffleDestination);
    }

    private void EndTurn()
    {
        pieceOne = null;
        pieceTwo = null;
        originSquare = null;
    }
}
