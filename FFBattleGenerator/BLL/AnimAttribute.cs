using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FFBattleGenerator.BLL
{
    public class AnimAttribute
    {
        public string Name { get; set; }
        public Dictionary<string,string> Properties { get; set; }

        public AnimAttribute()
        {

            Properties = new Dictionary<string, string>();
        }

        public int StartMs { get; set; }

        public bool Started { get; set; }
        public bool Finished { get; set; }
    }
}
