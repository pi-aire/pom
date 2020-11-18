using System.Collections.Generic;
using System;
using UnityEngine;

public class NoMove : Goal
{

    /*Constructeur de la classe NoMove*/
    public NoMove()
    {

    }

/*Methode implémenté de la classe Goal*/
    public override bool isValid(Board board)
    {
        return true;
    }

    //Cette implémentation permet de filter sur l'avancé et la rafle 
    public override List<Action> filter(Board board, List<Action> available)
    {
        return available;
    }

    public override string ToString()
    {
        return "NoMove Goal";
    }
}