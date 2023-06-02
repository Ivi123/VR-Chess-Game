using System;
using ChessLogic;
using Managers;
using UnityEngine;

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
    public Shared.TeamType team;
    public int currentX;
    public int currentY;
    public ChessPieceType type;

    // Art
    public Material Material { get; set; }

    // Logic
    private Quaternion desiredRotation;
    private Vector3 position;
    protected bool isMoved = false;

    public bool IsMoved { get => isMoved; set => isMoved = value; }
    public Tile HoveringTile { get; set; }
    public Chessboard Chessboard { get; set; }
    public MovementManager MovementManager { get; set; }

    //---------------------------------------------------- Methods ------------------------------------------------------

    public void Start()
    {
        transform.rotation = Quaternion.Euler(team == Shared.TeamType.White ? new Vector3(0, -90, 0) : new Vector3(0, 90, 0));
        this.SaveOrientation();
    }

    public void PickPiece()
    {
        transform.Find(Shared.TileDetectorName).gameObject.GetComponent<BoxCollider>().enabled = true;
        MovementManager.PieceWasPickedUp(currentX, currentY);
        transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
    }

    public void PlacePiece()
    {
        transform.Find(Shared.TileDetectorName).gameObject.GetComponent<BoxCollider>().enabled = false;
        MovementManager.PieceWasDropped(currentX, currentY, HoveringTile);

        transform.SetPositionAndRotation(position, desiredRotation);
        transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        HoveringTile = null;
    }

    public void SavePosition()
    {
        var transformPosition = transform.position;
        position = new Vector3(transformPosition.x, transformPosition.y, transformPosition.z);
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

    public abstract Moves CalculateAvailablePositions();
}
