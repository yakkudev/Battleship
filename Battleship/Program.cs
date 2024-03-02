using System;
using System.IO;
using System.Threading;
using yakkudev.Collections;

namespace Battleship {
	internal class Program {

		public static StreamWriter sw = new StreamWriter(Console.OpenStandardOutput());
		static Util.Messages msg;
		static State gameState;
		static Board boardDefer = null;
		static Random rng = new Random();

		static void CursorMove(Board board, ConsoleKey key, int cursorSize, ref Vec cursor, ref Vec cursorDir) {
			board.CellAt(cursor).isHighlighted = false;


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
			var ghostShip = new Ship(cursor, cursorSize, cursorDir);
			board.ClearHighlights();
			ghostShip.GetCellPositions().ForEach((Vec v) => {
				board.HighlightCell(v);
			});
		}

		enum State {
			Placing,
			Shooting,
			Transition,
		}

		static void UpdateMessage() {
			msg = gameState switch {
				State.Placing => Util.Messages.PlaceShip,

				_ => msg
			};
		}

		static Board GetOtherBoard(Board currentBoard, Board board1, Board board2) {
			if (currentBoard == board1) return board2;
			if (currentBoard == board2) return board1;
			return null;
		}

		static void Main(string[] args) {
			// TODO: https://stackoverflow.com/questions/37769194/modify-command-line-arguments-before-application-restart
			// Restart application and count wins with arguments

			// StreamWriter is a faster than regular printing, we can flush whenever we want
			sw.AutoFlush = false;
			Console.SetOut(sw);
			Console.CursorVisible = false;
			Console.Title = "Battleship : Dominik Brewka";

			var player1 = new Player("Player 1", ConsoleColor.Blue);
			var player2 = new Player("Player 2", ConsoleColor.Red); // TODO: set p2 to AI

			var board1 = new Board(player1);
			var board2 = new Board(player2);

			var currentBoard = board1;

			var cursor = Vec.Zero;
			var cursorDir = Vec.Right;
			ConsoleKey key = default;

			msg = Util.Messages.PlaceShip;
			string msgInterp = "";
			gameState = State.Placing;
			bool requireConfirmation = false;

			// TODO:
			Util.WriteColored("%r > Press &e[1]%r to play vs. player\n");
			Util.WriteColored(" > Press &e[2]%r to play vs. AI");

			do {
				// Switch board if deferred
				if (boardDefer != null && !requireConfirmation) {
					currentBoard = boardDefer;
					boardDefer = null;
					cursor = Vec.Zero;
				}

				// Print board
				Console.Clear();
				currentBoard.Print();

				// Write message
				Console.WriteLine();
				Util.messageMap.TryGetValue(msg, out string msgValue);
				Util.WriteColoredFormat(msgValue, msgInterp);
				sw.Flush();


				// Print side panel
				int xOffset = Board.width * 2 + 4;
				Util.WriteColoredAt(xOffset, 0, $"&{Util.GetColorKey(currentBoard.player.color)}{currentBoard.player.name}");
				Util.WriteColoredAt(xOffset, 1, "&8Move cursor: &e[arrow keys]%r");
				Util.WriteColoredAt(xOffset, 2, "&8Action: &e[space]%r");
				Util.WriteColoredAt(xOffset, 3, "&8Rotate: &e[r]%r");
				if (gameState == State.Placing) Util.WriteColoredAt(xOffset, 4, "&8Random: &e[q]%r");
				Util.WriteColoredAt(xOffset, 6, "&8Stop game: &e[x]%r");

				// Evaluate ship size
				var cursorSize = 1;
				if (gameState == State.Placing) {
					cursorSize = currentBoard.player.FirstAvailableShip();
					if (cursorSize == -1) {
						msg = Util.Messages.Confirm;
						requireConfirmation = true;
						boardDefer = GetOtherBoard(currentBoard, board1, board2);
						if (currentBoard == board2) {
							// TODO: Change game state here
							gameState = State.Shooting;
						}
					}
				} else if (gameState == State.Shooting) { }


				key = Console.ReadKey(true).Key;
				do {
					if (requireConfirmation && key == ConsoleKey.Spacebar) {
						requireConfirmation = false;
						UpdateMessage();
						break;
					}

					// Random ship setup
					if (key == ConsoleKey.Q && gameState == State.Placing) {
						while (currentBoard.player.FirstAvailableShip() != -1) {
							var pos = new Vec(
								rng.Next(0, Board.width),
								rng.Next(0, Board.height)
							);
							var dir = Vec.Right;
							for (int i = 0; i < rng.Next(0, 4); i++) {
								dir = Vec.RotateAA(dir);
							}

							int size = currentBoard.player.FirstAvailableShip();

							if (currentBoard.AddShip(new Ship(pos, size, dir))) {
								msg = Util.Messages.PlaceShip;
								currentBoard.player.shipsToPlace[size - 1]--;
								currentBoard.BuildCells();
							}
						}
					}

					// Cursor Move
					if (key != ConsoleKey.Spacebar) {
						CursorMove(currentBoard, key, cursorSize, ref cursor, ref cursorDir);
						break;
					}

					// Action
					// =====
					if (gameState == State.Placing) {
						if (currentBoard.AddShip(new Ship(cursor, cursorSize, cursorDir))) {
							msg = Util.Messages.PlaceShip;
							currentBoard.player.shipsToPlace[cursorSize - 1]--;
							currentBoard.BuildCells();
						} else {
							msg = Util.Messages.InvalidPos;
						}
						break;
					}

					if (gameState == State.Shooting) {
						var other = GetOtherBoard(currentBoard, board1, board2);
						if (currentBoard.CellAt(cursor).state == Cell.State.Shot ||
							currentBoard.CellAt(cursor).state == Cell.State.Sunk ||
							!currentBoard.IsPosOnBoard(cursor)
						) {
							msg = Util.Messages.InvalidPos;
							break;
						}
						bool hit = currentBoard.ShootAt(cursor);
					}
				} while (false);


				// Limit how fast the user can press keys
				// Makes printing look less slow
				Thread.Sleep(10);
				while (Console.KeyAvailable) { Console.ReadKey(true); }

				// Write message
				if (msg == Util.Messages.Turn) // Add player name
					msgInterp = currentBoard.player.name;
				if (msg == Util.Messages.PlaceShip)
					msgInterp = cursorSize.ToString();

			} while (key != ConsoleKey.X);
		}
	}
}
