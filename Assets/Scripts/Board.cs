using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    [SerializeField] private int wdith;
    [SerializeField] private int height;

    [SerializeField] private GameObject squarePrefab;    

    private Square[,] allSquares;


    // Start is called before the first frame update
    void Start()
    {
        allSquares = new Square[wdith, height];
        SetUp();
    }

    private void SetUp()
    {
       for(int x = 0; x < wdith; x++)
        {
            for(int y = 0; y < height; y++)
            {
                Vector2 tempPosition = new Vector2(x, y); 
                GameObject newSquare = Instantiate(squarePrefab, tempPosition, Quaternion.identity) as GameObject;
                newSquare.transform.parent = this.transform;
                newSquare.name = "Square (" + x + ", " + y + ")";
            }
        }
    }
}
