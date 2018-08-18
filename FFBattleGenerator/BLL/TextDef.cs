using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FFBattleGenerator.BLL
{
    public class TextDef : IRenderableDef 
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public CharSet CharacterSet { get; set; }
    }
}
