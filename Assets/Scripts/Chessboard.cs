using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Defines the <see cref="Chessboard" />.
/// </summary>
public class Chessboard : MonoBehaviour
{
    [Header("Art stuff")]
    private float tileSize = 0.120f;
    public float yOffset;
    public Vector3 boardCenter;

    [Header("Prefabs & Materials")]
    public GameObject[] prefabs;
    public Material[] teamMaterials;
    public Material[] tilesMaterials;

    //Logic
    protected ChessPiece[,] chessPieces;
    private const int TILE_COUNT_X = 8;
    private const int TILE_COUNT_Y = 8;
    private GameObject[,] tiles;
    public Vector3 bounds;
    private Moves moves;

    /// <summary>
    /// The Awake.
    /// </summary>
    private void Awake()
    {
        boardCenter = new Vector3(transform.position.x * -1, 0, transform.position.z * -1);
        GenerateAllTiles(tileSize, TILE_COUNT_X, TILE_COUNT_Y);

        SpawnAllPieces();
        PositionAllPieces();
    }

    /// <summary>
    /// The Update.
    /// </summary>
    private void Update()
    {
    }

    /// <summary>
    /// The GenerateAllTiles.
    /// </summary>
    /// <param name="tileSize">The tileSize<see cref="float"/>.</param>
    /// <param name="tileCountX">The tileCountX<see cref="int"/>.</param>
    /// <param name="tileCountY">The tileCountY<see cref="int"/>.</param>
    private void GenerateAllTiles(float tileSize, int tileCountX, int tileCountY)
    {
        yOffset += transform.position.y;
        bounds = new Vector3((tileCountX / 2) * tileSize, 0, (tileCountX / 2) * tileSize) + boardCenter;

        tiles = new GameObject[tileCountX, tileCountY];
        //To easily determine the type of the generated tile we add the bool isTileTypeWhite and set it to true.
        // For each x iteration we negate the type as each new row starts with what the previous row ended
        // For each y iteration we negate the type as the adjacent tile should be of a different type.
        bool isTileTypeWhite = true;
        for (int x = 0; x < tileCountX; x++)
        {
            isTileTypeWhite = !isTileTypeWhite;
            for (int y = 0; y < tileCountY; y++)
            {
                tiles[x, y] = GenerateSingleTile(tileSize, x, y, isTileTypeWhite);
                isTileTypeWhite = !isTileTypeWhite;
            }
        }
    }

    /// <summary>
    /// The GenerateSingleTile.
    /// </summary>
    /// <param name="tileSize">The tileSize<see cref="float"/>.</param>
    /// <param name="x">The x<see cref="int"/>.</param>
    /// <param name="y">The y<see cref="int"/>.</param>
    /// <param name="isTileWhite">The isTileWhite<see cref="bool"/>.</param>
    /// <returns>The <see cref="GameObject"/>.</returns>
    private GameObject GenerateSingleTile(float tileSize, int x, int y, bool isTileWhite)
    {

        GameObject tileObject = new GameObject(string.Format("Tile: X:{0}, Y:{1}", x, y));
        tileObject.transform.parent = transform;

        tileObject.AddComponent<Tile>();
        tileObject.GetComponent<Tile>().Position = new Vector2Int(x, y);
        tileObject.GetComponent<Tile>().IsWhiteTile = isTileWhite;

        Mesh mesh = new Mesh();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        tileObject.AddComponent<MeshRenderer>().material = tilesMaterials[((int)Shared.TileType.Default)];

        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x * tileSize, yOffset, y * tileSize) - bounds;
        vertices[1] = new Vector3(x * tileSize, yOffset, (y + 1) * tileSize) - bounds;
        vertices[2] = new Vector3((x + 1) * tileSize, yOffset, y * tileSize) - bounds;
        vertices[3] = new Vector3((x + 1) * tileSize, yOffset, (y + 1) * tileSize) - bounds;

        int[] tris = new int[] { 0, 1, 2, 1, 3, 2 };

        mesh.vertices = vertices;
        mesh.triangles = tris;

        mesh.RecalculateNormals();

        tileObject.layer = LayerMask.NameToLayer("Tile");
        tileObject.AddComponent<BoxCollider>();
        tileObject.GetComponent<BoxCollider>().isTrigger = true;

        return tileObject;
    }

    /// <summary>
    /// The SpawnAllPieces.
    /// </summary>
    private void SpawnAllPieces()
    {
        chessPieces = new ChessPiece[TILE_COUNT_X, TILE_COUNT_Y];

        // white team
        chessPieces[0, 0] = SpawnSinglePiece(ChessPieceType.Rook, Shared.TeamType.White);
        chessPieces[0, 1] = SpawnSinglePiece(ChessPieceType.Knight, Shared.TeamType.White);
        chessPieces[0, 2] = SpawnSinglePiece(ChessPieceType.Bishop, Shared.TeamType.White);
        chessPieces[0, 3] = SpawnSinglePiece(ChessPieceType.King, Shared.TeamType.White);
        chessPieces[0, 4] = SpawnSinglePiece(ChessPieceType.Queen, Shared.TeamType.White);
        chessPieces[0, 5] = SpawnSinglePiece(ChessPieceType.Bishop, Shared.TeamType.White);
        chessPieces[0, 6] = SpawnSinglePiece(ChessPieceType.Knight, Shared.TeamType.White);
        chessPieces[0, 7] = SpawnSinglePiece(ChessPieceType.Rook, Shared.TeamType.White);
        for (int i = 0; i < TILE_COUNT_X; i++)
        {
            chessPieces[1, i] = SpawnSinglePiece(ChessPieceType.Pawn, Shared.TeamType.White);
        }

        //black team
        chessPieces[7, 0] = SpawnSinglePiece(ChessPieceType.Rook, Shared.TeamType.Black);
        chessPieces[7, 1] = SpawnSinglePiece(ChessPieceType.Knight, Shared.TeamType.Black);
        chessPieces[7, 2] = SpawnSinglePiece(ChessPieceType.Bishop, Shared.TeamType.Black);
        chessPieces[7, 3] = SpawnSinglePiece(ChessPieceType.King, Shared.TeamType.Black);
        chessPieces[7, 4] = SpawnSinglePiece(ChessPieceType.Queen, Shared.TeamType.Black);
        chessPieces[7, 5] = SpawnSinglePiece(ChessPieceType.Bishop, Shared.TeamType.Black);
        chessPieces[7, 6] = SpawnSinglePiece(ChessPieceType.Knight, Shared.TeamType.Black);
        chessPieces[7, 7] = SpawnSinglePiece(ChessPieceType.Rook, Shared.TeamType.Black);
        for (int i = 0; i < TILE_COUNT_X; i++)
        {
            chessPieces[6, i] = SpawnSinglePiece(ChessPieceType.Pawn, Shared.TeamType.Black);
        }
    }

    /// <summary>
    /// The SpawnSinglePiece.
    /// </summary>
    /// <param name="type">The type<see cref="ChessPieceType"/>.</param>
    /// <param name="team">The team<see cref="Shared.TeamType"/>.</param>
    /// <returns>The <see cref="ChessPiece"/>.</returns>
    private ChessPiece SpawnSinglePiece(ChessPieceType type, Shared.TeamType team)
    {
        GameObject cpGameObject = Instantiate(prefabs[(int)type - 1], transform);
        AddTileDetector(cpGameObject);

        ChessPiece cp = cpGameObject.GetComponent<ChessPiece>();

        cp.type = type;
        cp.team = team;
        cp.GetComponent<MeshRenderer>().material = teamMaterials[(int)team];
        cp.Material = teamMaterials[(int)team];

        return cp;
    }

    /// <summary>
    /// The AddTileDetector.
    /// </summary>
    /// <param name="cpGameObject">The cpGameObject<see cref="GameObject"/>.</param>
    private void AddTileDetector(GameObject cpGameObject)
    {
        GameObject cpTileDetector = new GameObject(Shared.TileDetectorName);
        cpTileDetector.transform.parent = cpGameObject.transform;

        cpTileDetector.AddComponent<TileDetector>();
        cpTileDetector.GetComponent<TileDetector>().ChessPiece = cpGameObject.GetComponent<ChessPiece>();

        cpTileDetector.AddComponent<BoxCollider>();
        BoxCollider tileDetectorBoxColider = cpTileDetector.GetComponent<BoxCollider>();
        tileDetectorBoxColider.isTrigger = true;
        tileDetectorBoxColider.center = new Vector3(0.0f, -0.065f, 0.0f);
        tileDetectorBoxColider.size = new Vector3(0.002f, 0.20f, 0.02f);
        tileDetectorBoxColider.enabled = false;
    }

    /// <summary>
    /// The PositionAllPieces.
    /// </summary>
    private void PositionAllPieces()
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
            for (int y = 0; y < TILE_COUNT_Y; y++)
                if (chessPieces[x, y] != null)
                    PositionSinglePiece(x, y, true);
    }

    /// <summary>
    /// The PositionSinglePiece.
    /// </summary>
    /// <param name="x">The x<see cref="int"/>.</param>
    /// <param name="y">The y<see cref="int"/>.</param>
    /// <param name="force">The force<see cref="bool"/>.</param>
    private void PositionSinglePiece(int x, int y, bool force = false)
    {
        chessPieces[x, y].currentX = x;
        chessPieces[x, y].currentY = y;
        chessPieces[x, y].transform.position = GetTileCenter(x, y);
        chessPieces[x, y].transform.Find(Shared.TileDetectorName).transform.position = GetTileCenter(x, y);
        chessPieces[x, y].Chessboard = this;
        chessPieces[x, y].SavePosition();
        chessPieces[x, y].SaveOrientation();
    }

    /// <summary>
    /// The GetTileCenter.
    /// </summary>
    /// <param name="x">The x<see cref="int"/>.</param>
    /// <param name="y">The y<see cref="int"/>.</param>
    /// <returns>The <see cref="Vector3"/>.</returns>
    private Vector3 GetTileCenter(int x, int y)
    {
        return new Vector3(x * tileSize, yOffset, y * tileSize) - bounds + new Vector3(tileSize / 2, 0, tileSize / 2);
    }

    /// <summary>
    /// The PieceWasPickedUp.
    /// </summary>
    /// <param name="x">The x<see cref="int"/>.</param>
    /// <param name="y">The y<see cref="int"/>.</param>
    public void PieceWasPickedUp(int x, int y)
    {
        tiles[x, y].GetComponent<MeshRenderer>().material = tilesMaterials[((int)Shared.TileType.Selected)];
        moves = chessPieces[x, y].CalculateAvailablePositions();
        foreach (var move in moves.AvailableMoves)
        {
            var moveToTile = tiles[move.x, move.y];
            if (moveToTile.GetComponent<Tile>().IsWhiteTile)
            {
                moveToTile.GetComponent<MeshRenderer>().material = tilesMaterials[((int)Shared.TileType.AvailableWhite)];
            }
            else
            {
                moveToTile.GetComponent<MeshRenderer>().material = tilesMaterials[((int)Shared.TileType.AvailableBlack)];
            }
            moveToTile.GetComponent<Tile>().IsAvailableTile = true;
        }

        foreach (var move in moves.AttackMoves)
        {
            var moveToTile = tiles[move.x, move.y];
            if (moveToTile.GetComponent<Tile>().IsWhiteTile)
            {
                moveToTile.GetComponent<MeshRenderer>().material = tilesMaterials[((int)Shared.TileType.AttackTileWhite)];
            }
            else
            {
                moveToTile.GetComponent<MeshRenderer>().material = tilesMaterials[((int)Shared.TileType.AttackTileBlack)];
            }
        }

        DisablePickUpOnOtherPieces(chessPieces[x, y]);
    }

    /// <summary>
    /// The PieceWasDropped.
    /// </summary>
    /// <param name="currentX">The currentX<see cref="int"/>.</param>
    /// <param name="currentY">The currentY<see cref="int"/>.</param>
    /// <param name="newTile">The newTile<see cref="Tile"/>.</param>
    public void PieceWasDropped(int currentX, int currentY, Tile newTile)
    {
        tiles[currentX, currentY].GetComponent<MeshRenderer>().material = tilesMaterials[((int)Shared.TileType.Default)];
        foreach (var move in moves.AvailableMoves)
        {
            tiles[move.x, move.y].GetComponent<MeshRenderer>().material = tilesMaterials[((int)Shared.TileType.Default)];
            tiles[move.x, move.y].GetComponent<Tile>().IsAvailableTile = false;
        }

        foreach (var move in moves.AttackMoves)
        {
            tiles[move.x, move.y].GetComponent<MeshRenderer>().material = tilesMaterials[((int)Shared.TileType.Default)];
        }

        moves = null;

        if (newTile != null)
        {
            Vector2Int newPosition = newTile.Position;
            ChessPiece chessPiece = chessPieces[currentX, currentY];

            chessPiece.transform.position = GetTileCenter(newPosition.x, newPosition.y);
            chessPieces[newPosition.x, newPosition.y] = chessPiece;
            chessPieces[currentX, currentY] = null;
            chessPiece.currentX = newPosition.x;
            chessPiece.currentY = newPosition.y;

            chessPiece.SavePosition();
            chessPiece.IsMoved = true;
        }

        EnablePickUpOnPieces();
    }

    /// <summary>
    /// The DisablePickUpOnOtherPieces.
    /// </summary>
    /// <param name="pickedPiece">The pickedPiece<see cref="ChessPiece"/>.</param>
    public void DisablePickUpOnOtherPieces(ChessPiece pickedPiece)
    {
        foreach (var piece in chessPieces)
        {
            if (piece != null && !piece.Equals(pickedPiece))
            {
                piece.GetComponent<XRGrabInteractable>().enabled = false;
            }
        }
    }

    /// <summary>
    /// The EnablePickUpOnPieces.
    /// </summary>
    public void EnablePickUpOnPieces()
    {
        foreach (var piece in chessPieces)
        {
            if (piece != null)
            {
                piece.GetComponent<XRGrabInteractable>().enabled = true;
            }
        }
    }

    /// <summary>
    /// The HandleTileTrigger.
    /// </summary>
    /// <param name="possition">The possition<see cref="Vector2Int"/>.</param>
    /// <param name="enterTrigger">The enterTrigger<see cref="bool"/>.</param>
    public void HandleTileTrigger(Vector2Int possition, bool enterTrigger)
    {
        if (enterTrigger)
        {
            tiles[possition.x, possition.y].GetComponent<MeshRenderer>().material = tilesMaterials[((int)Shared.TileType.MoveTo)];
        }
        else
        {
            var tile = tiles[possition.x, possition.y];
            if (tile.GetComponent<Tile>().IsWhiteTile)
            {
                tiles[possition.x, possition.y].GetComponent<MeshRenderer>().material = tilesMaterials[((int)Shared.TileType.AvailableWhite)];
            }
            else
            {
                tiles[possition.x, possition.y].GetComponent<MeshRenderer>().material = tilesMaterials[((int)Shared.TileType.AvailableBlack)];
            }
        }
    }

    /// <summary>
    /// Will calculate the occupation status of a tile by checking if we have a piece or not on a given possition. It will also
    /// take into consideration whether a tile is occupied by a friendly or an enemy piece.
    /// </summary>
    /// <param name="position">position to be checked.</param>
    /// <param name="selectedPieceTeam">selected piece team.</param>
    /// <returns>The <see cref="Shared.TileOccuppiedBy"/>.</returns>
    public Shared.TileOccuppiedBy CalculateSpaceOccupation(Vector2Int position, Shared.TeamType selectedPieceTeam)
    {
        ChessPiece chessPiece;
        try
        {
            chessPiece = chessPieces[position.x, position.y];
        }
        catch
        {
            return Shared.TileOccuppiedBy.EndOfTable;
        }

        if (chessPiece == null)
        {
            return Shared.TileOccuppiedBy.None;
        }

        return selectedPieceTeam.Equals(chessPiece.team) ? Shared.TileOccuppiedBy.FriendlyPiece : Shared.TileOccuppiedBy.EnemyPiece;
    }
}
