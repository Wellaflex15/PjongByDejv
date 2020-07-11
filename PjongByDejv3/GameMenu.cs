using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PjongByDejv3
{
    public class GameMenu
    {
        public string Name { get; set; }
        public List<MenuChoice> ListOfChoices { get; set; } = new List<MenuChoice>();
        public List<string> ListOfInfo { get; set; } = new List<string>();

        public GameMenu()
        {
            // Empty contructor
        }

        public GameMenu(string name, List<MenuChoice> listOfChoices)
        {
            Name = name;
            ListOfChoices = listOfChoices;
        }

        public void ShowMenu()
        {
            bool showMenu = true;

            while (showMenu)
            {
                Console.WriteLine(Name);
                Console.WriteLine();
                
                foreach(var choice in ListOfChoices)
                {
                    if (choice.Selected)
                    {
                        Console.BackgroundColor = ConsoleColor.Green;
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                    }
                    Console.WriteLine(choice.Name);
                    Console.ResetColor();
                }

                
                var key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.DownArrow)
                {
                    foreach (var choice in ListOfChoices)
                    {
                        if (choice.Selected == true)
                        {
                            choice.Selected = false;
                        }
                        else
                        {
                            choice.Selected = true;
                        }
                    }
                } else if(key.Key == ConsoleKey.Enter)
                {
                    foreach (var choice in ListOfChoices)
                    {
                        if (choice.Name == "Start" && choice.Selected == true)
                        {
                            choice.Selected = false;
                            showMenu = false;
                        }
                        if (choice.Name == "HighScore" && choice.Selected == true)
                        {
                            choice.Selected = false;
                            showMenu = false;
                        }
                        
                    }
                }
                
                Console.Clear();
            }
        }
    }

    public class MenuChoice
    {
        public string Name { get; set; }
        public bool Selected { get; set; }

        public MenuChoice(string name, bool selected)
        {
            Name = name;
            Selected = selected;
        }
    }
}
