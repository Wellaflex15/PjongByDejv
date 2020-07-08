using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech;
using System.Speech.Synthesis;
using System.Runtime.InteropServices;
using System.Threading;

namespace PjongByDejv2
{
    
    public class Program
    {
        //For CPU talking
        private static SpeechSynthesizer cpuTalk = new SpeechSynthesizer();

        private static List<cpuPhrases> phrases = new List<cpuPhrases>() { 
            new cpuPhrases { name = cpuMessages.LostPoint, phrase = "Shit! You're lucky! Buy a triss please!" },
            new cpuPhrases { name = cpuMessages.WonPoint, phrase = "Haha, I'm the best! I win every point!" },
            new cpuPhrases { name = cpuMessages.WonGame, phrase = "You will never beat me! I'm superior to you insect!" },
            new cpuPhrases { name = cpuMessages.LostGame, phrase = "You must have cheated. I will now destroy your computer. Initialiing removment of harddrive. Beep beep boop boop!" },
            new cpuPhrases { name = cpuMessages.Random1, phrase = "I will win. I am machine and you are just a sad human" },
            new cpuPhrases { name = cpuMessages.Random2, phrase = "Do you think this is fun. Losing to a computer all the time?" }
            };

        //Make console locked at a fixed size -> https://docs.microsoft.com/de-de/windows/desktop/menurc/wm-syscommand
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

        // Game vaiables
        public static int height = 20;
        public static int width = 50;

        // Ball variables
        public static int ballPosition = 525;
        public static int ballMovement;
        public static int ballMovementRightUp = -49;
        public static int ballMovementLeftUp = -51;
        public static int ballMovementRightUpDouble = -99;
        public static int ballMovementLeftUpDouble = -101;
        public static int ballMovementRightDown = 51;
        public static int ballMovementLeftDown = 49;
        public static int ballMovementRightDownDouble = 101;
        public static int ballMovementLeftDownDouble = 99;
        public static int playerPositionX = 2;
        public static int playerPositionY = 9;
        public static int playerLength = 3;
        public static int cpuPositionX = width - 4;
        public static int cpuPositionY = 9;
        public static int cpuLength = 3;

        public static int scorePositionNameX = 5;
        public static int scorePositionNameY = 3;
        public static int scorePositionX = 5;
        public static int scorePositionY = 2;
        public static string playerOneName;
        public static int playerOneScore = 0;
        public static int cpuScore = 0;
        public static int scoreToWin = 5;

        public static bool gameRunning = true;
        public static bool gameStart = true; // TODO(david) false for testing
        public static bool gameScore = false;
        public static bool gameEnd = false;

        public static int gameSpeed = 200; // Checking the thread sleep to change the game speed

        public static DateTime start;
        public static DateTime end;
        static void Main(string[] args)
        {


            //Lock the console -> so it can't break
            DeleteMenu(GetSystemMenu(GetConsoleWindow(), false), SC_MINIMIZE, MF_BYCOMMAND);
            DeleteMenu(GetSystemMenu(GetConsoleWindow(), false), SC_MAXIMIZE, MF_BYCOMMAND);
            DeleteMenu(GetSystemMenu(GetConsoleWindow(), false), SC_SIZE, MF_BYCOMMAND);

            Console.WindowHeight = 24;
            Console.WindowWidth = 50;
            Console.BufferHeight = 24;
            Console.BufferWidth = 50;

            //Create game buffer to draw the game to
            char[] gameDrawing = new char[height * width];

            //Player always start
            ballMovement = ballMovementRightDown;
            while (gameRunning)
            {
                if (gameStart)
                {
                    Console.WriteLine("PjongByDejv");
                    Console.WriteLine("Enter your name:");
                    playerOneName = Console.ReadLine();

                    start = DateTime.Now;
                    gameStart = false;
                }
                else if(gameScore)
                {

                }
                else if(gameEnd)
                {

                }

                //Console.WriteLine($"{playerOneName} : {playerOneScore} - CPU : {cpuScore}");
                Console.WriteLine(ballPosition);
                // Print out the duration to see the time played.
                TimeSpan duration = DateTime.Now.Subtract(start);
                Console.WriteLine(duration);

                ClearGameDrawing(gameDrawing);
                DrawBall(gameDrawing);
                DrawPlayer(gameDrawing);
                MovePlayer(gameDrawing);

                //Draw the game to the screen with all the game stuff
                Console.WriteLine(gameDrawing);
                Thread.Sleep(gameSpeed);

            }

            //foreach (cpuPhrases phrase in phrases)
            //{
            //    Console.WriteLine(phrase.phrase);
            //    cpuSpeaks(phrase, VoiceGender.Neutral, 5);
            //}

            end = DateTime.Now;

            

            

            Console.ReadLine();

        }

        private static void ClearGameDrawing(char[] gameDrawing)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if(x == (width - 1)) // Put in new line to create a board of the array
                    {
                        gameDrawing[(y * width) + x] = '\n';
                    }
                    else if(x == 0 || x == (width - 2)) // Left and right boarder
                    {
                        gameDrawing[(y * width) + x] = '|';
                    }
                    else if(y == 0 || y == (height - 1)) // Top and bottom boarder
                    {
                        gameDrawing[(y * width) + x] = '-';
                    }
                    else // Empty space for the board
                    {
                        gameDrawing[(y * width) + x] = ' ';
                    }
                }
            }
        }

        private static void DrawBall(char[] gameDrawing)
        {
            gameDrawing[ballPosition] = 'O';

            if(((ballPosition - 48)%50) == 0) // Player has scored
            {
                ballMovement *= -1;
                playerOneScore++;
            }
            if((ballPosition % 50) == 0) // Cpu has scored
            {
                ballMovement *= -1;
                cpuScore++;
            }

            if ((ballPosition > ((height * width) - (width * 2)) && (ballPosition < ((height * width) - width)))) // To change direction on bottom line everyone on greater than 900
            {
                if(ballMovement == ballMovementRightDown)
                {
                    ballMovement = ballMovementRightUp;
                }
                if(ballMovement == ballMovementLeftDown)
                {
                    ballMovement = ballMovementLeftUp;
                }
            }
            else if (ballPosition < (width * 2) && ballPosition > 0) // To change direction on top line if less than 100
            {
                if (ballMovement == ballMovementRightUp)
                {
                    ballMovement = ballMovementRightDown;
                }
                if (ballMovement == ballMovementLeftUp)
                {
                    ballMovement = ballMovementLeftDown;
                }
            }

            ballPosition += ballMovement;
        }

        private static void DrawPlayer(char[] gameDrawing)
        {
            for (int i = 0; i < playerLength; i++)
            {
                gameDrawing[(playerPositionX + (playerPositionY * (width)) + (width * i))] = 'X';
            }
            for (int i = 0; i < cpuLength; i++)
            {
                gameDrawing[(cpuPositionX + (cpuPositionY * (width)) + (width * i))] = 'X';
            }
        }

        private static void MovePlayer(char[] gameDrawing)
        {
            if (Console.KeyAvailable)
            {
                var keyPressed = Console.ReadKey(true);

                switch (keyPressed.Key)
                {
                    case ConsoleKey.UpArrow:
                        if (playerPositionY > 1)
                            playerPositionY--;
                        break;
                    case ConsoleKey.DownArrow:
                        if (playerPositionY < height - playerLength - 1)
                            playerPositionY++;
                        break;
                    default:
                        // Do nothing
                        break;
                }
            }
        }


        public static void cpuSpeaks(cpuPhrases cpuPhrase, VoiceGender voiceGender, int rate)
        {
            cpuTalk.Rate = rate;
            cpuTalk.SelectVoiceByHints(voiceGender);
            cpuTalk.Speak(cpuPhrase.phrase);
        }
    }

    public class cpuPhrases
    {
        public cpuMessages name { get; set; }
        public string phrase { get; set; }
    }


    public enum cpuMessages
    {
        LostPoint,
        WonPoint,
        WonGame,
        LostGame,
        Random1,
        Random2,
        Random3
    }
}
