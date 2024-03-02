using System.Text;

namespace Battleship {
	internal class Cell {

		public bool isHighlighted;
		public enum State {
			None = '.',
			Ship = '@',
			Sunk = '#',
			Shot = 'X',
			Missed = 'O',
		}

		// Temporarily public
		public State state = State.None;

		public Cell() { }
		public Cell(State state) { this.state = state; }

		public static string GetRender(State s) {
			return new Cell(s).GetRender();
		}



		// The render value is a string after adding features like highlighting and state
		public string GetRender() {
			var str = new StringBuilder("");
			// Colors
			if (isHighlighted) {
				str.Append("%f&0");
			} else {
				str.Append(state switch {
					State.Sunk => "%0&7",
					State.Shot => "%c&0",
					_ => "",
				});
			}

			// Character (icon)
			str.Append((char)state);
			str.Append("%r ");
			return str.ToString();
		}
	}
}
