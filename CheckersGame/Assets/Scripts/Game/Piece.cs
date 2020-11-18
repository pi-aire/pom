using System;
using UnityEngine;

/* Enumerateur des couleurs possibles d'une piece*/
public enum Color
{
    White, Black
};
public class Burn
{
    public static int value = 4;
}
public abstract class Piece
{
    /* Variable de Classe*/
    public static int idGenerator = 0;

    /* Attribut de classe commun à toute les classes heritant de Piece*/
    public int id; // Id de la piece attribué à la création d'un objet
    public Vector2Int position { get; protected set; } // Position courant de la Piece
    public int life { get; protected set; } // Point de vie de la Piece
    public Color color { get; protected set; } // Couleur de la Piece
    public Goal goal; // définie l'objectif de déplecement de la Piece
    public Selection predictions; // Selection gerant la prédiction de la Piece et les prédictions reçu

    public int standBy = Burn.value;

    /*Methode de la Classe Piece*/
    // Fournie un id à un objet herité de la class piece
    private int getNewId()
    {
        int nid = idGenerator;
        idGenerator += 1;
        return nid;
    }

    // Constructeur de Piece qui sert seulement à attribuer un ID
    protected Piece()
    {
        this.id = getNewId();
        this.predictions = new Selection();
    }

    /*Methode abstraite a implémenter par les sous classes*/
    // Inflige les dégats à la piece donné en paramètre
    public abstract void takeDamage(int dm);
    // Realise un coup et les conscéquences de c'est coup
    public abstract bool action(Player p, Board board);

    // Set l'attribut position avec la valeur fournie en parametre
    public abstract void setPosition(Vector2Int npositon);

    // Retourne le rang de la piece
    public abstract int getRank();

    public abstract int getRankNoBurn();

    // Clone
    public abstract Piece clone();

    /*Methode implémenetée commune à toute les sous classe de Piece*/
    // Verifie que la Piece est vivante si oui retourne True
    public bool isAlive()
    {
        return this.life > 0;
    }

    // Suit les activité de déplacement de la Pieces
    public void hasMove(bool v)
    {
        if (v)
        {
            standBy = Burn.value;
        }
        else
        {
            standBy--;
            if (standBy == 0)
            {
                takeDamage(1);
                standBy = Burn.value;
            }
        }
    }

    // Verifier que entre la Piece (this) et la destination il n'y a pas de Piece
    public bool viewLine(int x, int y, Board board)
    {
        int posX = this.position.x;
        int posY = this.position.y;
        bool end = false;
        while (!end)
        {
            if (posX > x && posY > y)
            {
                posX = posX - 1;
                posY = posY - 1;
            }
            else if (posX < x && posY > y)
            {
                posX = posX + 1;
                posY = posY - 1;
            }
            else if (posX < x && posY < y)
            {
                posX = posX + 1;
                posY = posY + 1;
            }
            else if (posX > x && posY < y)
            {
                posX = posX - 1;
                posY = posY + 1;
            }
            else return true;
            if (posX < board.size && posX >= 0 && posY < board.size && posY >= 0
                && board.grille[posX, posY] != null && posX != x && posY != y)
            {
                return false;
            }
        }
        return true;
    }

    // Donne la distance entre this et une cible
    public int distance(Piece target, Board board)
    {
        return Mathf.Abs(this.position.x - target.position.x)
            + Mathf.Abs(this.position.y - target.position.y);
    }

    // Calcul une prédiction et l'ajoute à la selection
    public void predict(Player p, Board board)
    {
        p.predictMoves(this, board);
    }

    // 
    public void election(Player p, Board board)
    {
        p.electionMoves(this, board);
    }

    // Selectionne une cible pour la piece et retourne sa position actuel
    public void newGoal(Board board)
    {
        Tuple<int, Vector2Int> infoTarget = board.findOnePiece(typeof(Tower), this);
        if (infoTarget.Item1 == -1 && this.GetType() == typeof(Tower)) // S'il n'y a pas de tour sur le plateau on cherche un pion
        {
            infoTarget = board.findOnePiece(typeof(Pion), this);
            if (infoTarget.Item1 != -1)
            {
                this.goal = new Target(infoTarget);// Cas où une tour cible un Pion
            }
            else
            {
                this.goal = new NoMove();
            }
        }
        else if (infoTarget.Item1 == -1 && this.GetType() == typeof(Pion) && board.count(color) > 1)
        {
            this.goal = new Forward(this.color); // Cas où un Pion veux se transformer en tour
        }
        else if (infoTarget.Item1 == -1 && this.GetType() == typeof(Pion) && board.count(color) == 1) // il ne reste qu'un pion d'une couleur
        {
            infoTarget = board.findOnePiece(typeof(Pion), this);
            this.goal = new Target(infoTarget);// Cas où une tour cible un Pion
        }
        else
        {
            this.goal = new Target(infoTarget);// Cas où une Piece cible une tour
        }
    }

    // Réactualise le but de la piece
    public void refreshGoal(Board board)
    {
        if (this.goal == null || !goal.isValid(board))
        {
            this.newGoal(board);
        }
    }

    // Implementation de ToString
    public override string ToString()
    {
        if (color.Equals(Color.White))
        {
            return "0";
        }
        else
        {
            return "1";
        }
    }

}
