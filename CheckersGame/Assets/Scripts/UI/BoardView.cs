using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceTest
{
    public Vector3 position;
    private int life;
    public Color color { get; private set; }
    public PieceTest(Color color,Vector3 position)
    {
        this.position = position;
        this.color = color;
    }
}
public class BoardView : MonoBehaviour
{
    public GameObject[] piecesPrefabs;

    private PieceTest[][] pieces = new PieceTest[10][];
    // Start is called before the first frame update
    void Start()
    {
        for (int i=0; i<10; ++i)
        {
            pieces[i] = new PieceTest[10];
            for (int j = 0; j < 10; ++j)
            {
                if (i<3)
                {
                    if (i%2 == 0 && j % 2 == 0)
                    {
                        pieces[i][j] = new PieceTest(Color.White, new Vector3(j, 0.65f, i));
                    } 
                    else if (i % 2 == 1 && j % 2 == 1)
                    {
                        pieces[i][j] = new PieceTest(Color.White, new Vector3(j, 0.65f, i));
                    }
                }

                if (i > 6)
                {
                    if (i % 2 == 0 && j % 2 == 0)
                    {
                        pieces[i][j] = new PieceTest(Color.Black, new Vector3(j, 0.65f, i));
                    }
                    else if (i % 2 == 1 && j % 2 == 1)
                    {
                        pieces[i][j] = new PieceTest(Color.Black, new Vector3(j, 0.65f, i));
                    }
                }
            }
        }
        displayPieces();
    }

    private void displayPieces()
    {
        for(int i = 0; i < 10; ++i)
        {
            for(int j = 0; j < 10; ++j)
            {
                if (pieces[i][j] != null)
                {
                    PieceTest piece = pieces[i][j];
                    Instantiate(piecesPrefabs[(int)piece.color], piece.position, Quaternion.identity);
                }
            }
        }
    }

    public static void refreshBoard(Board board)
    {
        //Debug.Log("Nombre de gameObject supprimé "+GameObject.FindGameObjectsWithTag("piece").Length);
        foreach (var obj in GameObject.FindGameObjectsWithTag("piece"))
        {
            Destroy(obj);
        }
        GameObject[] prefabs = new GameObject[2];
        prefabs[0] = Resources.Load("pieceWhite") as GameObject;
        prefabs[1] = Resources.Load("pieceBlack") as GameObject;
        for (int i = 0; i < board.size; ++i)
        {
            for (int j = 0; j < board.size; ++j)
            {
                if (board.grille[i,j]!= null)
                {
                    Piece piece = (Piece)board.grille[i, j];
                    if (board.grille[i, j].GetType() == typeof(Tower))
                    {
                        Tower t = (Tower)board.grille[i, j];
                        prefabs[(int)piece.color].GetComponent<LifeController>().setTowerHeight(t.height);
                    }
                    else if (board.grille[i, j].GetType() == typeof(Pion))
                    {
                        prefabs[(int)piece.color].GetComponent<LifeController>().setLifePoint(piece.life);
                    }
                    
                    prefabs[(int)piece.color].GetComponent<LifeController>().viewBurn(piece.standBy);
                    Instantiate(prefabs[(int)piece.color], new Vector3(piece.position.y,0.65f,piece.position.x), Quaternion.identity);
                }
            }
        }
    }
}
