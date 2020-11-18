using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

public class DataBase
{
    private const string FILE_PATH = "./Assets/Data/model.xml";
    public List<GenericPrediction> data;

    public DataBase()
    {
        load();
    }

    /*Gestion du fichier de sauvegarde*/
    public void save()
    {
        List<int> toRemove = new List<int>();
        for (int i = 0; i < data.Count; ++i)
        {
            if (data[i].score < -1)
            {
                toRemove.Add(i);
            }
        }
        foreach (int i in toRemove)
        {
            data.RemoveAt(i);
        }

        XmlSerializer writer = new XmlSerializer(typeof(List<GenericPrediction>));
        StreamWriter wfile = new StreamWriter(FILE_PATH);
        writer.Serialize(wfile, data);
        wfile.Close();
    }

    // On charge le fichier
    public void load()
    {
        try
        {
            XmlSerializer reader = new XmlSerializer(typeof(List<GenericPrediction>));
            StreamReader file = new System.IO.StreamReader(FILE_PATH);
            data = (List<GenericPrediction>)reader.Deserialize(file);
            file.Close();
        }
        catch
        {
            data = new List<GenericPrediction>();
        }
    }

    /* Methode de la gestion de la base de données*/
    public void Add(Prediction predi, Color colorPiece, Board innitial, Vector2Int context)
    {
        if (predi.actions.Count != 0)
        {
            data.Add(predi.toGeneric(context, colorPiece, innitial));
            //save();
        }
    }

    /* Note d'un prédicat générique*/
    public void Evaluate(int id, bool conclusive)
    {
        if (conclusive)
        {
            data[id].score += 4;
        }
        else
        {
            data[id].score -= 1;
        }
    }

    /* Rotation des prédictions*/
    public GenericPrediction rotation(bool xInverse, bool yInverse)
    {
        return null;
    }

    /* Methode de recherche des prédicats*/
    public Tuple<int, Prediction> Find(Board current, Vector2Int context)
    {
        Prediction match = null;
        int find = -1;
        int max = int.MinValue;

        foreach (GenericPrediction gp in data)
        {
            try
            {
                Tuple<int, Prediction> tmp = gp.toPrediciton(current, context);
                if (max < tmp.Item1 && gp.score > -1)
                {
                    max = tmp.Item1;
                    match = tmp.Item2;
                    find = data.IndexOf(gp);
                }
            }
            catch { }
        }
        Debug.Log("Find " + find + ", Maximum " + max);
        if (max < 0)
        {
            match = null;
            find = -1;
        }
        else if (find >= 0)
        {
            Debug.Log("Database Gain : " + max);
            match.idG = find;
        }
        return new Tuple<int, Prediction>(find, match);
    }
}
