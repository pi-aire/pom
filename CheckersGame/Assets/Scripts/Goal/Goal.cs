using System.Collections.Generic;
using UnityEngine;

public abstract class Goal
{

/* Méthode abstraite*/
    // Verifie si l'objectif est toujours atteignable
    public abstract bool isValid(Board board);

    // Selection des déplacement disponible qui réponde le mieux à l'objectif
    public abstract List<Action> filter(Board board, List<Action> available);

}
