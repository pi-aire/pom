using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;

[Serializable]
public class TypePiece
{
    public enum tPiece
    {
        TOWER,PION
    }

    public tPiece type; // type de la piece
    public int height; // Hauteur de la tour si en est
    public TypePiece(Type t, int h = 0)
    {
        if (t == typeof(Tower))
        {
            type = tPiece.TOWER;
        }
        else
        {
            type = tPiece.PION;
        }
        height = h;
    }
    public TypePiece() { }
}

[Serializable]
public class Capture
{
    public Vector2Int vector;
    public TypePiece type;
    public Capture(Vector2Int v,TypePiece t)
    {
        vector = v;
        type = t;
    }
    public Capture() { }
}

[Serializable]
public class GenericAction
{
    /* Attribut de la classe de Action*/
    public Vector2Int newPosition; // la nouvelle position de la piece
    public Vector2Int oldPosition; // ancienne position de la piece
    public TypePiece type;
    public HashSet<Capture> captured = new HashSet<Capture>(); // Set des postions de piece capturé
    public Camp camp;
    /*Constructeur*/
    // On crée une action générique à partir d'un action, on réalise la conversion 
    public GenericAction(Board board, Vector2Int context, Action ac, Camp camp)
    {
        this.oldPosition = new Vector2Int(ac.oldPosition.x - context.x, ac.oldPosition.y - context.y);
        this.newPosition = new Vector2Int(ac.newPosition.x - context.x, ac.newPosition.y - context.y);
        this.captured = new HashSet<Capture>();
        type = getTypePiece(board, ac.oldPosition);
        foreach (Vector2Int vec in ac.captured)
        {
            this.captured.Add(
                new Capture(new Vector2Int(vec.x - ac.oldPosition.x, vec.y - ac.oldPosition.y), 
                getTypePiece(board, vec)));
        }
        this.camp = camp;
    }
    public GenericAction() { }

    /*Méthodes*/
    // Retourne le type de piece donnée en paramètre
    public TypePiece getTypePiece(Board board, Vector2Int vec)
    {
        if (board.grille[vec.x, vec.y].GetType() == typeof(Tower))
        {
            return new TypePiece(typeof(Tower), ((Tower)board.grille[vec.x, vec.y]).height);
        }
        else
        {
            return new TypePiece(typeof(Pion));
        }
    }

    // Regarde si la pièce est bien compatible avec le TypePiece
    public bool compatibleType(TypePiece tp, Board board, Vector2Int vec)
    {
        if (board.grille[vec.x, vec.y].GetType() == typeof(Tower) && tp.type == TypePiece.tPiece.TOWER)
        {
            return tp.height == ((Tower)board.grille[vec.x, vec.y]).height;
        }
        return board.grille[vec.x, vec.y].GetType() == typeof(Pion) && tp.type == TypePiece.tPiece.PION;
    }

    /*Méthode de convertion*/
    public Action toAction(Board current, Vector2Int context)
    {
        Vector2Int old = new Vector2Int(oldPosition.x + context.x, oldPosition.y + context.y);
        Vector2Int newp = new Vector2Int(newPosition.x + context.x, newPosition.y + context.y);
        List<Vector2Int> list = new List<Vector2Int>();

        Color ally = current.grille[context.x, context.y].color;
        Color opponent = (Color)(((int)ally + 1) % 2);

        if (!current.existe(old)
                || !current.isCorrect(old)
                || (camp == Camp.ALLY && current.grille[old.x, old.y].color != ally)
                || (camp == Camp.OPPONENT && current.grille[old.x, old.y].color != opponent))
        {
            throw new Exception("L'action générique n'est pas applicable");
        }
        else
        {
            if (!compatibleType(type, current, old))
            {
                throw new Exception("L'action générique n'est pas applicable");
            }
        }
        // On regarde si l'action est possible sur le plateau avec le context donné
        foreach (Capture cap in captured)
        {
            if (!current.isCorrect(cap.vector)
                || (camp == Camp.ALLY && current.grille[cap.vector.x, cap.vector.y].color == ally)
                || (camp == Camp.OPPONENT && current.grille[cap.vector.x, cap.vector.y].color == opponent))
            {
                //Debug.Log("erreur de capture " +vec+ ", Action "+ tmp +"\n"+current);
                throw new Exception("L'action générique n'est pas applicable");
            }
            else
            {
                if (!compatibleType(cap.type, current,cap.vector))
                {
                    throw new Exception("L'action générique n'est pas applicable");
                }
                else
                {
                    list.Add(new Vector2Int(cap.vector.x + context.x, cap.vector.y + context.y));
                }
            }
        }

        return new Action(old, newp, list);
    }
    /*Méthode implémenter des classes herité*/
    public override string ToString()
    {
        string s = "[ " + oldPosition.ToString() + "=> ";
        foreach (Capture cap in captured)
        {
            s += cap.vector.ToString() + "=> ";
        }
        s += newPosition + " ]";
        return s;
    }
}
