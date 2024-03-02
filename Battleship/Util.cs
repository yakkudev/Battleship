using System;
using System.Collections.Generic;
using System.Linq;
using yakkudev.Collections;

namespace Battleship {
	internal static class Util {

		static readonly Vec[] surroundingOffsets = {
			Vec.Down, Vec.Up, Vec.Left, Vec.Right,
			Vec.Down + Vec.Left,
			Vec.Down + Vec.Right,
			Vec.Up + Vec.Left,
			Vec.Up + Vec.Right,
		};
		public static List<Vec> GetSurrounding(List<Vec> obj, bool doTrim = false) {
			var positions = new List<Vec>();
			foreach (var o in obj) {
				foreach (var offset in surroundingOffsets) {
					positions.Add(o + offset);
				}
			}
			if (doTrim) {
				positions = positions.Distinct().ToList();
			}

			return positions;

		}

		public static char GetLetter(int index) {
			return (char)('a' + index);
		}

		public static int GetLetterIndex(char letter) {
			return letter - 'a';
		}

		public static readonly Map<char, ConsoleColor> colorMap = new Map<char, ConsoleColor> {
			{'0', ConsoleColor.Black},
			{'1', ConsoleColor.DarkBlue},
			{'2', ConsoleColor.DarkGreen},
			{'3', ConsoleColor.DarkCyan},
			{'4', ConsoleColor.DarkRed},
			{'5', ConsoleColor.DarkMagenta},
			{'6', ConsoleColor.DarkYellow},
			{'7', ConsoleColor.Gray},
			{'8', ConsoleColor.DarkGray},
			{'9', ConsoleColor.Blue},

			{'a', ConsoleColor.Green},
			{'b', ConsoleColor.Cyan},
			{'c', ConsoleColor.Red},
			{'d', ConsoleColor.Magenta},
			{'e', ConsoleColor.Yellow},
			{'f', ConsoleColor.White},
		};

		public static ConsoleColor? GetConsoleColor(char c) {
			if (colorMap.TryGetForward(c, out ConsoleColor col)) {
				return col;
			}
			if (c == 'r') return ConsoleColor.Black; // Reset
			return null;
		}

		public static char GetColorKey(ConsoleColor col) {
			colorMap.TryGetReverse(col, out char c);
			return c;
		}

		public static void WriteColored(string text) {

			for (int i = 0; i < text.Length; i++) {

				// Return at end of string
				if (i + 2 > text.Length) {
					Console.Write(text[i]);
					Program.sw.Flush();
					return;
				}

				ConsoleColor? col = GetConsoleColor(text[i + 1]);

				if (col != null) {
					Program.sw.Flush();
					if (text[i] == '&') {
						Console.ForegroundColor = (ConsoleColor)col;
						i++;
						continue;
					} else if (text[i] == '%') {
						Console.BackgroundColor = (ConsoleColor)col;
						if (text[i + 1] == 'r') { // Reset color
							Console.ForegroundColor = ConsoleColor.White;
							Console.BackgroundColor = ConsoleColor.Black;
						}
						i++;
						continue;
					}
				}
				Console.Write(text[i]);
			}
			Program.sw.Flush();
		}

		public static void WriteColoredAt(int x, int y, string text) {
			Vec original = new Vec(Console.CursorLeft, Console.CursorTop);
			Console.SetCursorPosition(x, y);
			WriteColored(text);
			Console.SetCursorPosition(original.x, original.y);
		}

		public static void WriteColoredFormat(string text, Object obj) {
			WriteColored(String.Format(text, obj));
		}

		public enum Messages {
			None,
			PlaceShip,
			InvalidPos,
			Shoot,
			Hit,
			Miss,
			Turn,
			Confirm
		}

		public static readonly Dictionary<Messages, string> messageMap = new Dictionary<Messages, string> {
			// TODO: FIX
			{ Messages.None, "" },
			{ Messages.PlaceShip, "Place your ship! Size: {0}" },
			{ Messages.InvalidPos, "&4Invalid position." },
			{ Messages.Shoot, "Pick a place to shoot." },
			{ Messages.Hit, "&2Hit!" },
			{ Messages.Miss, "&cMiss!" },
			{ Messages.Turn, "{0}'s turn." },
			{ Messages.Confirm, "Press &e[action]%r to confirm." },
		};
	}
}