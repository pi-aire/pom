using System;
using UnityEngine;
using System.Collections.Generic;

public class Tower : Piece
{
/* Attribut de la Classe Tower*/
    public int height { get; private set; } // Définie la hauteur de la tour

/* Constructeur de la Classe Tower*/
    //public Tower(int x, int y, Color color)
    //{
    //    this.life = 4;
    //    this.color = color;
    //    position = new Vector2Int(x, y);
    //    height = 2;
    //    //this.goal = new Target(this.getTarget(board));
    //    //this.goal = new Forward(color);
    //}

    public Tower(int id, int x, int y, Color color)
    {
        this.id = id;
        this.life = 4;
        this.color = color;
        position = new Vector2Int(x, y);
        height = 2;
        //this.goal = new Target(this.getTarget(board));
        //this.goal = new Forward(color);
    }
    /*Methode implémenté de la Classe Piece*/
    public override bool action(Player p, Board board)
    {
        Action action = p.movePiece(this, board);
        if (action.canMove(board))
        {
            action.doAction(board);
            return true;
        }
        else
        {
            return false;
        }
    }

    public override void setPosition(Vector2Int npositon)
    {
        this.position = npositon;
    }

    public override void takeDamage(int dmg)
    {
        life -= dmg;
        // on recalcul la hauteur de la tour
        height = (int)Math.Ceiling(Convert.ToDouble(life) / 2.0f);
    }

    public override int getRank()
    {
        return 7 + life + standBy;
    }
    public override int getRankNoBurn()
    {
        return 7 + life;
    }


    public override Piece clone()
    {
        Tower clone = new Tower(this.id, position.x, position.y, color);
        clone.life = this.life;
        clone.goal = this.goal;
        clone.height = this.height;
        clone.predictions = this.predictions;
        clone.standBy = this.standBy;
        return clone;
    }

    /*Méthodes propres a la classe Tower*/
    //La tour de compose d'un pion de plus
    public void upgrade()
    {
        height += 1;
        life += 2;
    }

    // Indique si la tour peut attaquer la tour donnée en parametre
    public bool canAttack(Tower enemy)
    {
        return height >= enemy.height;
    }

    // Calul les domages que prend la tour fournie en parametre si la tour (this) l'attaque
    public int calculatDamage(Tower target)
    {
        if (target.height == height)
        {
            return 1;
        }
        return height - target.height;
    }

    // Regarde si elle (this) est capturé par les pieces Adverse, dans ce cas elle change de camps
    public void captured(Board board)
    {
        bool captured = true;
        Color opponnentC = (Color) (((int)color + 1) % 2);
        for(int i = -1; i<= 1; i += 2)
        {
            for (int j = -1; j <= 1; j += 2)
            {
                Vector2Int voisin = new Vector2Int(position.x + i, position.y + j);
                if (board.isCorrect(voisin) 
                    && (board.grille[voisin.x, voisin.y] == null 
                        || board.grille[voisin.x, voisin.y].color != opponnentC))
                {
                    captured = false;
                }
            }
        }
        if (captured)
        {
            color = opponnentC;
        }
    }
}

