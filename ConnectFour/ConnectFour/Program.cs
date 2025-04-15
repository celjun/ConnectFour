using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

// Main Connect Four game class
class ConnectFour
{
    private GameBoard board;
    private Player[] players;
    private int currentPlayerIndex;
    private bool aiEnabled;
    private List<string> winHistory;

    // Constructor initializes board, players, and loads win history
    public ConnectFour(bool aiOpponent = false)
    {
        board = new GameBoard();
        players = new Player[]
        {
            new Player('X'),
            aiOpponent ? new AIPlayer('O') : new Player('O')
        };
        currentPlayerIndex = 0;
        aiEnabled = aiOpponent;
        winHistory = new List<string>();

        players[0].SetName(); // Ask for name for Player X
        if (!aiEnabled)
        {
            players[1].SetName(); // Ask for name for Player O (unless it's AI)
        }

        LoadWinHistory(); // Load past win records from file
    }

    // Core gameplay loop
    public void Play()
    {
        bool gameOver = false;
        while (!gameOver)
        {
            board.Display(); // Show the current board
            int column;

            if (players[currentPlayerIndex] is AIPlayer aiPlayer)
            {
                // Let AI choose its move
                column = aiPlayer.ChooseMove(board);
                Console.WriteLine($"\n{aiPlayer.Name} chooses column {column}");
                board.DropDisc(column, aiPlayer.Symbol);
            }
            else
            {
                // Ask user for a valid move
                Console.WriteLine($"\n{players[currentPlayerIndex].Name} (Player {players[currentPlayerIndex].Symbol}), choose a column (0-6): ");
                while (!int.TryParse(Console.ReadLine(), out column) || board.DropDisc(column, players[currentPlayerIndex].Symbol) == -1)
                {
                    Console.WriteLine("Invalid move. Try again: ");
                }
            }

            // Check for a win
            if (board.CheckWin(players[currentPlayerIndex].Symbol))
            {
                board.Display();
                Console.WriteLine($"\n{players[currentPlayerIndex].Name} (Player {players[currentPlayerIndex].Symbol}) wins!");
                winHistory.Add($"{players[currentPlayerIndex].Name} wins!");
                SaveWinHistory();
                gameOver = true;
            }
            // Check for a draw
            else if (board.IsFull())
            {
                board.Display();
                Console.WriteLine("\nIt's a draw!");
                winHistory.Add("Draw");
                SaveWinHistory();
                gameOver = true;
            }

            currentPlayerIndex = (currentPlayerIndex + 1) % 2; // Switch turns
        }
    }

    // Show past winners and draws
    public void DisplayWinHistory()
    {
        Console.WriteLine("\nWin History:");
        if (winHistory.Count == 0)
        {
            Console.WriteLine("No wins yet.");
        }
        else
        {
            foreach (string record in winHistory)
            {
                Console.WriteLine(record);
            }
        }
    }

    // Load history from file
    private void LoadWinHistory()
    {
        if (File.Exists("winHistory.txt"))
        {
            winHistory = File.ReadAllLines("winHistory.txt").ToList();
        }
    }

    // Save updated history to file
    private void SaveWinHistory()
    {
        File.WriteAllLines("winHistory.txt", winHistory);
    }
}

// Board representation and logic
class GameBoard
{
    private char[,] grid;
    private const int Rows = 6;
    private const int Cols = 7;

    public GameBoard()
    {
        grid = new char[Rows, Cols];
        for (int r = 0; r < Rows; r++)
            for (int c = 0; c < Cols; c++)
                grid[r, c] = '.'; // Initialize all cells as empty
    }

    // Display the board
    public void Display()
    {
        Console.WriteLine();
        for (int r = 0; r < Rows; r++)
        {
            for (int c = 0; c < Cols; c++)
            {
                Console.Write(grid[r, c] + " ");
            }
            Console.WriteLine();
        }
        Console.WriteLine("0 1 2 3 4 5 6");
    }

    // Drop a disc into a column
    public int DropDisc(int column, char symbol)
    {
        if (column < 0 || column >= Cols) return -1;

        for (int r = Rows - 1; r >= 0; r--)
        {
            if (grid[r, column] == '.')
            {
                grid[r, column] = symbol;
                return r; // Return the row where the disc landed
            }
        }
        return -1; // Column is full
    }

    // Undo the disc at a specific location (for AI trial moves)
    public bool UndoDrop(int row, int column)
    {
        if (row >= 0 && row < Rows && column >= 0 && column < Cols && grid[row, column] != '.')
        {
            grid[row, column] = '.';
            return true;
        }
        return false;
    }

    // Check if a player has won
    public bool CheckWin(char symbol)
    {
        // Horizontal
        for (int r = 0; r < Rows; r++)
            for (int c = 0; c < Cols - 3; c++)
                if (grid[r, c] == symbol && grid[r, c + 1] == symbol && grid[r, c + 2] == symbol && grid[r, c + 3] == symbol)
                    return true;

        // Vertical
        for (int r = 0; r < Rows - 3; r++)
            for (int c = 0; c < Cols; c++)
                if (grid[r, c] == symbol && grid[r + 1, c] == symbol && grid[r + 2, c] == symbol && grid[r + 3, c] == symbol)
                    return true;

        // Diagonal down-right
        for (int r = 0; r < Rows - 3; r++)
            for (int c = 0; c < Cols - 3; c++)
                if (grid[r, c] == symbol && grid[r + 1, c + 1] == symbol && grid[r + 2, c + 2] == symbol && grid[r + 3, c + 3] == symbol)
                    return true;

        // Diagonal up-right
        for (int r = 3; r < Rows; r++)
            for (int c = 0; c < Cols - 3; c++)
                if (grid[r, c] == symbol && grid[r - 1, c + 1] == symbol && grid[r - 2, c + 2] == symbol && grid[r - 3, c + 3] == symbol)
                    return true;

        return false;
    }

    // Check if the board is full (draw)
    public bool IsFull()
    {
        for (int c = 0; c < Cols; c++)
            if (grid[0, c] == '.')
                return false;
        return true;
    }

    // Return a list of available columns (for AI)
    public int[] GetAvailableColumns()
    {
        return Enumerable.Range(0, Cols).Where(c => grid[0, c] == '.').ToArray();
    }
}

// Player class (base for both human and AI)
class Player
{
    public char Symbol { get; }
    public string Name { get; protected set; }

    public Player(char symbol) => Symbol = symbol;

    // Prompt user to enter name and validate it
    public virtual void SetName()
    {
        Console.WriteLine($"Enter a name for Player {Symbol}: ");
        string name;
        while (true)
        {
            name = Console.ReadLine().Trim();
            if (IsValidName(name))
            {
                Name = name;
                break;
            }
            else
            {
                Console.WriteLine("Invalid name. Name must be between 1 and 20 characters and cannot contain special characters. Try again: ");
            }
        }
    }

    // Check if name is valid (alphanumeric, <= 20 characters)
    private bool IsValidName(string name)
    {
        return !string.IsNullOrEmpty(name) && name.Length <= 20 && Regex.IsMatch(name, @"^[a-zA-Z0-9\s]+$");
    }
}

// AI player with basic strategy
class AIPlayer : Player
{
    private Random random = new Random();

    public AIPlayer(char symbol) : base(symbol)
    {
        Name = "AI Bot"; // Default name
    }

    // Choose best move based on win/block/dumb strategy
    public int ChooseMove(GameBoard board)
    {
        int[] availableColumns = board.GetAvailableColumns();

        // Try to win
        foreach (int col in availableColumns)
        {
            int row = board.DropDisc(col, Symbol);
            if (row != -1 && board.CheckWin(Symbol))
            {
                board.UndoDrop(row, col);
                return col;
            }
            if (row != -1) board.UndoDrop(row, col);
        }

        // Try to block opponent
        foreach (int col in availableColumns)
        {
            int row = board.DropDisc(col, 'X'); // Assume opponent is X
            if (row != -1 && board.CheckWin('X'))
            {
                board.UndoDrop(row, col);
                return col;
            }
            if (row != -1) board.UndoDrop(row, col);
        }

        // Else choose random move
        return availableColumns[random.Next(availableColumns.Length)];
    }
}

// Program entry point
class Program
{
    static void Main()
    {
        Console.WriteLine("Play against AI? (yes/no): ");
        bool aiOpponent = Console.ReadLine()?.Trim().ToLower() == "yes";

        ConnectFour game = new ConnectFour(aiOpponent);

        bool playAgain = true;
        while (playAgain)
        {
            game.Play();
            game.DisplayWinHistory();

            Console.WriteLine("\nPlay another round? (yes/no): ");
            string response = Console.ReadLine()?.Trim().ToLower();
            if (response != "yes")
            {
                playAgain = false;
            }
        }

        Console.WriteLine("\nThanks for playing! Press Enter to exit...");
        Console.ReadLine();
    }
}
