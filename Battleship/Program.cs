using System;
using System.IO;
using System.Threading;
using yakkudev.Collections;
namespace Battleship {

	internal class Program {

		public static StreamWriter sw = new StreamWriter(Console.OpenStandardOutput());
		static Util.Messages msg;
		static State gameState;
		static bool deferSwitch = false;
		static Random rng = new Random();

		static void CursorMove(Board board, ConsoleKey key, int cursorSize, ref Vec cursor, ref Vec cursorDir) {
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
			cursor.y %= Board.height

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
		}

		static void UpdateMessage() {
			msg = gameState switch {
				State.Placing => Util.Messages.PlaceShip,
				State.Shooting => Util.Messages.Shoot,
				_ => msg
			};
		}

		static Board GetOtherBoard(Board currentBoard, Board board1, Board board2) {
			if (currentBoard == board1) return board2;
			if (currentBoard == board2) return board1;
			return null;
		}

		static void PrintTransition() {
			Console.Clear();
			sw.Flush();
			Util.WriteColored("&aTransition Screen%r\nGive the seat to the other player.\nPress &e[action]%r to continue.");
		}

		static void Main(string[] args) {
			// TODO: https://stackoverflow.com/questions/37769194/modify-command-line-arguments-before-application-restart
			// Restart application and count wins with arguments

			// StreamWriter is a faster than regular printing, we can flush whenever we want
			sw.AutoFlush = false;
			Console.SetOut(sw);
			Console.CursorVisible = false;
			Console.Title = "Battleship : Dominik Brewka";
			Console.SetWindowSize(60, 30);

			var player1 = new Player("Player 1", ConsoleColor.Blue);
			var player2 = new Player("Player 2", ConsoleColor.Red); // TODO: set p2 to AI

			var board1 = new Board(player1);
			var board2 = new Board(player2);

			var current = board1;
			Board other;

			var cursor = Vec.Zero;
			var cursorDir = Vec.Right;
			ConsoleKey key = default;

			msg = Util.Messages.PlaceShip;
			gameState = State.Placing;
			bool requireConfirmation = false;
			bool deferTransition = false;

			// TODO:
			Util.WriteColored("%r > Press &e[1]%r to play vs. player\n");
			Util.WriteColored(" > Press &e[2]%r to play vs. AI");

			do {
				// Define other board
				other = GetOtherBoard(current, board1, board2);

				// Printing
				do {
					if (current.HasLost()) {
						Console.Clear();
						Util.WriteColored($"{other.player.name} wins!\n");
						Util.WriteColored($"{player1.name}: {player1.wins}\n");
						Util.WriteColored($"{player2.name}: {player2.wins}\n");
						Util.WriteColored($"Press &e[action]%r to rematch\n");
						Util.WriteColored($"Press &e[x]%r to exit\n");
						requireConfirmation = true;
						break;
					}

					if (deferTransition && !other.player.lastWasHit) {
						PrintTransition();
						requireConfirmation = true;
						deferSwitch = true;
						break;
					}

					// Switch board if deferred
					if (deferSwitch && !requireConfirmation) {
						current.ClearHighlights();
						other = current;
						current = GetOtherBoard(current, board1, board2);
						deferSwitch = false;
						cursor = Vec.Zero;
					}

					// Print board
					Console.Clear();
					if (gameState == State.Placing)
						current.Print();
					else if (gameState == State.Shooting) {
						current.Print(true);
						Console.WriteLine();
						other.Print();
					}


					// Write message
					Console.WriteLine();
					Util.messageMap.TryGetValue(msg, out string msgValue);
					Util.WriteColored(msgValue);
					sw.Flush();

					// Print side panel
					int xOffset = Board.width * 2 + 4;

					Util.WriteColoredAt(xOffset, 0, $"&{Util.GetColorKey(current.player.color)}{current.player.name}");
					Util.WriteColoredAt(xOffset, 1, "&8Move cursor: &e[arrow keys]%r");
					Util.WriteColoredAt(xOffset, 2, "&8Action: &e[space]%r");
					Util.WriteColoredAt(xOffset, 3, "&8Rotate: &e[r]%r");
					if (gameState == State.Placing) Util.WriteColoredAt(xOffset, 4, "&8Random: &e[q]%r");
					Util.WriteColoredAt(xOffset, 6, "&8Stop game: &e[x]%r");

				} while (false);

				// Evaluate ship size
				var cursorSize = 1;
				if (gameState == State.Placing) {
					cursorSize = current.player.FirstAvailableShip();
					if (cursorSize == -1) {
						msg = Util.Messages.Confirm;
						requireConfirmation = true;
						deferSwitch = true;
						if (current == board2) {
							// TODO: Change game state here
							gameState = State.Shooting;
						}
					}
				} else if (gameState == State.Shooting) { }


				key = Console.ReadKey(true).Key;
				do {
					if (requireConfirmation && key == ConsoleKey.Spacebar) {
						requireConfirmation = false;
						if (!current.player.lastWasHit && gameState == State.Shooting && !deferTransition)
							deferTransition = true;
						else
							deferTransition = false;
						UpdateMessage();
						break;
					}

					// Random ship setup
					if (key == ConsoleKey.Q && gameState == State.Placing) {
						int n = 0;
						while (current.player.FirstAvailableShip() != -1) {
							if (n > 5000) { // Too many iterations, crash application (why bother fixing)
								Console.Clear();
								sw.Flush();
								Util.WriteColored("Your enemy hired a spy and knows where all your ships are. You lost.\n Please restart the application.");
								Console.ReadLine();
								return;
							}
							var pos = new Vec(
								rng.Next(0, Board.width),
								rng.Next(0, Board.height)
							);
							var dir = Vec.Right;
							for (int i = 0; i < rng.Next(0, 4); i++) {
								dir = Vec.RotateAA(dir);
							}

							int size = current.player.FirstAvailableShip();

							if (current.AddShip(new Ship(pos, size, dir))) {
								msg = Util.Messages.PlaceShip;
								current.player.shipsToPlace[size - 1]--;
								current.BuildCells();
							}
							n++;
						}
						break;
					}

					// Cursor Move
					if (key != ConsoleKey.Spacebar) {
						CursorMove(current, key, cursorSize, ref cursor, ref cursorDir);
						break;
					}

					// Action
					// =====
					if (gameState == State.Placing) {
						if (current.AddShip(new Ship(cursor, cursorSize, cursorDir))) {
							msg = Util.Messages.PlaceShip;
							current.player.shipsToPlace[cursorSize - 1]--;
							current.BuildCells();
						} else {
							msg = Util.Messages.InvalidPos;
						}
						break;
					}

					if (gameState == State.Shooting) {
						if (current.CellAt(cursor).state == Cell.State.Shot ||
							current.CellAt(cursor).state == Cell.State.Sunk ||
							current.CellAt(cursor).state == Cell.State.Missed ||
							!current.IsPosOnBoard(cursor)
						) {
							msg = Util.Messages.InvalidPos;
							break;
						}
						bool hit = current.ShootAt(cursor);
						other.player.lastWasHit = hit;
						if (!hit) {
							requireConfirmation = true;
						} else {
						}
						msg = (hit) ? Util.Messages.Hit : Util.Messages.Miss;

						break;
					}
				} while (false);


				// Limit how fast the user can press keys
				// Makes printing look less slow
				Thread.Sleep(10);
				while (Console.KeyAvailable) { Console.ReadKey(true); }
			} while (key != ConsoleKey.X);
		}
	}
}
