using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameBoard : MonoBehaviour
{
    [Header("Art")] 
    [SerializeField] private Material materialTexture;
    [SerializeField] private float tileSize = 1.0f;
    [SerializeField] private float yOffset = 0.2f;
    [SerializeField] private Vector3 boardCenter = Vector3.zero;

    [Header("Logic")]
    [SerializeField] private List<Rectangle> rectangles;

    [Header("Display")]
    [SerializeField] private Material[] teamMaterial;
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private TextMeshProUGUI white;
    [SerializeField] private TextMeshProUGUI black;
    [SerializeField] private TextMeshProUGUI whiteScore;
    [SerializeField] private TextMeshProUGUI blackScore;

    // LOGIC
    private const int tileCount_X = 13;
    private const int tileCount_Y = 10;
    private GameObject[,] tiles;

    private Camera mainCamera;
    private Vector2Int mousePos;

    private Vector3 border;
    private BoardPieces[,] boardPieces;
    private BoardPieces holding;

    private const string BLACK = "Player 1";
    private const string WHITE = "Player 2";
    public string currentPlayer;

    private void Awake()
    {
        mainCamera = Camera.main;

        GenerateGrid(1, tileCount_X, tileCount_Y);
        
        GenerateAllSquares();
        PositionAll();

        currentPlayer = BLACK;
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
                    holding = boardPieces[hitPosition.x, hitPosition.y];

                    // Is it your turn?
                    //if ((currentPlayer.Equals(BLACK) && boardPieces[hitPosition.x, hitPosition.y].team == 1) ||
                    //    (currentPlayer.Equals(WHITE) && boardPieces[hitPosition.x, hitPosition.y].team == 0))
                    //{
                    //    holding = boardPieces[hitPosition.x, hitPosition.y];
                    //}
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

    #region Startup
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

    private BoardPieces GenerateSingleSquare(pieceType type, int team)
    {
        Vector3 targetPosition = new Vector3(transform.position.x, transform.position.y + 0.6f, transform.position.z);

        BoardPieces bp = Instantiate(prefabs[(int)type - 1], transform).GetComponent<BoardPieces>();
        bp.team = team;
        bp.type = type;
        bp.GetComponent<MeshRenderer>().material = teamMaterial[team];
        bp.transform.position = targetPosition;

        return bp;
    }
    #endregion

    #region Gameplay
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

        // Catch statement
        return -Vector2Int.one; 
    }

    private void PositionSingle(int x, int y, bool hasMoved)
    {
        if (boardPieces[x, y] == null)
        {
            return;
        }

        boardPieces[x, y].currentX = x;
        boardPieces[x, y].currentY = y;
        boardPieces[x, y].transform.position = FindMiddle(x, y);

        // Player has moved a piece
        if (hasMoved)
        {
            Vector3 previousPosition = boardPieces[x, y].transform.position;
            previousPosition.y += 0.6f;

            boardPieces[x, y].transform.position = previousPosition;

            CheckForRectangle(x, y);
            UpdateUI();
        }
    }

    private void CheckForRectangle(int x, int y)
    {
        BoardPieces startCorner = boardPieces[x, y];
        BoardPieces firstFoundCorner = new BoardPieces();
        BoardPieces secondFoundCorner = new BoardPieces();
        BoardPieces thirdFoundCorner = new BoardPieces();

        for (int i = 0; i < 10; i++)
        {
            // Checks vertically for a piece in line with itself
            if (i != y && boardPieces[x, i] != null && CheckTeam(x, i))
            {
                firstFoundCorner = boardPieces[x, i];

                // Checks horizontally for a piece in line with itself
                for (int j = 0; j < 10; j++)
                {
                    if (boardPieces[x, y] != boardPieces[j, y] && boardPieces[j, y] != null && CheckTeam(j, y))
                    {
                        secondFoundCorner = boardPieces[j, y];
                    }
                }

                for (int k = 0; k < 10; k++)
                {
                    if (boardPieces[x, i] != boardPieces[k, i] && boardPieces[k, i] != null && CheckTeam(k, y))
                    {
                        thirdFoundCorner = boardPieces[k, i];
                    }
                }
            }
        }

        // Final check to see that corners line up
        if (secondFoundCorner.currentX != -1 && secondFoundCorner.currentX == thirdFoundCorner.currentX)
        {
            // Calculate rectangle's score
            float rectangleScore =
                Mathf.Abs(startCorner.currentY - firstFoundCorner.currentY) *
                Mathf.Abs(startCorner.currentX - secondFoundCorner.currentX);

            Rectangle created = new Rectangle(
                    new Vector2(startCorner.currentX, startCorner.currentY),
                    new Vector2(firstFoundCorner.currentX, firstFoundCorner.currentY),
                    new Vector2(secondFoundCorner.currentX, secondFoundCorner.currentY),
                    new Vector2(thirdFoundCorner.currentX, thirdFoundCorner.currentY),
                    rectangleScore,
                    startCorner.team
                    );

            // Create new rectangle object with discovered coordinates
            rectangles.Add(created);

            if (startCorner.team == 1)
            {
                blackScore.text += "\n" + rectangleScore;
            }
            else if (startCorner.team == 0)
            {
                whiteScore.text += "\n" + rectangleScore;
            }
        }

        // DEBUG CODE
        //Debug.Log(startCorner.currentX + "; " + startCorner.currentY + "\n" +
        //        firstFoundCorner.currentX + "; " + firstFoundCorner.currentY + "\n" +
        //        secondFoundCorner.currentX + "; " + secondFoundCorner.currentY + "\n" +
        //        thirdFoundCorner.currentX + "; " + thirdFoundCorner.currentY);
    }

    private bool CheckTeam(int x, int y)
    {
        return (boardPieces[x, y].team == 0 && currentPlayer.Equals("Player 1")) ||
                   (boardPieces[x, y].team == 1 && currentPlayer.Equals("Player 2"));
    }

    private void UpdateUI()
    {
        if (currentPlayer.Equals("Player 1"))
        {
            currentPlayer = WHITE;
            white.gameObject.SetActive(true);
            black.gameObject.SetActive(false);
        }
        else
        {
            currentPlayer = BLACK;
            white.gameObject.SetActive(false);
            black.gameObject.SetActive(true);
        }
    }
    #endregion

    #region Movement
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
    #endregion
}
