using System;
using System.Collections.Generic;
using UnityEngine;

/*La classe selection contient la liste des meilleurs board 
 * pour chaque Piece d'une couleur et le sondage 
 * pour qu'une stratégie soit adopté par l'ensemble des Pieces
 */

public class Selection
{
    /*Attribut de la classe selection*/
    public Dictionary<int, Tuple<string, Prediction>> data;
    private int[] strawpoll;

/* Constructeur */
    public Selection()
    {
        data = new Dictionary<int, Tuple<string, Prediction>>();
        strawpoll = new int[30];
    }

/* Méthode de la selection*/
    // Ajoute un plateau de prédiction a la selection
    public void add(int id, Prediction predi)
    {
        string key = predi.board.GetHashString();
        data.Add(id, new Tuple<string, Prediction>(key, predi));
    }

    // la base des doublons
    private void compress()
    {
        Dictionary<string, int> exits = new Dictionary<string, int>();
        List<int> toRemove = new List<int>();
        foreach ( KeyValuePair<int, Tuple<string, Prediction>> pair in data)
        {
            int autorID = pair.Key;
            string hash = pair.Value.Item1;
            Prediction predic = pair.Value.Item2;
            if (!exits.ContainsKey(hash))
            {
                exits.Add(hash, autorID);
            }
            else
            {
                int id1 = exits[hash];
                int id2 = autorID;
                if (id1 > id2)
                {
                    exits[hash] = id2;
                    toRemove.Add(id1);
                }
                else
                {
                    toRemove.Add(id2);
                }
            }
        }
        foreach (int id in toRemove)
        {
            data.Remove(id);
        }

    }

    // retourn l'id de l'auteur de la prédiction slectionné par P
    private int getBest(Piece p, Board board)
    {
        compress();
        int auteurMax = -1;
        int vMax = -1;
        foreach (KeyValuePair<int, Tuple<string, Prediction>> value in data)
        {
            if (value.Value.Item2.board.findPieceByID(p.id).HasValue)
            {
                int rank = value.Value.Item2.board.getRank(p.color);
                if (vMax == -1 || vMax < rank)
                {
                    auteurMax = value.Key;
                    vMax = rank;
                }
                //else if (vMax == rank && value.Value.Item2.getAction(p.id).willMove())
                else if (vMax == rank && !value.Value.Item2.Equals(board))
                {
                    auteurMax = value.Key;
                    //vMax = rank;
                }
            }
        }
        //Debug.Log(data.Count+"INDEX BEST DES BESTS "+auteurMax);
        return auteurMax;
    }

    // Réalise le vote d'une prédiction
    public void voting(Piece p, Board board)
    {
        int ballot = getBest(p,board);
        strawpoll[ballot] += 1;
        sendBallot(p, board, ballot);
    }

    // retour l'action de la prédiction qui fait conscensuce
    // retour l'action de la prédiction gagnante
    public Action getActionOfSelectedMove(Piece p)
    {
        int maxAutor = -1;
        int max = -1;
        for (int i = 0; i<30; ++i)
        {
            if (max == -1 || max < strawpoll[i])
            {
                max = strawpoll[i];
                maxAutor = i;
            }
        }
        return data[maxAutor].Item2.getAction(p);
    }

    // L'action des
    public int getIdDataBasePredic()
    {
        int maxAutor = -1;
        int max = -1;
        for (int i = 0; i < 30; ++i)
        {
            if (max == -1 || max < strawpoll[i])
            {
                max = strawpoll[i];
                maxAutor = i;
            }
        }
        return data[maxAutor].Item2.idG;
    }
    /* Méthode de la transmition entre piece */
    // Envoie sa prédiction au autre piece du même camp
    public void sendPrediction(Board board, Piece transmitter, Prediction prediction)
    {
        foreach (Piece recipient in board.pieces(transmitter.color))
        {
            if (recipient.id != transmitter.id)
            {
                recipient.predictions.receivePrediction(transmitter.id,prediction);
            }
        }
    }

    // Reception d'une prédiction d'une autre piece
    public void receivePrediction(int id, Prediction predication)
    {
        // On l'ajoute à sa base
        add(id, predication);
    }

    /* Méthode pour la communication du vote des pieces*/

    private void sendBallot(Piece transmitter, Board board, int ballot)
    {
        foreach (Piece recipient in board.pieces(transmitter.color))
        {
            if (recipient.id != transmitter.id)
            {
                recipient.predictions.receiveBallot(ballot);
            }
        }
    }

    private void receiveBallot(int ballot)
    {
        strawpoll[ballot] += 1;
    }
}
