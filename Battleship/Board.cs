using System.Collections.Generic;
using System.Linq;
using System.Text;
using yakkudev.Collections;

namespace Battleship {
	internal class Board {
		public const int height = 10;
		public const int width = 10;

		public Player player;
		Cell[,] cells = new Cell[height, width];
		public List<Ship> ships = new List<Ship>();

		List<Vec> highlights = new List<Vec>();

		public void Print(bool shotsAndMisses = false) {
			var str = new StringBuilder(" ");
			char color = Util.GetColorKey(player.color);

			// Write row of letter coordinates
			for (int i = 0; i < width; i++) {
				str.Append($" %{color}&0{Util.GetLetter(i)}");
			}

			str.Append("\n%r");

			for (int y = 0; y < height; y++) {
				// Write a coordinate number
				str.Append($"%{color}&0{y} %r");
				for (int x = 0; x < width; x++) {
					// Display normally
					var c = CellAt(x, y);
					if (!shotsAndMisses) str.Append(c.GetRender());
					else { // This is where the magic happens
						if (c.state == Cell.State.Ship)
							str.Append(Cell.GetRender(Cell.State.None, c.isHighlighted));
						else
							str.Append(c.GetRender());
					}
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

		public bool ShootAt(Vec v) {

			var cell = CellAt(v);
			if (cell.state == Cell.State.Ship) {
				cell.state = Cell.State.Shot;

				foreach (var s in ships) {
					if (s.ContainsPos(v) && s.IsSunken()) {
						s.Sink();
						var surrounding = new List<Vec>();
						surrounding.AddRange(
							Util.GetSurrounding(s.GetCellPositions())
						);
						surrounding = surrounding.Except(s.GetCellPositions()).ToList();
						// TODO: may break some things
						surrounding.ForEach(v => {
							if (IsPosOnBoard(v))
								CellAt(v).state = Cell.State.Missed;
						});
						break;
					}
				}
				return true;
			}
			cell.state = Cell.State.Missed;
			return false;
		}

		public void BuildCells() {
			// Add ship positions
			List<Vec> positions = new List<Vec>();
			foreach (var s in ships) {
				positions = s.GetCellPositions();
				for (int i = 0; i < positions.Count; i++) {
					cells[positions[i].y, positions[i].x] = s.GetCell(i);
				}
			}
		}

		public void HighlightCell(Vec v) {
			if (!IsPosOnBoard(v)) return;
			cells[v.y, v.x].isHighlighted = true;
			highlights.Add(v);
		}

		public void ClearHighlights() {
			foreach (var v in highlights) {
				CellAt(v).isHighlighted = false;
			}
			highlights.Clear();
		}

		public bool IsPosOnBoard(Vec v) {
			if (v.x < 0 || v.y < 0 ||
				v.x >= Board.width || v.y >= Board.height) return false;

			return true;
		}

		public bool IsPositionColliding(Vec v) {
			if (!IsPosOnBoard(v)) return false;
			if (CellAt(v.x, v.y).state != Cell.State.None) return true;
			return false;
		}

		public bool AddShip(Ship s) {
			var positions = s.GetCellPositions();
			foreach (var pos in positions) {
				if (!IsPosOnBoard(pos) || IsPositionColliding(pos))
					return false;
			}
			// See if any surrounding cells are touching a ship
			foreach (var v in Util.GetSurrounding(positions)) {
				if (IsPositionColliding(v))
					return false;
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
