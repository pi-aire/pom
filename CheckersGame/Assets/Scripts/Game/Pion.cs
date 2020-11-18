using System;
using UnityEngine;

public class Pion : Piece
{
/* Constructeur de la Classe Pion*/
	public Pion(int x, int y , Color color)
	{
		this.life = 2;
		this.color = color;
		position = new Vector2Int(x, y);
		//this.goal = new Forward(this.color);
	}

 /* Methode implémentée de la Classe abstraite Piece*/
	public override bool action(Player p,Board board)
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

	public override void takeDamage(int dm)
	{
		this.life -= dm;
	}

	public override void setPosition(Vector2Int nposition)
    {
		this.position = nposition;
    }

	public override int getRank()
	{
		return life + standBy;
	}
	public override int getRankNoBurn()
	{
		return life;
	}

	public override Piece clone()
	{
		Pion clone = new Pion(position.x, position.y, color);
		clone.life = this.life;
		clone.id = this.id;
		clone.goal = this.goal;
		clone.predictions = this.predictions;
		clone.standBy = this.standBy;
		return clone;
	}

}
