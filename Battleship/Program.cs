using System;
using System.IO;
namespace Battleship {

	internal class Program {

		public static StreamWriter sw = new StreamWriter(Console.OpenStandardOutput());

		static void Main(string[] args) {

			// StreamWriter is a faster than regular printing, we can flush whenever we want
			sw.AutoFlush = false;
			Console.SetOut(sw);
			Console.CursorVisible = false;
			Console.Title = "Battleship : Dominik Brewka";
			Console.SetWindowSize(60, 30);

			var player1 = new Player("Player 1", ConsoleColor.Blue);
			var player2 = new Player("Player 2", ConsoleColor.Red);

			Util.WriteColored("%r > Press &e[1]%r to play vs. player\n");
			Util.WriteColored(" > Press &e[2]%r to play vs. AI");

			ConsoleKey key;
			while (true) {
				key = Console.ReadKey(true).Key;
				if (key == ConsoleKey.D1) {
					while (new Game(player1, player2).PlayVS()) ;
					break;
				}

				if (key == ConsoleKey.D2) {
					player1.name = "Human";
					player2.name = "AI";
					while (new Game(player1, player2).PlayAI()) ;
					break;
				}
			}

		}
	}
}
