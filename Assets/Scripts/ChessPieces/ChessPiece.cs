using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Shared;

public enum ChessPieceType
{
    None = 0,
    Pawn = 1,
    Rook = 2,
    Knight = 3,
    Bishop = 4,
    Queen = 5,
    King = 6
}

public abstract class ChessPiece : MonoBehaviour
{
    public TeamType team;
    public int currentX;
    public int currentY;
    public ChessPieceType type;

    // Art
    private Material material;
    public Material Material { get => material; set => material = value; }

    // Logic
    private Quaternion desiredRotation;
    private Vector3 initialPosition;
    private Tile hoveringTile;
    private Chessboard chessboard; 
    protected bool isTouched = false;

    public bool IsTouched { get => isTouched; set => isTouched = value; }
    public Tile HoveringTile { get => hoveringTile; set => hoveringTile = value; }
    public Chessboard Chessboard { get => chessboard; set => chessboard = value; }

    //---------------------------------------------------- Methods ------------------------------------------------------

    public void PickPiece()
    {
        transform.Find(TileDetectorName).gameObject.GetComponent<BoxCollider>().enabled = true;
        chessboard.PieceWasPickedUp(currentX, currentY);
        transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
    }

    public void PlacePiece()
    {
        transform.Find(TileDetectorName).gameObject.GetComponent<BoxCollider>().enabled = false;
        chessboard.PieceWasDropped(currentX, currentY, hoveringTile);

        transform.SetPositionAndRotation(initialPosition, desiredRotation);
        transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        SavePosition();
    }

    public void SavePosition()
    {
        var transform = GetComponent<Transform>().transform;
        initialPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
    }

    public void SaveOrientation()
    {
        var rotation = GetComponent<Transform>().transform.rotation;
        desiredRotation = new Quaternion(rotation.x, rotation.y, rotation.z, rotation.w);
    }

    public void RevertToOriginalMaterial()
    {
        GetComponent<MeshRenderer>().material = Material;
    }

    public abstract List<Vector2Int> CalculateAvailablePositions();
}
