using System;

namespace Battleship {
	internal class Player {
		public string name;
		public ConsoleColor color;
		public bool lastWasHit;

		public int wins;

		public int[] shipsToPlace = { 4, 3, 2, 1 };

		public Player(string name, ConsoleColor color) {
			this.name = name;
			this.color = color;
		}

		public int FirstAvailableShip() {
			int ret = -1;
			for (int i = 0; i < shipsToPlace.Length; i++) {
				if (shipsToPlace[i] == 0) continue;
				else { ret = i + 1; break; }
			}
			return ret;
		}
	}
}
