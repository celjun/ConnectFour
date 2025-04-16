using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

// Main Connect Four game class
class ConnectFour
{
    private GameBoard board;
    private Player[] players;
    private int currentPlayerIndex;
    private bool aiEnabled;

    public ConnectFour(bool aiOpponent = false)
    {
        board = new GameBoard(); // Initialize the game board

        // Create two players: Player 1 is always human, Player 2 may be AI
        players = new Player[]
        {
            new Player('X'),
            aiOpponent ? new AIPlayer('O') : new Player('O')
        };

        currentPlayerIndex = 0;
        aiEnabled = aiOpponent;

        // Ask player(s) to set their names
        players[0].SetName();
        if (!aiEnabled)
        {
            players[1].SetName();
        }
    }

    // Main game loop
    public void Play()
    {
        board = new GameBoard(); // Reset the board
        currentPlayerIndex = 0;
        bool gameOver = false;

        while (!gameOver)
        {
            board.Display(); // Show the current board
            int column;

            // AI makes a move
            if (players[currentPlayerIndex] is AIPlayer aiPlayer)
            {
                column = aiPlayer.ChooseMove(board);
                Console.WriteLine($"\n{aiPlayer.Name} chooses column {column}");
                board.DropDisc(column, aiPlayer.Symbol);
            }
            else
            {
                // Human player chooses a move
                Console.WriteLine($"\n{players[currentPlayerIndex].Name} (Player {players[currentPlayerIndex].Symbol}), choose a column (0-6): ");
                while (!int.TryParse(Console.ReadLine(), out column) || board.DropDisc(column, players[currentPlayerIndex].Symbol) == -1)
                {
                    Console.WriteLine("Invalid move. Try again: ");
                }
            }

            // Check if current player has won
            if (board.CheckWin(players[currentPlayerIndex].Symbol))
            {
                board.Display();
                Console.WriteLine($"\n{players[currentPlayerIndex].Name} (Player {players[currentPlayerIndex].Symbol}) wins!");
                gameOver = true;
            }
            else if (board.IsFull()) // Check for draw
            {
                board.Display();
                Console.WriteLine("\nIt's a draw!");
                gameOver = true;
            }

            // Switch to the next player
            currentPlayerIndex = (currentPlayerIndex + 1) % 2;
        }
    }
}

// Class that manages the board state
class GameBoard
{
    private char[,] grid;
    private const int Rows = 6;
    private const int Cols = 7;

    public GameBoard()
    {
        grid = new char[Rows, Cols];

        // Initialize the board with dots (empty slots)
        for (int r = 0; r < Rows; r++)
            for (int c = 0; c < Cols; c++)
                grid[r, c] = '.';
    }

    // Display the current board to the console
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
        Console.WriteLine("0 1 2 3 4 5 6"); // Show column numbers
    }

    // Drop a disc into a column; returns row index or -1 if invalid
    public int DropDisc(int column, char symbol)
    {
        if (column < 0 || column >= Cols) return -1;

        // Place disc in lowest available row
        for (int r = Rows - 1; r >= 0; r--)
        {
            if (grid[r, column] == '.')
            {
                grid[r, column] = symbol;
                return r;
            }
        }
        return -1; // Column is full
    }

    // Undo a move (used by AI for simulation)
    public bool UndoDrop(int row, int column)
    {
        if (row >= 0 && row < Rows && column >= 0 && column < Cols && grid[row, column] != '.')
        {
            grid[row, column] = '.';
            return true;
        }
        return false;
    }

    // Check for a win for a given symbol
    public bool CheckWin(char symbol)
    {
        // Horizontal check
        for (int r = 0; r < Rows; r++)
            for (int c = 0; c < Cols - 3; c++)
                if (grid[r, c] == symbol && grid[r, c + 1] == symbol && grid[r, c + 2] == symbol && grid[r, c + 3] == symbol)
                    return true;

        // Vertical check
        for (int r = 0; r < Rows - 3; r++)
            for (int c = 0; c < Cols; c++)
                if (grid[r, c] == symbol && grid[r + 1, c] == symbol && grid[r + 2, c] == symbol && grid[r + 3, c] == symbol)
                    return true;

        // Diagonal down-right check
        for (int r = 0; r < Rows - 3; r++)
            for (int c = 0; c < Cols - 3; c++)
                if (grid[r, c] == symbol && grid[r + 1, c + 1] == symbol && grid[r + 2, c + 2] == symbol && grid[r + 3, c + 3] == symbol)
                    return true;

        // Diagonal up-right check
        for (int r = 3; r < Rows; r++)
            for (int c = 0; c < Cols - 3; c++)
                if (grid[r, c] == symbol && grid[r - 1, c + 1] == symbol && grid[r - 2, c + 2] == symbol && grid[r - 3, c + 3] == symbol)
                    return true;

        return false;
    }

    // Check if the board is full
    public bool IsFull()
    {
        for (int c = 0; c < Cols; c++)
            if (grid[0, c] == '.')
                return false;
        return true;
    }

    // Get all columns that are not full
    public int[] GetAvailableColumns()
    {
        return Enumerable.Range(0, Cols).Where(c => grid[0, c] == '.').ToArray();
    }
}

// Base Player class
class Player
{
    public char Symbol { get; }
    public string Name { get; protected set; }

    public Player(char symbol) => Symbol = symbol;

    // Ask user to enter a name for the player
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
                Console.WriteLine("Invalid name. Name must be 1–20 characters and contain no special characters. Try again: ");
            }
        }
    }

    // Validate player name (no special characters, max 20 chars)
    private bool IsValidName(string name)
    {
        return !string.IsNullOrEmpty(name) && name.Length <= 20 && Regex.IsMatch(name, @"^[a-zA-Z0-9\s]+$");
    }
}

// AI Player subclass that picks moves intelligently
class AIPlayer : Player
{
    private Random random = new Random();

    public AIPlayer(char symbol) : base(symbol)
    {
        Name = "AI Bot"; // Fixed name for AI
    }

    // Decide which move to make
    public int ChooseMove(GameBoard board)
    {
        int[] availableColumns = board.GetAvailableColumns();

        // Try to win if possible
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

        // Try to block opponent if they're about to win
        foreach (int col in availableColumns)
        {
            int row = board.DropDisc(col, 'X');
            if (row != -1 && board.CheckWin('X'))
            {
                board.UndoDrop(row, col);
                return col;
            }
            if (row != -1) board.UndoDrop(row, col);
        }

        // Pick random column if no immediate threat or win
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

            Console.WriteLine("\nPlay another round? (yes/no): ");
            string response = Console.ReadLine()?.Trim().ToLower();
            playAgain = response == "yes";
        }

        Console.WriteLine("\nThanks for playing! Press Enter to exit...");
        Console.ReadLine();
    }
}
