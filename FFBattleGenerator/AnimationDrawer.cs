using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FFBattleGenerator.BLL;
using System.Drawing;
using System.Drawing.Imaging;

namespace FFBattleGenerator
{
    public class AnimationDrawer
    {

        public static void DrawFrameTo(AnimationManager anim, Graphics g, bool drawDebugInfo)
        {
            List<Renderable> renderObjects = new List<Renderable>(anim.CurrentTimeBlock.Objects);
            renderObjects.Sort((o1, o2) => o1.ZOrder.CompareTo(o2.ZOrder));

            foreach (Renderable o in renderObjects)
            {
                if (o.Started && !o.Finished)
                {
                    if (o is Animation)
                    {
                        Animation a = (Animation)o;

                        Rectangle animDest = new Rectangle(a.X - a.CurrentFrame.BaseX, a.Y - a.CurrentFrame.BaseY, a.CurrentFrame.Width, a.CurrentFrame.Height);

                        Image img = ResourceManager.Instance.Get(a.CurrentFrame.Image);
                        if (a.Alpha < 1)
                        { // apply alpha
                            ImageAttributes ia = new ImageAttributes();
                            ColorMatrix cm = new ColorMatrix();
                            cm.Matrix00 = cm.Matrix11 = cm.Matrix22 = cm.Matrix44 = 1;
                            cm.Matrix33 = a.Alpha;
                            ia.SetColorMatrix(cm);
                            g.DrawImage(img, animDest, a.CurrentFrame.X, a.CurrentFrame.Y, a.CurrentFrame.Width, a.CurrentFrame.Height, GraphicsUnit.Pixel, ia);//, imgAttr);
                        }
                        else
                        {
                            g.DrawImage(img, animDest, a.CurrentFrame.X, a.CurrentFrame.Y, a.CurrentFrame.Width, a.CurrentFrame.Height, GraphicsUnit.Pixel);//, imgAttr);
                        }
                        /*g.DrawImage(img, new Rectangle(a.X, a.Y, a.CurrentFrame.Width, a.CurrentFrame.Height), 
                                         new Rectangle(a.CurrentFrame.X, a.CurrentFrame.Y, a.CurrentFrame.Width, a.CurrentFrame.Height), 
                                         GraphicsUnit.Pixel); */

                        if (drawDebugInfo)
                        {
                            g.DrawRectangle(Pens.Red, new Rectangle(a.X - 1, a.Y - 1, 2, 2));


                            int origx = (int)a.GetDebugInfo("origx");
                            int origy = (int)a.GetDebugInfo("origy");

                            foreach (AnimAttribute attr in a.Attributes)
                            {
                                if (attr.Started && attr.Name == "moveTo")
                                {
                                    int moveX = int.Parse(attr.Properties["x"]);
                                    int moveY = int.Parse(attr.Properties["y"]);

                                    g.DrawLine(new Pen(Color.FromArgb(128, Color.Red)), new Point(origx, origy), new Point(moveX, moveY));
                                }
                            }
                        }
                    }
                    else if (o is Text)
                    {
                        bool escapeNextCharacter = false;

                        Text txt = (Text)o;
                        int charOffset = 0;
                        foreach (char c in txt.TextString)
                        {
                            if (c == '^')
                                escapeNextCharacter = true;
                            else
                            {
                                if (txt.Definition.CharacterSet.Characters.ContainsKey(c))
                                {
                                    CharDef cc = txt.Definition.CharacterSet.Characters[c];
                                    Image img = ResourceManager.Instance.Get(cc.Image);

                                    Rectangle src;
                                    if (escapeNextCharacter)
                                    {
                                        CharDef space = txt.Definition.CharacterSet.Characters[' '];
                                        src = new Rectangle(space.X, space.Y, cc.Width, cc.Height);
                                    }
                                    else
                                        src = new Rectangle(cc.X, cc.Y, cc.Width, cc.Height);

                                    if (o.Alpha < 1)
                                    { // apply alpha

                                        ImageAttributes ia = new ImageAttributes();
                                        ColorMatrix cm = new ColorMatrix();
                                        cm.Matrix00 = cm.Matrix11 = cm.Matrix22 = cm.Matrix44 = 1;
                                        cm.Matrix33 = o.Alpha;
                                        ia.SetColorMatrix(cm);


                                        g.DrawImage(img, new Rectangle(txt.X + charOffset, txt.Y, cc.Width, cc.Height),
                                                         src.X, src.Y, src.Width, src.Height, GraphicsUnit.Pixel, ia);//, imgAttr);
                                    }
                                    else
                                    {
                                        g.DrawImage(img, new Rectangle(txt.X + charOffset, txt.Y, cc.Width, cc.Height),
                                                         src.X, src.Y, src.Width, src.Height, GraphicsUnit.Pixel);//, imgAttr);
                                    }

                                    charOffset += (cc.Width + 1);
                                    escapeNextCharacter = false;
                                }
                            }
                        }
                    }
                }
            }
        }

        public static Image GetImageFromFrame(AnimationManager anim, bool drawDebugInfo)
        {
            Bitmap b = new Bitmap(anim.Width, anim.Height);
            using (Graphics g = Graphics.FromImage(b))
            {
                DrawFrameTo(anim, g, drawDebugInfo);
            }

            return b;
        }
    }
}
