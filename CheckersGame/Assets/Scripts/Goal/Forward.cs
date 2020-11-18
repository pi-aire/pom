using System.Collections.Generic;
using System;
using UnityEngine;

public class Forward : Goal
{
/* Les attributs de la classe Forward*/
    Color color;

/*Constructeur de la classe Foward*/
 public Forward(Color c)
    {
        this.color = c;
    }

/*Methode implémenté de la classe Goal*/
    public override bool isValid(Board board)
    {
        return board.count(color) > 1;
    }

    //Cette implémentation permet de filter sur l'avancé et la rafle 
    public override List<Action> filter(Board board, List<Action> available)
    {
        List<Action> forwardAc = new List<Action>();
        int minDist = -1;
        int edge = 0;
        if (color.Equals(Color.White))
        {
            edge = 9;
        }

        foreach (Action ac in available)
        { 
            if (minDist > board.distanceEdge(ac.newPosition, edge) || minDist == -1)
            {
                forwardAc = new List<Action>();
                forwardAc.Add(ac);
                minDist = board.distanceEdge(ac.newPosition, edge);
            } 
            else if (minDist == board.distanceEdge(ac.newPosition, edge))
            {
                forwardAc.Add(ac);
            }
        }
        return forwardAc;
    }

    public override string ToString()
    {
        return "Forward Goal";
    }
}