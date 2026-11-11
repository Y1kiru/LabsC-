using System;
using System.Collections.Generic;

abstract class Game
{
    public string GameType { get; }
    public Game(string gameType)
    {
        GameType = gameType;
    }
    public abstract int GetRating(string playerName);
}

class StandardGame : Game
{
    private int rating;
    public StandardGame(int rating) : base("Звичайна гра")

    {
        if (rating < 0)
            throw new ArgumentException("Рейтниг не може бути від'ємним");
    }
    public override int GetRating(string playerName)
    {
        return rating;
    }
}

class TrainingGame : Game
{

    public TrainingGame() : base("Тренувальна гра")
    {
    }

    public override int GetRating(string playerName)
    {
        return 0;
    }
}

class OnePlayerGame : Game
{
    private int rating;
    private string ratedPlayer;
    public OnePlayerGame(string ratedPlayer, int rating) : base("Гра на рейтниг одного гравця")
    {
        if (string.IsNullOrWhiteSpace(ratedPlayer))
            throw new ArgumentException("Ім'я гравця не може бути порожнім!");

        if (rating < 0)
            throw new ArgumentException("Рейтинг не може бути від'ємним!");

        this.ratedPlayer = ratedPlayer;
        this.rating = rating;
    }

    public override int GetRating(string playerName)
    {
        if (playerName == ratedPlayer)
            return rating;

        return 0;

    }
    public static Game CreateOnePlayerGame(string playerName, int rating)
    {
        return new OnePlayerGame(playerName, rating);
    }
}

static class GameFactory
{
    public static Game CreateStandardGame(int rating)
    {
        return new StandardGame(rating);
    }

    public static Game CreateTrainingGame()
    {
        return new TrainingGame();
    }

    public static Game CreateOnePlayerGame(string playerName, int rating)
    {
        return new OnePlayerGame(playerName, rating);
    }
}

class GameRecord
{
    private static int globalId = 1;

    public int Index { get; }
    public string Opponent { get; }
    public string GameType { get; }
    public bool IsWin { get; }
    public int Rating { get; }

    public GameRecord(string opponent, string gameType, bool isWin, int rating)
    {
        Index = globalId++;
        Opponent = opponent;
        GameType = gameType;
        IsWin = isWin;
        Rating = rating;
    }
}

abstract class GameAccount
{
    public string UserName { get; set; }
    public int CurrentRating { get; protected set; }
    public int GamesCount => history.Count;
    protected List<GameRecord> history = new List<GameRecord>();

    public GameAccount(string userName, int initialRating = 100)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("Ім'я гравця не може бути порожнім!");

        if (initialRating < 1)
            throw new ArgumentException("Початковий рейтинг має бути більше нуля!");

        UserName = userName;
        CurrentRating = initialRating;
    }
    protected abstract int CalculateRating(Game game, bool isWin);

    public void WinGame(string opponentName, Game game)
    {
        if (string.IsNullOrWhiteSpace(opponentName))
            throw new ArgumentException("Ім'я суперника не може бути порожнім!");

        if (game == null)
            throw new ArgumentException(nameof(game));

        int oldRating = CurrentRating;
        int points = CalculateRating(game, true);

        CurrentRating += points;
        history.Add(new GameRecord(opponentName, game.GameType, true, CurrentRating - oldRating));
    }

    public void LoseGame(string opponentName, Game game)
    {
        if (string.IsNullOrWhiteSpace(opponentName))
            throw new ArgumentException("Ім'я суперника не може бути порожнім!");

        if (game == null)
            throw new ArgumentException(nameof(game));

        int oldRating = CurrentRating;
        int points = CalculateRating(game, false);

        CurrentRating = Math.Max(1, CurrentRating - points);
        history.Add(new GameRecord(opponentName, game.GameType, false, CurrentRating - oldRating));
    }

    public void GetStats()
    {
        Console.WriteLine("\n========================================");
        Console.WriteLine($"Гравець: {UserName}");
        Console.WriteLine($"Поточний рейтинг: {CurrentRating}");
        Console.WriteLine($"Кількість ігор: {GamesCount}");
        Console.WriteLine("========================================");
        Console.WriteLine("ID | Опенент | Тип гри | Результ | Рейтинг ");

        foreach (var game in history)
        {
            string result = game.IsWin ? "Перемога" : "Поразка";
            Console.WriteLine(
            $"{game.Index,2} | {game.Opponent,-8} | " +
            $"{game.GameType,-30} | {result,-9} | {game.Rating}"
            );
        }
    }
}

class StandardGameAccount : GameAccount
{
    public StandardGameAccount(string userName, int rating = 100)
        : base(userName, rating)
    {
    }

    protected override int CalculateRating(Game game, bool isWin)
    {
        return game.GetRating(UserName);
    }
}
class ReducedLossGameAccount : GameAccount
{
    public ReducedLossGameAccount(string userName, int rating = 100)
        : base(userName, rating)
    {
    }

    protected override int CalculateRating(Game game, bool isWin)
    {
        int points = game.GetRating(UserName);

        if (!isWin)
            points /= 2;

        return points;
    }
}

class WinStreakGameAccount : GameAccount
{
    private int winStreak = 0;

    public WinStreakGameAccount(string userName, int rating = 100)
        : base(userName, rating)
    {
    }

    protected override int CalculateRating(Game game, bool isWin)
    {
        int points = game.GetRating(UserName);

        if (isWin)
        {
            winStreak++;

            if (winStreak >= 3)
                points += 10;
        }
        else
        {
            winStreak = 0;
        }

        return points;
    }
}

class Program
{
    static void Main()
    {
        GameAccount player1 = new StandardGameAccount("Іван");
        GameAccount player2 = new ReducedLossGameAccount("Олег");
        GameAccount player3 = new WinStreakGameAccount("Андрій");

        Game standardGame = GameFactory.CreateStandardGame(20);
        Game trainingGame = GameFactory.CreateTrainingGame();
        Game onePlayerGame = GameFactory.CreateOnePlayerGame("Іван", 30);

        player1.WinGame("Олег", standardGame);
        player2.LoseGame("Іван", standardGame);

        player1.LoseGame("Андрій", standardGame);
        player3.WinGame("Іван", standardGame);

        player2.WinGame("Андрій", trainingGame);
        player3.LoseGame("Олег", trainingGame);

        player3.WinGame("Іван", standardGame);
        player3.WinGame("Олег", standardGame);
        player3.WinGame("Іван", standardGame);

        player1.WinGame("Комп'ютер", onePlayerGame);
        player2.LoseGame("Комп'ютер", onePlayerGame);

        player1.GetStats();
        player2.GetStats();
        player3.GetStats();

        Console.WriteLine("\nПрограма завершила роботу.");
    }
}






