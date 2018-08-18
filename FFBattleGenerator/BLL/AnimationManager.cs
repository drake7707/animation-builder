using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FFBattleGenerator.DAL;

namespace FFBattleGenerator.BLL
{
    public class AnimationManager
    {
        //public List<AnimationDef> AnimationDefs { get; set; }

        private Flow flow;

        public int CurrentMs { get; private set;  }

        public AnimationManager(string xmlFile)
        {
            AnimationsWrapper animWrapper = AnimationMapper.ReadAnimations(xmlFile);
            flow = animWrapper.Flow;
            Width = animWrapper.Width;
            Height = animWrapper.Height;
            CurrentMs = 0;

        }

        public TimeBlock CurrentTimeBlock
        {
            get
            {
                List<TimeBlock> blocks = (from tb in flow.TimeBlocks
                        where CurrentMs >= tb.From && CurrentMs <= tb.To
                        select tb).ToList();
                if (blocks.Count == 0)
                    return new TimeBlock();
                else
                    return blocks[0];
            }
        }

        // forward the animation by ms
        public bool NextFrame(int ms)
        {
            bool needToUpdate = false;
            CurrentMs += ms;
            foreach (Renderable r in CurrentTimeBlock.Objects)
            {
                bool changed = r.Update(ms);
                if (changed)
                    needToUpdate = true;
            }

            TimeChangedHanlder temp = TimeChanged;
            if (temp != null)
                temp(CurrentMs);

            return needToUpdate;
        }

        ///<summary>The flow of the animation</summary>
        ///<remarks></remarks>
        public Flow Flow
        {
            get
            {
                return flow;
            }
            private set
            {
                flow = value;
            }
        }
        public int Width { get; set; }
        public int Height { get; set; }

        public delegate void TimeChangedHanlder(int ms);
        public event TimeChangedHanlder TimeChanged;

    }
}
