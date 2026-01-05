using MyLibrary;
using Sudoku;
using System.IO;

TestProgram test = new TestProgram();
test.Main();

namespace Sudoku {
	class TestProgram {
		public async void Main()
		{
			SudokuGrid grid = new(16);
			SudokuGrid sgrid = new(9);
			SudokuGrid.TestFunction();
			SudokuGrid newgrid = new(SudokuGrid.TestGrid2);
			newgrid.Solve(true);
			newgrid.PrintAllSolutions();
		}
	}

	public class SudokuGrid {
		// vertical y
		// |
		// |
		// +----- horizontal x

		/// <summary>
		/// Represents the total size in x and y direction, since a Sudoku must be quadratic in every case both values are the same
		/// </summary>
		public int TotalSize { get => BoxSize.Item1 * BoxSize.Item2; }
		readonly public (int, int) BoxSize;
		public (int, int) BoxCount { get { return (BoxSize.Item2, BoxSize.Item1); } }
		public SudokuCell[,] Grid;

		private delegate void AllAction(int x, int y, SudokuGrid sudokuGrid, params object[] parameters);
		private delegate void XAction(int x, SudokuGrid sudokuGrid, params object[] parameters);

		public List<int> AllValues1ToLast { get => [.. Enumerable.Range(1, BoxSize.Item1 * BoxSize.Item2)]; }
		public List<int[,]> AllPossibleSolutions { get
			{
				if (possibleSolutionsSaved.Count == 0)
				{
					// The Soduku has 1 or 0 solutions saved, so it might never have been solved for more than 1 solution
					Solve(true);
				}
				return possibleSolutionsSaved;
			} }
		private List<int[,]> possibleSolutionsSaved = new();
		public Task<bool>? SolveSudokuAllSolutions;

		public SudokuGrid(int?[,] _grid)
		{
			int Length = _grid.GetLength(0);

			if (Length != _grid.GetLength(1)) throw new ArgumentException("Input grid isnt quadratic!", _grid.GetType().Name);
			Grid = new SudokuCell[Length, Length];

			AllAction lambda = (int x, int y, SudokuGrid sudokuGrid, params object[] parameters) =>
			{
				bool isdefright = (parameters[0] as int?[,])[x, y] != null;
				sudokuGrid.Grid[x, y] = new SudokuCell((parameters[0] as int?[,])[x, y], (x, y), isdefright);
			};

			loopAll(lambda, default, Length, Length, this, _grid);

			BoxSize = FindBoxSize(Length);
			//Console.WriteLine("BoxSize: " + BoxSize.ToString());
		}
		public SudokuGrid(int size) : this(new int?[size, size])
		{
			//Console.WriteLine($"An integer size for the SudokuGrid {this} was given. Please fill in your Sudoku");
			//ChangeGridInTerminal();
		}


		private static (int, int) FindBoxSize(int Length)
		{
			if ((int)Math.Sqrt(Length) == Math.Sqrt(Length)) return ((int)Math.Sqrt(Length), (int)Math.Sqrt(Length));

			// BoxSizeComparer is a custom type
			List<BoxSizeComparer> possibleMatches = new();
			for (int i = 1; i < Length; i++)
			{
				for (int j = 1; j < Length; j++)
				{
					if (i * j == Length) possibleMatches.Add(new(i, j));
				}
			}

			Sort.MergeSort(possibleMatches);
			if (possibleMatches.Count == 0) throw new Exception("No BoxSize Found!");
			return possibleMatches[0].Value;
		}
		private struct BoxSizeComparer : IComparable<BoxSizeComparer> {
			public (int, int) Value;
			public BoxSizeComparer(int x, int y) { Value = (x, y); }
			public int CompareTo(BoxSizeComparer other)
			{
				return Math.Abs(this.Value.Item1 - this.Value.Item2).CompareTo(Math.Abs(other.Value.Item1 - other.Value.Item2));
			}
		}


		private static void loopAll(AllAction? actionAll, XAction? actionX, int xLimit, int yLimit, SudokuGrid sudokuGrid, params object[] parameters)
		{
			for (int x = 0; x < xLimit; x++)
			{
				for (int y = 0; y < yLimit; y++)
				{
					actionAll?.Invoke(x, y, sudokuGrid, parameters);
				}
				actionX?.Invoke(x, sudokuGrid, parameters);
			}
		}
		public void ChangeGridInTerminal()
		{
			// Add someway of Terminal input here
			throw new NotImplementedException("There is an implementation, but it doesnt work, unfortunately. :-(");

			int lineAtStart = Console.CursorTop;
			int?[,] currentGrid = new int?[TotalSize, TotalSize];
			for (int x = 0; x < TotalSize; x++) for (int y = 0; y < TotalSize; y++) currentGrid[x, y] = Grid[x, y].Value;
			ConsoleKeyInfo input;
			(int, int) position = (1, 1);
			// 4, because its four lines of instructions below the grid
			int numberOfLinesToClear = TotalSize + BoxCount.Item2 + 4;
			do
			{
				// reset terminal
				//for (int i = lineAtStart; i < lineAtStart + numberOfLinesToClear; i++) { Console.WriteLine(); ClearTerminalLine(i + lineAtStart); }

				// make graphics, like the current grid and instructions
				Console.CursorTop = lineAtStart;
				Console.CursorLeft = 0;
				// some spaces for overwriting old stuff
				Console.WriteLine($"Your current position in the Grid is {position}                 ");
				int heightAtGridStart = Console.CursorTop;
				PrintEmptyGrid();
				Console.WriteLine("- Use arrow keys to move around\n" +
					"- Use numbers to enter a number\n" +
					"- enter: save and exit\n" +
					"- c to cancel");

				// set cursor to position the user wants to edit and read input
				(int, int) terminalPos = GridPosToTerminalPos(position);
				(int, int) maxTerminalPos = GridPosToTerminalPos((TotalSize, TotalSize));
				Console.CursorTop = terminalPos.Item1 + heightAtGridStart;
				Console.CursorLeft = terminalPos.Item2;
				input = Console.ReadKey();

				// evaluate input
				// x and y are 0 based index, position is not, x and y are for the position in int? [,] Grid
				int x = position.Item1 - 1;
				int y = position.Item2 - 1;
				switch (input.Key)
				{
					case ConsoleKey.UpArrow:
						if (position.Item1 > 1) position.Item1--;
						else position.Item1 = TotalSize;
						break;
					case ConsoleKey.DownArrow:
						if (position.Item1 < TotalSize) position.Item1++;
						else position.Item1 = 1;
						break;
					case ConsoleKey.LeftArrow:
						if (position.Item2 > 1) position.Item2--;
						else position.Item2 = TotalSize;
						break;
					case ConsoleKey.RightArrow:
						if (position.Item2 < TotalSize) position.Item2++;
						else position.Item2 = 1;
						break;
					case ConsoleKey.C:
						// cancel
						// see i and j as x and y, they were just set before the switch
						for (int i = 0; i < TotalSize; i++) for(int j = 0; j < TotalSize; j++)
						{
							Grid[i, j].Value = currentGrid[i, j];
							if (currentGrid[i, j] != null) Grid[i, j].IsDefRight = true;
							else Grid[i, j].IsDefRight = false;
						}
						return;
					// a number or dot/space was pressed, or nothing relevant
					// !!!Dont look at this unless you really need to!!!
					default:
						// try to parse number
						if (int.TryParse(input.KeyChar.ToString(), out int number))
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
									string numberString = input.KeyChar.ToString();

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
											break;
										}
										// set the number
										Grid[x, y].Value = fullNumber;
										Grid[x, y].IsDefRight = true;
									}
								}
								else
								{
									// only one digit needed
									Grid[x, y].Value = number;
									Grid[x, y].IsDefRight = true;
								}
							}
						}
						// if its a dot or a space, remove the number
						else if (input.KeyChar == '.' || input.KeyChar == ' ')
						{
							Grid[x, y].Value = null;
							Grid[x, y].IsDefRight = false;
						}

						break;
				}
			} while (input.Key != ConsoleKey.Enter);
			Console.SetCursorPosition(0, numberOfLinesToClear + 2 + lineAtStart);
		}
		private void ClearTerminalLine(int lineNumber)
		{
			(int, int) currentPos = Console.GetCursorPosition();
			Console.SetCursorPosition(0, lineNumber);
			Console.Write(new string(' ', Console.BufferWidth));
			Console.SetCursorPosition(currentPos.Item1, currentPos.Item2);
		}
		private (int, int) GridPosToTerminalPos ((int, int) gridPosition)
		{
			int characterCountOfOneCell = TotalSize.ToString().Length;

			// change to 0 based index
			gridPosition.Item1 --;
			gridPosition.Item2 --;

			(int, int) terminalPos;
			// vertical position is the gridposition plus the spaces for the horizontal lines coming from the boxes
			terminalPos.Item1 = gridPosition.Item1 + (gridPosition.Item1 / BoxSize.Item1);
			// horizontal position is the character count +1 (for spaces and |)
			// I think when there is | there is no space, so +1 is enough
			terminalPos.Item2 = gridPosition.Item2 * (characterCountOfOneCell + 1); //+ (gridPosition.Item1 / BoxSize.Item1);

			return terminalPos;
		}

		public SudokuGrid(string filePath)
		{
			static string? readLine(StreamReader file, ref int currentLineNumber)
			{
				currentLineNumber++;
				return file.ReadLine().Trim('\n');
			}

			// open file
			StreamReader file = new(filePath);
			int currentLineNumber = 0;
			List<string?> allLines = new();

			// check if first line is valid
			allLines.Add(readLine(file, ref currentLineNumber));
			if (allLines[0] == null || allLines[0] == "") throw new Exception("File is empty!");
			if (allLines[0][^1] == ' ') allLines[0] = allLines[0].Remove(allLines[0].Length - 1, 1);

			// check for amount of spaces to determine size of Sudoku
			string[] firstLineParts = allLines[0].Split(' ');

			// we need the totalSize for thr FOR loop below
			int totalSizeOfSudoku = firstLineParts.Length;

			// lambda to parse one line of the file into the grid, 
			static void parseLineToGrid(int lineNumberInGrid, string lineAsString, ref int?[,] intGrid, out int countOfComponents)
			{
				string[] parts = lineAsString.Split(' ');
				countOfComponents = parts.Length;

				for( int i = 0; i < countOfComponents; i++)
				{
					string onePart = parts[i];

					// check for empty values
					if (onePart == "." || onePart == "_" || onePart == "0")
					{
						// value is empty
						intGrid[lineNumberInGrid, i] = null;
					}
					else
					{
						// value is not empty, try to parse it
						if (int.TryParse(onePart, out int value))
						{
							intGrid[lineNumberInGrid, i] = value;
						}
						else
						{
							throw new Exception($"Invalid value '{onePart}' found in line {lineNumberInGrid +1}");
						}
					}
				}
			}

			// Get all remaining lines into allLines variable
			string? currentLine;
			do
			{
				// save the line numnber, because it will change, but we still need the value
				int thisLineNumber = currentLineNumber;
				allLines[thisLineNumber] = readLine(file, ref currentLineNumber);
				currentLine = allLines[thisLineNumber];
			}
			while(currentLine != null);

			// Remove all null lines and empty lines
			allLines.ConvertAll(new Converter<string?, string>((string? line) => (string)(line ?? "")));
			allLines.RemoveAll(line => line == "");
		}

		public static void StaticPrintGrid(SudokuGrid _grid)
		{
			AllAction printhorizontallines = static (int x, int y, SudokuGrid sudokuGrid, params object[] parameters) =>
			{
				// print number with colour if its a given value, print in gray if its empty
				if (sudokuGrid.Grid[x, y].IsDefRight) Console.ForegroundColor = ConsoleColor.DarkCyan;
				else if(sudokuGrid.Grid[x, y].Value == null) Console.ForegroundColor = ConsoleColor.DarkGray;
				// print the value, the ToString method of SudokuCell is overwritten for this.
				Console.Write(sudokuGrid.Grid[x, y]);

				// add spaces to align numbers, it was originally with an if statement chcking if the lengths are different, but this way is cleaner and should have a similar performance
				for (int i = 0; i < sudokuGrid.TotalSize.ToString().Length - sudokuGrid.Grid[x, y].ToString().Length; i++)
				{
					Console.Write(' ');
				}

				//reset colour
				Console.ForegroundColor = ConsoleColor.White;

				// make lines for visualization
				if ((y + 1) % sudokuGrid.BoxCount.Item1 == 0 && y + 1 != sudokuGrid.TotalSize)
				{
					Console.Write('|');
				}
				else
				{
					Console.Write(' ');
				}
			};
			XAction printhorizontalspacing = static (int x, SudokuGrid sudokuGrid, params object[] parameters) =>
			{
				Console.WriteLine();
				if ((x + 1) % sudokuGrid.BoxCount.Item2 == 0 && x + 1 != sudokuGrid.TotalSize)
				{
					// for each
					for (int y = 0; y < sudokuGrid.BoxCount.Item2; y++)
					{
						// j is boxWherePosIsIn of row
						for (int i = 0; i < sudokuGrid.BoxSize.Item2; i++)
						{
							// i is position in boxWherePosIsIn

							// print '-' for each position where a number would be
							// for that the string length of the biggest number is taken and that many '-' are printed
							for (int a = 0; a < sudokuGrid.TotalSize.ToString().Length; a++) Console.Write('-');

							// print the '+' if its the end of a box, its also where the vertical lines cross the horizontal lines
							if ((i + 1) % sudokuGrid.BoxSize.Item2 == 0 && y * sudokuGrid.BoxSize.Item2 + i + 1 != sudokuGrid.TotalSize) Console.Write('+');
							else Console.Write(' ');
						}
					}
					Console.WriteLine();
				}
			};

			loopAll(printhorizontallines, printhorizontalspacing, _grid.TotalSize, _grid.TotalSize, _grid, []);
		}
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
		}
		public void PrintFirstSolution()
		{
			StaticPrintGrid(GetSudokuGridWithColours(GetNullerableGrid(AllPossibleSolutions[0])));
		}


		public bool IsValid(out List<(int, int)> posOfErrors)
		{
			List<SudokuCell> DuplicateCheck(List<SudokuCell> listWithDoubles, ref List<(int, int)> errorList, ref bool isvalid)
			{
				// https://stackoverflow.com/questions/18547354/c-sharp-linq-find-duplicates-in-list 
				List<SudokuCell> duplicates = listWithDoubles.GroupBy(x => x.Value)
					.Where(g => g.Count() > 1)
					.Select(y => y.ElementAt<SudokuCell>(0)) // make it a List of SudokuCells instead of IGroup
					.ToList();
				duplicates.RemoveAll(x => x.Value.Equals(null));

				// Add all duplicates to ErrorList and return false
				foreach (SudokuCell dup in duplicates) errorList.Add(dup.Position);
				if (duplicates.Count != 0) isvalid = false;

				return duplicates;
			}

			bool isvalid = true;
			posOfErrors = new List<(int, int)>();

			// vertical colums
			for (int x = 0; x < TotalSize; x++)
			{
				List<SudokuCell> list = new();
				// look for all numbers in one column
				for (int y = 0; y < (int)TotalSize; y++) list.Add(Grid[x, y]);
				// return a List of all duplicates excluding nulls
				List<SudokuCell> duplicates = DuplicateCheck(list, ref posOfErrors, ref isvalid);

				//foreach(SudokuCell cell in duplicates) Console.WriteLine(cell.ToString());
			}

			// horizontal rows
			for (int y = 0; y < TotalSize; y++)
			{
				List<SudokuCell> list = new();
				// look for all numbers in one row
				for (int x = 0; x < (int)TotalSize; x++) list.Add(Grid[x, y]);
				// returns a List of all duplicates excluding nulls
				List<SudokuCell> duplicates = DuplicateCheck(list, ref posOfErrors, ref isvalid);

				//foreach(SudokuCell cell in duplicates) Console.WriteLine(cell.ToString());
			}

			// boxes (3x3 thingys)
			for (int x = 0; x < BoxCount.Item1; x++)
			{
				for (int y = 0; y < BoxCount.Item2; y++)
				{
					// This Code is inside a boxWherePosIsIn
					List<SudokuCell> list = new();
					for (int a = 0; a < BoxSize.Item1; a++)
					{
						// a is the local i-position
						for (int b = 0; b < BoxSize.Item2; b++)
						{
							// b is the local j-position
							int globalX = x * BoxSize.Item1 + a;
							int globalY = y * BoxSize.Item2 + b;

							list.Add(Grid[globalX, globalY]);
						}
					}
					// This code is inside the boxWherePosIsIn

					// Get all the duplicates without nulls
					List<SudokuCell> duplicates = DuplicateCheck(list, ref posOfErrors, ref isvalid);

					//foreach(SudokuCell cell in duplicates) Console.WriteLine(i.ToString() + j.ToString()+ ": " + cell.ToString());
				}
			}

			return isvalid;
		}
		private (int, int) BoxPosToAbsolutePos((int, int) posInBox, (int, int) boxWherePosIsIn)
		{
			return (posInBox.Item1 + boxWherePosIsIn.Item1 * BoxSize.Item1, posInBox.Item2 + boxWherePosIsIn.Item2 * BoxSize.Item2);
		}
		private (int, int) AbsolutePosToBoxPos((int, int) absolutePos, out (int, int) boxThePosIsIn)
		{
			boxThePosIsIn = (absolutePos.Item1 / BoxSize.Item1, absolutePos.Item2 / BoxSize.Item2);
			return (absolutePos.Item1 % BoxSize.Item1, absolutePos.Item2 % BoxSize.Item2);
		}
		private SudokuGrid GetSudokuGridWithColours(int?[,] numbers)
		{
			SudokuGrid sgrid = new SudokuGrid(numbers);
			AllAction checkIfGiven = (int x, int y, SudokuGrid grid, params object[] paramteers) => sgrid.Grid[x, y].IsDefRight = this.Grid[x, y].IsDefRight;
			loopAll(checkIfGiven, null, TotalSize, TotalSize, sgrid, []);

			return sgrid;
		}
		private int?[,] GetNullerableGrid(int[,] input)
		{
			int?[,] nullableGrid = new int?[TotalSize, TotalSize];
			AllAction castToNullable = (int x, int y, SudokuGrid grid, params object[] parameters) => nullableGrid[x, y] = input[x, y];
			loopAll(castToNullable, null, TotalSize, TotalSize, this, []);

			return nullableGrid;
		}
		private static T[] Convert2DArrayTo1D<T>(T[,] twoDArray)
		{
			T[] oneDArray = new T[twoDArray.Length];
			int count = 0;
			for (int x = 0; x < twoDArray.GetLength(0); x++)
			{
				for (int y = 0; y < twoDArray.GetLength(1); y++)
				{
					oneDArray[count] = twoDArray[x, y];
					count++;
				}
			}
			return oneDArray;
		}


		public List<int> FindPossibleOptionsForCell(SudokuCell cell)
		{
			List<int> vertical = new();
			List<int> horizontal = new();
			List<int> box = new();

			(int, int) position = cell.Position;
			// vertical
			for (int i = 0; i < TotalSize; i++)
			{
				if (i == position.Item2) continue;
				if (Grid[position.Item1, i].Value != null) vertical.Add((int)Grid[position.Item1, i].Value);
			}

			// horizontal
			for (int i = 0; i < TotalSize; i++)
			{
				if (i == position.Item1) continue;
				if (Grid[i, position.Item2].Value != null) horizontal.Add((int)Grid[i, position.Item2].Value);
			}

			// box 
			// Get info of current box and position in box
			(int, int) boxWherePosIsIn;
			(int, int) posInBox = AbsolutePosToBoxPos(position, out boxWherePosIsIn);


			// loop through box
			for (int a = 0; a < BoxSize.Item1; a++)
			{
				for (int b = 0; b < BoxSize.Item2; b++)
				{
					// skip position of cell itself
					if (a == posInBox.Item1 && b == posInBox.Item2) continue;

					// get global position from box position
					(int, int) globalPos = BoxPosToAbsolutePos((a, b), boxWherePosIsIn);

					// add value to box list if not null, here the global position is used, bacause the Grid with the value needs global positions
					if (Grid[globalPos.Item1, globalPos.Item2].Value != null) box.Add((int)Grid[globalPos.Item1, globalPos.Item2].Value);
				}
			}

			// combine all found values
			List<int> combined = [.. vertical, .. horizontal, .. box];

			// take all possible values and remove the ones that are already in the combined list
			List<int> possibleOutcomes = [.. AllValues1ToLast];
			possibleOutcomes.RemoveAll(x => combined.Contains(x));

			/*
			// Check for whitespaces
			// It actually doesnt matter, its only for debug reasons.
			if(vertical.Count != AllValues1ToLast.Count)
			{
				// vertical has whitespace
				if(possibleOutcomes.Count == 0)
				{
					// no possible outcomes left, so there is an error in the grid
					StaticPrintGrid();
					Console.WriteLine("Error: No possible outcomes for cell at position " + position.ToString());
				}
			}
			if(horizontal.Count != AllValues1ToLast.Count)
			{
				// horizontal has whitespace
				if (possibleOutcomes.Count == 0)
				{
					// no possible outcomes left, so there is an error in the grid
					StaticPrintGrid();
					Console.WriteLine("Error: No possible outcomes for cell at position " + position.ToString());
				}
			}
			if(box.Count != AllValues1ToLast.Count)
			{
				// box has whitespace
				if (possibleOutcomes.Count == 0)
				{
					// no possible outcomes left, so there is an error in the grid
					StaticPrintGrid();
					Console.WriteLine("Error: No possible outcomes for cell at position " + position.ToString());
				}
			}
			*/

			return possibleOutcomes;
		}
		public List<int> FindPossibleOptionsForCell((int, int) position)
		{
			return FindPossibleOptionsForCell(Grid[position.Item1, position.Item2]);
		}
		public void FindAndSetAllOptionsForAllCells()
		{
			//List<int>[,] allOptionsOfAllCells = new List<int>[TotalSize, TotalSize];

			AllAction findOptions = (int x, int y, SudokuGrid grid, params object[] parameters) =>
			{
				if (grid.Grid[x, y].Value == null)
				{
					// for each cell, find possible options and set it in the Cell
					grid.Grid[x, y].OptionsLeft = grid.FindPossibleOptionsForCell(grid.Grid[x, y]);
					// add it to the array
					//allOptionsOfAllCells[i, j] = grid.Grid[i, j].OptionsLeft;
				}
				else
				{
					grid.Grid[x, y].OptionsLeft = [];
					//allOptionsOfAllCells[i, j] = [];
				}
			};

			// actually do the lambda for all cells
			loopAll(findOptions, null, TotalSize, TotalSize, this, new object[1]);

			//return allOptionsOfAllCells;
		}


		private bool RecursiveSolve(
			Action<SudokuGrid, List<SudokuCell>>? printEmptyCellsStats,
			Action<SudokuGrid, SudokuCell, List<int>, int>? beforeRecursiveSolve,
			Action<SudokuGrid, bool, bool, SudokuCell, List<int>, int>? afterRecursiveSolve,
			Action<SudokuGrid, bool, SudokuCell>? afterAllOptionsTried,
			bool calcAllSolutions = true)
		{
			FindAndSetAllOptionsForAllCells();

			List<SudokuCell> emptyCells = [];

			// Get all cells that have options left by using a loop
			foreach (SudokuCell item in Grid) if (item.OptionsLeft.Count != 0) emptyCells.Add(item);

			printEmptyCellsStats?.Invoke(this, emptyCells);
			// if there is no item in the List it could be finished or a unsolvable
			// if the following statement is true, the end of this branch is near
			if (emptyCells.Count == 0)
			{
				// Its solved if all Cells have a Value

				if (Convert2DArrayTo1D(Grid).All(a => a.Value != null))
				{
					int[,] thisSolution = new int[TotalSize, TotalSize];

					AllAction makeArrayOfValues = (int x, int y, SudokuGrid grid, params object[] parameters) =>
					{
						// Since all Values should be set here, no null should occur
						thisSolution[x, y] = (int)grid.Grid[x, y].Value;
					};
					loopAll(makeArrayOfValues, null, TotalSize, TotalSize, this, []);

					possibleSolutionsSaved.Add(thisSolution);
					return true;
				}

				// else there are cells Empty, without options left. 
				// So the sudoku isnt solvable at this point
				return false;

			}
			// If this following code runs, the end of a branch isnt found yet, so there has to be searched for more

			// Sort empty cells by the amount of options left
			// The lowest options left should then be at the start of the list
			// There shouldnt be any Count 0 because of the loop before
			Sort.MergeSort(emptyCells, (a, b) => a.OptionsLeft.Count.CompareTo(b.OptionsLeft.Count));

			SudokuCell chosenCell = emptyCells[0];

			// Make an immutable List to try all numbers (options) left
			List<int> optionsToTry = [.. chosenCell.OptionsLeft];
			// When wanting all Solutions of this Sudoku, the following boolean is needed
			bool atLeastOneWorked = false;

			// Try all Numbers, the cell has as options
			foreach (int tryNumber in optionsToTry)
			{
				beforeRecursiveSolve?.Invoke(this, chosenCell, optionsToTry, tryNumber);

				// Try with new number
				chosenCell.Value = tryNumber;
				bool worked = RecursiveSolve(printEmptyCellsStats, beforeRecursiveSolve, afterRecursiveSolve, afterAllOptionsTried, calcAllSolutions);

				afterRecursiveSolve?.Invoke(this, worked, atLeastOneWorked | worked, chosenCell, optionsToTry, tryNumber);

				if (worked)
				{
					atLeastOneWorked = true;

					// If only one solution is wanted, return here
					if (!calcAllSolutions) return true;
				}

				// If more than one solution is wanted, or tryNumber isnt solvable, continue
				// We dont need to to anything else here, since here we are just recursivly coming back from a solved Sudoku
				//continue;
			}
			afterAllOptionsTried?.Invoke(this, atLeastOneWorked, chosenCell);

			chosenCell.Value = null;

			// When multipleSolutions are wanted, there might be some correct solved Sudoku
			// return this information
			return atLeastOneWorked;
		}
		public bool RecursiveSolve(bool calcAllSolutions = true,  Action<SudokuGrid, List<SudokuCell>>? printEmptyCellsStats = null, Action<SudokuGrid, SudokuCell, List<int>, int>? beforeRecursiveSolve = null, Action<SudokuGrid, bool, bool, SudokuCell, List<int>, int>? afterRecursiveSolve = null, Action<SudokuGrid, bool, SudokuCell>? afterAllOptionsTried = null)
		{
			// Check if Sudoku does have an solution thats worth calculating
			List<SudokuCell> listOfAllCells = Convert2DArrayTo1D(Grid).ToList();
			listOfAllCells.RemoveAll(a => a.Value == null);
			if (listOfAllCells.Count < 10) { Console.WriteLine("The input Sudoku hadnt had enough filled Cells." + ToString() + "\nThe minimus amount of filled cells in a 9x9 Sudoku is 17 (filled cells) to get a sudoku with only one solution"); return false; }
			
			possibleSolutionsSaved.Clear();
			return RecursiveSolve(printEmptyCellsStats, beforeRecursiveSolve, afterRecursiveSolve, afterAllOptionsTried, calcAllSolutions);
		}
		/// <summary>
		/// The function will solve the Sudoku. Returning if it was successful or not.
		/// </summary>
		/// <remarks>This funtion will call the RecursiveSolve funtion with some standard parameters. Call the RecursiveSolve function directly for the most efficent way of solving the sudoku</remarks>
		/// <param name="printLogs">If true, the solving process will be printed to the console. This usually takes much more time, so handle with care. We're talking from 1s to like 20s</param>
		/// <returns>Returns if the solving was succesfull</returns>
		public bool Solve(bool printLogs = false)
		{
			Action<SudokuGrid, List<SudokuCell>>? printEmptyCellsStats = printLogs ? ((SudokuGrid grid, List<SudokuCell> a) => Console.WriteLine("Empty cells at the moment: " + a.Count.ToString())) : null;
			Action<SudokuGrid, SudokuCell, List<int>, int>? beforeRecursiveSolve = printLogs ? ((SudokuGrid grid, SudokuCell chosenCell, List<int> optionsToTry, int tryNumber) =>
			{
				Console.WriteLine($"Trying {tryNumber} for cell {chosenCell.Position} with options left: {string.Join(", ", optionsToTry)}");
				StaticPrintGrid(this);
			}) : null;
			Action<SudokuGrid, bool, bool, SudokuCell, List<int>, int>? afterRecursiveSolve = printLogs ? ((SudokuGrid grid, bool worked, bool atLeastOneWorked, SudokuCell chosenCell, List<int> optionsToTry, int tryNumber) => {
				if (worked) Console.WriteLine($"The try with {tryNumber} for cell {chosenCell.Position} worked!");
				else Console.WriteLine($"The try with {tryNumber} for cell {chosenCell.Position} didnt work!");
			}) : null;
			Action<SudokuGrid, bool, SudokuCell>? afterAllOptionsTried = printLogs ? ((SudokuGrid grid, bool atLeastOneWorked, SudokuCell chosenCell) => {
				if (atLeastOneWorked)
				{
					Console.WriteLine($"At least one option for cell {chosenCell.Position} worked, going back to previous cell");
				}
				else
				{
					Console.WriteLine($"No option for cell {chosenCell.Position} worked, backtracking...");
				}
			}) : null;

			Console.WriteLine("Solving the sudoku has startet, this might take a while");
			bool IsSolved = RecursiveSolve(true, printEmptyCellsStats, beforeRecursiveSolve, afterRecursiveSolve, afterAllOptionsTried);
			Console.WriteLine($"Solving succeeded: {IsSolved}");
			return IsSolved;
		}
		

		public static void TestFunction()
		{
			SudokuGrid testGrid = new SudokuGrid(TestGrid1);
			testGrid.PrintEmptyGrid();
			Console.Write("Let's first make some fancy stuff, press any key to continue: ");
			Console.ReadKey();
			testGrid.Solve(true);
			testGrid.PrintAllSolutions();

			Task.Delay(2500).Wait();
			Console.WriteLine("-----------------------------\n");

			SudokuGrid testGrid2 = new SudokuGrid(TestGrid2);
			testGrid2.PrintEmptyGrid();
			Console.Write("Now for another test of speed, press any key to continue: ");
			Console.ReadKey();
			testGrid2.Solve(false);
			testGrid2.PrintAllSolutions();

			Task.Delay(2500).Wait();
			Console.WriteLine("-----------------------------\n");

			Console.WriteLine("Now you can put in some Sudoku you want to solve: ");
			SudokuGrid testGrid3 = new SudokuGrid(9);
			testGrid3.PrintEmptyGrid();
			testGrid3.Solve(false);
			testGrid3.PrintAllSolutions();

			Console.WriteLine();
			Console.Write("Thats it for now, press any key to continue: ");
			Console.ReadKey();
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
	
	public class SudokuCell {
		public int? Value;
		public bool IsDefRight;
		public (int, int) Position;
		public List<int> OptionsLeft = new();

		public SudokuCell(int? _value, (int, int) position, bool isDefRight = false)
		{
			IsDefRight = isDefRight;
			Value = _value;
			Position = position;
			if (isDefRight) OptionsLeft.Add((int)Value);
		}


		public override string ToString()
		{
			if (Value == null) return ".";
			else return Value.ToString(); // ingore the null warning, because its checked before. Please dont remove it, it helps in the Terminal input funtion of the SufdokuGrid class
		}
	}
}
