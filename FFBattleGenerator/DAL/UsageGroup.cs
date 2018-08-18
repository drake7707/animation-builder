using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FFBattleGenerator.BLL;

namespace FFBattleGenerator.DAL
{
    class UsageGroup
    {
        public string Name { get; set; }
        public List<Renderable> Objects { get; set; }
    }
}
