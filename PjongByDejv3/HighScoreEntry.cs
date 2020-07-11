using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PjongByDejv3
{
    public class HighScoreEntry
    {
        public int Place { get; set; }
        public string Name { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime Date { get; set; }

        public override string ToString()
        {
            return $"# {Place} Name: {Name} Time: {Duration} Date: {Date}";
        }
    }
}
