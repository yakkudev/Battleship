using System;
using System.IO;
using System.Threading;

namespace Battleship {
	internal class Program {

		public static StreamWriter sw = new StreamWriter(Console.OpenStandardOutput());

		static void Main(string[] args) {

			// StreamWriter is a bit faster than regular printing, also causes less flickering
			sw.AutoFlush = false;
			Console.SetOut(sw);

			var player1 = new Player("Player 1");
			// TOOD: set p2 to AI
			Player player2 = new Player("Player 2");

			var board1 = new Board(player1);
			var board2 = new Board(player2);


			var cursor = new Vec(0, 0);
			ConsoleKey key = default;
			Util.WriteColored("Battleship > Press any key");
			sw.Flush();

			var ghostDir = Vec.Right;

			do {

				board1.CellAt(cursor).isHighlighted = false;
				key = Console.ReadKey(true).Key;


				// Highlight
				cursor += key switch {
					ConsoleKey.UpArrow => new Vec(0, -1),
					ConsoleKey.DownArrow => new Vec(0, 1),
					ConsoleKey.LeftArrow => new Vec(-1, 0),
					ConsoleKey.RightArrow => new Vec(1, 0),
					_ => new Vec(0, 0),
				};

				if (cursor.x < 0) cursor.x = Board.width - 1;
				if (cursor.y < 0) cursor.y = Board.height - 1;
				cursor.x %= Board.width;
				cursor.y %= Board.height;


                if (key == ConsoleKey.R)
                {
                    ghostDir = Vec.RotateAA(ghostDir);
                }

                // Show ghost ship
                int currentSize = 1;
				var ghostShip = new Ship(cursor, currentSize, ghostDir);
                board1.ClearHighlights();
                ghostShip.GetCellPositions().ForEach((Vec v) => {
                    board1.HighlightCell(v);
                });


                if (key == ConsoleKey.Spacebar){
					board1.AddShip(ghostShip);
					Console.Clear();
                    sw.Flush();
					board1.BuildCells(true);
                    board1.Print();
                } else {
					Console.Clear();
					sw.Flush();
					board1.BuildCells(false);
					board1.Print();
                }
					


				Console.WriteLine(cursor.ToString());
				sw.Flush();
				Thread.Sleep(10);
				while (Console.KeyAvailable) { Console.ReadKey(true); }

			} while (key != ConsoleKey.X);
		}
	}
}
