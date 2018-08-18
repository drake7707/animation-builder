using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FFBattleGenerator.BLL
{
    public abstract class Renderable
    {

        public Renderable(TimeBlock parent)
        {
            this.ParentBlock = parent;
        }

        protected int x;
        protected int y;
        protected int origx;
        protected int origy;
        
        ///<summary>The current X position of the animation</summary>
        ///<remarks>Setting the X value will result in changing the original starting X position. (and not the current one if modified by example moveTo)</remarks>
        public int X
        {
            get
            {
                return x;
            }
            set
            {
                x = value;
                origx = value;
            }
        }

        ///<summary>The current Y position of the animation</summary>
        ///<remarks>Setting the Y value will result in changing the original starting Y position. (and not the current one if modified by example moveTo)</remarks>
        public int Y
        {
            get
            {
                return y;
            }
            set
            {
                y = value;
                origy = value;
            }
        }

        protected float alpha;
        protected float origAlpha;
        public float Alpha
        {
            get
            {
                return alpha;
            }
            set
            {
                alpha = value;
                origAlpha = value;
            }
        }

        /// <summary>
        /// Color category
        /// </summary>
        public System.Drawing.Color Category { get; set; }

        ///<summary>The special attributes of the animation</summary>
        ///<remarks>e.g 
        ///moveTo (with x,y, duration)
        ///fade (with start, end, duration)
        /// </remarks>
        public List<AnimAttribute> Attributes { get; set; }

        ///<summary>The amount of milliseconds that need to be passed since the animation was initialized to start the animation</summary>
        ///<remarks></remarks>
        public int StartMs { get; set; }
        public int EndMs { get; set; }

        protected int totalCurrentMs = 0;

        public int ZOrder { get; set; }

        public TimeBlock ParentBlock { get; set; }


        ///<summary>Updates the animation if needed</summary>
        ///<remarks></remarks>
        ///<param name="addMs">The amount of milliseconds that the animation needs to be advanced</param>
        public virtual bool Update(int addMs)
        {
            totalCurrentMs += addMs;

            bool needToUpdate = false;

            if (Started && !Finished) // wait till we reach start ms before starting animation
                needToUpdate = UpdateAttributes();

            return needToUpdate;
        }

        
        /// <summary>
        /// Initialize the specified attribute
        /// </summary>
        /// <param name="attr"></param>
        protected virtual void StartAttribute(AnimAttribute attr)
        {
            if (attr.Name == "init")
            {
                if (attr.Properties.ContainsKey("alpha"))
                    Alpha = float.Parse(attr.Properties["alpha"], System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        //private Dictionary<AnimAttribute, bool> startedAttributes = new Dictionary<AnimAttribute, bool>();

        private bool UpdateAttributes()
        {
            bool needToUpdate = false;

            foreach (AnimAttribute attr in Attributes)
            {
                // start attributes if they aren't started and need to start
                if (totalCurrentMs >= StartMs + attr.StartMs)
                {
                    if (!attr.Started)
                    {
                        StartAttribute(attr);
                        attr.Started = true;
                    }

                    bool retVal = UpdateAttribute(attr);
                    if (!needToUpdate && retVal)
                        needToUpdate = true;
                }
            }
            return needToUpdate;
        }

        protected virtual bool UpdateAttribute(AnimAttribute attr)
        {
            bool needToUpdate = false;

            if (attr.Name == "moveTo")
            {
                int moveX = int.Parse(attr.Properties["x"]);
                int moveY = int.Parse(attr.Properties["y"]);

                // take duration, if not specified take total time
                int duration;
                if (attr.Properties.ContainsKey("duration"))
                    duration = int.Parse(attr.Properties["duration"]);
                else
                    duration = TotalTime;

                float perc = (float)((float)(totalCurrentMs - StartMs) / (float)duration);

                if (perc >= 0 && perc <= 1)
                {
                    int oldx = x;
                    int oldy = y;

                    x = origx + (int)((moveX - origx) * perc);
                    y = origy + (int)((moveY - origy) * perc);

                    if (oldx != x || oldy != y) // only update if necessary
                        needToUpdate = true;
                }
                else
                    attr.Finished = true;
            }
            else if (attr.Name == "fade")
            {
                float start = float.Parse(attr.Properties["start"], System.Globalization.CultureInfo.InvariantCulture);
                float end = float.Parse(attr.Properties["end"], System.Globalization.CultureInfo.InvariantCulture);

                int duration;
                if (attr.Properties.ContainsKey("duration"))
                    duration = int.Parse(attr.Properties["duration"]);
                else
                    // totaltime is animation length!
                    duration = TotalTime;

                float perc = (float)((float)(totalCurrentMs -StartMs - attr.StartMs) / (float)duration);
                if (perc >= 0 && perc < 1)
                {
                    alpha = start + ((end - start) * perc);
                    needToUpdate = true;
                }
                else
                    attr.Finished = true;
            }
            return needToUpdate;
        }

        public abstract int TotalTime { get; }

        ///<summary>Indicates whether the animation has been started yet</summary>
        ///<remarks></remarks>
        public bool Started
        {
            get
            {
                return totalCurrentMs >= StartMs;
            }
        }

        public abstract bool Finished { get; }

        public abstract Renderable Clone();

        public object GetDebugInfo(string name)
        {
            return this.GetType().GetField(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(this);
        }
    }
}
