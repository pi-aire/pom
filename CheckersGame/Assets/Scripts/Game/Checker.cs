using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public enum TypePlayer
{
    HUMAN, NAIVE, TARGET, PREDICT, ALPHABETA, ABMEMORY
}
public class Checker : MonoBehaviour
{
    public Board board;
    private Player[] players;

    public TypePlayer white;
    public int smartP1 = 3;
    public TypePlayer black;
    public int smartP2 = 3;
    public bool loop = false;
    public bool multiThreading = false;
    [TextArea]
    public string Notes = "Le multi-threading peut provoquer des erreurs.";
    private DataBase dataBase = new DataBase();

    void Start()
    {
        Random.InitState(110101011);
        Debug.Log("Player 1 is white");
        Debug.Log("Player 2 is black");
        this.players = new Player[2];
        switch (white)
        {
            case TypePlayer.HUMAN:
                this.players[0] = new Human();
                break;
            case TypePlayer.NAIVE:
                this.players[0] = new IA(new Naive());
                Debug.Log("P1 -> NAIVE AI");
                break;
            case TypePlayer.TARGET:
                this.players[0] = new IA(new TargetingR());
                Debug.Log("P1 -> Targeting AI");
                break;
            case TypePlayer.PREDICT:
                this.players[0] = new IA(new PossibilityTree(smartP1));
                Debug.Log("P1 -> Prediction AI");
                break;
            case TypePlayer.ALPHABETA:
                this.players[0] = new IA(new AlphaBeta(smartP2));
                Debug.Log("P1 -> ALPHABETA AI");
                break;
            case TypePlayer.ABMEMORY:
                this.players[0] = new IA(new AlphaBetaMemory(dataBase));
                Debug.Log("P1 -> ABMemory AI");
                break;
            default:
                Debug.LogError("Type de player non connus");
                break;
        }
        switch (black)
        {
            case TypePlayer.HUMAN:
                this.players[1] = new Human();
                break;
            case TypePlayer.NAIVE:
                this.players[1] = new IA(new Naive());
                Debug.Log("P2 -> NAIVE AI");
                break;
            case TypePlayer.TARGET:
                this.players[1] = new IA(new TargetingR());
                Debug.Log("P2 -> Targeting AI");
                break;
            case TypePlayer.PREDICT:
                this.players[1] = new IA(new PossibilityTree(smartP2));
                Debug.Log("P2 -> Prediction AI");
                break;
            case TypePlayer.ALPHABETA:
                this.players[1] = new IA(new AlphaBeta(smartP2));
                Debug.Log("P1 -> ALPHABETA AI");
                break;
            case TypePlayer.ABMEMORY:
                this.players[1] = new IA(new AlphaBetaMemory(dataBase));
                Debug.Log("P1 -> ABMemory AI");
                break;
            default:
                Debug.LogError("Type de player non connus");
                break;
        }

        //Innitialisation du board
        board = new Board(true);
        //board.grille[9, 5] = new Pion(9,5,Color.Black);
        //Debug.Log(board.grille.Length);
        //BoardView.refreshBoard(board);
        //// On lance le jeu
        StartCoroutine(updateControled());
    }

    private IEnumerator updateControled()
    {
        int roundP = 0;
        //Debug.Log(board);
        BoardView.refreshBoard(board);
        int[] prev = new int[2];
        int[] idPrev = { -1, -1 };

        while (board.count(Color.Black) > 0 && board.count(Color.White) > 0)
        {
            //Debug.Log("Tour du Player " + (roundP + 1));
            
            //Tour de jeu
            // Indique à quel joueur jouer on commence par le joueur 1
            Board newboard = new Board(board);

            //Debug.Log("Piece prediction");
            //Debug.Log(newboard.pieces((Color)roundP)[0].predictions.data.Count);
            if (idPrev[roundP] != -1)
            {
                int id = idPrev[roundP];
                Debug.Log("La prédiction : " + (dataBase.data[id].gain + prev[roundP]) + " , La réalitée : " + newboard.getRankNoBurn((Color)roundP));
                //dataBase.Evaluate(id, dataBase.data[id].gain + prev[roundP] <= newboard.getRankNoBurn((Color)roundP));
            }
            foreach (Piece piece in newboard.pieces((Color)roundP))
            {
                piece.predictions = new Selection();
            }


            List<Task> listT = new List<Task>();
            foreach (Piece piece in newboard.pieces((Color)roundP))
            {
                if (roundP == (int)piece.color)
                {
                    if (multiThreading)
                    {
                        Task tmp = Task.Factory.StartNew(() => piece.predict(players[roundP], newboard));
                        listT.Add(tmp);
                    }
                    else
                    {
                        piece.predict(players[roundP], newboard);
                    }
                }
            }
            foreach (Task task in listT)
            {
                task.Wait();
            }

            //Debug.Log("Piece election");
            foreach (Piece piece in newboard.pieces((Color)roundP))
            {
                if (roundP == (int)piece.color)
                {
                    piece.election(players[roundP], newboard);
                }
            }
            // Réalisation de l'action dans le meilleur possible
            List<Piece> pieces = newboard.getPiecesTab((Color)roundP);
            List<Piece> historic = new List<Piece>();
            //Debug.Log("Realisation des actions");
            int iteration = 0;
            while(pieces.Count != 0)
            {
                if ((roundP == 0 && (white == TypePlayer.ABMEMORY || white == TypePlayer.ALPHABETA)) 
                    || (roundP == 1 && (white == TypePlayer.ABMEMORY || white == TypePlayer.ALPHABETA)))
                {
                    idPrev[roundP] = pieces[iteration].predictions.getIdDataBasePredic();
                }
                if (pieces[iteration].action(players[roundP], newboard))
                {
                    if (historic.Contains(pieces[iteration]))
                    {
                        historic.Remove(pieces[iteration]);
                    }
                    Debug.Log(((roundP == 1)?"Black":"White") + " : "+players[roundP].movePiece(pieces[iteration], newboard));
                    pieces.Remove(pieces[iteration]);
                    BoardView.refreshBoard(newboard);
                    iteration = 0;
                    yield return new WaitForSeconds(0.05f);
                }
                else
                {
                    if (historic.Count == pieces.Count)// On ne peut pas 
                    {
                        foreach(Piece notM in historic)
                        {
                            Action a = new Action(notM.position);
                            a.doAction(newboard);
                        }
                        break;
                    }
                    if (!historic.Contains(pieces[iteration]))
                    {
                        historic.Add(pieces[iteration]);
                    }
                    iteration = (iteration + 1) % pieces.Count;
                }
            }
            prev[roundP] = board.getRankNoBurn((Color)roundP); 
            board = newboard;
            yield return new WaitForSeconds(0.01f);
            roundP = (roundP + 1) % 2;
        }
        dataBase.save();
        dataBase.load();
        if (loop)
        {
            Debug.Log("Nouvelle partie");
            Piece.idGenerator = 0;
            Start();
        }
    }
}
