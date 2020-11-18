using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Player
{
    void predictMoves(Piece subject, Board board);

    void electionMoves(Piece subject, Board board);


    Action movePiece(Piece subject, Board board);
}
