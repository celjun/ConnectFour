using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

class ConnectFour
{
    private GameBoard board;
    private Player[] players;
    private int currentPlayerIndex;
    private bool aiEnabled;
    private List<string> winHistory; // To store win history

    // Initialize the game, optionally with an AI opponent
    public ConnectFour(bool aiOpponent = false)
    {
        board = new GameBoard();
        players = new Player[] { new Player('X'), aiOpponent ? new AIPlayer('O') : new Player('O') };
        currentPlayerIndex = 0;
        aiEnabled = aiOpponent;
        winHistory = new List<string>();

        // Prompt for player names
        players[0].SetName();
        if (!aiEnabled)
        {
            players[1].SetName();
        }

        LoadWinHistory(); // Load win history when starting a new game
    }

    // Starts and runs the game loop until win or draw
    public void Play()
    {
        bool gameOver = false;
        while (!gameOver)
        {
            board.Display();
            int column;

            if (players[currentPlayerIndex] is AIPlayer aiPlayer)
            {
                column = aiPlayer.ChooseMove(board);
                Console.WriteLine($"AI chooses column {column}");
                board.DropDisc(column, players[currentPlayerIndex].Symbol);
            }
            else
            {
                Console.WriteLine($"{players[currentPlayerIndex].Name} (Player {players[currentPlayerIndex].Symbol}), choose a column (0-6): ");
                while (!int.TryParse(Console.ReadLine(), out column) || board.DropDisc(column, players[currentPlayerIndex].Symbol) == -1)
                {
                    Console.WriteLine("Invalid move. Try again.");
                }
            }

            if (board.CheckWin(players[currentPlayerIndex].Symbol))
            {
                board.Display();
                Console.WriteLine($"{players[currentPlayerIndex].Name} (Player {players[currentPlayerIndex].Symbol}) wins!");
                winHistory.Add($"{players[currentPlayerIndex].Name} wins!");
                SaveWinHistory(); // Save the win history
                gameOver = true;
            }
            else if (board.IsFull())
            {
                board.Display();
                Console.WriteLine("It's a draw!");
                winHistory.Add("Draw");
                SaveWinHistory(); // Save the win history
                gameOver = true;
            }

            currentPlayerIndex = (currentPlayerIndex + 1) % 2;
        }
    }

    // Display the win history
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

    // Load the win history from a file
    private void LoadWinHistory()
    {
        if (File.Exists("winHistory.txt"))
        {
            winHistory = File.ReadAllLines("winHistory.txt").ToList();
        }
    }

    // Save the win history to a file
    private void SaveWinHistory()
    {
        File.WriteAllLines("winHistory.txt", winHistory);
    }
}

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
                grid[r, c] = '.';
    }

    public void Display()
    {
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

    public int DropDisc(int column, char symbol)
    {
        if (column < 0 || column >= Cols) return -1;

        for (int r = Rows - 1; r >= 0; r--)
        {
            if (grid[r, column] == '.')
            {
                grid[r, column] = symbol;
                return r;
            }
        }
        return -1;
    }

    public bool UndoDrop(int row, int column)
    {
        if (row >= 0 && row < Rows && column >= 0 && column < Cols && grid[row, column] != '.')
        {
            grid[row, column] = '.';
            return true;
        }
        return false;
    }

    public bool CheckWin(char symbol)
    {
        for (int r = 0; r < Rows; r++)
            for (int c = 0; c < Cols - 3; c++)
                if (grid[r, c] == symbol && grid[r, c + 1] == symbol && grid[r, c + 2] == symbol && grid[r, c + 3] == symbol)
                    return true;

        for (int r = 0; r < Rows - 3; r++)
            for (int c = 0; c < Cols; c++)
                if (grid[r, c] == symbol && grid[r + 1, c] == symbol && grid[r + 2, c] == symbol && grid[r + 3, c] == symbol)
                    return true;

        for (int r = 0; r < Rows - 3; r++)
            for (int c = 0; c < Cols - 3; c++)
                if (grid[r, c] == symbol && grid[r + 1, c + 1] == symbol && grid[r + 2, c + 2] == symbol && grid[r + 3, c + 3] == symbol)
                    return true;

        for (int r = 3; r < Rows; r++)
            for (int c = 0; c < Cols - 3; c++)
                if (grid[r, c] == symbol && grid[r - 1, c + 1] == symbol && grid[r - 2, c + 2] == symbol && grid[r - 3, c + 3] == symbol)
                    return true;

        return false;
    }

    public bool IsFull()
    {
        for (int c = 0; c < Cols; c++)
            if (grid[0, c] == '.')
                return false;
        return true;
    }

    public int[] GetAvailableColumns()
    {
        return Enumerable.Range(0, Cols).Where(c => grid[0, c] == '.').ToArray();
    }
}

class Player
{
    public char Symbol { get; }
    public string Name { get; private set; }

    public Player(char symbol) => Symbol = symbol;

    // Method to set a valid name
    public void SetName()
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

    // Validate the name (non-empty, no special characters, and between 1 and 20 characters)
    private bool IsValidName(string name)
    {
        return !string.IsNullOrEmpty(name) && name.Length <= 20 && Regex.IsMatch(name, @"^[a-zA-Z0-9\s]+$");
    }
}

class AIPlayer : Player
{
    private Random random = new Random();
    public AIPlayer(char symbol) : base(symbol) { }

    public int ChooseMove(GameBoard board)
    {
        int[] availableColumns = board.GetAvailableColumns();

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

        return availableColumns[random.Next(availableColumns.Length)];
    }
}

class Program
{
    static void Main()
    {
        Console.WriteLine("Play against AI? (yes/no): ");
        bool aiOpponent = Console.ReadLine()?.Trim().ToLower() == "yes";
        ConnectFour game = new ConnectFour(aiOpponent);
        game.Play();

        // Display win history after the game
        game.DisplayWinHistory();
    }
}
