using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Naive : IResolver
{
/* Méthode définie de l'interface */
    public Action resolve(Piece subject, Board board)
    {
        Action ac = new Action(subject.position);

        if (subject.GetType() == typeof(Pion))
        {

            ac = movePiece((Pion)subject, board);
            //on avance si bloqué on tente une rafle
            if (!ac.willMove())
            {
                ac = grabPieces((Pion)subject, board);
            }
        }
        else if (subject.GetType() == typeof(Tower))
        {

            ac = grabPieces((Tower)subject, board);
            if (!ac.willMove())
            {
                ac = moveTower((Tower)subject, board);
            }
        }
        return ac;
    }

    // Pas de prédiction avec ce resolver
    public void predicte(Piece subject, Board board) { }

    public void election(Piece subject, Board board) { }


    /* Méthode utilisé pour le résolveur*/
    // Déplacement des pions avec objectif d'aller dans le camp adverse
    public Action movePiece(Pion p, Board board)
    {
        List<Vector2Int> availableMove = new List<Vector2Int>();

        // On cherche les déplacements possibles
        for (int i = -1; i <= 1; i += 2)
        {
            for (int j = -1; j <= 1; j += 2)
            {
                int x = p.position.x + i;
                int y = p.position.y + j;
                if (x < board.size && x >= 0 && y < board.size && y >= 0 && board.grille[x, y] == null)
                {
                    availableMove.Add(new Vector2Int(i, j));
                }
            }
        }
        Action ac = new Action(p.position);

        // On prend un déplacement aleatoire si la piece ne peut pas se rapprocher du camps adverse
        if (availableMove.Count != 0)
        {
            // Le pion ne peux pas se déplacer dans la direction du camp adverse
            Vector2Int move = availableMove.ToArray()[Random.Range(0, availableMove.Count)];
            ac.newPosition = new Vector2Int(move.x + p.position.x, move.y + p.position.y);
            return ac;
        }

        //Pas de déplacement possible
        return ac;

    }

    public Action grabPieces(Pion pion, Board board)
    {
        List<Vector2Int[]> steps = new List<Vector2Int[]>();
        bool end = false;
        Vector2Int position = pion.position;
        Action ac = new Action(pion.position);

        // tant qu'on peux prendre un pion on actualise la position du pion
        // on ne peux pas sauter 2 fois sur le meme pions
        while (!end)
        {
            for (int i = -1; i <= 1; i += 2)
            {
                for (int j = -1; j <= 1; j += 2)
                {
                    int xTarget = position.x + i;
                    int yTarget = position.y + j;
                    int xDest = position.x + 2 * i;
                    int yDest = position.y + 2 * j;

                    if (xTarget < board.size && xTarget >= 0 && yTarget < board.size && yTarget >= 0 && board.grille[xTarget, yTarget] != null
                    && xDest < board.size && xDest >= 0 && yDest < board.size && yDest >= 0 && board.grille[xDest, yDest] == null)
                    {
                        if (board.grille[xTarget, yTarget].GetType() == typeof(Pion) && board.grille[xTarget, yTarget].color != pion.color)
                        {
                            // on interdis de resauter sur un pion deja raflé
                            if (!ac.alreadyCaptured(new Vector2Int(xTarget, yTarget)))
                            {
                                Vector2Int[] step = { new Vector2Int(xDest, yDest), new Vector2Int(xTarget, yTarget) };
                                steps.Add(step);
                            }
                        }
                    }
                }
            }

            // si plusieurs prise possibles pour cette rafle on en prend une aléatoire
            // on banis la position 
            if (steps.Count != 0)
            {
                int rand = Random.Range(0, steps.Count);
                Vector2Int[] move = steps.ToArray()[rand];

                //on banis le pions sur lequel on a sauté et on actualise la position
                ac.addCapture(move[1]);

                position.x = move[0].x;
                position.y = move[0].y;

                // on vide les moveùent du premier saut
                steps.Clear();
            }
            else end = true;
        }
        ac.newPosition = position;
        return ac;
    }


    public Action moveTower(Tower tower, Board board)
    {
        //déplacement de 2n sur les diagonales ET ligne de vue libre
        List<Vector2Int> availableMove = new List<Vector2Int>();
        Action ac = new Action(tower.position);
        int range = 2 * tower.height;
        for (int i = -range; i <= range; i++)
        {
            for (int j = -range; j <= range; j++)
            {
                if (j == i || j == -i)
                {
                    int x = tower.position.x + i;
                    int y = tower.position.y + j;
                    if (x < board.size && x >= 0 && y < board.size && y >= 0 && board.grille[x, y] == null)
                    {
                        //verifier que la ligne soit libre
                        if (tower.viewLine(x, y, board))
                        {
                            availableMove.Add(new Vector2Int(i, j));
                        }
                    }
                }
            }
        }

        if (availableMove.Count != 0)
        {
            Vector2Int move = availableMove.ToArray()[Random.Range(0, availableMove.Count)];
            ac.newPosition = new Vector2Int(move.x + tower.position.x, move.y + tower.position.y);
            return ac;
        }

        return ac;
    }

    public Action grabPieces(Tower tower, Board board)
    {
        List<Vector2Int[]> steps = new List<Vector2Int[]>();
        bool end = false;
        Vector2Int position = tower.position;
        Action ac = new Action(tower.position);
        int range = 2 * tower.height;
        //une tour peux prendre a 2n de distance et se retrouve 2n deriere sa cible 
        while (!end)
        {
            for (int i = -range; i <= range; i++)
            {
                for (int j = -range; j <= range; j++)
                {
                    if (j == i || j == -i)
                    {

                        int xTarget = position.x + i;
                        int yTarget = position.y + j;
                        for (int k = 1; k <= range; ++k)
                        {
                            int afterTarDi = i >= 0 ? k : -k;
                            int afterTarDj = j >= 0 ? k : -k;
                            int xDest = position.x + i + afterTarDi;
                            int yDest = position.y + j + afterTarDj;

                            if (board.isCorrect(xTarget,yTarget) && board.grille[xTarget, yTarget] != null
                             && board.isCorrect(xDest, yDest) && board.grille[xDest, yDest] == null)
                            {
                                //une tour peut rafler les pions et les tours
                                if (board.grille[xTarget, yTarget].color != tower.color)
                                {
                                    // on interdis de resauter sur une piece deja raflé
                                    if (!ac.alreadyCaptured(new Vector2Int(xTarget, yTarget)))
                                    {
                                        // la tour doit avoir la ligne de vue sur la cible et avoir le champ libre pour se déplacer
                                        if (tower.viewLine(xTarget, yTarget, board) && board.grille[xTarget, yTarget].viewLine(xDest, yDest, board))
                                        {
                                            if (board.grille[xTarget, yTarget].GetType() == typeof(Tower)
                                                && tower.canAttack((Tower)board.grille[xTarget, yTarget]))
                                            {
                                                Vector2Int[] step = { new Vector2Int(xDest, yDest), new Vector2Int(xTarget, yTarget) };
                                                steps.Add(step);
                                            }
                                            else if (board.grille[xTarget, yTarget].GetType() == typeof(Pion))
                                            {
                                                Vector2Int[] step = { new Vector2Int(xDest, yDest), new Vector2Int(xTarget, yTarget) };
                                                steps.Add(step);
                                            }

                                            //Debug.Log("[" + tower.position.x + "]" + "[" + tower.position.y + "] --> [" + xTarget + "]" + "[" + yTarget + "] -->[" + xDest + "]"
                                            //+ "[" + yDest + "]");

                                        }
                                    }
                                }
                            }

                        }

                    }
                }
            }

            if (steps.Count != 0)
            {
                
                int rand = Random.Range(0, steps.Count);
                Vector2Int[] move = steps.ToArray()[rand];

                //on banis la piece sur laquel on a sauté et on actualise la position
                ac.addCapture(move[1]);

                position.x = move[0].x;
                position.y = move[0].y;

                // on vide les mouvement du premier saut
                steps.Clear();

            }
            else end = true;
        }
        ac.newPosition = position;
        return ac;

    }

}