using UnityEngine;
using System.Collections.Generic;

public class Targeting : IResolver
{
/* Méthode de l'interface */
    public Action resolve(Piece subject, Board board)
    {
        Action ac = new Action(subject.position);

        if (subject.GetType() == typeof(Pion))
        {
            ac = movePion((Pion)subject, board);
        }
        else if (subject.GetType() == typeof(Tower))
        {
            ac = moveTower((Tower)subject, board);
        }

        return ac;
    }

    // Pas de prédiction avec ce resolver
    public void predicte(Piece subject, Board board) { }

    public void election(Piece subject, Board board) { }


    /* Méthode sur le déplacement des Pions */

    // Déplacement des pions avec objectif d'aller dans le camp adverse
    public Action movePion(Pion p, Board board)
    {
        List<Action> availableMove = new List<Action>();

        // On cherche les déplacements possibles
        for (int i = -1; i <= 1; i += 2)
        {
            for (int j = -1; j <= 1; j += 2)
            {
                int x = p.position.x + i;
                int y = p.position.y + j;
                if (x < board.size && x >= 0 && y < board.size && y >= 0 && board.grille[x, y] == null)
                {
                    availableMove.Add(new Action(p.position, new Vector2Int(x, y)));
                }
            }
        }
        // On cherche les prises qui font avancer
        availableMove.AddRange(grabPieces(p,board));
        //availableMove.Add(grabPieces(p, board));

        // On filtre les mouvements disponiblent
        p.refreshGoal(board);
        List<Action> selectMove = p.goal.filter(board,availableMove);

        // On choisie une avancé aleatoirement
        if (selectMove.Count != 0)
        {
            return selectMove.ToArray()[Random.Range(0, selectMove.Count)];
        }

        //Pas de déplacement possible
        return new Action(p.position);

    }

    //Liste les actions possible pour un pion
    public List<Action> grabPieces(Pion pion, Board board)
    {
        List<Action> availableMoves = new List<Action>();
        List<Vector2Int[]> steps = new List<Vector2Int[]>();
        Queue<Vector2Int> todo = new Queue<Vector2Int>();
        List<Vector2Int> banned = new List<Vector2Int>();
        //bool end = false;
        Vector2Int position = pion.position;
        Action ac = new Action(pion.position);

        // tant qu'on peux prendre un pion on actualise la position du pion
        // on ne peux pas sauter 2 fois sur le meme pions
        todo.Enqueue(pion.position);
        while (todo.Count > 0)
        {
            position = todo.Dequeue();

            for (int i = -1; i <= 1; i += 2)
            {
                for (int j = -1; j <= 1; j += 2)
                {
                    if (j == i || j == -i)
                    {
                        int xTarget = position.x + i;
                        int yTarget = position.y + j;
                        int xDest = position.x + 2 * i;
                        int yDest = position.y + 2 * j;
                        // TODO: Penser à utiliser la fonction isCorrect pour savoir si le vecteur se trouve dans le board
                        if (xTarget < board.size && xTarget >= 0 && yTarget < board.size && yTarget >= 0 && board.grille[xTarget, yTarget] != null
                        && xDest < board.size && xDest >= 0 && yDest < board.size && yDest >= 0 && board.grille[xDest, yDest] == null)
                        {
                            if (board.grille[xTarget, yTarget].GetType() == typeof(Pion) && board.grille[xTarget, yTarget].color != pion.color)
                            {
                                // on interdis de resauter sur un pion deja raflé
                                if (!ac.alreadyCaptured(new Vector2Int(xTarget, yTarget)))
                                {
                                    //Debug.Log("[" + position.x + "]" + "[" + position.y + "] --> [" + xTarget + "]" + "[" + yTarget + "] -->[" + xDest + "]"
                                    //    + "[" + yDest + "]");
                                    Vector2Int[] step = { new Vector2Int(xDest, yDest), new Vector2Int(xTarget, yTarget) };
                                    steps.Add(step);
                                }
                            }
                        }
                    }
                }
            }

            // si plusieurs prise possibles pour cette rafle on en prend une aléatoire
            // on banis la position 
            if (steps.Count != 0)
            {

                foreach (Vector2Int[] m in steps)
                {
                    if (!banned.Contains(m[0]))
                    {
                        banned.Add(m[0]);
                        todo.Enqueue(m[0]);
                        ac.newPosition = m[0];
                        ac.addCapture(m[1]);
                    }
                }
                // on vide les moveùent du premier saut
                steps.Clear();
                availableMoves.Add(ac);

            }
            Debug.Log("Loop grab piece targeting");


        }

        return availableMoves;
    }

/* Méthode sur le déplacement des Tours */

    // Déplacement des pions avec objectif d'aller dans le camp adverse
    public Action moveTower(Tower tower, Board board)
    {
        List<Action> availableMove = new List<Action>();

        // On cherche les déplacements possibles
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
                            availableMove.Add(new Action(tower.position, new Vector2Int(x, y)));
                        }
                    }
                }
            }
        }
        // On cherche les prises qui font avancer
        //availableMove.AddRange(grapPossible(tower, new Action(tower.position), board));
        availableMove.AddRange(grabPieces(tower,board));

        // On filtre les mouvements disponiblent
        tower.refreshGoal(board);
        List<Action> selectMove = tower.goal.filter(board, availableMove);

        // On choisie une avancé aleatoirement
        if (selectMove.Count != 0)
        {
            return selectMove.ToArray()[Random.Range(0, selectMove.Count)];
        }

        //Pas de déplacement possible
        return new Action(tower.position);

    }
    
    // Liste les actions possible pour une tour
    public List<Action> grabPieces(Tower tower, Board board)
    {
        List<Action> availableMoves = new List<Action>();
        Queue<Vector2Int> todo = new Queue<Vector2Int>();
        List<Vector2Int> banned = new List<Vector2Int>();
        List<Vector2Int[]> steps = new List<Vector2Int[]>();

        Vector2Int position = tower.position;
        Action ac = new Action(tower.position);
        int range = 2 * tower.height;
        //une tour peux prendre a 2n de distance et se retrouve 2n deriere sa cible 
        todo.Enqueue(tower.position);
        while (todo.Count > 0)
        {
            position = todo.Dequeue();
            for (int i = -range; i <= range; i++)
            {
                for (int j = -range; j <= range; j++)
                {
                    if (j == i || j == -i)
                    {

                        int xTarget = position.x + i;
                        int yTarget = position.y + j;
                        int xDest = position.x + 2 * i; // TODO ERREUR
                        int yDest = position.y + 2 * j; // TODO Erreur

                        if (xTarget < board.size && xTarget >= 0 && yTarget < board.size && yTarget >= 0 && board.grille[xTarget, yTarget] != null
                             && xDest < board.size && xDest >= 0 && yDest < board.size && yDest >= 0 && board.grille[xDest, yDest] == null)
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

            if (steps.Count != 0)
            {
                foreach (Vector2Int[] m in steps)
                {
                    if (!banned.Contains(m[0]))
                    {
                        banned.Add(m[0]);
                        todo.Enqueue(m[0]);
                        ac.newPosition = m[0];
                        ac.addCapture(m[1]);
                    }
                }
                // on vide les moveùent du premier saut
                steps.Clear();
                availableMoves.Add(ac);
            }
        }
        return availableMoves;
    }

}