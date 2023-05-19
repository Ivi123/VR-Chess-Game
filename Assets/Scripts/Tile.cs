using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    private bool isAvailableTile;
    public bool IsAvailableTile { get => isAvailableTile; set => isAvailableTile = value; }

    private Vector2Int position;
    public Vector2Int Position { get => position; set => position = value; }

    public Tile(int x, int y)
    {
        position = new Vector2Int(x, y);
    }

    public void TriggeredAvailableMove()
    {
        transform.parent.GetComponent<Chessboard>().HandleTileTrigger(position, true);
    }

    public void ExitedTriggeredAvailableMove()
    {
        transform.parent.GetComponent<Chessboard>().HandleTileTrigger(position, false);
    }
}
