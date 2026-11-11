using System;
using System.Collections.Generic;

class Game
{
    private static int globalId = 1;
    public int Index { get; }
    public string Opponent { get; }
    public bool IsWin { get; }
    public int Rating { get; }

    public Game(string opponent, bool isWin, int rating)
    {
        Index = globalId++;
        Opponent = opponent;
        IsWin = isWin;
        Rating = rating;
    }
}

class GameAccount
{
    public string UserName { get; set; }
    public int CurrentRating { get; private set; }
    public int GamesCount => history.Count;
    private List<Game> history = new List<Game>();

    public GameAccount(string userName, int initialRating = 100)
    {
        UserName = userName;
        CurrentRating = initialRating < 1 ? 1 : initialRating;
    }

    public void WinGame(string opponentName, int rating)
    {
        if (rating < 0) throw new ArgumentException("Рейтинг не може бути від'ємним!");
        CurrentRating += rating;
        history.Add(new Game(opponentName, true, rating));
    }

    public void LoseGame(string opponentName, int rating)
    {
        if (rating < 0) throw new ArgumentException("Рейтинг не може бути від'ємним!");
        CurrentRating = Math.Max(1, CurrentRating - rating);
        history.Add(new Game(opponentName, false, rating));
    }

    public void GetStats()
    {
        Console.WriteLine($"\nСтатистика гравця {UserName} (Рейтинг: {CurrentRating})");
        Console.WriteLine("ID гри | Опонент | Результат | Рейтинг");

        foreach (var g in history)
        {
            string result = g.IsWin ? "Перемога" : "Поразка";
            Console.WriteLine($"{g.Index,6} | {g.Opponent,-7} | {result,-8} | {g.Rating}");
        }
    }
}

class Program
{
    static void Main()
    {
        GameAccount player1 = new GameAccount("Іван", 100);
        GameAccount player2 = new GameAccount("Олег", 100);

        player1.WinGame("Олег", 20);
        player2.LoseGame("Іван", 20);

        player1.LoseGame("Олег", 50);
        player2.WinGame("Іван", 50);

        player1.GetStats();
        player2.GetStats();
    }
}