
using System;
using Sudoku;

namespace Sudoku {
	public partial class SudokuGrid {

		public SudokuGrid (string txtFile) {
			StreamReader reader = new(txtFile);
			List<int?> OneDimCells = new();
			// 0 -> not set yet
			int boxWideness = 0;
			int sudokuSize = 0;
			do {
				int readNum = reader.Peek();
				bool isNumber = TryForNumber(reader, readNum, out int foundNum);

				// End of file
				if (readNum < 0) { break; }

				else if (isNumber) {
					OneDimCells.Add(foundNum);
					continue;
				}

				char readChar = (char)readNum;

				switch (readChar) {
					case '.':
						// Empty Cell [46]
						do { reader.Read();
						} while (reader.Peek() == '.');
						OneDimCells.Add(null);
						continue;
					case '#':
						// Comment: skip line [35]
						reader.ReadLine();
						continue;
					case '|':
						// box wideness [124]
						reader.Read(); 
						if (boxWideness == 0) {
							boxWideness = OneDimCells.Count;
						}
						continue;
					case '-':
						// sudoku size [45]
						reader.Read();
						if (sudokuSize == 0) sudokuSize = OneDimCells.Count;
						continue;
					default:
						// next character
						reader.Read();
						break;
				}
			} while (true);

			// Check if grid has the correct total size
			Console.WriteLine($"SudokuSize: {sudokuSize}   CellCount from file: {OneDimCells.Count}");
			if (sudokuSize * sudokuSize == OneDimCells.Count) {}
			else { throw new Exception("the read sudoku hadnt had the correct size regarding the outer size and cell count!"); }

			// Assemble grid
			Grid = new SudokuCell[sudokuSize,sudokuSize];

			int currentIndex = 0;
			// x and y are reversed because its necessary to first iterate horizontally
			for (int y = 0; y < sudokuSize; y++) for (int x = 0; x < sudokuSize; x++) {
				Grid[x,y] = new SudokuCell(OneDimCells[currentIndex], (x,y), OneDimCells[currentIndex] != null);
				currentIndex++;
			}

			BoxSize = (boxWideness, sudokuSize / boxWideness);
		}

		private bool TryForNumber (StreamReader sr, int readNum, out int foundNum){
			foundNum = 0;
			if (IsNumber(readNum)) {
				// Read character is actually a number and not a char

				string allDigits = "";
				// check if next character is also a number
				bool isDig = true;
				do {
					int nextDig = sr.Read();

					isDig = IsNumber(nextDig);

					if(isDig) {
						// The next character is also a number
						allDigits += (nextDig-48).ToString();
					}
				} while (isDig);

				foundNum = Int32.Parse(allDigits);
				return true;
			}
			else {
				// Its no number
				return false;
			}
		}

		// 1: [48]    9: [58]
		private bool IsNumber(int readNum) => readNum>48 && readNum<58;

		private bool IsNumber(char character) => IsNumber((char)character);
	}
}
