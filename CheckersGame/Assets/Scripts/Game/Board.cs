using System;
using System.Collections.Generic;
using UnityEngine;


public class Board
{
/* Attribut de la classe Board */
    public Piece[,] grille;
    public int size = 10;
 
    /* Constructeur de la classe Board */
    // Constructeur qui innitialise le tableau avec ou sans les pieces
    public Board(bool input)
    {
        if (input)
        {
            initBoard();
        }
        else
        {
            grille = new Piece[10, 10];
        }
    }
    // Constructeur par copie
    public Board(Board copy)
    {
        size = copy.size;
        grille = new Piece[size, size];
        for (int i = 0; i < this.size; ++i)
        {
            for (int j = 0; j < this.size; ++j)
            {
                if(copy.grille[i, j] != null)
                {
                    grille[i, j] = copy.grille[i, j].clone();
                }
                else
                {
                    grille[i, j] = null;
                }
            }
        }
    }

/* Méthode de la classe Board*/
    // Remplie le tableau en plaçant les pieces au position d'innitialisation du jeu de Dames
    public void initBoard()
    {
        grille = new Piece[10,10];
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)    
            {
                    if (i % 2 == 0 && j % 2 == 0)
                    {
                        if(i < 3)
                        {
                            grille[i,j] = new Pion(i, j, Color.White);
                        } else if(i > 6)
                        {
                            grille[i,j] = new Pion(i, j, Color.Black);
                        }         
                    }
                    else if (i % 2 == 1 && j % 2 == 1)
                    {
                        if (i < 3)
                        {
                            grille[i,j] = new Pion(i, j, Color.White);
                        }
                        else if (i > 6)
                        {
                            grille[i,j] = new Pion(i, j, Color.Black);
                        }
                    }
            }
        }
    }

    // retourne le nombre de piece d'une couleur sur le plateau
    public int count(Color c)
    {
        int count = 0;
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                if (grille[i, j] != null && grille[i, j].color == c)
                {
                    count++;
                }
            }
        }
        return count;
    }

    // Cherche une Piece de type 'type' de la couleur donné en paramètre
    public Tuple<int,Vector2Int> findOnePiece(Type type, Piece agent)
    {
        List<Vector2Int> list = new List<Vector2Int>(); 
        for (int i = 0; i< size; ++i)
        {
            for(int j= 0; j< size; ++j)
            {
                if (grille[i, j] != null 
                    && grille[i, j].GetType() == type
                    && grille[i, j].color.Equals((Color)(((int) agent.color + 1) % 2)))
                {
                    list.Add(new Vector2Int(grille[i, j].position.x, grille[i, j].position.y));
                }
            }
        }
        if (list.Count > 0)
        {
            int minDist = -1;
            Vector2Int min = new Vector2Int();
            foreach (Vector2Int vec in list)
            {
                if (minDist == -1 
                    || minDist > agent.distance(grille[vec.x, vec.y], this))
                {
                    minDist = agent.distance(grille[vec.x, vec.y], this);
                    min = vec;
                }
            }
            return new Tuple<int, Vector2Int>(grille[min.x, min.y].id, min);
        }
        else
        {
            return new Tuple<int, Vector2Int>(-1, new Vector2Int(-1, -1));
        }
    }

    // Cherche une piece qui possède l'id donnée en paramètre
    public Vector2Int? findPieceByID(int id)
    {
        for (int i = 0; i < size; ++i)
        {
            for (int j = 0; j < size; ++j)
            {
                if (grille[i, j] != null
                    && grille[i, j].id == id)
                {
                    return new Vector2Int(grille[i, j].position.x, grille[i, j].position.y);
                }
            }
        }
        return null;
    }

    //Trouve toute les pieces de type type
    public List<Piece> findAll(Type type)
    {
        List<Piece> list = new List<Piece>();
        for (int i = 0; i < size; ++i)
        {
            for (int j = 0; j < size; ++j)
            {
                if (grille[i, j] != null
                    && grille[i, j].GetType() == type)
                {
                    list.Add(grille[i, j]);
                }
            }
        }
        return list;
    }

    //Trouve toute les pieces de type type et de couleur couleur
    public List<Piece> findAll(Type type, Color color)
    {
        List<Piece> list = new List<Piece>();
        for (int i = 0; i < size; ++i)
        {
            for (int j = 0; j < size; ++j)
            {
                if (grille[i, j] != null
                    && grille[i, j].GetType() == type
                    && grille[i, j].color.Equals(color))
                {
                    list.Add(grille[i, j]);
                }
            }
        }
        return list;
    }

    // Indique si les coordonnées sont dans le palteau
    public bool isCorrect(Vector2Int p)
    {
        return p.x < size && p.y < size && p.x >= 0 && p.y >= 0;
    }
    
    // Indique si les coordonnées sont dans le palteau
    public bool isCorrect(int x, int y)
    {
        return isCorrect(new Vector2Int(x, y));
    }

    // Indique si les coordonnées pointent sur une Piece
    public bool existe(Vector2Int p)
    {
        if (isCorrect(p))
        {
            return grille[p.x, p.y] != null;
        }
        return false;
    }

    // Donne la distance entre deux vecteurs
    public int distance(Vector2Int v1, Vector2Int v2)
    {
        return Mathf.Abs(v1.x - v2.x)
            + Mathf.Abs(v1.y - v2.y);
    }

    // Distance d'une ligne
    public int distanceEdge(Vector2Int v, int x)
    {
        return Mathf.Abs(v.x - x);
    }

    // Retourne la valeur du plateau
    public int getRank(Color color)
    {
        int total = 0;
        int totalOpponante = 0;
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                if (grille[i, j] != null)
                {
                    if (grille[i, j].color.Equals(color))
                    {
                        total += grille[i, j].getRank();
                    }
                    else
                    {
                        totalOpponante += grille[i, j].getRank();
                    }
                }
            }
        }
        int scoreTower = 7 * (findAll(typeof(Tower), (Color)(((int)color +1) % 2)).Count); // A regarder
        return total + (30+(Burn.value * 15) + scoreTower - totalOpponante); 
    }

    // Retourne la valeur du plateau
    public int getRankNoBurn(Color color)
    {
        int total = 0;
        int totalOpponante = 0;
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                if (grille[i, j] != null)
                {
                    if (grille[i, j].color.Equals(color))
                    {
                        total += grille[i, j].getRankNoBurn();
                    }
                    else
                    {
                        totalOpponante += grille[i, j].getRankNoBurn();
                    }
                }
            }
        }
        int scoreTower = 7 * (findAll(typeof(Tower), (Color)(((int)color + 1) % 2)).Count); // A regarder
        return total + (30 + scoreTower - totalOpponante);
    }

    // Iterateur sur les pieces d'une couleur
    public List<Piece> pieces(Color color)
    {
        List<Piece> pieces = new List<Piece>();
        for (int i = 0; i < size; ++i)
        {
            for (int j = 0; j < size; ++j)
            {
                if (grille[i, j] != null
                    && grille[i, j].color == color)
                {
                    pieces.Add(grille[i, j]);
                }
            }
        }
        return pieces;
    }

    // Retour un tableau de Piece d'une couleur
    public List<Piece> getPiecesTab(Color color)
    {
        List<Piece> pieces = new List<Piece>();
        for (int i = 0; i < size; ++i)
        {
            for (int j = 0; j < size; ++j)
            {
                if (grille[i, j] != null
                    && grille[i, j].color == color)
                {
                    pieces.Add(grille[i, j]);
                }
            }
        }
        return pieces;
    }

    // /!\ /!\ /!\ /!\ /!\ /!\ /!\ /!\ /!\ /!\ /!\ /!\
    // Critique /!\
    // Retourne toutes les pièces en partent d'un id précis
    // /!\ /!\ /!\ /!\ /!\ /!\ /!\ /!\ /!\ /!\ /!\ /!\
    public List<Piece> getOrdonedList(int iD)
    {
        Vector2Int init = findPieceByID(iD).Value;
        Color color = grille[init.x, init.y].color;

        Piece[] piecesTAB = getPiecesTab(color).ToArray();
        List<Piece> pieces = new List<Piece>();

        for(int i = 0; i < piecesTAB.Length; ++i)
        {
            for (int j = 0; j < piecesTAB.Length; ++j)
            {
                if (piecesTAB[i].id < piecesTAB[j].id)
                {
                    Piece tmpP = piecesTAB[j];
                    piecesTAB[j] = piecesTAB[i];
                    piecesTAB[i] = tmpP;
                }
            }
        }
        int position = 0;
        for (int i = 0; i< piecesTAB.Length;++i)
        {
            if (piecesTAB[i].id == iD)
            {
                position = i;
            }
        }
        
        for (int i =  position; i < piecesTAB.Length; ++i)
        {
            pieces.Add(piecesTAB[i]);
        }
        for (int i = 0; i < position; ++i)
        {
            pieces.Add(piecesTAB[i]);
        }

        ////Liste de pion adverse
        for (int j = 0; j < 15; ++j)
        {
            Vector2Int? pos;
            if (color.Equals(Color.Black))
            {
                pos = findPieceByID(14 - j); // recuperation des pieces Blanche
            }
            else
            {
                pos = findPieceByID(j + 15);
            }
            if (pos.HasValue)
            {
                pieces.Add(grille[pos.Value.x, pos.Value.y]); // recuperation des pieces Noir
            }
        }
        return pieces;
    
    }
    // retour la liste des PIeces de la team de manière ordonné
    public List<Piece> getOrdonedListTeam(int iD)
    {
        Vector2Int init = findPieceByID(iD).Value;
        Color color = grille[init.x, init.y].color;

        Piece[] piecesTAB = getPiecesTab(color).ToArray();
        List<Piece> pieces = new List<Piece>();

        for (int i = 0; i < piecesTAB.Length; ++i)
        {
            for (int j = 0; j < piecesTAB.Length; ++j)
            {
                if (piecesTAB[i].id < piecesTAB[j].id)
                {
                    Piece tmpP = piecesTAB[j];
                    piecesTAB[j] = piecesTAB[i];
                    piecesTAB[i] = tmpP;
                }
            }
        }
        int position = 0;
        for (int i = 0; i < piecesTAB.Length; ++i)
        {
            if (piecesTAB[i].id == iD)
            {
                position = i;
            }
        }

        for (int i = position; i < piecesTAB.Length; ++i)
        {
            pieces.Add(piecesTAB[i]);
        }
        for (int i = 0; i < position; ++i)
        {
            pieces.Add(piecesTAB[i]);
        }
        return pieces;

    }

    /* Methode implémenté*/
    public override string ToString()
    {
        string tab = "";
        for (int i = 0; i < this.size; ++i)
        {
            tab += "|";
            for (int j = 0; j < this.size; ++j)
            {
                if (this.grille[i, j] != null)
                {
                    tab += this.grille[i, j] +" |";
                }
                else
                {
                    tab += "   |";
                }
            }
            tab += "\n";
        }
        return tab;
    }

    public string GetHashString()
    {
        string hash = "";
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                if (grille[i, j] != null)
                {
                    hash += grille[i, j].getRank();
                }
                else
                {
                    hash += 0;
                }
            }
        }
        return hash;
    }

    public override int GetHashCode()
    {
        // Création d'un plateau de jeu simplifié
        int[,] sumGrille = new int[size, size];

        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                if (grille[i, j] != null)
                {
                    sumGrille[i,j] = grille[i, j].getRank();
                }
            }
        }
        return sumGrille.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        if (obj is Board)
        {
            return GetHashString().Equals(((Board)obj).GetHashString());

        }
        return base.Equals(obj);
    }
}
