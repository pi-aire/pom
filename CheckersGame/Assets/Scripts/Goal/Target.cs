using UnityEngine;
using System;
using System.Collections.Generic;

public class Target : Goal
{
/* Attribut spécifique de la classe Target*/
    private int id;
    private Vector2Int position;

/* Constructeur de la classe Target*/
    public Target(Tuple<int, Vector2Int> target)
    {
        this.id = target.Item1;
        this.position = target.Item2;
    }

/* Méthode imlémenté de la classe Goal*/
    public override bool isValid(Board board)
    {
        Vector2Int? find = board.findPieceByID(this.id);
        return find.HasValue && find.Value != null;
    }

    public override List<Action> filter(Board board, List<Action> available)
    {
        List<Action> targetAC = new List<Action>();
        int minDist = -1;
        this.position = (Vector2Int)board.findPieceByID(this.id) ;
        foreach (Action ac in available)
        {
            if (ac.alreadyCaptured(this.position))
            {
                minDist = -1;
                targetAC = new List<Action>();
                targetAC.Add(ac);
                break;
            }
            minDist = board.distance(ac.oldPosition, this.position);
            if (minDist > board.distance(ac.newPosition, this.position))
            {
                targetAC = new List<Action>();
                targetAC.Add(ac);
                minDist = board.distance(ac.newPosition, this.position);
            } 
            else if(minDist == board.distance(ac.newPosition, this.position))
            {
                targetAC.Add(ac);
            }
            

        }
        return targetAC;
    }

    public override string ToString()
    {
        return id + ": Target Goal " + position;
    }
}
