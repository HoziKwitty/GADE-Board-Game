using System;
using System.Collections;
using System.Collections.Generic;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEditor;
using UnityEngine;

public class GameBoard : MonoBehaviour
{
    [Header("Art")] 
    [SerializeField] private Material materialTexture;
    [SerializeField] private float tileSize = 1.0f;
    [SerializeField] private float yOffset = 0.2f;
    [SerializeField] private Vector3 boardCenter = Vector3.zero;

    [Header("Arrays")]
    [SerializeField] private Material[] teamMaterial;
    [SerializeField] private GameObject[] prefabs;
    
    // LOGIC
    private const int tileCount_X = 13;
    private const int tileCount_Y = 10;
    private GameObject[,] tiles;

    private Camera mainCamera;
    private Vector2Int mousePos;

    private Vector3 border;
    private BoardPieces[,] boardPieces;
    private BoardPieces holding;

    private void Awake()
    {
        mainCamera = Camera.main;

        GenerateGrid(1, tileCount_X, tileCount_Y);
        
        GenerateAllSquares();
        PositionAll();
    }

    private void Update()
    {
        RaycastHit info;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        
        
        // Checks if the raycast hits something
        if (Physics.Raycast(ray, out info, 100, LayerMask.GetMask("BoardTile", "Mouse"))) 
        {
            // Gets indices of hit tiles
            Vector2Int hitPosition = CheckTile(info.transform.gameObject);

            // This value is used when the mouse wasn't over anything
            if (mousePos == -Vector2Int.one)
            {
                mousePos = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Mouse");
            }
            
            // This value is used when the mouse was over another tile
            if (mousePos != hitPosition)
            {
                tiles[hitPosition.x,mousePos.y].layer = LayerMask.NameToLayer("BoardTile");
                mousePos = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Mouse");
            }
            
            // When the mouse button is pressed down
            if (Input.GetMouseButtonDown(0))
            {
                if (boardPieces[hitPosition.x, hitPosition.y] != null)
                {
                    // Is it your turn?
                    if (true)
                    {
                        holding = boardPieces[hitPosition.x, hitPosition.y];
                    }
                }
            }
            
            // When the mouse button is released
            if (holding != null && Input.GetMouseButtonDown(0))
            {
                Vector2Int previousPosition = new Vector2Int(holding.currentX, holding.currentY);

                bool isValid = MoveTo(holding, hitPosition.x, hitPosition.y);

                // Moves the piece back if it cant be moved where you want
                if (!isValid)
                {
                    holding.transform.position = FindMiddle(previousPosition.x, previousPosition.y);
                    holding = null;
                }
            }
        }
        else
        {
            if (mousePos != -Vector2Int.one)
            {
                tiles[mousePos.x, mousePos.y].layer = LayerMask.NameToLayer("BoardTile");
                mousePos = -Vector2Int.one;
            }
        }
    }

    private void GenerateGrid(float tileSize, int tileCountX, int tileCountY)
    {
        yOffset += transform.position.y;
        border = new Vector3((tileCountX / 2) * tileSize, 0, (tileCountX / 2) * tileSize) + boardCenter;
            
        tiles = new GameObject[tileCountX, tileCountY];
        for (int x = 0; x < tileCountX; x++)
        {
            for (int y = 0; y < tileCountY; y++)
            {
                tiles[x, y] = SpawnSingleTile(tileSize, x, y);
            }
        }
    }

    private GameObject SpawnSingleTile(float tileSize, int x, int y)
    {
        GameObject tileObject = new GameObject(string.Format("X:{0}, Y:{1}", x, y));
        // Allows tiles to move when moving above GameObjects
        tileObject.transform.parent = transform; 

        Mesh mesh = new Mesh();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        
        // Uses material declared in header
        tileObject.AddComponent<MeshRenderer>().material = materialTexture;

        //generates the 4 corners of the board
        Vector3[] vertices = new Vector3[4]; 
        vertices[0] = new Vector3(x * tileSize, yOffset, y * tileSize) - border;
        vertices[1] = new Vector3(x * tileSize, yOffset, (y+1) * tileSize) - border;
        vertices[2] = new Vector3((x+1) * tileSize, yOffset, y * tileSize) - border;
        vertices[3] = new Vector3((x+1) * tileSize, yOffset, (y+1) * tileSize) - border;

        // Generates 2 triangles that form the square board
        int[] triangles = new int[] {0, 1, 2, 1, 3, 2}; 

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        
        mesh.RecalculateNormals();

        tileObject.AddComponent<BoxCollider>();
        tileObject.layer = LayerMask.NameToLayer("BoardTile");
        
        return tileObject;
    }

    private Vector2Int CheckTile(GameObject raycastInfo)
    {
        for (int x = 0; x < tileCount_X; x++)
        {
            for (int y = 0; y < tileCount_Y; y++)
            {
                if (tiles[x,y] == raycastInfo)
                {
                    return new Vector2Int(x, y);
                }
            }
        }

        return -Vector2Int.one; 
    }

    private void GenerateAllSquares()
    {
        boardPieces = new BoardPieces[tileCount_X, tileCount_Y];

        int white = 0;
        int black = 1;

        // White Team
        boardPieces[11, 9] = GenerateSingleSquare(pieceType.Square, white);
        boardPieces[11, 8] = GenerateSingleSquare(pieceType.Square, white);
        boardPieces[11, 7] = GenerateSingleSquare(pieceType.Square, white);
        boardPieces[11, 6] = GenerateSingleSquare(pieceType.Square, white);
        boardPieces[11, 5] = GenerateSingleSquare(pieceType.Square, white);
        boardPieces[12, 9] = GenerateSingleSquare(pieceType.Square, white);
        boardPieces[12, 8] = GenerateSingleSquare(pieceType.Square, white);
        boardPieces[12, 7] = GenerateSingleSquare(pieceType.Square, white);
        boardPieces[12, 6] = GenerateSingleSquare(pieceType.Square, white);
        boardPieces[12, 5] = GenerateSingleSquare(pieceType.Square, white);
        
        // Black Team
        boardPieces[11, 4] = GenerateSingleSquare(pieceType.Square, black);
        boardPieces[11, 3] = GenerateSingleSquare(pieceType.Square, black);
        boardPieces[11, 2] = GenerateSingleSquare(pieceType.Square, black);
        boardPieces[11, 1] = GenerateSingleSquare(pieceType.Square, black);
        boardPieces[11, 0] = GenerateSingleSquare(pieceType.Square, black);
        boardPieces[12, 4] = GenerateSingleSquare(pieceType.Square, black);
        boardPieces[12, 3] = GenerateSingleSquare(pieceType.Square, black);
        boardPieces[12, 2] = GenerateSingleSquare(pieceType.Square, black);
        boardPieces[12, 1] = GenerateSingleSquare(pieceType.Square, black);
        boardPieces[12, 0] = GenerateSingleSquare(pieceType.Square, black);
        
    }

    private void PositionAll()
    {
        for (int x = 0; x < tileCount_X; x++)
        {
            for (int y = 0; y < tileCount_Y; y++)
            {
                if (boardPieces[x, y] != null)
                {
                    PositionSingle(x, y, false);
                }
            }
        }
    }

    private void PositionSingle(int x, int y, bool hasMoved)
    {
        boardPieces[x, y].currentX = x;
        boardPieces[x, y].currentY = y;
        boardPieces[x, y].transform.position = FindMiddle(x, y);

        if (hasMoved)
        {
            boardPieces[x, y].transform.position = new Vector3(
                boardPieces[x, y].transform.position.x,
                boardPieces[x, y].transform.position.y + 0.6f,
                boardPieces[x, y].transform.position.z
                );
        }
    }
    
    private BoardPieces GenerateSingleSquare(pieceType type, int team)
    {
        Vector3 targetPosition = new Vector3(transform.position.x, transform.position.y + 0.6f, transform.position.z);

        BoardPieces bp = Instantiate(prefabs[(int) type - 1], transform).GetComponent<BoardPieces>();
        bp.team = team;
        bp.type = type;
        bp.GetComponent<MeshRenderer>().material = teamMaterial[team];
        bp.transform.position = targetPosition;

        return bp;
    }

    private Vector3 FindMiddle(int x, int y)
    {
        return new Vector3(x * tileSize, yOffset, y * tileSize) - border + new Vector3(tileSize / 2, 0, tileSize / 2);
    }
    
    private bool MoveTo(BoardPieces bp, int x, int y)
    {
        Vector2Int previousPosition = new Vector2Int(bp.currentX, bp.currentY);
        boardPieces[x, y] = bp;
        boardPieces[previousPosition.x, previousPosition.y] = null;
        PositionSingle(x, y, true);

        return true;
    }
}
