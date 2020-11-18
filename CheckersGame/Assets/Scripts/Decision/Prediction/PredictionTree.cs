using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public enum ABType
{
    MIN, MAX
};

public class PredictionTree
{
    private bool isLeaf;
    public int value;
    public Prediction prediction;
    public ABType type;
    public List<PredictionTree> sons;

    //Creation si c'est un max ou un min
    public PredictionTree(ABType t, Prediction prediction)
    {
        isLeaf = false;
        type = t;
        this.prediction = prediction;
        value = 0;
        sons = new List<PredictionTree>();
    }

    // Création si c'est une feuille
    public PredictionTree(Prediction prediction, Color color)
    {
        isLeaf = true;
        type = ABType.MAX;
        this.prediction = prediction;
        value = prediction.board.getRank(color);
        sons = new List<PredictionTree>();
    }

    public void addSon(PredictionTree son)
    {
        sons.Add(son);
    }

    public void removeSon(string key)
    {
        //sons.Remove(key);
    }

    /* Informe si l'objet est un noeud ou une feuille*/
    public bool isNode()
    {
        return !isLeaf;
    }

    public IEnumerable getSons(Color color, int iD = 0, bool wantLeaf = false)
    {
        if (isLeaf)
        {
            throw new Exception("C'est une feuille");
        }
        else if (wantLeaf && type.Equals(ABType.MAX))
        {
            throw new Exception("Un noeud MAX ne peut pas être avoir de fils feuille");
        }
        else if (type.Equals(ABType.MIN)) // On calcul les mouvement possible de l'opposant
        {
            if (sons.Count == 0) // On calcul les fils
            {
                Color colorOpponent = (Color)(((int)color + 1) % 2);
                Dictionary<string, Prediction> dicOpponent = getAllBoardOpponent(prediction.board, colorOpponent);
                if (dicOpponent.Values.Count == 0)
                {
                    addSon(new PredictionTree(prediction, color));
                }
                else
                {
                    foreach (Prediction predi in dicOpponent.Values)
                    {
                        if (wantLeaf)// Generate Leaf 
                        {
                            addSon(new PredictionTree(predi, color));
                        }
                        else // 
                        {
                            addSon(new PredictionTree(ABType.MAX, predi));
                        }
                    }
                }
            }
            return ((IEnumerable)sons);
        }
        else // On calcul les mouvements possibles du player MAX
        {
            // on regarde s'il existe dans la prédiction la piece qui doit choisir son mouvement
            if (!prediction.board.findPieceByID(iD).HasValue)// On ne trouve pas la piece
            {
                addSon(new PredictionTree(prediction, color));
            }
            else if (sons.Count == 0)
            {
                foreach (Prediction predi in getAllBoardPossibleMove(iD, prediction.board, color).Values)
                {
                    addSon(new PredictionTree(ABType.MIN, predi));
                }
            }
            return ((IEnumerable)sons);
        }
    }
    public override string ToString()
    {
        string total = "";
        if (type.Equals(ABType.MAX))
        {

            total += $"MAX({value})\n";
        }
        else
        {
            total += $"MIN({value})\n";

        }
        foreach (PredictionTree son in sons)
        {
            if (son.isLeaf)
            {
                total += "Leaf";
            }
            if (son.type.Equals(ABType.MAX))
            {

                total += $"MAX({son.value})";
            }
            else
            {
                total += $"MIN({son.value})";

            }
        }
        return total;
    }

    /* Méthode pour la génération des plateau possible */
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
                        int afterTarDi = i >= 0 ? k : -k;
                        int afterTarDj = j >= 0 ? k : -k;
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

    //Retourne tous les déplacements que peut le pion fournie en paramètre
    public List<Action> getAvalaibleMove(Pion p, Board board)
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
        availableMove.Add(new Action(p.position));
        // On cherche les prises qui font avancer
        availableMove.AddRange(grapPossible(p, new Action(p.position), board));
        return availableMove;
    }

    //Retourne tous les déplacements que peut la tour fournie en paramètre
    public List<Action> getAvalaibleMove(Tower tower, Board board)
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
        availableMove.Add(new Action(tower.position));
        // On cherche les prises qui font avancer
        availableMove.AddRange(grapPossible(tower, new Action(tower.position), board));
        return availableMove;
    }

    //Fonction récursive qui calcul la listes des plateaux possibles pour le tour du joueur
    private Dictionary<string, Prediction> getAllBoardPossibleMove(
       int ID, Board initBoard, Color color)
    {
        // Le int le tour actuel de la piece donc la profondeur
        Dictionary<int, Dictionary<string, Prediction>> metaDic = new Dictionary<int, Dictionary<string, Prediction>>();

        // On innitialise le premier plateau
        Dictionary<string, Prediction> init = new Dictionary<string, Prediction>();

        init.Add(
            initBoard.GetHashString(),
            new Prediction(
                initBoard, color
            )
        );

        metaDic.Add(0, init);
        List<Piece> pieces = initBoard.getOrdonedListTeam(ID);
        int end = pieces.Count;
        int iteration = 0;
        do
        {
            Dictionary<string, Prediction> currentDic;
            if (metaDic.ContainsKey(iteration))
            {
                currentDic = metaDic[iteration];
            }
            else // On descend dans les tours
            {
                iteration++;
                continue;
            }
            List<string> tmpListKey = new List<string>(currentDic.Keys);
            foreach (string Key in tmpListKey) // On test les possibilité possible au tour suivant du board
            {
                if (!currentDic.ContainsKey(Key))
                {
                    continue;
                }
                Prediction current = currentDic[Key];
                Board board = current.board;
                Vector2Int? v = board.findPieceByID(pieces[iteration].id);
                if (v != null && v.HasValue != false)
                {
                    Piece piece = board.grille[v.Value.x, v.Value.y];
                    List<Action> la;
                    if (piece is Pion)
                    {
                        la = getAvalaibleMove((Pion)piece, board);
                    }
                    else
                    {
                        la = getAvalaibleMove((Tower)piece, board);
                    }
                    // On filtrer pour diminuer le nombre de mouve possible
                    piece.refreshGoal(board);
                    List<Action> tmpLA = piece.goal.filter(board, la);

                    la = new List<Action>();
                    var rand = new Random();
                    for (int i = 0; i < Math.Min(2, tmpLA.Count); ++i)
                    {
                        la.Add(tmpLA[rand.Next(0, tmpLA.Count)]); // On prend que deux moves
                    }
                    //Debug.Log("nombre d'action pour " + piece.id + " : " + la.Count);

                    //On charge le dictionnaire de l'index suivant
                    int next = (iteration + 1) % pieces.Count;
                    Dictionary<string, Prediction> nextBoards;
                    if (metaDic.ContainsKey(next))
                    {
                        nextBoards = metaDic[next];
                    }
                    else
                    {
                        nextBoards = new Dictionary<string, Prediction>();
                    }
                    // On parcour les actions possibles et on les réalisent
                    foreach (Action ac in la)
                    {
                        //Prediction tmp = new Prediction(board, ac);
                        Prediction tmp = current.extendPrediction(ac);
                        string hash = tmp.GetHashString();
                        if (!nextBoards.ContainsKey(hash)
                            && tmp.board.findPieceByID(ID) != null
                            && tmp.board.findPieceByID(ID).HasValue)
                        {
                            nextBoards.Add(hash, tmp);
                        }
                    }
                    metaDic[next] = nextBoards;
                }
            }
            iteration += 1;

        } while (iteration < end);

        if (end < pieces.Count && metaDic.ContainsKey(end))
        {
            return metaDic[end];
        }
        else
        {
            return metaDic[0];
        }
    }

    // Retour tous les plateaux possible prédictible de l'opposant
    private Dictionary<string, Prediction> getAllBoardOpponent(Board initBoard, Color colorOpponent)
    {
        Dictionary<string, Prediction> total = new Dictionary<string, Prediction>();
        List<Piece> tab = initBoard.getPiecesTab(colorOpponent);
        if (tab.Count == 0)
        {
            return new Dictionary<string, Prediction>();
        }
        int i = 0;
        foreach (Piece p in initBoard.getPiecesTab(colorOpponent))
        {
            if (i > 5)
            {
                break;
            }
            i++;
            Board virtualBoard = new Board(initBoard);
            p.predictions = new Selection();
            Dictionary<string, Prediction> result = getAllBoardPossibleMove(p.id, virtualBoard, p.color);
            foreach (string key in result.Keys)
            {
                if (!total.ContainsKey(key))
                {
                    total.Add(key, result[key]);
                }
            }
        }

        //Board virtualBoard = new Board(initBoard);
        //Piece p = tab[0];
        //p.predictions = new Selection();

        //Dictionary<string, Prediction> result = getAllBoardPossibleMove(p.id, virtualBoard, p.color);
        //foreach (string key in result.Keys)
        //{
        //    if (!total.ContainsKey(key))
        //    {
        //        total.Add(key, result[key]);
        //    }
        //}
        return total;
    }

}