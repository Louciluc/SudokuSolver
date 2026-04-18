
using Sudoku;

namespace Sudoku {
	public partial class SudokuGrid {
		public void PrintEmptyGrid()
		{
			int?[,] gridOnlyGivenValues = new int?[TotalSize, TotalSize];
			AllAction makeArrayOfValues = (int x, int y, SudokuGrid grid, params object[] parameters) =>
			{
				// Since all Values should be set here, no null should occur
				if (grid.Grid[x, y].IsDefRight) gridOnlyGivenValues[x, y] = grid.Grid[x, y].Value;
				else gridOnlyGivenValues[x, y] = null;
			};
			loopAll(makeArrayOfValues, null, TotalSize, TotalSize, this, []);

			StaticPrintGrid(new SudokuGrid(gridOnlyGivenValues));
		}
		public void PrintAllSolutions()
		{
			for (int i = 0; i < AllPossibleSolutions.Count; i++)
			{
				int[,] solution = AllPossibleSolutions[i];
				Console.WriteLine($"{i +1}. solution to this Sudoku: ");
				StaticPrintGrid(GetSudokuGridWithColours(GetNullerableGrid(solution)));
			}

			Console.Write($"Its a total of {AllPossibleSolutions.Count} solution");

			// TODO: Find differences if there are more than one solution
			if (AllPossibleSolutions.Count > 1)
			{
				Console.WriteLine('s');
			}
			else { Console.WriteLine(""); }
		}
		public void PrintFirstSolution()
		{
			StaticPrintGrid(GetSudokuGridWithColours(GetNullerableGrid(AllPossibleSolutions[0])));
		}

		public void ChangeGridInTerminal()
		{
			// Add someway of Terminal input here
			Console.Clear();

			var position = (1,1);
			var backup = CopyGrid();

			ConsoleKeyInfo input;

			do
			{
				RenderGridUI(position);

				input = Console.ReadKey();

				if (!HandleInput(input, ref position, backup))
					break;

			} while (input.Key != ConsoleKey.Enter);

			Console.Clear();
		}
		private void ClearTerminalLine(int lineNumber)
		{
			(int, int) currentPos = Console.GetCursorPosition();
			Console.SetCursorPosition(0, lineNumber);
			Console.Write(new string(' ', Console.BufferWidth));
			Console.SetCursorPosition(currentPos.Item1, currentPos.Item2);
		}
		private void RenderGridUI((int, int) position) {

			Console.Clear();
			Console.WriteLine($"Your current position in the Grid is {position}");
			int heightAtGridStart = Console.CursorTop;
			PrintEmptyGrid();
			Console.WriteLine("- Use arrow keys to move around\n" +
				"- Use numbers to enter a number\n" +
				"- \' \', \'.\', \'d\', \'x\' to delete a cell\n" +
				"- enter: save and exit\n" +
				"- c to cancel");

			// set cursor to position the user wants to edit and read input
			(int, int) terminalPos = GridPosToTerminalPos(position);
			(int, int) maxTerminalPos = GridPosToTerminalPos((TotalSize, TotalSize));
			Console.CursorTop = terminalPos.Item1 + heightAtGridStart;
			Console.CursorLeft = terminalPos.Item2;
		}

		private bool HandleInput(ConsoleKeyInfo input, ref (int, int) position, int?[,] backupGrid) {
			// x and y are 0 based index, position is not, x and y are for the position in int? [,] Grid
			int x = position.Item1 - 1;
			int y = position.Item2 - 1;
			switch (input.Key)
			{
				// Move Position:
				case ConsoleKey.UpArrow:
					if (position.Item1 > 1) position.Item1--;
					else position.Item1 = TotalSize;
					return true;
				case ConsoleKey.DownArrow:
					if (position.Item1 < TotalSize) position.Item1++;
					else position.Item1 = 1;
					return true;
				case ConsoleKey.LeftArrow:
					if (position.Item2 > 1) position.Item2--;
					else position.Item2 = TotalSize;
					return true;
				case ConsoleKey.RightArrow:
					if (position.Item2 < TotalSize) position.Item2++;
					else position.Item2 = 1;
					return true;
				case ConsoleKey.C:
					// cancel
					// see i and j as x and y, they were just set before the switch
					for (int i = 0; i < TotalSize; i++) for(int j = 0; j < TotalSize; j++)
					{
						Grid[i, j].Value = backupGrid[i, j];
						if (backupGrid[i, j] != null) Grid[i, j].IsDefRight = true;
						else Grid[i, j].IsDefRight = false;
					}
					return true;
				// a number or dot/space was pressed
				default:
					if(TryReadNumberInput(input, out int? num)){
						Grid[x,y].Value = num;
						if (num == null) Grid[x,y].IsDefRight = false;
						else Grid[x,y].IsDefRight = true;
					}
					return true;
			}
			return false;

		}
		private bool TryReadNumberInput(ConsoleKeyInfo firstKey, out int? outNumber) 
		{
			// deleting the colls Value is handled at the end of this function.
			//
			//
			// try to parse number
			if (int.TryParse(firstKey.KeyChar.ToString(), out int number))
			{
				// inputs can have multiple digits
				// so check if number is in range, it always should be in range when having two digits
				// I'm going with the way of expecting the second (or more) digit(s) to be given right after the first one
				if(number >= 1 && number <= TotalSize)
				{
					// first digit is in range
					// Now check are more than one digit needed
					if (TotalSize.ToString().Length > 1)
					{
						// Two or more digits needed, most of the times
						string numberString = firstKey.KeyChar.ToString();

						// save current terminal position, SetCursorPosition() expects (left, top)
						(int, int) tempTerminalPosition = Console.GetCursorPosition();

						// give some input instructions on the previous line
						Console.SetCursorPosition(0, tempTerminalPosition.Item2 - 1);
						Console.Write($"Please enter the next {TotalSize.ToString().Length -1} digit(s) for the number, you can exit with q, space or enter: ");

						// index i is 0 based
						// for every digit needed, read a new input
						for (int i = 1; i < TotalSize.ToString().Length; i++)
						{
							// (left, top), +i to visualize the input position
							Console.SetCursorPosition(tempTerminalPosition.Item1 + i, tempTerminalPosition.Item2);
							ConsoleKeyInfo nextDigitInput = Console.ReadKey();

							if (int.TryParse(nextDigitInput.KeyChar.ToString(), out int nextDigit))
							{
								numberString += nextDigit.ToString();
							}
							else
							{
								if(nextDigitInput.Key == ConsoleKey.Q || nextDigitInput.Key == ConsoleKey.Enter || nextDigitInput.Key == ConsoleKey.Spacebar) break;
								else
								{
									// invalid input, give another chance for this digit
									Console.SetCursorPosition(0, tempTerminalPosition.Item2 + 1);
									Console.WriteLine($"Invalid input '{nextDigitInput.KeyChar}' for digit {i + 1}, please try again. Press 'Q', ' ' or enter to cancel entering the number.");
									i--;
								}
							}
						}

						// try to parse the full number
						// this should always be a wanted number, since the exit was already offered by pressing q
						if (int.TryParse(numberString, out int fullNumber))
						{
							// Check if full number is in range
							if (fullNumber < 1 || fullNumber > TotalSize)
							{
								// invalid number
								Console.SetCursorPosition(0, tempTerminalPosition.Item2 +1);
								Console.WriteLine($"The number {fullNumber} is not in the range of 1 to {TotalSize}, press any key to continue.");
								Console.ReadKey();
								outNumber = null;
								return true;
							}
							outNumber = fullNumber;
							return true;
						}
					}
					else
					{
						// only one digit needed
						outNumber = number;
						return true;
					}
				}
			}

			// if its a dot or a space, remove the number
			else if (firstKey.KeyChar == '.' || firstKey.KeyChar == ' ' || firstKey.KeyChar == 'd' || firstKey.KeyChar == 'x')
			{
				outNumber = null;
				return true;
			}

			outNumber = null;
			return false;
		}

		public static void TestFunction()
		{
			SudokuGrid testGrid = new SudokuGrid(TestGrid1);
			testGrid.PrintEmptyGrid();
			Console.Write("Let's first make some fancy stuff, press any key to continue: ");
			Console.ReadKey();
			Console.WriteLine("");
			testGrid.Solve(true);
			testGrid.PrintAllSolutions();

			Task.Delay(2500).Wait();
			Console.WriteLine("-----------------------------\n");

			SudokuGrid testGrid2 = new SudokuGrid(TestGrid2);
			testGrid2.PrintEmptyGrid();
			Console.Write("Now for a test of speed, press any key to continue: ");
			Console.ReadKey();
			Console.WriteLine("");
			testGrid2.Solve(false);
			testGrid2.PrintAllSolutions();

			Task.Delay(2500).Wait();
			Console.WriteLine("-----------------------------\n");

			Console.WriteLine("use --new to create your own sudoku and solve it.");
		}
		public static readonly int?[,] TestGrid1 =
		{
			// this shouldnt be solvable in one solution
			{2,	8, 6, 1, 5,	   9,    7, 4, 3},
			{3, 5, 7, 6, 4,    8,    2, 1, 9},
			{4, 1, 9, 7, null, null, 5, 6, 8},
			{8, 2, 1, 9, 6,    5,    4, 3, 7},
			{6, 9, 3, 8, 7,    4,    1, 2, 5},
			{7, 4, 5, 3, null, null, 8, 9, 6},
			{5, 6, 8, 2, null, null, 9, 7, 4},
			{1, 3, 4, 5, 9,    7,    6, 8, 2},
			{9, 7, 2, 4, 8,    6,    3, 5, 1}
		};
		public static readonly int?[,] TestGrid2 =
		{
			{null,  8,      6,      9,      null,   null,   null,   null,   null},
			{5,     9,      null,   6,      2,      null,   null,   null,   null},
			{7,     null,   null,   null,   null,   1,      null,   null,   null},
			{2,     5,      null,   null,   null,   null,   7,      null,   null},
			{null,  4,      null,   null,   null,   null,   null,   3,      null},
			{null,  null,   3,      null,   null,   null,   null,   9,      2   },
			{null,  null,   null,   3,      null,   null,   null,   null,   1   },
			{null,  null,   null,   null,   5,      8,      null,   6,      9   },
			{null,  null,   null,   null,   null,   2,      5,      8,      null}
		};
	}
}


