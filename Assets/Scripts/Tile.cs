using UnityEngine;

/// <summary>
/// Defines the <see cref="Tile" />.
/// </summary>
public class Tile : MonoBehaviour
{
    /// <summary>
    /// Defines the isWhiteTile.
    /// </summary>
    [SerializeField] private bool isWhiteTile;

    /// <summary>
    /// Gets or sets a value indicating whether IsWhiteTile.
    /// </summary>
    public bool IsWhiteTile { get => isWhiteTile; set => isWhiteTile = value; }

    /// <summary>
    /// Defines the isAvailableTile.
    /// </summary>
    private bool isAvailableTile;

    /// <summary>
    /// Gets or sets a value indicating whether IsAvailableTile.
    /// </summary>
    public bool IsAvailableTile { get => isAvailableTile; set => isAvailableTile = value; }

    /// <summary>
    /// Defines the position.
    /// </summary>
    private Vector2Int position;

    /// <summary>
    /// Gets or sets the Position.
    /// </summary>
    public Vector2Int Position { get => position; set => position = value; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Tile"/> class.
    /// </summary>
    /// <param name="x">The x<see cref="int"/>.</param>
    /// <param name="y">The y<see cref="int"/>.</param>
    public Tile(int x, int y)
    {
        position = new Vector2Int(x, y);
    }

    /// <summary>
    /// The TriggeredAvailableMove.
    /// </summary>
    public void TriggeredAvailableMove()
    {
        transform.parent.GetComponent<Chessboard>().HandleTileTrigger(position, true);
    }

    /// <summary>
    /// The ExitedTriggeredAvailableMove.
    /// </summary>
    public void ExitedTriggeredAvailableMove()
    {
        transform.parent.GetComponent<Chessboard>().HandleTileTrigger(position, false);
    }
}
