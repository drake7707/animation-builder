using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FFBattleGenerator.BLL
{
    public class TimeBlock
    {
        public int From { get; set; }
        public int To { get; set; }
        public string Description { get; set; }

        //public List<Animation> Animations { get; set; }
        //public List<Text> Texts { get; set; }

        public List<Renderable> Objects { get; set; }

        public TimeBlock()
        {
            Objects = new List<Renderable>();

            //Animations = new List<Animation>();
            //Texts = new List<Text>();
        }

        public List<Animation> Animations
        {
            get
            {
                return (from o in Objects
                        where o is Animation
                        select (Animation)o).ToList();
            }
        }

        public List<Text> Texts
        {
            get
            {
                return (from o in Objects
                        where o is Text
                        select (Text)o).ToList();
            }
        }

    }
}
