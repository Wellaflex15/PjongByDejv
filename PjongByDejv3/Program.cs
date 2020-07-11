using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PjongByDejv3
{
    class Program
    {
        /* Classic Pjong v.3 inspired by RWFRS */

        /* https://en.wikipedia.org/wiki/List_of_Unicode_characters */

        // Console variables and stuff


        // Game field variables
        private static int gameSpeed = 100;
        private static int gameHeight = 25;
        private static int gameWidth = 55;

        // The game buffer -> that I will draw to
        private static char[,] gameMap = new char[gameHeight, gameWidth];

        // Ball variables
        private static int ballX = gameWidth/2; // Start in the middle
        private static int ballY = gameHeight/2; // Start in the middle
        private static int ballMovementX = 1;
        private static int ballMovementY = 1;

        // Player variables 
        private static int playerOnePositionX = 2;
        private static int playerOnePositionY = (gameHeight/2 - 1); // If 3 long position starts in the middle
        private static int playerOneLength = 3; // Paddle length
        private static int playerTwoPositionX = gameWidth - 4;
        private static int playerTwoPositionY = (gameHeight / 2 - 1); // If 3 long position starts in the middle
        private static int playerTwoLength = 3; // Paddle length

        // Game state variables
        private static bool gameStart = true; // Set true if game start menu is to show
        private static bool gameScore = false; // If true one player has scored
        private static bool gameEnd = false; // A player has reached the winning score or ESC has been pressed to end the game
        private static bool gameRunning = true; // For the main game loop

        // Timing variables - for keeping track of the time
        private static DateTime startTime;
        private static TimeSpan gameDuration;
        private static DateTime endTime;

        // Menu items
        private static MenuChoice start = new MenuChoice("Start", true);
        private static MenuChoice highScore = new MenuChoice("HighScore", false);
        private static List<MenuChoice> menuChoices = new List<MenuChoice>();

        static void Main(string[] args)
        {
            HighScoreEntry highScoreEntry = new HighScoreEntry();
            highScoreEntry.Place = 1;
            highScoreEntry.Name = "David";
            highScoreEntry.Duration = DateTime.Now - DateTime.MinValue;
            highScoreEntry.Date = DateTime.Now;
            Console.WriteLine(highScoreEntry.ToString());

            Console.ReadLine();


            menuChoices.Add(start);
            menuChoices.Add(highScore);

            GameMenu gameMenu = new GameMenu("StartMenu", menuChoices); // GameMenuItems into the gameMenu
            
            Console.CursorVisible = false;

            Thread playerOneInputThread = new Thread(new ThreadStart(PlayerOneInputThread));
            Thread playerTwoInputThread = new Thread(new ThreadStart(PlayerTwoInputThread));


            while (gameRunning)
            {
                if (gameStart)
                {
                    gameMenu.ShowMenu();
                    playerOneInputThread.Start();
                    playerTwoInputThread.Start();
                    gameStart = false;
                }
                createBoarder(gameMap);
                drawBall(gameMap);
                drawPlayers(gameMap);
                drawGame(gameMap);
                
                // Slows the game down
                Thread.Sleep(gameSpeed);
            }



        }


        /// <summary>
        /// Thread that controlls the inputs from the player
        /// </summary>
        private static void PlayerOneInputThread()
        {


            while (gameRunning)
            {

                Thread.Sleep(10);
                
                if (Console.KeyAvailable)
                {
                    var keyPressed = Console.ReadKey(true);

                    switch (keyPressed.Key)
                    {
                        case ConsoleKey.Q:
                            if (playerOnePositionY > 1)
                                playerOnePositionY--;
                            break;
                        case ConsoleKey.A:
                            if (playerOnePositionY < gameHeight - playerOneLength - 1)
                                playerOnePositionY++;
                            break;
                        default:
                            // Do nothing
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Thread that controlls the AI for the CPU
        /// </summary>
        private static void PlayerTwoInputThread()
        {
            while (gameRunning)
            {

                Thread.Sleep(10);

                //Super AI created by Dejv
                Random RNG = new Random();
                int RandomPosition = RNG.Next(2, gameHeight - playerTwoLength - 1);
                int RandomNumber = RNG.Next(0, 10);

                //Up
                if (ballY < playerTwoPositionY)
                {
                    if (playerTwoPositionY > 1)
                        playerTwoPositionY--;
                }
                //Down
                if (ballY > playerTwoPositionY + playerTwoLength - 1)
                {
                    if (playerTwoPositionY < gameHeight - playerTwoLength - 1)
                        playerTwoPositionY++;
                }
                if (RandomNumber == 4)
                {
                    playerTwoPositionY = RandomPosition;
                }
            }

        }

        /// <summary>
        /// Creates the game boarder and game field
        /// </summary>
        /// <param name="gameMap">a char array that works as a buffer</param>
        private static void createBoarder(char[,] gameMap)
        {
            for (int heightPos = 0; heightPos < gameHeight; heightPos++)
            {
                for (int widthPos = 0; widthPos < gameWidth; widthPos++)
                {
                    gameMap[heightPos, widthPos] = ' ';

                    if (heightPos == 0)
                    {
                        gameMap[heightPos, widthPos] = '-';
                    }
                    if (heightPos == (gameHeight - 1))
                    {
                        gameMap[heightPos, widthPos] = '-';
                    }
                    if (widthPos == 0)
                    {
                        gameMap[heightPos, widthPos] = '█';
                    }
                    if(widthPos == (gameWidth - 1))
                    {
                        gameMap[heightPos, widthPos - 1] = '█';
                        gameMap[heightPos, widthPos] = '\n';
                    }
                    if(widthPos == (gameWidth/2) && (heightPos % 2 != 0) && (heightPos != 0))
                    {
                        gameMap[heightPos, widthPos] = '.';
                    }
                }
            }
        }

        /// <summary>
        /// Draws the ball and handle ball physics
        /// </summary>
        /// <param name="gameMap">a char array that works as a buffer</param>
        private static void drawBall(char[,] gameMap)
        {

            if ((ballX + ballMovementX <= 0) || (ballX + ballMovementX >= gameWidth - 2))
            {
                ballMovementX *= -1;
            }
            if ((ballY + ballMovementY <= 0) || (ballY + ballMovementY >= gameHeight - 1))
            {
                ballMovementY *= -1;
            }

            ballX += ballMovementX;
            ballY += ballMovementY;

            gameMap[ballY, ballX] = 'O';
        }

        /// <summary>
        /// Draw the players to the field - Player 1 and Player 2 or CPU
        /// </summary>
        /// <param name="gameMap"></param>
        private static void drawPlayers(char[,] gameMap)
        {
            for (int heightPos = playerOnePositionY; heightPos < playerOnePositionY + playerOneLength; heightPos++)
            {
                gameMap[heightPos, playerOnePositionX] = '█';
            }
            for (int heightPos = playerTwoPositionY; heightPos < playerTwoPositionY + playerTwoLength; heightPos++)
            {
                gameMap[heightPos, playerTwoPositionX] = '█';
            }
        }

        private static void drawGame(char[,] gameMap)
        {
            char[] drawGameString = new char[gameMap.Length];

            int numberOfBytes = Buffer.ByteLength(drawGameString);

            Buffer.BlockCopy(gameMap, 0, drawGameString, 0, numberOfBytes);

            Console.SetCursorPosition(0, 0);
            Console.Write(drawGameString);


        }

    }
}
