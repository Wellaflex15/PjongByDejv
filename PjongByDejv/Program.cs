using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Threading;


namespace PjongByDejv
{


    public class Program
    {
        //TODO(david) Fix a line in the middle
        //TODO(david) Fix a opponet paddle
        //TODO(david) Fix movement to computer paddle(AI)
        //TODO(david) Fix controlls for player two
        //TODO(david) Fix Start/End Screen
        //TODO(david) Fix Score Panels
        //TODO(david) Sound
        //TODO(david) Game modes - Standard - with bonuses(smaller bigger paddles, obstacles, faster ball)
        //TODO(david) Difficulty levels
        //TODO(david) Options on the startscreen

        //Make console locked -> https://docs.microsoft.com/de-de/windows/desktop/menurc/wm-syscommand
        const int MF_BYCOMMAND = 0x00000000;
        const int SC_MINIMIZE = 0xF020;
        const int SC_MAXIMIZE = 0xF030;
        const int SC_SIZE = 0xF000;

        [DllImport("user32.dll")]
        public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();

        

        //Game size variables
        public static int height = 20;
        public static int width = 50;
        public static int ballX = 25;
        public static int ballY = 10;
        public static int ballXMove = 1;
        public static int ballYMove = 0;
        public static int playerPositionX = 3;
        public static int playerPositionY = 9;
        public static int playerLength = 3;
        public static int playerTwoPositionX = width - 4;
        public static int playerTwoPositionY = 9;
        public static int playerTwoLength = 3;

        public static int scorePositionNameX = 5;   
        public static int scorePositionNameY = 3;
        public static int scorePositionX = 5;   
        public static int scorePositionY = 2;   
        public static string playerOneName = "David";
        public static string playerTwoName = "Random";
        public static string computerName = "CPU";
        public static int playerOneScore = 0;
        public static int playerTwoScore = 0;
        public static int scoreToWin = 0;

        public static bool gameStart = true;
        public static bool gameScore = false;
        public static bool gameEnd = false;

        public static bool twoPlayers = false;

        public static int gameSpeed = 150; // Checking the thread sleep to change the game speed

        public static DateTime start;
        static void Main(string[] args)
        {
            //Lock the console -> so it can't break
            DeleteMenu(GetSystemMenu(GetConsoleWindow(), false), SC_MINIMIZE, MF_BYCOMMAND);
            DeleteMenu(GetSystemMenu(GetConsoleWindow(), false), SC_MAXIMIZE, MF_BYCOMMAND);
            DeleteMenu(GetSystemMenu(GetConsoleWindow(), false), SC_SIZE, MF_BYCOMMAND);

            Console.WindowHeight = 22;
            Console.WindowWidth = 52;
            Console.BufferHeight = 22;
            Console.BufferWidth = 52;
            Console.CursorVisible = false;
            // Create buffer to render to (create only once and store at class level)
            char[][] render = new char[height][];
            for (int y = 0; y < height; ++y)
                render[y] = new char[width];

            for (; ;)
            {
                if (gameStart)
                {
                    Console.WriteLine("Pjong by Dejv");
                    Console.WriteLine("Enter how many points are needed to win(1-9)");
                    bool validScore = false;
                    while (!validScore)
                    {
                        validScore = int.TryParse(Console.ReadLine(), out scoreToWin);
                        
                        if(scoreToWin <= 0 || scoreToWin > 9)
                        {
                            validScore = false;
                            Console.WriteLine("Number 1-9 please");
                        }
                    }
                    gameStart = false;

                    start = DateTime.Now;

                }
                else if (gameScore)
                {

                }
                else if (gameEnd)
                {

                }
                else
                {
                    TimeSpan duration = DateTime.Now - start;
                    Console.WriteLine(duration);
                    // Clear buffer and draw boarder
                    for (int y = 0; y < height; ++y)
                        for (int x = 0; x < width; ++x)
                            render[y][x] = ' ';
                    DrawBoarder(render);
                    // Draw game-stuff and player movement
                    DrawBall(render);
                    DrawPlayer(render);
                    DrawScore(render);
                    MovePlayer();
                    MovePlayerTwo(twoPlayers);

                    // Render to console
                    for (int y = 0; y < height; ++y)
                        Console.WriteLine(render[y]);
                    Thread.Sleep(gameSpeed);
                }
            }
        }


        public static void DrawBoarder(char[][] map)
        {
            for (int y = 0; y < height; ++y)
                for (int x = 0; x < width; ++x)
                {
                    if(x == 0 || x == width - 1)
                    {
                        map[y][x] = '|';
                    }
                    if(y == 0 || y == height - 1)
                    {
                        map[y][x] = '-';
                    }
                }
        }

        public static void DrawScore(char[][] map)
        {
            map[scorePositionY][scorePositionX] = Convert.ToChar(playerOneScore.ToString());
            map[scorePositionY][scorePositionX + 39] = Convert.ToChar(playerTwoScore.ToString());
        }

        public static void DrawPlayer(char[][] map)
        {
            for (int y = playerPositionY; y < playerPositionY + playerLength; y++)
            {
                map[y][playerPositionX] = 'X';
            }
            for (int y = playerTwoPositionY; y < playerTwoPositionY + playerTwoLength; y++)
            {
                map[y][playerTwoPositionX] = 'X';
            }
        }

        public static void DrawBall(char[][] map)
        {
            map[ballY][ballX] = 'O';

            //Score for player 1
            if (ballX == 1)
            {
                Console.Clear();
                Console.WriteLine("Poäng till {0}!\n", computerName);
                Console.WriteLine("Tryck på en knapp för att starta nästa boll!");

                ballX = 25;
                ballY = 10;
                ballXMove *= -1;
                ballYMove *= -1;
                //TODO(david) testar lite
                playerTwoScore++;
                Console.ReadLine();
            }

            //Score for player 2
            if (ballX == width - 2)
            {
                Console.Clear();
                Console.WriteLine("Poäng till {0}!\n", playerOneName);
                Console.WriteLine("Tryck på en knapp för att starta nästa boll!");

                ballX = 25;
                ballY = 10;
                ballXMove *= -1;
                ballYMove *= -1;
                //TODO(david) testar lite
                playerOneScore++;
                Console.ReadLine();
            }


            // Player 1 paddle touches
            if (ballX == playerPositionX && ballY == playerPositionY)
            {
                ballXMove *= -1;
                
                if(ballY > 1)
                    ballYMove = -2;
            }
            if (ballX == playerPositionX && ballY == playerPositionY + 1)
            {
                ballXMove *= -1;
                ballYMove = 0;
            }
            if (ballX == playerPositionX && ballY == playerPositionY + 2)
            {
                ballXMove *= -1;
                
                if(ballY < height - 2)
                    ballYMove = 2;
            }

            // Player 2 paddle touches
            if (ballX == playerTwoPositionX && ballY == playerTwoPositionY)
            {
                ballXMove *= -1;
                ballYMove = -2;
            }
            if (ballX == playerTwoPositionX && ballY == playerTwoPositionY + 1)
            {
                ballXMove *= -1;
                ballYMove = 0;
            }
            if (ballX == playerTwoPositionX && ballY == playerTwoPositionY + 2)
            {
                ballXMove *= -1;
                ballYMove = 2;
            }


            ballX += ballXMove;
            ballY += ballYMove;

            //TODO(david) Problem when corner touch paddle and top or bottom wall. This position seems to fix it. 
            // Bounces for Top and Bottom
            if (ballY >= height - 2 || ballY <= 1)
            {
                ballYMove *= -1;
            }
        }

        private static void MovePlayer()
        {
            if (Console.KeyAvailable)
            {
                var keyPressed = Console.ReadKey();

                switch (keyPressed.Key)
                {
                    case ConsoleKey.UpArrow:
                        if(playerPositionY > 1)
                            playerPositionY--;
                        break;
                    case ConsoleKey.DownArrow:
                        if(playerPositionY < height - playerLength - 1)
                            playerPositionY++;
                        break;                    
                    default:
                        // Do nothing
                        break;
                }
            }
        }

        private static void MovePlayerTwo(bool twoPlayers)
        {
            if (twoPlayers)
            {
                if (Console.KeyAvailable)
                {
                    var keyPressed = Console.ReadKey();

                    switch (keyPressed.Key)
                    {
                        case ConsoleKey.W:
                            if (playerTwoPositionY > 1)
                                playerTwoPositionY--;
                            break;
                        case ConsoleKey.S:
                            if (playerTwoPositionY < height - playerTwoLength - 1)
                                playerTwoPositionY++;
                            break;
                        default:
                            // Do nothing
                            break;
                    }
                }
            }
            else
            {
                //AI
                //Down
                if(ballY > playerTwoPositionY + playerTwoLength - 1)
                {
                    if (playerTwoPositionY < height - playerTwoLength - 1)
                        playerTwoPositionY++;
                }
                //Up
                if (ballY < playerTwoPositionY)
                {
                    if (playerTwoPositionY > 1)
                        playerTwoPositionY--;
                }
            }
        }
    }
}
