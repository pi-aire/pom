using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
//public class PossibilityTree : MonoBehaviour, IResolver

public class AlphaBeta : MonoBehaviour, IResolver
{
    int smart = 1;
    public AlphaBeta(int ismart = 1)
    {
        smart = ismart;
    }

    /* Méthode défini par l'interface IResolvers*/
    public Action resolve(Piece subject, Board board)
    {
        Action ac = subject.predictions.getActionOfSelectedMove(subject);
        //Debug.LogWarning(ac);
        return ac;
    }
    // Pas de prédiction avec ce resolver
    public void predicte(Piece subject, Board board)
    {
        //Debug.Log("Prédiction de "+subject);
        Prediction predi = predictionMove(subject, board);
        subject.predictions.add(subject.id, predi);
        subject.predictions.sendPrediction(board, subject, predi);
    }

    public void election(Piece subject, Board board)
    {
        //Debug.Log("Avant voting " + subject.predictions.data.Count);
        subject.predictions.voting(subject, board);
    }

    // Syteme de calcul alpha beta
    // La valeur smart le nombre de prédiction que l'on réalise
    // la piece fournit en paramètre est la piece qui réalise l
    private int alphaBeta(PredictionTree node, Piece p, int smart, int alpha, int beta)/* alpha et toujours inférieur à beta*/
    {
        if (!node.isNode())
        {
            //Debug.Log("C'est une feuille");
            return node.value;
        }
        else if (node.type == ABType.MIN) // Noeud de type Min
        {
            int v = int.MaxValue;
            foreach (PredictionTree son in node.getSons(p.color, p.id, (smart == 0)))
            {
                v = Math.Min(v, alphaBeta(son, p, smart, alpha, beta));
                node.value = v;
                if (alpha >= v)
                {
                    return v;
                }
                beta = Math.Min(beta, v);
            }
            return v;
        }
        else // Noeud de type Max
        {
            int v = int.MinValue;
            foreach (PredictionTree son in node.getSons(p.color, p.id)) // On parcours tous les mins
            {
                v = Math.Max(v, alphaBeta(son, p, smart - 1, alpha, beta));
                node.value = v;
                if (v >= beta)
                {
                    return v;
                }
                alpha = Math.Max(alpha, v);
            }
            return v;
        }
    }

    private Prediction predictionMove(Piece p, Board board)
    {
        Board virtualBoard = new Board(board);
        // On créer le premier noeud
        PredictionTree node = new PredictionTree(ABType.MAX, new Prediction(virtualBoard,p.color)); // Max car c'est les predictions du joueur
        //Debug.Log(node);
        int maxValue = alphaBeta(node, p, smart, int.MinValue, int.MaxValue);
        //Debug.Log(node);

        Prediction predi = null;
        foreach(PredictionTree son in node.sons)
        {
            if (son.value == maxValue)
            {
                predi = son.prediction;
                break;
            }
        }
        //Debug.Log(predi.board);
        //Debug.Log( "AlphaBeta : "+  predi.getAction(p));

        // On cherche alors la meilleur prédiction
        return predi;
    }

    public void Start()
    {
        Board b = new Board(true);
        List<Piece> pieces = b.getPiecesTab(Color.White);
        Debug.Log(System.DateTime.Now.ToString());
        Vector2Int v = b.findPieceByID(12).Value;
        predictionMove(b.grille[v.x, v.y], b);
        Debug.Log(System.DateTime.Now.ToString());
        EditorApplication.isPlaying = false;
    }
}
