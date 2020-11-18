using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
//public class PossibilityTree : MonoBehaviour, IResolver

public class PossibilityTree : MonoBehaviour, IResolver
{
    int smart = 3;
    public PossibilityTree(int ismart)
    {
        smart = ismart;
    }

    public PossibilityTree()
    {

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

/* Methode pour l'exploration des solutions possible avec les anticipations*/
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

    //Fonction récursive qui calcul la listes des plateaux possibles 
    /*  private void getAllBoardPossible(int index, int end, Board board, Dictionary<int, Board> result)
      {
          index = index % 30;
          if (index != end || result.Count == 0)
          {
              Vector2Int? v = board.findPieceByID(index);
              if (v.HasValue == false)
              {
                  getAllBoardPossible(index + 1, end, board, result);
              }
              else
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

                  foreach (Action ac in la)
                  {
                      Board tmp = ac.toBoard(board);
                      int hash = tmp.GetHashCode();
                      if (!result.ContainsKey(hash))
                      {
                          result.Add(hash, tmp);
                          getAllBoardPossible(index + 1, end, tmp, result);
                      }
                  }
              }
          }
      }*/
    private Dictionary<string, Prediction> getAllBoardPossible(
        int index, int end, Board board, bool start)
    {
        //Debug.Log(index + ":" + end);
        index = index % 30;
        // Cas d'arret
        if (index == end && start == false)
        {
            Dictionary<string, Prediction> dic = new Dictionary<string, Prediction>();

            Vector2Int vectmp = board.findPieceByID(index).Value;
            Prediction tmp = new Prediction(board, board.grille[vectmp.x,vectmp.y].color);
            return dic;
        }
        else // Recurtion
        {
            Vector2Int? v = board.findPieceByID(index);
            if (v == null || v.HasValue == false)
            {
                return getAllBoardPossible(1 + index, end, board, false);
            }
            else
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
                //Debug.Log("nombre d'action pour " + piece.id + " : " + la.Count);

                // On parcour les actions possibles et on les réalisent
                Dictionary<string, Prediction> dic = new Dictionary<string, Prediction>();
                foreach (Action ac in la)
                {
                    Prediction tmp = new Prediction(board, board.grille[ac.oldPosition.x, ac.oldPosition.y].color);
                    string hash = tmp.GetHashString();

                    if (!dic.ContainsKey(hash))
                    {
                        dic.Add(hash, tmp);
                        Dictionary<string, Prediction> tmpdic = getAllBoardPossible(1 + index, end, tmp.board, false);
                        foreach (KeyValuePair<string, Prediction> entry in tmpdic)
                        {
                            if (!dic.ContainsKey(entry.Key))
                            {
                                dic.Add(entry.Key, entry.Value);
                            }
                        }
                    }
                }
                return dic;
            }
        }
        
    }

    //private Dictionary<string, Prediction> getAllBoardPossibleIte(
    //int start, int end, Board initBoard)
    //{
    //    int index = 0 + start;
    //    // Le int le tour actuel de la piece donc la profondeur
    //    Dictionary<int, Dictionary<string, Prediction>> metaDic = new Dictionary<int, Dictionary<string, Prediction>>();

    //    // On innitialise le premier plateau
    //    Dictionary<string, Prediction> init = new Dictionary<string, Prediction>();

    //    init.Add(
    //        initBoard.GetHashString(),
    //        new Prediction(
    //            initBoard,
    //            new Action(initBoard.findPieceByID(start).Value)
    //        )
    //    );

    //    metaDic.Add(index, init);

    //    do
    //    {
    //        Dictionary<string, Prediction> currentDic;
    //        if (metaDic.ContainsKey(index))
    //        {
    //            currentDic = metaDic[index];
    //        }
    //        else // On descend dans les tours
    //        {
    //            index = (index + 1) % 30;
    //            continue;
    //        }

    //        foreach (KeyValuePair<string, Prediction> current in currentDic) // On test les possibilité possible au tour suivant du board
    //        {
    //            Board board = current.Value.board;
    //            Vector2Int? v = board.findPieceByID(index);
    //            if (v != null && v.HasValue != false)
    //            {
    //                Piece piece = board.grille[v.Value.x, v.Value.y];
    //                List<Action> la;
    //                if (piece is Pion)
    //                {
    //                    la = getAvalaibleMove((Pion)piece, board);
    //                }
    //                else
    //                {
    //                    la = getAvalaibleMove((Tower)piece, board);
    //                }
    //                piece.refreshGoal(board);
    //                la = piece.goal.filter(board, la);
    //                //Debug.Log("nombre d'action pour " + piece.id + " : " + la.Count);

    //                //On charge le dictionnaire de l'index suivant
    //                int next = (index + 1) % 30;
    //                Dictionary<string, Prediction> nextBoards;
    //                if (metaDic.ContainsKey(next))
    //                {
    //                    nextBoards = metaDic[next];
    //                }
    //                else
    //                {
    //                    nextBoards = new Dictionary<string, Prediction>();
    //                }
    //                // On parcour les actions possibles et on les réalisent
    //                foreach (Action ac in la)
    //                {
    //                    Prediction tmp = new Prediction(board, ac);
    //                    string hash = tmp.GetHashString();
    //                    string symetric = tmp.GetHashStringSymetrie();
    //                    if (!nextBoards.ContainsKey(hash)
    //                        && !nextBoards.ContainsKey(symetric)
    //                        && tmp.board.findPieceByID(start) != null
    //                        && tmp.board.findPieceByID(start).HasValue)
    //                    {
    //                        nextBoards.Add(hash, tmp);
    //                    }
    //                }
    //                metaDic[next] = nextBoards;
    //            }
    //        }
    //        index = (index + 1) % 30;
    //    }
    //    while (index != end);
    //    if (metaDic.ContainsKey(end))
    //    {
    //        return metaDic[end];
    //    }
    //    else
    //    {
    //        return metaDic[start];
    //    }
    //}
    private Dictionary<string, Prediction> getAllBoardPossibleIte(
    int ID, int end, Board initBoard)
    {
        // Le int le tour actuel de la piece donc la profondeur
        Dictionary<int, Dictionary<string, Prediction>> metaDic = new Dictionary<int, Dictionary<string, Prediction>>();
        
        // On innitialise le premier plateau
        Dictionary<string, Prediction> init = new Dictionary<string, Prediction>();
        Vector2Int tmpvec = initBoard.findPieceByID(ID).Value;
        init.Add(
            initBoard.GetHashString(),
            new Prediction(
                initBoard, initBoard.grille[tmpvec.x,tmpvec.y].color
            )
        );

        metaDic.Add(0, init);
        List<Piece> pieces = initBoard.getOrdonedList(ID);
        end = (end > pieces.Count)? pieces.Count:end;
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

            foreach (KeyValuePair<string, Prediction> current in currentDic) // On test les possibilité possible au tour suivant du board
            {
                Board board = current.Value.board;
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
                    //piece.refreshGoal(board);
                    //la = piece.goal.filter(board, la);
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

                        Prediction tmp = current.Value.extendPrediction(ac);
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

    // Génère toutes les conséquences possiblent de chaque mouvement que peut faire la piece
    // Et retourne le résultat sous forme de List de Plateau
    public Prediction predictionMove(Piece p, Board board)
    {
        Board virtualBoard = new Board(board);
        //Debug.Log(board);
        //Dictionary<string, Board> result = getAllBoardPossible(p.id, 20, virtualBoard, true);

        //Dictionary<string, Prediction> result = getAllBoardPossibleIte(p.id, (p.id + 10) % 30, virtualBoard);
        Dictionary<string, Prediction> result = getAllBoardPossibleIte(p.id, smart, virtualBoard); // ON prédit que les pieces

        //Debug.Log("Nombre de board predit: " + result.Count);
        string keyMax = "";
        int max = -1;
        foreach (KeyValuePair<string, Prediction> pair in result)
        {
            //Debug.Log("position: "+ pair.Value.findPieceByID(p.id).Value.ToString());
            int total = pair.Value.board.getRank(p.color);
            if (max == -1 || max < total)
            {
                keyMax = pair.Key;
                max = total;
            }
            else if( max == total)// on favorise le déplacement
            {
                //Action action = new Action(p.position, result[pair.Key].findPieceByID(p.id).Value);
                Action action = result[pair.Key].getAction(p);
                if (action.willMove())
                {
                    keyMax = pair.Key;
                    max = total;
                }

            }
        }
        if (result.ContainsKey(keyMax))
        {
            //Debug.Log(result[keyMax].board);
            //Action ac = result[keyMax].getAction(p.id);
            //Debug.Log("New board : " + max + " Le move de la piece " + p.id + " : " + ac);
            return result[keyMax];
        }
        else
        {
            return new Prediction(board,p.color);
        }
    }

    //public void Start()
    //{
    //    Board b = new Board(true);
    //    List<Piece> pieces = b.getPiecesTab(Color.White);
    //    Debug.Log(System.DateTime.Now.ToString());
    //    Vector2Int v = b.findPieceByID(12).Value;
    //    predictionMove(b.grille[v.x,v.y], b);
    //    Debug.Log(System.DateTime.Now.ToString());
    //    EditorApplication.isPlaying = false;
    //}
}
