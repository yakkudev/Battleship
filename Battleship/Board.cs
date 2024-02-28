using System;
using System.Collections.Generic;
using System.Text;

namespace Battleship {
	internal class Board {
		public const int height = 10;
		public const int width = 10;

		public Player player;
		Cell[,] cells = new Cell[height, width];
		List<Ship> ships = new List<Ship>();

		List<Vec> highlights = new List<Vec>();

		public void Print() {
			var str = new StringBuilder(" ");
			// Write row of letter coordinates
			for (int i = 0; i < width; i++) {
				str.Append($" %b&0{Util.GetLetter(i)}");
			}

			str.Append("\n&r");

            for (int y = 0; y < height; y++) {
				// Write a coordinate number
				str.Append($"%b&0{y} &r");
				for (int x = 0; x < width; x++) {
					str.Append($"{CellAt(x, y).GetRender()} ");
				}
				str.Append("\n");
			}

			Util.WriteColored(str.ToString());
        }

		public void Reset() {
			for (int i = 0; i < cells.GetLength(0); i++) {
				for (int j = 0; j < cells.GetLength(1); j++) {
					cells[i, j] = new Cell();
				}
			}
		}

		public Cell CellAt(int x, int y) {
			return cells[y, x];
		}

		public Cell CellAt(Vec v) {
			return CellAt(v.x, v.y);
		}

		public void BuildCells(bool full = true) {
			// Reset cells
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					CellAt(x, y).isHighlighted = false;
					// Clear only if full build
                    if (full) CellAt(x, y).state = Cell.State.None;
				}
			}

            foreach (var h in highlights) {
				if (IsPosOnBoard(h))
					cells[h.y, h.x].isHighlighted = true;
            }

            if (full) {
				// Add ship positions
				List<Vec> positions = new List<Vec>();
				foreach (var s in ships) {
					positions = s.GetCellPositions();
                    for (int i = 0; i < positions.Count; i++){
						cells[positions[i].y, positions[i].x] = s.GetCell(i);
                    }
				}
			}
        }

		public void HighlightCell(Vec v) {
			highlights.Add(v);
		}

		public void ClearHighlights() {
			highlights.Clear();
		}

		public bool IsPosOnBoard(Vec v) {
			if (v.x < 0 || v.y < 0 ||
				v.x >= Board.width || v.y >= Board.height) return false;

			return true;
		}

		public bool IsPositionTouching(Vec v) {
			if (!IsPosOnBoard(v)) return false; 
			if (CellAt(v.x,v.y).state != Cell.State.None) return true;
			return false;
		}

		public bool AddShip(Ship s){
			var positions = s.GetCellPositions();
			foreach (var pos in positions)
			{
				if (!IsPosOnBoard(pos))
					return false;

				// See if any surrounding cells are touching a ship
				foreach (var v in Util.GetSurrounding(positions)) {
					if (IsPositionTouching(v))
						return false;
				}
			}
			ships.Add(s);
			return true;
		}

		public Board(Player player) {
			this.player = player;
			Reset();
		}
	}
}
