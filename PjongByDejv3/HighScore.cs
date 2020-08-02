using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace PjongByDejv3
{
    public static class HighScore
    {
        public static string[] highScoreStartData = new string[] 
        {
            "1;David;737634.10:31:25.2027629;2020-07-29 10:31:25",
            "2;David;737634.10:31:25.2027629;2020-07-29 10:31:25",
            "3;David;737634.10:31:25.2027629;2020-07-29 10:31:25",
            "4;David;737634.10:31:25.2027629;2020-07-29 10:31:25",
            "5;David;737634.10:31:25.2027629;2020-07-29 10:31:25",
            "6;David;737634.10:31:25.2027629;2020-07-29 10:31:25",
            "7;David;737634.10:31:25.2027629;2020-07-29 10:31:25",
            "8;David;737634.10:31:25.2027629;2020-07-29 10:31:25",
            "9;David;737634.10:31:25.2027629;2020-07-29 10:31:25",
            "10;David;737634.10:31:25.2027629;2020-07-29 10:31:25"
        };
        public static string[] highScoresOnFile { get; set; }
        public static List<HighScoreEntry> HighScoresList { get; set; } = new List<HighScoreEntry>();
        public static string CurrentDirectory { get; set; } = Environment.CurrentDirectory;

        // Format of file ->
        //1;David;99-99-9999.8754332;99-99-9999
        //# 1 Name: David Time: 737634.10:31:25.2027629 Date: 2020-07-29 10:31:25
        public static void GetHighScores()
        {
            //Check if file exist -> if it doesn't create file and fil it with start data
            if(File.Exists(CurrentDirectory + "\\highscore.txt"))
            {
                if(new FileInfo("highscore.txt").Length == 0)
                {
                    File.AppendAllLines(CurrentDirectory + "\\highscore.txt", highScoreStartData);
                    highScoresOnFile = highScoreStartData;
                }
                else
                {
                    highScoresOnFile = File.ReadAllLines(CurrentDirectory + "\\highscore.txt");
                }
            }
            else
            {
                File.Create(CurrentDirectory + "\\highscore.txt");
                File.AppendAllLines(CurrentDirectory + "\\highscore.txt", highScoreStartData);
                highScoresOnFile = highScoreStartData;
            }

            //Convert highScoresOnFile to HighScores List
            for (int i = 0; i < highScoresOnFile.Length; i++)
            {
                HighScoreEntry newHighScoreEntry = new HighScoreEntry();

                var highScoreValues = highScoresOnFile[i].Split(';');

                newHighScoreEntry.Place = Convert.ToInt32(highScoreValues[0]); // Place
                newHighScoreEntry.Name = highScoreValues[1]; // Name
                newHighScoreEntry.Duration = TimeSpan.Parse(highScoreValues[2]); // Duration
                newHighScoreEntry.Date = Convert.ToDateTime(highScoreValues[3]); // Date

                HighScoresList.Add(newHighScoreEntry);
            }
        }

        // display with cool colors

        public static void AddHighScore(HighScoreEntry highScoreEntry)
        {
            // TODO - And how this should work
            HighScoresList.Add(highScoreEntry);
            // TODO - SORT!!! 
            // HighScoresList.Sort();

            foreach (var item in HighScoresList)
            {
                Console.WriteLine(item.ToString());
            }
            string highScoreToSave = ToFileFormat(highScoreEntry);
        }

        public static string ToFileFormat(HighScoreEntry highScoreEntry)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(highScoreEntry.Place + ";");
            sb.Append(highScoreEntry.Name + ";");
            sb.Append(highScoreEntry.Duration + ";");
            sb.Append(highScoreEntry.Date + ";");

            Console.WriteLine(sb.ToString());

            Console.WriteLine("");

            return "";
        }
        // Add a highscore to the right position in the list
        // 10 Highscore is the max
        // Remove if > 10



    }
}
