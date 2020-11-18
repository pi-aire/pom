using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Gère que les prediction
public class Prediction
{
    public int idG; // l'identifiant de la GénériquePrédiction. Valeur par défaut à -1
    public Board board;
    public Dictionary<int, Action> actions; //liste des actions
    public int innitialRank; // Donne le rang innitial tu plateau
    // Creation d'une prédiction
    public Prediction(Board innitial, Color color, int id = -1)
    {
        this.idG = id;
        board = new Board(innitial);
        actions = new Dictionary<int, Action>();
        innitialRank = board.getRankNoBurn(color);
        //actions.Add(innitial.grille[a.oldPosition.x, a.oldPosition.y].id, a);
    }

    public Prediction(Prediction other)
    {
        board = new Board(other.board);
        actions = new Dictionary<int, Action>(other.actions);
        innitialRank = other.innitialRank;
        idG = other.idG;
    }

    //Récuperation du hash pour la comparaison des tableaus
    public string GetHashString()
    {
        string hash = "";
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                if (board.grille[i, j] != null)
                {
                    hash += board.grille[i, j].getRank();
                }
                else
                {
                    hash += 0;
                }
            }
        }
        return hash;
    }

    // Créé une nouvelle prédiction à partir de celle ci et ajoute la nouvelle action
    public Prediction extendPrediction(Action a)
    {
        int id = board.grille[a.oldPosition.x, a.oldPosition.y].id;
        Prediction result = new Prediction(this);
        result.board = a.toBoard(board);
        try
        {
            result.actions.Add(id, a);
        }
        catch { }
        return result;
    }

    // get Actions avec l'id de la Piece
    public Action getAction(Piece p)
    {
        if (actions.ContainsKey(p.id))
        {
            return actions[p.id];
        }
        else
        {
            return new Action(p.position); // new action ne bouge pas
        }
    }

    // Conversion d'une prédiction en prediction generique
    public GenericPrediction toGeneric(Vector2Int context, Color color, Board innitial)
    {
        return new GenericPrediction(this, color, innitial, context);
    }

    /* Methode implémenté*/
    public override int GetHashCode()
    {
        return board.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        return board.Equals(obj);
    }


}
