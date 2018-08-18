using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FFBattleGenerator.BLL;

namespace FFBattleGenerator.DAL
{
    public class AnimationsWrapper
    {
        public Dictionary<string, AnimationDef> AnimationDefinitions { get; set; }
        public Dictionary<string, CharSet> CharSets { get; set; }
        public Flow Flow { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }
    }
}
