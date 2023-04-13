using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rectangle : MonoBehaviour
{
    public Vector2 corner1 = new Vector2(-1, -1);
    public Vector2 corner2 = new Vector2(-1, -1);
    public Vector2 corner3 = new Vector2(-1, -1);
    public Vector2 corner4 = new Vector2(-1, -1);

    public bool isBlocked = false;

    public float score = 0;

    int team = -1;

    public Rectangle(Vector2 corner1, Vector2 corner2, Vector2 corner3, Vector2 corner4, float score, int team)
    {
        this.corner1 = corner1;
        this.corner2 = corner2;
        this.corner3 = corner3;
        this.corner4 = corner4;

        this.score = score;
        this.team = team;
    }
}
