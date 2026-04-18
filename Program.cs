using System.IO;

namespace Sudoku {
	class TestProgram {
		public static void Main(string[] args)
		{
			/*
			StreamReader rs = new("hi.txt");
			Console.WriteLine(rs.Read());
			Console.WriteLine(rs.Read());
			Console.WriteLine(rs.Read());
			Console.WriteLine(rs.Read());
			int ten = 49;
			int nine = 57;
			Console.WriteLine(ten.ToString() + "  " + (char)ten);
			Console.WriteLine(nine.ToString() + "  " + (char)nine);
			*/

			List<string> lArgs = args.ToList();
			if (lArgs.Contains("--test")) SudokuGrid.TestFunction();
			if(lArgs.Contains("--new") || lArgs.Count == 0) NewSudoku(lArgs);
			if(lArgs.Contains("--file")) FromFile(lArgs);
		} 

		private static void FromFile(List<string> args){
			int indx = args.IndexOf("--file");
			string path = "";
			try {
				path = args[indx+1];
			} catch (ArgumentOutOfRangeException e) {
				Console.WriteLine("!!!------------------!!!");
				Console.WriteLine("You must provide a path to yoursudoku.txt file right after the '--file' flag");
				Console.WriteLine("");
				throw e;
			}

			SudokuGrid fromFile = new("TestSdk.txt");
			bool exit = false;
			do {
				AskAgain:
				SudokuGrid.StaticPrintGrid(fromFile);
				int input = GetUserInput(["Solve sudoku", "Edit sudoku", "exit"]);
				switch (input) {
					case 0:
						solve(fromFile);
						return;
					case 1:
						fromFile.ChangeGridInTerminal();
						goto AskAgain;
					case 2:
						exit = true;
						break;
					default:
						solve(fromFile);
						return;
				}
			} while (!exit);

		}


		
		private static void NewSudoku(List<string> args){
			NewSudoku:
			SudokuGrid sdk = new(9);

			EditSudoku:
			sdk.ChangeGridInTerminal();
			SudokuGrid.StaticPrintGrid(sdk);

			int answer = GetUserInput(["Solve sudoku", "Edit sudoku", "Start fresh"]);
			switch (answer) {
				case 0:
					// Solve sdk
					solve(sdk);
					break;
				case 1:
					// Edit
					goto EditSudoku;
				case 2:
					// New
					goto NewSudoku;
				default:
					solve(sdk);
					break;
			}

		}

		private static void solve(SudokuGrid sdk){
			sdk.Solve(false);
			Console.WriteLine("");
			sdk.PrintAllSolutions();
		}

		private static int GetUserInput(string[] options){
			int numLength = options.Length.ToString().Length;
			for (int i = 0; i < options.Length; i++){
				Console.Write(i);
				Console.Write(new string(' ', numLength-i.ToString().Length + 2));
				Console.WriteLine(options[i]);
			}
			Console.WriteLine("Choose one option above! ");
			int answer;
			while(true) {
				string sAnswer = Console.ReadLine();
				if (Int32.TryParse(sAnswer, out answer)){
					return answer;
				}
				else if (sAnswer == "") return 0;
				else Console.WriteLine("Your input was incorrect!");
			}
		}
	}

	
}
