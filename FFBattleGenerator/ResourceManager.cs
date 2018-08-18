using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace FFBattleGenerator
{
    public class ResourceManager
    {
        private static ResourceManager instance = null;
        public static ResourceManager Instance {
            get { 
                if(instance == null)
                    instance = new ResourceManager();
                return instance;
            }
        }

        private Dictionary<string, Image> images = new Dictionary<string, Image>();

        public Image Get(string img)
        {
            Bitmap i;
            if(!images.ContainsKey(img)) {
                i = new Bitmap(img);
                i.MakeTransparent(Color.Magenta);

                images.Add(img, i);
            }
            else
                i = (Bitmap)images[img];

            return i;
        }

    }
}
