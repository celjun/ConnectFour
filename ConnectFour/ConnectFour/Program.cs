using System;
using System.Linq;

class ConnectFour
{
    private GameBoard board;
    private Player[] players;
    private int currentPlayerIndex;
    private bool aiEnabled;


    // Initialize the game, optionally with an AI opponent
    public ConnectFour(bool aiOpponent = false)
    {
        board = new GameBoard();

        // Create 2 players: human vs. human or human vs. AI
        players = new Player[] { new Player('X'), aiOpponent ? new AIPlayer('O') : new Player('O') };
        currentPlayerIndex = 0;
        aiEnabled = aiOpponent;
    }

    // Starts and runs the game loop until win or draw
    public void Play()
    {
        bool gameOver = false;
        while (!gameOver)
        {
            board.Display();
            int column;

            // If it's the AI's turn, get the AI's move
            if (players[currentPlayerIndex] is AIPlayer aiPlayer)
            {
                column = aiPlayer.ChooseMove(board);
                Console.WriteLine($"AI chooses column {column}");
            }
            else
            {
                // Human player's turn: ask for input
                Console.WriteLine($"Player {players[currentPlayerIndex].Symbol}, choose a column (0-6): ");
                while (!int.TryParse(Console.ReadLine(), out column) || !board.DropDisc(column, players[currentPlayerIndex].Symbol))
                {
                    Console.WriteLine("Invalid move. Try again.");
                }
            }


            // Check if the current player has won
            if (board.CheckWin(players[currentPlayerIndex].Symbol))
            {
                board.Display();
                Console.WriteLine($"Player {players[currentPlayerIndex].Symbol} wins!");
                gameOver = true;
            }
            // If no win, check for draw (board is full)
            else if (board.IsFull())
            {
                board.Display();
                Console.WriteLine("It's a draw!");
                gameOver = true;
            }

            currentPlayerIndex = (currentPlayerIndex + 1) % 2;
        }
    }

}

// Represents the game board
class GameBoard
{
    private char[,] grid;
    private const int Rows = 6;
    private const int Cols = 7;


    // Initialize empty board with '.' characters
    public GameBoard()
    {
        grid = new char[Rows, Cols];
        for (int r = 0; r < Rows; r++)
            for (int c = 0; c < Cols; c++)
                grid[r, c] = '.';
    }

    // Display the current state of the board in the console
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

    public bool DropDisc(int column, char symbol)
    {
        if (column < 0 || column >= Cols) return false;

        for (int r = Rows - 1; r >= 0; r--)
        {
            if (grid[r, column] == '.')
            {
                grid[r, column] = symbol;
                return true;
            }
        }
        return false;
    }

    // Check for a win (4 in a row) for the given symbol
    public bool CheckWin(char symbol)
    {
        for (int r = 0; r < Rows; r++)
            for (int c = 0; c < Cols - 3; c++)
                if (grid[r, c] == symbol && grid[r, c + 1] == symbol && grid[r, c + 2] == symbol && grid[r, c + 3] == symbol)
                    return true;

        // Vertical check
        for (int r = 0; r < Rows - 3; r++)
            for (int c = 0; c < Cols; c++)
                if (grid[r, c] == symbol && grid[r + 1, c] == symbol && grid[r + 2, c] == symbol && grid[r + 3, c] == symbol)
                    return true;

        // Diagonal check (\ direction)
        for (int r = 0; r < Rows - 3; r++)
            for (int c = 0; c < Cols - 3; c++)
                if (grid[r, c] == symbol && grid[r + 1, c + 1] == symbol && grid[r + 2, c + 2] == symbol && grid[r + 3, c + 3] == symbol)
                    return true;

        // Diagonal check (/ direction)
        for (int r = 3; r < Rows; r++)
            for (int c = 0; c < Cols - 3; c++)
                if (grid[r, c] == symbol && grid[r - 1, c + 1] == symbol && grid[r - 2, c + 2] == symbol && grid[r - 3, c + 3] == symbol)
                    return true;

        return false;
    }

    // Check if the board is completely filled
    public bool IsFull()
    {
        for (int c = 0; c < Cols; c++)
            if (grid[0, c] == '.')
                return false;
        return true;
    }

    // Return array of columns that are still available to play
    public int[] GetAvailableColumns()
    {
        return Enumerable.Range(0, Cols).Where(c => grid[0, c] == '.').ToArray();
    }
}


// Base Player class (used for both human and AI)
class Player
{
    public char Symbol { get; }
    public Player(char symbol) => Symbol = symbol;
}

// AI Player that randomly selects a valid column
class AIPlayer : Player
{
    private Random random = new Random();
    public AIPlayer(char symbol) : base(symbol) { }


    // AI logic: pick a random available column
    public int ChooseMove(GameBoard board)
    {
        int[] availableColumns = board.GetAvailableColumns();
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
    }
}
