using System;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;

public enum Camp
{
    ALLY, OPPONENT
}

[Serializable]
public class GenericPrediction
{
    public HashSet<GenericAction> actions; //liste des actions
    public int gain; // informe sur la se que peut rapporter la prediction
    public int score; // Score de la prédiction
    /*Constructeur*/
    /*
     Pour le constructueur on prend en parametrer la couleur du point qui est à l'origine de la prédiction,
     avec cela on en déduit les allier et les opposants
         */
    public GenericPrediction(Prediction prediction, Color color, Board innitial, Vector2Int context)
    {
        actions = new HashSet<GenericAction>();
        foreach (KeyValuePair<int, Action> pair in prediction.actions)
        {
            Vector2Int tmp = pair.Value.oldPosition;
            if (innitial.grille[tmp.x, tmp.y].color == color)
            {
                actions.Add(pair.Value.toGenericAction(innitial, context, Camp.ALLY));
            }
            else
            {
                actions.Add(pair.Value.toGenericAction(innitial, context, Camp.OPPONENT));
            }
        }
        score = 0; // Score de départ 
        gain = prediction.board.getRankNoBurn(color) - prediction.innitialRank;
    }
    public GenericPrediction() { }

    // Informe si on peut appliquer le prédicat générique sur le plateau actuel
    public Tuple<int, Prediction> toPrediciton(Board current, Vector2Int context)
    {
        bool possible = true;
        Prediction predi = new Prediction(current, current.grille[context.x, context.y].color);


        foreach (GenericAction gac in actions)
        {
            Action tmp;
            try
            {
                tmp = gac.toAction(current, context); // On remet du context à l'action générique
            }
            catch
            {
                throw new Exception("La prédiction générique n'est pas applicable");
            }

            if (possible)
            {
                predi = predi.extendPrediction(tmp);
            }
            else
            {
                break;
            }
        }
        if (possible)
        {
            //Debug.Log("On a trouvé une resultat");
            return new Tuple<int, Prediction>(gain, predi);
        }
        else
        {
            throw new Exception("La prédiction générique n'est pas applicable");
        }
    }
    public override string ToString()
    {
        return "Générique prédiction gain : " + gain;
    }
}