using UnityEngine;
using System.Collections.Generic;

public class TargetingR : IResolver
{
/* Méthode principale de resolution de déplacement*/
    public Action resolve(Piece subject, Board board)
    {
        Action ac = new Action(subject.position);

        if (subject.GetType() == typeof(Pion))
        {
            ac = movePion((Pion)subject, board);
            //Debug.Log(ac);
        }
        else if (subject.GetType() == typeof(Tower))
        {
            ac = moveTower((Tower)subject, board);
            //Debug.Log(ac);
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
        availableMove.AddRange(grapPossible(p, new Action(p.position), board));
        //availableMove.Add(grabPieces(p, board));

        // On filtre les mouvements disponiblent
        p.refreshGoal(board);
        List<Action> selectMove = p.goal.filter(board, availableMove);

        // On choisie une avancé aleatoirement en répondant au mieux à l'objectif
        if (selectMove.Count != 0)
        {
            return selectMove.ToArray()[Random.Range(0, selectMove.Count)];
        }
        else if (availableMove.Count != 0) // On choisie un déplacement aléatoire
        {
            return availableMove.ToArray()[Random.Range(0, availableMove.Count)];
        }
        //Pas de déplacement possible
        return new Action(p.position);

    }

    //Liste les rafles de pion possible
    public List<Action> grapPossible(Pion pion, Action current, Board board)
    {
        List<Action> available = new List<Action>();
        for (int i = -1; i <= 1; i += 2)
        {
            for (int j = -1; j <= 1; j += 2)
            {
                if (j == i || j == -i)
                {
                    int xTarget = current.newPosition.x + i;
                    int yTarget = current.newPosition.y + j;
                    int xDest = current.newPosition.x + 2 * i;
                    int yDest = current.newPosition.y + 2 * j;

                    if (board.isCorrect(new Vector2Int(xTarget, yTarget))
                        && board.isCorrect(new Vector2Int(xDest, yDest))
                        && board.grille[xTarget, yTarget] != null
                        && board.grille[xDest, yDest] == null)
                    {
                        if (board.grille[xTarget, yTarget].GetType() == typeof(Pion) &&
                            board.grille[xTarget, yTarget].color != pion.color)
                        {
                            // on interdis de resauter sur un pion deja raflé
                            if (!current.alreadyCaptured(new Vector2Int(xTarget, yTarget)))
                            {
                                Action tmp = new Action(current);
                                tmp.newPosition = new Vector2Int(xDest, yDest);
                                tmp.addCapture(new Vector2Int(xTarget, yTarget));
                                available.Add(tmp); // On ajoute l'action contenant la capture dans la liste de actions possibles
                            }

                        }
                    }
                }
            }
        }
        // On regarde si on peut faire une nouvelle capture
        foreach (Action ac in available.ToArray())
        {
            available.AddRange(grapPossible(pion, new Action(ac), board));
        }
        return available;
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
        availableMove.AddRange(grapPossible(tower, new Action(tower.position), board));

        // On filtre les mouvements disponiblent
        tower.refreshGoal(board);
        List<Action> selectMove = tower.goal.filter(board, availableMove);

        // On choisie une avancé aleatoirement en répondant au mieux à l'objectif
        if (selectMove.Count != 0)
        {
            return selectMove.ToArray()[Random.Range(0, selectMove.Count)];
        }
        else if (availableMove.Count != 0) // On choisie un déplacement aléatoire
        {
            return availableMove.ToArray()[Random.Range(0, availableMove.Count)];
        }

        //Pas de déplacement possible
        return new Action(tower.position);

    }

    //Liste les rafles de tour possible
    public List<Action> grapPossible(Tower tower, Action current, Board board)
    {
        List<Action> availables = new List<Action>();

        int range = 2 * tower.height;

        for (int i = -range; i <= range; i++)
        {
            for (int j = -range; j <= range; j++)
            {
                if (j == i || j == -i)
                {

                    int xTarget = current.newPosition.x + i;
                    int yTarget = current.newPosition.y + j;
                    for (int k = 1; k <= range; ++k)
                    {
                        int afterTarDi = i>=0?k:-k;
                        int afterTarDj = j>=0?k:-k;
                        int xDest = current.newPosition.x + i + afterTarDi;
                        int yDest = current.newPosition.y + j + afterTarDj;

                        if (board.isCorrect(xTarget, yTarget) && board.grille[xTarget, yTarget] != null
                                && board.isCorrect(xDest, yDest) && board.grille[xDest, yDest] == null
                                && board.grille[xTarget, yTarget].color != tower.color //une tour peut rafler les pions et les tours
                                && !current.alreadyCaptured(new Vector2Int(xTarget, yTarget)) // on interdis de resauter sur une piece deja raflé
                                && tower.viewLine(xTarget, yTarget, board) // la tour doit avoir la ligne de vue sur la cible
                                && board.grille[xTarget, yTarget].viewLine(xDest, yDest, board)// doit avoir le champ libre pour se déplacer
                                )
                        {
                            if (
                                (board.grille[xTarget, yTarget].GetType() == typeof(Tower) && tower.canAttack((Tower)board.grille[xTarget, yTarget]))
                                || board.grille[xTarget, yTarget].GetType() == typeof(Pion))
                            {
                                Action tmp = new Action(current);
                                tmp.newPosition = new Vector2Int(xDest, yDest);
                                tmp.addCapture(new Vector2Int(xTarget, yTarget));
                                availables.Add(tmp); // On ajoute l'action contenant la capture dans la liste de actions possibles
                            }
                        }
                    }
                }
            }
        }
        // On regarde si on peut faire une nouvelle capture
        foreach (Action ac in availables.ToArray())
        {
            availables.AddRange(grapPossible(tower, new Action(ac), board));
        }
        return availables;
    }

}