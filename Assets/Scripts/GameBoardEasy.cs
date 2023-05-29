using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameBoardEasy : MonoBehaviour
{
    [Header("Art")]
    [SerializeField] private Material materialTexture;
    [SerializeField] private float tileSize = 1.0f;
    [SerializeField] private float yOffset = 0.2f;
    [SerializeField] private Vector3 boardCenter = Vector3.zero;

    [Header("Logic")]
    [SerializeField] private List<Rectangle> rectangles;
    [SerializeField] private GameObject rectangleObject;

    [Header("In-Game Display")]
    [SerializeField] private Material[] teamMaterial;
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private TextMeshProUGUI white;
    [SerializeField] private TextMeshProUGUI black;
    [SerializeField] private TextMeshProUGUI whiteScore;
    [SerializeField] private TextMeshProUGUI blackScore;

    [Header("Results Display")]
    [SerializeField] private GameObject resultsScreen;
    [SerializeField] private TextMeshProUGUI whiteResult;
    [SerializeField] private TextMeshProUGUI blackResult;

    // LOGIC
    private const int tileCount_X = 13;
    private const int tileCount_Y = 10;
    private GameObject[,] tiles;

    private Camera mainCamera;
    private Vector2Int mousePos;

    private Vector3 border;
    private BoardPieces[,] boardPieces;
    private BoardPieces holding;

    private const string WHITE = "Player 1";
    private const string BLACK = "Player 2";
    private const int WHITE_INT = 0;
    private const int BLACK_INT = 1;
    public string currentPlayer;
    private bool aiToPlay = false;

    private const float utilityCoefficientThreshold = 10f;

    private RaycastHit info;
    private Ray ray;

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
        ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        // Check who is currently in play
        if (currentPlayer == WHITE)
        {
            // Make it look like the AI is thinking
            StartCoroutine(ThinkAppearance());

            if (aiToPlay)
            {
                aiToPlay = false;

                // AI Logic
                int rnd = Random.Range(0, 10);
                if (rnd == 0)
                {
                    RandomMove();
                }
                else
                {
                    Minimax();
                }
            }
        }
        else
        {
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
                    tiles[hitPosition.x, mousePos.y].layer = LayerMask.NameToLayer("BoardTile");
                    mousePos = hitPosition;
                    tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Mouse");
                }

                // Select with left click
                if (Input.GetMouseButtonDown(0))
                {
                    if (boardPieces[hitPosition.x, hitPosition.y] != null &&
                       ((currentPlayer.Equals(BLACK) && boardPieces[hitPosition.x, hitPosition.y].team == 1) ||
                        (currentPlayer.Equals(WHITE) && boardPieces[hitPosition.x, hitPosition.y].team == 0)))
                    {
                        holding = boardPieces[hitPosition.x, hitPosition.y];
                    }
                }

                // Pick up piece after left click release
                if (holding != null && Input.GetMouseButtonDown(0))
                {
                    Vector2Int previousPosition = new Vector2Int(holding.currentX, holding.currentY);

                    bool isValid = MoveTo(holding, hitPosition.x, hitPosition.y);

                    // Moves the piece back if it can't be moved where you want
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
        vertices[1] = new Vector3(x * tileSize, yOffset, (y + 1) * tileSize) - border;
        vertices[2] = new Vector3((x + 1) * tileSize, yOffset, y * tileSize) - border;
        vertices[3] = new Vector3((x + 1) * tileSize, yOffset, (y + 1) * tileSize) - border;

        // Generates 2 triangles that form the square board
        int[] triangles = new int[] { 0, 1, 2, 1, 3, 2 };

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

        int black = 1;
        int white = 0;

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
                if (tiles[x, y] == raycastInfo)
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
            UpdateCurrentPlayer();
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
            if (i != y &&
                boardPieces[x, i] != null &&
                boardPieces[x, i].team == startCorner.team)
            {
                firstFoundCorner = boardPieces[x, i];

                // Checks horizontally for a piece in line with itself
                for (int j = 0; j < 10; j++)
                {
                    // Checks horizontally in line with the starting corner
                    if (boardPieces[x, y] != boardPieces[j, y] &&
                        boardPieces[j, y] != null &&
                        boardPieces[j, y].team == startCorner.team)
                    {
                        secondFoundCorner = boardPieces[j, y];
                    }

                    // Check horizontally in line with the first valid corner found
                    if (boardPieces[x, i] != boardPieces[j, i] &&
                        boardPieces[j, i] != null &&
                        boardPieces[j, i].team == startCorner.team)
                    {
                        thirdFoundCorner = boardPieces[j, i];
                    }

                    // Final check to see that corners line up
                    if (secondFoundCorner.currentX != -1 && secondFoundCorner.currentX == thirdFoundCorner.currentX)
                    {
                        AddRectangleToList(startCorner, firstFoundCorner, secondFoundCorner, thirdFoundCorner);
                    }
                }
            }
        }

        CheckForBlocks();

        if (CheckForGameEnd())
        {
            EndGame();
        }
    }

    private void AddRectangleToList(BoardPieces startCorner, BoardPieces firstFoundCorner, 
                                    BoardPieces secondFoundCorner, BoardPieces thirdFoundCorner)
    {
        // Calculate rectangle's score
        float rectangleScore =
            Mathf.Abs(startCorner.currentY - firstFoundCorner.currentY) *
            Mathf.Abs(startCorner.currentX - secondFoundCorner.currentX);

        // Create new rectangle based on calculated parameters
        Rectangle newRect = rectangleObject.AddComponent<Rectangle>();
        newRect.Create(
                new Vector2(startCorner.currentX, startCorner.currentY),
                new Vector2(firstFoundCorner.currentX, firstFoundCorner.currentY),
                new Vector2(secondFoundCorner.currentX, secondFoundCorner.currentY),
                new Vector2(thirdFoundCorner.currentX, thirdFoundCorner.currentY),
                rectangleScore,
                startCorner.team
                );

        // Create new rectangle object with discovered coordinates
        Rectangle[] storage = rectangleObject.GetComponents<Rectangle>();

        // Check that the rectangle hasn't been found before
        if (!CheckForDuplicate(newRect))
        {
            rectangles.Add(storage[storage.Length - 1]);

            // Update in-game score displays
            if (startCorner.team == 1)
            {
                blackScore.text += "\n" + rectangleScore;
            }
            else if (startCorner.team == 0)
            {
                whiteScore.text += "\n" + rectangleScore;
            }
        }
    }

    private void CheckForBlocks()
    {
        float min;
        float max;
        float y;
        float x;

        // DEBUG REFERENCE
        // 1 -> 2 X   3 -> 4 X  1 -> 3 Y  2 -> 4 Y
        for (int i = 0; i < rectangles.Count; i++)
        {
            max = Mathf.Max(rectangles[i].corner1.y, rectangles[i].corner2.y);
            min = Mathf.Min(rectangles[i].corner1.y, rectangles[i].corner2.y);
            y = rectangles[i].corner1.y;
            if (CheckForEdgeBlock(rectangles[i], 0f, y, min, max, true))
            {
                rectangles[i].isBlocked = true;
                ConvertToBlockedScore(rectangles[i]);
                break;
            }

            max = Mathf.Max(rectangles[i].corner3.y, rectangles[i].corner4.y);
            min = Mathf.Min(rectangles[i].corner3.y, rectangles[i].corner4.y);
            y = rectangles[i].corner3.y;
            if (CheckForEdgeBlock(rectangles[i], 0f, y, min, max, true))
            {
                rectangles[i].isBlocked = true;
                ConvertToBlockedScore(rectangles[i]);
                break;
            }

            max = Mathf.Max(rectangles[i].corner1.x, rectangles[i].corner3.x);
            min = Mathf.Min(rectangles[i].corner1.x, rectangles[i].corner3.x);
            x = rectangles[i].corner1.x;
            if (CheckForEdgeBlock(rectangles[i], x, 0f, min, max, false))
            {
                rectangles[i].isBlocked = true;
                ConvertToBlockedScore(rectangles[i]);
                break;
            }

            max = Mathf.Max(rectangles[i].corner2.x, rectangles[i].corner4.x);
            min = Mathf.Min(rectangles[i].corner2.x, rectangles[i].corner4.x);
            x = rectangles[i].corner2.x;
            if (CheckForEdgeBlock(rectangles[i], x, 0f, min, max, false))
            {
                rectangles[i].isBlocked = true;
                ConvertToBlockedScore(rectangles[i]);
                break;
            }
        }
    }

    private bool CheckForEdgeBlock(Rectangle rect, float x, float y, float min, float max, bool isHorizontal)
    {
        BoardPieces piece;

        for (float j = min + 1; j < max; j++)
        {
            if (isHorizontal)
            {
                piece = boardPieces[(int)y, (int)j];
                if (piece != null && piece.team != rect.team)
                {
                    return true;
                }
            }
            else
            {
                piece = boardPieces[(int)j, (int)x];
                if (piece != null && piece.team != rect.team)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void ConvertToBlockedScore(Rectangle rect)
    {
        float newScore = rect.score / 4f;
        newScore = Mathf.Round(newScore);
        rect.score = newScore;
    }

    private bool CheckForDuplicate(Rectangle rect)
    {
        for (int i = 0; i < rectangles.Count; i++)
        {
            if (Equals(rect, rectangles[i]))
            {
                return true;
            }
        }
        return false;
    }

    private bool Equals(Rectangle rect1, Rectangle rect2)
    {
        if (rect1.corner1 == rect2.corner1 &&
            rect1.corner2 == rect2.corner2 &&
            rect1.corner3 == rect2.corner3 &&
            rect1.corner4 == rect2.corner4)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void UpdateCurrentPlayer()
    {
        if (currentPlayer.Equals(BLACK))
        {
            currentPlayer = WHITE;
            white.gameObject.SetActive(true);
            black.gameObject.SetActive(false);
        }
        else if (currentPlayer.Equals(WHITE))
        {
            currentPlayer = BLACK;
            white.gameObject.SetActive(false);
            black.gameObject.SetActive(true);
        }

        aiToPlay = false;
    }

    private bool CheckForGameEnd()
    {
        for (int i = 11; i < 13; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                if (boardPieces[i, j] != null)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private void EndGame()
    {
        resultsScreen.SetActive(true);

        for (int i = 0; i < rectangles.Count; i++)
        {
            if (rectangles[i].team == 1)
            {
                whiteResult.text += rectangles[i].score + "\n";
            }
        }
        whiteResult.text += "\n\nFinal Score:\n" + CalculateFinalScore(false);

        for (int i = 0; i < rectangles.Count; i++)
        {
            if (rectangles[i].team == 0)
            {
                blackResult.text += rectangles[i].score + "\n";
            }
        }
        blackResult.text += "\n\nFinal Score:\n" + CalculateFinalScore(true);

        Time.timeScale = 0f;
    }

    private float CalculateFinalScore(bool isBlack)
    {
        float returnScore = 0f;

        for (int i = 0; i < rectangles.Count; i++)
        {
            if ((rectangles[i].team == 1 && isBlack) || (rectangles[i].team == 0 && !isBlack))
            {
                returnScore += rectangles[i].score;
            }
        }

        return returnScore;
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

    #region AI Logic
    private IEnumerator ThinkAppearance()
    {
        yield return new WaitForSeconds(1f);
        aiToPlay = true;
    }

    private void RandomMove()
    {
        bool hasPicked = false;
        int x1 = 0;
        int x2 = 10;
        int y1 = 0;
        int y2 = 10;

        while(!hasPicked)
        {
            int rndY = Random.Range(y1, y2);
            int rndX = Random.Range(x1, x2);

            if (boardPieces[rndX, rndY] == null)
            {
                hasPicked = true;

                // Check if the AI has any available pieces
                BoardPieces bp = AICheckForAvailablePiece();
                if (bp != null)
                {
                    MoveTo(bp, rndX, rndY);
                }
                else
                {
                    EndGame();
                }
            }
        }
    }

    private void Minimax()
    {
        // Check if the AI has any pieces left
        BoardPieces bp = AICheckForAvailablePiece();
        if (bp == null)
        {
            EndGame();
            return;
        }

        // Check if the AI can complete a rectangle of its own
        if (AICompleteRectangle(bp, true))
        {
            return;
        }

        // Check if the AI can sabotage a player's rectangle
        if (AICompleteRectangle(bp, false))
        {
            return;
        }

        // Check if the AI can block a pre-existing rectangle
        if (AIBlockRectangle(bp))
        {
            return;
        }

        // Play a random move OR Attempt to draw a rectangle
        AILastResort(bp);
    }

    private int[] AICheckForRectangle(int x, int y, bool isCreating)
    {
        BoardPieces startCorner = boardPieces[x, y];
        int[] returnCoords = new int[2];

        int teamToCheck;
        if (isCreating)
        {
            teamToCheck = WHITE_INT;
        }
        else
        {
            teamToCheck = BLACK_INT;
        }

        if (startCorner == null)
        {
            returnCoords = null;
            return null;
        }
        else if (startCorner.team != teamToCheck)
        {
            returnCoords = null;
            return null;
        }

        BoardPieces firstFoundCorner = new BoardPieces();
        BoardPieces secondFoundCorner = new BoardPieces();
        BoardPieces thirdFoundCorner = new BoardPieces();

        for (int i = 0; i < 10; i++)
        {
            // Checks vertically for a piece in line with itself
            if (i != y &&
                boardPieces[x, i] != null &&
                boardPieces[x, i].team == teamToCheck)
            {
                firstFoundCorner = boardPieces[x, i];

                // Checks horizontally for a piece in line with itself
                for (int j = 0; j < 10; j++)
                {
                    // Checks horizontally in line with the starting corner
                    if (boardPieces[x, y] != boardPieces[j, y] &&
                        boardPieces[j, y] != null &&
                        boardPieces[j, y].team == teamToCheck)
                    {
                        secondFoundCorner = boardPieces[j, y];
                        thirdFoundCorner = boardPieces[j, i];

                        if (thirdFoundCorner == null)
                        {
                            returnCoords[0] = j;
                            returnCoords[1] = i;
                            return returnCoords;
                        }
                    }

                    // Check horizontally in line with the first valid corner found
                    if (boardPieces[x, i] != boardPieces[j, i] &&
                        boardPieces[j, i] != null &&
                        boardPieces[j, i].team == teamToCheck)
                    {
                        thirdFoundCorner = boardPieces[j, i];
                        secondFoundCorner = boardPieces[j, i];

                        if (secondFoundCorner == null)
                        {
                            returnCoords[0] = j;
                            returnCoords[1] = i;
                            return returnCoords;
                        }
                    }
                }
            }
        }

        returnCoords[0] = -1;
        returnCoords[1] = -1;
        return returnCoords;
    }

    private bool AICheckForTwoPoints(BoardPieces bp, int x, int y, float utilityCoefficient)
    {
        bool success = false;

        BoardPieces startPoint = boardPieces[x, y];
        BoardPieces endPoint = new BoardPieces();

        int[] returnCoords = new int[2];
        int idealLength;

        for (int i = 0; i < 10; i++)
        {
            if (i != y &&
                boardPieces[x, i] != null &&
                boardPieces[x, i].team == WHITE_INT)
            {
                endPoint = boardPieces[x, i];

                idealLength = (int)Mathf.Round(utilityCoefficient);
                idealLength = Mathf.Clamp(idealLength, 1, 9);

                int rnd = Random.Range(0, 2);
                if (rnd == 0)
                {
                    if (idealLength > startPoint.currentX)
                    {
                        for (int j = idealLength; j >= startPoint.currentX; j--)
                        {
                            if (boardPieces[j, startPoint.currentY] == null)
                            {
                                MoveTo(bp, j, startPoint.currentY);
                                success = true;
                                return success;
                            }
                        }
                    }
                    else if (idealLength < startPoint.currentX)
                    {
                        for (int j = startPoint.currentX; j >= idealLength; j--)
                        {
                            if (boardPieces[j, startPoint.currentY] == null)
                            {
                                MoveTo(bp, j, startPoint.currentY);
                                success = true;
                                return success;
                            }
                        }
                    }
                }
                else
                {
                    if (idealLength > endPoint.currentX)
                    {
                        for (int j = idealLength; j >= endPoint.currentX; j--)
                        {
                            if (boardPieces[j, endPoint.currentY] == null)
                            {
                                MoveTo(bp, j, endPoint.currentY);
                                success = true;
                                return success;
                            }
                        }
                    }
                    else if (idealLength < endPoint.currentX)
                    {
                        for (int j = endPoint.currentX; j >= idealLength; j--)
                        {
                            if (boardPieces[j, endPoint.currentY] == null)
                            {
                                MoveTo(bp, j, endPoint.currentY);
                                success = true;
                                return success;
                            }
                        }
                    }
                }
            }
        }

        return success;
    }

    private bool AICheckForPoint(BoardPieces bp, int x, int y, float utilityCoefficient)
    {
        bool success = false;

        BoardPieces startPoint = boardPieces[x, y];

        int idealLength = (int)Mathf.Round(utilityCoefficient);
        idealLength = Mathf.Clamp(idealLength, 1, 9);

        int rnd = Random.Range(0, 2);
        if (rnd == 0) {
            if (idealLength > x)
            {
                for (int i = idealLength; i >= x; i--)
                {
                    if (boardPieces[i, startPoint.currentY] == null)
                    {
                        MoveTo(bp, i, startPoint.currentY);
                        success = true;
                        return success;
                    }
                }
            }
            else if (idealLength < x)
            {
                for (int i = x; i >= idealLength; i--)
                {
                    if (boardPieces[i, startPoint.currentY] == null)
                    {
                        MoveTo(bp, i, startPoint.currentY);
                        success = true;
                        return success;
                    }
                }
            }
        }
        else
        {
            if (idealLength > y)
            {
                for (int i = idealLength; i >= y; i--)
                {
                    if (boardPieces[startPoint.currentX, i] == null)
                    {
                        MoveTo(bp, startPoint.currentX, i);
                        success = true;
                        return success;
                    }
                }
            }
            else if (idealLength < y)
            {
                for (int i = y; i >= idealLength; i--)
                {
                    if (boardPieces[startPoint.currentX, i] == null)
                    {
                        MoveTo(bp, startPoint.currentX, i);
                        success = true;
                        return success;
                    }
                }
            }
        }

        return success;
    }

    private bool AICompleteRectangle(BoardPieces bp, bool isCreating)
    {
        bool success = false;

        int[] coords;
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                coords = AICheckForRectangle(i, j, isCreating);

                if (coords != null && coords[0] > -1)
                {
                    // SUCCESS
                    MoveTo(bp, coords[0], coords[1]);
                    success = true;
                    return success;
                }
            }
        }

        return success;
    }

    private bool AIBlockRectangle(BoardPieces bp)
    {
        // DEBUG: Rectangle corner reference
        //   1   ->   3
        //
        //   |        |
        //   V        V
        //
        //   2   ->   4

        bool success = false;

        Vector2 corner1;
        Vector2 corner2;
        Vector2 corner3;
        Vector2 corner4;

        float firstRangeMin;
        float firstRangeMax;
        float secondRangeMin;
        float secondRangeMax;
        float thirdRangeMin;
        float thirdRangeMax;
        float fourthRangeMin;
        float fourthRangeMax;

        int rndLine;
        float rndPoint1;
        float rndPoint2;
        float[] coords = { - 1, -1};

        bool pointFound = false;

        for (int i = 0; i < rectangles.Count; i++)
        {
            if (rectangles[i].team == BLACK_INT)
            {
                corner1 = rectangles[i].corner1;
                corner2 = rectangles[i].corner2;
                corner3 = rectangles[i].corner3;
                corner4 = rectangles[i].corner4;

                // Find viable ranges on the rectangle's perimeter
                firstRangeMin = System.Math.Min(corner1.y, corner2.y);
                firstRangeMax = System.Math.Max(corner1.y, corner2.y);

                secondRangeMin = System.Math.Min(corner3.y, corner4.y);
                secondRangeMax = System.Math.Max(corner3.y, corner4.y);

                thirdRangeMin = System.Math.Min(corner1.x, corner3.x);
                thirdRangeMax = System.Math.Max(corner1.x, corner3.x);

                fourthRangeMin = System.Math.Min(corner2.x, corner4.x);
                fourthRangeMax = System.Math.Max(corner2.x, corner4.x);

                // Ensures while loop does not persist
                int maxLoops = 30;
                int loopCount = 0;
                // Select a random point
                while (!pointFound || loopCount > maxLoops)
                {
                    loopCount++;

                    rndLine = Random.Range(0, 4);
                    if (rndLine == 0)
                    {
                        rndPoint1 = Random.Range(firstRangeMin + 1, firstRangeMax);
                        rndPoint2 = corner1.x;
                        coords[0] = rndPoint2;
                        coords[1] = rndPoint1;
                    }
                    else if (rndLine == 1)
                    {
                        rndPoint1 = Random.Range(secondRangeMin + 1, secondRangeMax);
                        rndPoint2 = corner3.x;
                        coords[0] = rndPoint2;
                        coords[1] = rndPoint1;
                    }
                    else if (rndLine == 2)
                    {
                        rndPoint1 = Random.Range(thirdRangeMin + 1, thirdRangeMax);
                        rndPoint2 = corner1.y;
                        coords[0] = rndPoint1;
                        coords[1] = rndPoint2;
                    }
                    else
                    {
                        rndPoint1 = Random.Range(fourthRangeMin + 1, fourthRangeMax);
                        rndPoint2 = corner2.y;
                        coords[0] = rndPoint1;
                        coords[1] = rndPoint2;
                    }

                    if (boardPieces[(int)coords[0], (int)coords[1]] == null)
                    {
                        pointFound = true;
                    }
                    else
                    {
                        coords[0] = -1;
                        coords[1] = -1;
                    }
                }
            }

            if (pointFound)
            {
                break;
            }
        }

        if (coords[0] > -1 && boardPieces[(int)coords[0], (int)coords[1]] == null)
        {
            MoveTo(bp, (int)coords[0], (int)coords[1]);
            success = true;
            return success;
        }

        return success;
    }

    private void AILastResort(BoardPieces bp)
    {
        // Utility function
        float utilityCoefficient = (CalculateFinalScore(true) + 1) / (CalculateFinalScore(false) + 1);

        if (utilityCoefficient > utilityCoefficientThreshold)
        {
            RandomMove();
        }
        else
        {
            AITryDrawRectangle(bp, utilityCoefficient);
        }
    }

    private void AITryDrawRectangle(BoardPieces bp, float utilityCoefficient)
    {
        // Check for various configurations of potential rectangles
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                if (boardPieces[i, j] != null && boardPieces[i, j].team == WHITE_INT)
                {
                    if (AICheckForTwoPoints(bp, i, j, utilityCoefficient))
                    {
                        return;
                    }
                }
            }
        }

        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                if (boardPieces[i, j] != null && boardPieces[i, j].team == WHITE_INT)
                {
                    if (AICheckForPoint(bp, i, j, utilityCoefficient))
                    {
                        return;
                    }
                }
            }
        }

        // If no appropriate moves are found, play a random move
        RandomMove();
    }

    private BoardPieces AICheckForAvailablePiece()
    {
        for (int i = 11; i <= 12; i++)
        {
            for (int j = 9; j >= 5; j--)
            {
                if (boardPieces[i, j] != null)
                {
                    return boardPieces[i, j];
                }
            }
        }

        return null;
    }
    #endregion
}
