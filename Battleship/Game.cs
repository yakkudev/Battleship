using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using yakkudev.Collections;

namespace Battleship {
	internal class Game {
		StreamWriter sw;
		Random rng = new Random();

		Util.Messages msg;
		State gameState;
		bool deferSwitch = false;

		Player player1;
		Player player2;

		enum State {
			Placing,
			Shooting,
		}

		void CursorMove(Board board, ConsoleKey key, int cursorSize, ref Vec cursor, ref Vec cursorDir) {
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


		void UpdateMessage() {
			msg = gameState switch {
				State.Placing => Util.Messages.PlaceShip,
				State.Shooting => Util.Messages.Shoot,
				_ => msg
			};
		}

		Board GetOtherBoard(Board currentBoard, Board board1, Board board2) {
			if (currentBoard == board1) return board2;
			if (currentBoard == board2) return board1;
			return null;
		}

		void FillBoardRandom(Board board) {
			int n = 0;
			while (board.player.FirstAvailableShip() != -1) {
				if (n > 5000) { // Too many iterations, crash application (why bother fixing)
					Console.Clear();
					sw.Flush();
					Util.WriteColored("Your enemy hired a spy and knows where all your ships are. You lost.\n Please restart the application.");
					Console.ReadLine();
					throw new Exception();
				}
				var pos = new Vec(
					rng.Next(0, Board.width),
					rng.Next(0, Board.height)
				);
				var dir = Vec.Right;
				for (int i = 0; i < rng.Next(0, 4); i++) {
					dir = Vec.RotateAA(dir);
				}

				int size = board.player.FirstAvailableShip();

				if (board.AddShip(new Ship(pos, size, dir))) {
					msg = Util.Messages.PlaceShip;
					board.player.shipsToPlace[size - 1]--;
					board.BuildCells();
				}
				n++;
			}
		}

		bool IsValidShootPos(Board board, Vec v) {
			if (!board.IsPosOnBoard(v)) return false;
			return board.CellAt(v).state != Cell.State.Shot &&
					board.CellAt(v).state != Cell.State.Sunk &&
					board.CellAt(v).state != Cell.State.Missed;
		}

		void PrintTransition() {
			Console.Clear();
			sw.Flush();
			Util.WriteColored("&aTransition Screen%r\nGive the seat to the other player.\nPress &e[action]%r to continue.");
		}

		public Game(Player player1, Player player2) {
			sw = Program.sw;
			this.player1 = player1;
			this.player2 = player2;
		}

		bool Win(Board other) {
			other.player.wins++;
			Console.Clear();
			Util.WriteColored($"{other.player.name} wins!\n");
			Util.WriteColored($"{player1.name}: {player1.wins}\n");
			Util.WriteColored($"{player2.name}: {player2.wins}\n\n");
			Util.WriteColored($"Press &e[action]%r to rematch.\n");
			Util.WriteColored($"Press &e[x]%r to exit.\n");

			player1.Reset();
			player2.Reset();

			ConsoleKey key;
			while (true) {
				key = Console.ReadKey(true).Key;
				if (key == ConsoleKey.Spacebar) return true;
				else if (key == ConsoleKey.X) return false;
			}
		}

		void Print(Board current, Board other) {
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

			if (gameState == State.Placing) Util.WriteColoredAt(xOffset, 0, $"&{Util.GetColorKey(current.player.color)}{current.player.name}");
			else Util.WriteColoredAt(xOffset, 0, $"&aShoot at &{Util.GetColorKey(current.player.color)}{current.player.name}");
			Util.WriteColoredAt(xOffset, 1, "&8Move cursor: &e[arrow keys]%r");
			Util.WriteColoredAt(xOffset, 2, "&8Action: &e[space]%r");
			Util.WriteColoredAt(xOffset, 3, "&8Rotate: &e[r]%r");
			if (gameState == State.Placing) Util.WriteColoredAt(xOffset, 4, "&8Random: &e[q]%r");
			Util.WriteColoredAt(xOffset, 6, "&8Stop game: &e[x]%r");
		}

		public bool PlayVS() {
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


			do {
				// Define other board
				other = GetOtherBoard(current, board1, board2);

				do {
					if (current.HasLost() && gameState == State.Shooting) {
						return Win(other);
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
					Print(current, other);

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
							gameState = State.Shooting;
						}
					}
				}


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
						FillBoardRandom(current);
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
						if (!IsValidShootPos(current, cursor)) {
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
			return false;
		}

		public bool PlayAI() {
			var board1 = new Board(player1);
			var board2 = new Board(player2);

			var current = board1;
			Board other = board2;

			var cursor = Vec.Zero;
			var cursorDir = Vec.Right;
			ConsoleKey key = default;

			msg = Util.Messages.PlaceShip;
			gameState = State.Placing;
			bool requireConfirmation = false;
			bool deferTransition = false;

			Vec aiPos = Vec.Zero;
			List<Vec> aiHits = new List<Vec>() { Vec.Up };
			int aiMoves = 0;

			do {
				do {
					if (current.HasLost() && gameState == State.Shooting) {
						return Win(other);
					}

					if (deferTransition && !other.player.lastWasHit) {
						requireConfirmation = true;
						deferSwitch = true;
						break;
					}

					// Switch board if deferred
					if (deferSwitch) {
						other.ClearHighlights();
						other = current;
						current = GetOtherBoard(current, board1, board2);
						deferSwitch = false;
					}

					if ((gameState == State.Placing) == (current == board2)) {
						// Print(other, current);
						break;
					}

					// Print board
					Print(current, other);

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
							gameState = State.Shooting;
						}
					}
				}

				if (gameState == State.Placing == (current == board1))
					key = Console.ReadKey(true).Key;
				do {
					if (current == board2) {
						if (gameState == State.Placing) {
							if (requireConfirmation) {
								requireConfirmation = false;
								UpdateMessage();
								break;
							}
							FillBoardRandom(current);
							break;
						}
					}

					if (gameState == State.Shooting && other == board2) {
						if (requireConfirmation) {
							requireConfirmation = false;
							UpdateMessage();
							break;
						}
						if (aiMoves == 0) {
							deferSwitch = true;
							aiMoves++;
							break;
						}
						var n = 0;
						var k = 0;
						const int maxIterations = 500;
						Vec lastPos = aiPos;
						do {
							int sw = 0;
							if (aiHits.Count > 0 && n < maxIterations) {
								List<Vec> surr = Util.GetSurroundingAA(aiHits, false);
								aiPos = surr[rng.Next(0, surr.Count)];
								n++;
							} else if (k < maxIterations * 2) {
								if (k > maxIterations)
									sw = 1;

								aiPos = new Vec(
									rng.Next(0, Board.width / 2),
									rng.Next(0, Board.height / 2)
								) * 2 + (sw * (Vec.Down + Vec.Left));
								k++;
							} else {
								aiPos = new Vec(
									rng.Next(0, Board.width),
									rng.Next(0, Board.height)
								);
							}
						} while (!IsValidShootPos(current, aiPos));

						var hit = current.ShootAt(aiPos);
						other.player.lastWasHit = hit;
						if (hit) {
							for (int i = 0; i < aiHits.Count; i++) {
								if (!current.IsPosOnBoard(aiHits[i])) continue;
								if (current.CellAt(aiHits[i]).state == Cell.State.Sunk) {
									aiHits.Remove(aiHits[i]);
								}
							}
							aiHits.Add(aiPos);
						}
						deferSwitch = !hit;

						aiMoves++;
						Thread.Sleep(100);
						break;
					}

					if (requireConfirmation && key == ConsoleKey.Spacebar) {
						requireConfirmation = false;
						UpdateMessage();
						break;
					}

					// Random ship setup
					if (key == ConsoleKey.Q && gameState == State.Placing) {
						FillBoardRandom(current);
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

						if (!IsValidShootPos(current, cursor)) {
							msg = Util.Messages.InvalidPos;
							break;
						}
						bool hit = current.ShootAt(cursor);
						other.player.lastWasHit = hit;
						if (!hit) {
							deferSwitch = true;
						}
						msg = (hit) ? Util.Messages.Hit : Util.Messages.Miss;

						break;
					}
				} while (false);


				// Limit how fast the user can press keys
				// Makes printing look less slow
				if (current.player == player1)
					Thread.Sleep(10);
				while (Console.KeyAvailable) { Console.ReadKey(true); }
			} while (key != ConsoleKey.X);
			return false;
		}

	}
}
