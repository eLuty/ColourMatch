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
    }
}
