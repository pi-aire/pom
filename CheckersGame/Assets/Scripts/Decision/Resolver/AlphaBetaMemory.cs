using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
//public class PossibilityTree : MonoBehaviour, IResolver

public class AlphaBetaMemory: MonoBehaviour, IResolver
{
    int smart = 1;
    DataBase data;//contient toute les données
    public AlphaBetaMemory(DataBase data, int ismart = 1)
    {
        smart = ismart;
        this.data = data;
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
        Debug.Log("predi id find " + predi.idG );
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

        Tuple<int,Prediction> find = data.Find(virtualBoard, p.position);

        // On n'a pas trouvé dans la base un déplacement
        Prediction result = find.Item2;
        if (find.Item1 == -1)
        {
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
            //Debug.Log("Compute:" + predi.getAction(p));
            // On sauvegarde le prédicat dans la base de données
            data.Add(predi, p.color, virtualBoard,p.position);

            result = predi;
        }
        else
        {
            Debug.Log("Database:" + result.getAction(p));
        }
        // On cherche alors la meilleur prédiction
        return result;
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
