using UnityEngine;
using System.Collections;

public interface IResolver
{
    void predicte(Piece subject, Board board);
    void election(Piece subject, Board board);
    Action resolve(Piece subject, Board board);
}
