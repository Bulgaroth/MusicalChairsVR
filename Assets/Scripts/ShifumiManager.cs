using AYellowpaper.SerializedCollections;
using UnityEngine;

public class ShifumiManager : MonoBehaviour
{
	public enum Move { Rock, Paper, Scissors, Spock, Lizard, Well}

	public SerializedDictionary<Move, Move[]> moves;
	public Move aiMove;
	public Move playerMove;

	private enum GameState { Win, Lose, Draw }
	GameState gameState;

	public void OnShapeRecognized(int shapeIdx)
	{
		playerMove = (Move)shapeIdx;
		if (aiMove == playerMove)
		{
			gameState = GameState.Draw;
			Debug();
			return;
		}

		foreach (Move mv in moves[playerMove])
		{
			if (mv == aiMove)
			{
				gameState = GameState.Win;
				Debug();
				return;
			}
		}

		gameState = GameState.Lose;
		Debug();
	}

	public void Debug()
	{
		print($"Ai -> {aiMove} | You -> {playerMove} => {gameState}");
	}
}
