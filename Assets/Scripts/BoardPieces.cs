using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum pieceType
{
    None = 0,
    Square = 1
}

public class BoardPieces : MonoBehaviour
{
    public pieceType type;
    public int team;
    public int currentX;
    public int currentY;

    private Vector3 desiredPosition;
}
