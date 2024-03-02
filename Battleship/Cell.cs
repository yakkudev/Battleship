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
		public Cell(State state, bool isHighlighted = false) { this.state = state; this.isHighlighted = isHighlighted; }

		public static string GetRender(State s, bool isHighlighted = false) {
			return new Cell(s, isHighlighted).GetRender();
		}



		// The render value is a string after adding features like highlighting and state
		public string GetRender() {
			var str = new StringBuilder("");
			if (isHighlighted) {
				str.Append("%a&0");
			} else {
				str.Append(state switch {
					State.Sunk => "%7&0",
					State.Missed => "%8&f",
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
