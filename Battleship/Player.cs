using System;

namespace Battleship {
	internal class Player {
		public string name;
		public ConsoleColor color;

		public Player(string name, ConsoleColor color) {
			this.name = name;
			this.color = color;
		}
	}
}
