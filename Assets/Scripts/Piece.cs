using UnityEngine;

public class Piece : MonoBehaviour
{
    // Game vars
    public GameController gameControl;

    private GameObject gamePiece;

    // Position & movement vars
    private Vector2 homePosition;
    private Vector2 destinationPosition;

    private bool isMoving;

    // Matched vars
    public bool isMatched = false;
    
    private void Start()
    {
        // DI this?
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
                transform.Translate(4.5f * Time.deltaTime * direction);
            }
            else
            {
                FinishMove();
            }
        }

        // piece is matched visual cue
        if (isMatched)
        {
            transform.Rotate(0, 0.4f, 0);
        }
    }

    // Sets the position data for the game piece
    public void SetPositionData(Vector2 newPosition)
    {
        homePosition = newPosition;
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
        gameControl.MovementComplete(gamePiece);
    }

    // Sets the game piece's match status
    public void SetPieceMatchStatus(bool matchStatus)
    {
        isMatched = matchStatus;

        // reset piece rotation
        if (!isMatched)
            transform.rotation = Quaternion.identity;
    }
}
