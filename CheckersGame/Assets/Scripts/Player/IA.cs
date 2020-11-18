using UnityEngine;
using System.Collections;

public class IA : Player
{
    private IResolver resolver;
    public IA(IResolver resolver)
    {
        this.resolver = resolver;
        Debug.Log("Création player IA");
    }

    public void predictMoves(Piece subject, Board board)
    {
        resolver.predicte(subject, board);
    }

    public void electionMoves(Piece subject, Board board)
    {
        resolver.election(subject, board);
    }

    public Action movePiece(Piece subject, Board board)
    {
        return resolver.resolve(subject, board);
    }
}