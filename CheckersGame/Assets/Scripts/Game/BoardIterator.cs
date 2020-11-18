using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*Iterateur pour la récupération des pièces une par une dans l'ordre*/
public class BoardIterator
{
/* Les attributs des la classe BoardIterator*/
    int index;
    int idMax;
    Color color;

    /* Constructor du BoardIterator*/
    public BoardIterator(Color nColor)
    {
        index = 0;
        idMax = 15;
        this.color = nColor;
    }
    // Position actuel de la piece
    public BoardIterator(Color nColor, int id)
    {
        index = id % 15;
        index += 1;
        idMax = 15;
        this.color = nColor;
    }
    // De zero a la position actuel
    public BoardIterator(Color nColor, int id, bool max)
    {
        this.color = nColor;
        if (max)
        {
            index = 0;
            idMax = id;
        }
        else
        {
            index = id % 15;
            index += 1;
            idMax = 15;
        }
    }
    /* Methode de l'itérateur*/
    //Retourne une piece suivante
    public IEnumerable values(Board board)
    {
        while (index < idMax)
        {
            Piece piece = null;
            while (index < idMax && piece == null)
            {
                Vector2Int? posi;
                if (color.Equals(Color.Black))
                {
                    posi = board.findPieceByID(index + 15);
                }
                else
                {
                    posi = board.findPieceByID(index);
                }
                if (posi.HasValue)
                {
                    Vector2Int v = posi.Value;
                    if (board.grille[v.x, v.y].color.Equals(color))
                    {
                        piece = board.grille[v.x, v.y];
                    }
                }
                index += 1;
            }
            yield return piece;
        }
    }
}