using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FFBattleGenerator.BLL
{
    public class CharSet
    {
        public string Name { get; set; }
        public Dictionary<char, CharDef> Characters { get; set; }

        public CharSet()
        {
            Characters = new Dictionary<char, CharDef>();
        }
    }
}
