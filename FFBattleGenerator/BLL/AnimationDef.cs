using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FFBattleGenerator.BLL
{
    public class AnimationDef : IRenderableDef
    {
        ///<summary>Sequence of the frames to be displayed</summary>
        ///<remarks></remarks>
        public string[] Sequence { get; set; }
        ///<summary>Name of the animation definition</summary>
        ///<remarks></remarks>
        public string Name { get; set; }
        ///<summary>The frames of the animation</summary>
        ///<remarks></remarks>
        public List<FrameDef> Frames { get; set; }
        ///<summary>When the animation starts, does it has to start on a random frame</summary>
        ///<remarks>This should probably only be used when the animation is looping</remarks>
        public bool RandomSeqStart { get; set; }

        public AnimationDef()
        {
            Frames = new List<FrameDef>();

        }
    }
}
