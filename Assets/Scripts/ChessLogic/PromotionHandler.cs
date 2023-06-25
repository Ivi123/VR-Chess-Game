using System;
using System.Collections;
using System.Collections.Generic;
using ChessLogic;
using ChessPieces;
using Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class PromotionHandler : MonoBehaviour
{
    public GameObject promotionUI;
    public GameObject promotionInteractor;
    public ChessPieceType chessPieceType;
    
    public MovementManager movementManager;
    public Chessboard chessboard;
    private ChessPiece pawnToPromote;
    
    //Buttons
    public Button bishopButton;
    public Button knightButton;
    public Button queenButton;
    public Button rookButton;

    public void EnableDisablePromotionCanvas(bool value, ChessPiece passedPawn)
    {
        pawnToPromote = passedPawn;
        promotionUI.SetActive(value);
        promotionInteractor.SetActive(value);
    }

    public void ChangeInBishop()
    {
        chessPieceType = ChessPieceType.Bishop;
        MarkPromotion();
        EnableDisablePromotionCanvas(false, null);
        EventSystem.current.SetSelectedGameObject(null);
        bishopButton.OnDeselect(null);
    }
    
    public void ChangeInQueen()
    {
        chessPieceType = ChessPieceType.Queen;
        MarkPromotion();
        EnableDisablePromotionCanvas(false, null);
        EventSystem.current.SetSelectedGameObject(null);
        queenButton.OnDeselect(null);
    }
    
    public void ChangeInRook()
    {
        chessPieceType = ChessPieceType.Rook;
        MarkPromotion();
        EnableDisablePromotionCanvas(false, null);EventSystem.current.SetSelectedGameObject(null);
        rookButton.OnDeselect(null);
    }
    
    public void ChangeInKnight()
    {
        chessPieceType = ChessPieceType.Knight;
        MarkPromotion();
        EnableDisablePromotionCanvas(false, null);
        EventSystem.current.SetSelectedGameObject(null);
        knightButton.OnDeselect(null);
    }

    private void MarkPromotion()
    {
        // We eliminate the pawn to be promoted from every list that it's tracking the pieces
        var pawnTeam = pawnToPromote.team == Shared.TeamType.White
            ? movementManager.WhitePieces
            : movementManager.BlackPieces;
        pawnTeam.Remove(pawnToPromote.gameObject);
        pawnToPromote.MyPlayer.Pieces.Remove(pawnToPromote);
        movementManager.ChessPieces[pawnToPromote.currentX, pawnToPromote.currentY] = null;

        //We create the new promoted gameObject
        var newPiece = chessboard.SpawnSinglePiece(chessPieceType, pawnToPromote.team);
        newPiece.IsMoved = true;
        newPiece.MyPlayer = pawnToPromote.MyPlayer;
        movementManager.ChessPieces[pawnToPromote.currentX, pawnToPromote.currentY] = newPiece;
        chessboard.PositionSinglePiece(pawnToPromote.currentX, pawnToPromote.currentY);
        
        // Add the new piece to the correct team
        pawnTeam.Add(newPiece.gameObject);
        pawnToPromote.MyPlayer.Pieces.Add(newPiece);
        
        // Finally we destroy the old gameObject
        Destroy(pawnToPromote.gameObject);
        newPiece.MyPlayer.HasMoved = true;
        newPiece.DisablePickUpOnPiece();
    }
    
}
