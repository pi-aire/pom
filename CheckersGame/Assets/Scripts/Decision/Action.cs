using UnityEngine;
using System.Collections.Generic;

// Une action représente l'action que réalise une piece sur le plateau de jeu
// Cela comprend la liste des autres pieces capturé et sa nouvelle position dans le plateau
// Par la suite on pourra ajouter des choses
public class Action
{
    /* Attribut de la classe de Action*/
    public Vector2Int newPosition; // la nouvelle position de la piece
    public Vector2Int oldPosition; // ancienne position de la piece
    public HashSet<Vector2Int> captured = new HashSet<Vector2Int>(); // Set des postions de piece capturé


    /* Constructeur de la classe Action*/
    public Action(Vector2Int old)
    {
        this.oldPosition = old;
        this.newPosition = old;
    }

    public Action(Vector2Int old, Vector2Int newp)
    {
        this.oldPosition = old;
        this.newPosition = newp;
    }

    public Action(Vector2Int old, Vector2Int newp, List<Vector2Int> targets)
    {
        this.oldPosition = old;
        this.newPosition = newp;
        this.captured = new HashSet<Vector2Int>(targets);
    }

    public Action(Action copy)
    {
        this.newPosition = new Vector2Int(copy.newPosition.x, copy.newPosition.y);
        this.oldPosition = new Vector2Int(copy.oldPosition.x, copy.oldPosition.y);
        this.captured = new HashSet<Vector2Int>(copy.captured);
    }

    /* Methode de la Classe Action*/
    // Ajoute des Pieces capturés
    public void addCaptures(List<Vector2Int> targets)
    {
        foreach (Vector2Int target in targets)
        {
            captured.Add(target); // Verifie en même temps que la Piece n'est pas déjà dans la liste des Pieces capturées
        }
    }

    // Ajoute une Piece capturé
    public void addCapture(Vector2Int target)
    {
        captured.Add(target); // Verifie en même temps que la Piece n'est pas déjà dans la liste des Pieces capturées
    }

    // Realise l'action
    public void doAction(Board board)
    {
        // On réalise le déplacement de la piece à l'origine de l'action
        Piece piece = board.grille[oldPosition.x, oldPosition.y];
        //Debug.Log(oldPosition + "  " + newPosition + " " + !oldPosition.Equals(newPosition));
        piece.hasMove(!oldPosition.Equals(newPosition));

        board.grille[oldPosition.x, oldPosition.y] = null;
        if (!piece.isAlive())
        {
            piece.setPosition(new Vector2Int(-1, -1));
            return;
        }
        board.grille[newPosition.x, newPosition.y] = piece;
        piece.setPosition(newPosition);
        // On enlève un point de vie au Piece capturé
        // Si il n'y a plus de vie on supprime la Piece
        foreach (Vector2Int cap in captured)
        {
            if (board.grille[cap.x, cap.y].GetType() == typeof(Pion))
            {
                board.grille[cap.x, cap.y].takeDamage(1);
            }
            else if (board.grille[cap.x, cap.y].GetType() == typeof(Tower))
            {
                int damage = ((Tower)board.grille[newPosition.x, newPosition.y]).calculatDamage((Tower)board.grille[cap.x, cap.y]);
                board.grille[cap.x, cap.y].takeDamage(damage);
            }
            if (!board.grille[cap.x, cap.y].isAlive())
            {
                board.grille[cap.x, cap.y] = null;
            }
        }
        // On regarde si on peut construire des Tours
        towerManager(board);
        // On regarde s'il y a une tour capturé
        towerCapture(board);
    }

    // Indique si la Piece va bouger si l'action est réaliser
    public bool willMove()
    {
        return !newPosition.Equals(oldPosition);
    }

    // Indique si on peut réaliser l'action ou une pice du même camp bloque le déplacement
    public bool canMove(Board board)
    {
        return board.grille[newPosition.x, newPosition.y] == null || !willMove();
    }

    // Indique si les coordonées de la piece n'est pas déjà raflé
    public bool alreadyCaptured(Vector2Int position)
    {
        return captured.Contains(position);
    }

    // Verification si on ne peux pas transformer le pion en Tour ou augmenter la taille de la tour présent sur la tour
    public void towerManager(Board board)
    {
        Piece piece = board.grille[newPosition.x, newPosition.y];
        if (piece.GetType() == typeof(Pion))
        {
            int x;
            if (piece.color == Color.White && newPosition.x == 9)
            {
                x = 9;
            }
            else if (piece.color == Color.Black && newPosition.x == 0)
            {
                x = 0;
            }
            else
            {
                return;
            }

            Vector2Int intresting = new Vector2Int();
            bool find = false;
            for (int j = 0; j < board.size; ++j)
            {
                if (j != newPosition.y
                    && board.grille[x, j] != null
                    && board.grille[x, j].color == board.grille[newPosition.x, newPosition.y].color)
                {
                    if (board.grille[x, j].GetType() == typeof(Pion))
                    {
                        intresting = board.grille[x, j].position;
                        find = true;
                    }
                    else if (board.grille[x, j].GetType() == typeof(Tower))
                    {
                        Tower t = (Tower)board.grille[x, j];
                        t.upgrade();//on ajoute 1 à la hauteur de la tour déjà existance
                        board.grille[x, j] = t;
                        board.grille[newPosition.x, newPosition.y] = null;
                        return;
                    }
                }
            }
            if (find)
            {
                Piece tmp = board.grille[intresting.x, intresting.y];
                board.grille[intresting.x, intresting.y] = new Tower(tmp.id, intresting.x, intresting.y, tmp.color);
                board.grille[newPosition.x, newPosition.y] = null;
            }
        }
    }

    // Verification si une dés tour n'a pas été capturé
    public void towerCapture(Board board)
    {
        List<Piece> list = board.findAll(typeof(Tower));
        foreach (Piece p in list)
        {
            ((Tower)p).captured(board);
        }
    }

    /*Méthode de convertion*/
    // Convertie une action en un plateau
    public Board toBoard(Board board)
    {
        Board virtualBoard = new Board(board);
        doAction(virtualBoard);
        return virtualBoard;
    }

    // Convertie une action en action générique
    public GenericAction toGenericAction(Board board, Vector2Int context, Camp c)
    {
        return new GenericAction(board, context, this, c);
    }

    /*Méthode implémenter des classes herité*/
    public override string ToString()
    {
        string s = "[ " + oldPosition.ToString() + "=> ";
        foreach (Vector2Int v in captured)
        {
            s += v.ToString() + "=> ";
        }
        s += newPosition + " ]";
        return s;
    }
}