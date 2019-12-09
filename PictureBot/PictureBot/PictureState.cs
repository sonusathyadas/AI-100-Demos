using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PictureBot
{
    public class PictureState
    {
        public string Greeted { get; set; } = "not greeted";
        public List<string> UtteranceList { get; private set; } = new List<string>();
        public string Search { get; set; } = "";
        public string Searching { get; set; } = "no";
    }
}
