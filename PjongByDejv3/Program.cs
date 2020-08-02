using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Speech.Synthesis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PjongByDejv3
{
    class Program
    {
        // TODO - Fix highscore and game menu
        // TODO - Speed increase -> Method
        // TODO - Move all speaking to SoundThread
        // TODO - Fix method for inserting new time in right place
        // TODO - save highscore
        // TODO - Refactor everything

        /* Classic Pjong v.3 inspired by RWFRS */

        /* https://en.wikipedia.org/wiki/List_of_Unicode_characters */

        #region ConsoleVariables
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
        #endregion

        #region Variables

        // Game field variables
        private static int gameSpeed = 100;
        private static int gameStartSpeed = 100;
        private static int gameHeight = 25;
        private static int gameWidth = 55;
        private static int scorePositionY = 5;
        private static int scorePositionX = 10;
        private static int playerOneScore = 0;
        private static int playerTwoScore = 0;
        private static int gameEndScore = 3;
        private static int globalCounter = 0;

        // The game buffer -> that I will draw to
        private static char[,] gameMap = new char[gameHeight, gameWidth];

        // Ball variables
        private static int ballXStartPosition = gameWidth/2; // Start in the middle
        private static int ballYStartPosition = gameHeight/2; // Start in the middle
        private static int ballX = gameWidth/2; // Start in the middle
        private static int ballY = gameHeight/2; // Start in the middle
        private static int ballMovementX = 1;
        private static int ballMovementY = 1;

        // Player variables 
        private static string playerOneName;
        private static int playerOnePositionX = 4;
        private static int playerOnePositionY = (gameHeight/2 - 1); // If 3 long position starts in the middle
        private static int playerOneLength = 4; // Paddle length
        private static int playerTwoPositionX = gameWidth - 4;
        private static int playerTwoPositionY = (gameHeight / 2 - 1); // If 3 long position starts in the middle
        private static int playerTwoLength = 4; // Paddle length

        // Game state variables
        private static bool gameStartAndRunning = true;
        private static bool gameStart = true; // Set true if game start menu is to show
        private static bool gameScore = false; // If true one player has scored
        private static bool playerOnePoint = false; // If true one player has scored
        private static bool playerTwoPoint = false; // If true one player has scored
        private static bool gameEnd = false; // A player has reached the winning score or ESC has been pressed to end the game
        private static bool gameRunning = true; // For the main game loop
        private static bool speedIncrease = false; // Bool for checkning if game speed should increase


        // Timing variables - for keeping track of the time
        private static DateTime startTime;
        private static TimeSpan gameDuration;
        private static DateTime endTime;

        // Menu items
        private static MenuChoice start = new MenuChoice("Start", true);
        private static MenuChoice highScore = new MenuChoice("HighScore", false);
        private static List<MenuChoice> menuChoices = new List<MenuChoice>();

        // CPU speech 
        //For CPU talking
        private static SpeechSynthesizer cpuTalk = new SpeechSynthesizer();
        #endregion

        #region Game Loop and logic
        static void Main(string[] args)
        {
            // Console settings
            DeleteMenu(GetSystemMenu(GetConsoleWindow(), false), SC_MINIMIZE, MF_BYCOMMAND);
            DeleteMenu(GetSystemMenu(GetConsoleWindow(), false), SC_MAXIMIZE, MF_BYCOMMAND);
            DeleteMenu(GetSystemMenu(GetConsoleWindow(), false), SC_SIZE, MF_BYCOMMAND);

            Console.WindowHeight = gameHeight + 2;
            Console.WindowWidth = gameWidth;
            Console.BufferHeight = gameHeight + 2;
            Console.BufferWidth = gameWidth;
            Console.CursorVisible = false;

            // Create menu for game
            menuChoices.Add(start);
            menuChoices.Add(highScore);
            GameMenu gameMenu = new GameMenu("StartMenu", menuChoices); // GameMenuItems into the gameMenu
            
            //Starting Threads
            Thread playerOneInputThread = new Thread(new ThreadStart(PlayerOneInputThread));
            Thread playerTwoInputThread = new Thread(new ThreadStart(PlayerTwoInputThread));
            Thread soundThread = new Thread(new ThreadStart(SoundThread));
            playerOneInputThread.Start();
            playerTwoInputThread.Start();
            soundThread.Start();

            // Get highscores from file
            HighScore.GetHighScores();

            while (gameStartAndRunning)
            {
                if (gameStart)
                {
                    HighScoreEntry a = new HighScoreEntry();
                    a.Place = 5;
                    a.Name = "Tuffs";
                    a.Duration = DateTime.Now - DateTime.UtcNow;
                    a.Date = DateTime.Now;

                    HighScore.ToFileFormat(a);

                    HighScore.AddHighScore(a);

                    gameMenu.ShowMenu();
                    Console.WriteLine("Enter your name and press enter:");
                    playerOneName = Console.ReadLine();
                    startTime = DateTime.Now;
                    gameStart = false;
                }
                else if(gameScore)
                {
                    Console.Clear();
                    ballX = ballXStartPosition;
                    ballY = ballYStartPosition;
                    gameSpeed = gameStartSpeed;

                    if (playerOnePoint)
                    {
                        Console.WriteLine("You scored!");
                        Console.WriteLine($"You: {playerOneScore} - CUP: {playerTwoScore}");
                        Console.WriteLine("Press Enter to start");
                        cpuSpeaks("You're lucky!", VoiceGender.Male, 5);
                        playerOnePoint = false;
                    }
                    else if (playerTwoPoint)
                    {
                        Console.WriteLine("CPU scored!");
                        Console.WriteLine($"You: {playerOneScore} - CUP: {playerTwoScore}");
                        Console.WriteLine("Press Enter to start");
                        cpuSpeaks("I'm the best! You will never beat me!", VoiceGender.Male, 5);
                        playerTwoPoint = false;
                    }

                    Console.ReadLine();
                    gameScore = false;
                }
                else if(gameEnd)
                {
                    endTime = DateTime.Now;
                    gameDuration = endTime - startTime;
                    Console.Clear();
                    ballX = ballXStartPosition;
                    ballY = ballYStartPosition;
                    gameSpeed = gameStartSpeed;
                    gameStart = true;
                    gameEnd = false;
                    if(playerOneScore > playerTwoScore)
                    {
                        Console.WriteLine($"You won! Your time is {gameDuration}");
                        Console.WriteLine();

                        foreach (var highscore in HighScore.HighScoresList)
                        {
                            Console.WriteLine(highscore.ToString());
                        }

                        Console.ReadLine();
                    }
                    else
                    {
                        Console.WriteLine("You lost! Here is the HIGHSCORE:");
                        Console.WriteLine();

                        foreach (var highscore in HighScore.HighScoresList)
                        {
                            Console.WriteLine(highscore.ToString());
                        }

                        cpuSpeaks("Haha! Go home! 3270! Beep beep boop boop! Ha ha ha!", VoiceGender.Male, 5);

                        Console.ReadLine();
                    }
                    playerOneScore = 0;
                    playerTwoScore = 0;
                    Console.Clear();
                }
                else if(gameRunning)
                {
                    createBoarder(gameMap);
                    drawInfoText(gameMap);
                    drawBall(gameMap);
                    drawPlayers(gameMap);
                    drawScore(gameMap);
                    drawGame(gameMap);
                    Console.WriteLine($"Name {playerOneName} Time: {DateTime.Now - startTime}");
                    
                    Thread.Sleep(gameSpeed);
                }
            }
        }

        #endregion

        #region Game Threads
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
                        case ConsoleKey.Enter:  // TDOD(david): Ta bort sen! För att debugga.
                            ballMovementY = 0;
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
                int RandomNumber = RNG.Next(0, 30);

                //Up
                if (ballY <= playerTwoPositionY)
                {
                    if (playerTwoPositionY > 1)
                        playerTwoPositionY--;
                }
                //Down
                if (ballY >= playerTwoPositionY + playerTwoLength - 1)
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

        private static void SoundThread()
        {
            while (gameRunning)
            {
                // Controlling player1 player paddle touches - Think it's working
                if (((ballY + ballMovementY) >= playerOnePositionY) && ((ballY + ballMovementY) <= (playerOnePositionY + playerOneLength)) && ((ballX + ballMovementX) == playerOnePositionX))
                {
                    cpuSpeaks("Boing", VoiceGender.Neutral, 10);
                    //if (ballY + ballMovementY >= playerOnePositionY + (playerOneLength / 2 - 1) && ballY + ballMovementY < playerOnePositionY + (playerOneLength / 2 + 1)) // 2 3 
                    //{
                    //    cpuSpeaks("Higher speed! Hell Yeaaaaaah!", VoiceGender.Neutral, 5);
                    //}
                }

                // Controlling player1 player paddle touches - Think it's working
                if (((ballY + ballMovementY) >= playerTwoPositionY) && ((ballY + ballMovementY) <= (playerTwoPositionY + playerTwoLength)) && ((ballX + ballMovementX) == playerTwoPositionX))
                {
                    cpuSpeaks("Boing", VoiceGender.Neutral, 10);
                    //if (ballY + ballMovementY >= playerTwoPositionY + (playerTwoLength / 2 - 1) && ballY + ballMovementY < playerTwoPositionY + (playerTwoLength / 2 + 1)) // 2 3 
                    //{
                    //    cpuSpeaks("Speed increase! Wooohoo!", VoiceGender.Neutral, 5);
                    //}
                }
            }
        }
        #endregion

        #region Game Methods
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
        /// Prints out info meassages on the screes. 
        /// </summary>
        /// <param name="gameMap"></param>
        private static void drawInfoText(char[,] gameMap)
        {
            // TODO(david): Fixa detta! 
            string speedMsg = "Speed Increase!";
            if (speedIncrease)
            {
                int i = 0;
                for (int messageStartPos = (gameWidth / 2) - (speedMsg.Length / 2); messageStartPos < ((gameWidth / 2) - (speedMsg.Length / 2)) + speedMsg.Length; messageStartPos++)
                {
                    gameMap[5, messageStartPos] = speedMsg[i++];
                }

                globalCounter++;

                if(globalCounter == 20)
                {
                    speedIncrease = false;
                    globalCounter = 0;
                }
            }
        }

        /// <summary>
        /// Draws the ball and handle ball physics
        /// </summary>
        /// <param name="gameMap">a char array that works as a buffer</param>
        private static void drawBall(char[,] gameMap)
        {

            // Controlling player1 player paddle touches - Think it's working
            if (((ballY + ballMovementY) >= playerOnePositionY) && ((ballY + ballMovementY) <= (playerOnePositionY + playerOneLength)) && ((ballX + ballMovementX) == playerOnePositionX))
            {
                if (ballY + ballMovementY == playerOnePositionY) // 1
                {
                    ballMovementY = -1;
                    ballMovementX *= -1;
                }
                else if (ballY + ballMovementY >= playerOnePositionY + (playerOneLength / 2 - 1) && ballY + ballMovementY < playerOnePositionY + (playerOneLength / 2 + 1)) // 2 3 
                {
                    // TODO(david): funkar bara på 3:an inte på 2:an... Undersök
                    if (gameSpeed > 20)
                    {
                        gameSpeed -= 20;
                        speedIncrease = true;
                    }
                    ballMovementY = 0;
                    ballMovementX *= -1;
                }
                else if (ballY + ballMovementY == playerOnePositionY + playerOneLength - 1) // 4
                {
                    ballMovementY = 1;
                    ballMovementX *= -1;
                }
                else
                {
                    //Nothing
                }
            }

            // Controlling CPU paddle touches - Think it's working
            if (((ballY + ballMovementY) >= playerTwoPositionY) && ((ballY + ballMovementY) <= (playerTwoPositionY + playerTwoLength)) && ((ballX + ballMovementX) == playerTwoPositionX))
            {
                if (ballY + ballMovementY == playerTwoPositionY) // 1
                {
                    ballMovementY = -1;
                    ballMovementX *= -1;
                }
                else if (ballY + ballMovementY >= playerTwoPositionY + (playerTwoLength / 2 - 1) && ballY + ballMovementY < playerTwoPositionY + (playerTwoLength / 2 + 1)) // 2 3 
                {
                    // TODO(david): funkar bara på 3:an inte på 2:an... Undersök
                    if(gameSpeed > 20)
                    {
                        gameSpeed -= 20;
                        speedIncrease = true;
                    }
                    ballMovementY = 0;
                    ballMovementX *= -1;
                }
                else if (ballY + ballMovementY == playerTwoPositionY + playerTwoLength - 1) // 4
                {
                    ballMovementY = 1;
                    ballMovementX *= -1;
                }
                else
                {
                    //Nothing
                }
            }

            // Player One Score
            if (ballX + ballMovementX >= gameWidth - 2)
            {
                ballMovementX *= -1;
                playerOneScore++;
                if (playerOneScore < gameEndScore)
                {
                    gameScore = true;
                    playerOnePoint = true;
                }
                else
                {
                    gameEnd = true;
                }
            }
            // Player Two Score
            if (ballX + ballMovementX <= 0)
            {
                ballMovementX *= -1;
                playerTwoScore++;
                if (playerTwoScore < gameEndScore)
                {
                    gameScore = true;
                    playerTwoPoint = true;
                }
                else
                {
                    gameEnd = true;
                }
            }

            // Roof and floor bounces
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

        public static void drawScore(char[,] gameMap)
        {
            gameMap[scorePositionY, scorePositionX] = Convert.ToChar(playerOneScore.ToString());
            gameMap[scorePositionY, scorePositionX + 35] = Convert.ToChar(playerTwoScore.ToString());
        }

        public static void cpuSpeaks(string cpuPhrase, VoiceGender voiceGender, int rate)
        {
            cpuTalk.Rate = rate;
            cpuTalk.SelectVoiceByHints(voiceGender);
            cpuTalk.Speak(cpuPhrase);
        }
        #endregion
    }
}
