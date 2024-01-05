using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool SharedInstance;
    
    [SerializeField] private GameObject gameBoard;
    [SerializeField] private GameObject gamePiece;

    private List<GameObject> pooledGamePieces = new List<GameObject>();

    private int maxGamePieces;

    private void Awake()
    {
        SharedInstance = this;
    }

    public void InitPool(int boardHeight, int boardWidth)
    {
        maxGamePieces = boardHeight * boardWidth;

        GameObject tmp;
        for (int i = 0; i < maxGamePieces; i++)
        {
            tmp = Instantiate(gamePiece,this.transform.position, Quaternion.identity);
            tmp.transform.parent = gameBoard.transform;
            tmp.SetActive(false);
            pooledGamePieces.Add(tmp);
        }
    }

    public GameObject GetPooledGamePiece(GameObject template)
    {
        for (int i = 0; i < maxGamePieces; i++)
        {
            // Cycle through pooled objects and check if active
            if (!pooledGamePieces[i].activeInHierarchy)
            {
                GameObject newPiece = pooledGamePieces[i];

                // Set to template vars
                newPiece.name = template.name;
                newPiece.tag = template.tag;
                newPiece.GetComponent<SpriteRenderer>().color = template.GetComponent<SpriteRenderer>().color;

                // Return pooled obj
                return newPiece;
            }
        }

        // No actives - return
        return null;
    }
}
