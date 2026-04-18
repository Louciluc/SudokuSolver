
using Sudoku;
using System;

namespace Sudoku {
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
			//if (isDefRight) OptionsLeft.Add((int)Value);
		}


		public override string ToString()
		{
			if (Value == null) return ".";
			else return Value.ToString(); // ingore the null warning, because its checked before. Please dont remove it, it helps in the Terminal input funtion of the SufdokuGrid class
		}
	}
}
