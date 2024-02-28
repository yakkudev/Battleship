using System;
using System.IO;
using System.Threading;

namespace Battleship {
	internal class Program {

		public static StreamWriter sw = new StreamWriter(Console.OpenStandardOutput());

		static ConsoleKey Play(Board board, ref Vec cursor, ref Vec cursorDir) {
			board.CellAt(cursor).isHighlighted = false;
			var key = Console.ReadKey(true).Key;


			// Move cursor
			cursor += key switch {
				ConsoleKey.UpArrow => Vec.Up,
				ConsoleKey.DownArrow => Vec.Down,
				ConsoleKey.LeftArrow => Vec.Left,
				ConsoleKey.RightArrow => Vec.Right,
				_ => Vec.Zero,
			};

			// Loop cursor
			if (cursor.x < 0) cursor.x = Board.width - 1;
			if (cursor.y < 0) cursor.y = Board.height - 1;
			cursor.x %= Board.width;
			cursor.y %= Board.height;


			// Rotate ghost ship
			if (key == ConsoleKey.R) {
				cursorDir = Vec.RotateAA(cursorDir);
			}

			// Show ghost ship
			int currentSize = 2;
			var ghostShip = new Ship(cursor, currentSize, cursorDir);
			board.ClearHighlights();
			ghostShip.GetCellPositions().ForEach((Vec v) => {
				board.HighlightCell(v);
			});

			if (key == ConsoleKey.Spacebar) {
				board.AddShip(ghostShip);
				board.BuildCells();
			}

			return key;
		}

		static void Main(string[] args) {

			// StreamWriter is a bit faster than regular printing, also causes less flickering
			sw.AutoFlush = false;
			Console.SetOut(sw);
			Console.CursorVisible = false;
			Console.Title = "Battleship : Dominik Brewka";

			var player1 = new Player("Player 1", ConsoleColor.Red);
			// TOOD: set p2 to AI
			Player player2 = new Player("Player 2", ConsoleColor.Blue);

			var board1 = new Board(player1);
			var board2 = new Board(player2);

			var currentBoard = board1;

			var cursor = new Vec(0, 0);
			ConsoleKey key = default;
			Util.WriteColored("Battleship > Press any key");
			sw.Flush();

			var cursorDir = Vec.Right;

			do {
				key = Play(currentBoard, ref cursor, ref cursorDir);

				Console.Clear();
				currentBoard.Print();

				Console.WriteLine(cursor.ToString());
				sw.Flush();

				// Limit how fast the user can press keys
				// Makes printing look less slow
				Thread.Sleep(10);
				while (Console.KeyAvailable) { Console.ReadKey(true); }

			} while (key != ConsoleKey.X);
		}
	}
}
