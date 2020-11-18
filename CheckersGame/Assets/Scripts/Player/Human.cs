using UnityEngine;
using System.Collections;

public class Human : Player
{
    public Human()
    {
        Debug.Log("Création player Human");
    }

    public void predictMoves(Piece subject, Board board)
    {
        
    }


    public Action movePiece(Piece subject, Board board)
    {
        return new Action(subject.position);
    }

    public void electionMoves(Piece subject, Board board)
    {
    }
}
