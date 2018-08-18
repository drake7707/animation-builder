using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FFBattleGenerator.BLL
{
    public class FrameDef
    {
        /// <summary>
        /// Source position of frame
        /// </summary>
        public int X { get; set; }
        public int Y { get; set; }

        /// <summary>
        /// Source image
        /// </summary>
        public string Image { get; set; }

        /// <summary>
        /// Source width and height of frame
        /// </summary>
        public int Width { get; set; }
        public int Height { get; set; }

        /// <summary>
        /// How long the frame should be shown before the next frame is shown
        /// </summary>
        public int Delay { get; set; }

        /// <summary>
        /// The base coordinates where the sprite touches the ground. This is used to seamlessly switch from
        /// one animation to another using the same x & y coordinates without taking in account the difference in size
        /// of the sprite (as these properties will correct that).
        /// </summary>
        public int BaseX { get; set; }
        public int BaseY { get; set; }
    }
}
