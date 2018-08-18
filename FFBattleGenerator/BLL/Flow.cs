using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FFBattleGenerator.BLL
{
    public class Flow
    {
        public List<TimeBlock> TimeBlocks { get; set; }

        public Flow()
        {
            TimeBlocks = new List<TimeBlock>();
        }

        public int TotalDuration
        {
            get
            {
                int max = 0;
                foreach (TimeBlock tb in TimeBlocks)
                {
                    if (tb.To > max)
                        max = tb.To;
                }
                return max;
            }
        }
    }
}
