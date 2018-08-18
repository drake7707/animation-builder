using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FFBattleGenerator.BLL
{

    public class Animation : Renderable
    {

        public Animation(TimeBlock block, AnimationDef def)
            : base(block)
        {
            if (def.RandomSeqStart)
            {
                seqPointer = RandomNumber.GetRandomNr(0, def.Sequence.Length);
                string idx = def.Sequence[seqPointer];

                if (idx.Equals("L", StringComparison.InvariantCultureIgnoreCase))
                    seqPointer = 0; // loop
            }
            else
                seqPointer = 0;

            Definition = def;
        }



        ///<summary>The definition (or source info) of the animation</summary>
        ///<remarks></remarks>
        public AnimationDef Definition { get; set; }

        private int seqPointer;

        ///<summary>The current frame of the animation</summary>
        ///<remarks></remarks>
        public FrameDef CurrentFrame
        {
            get
            {
                if (Finished)
                    return null;
                else
                {

                    string idx = Definition.Sequence[seqPointer];
                    if (idx == "L")
                        return Definition.Frames[0];
                    else
                        return Definition.Frames[int.Parse(idx)];
                }
            }
        }

        private int currentMs = 0;

        public override bool Update(int addMs)
        {
            bool needToUpdate = false;

            //currentMs += addMs;

            if (true)//Started && !Finished) // wait till we reach start ms before starting animation
            {
                if (addMs > 0)
                {
                    while (!Finished && totalCurrentMs - currentMs - StartMs >= CurrentFrame.Delay)
                    {
                        currentMs += CurrentFrame.Delay;
                        NextFrame();
                        needToUpdate = true;
                    }
                }
                else
                {
                    while (Started && totalCurrentMs - currentMs - StartMs < 0)
                    {
                        seqPointer -= 1;
                        if (seqPointer < 0)
                            seqPointer = Definition.Sequence.Length-1;

                        string idx = Definition.Sequence[seqPointer];
                        if (idx.Equals("L", StringComparison.InvariantCultureIgnoreCase))
                            seqPointer -= 1; // loop

                        currentMs -= CurrentFrame.Delay;

                        needToUpdate = true;
                    }
                }
            }

            
            bool retVal = base.Update(addMs);
            if (!needToUpdate && retVal)
                needToUpdate = true;

            return needToUpdate;
        }


        ///<summary>Moves the animation to the next frame</summary>
        ///<remarks></remarks>
        private void NextFrame()
        {
            seqPointer = (seqPointer + 1);

            if (seqPointer < Definition.Sequence.Length)
            {
                string idx = Definition.Sequence[seqPointer];

                if (idx.Equals("L", StringComparison.InvariantCultureIgnoreCase))
                    seqPointer = 0; // loop

            }
        }

        ///<summary>Indicates whether the animation is finished</summary>
        ///<remarks></remarks>
        public override bool Finished
        {
            get
            {
                if (EndMs > 0)
                {
                    if (totalCurrentMs <= EndMs)
                    {
                        return (seqPointer >= Definition.Sequence.Length);
                    }
                    else
                        return true;
                }
                else
                {
                    return (seqPointer >= Definition.Sequence.Length);
                }
            }
        }

        ///<summary>Total time needed for the entire animation</summary>
        ///<remarks>Total time of parent timeblock when looping</remarks>
        public override int TotalTime
        {
            get
            {
                if (Definition.Sequence.Contains("L"))
                    if (EndMs > 0)
                        return EndMs - StartMs;
                    else
                        //return int.MaxValue;
                        // calculate time till end of timeblock
                        return ParentBlock.To - ParentBlock.From - StartMs;
                else
                {
                    int sum = 0;
                    foreach (string c in Definition.Sequence)
                    {
                        int idx = int.Parse(c);
                        sum += Definition.Frames[idx].Delay;
                    }
                    if (EndMs > 0)
                    {
                        if (EndMs - StartMs < sum)
                            return EndMs - StartMs;
                        else
                            return sum;
                    }
                    else
                    {
                        if (ParentBlock.To - ParentBlock.From - StartMs < sum)
                            return ParentBlock.To - ParentBlock.From - StartMs;
                        else
                            return sum;
                    }
                }
            }
        }

        public override Renderable Clone()
        {
            Animation a = new Animation(this.ParentBlock, this.Definition);
            a.alpha = this.alpha;
            a.origAlpha = this.origAlpha;
            a.origx = this.origx;
            a.origy = this.origy;
            a.alpha = this.alpha;
            a.x = this.x;
            a.y = this.y;
            a.ZOrder = this.ZOrder;
            a.StartMs = this.StartMs;
            a.EndMs = this.EndMs;
            // warning, maybe TODO, if changed afterwards references will all be changed
            a.Attributes = this.Attributes;
            a.currentMs = this.currentMs;
            a.totalCurrentMs = this.totalCurrentMs;

            return a;
        }
    }
}
