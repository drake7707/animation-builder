using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FFBattleGenerator.BLL
{
    public class Text : Renderable
    {

        public Text(TimeBlock parent, TextDef definition)
            : base(parent)
        {
            this.Definition = definition;
            this.TextString = definition.Value;

        }
        /// <summary>
        /// Text definition
        /// </summary>
        public TextDef Definition { get; private set; }

        public override bool Finished
        {
            get
            {
                if (EndMs > 0)
                    return (totalCurrentMs > EndMs);
                else
                    return false;
            }
        }

        public override int TotalTime
        {
            get
            {
                // calculate time till end of timeblock
                if (EndMs > 0)
                {
                    return EndMs - StartMs;
                }
                else
                {
                    return ParentBlock.To + StartMs  - ParentBlock.From;
                }
                
                
            }
        }

        public string TextString { get; set; }

        public override bool Update(int addMs)
        {
            return base.Update(addMs);
        }


        private int lastTextUpdate;
        private int charactersWritten;

        protected override void StartAttribute(AnimAttribute attr)
        {
            if (attr.Name == "typeText")
            {
                charactersWritten = 0;
                lastTextUpdate = 0;
            }
            else if (attr.Name == "eraseText")
            {
                charactersWritten = Definition.Value.Length;
                lastTextUpdate = 0;
            }
            else if (attr.Name == "init")
            {
                if(attr.Properties.ContainsKey("text"))
                    TextString = attr.Properties["text"];
                base.StartAttribute(attr);
            }
            else
                base.StartAttribute(attr);
        }

        protected override bool UpdateAttribute(AnimAttribute attr)
        {
            bool needToUpdate = false;
            if (attr.Name == "typeText")
            {
                int delay = int.Parse(attr.Properties["delay"]);
                bool reverse = bool.Parse(attr.Properties["reverse"]);

                if (totalCurrentMs >= lastTextUpdate + delay)
                {
                    if (charactersWritten <= Definition.Value.Length)
                    {
                        if (!reverse)
                            TextString = Definition.Value.Substring(0, charactersWritten);
                        else
                        {
                            StringBuilder str = new StringBuilder();
                            for (int i = 0; i < Definition.Value.Length - (charactersWritten); i++)
                                str.Append(@"^" + Definition.Value[i]);
                            str.Append(Definition.Value.Substring(Definition.Value.Length - (charactersWritten)));
                            TextString = str.ToString();
                        }

                        charactersWritten++;
                        needToUpdate = true;
                    }
                    else
                        attr.Finished = true;

                    lastTextUpdate = totalCurrentMs;
                }
            }
            else if (attr.Name == "eraseText")
            {
                int delay = int.Parse(attr.Properties["delay"]);
                bool reverse = bool.Parse(attr.Properties["reverse"]);

                if (totalCurrentMs >= lastTextUpdate + delay)
                {
                    if (charactersWritten >= 0)
                    {
                        if (!reverse)
                            TextString = Definition.Value.Substring(0, charactersWritten);
                        else
                        {
                            StringBuilder str = new StringBuilder();
                            for (int i = 0; i < Definition.Value.Length - (charactersWritten); i++)
                                str.Append(@"^" + Definition.Value[i]);
                            str.Append(Definition.Value.Substring(Definition.Value.Length - (charactersWritten)));

                            TextString = str.ToString();
                            // fill first characters
                            //TextString = //GetCharString(' ', Definition.Value.Length - (charactersWritten - 1)) + Definition.Value.Substring(Definition.Value.Length - (charactersWritten - 1));
                        }
                        charactersWritten--;
                        needToUpdate = true;
                    }
                    else
                        attr.Finished = true;

                    lastTextUpdate = totalCurrentMs;
                }
            }
            else
                needToUpdate = base.UpdateAttribute(attr);

            return needToUpdate;
        }

        //private String GetCharString(char c, int length)
        //{
        //    StringBuilder str = new StringBuilder();
        //    for (int i = 0; i < length; i++)
        //        str.Append(c);
        //    return str.ToString();
        //}

        public override Renderable Clone()
        {
            Text t = new Text(this.ParentBlock, this.Definition);

            t.alpha = this.alpha;
            t.origAlpha = this.origAlpha;
            t.origx = this.origx;
            t.origy = this.origy;
            t.alpha = this.alpha;
            t.x = this.x;
            t.y = this.y;
            t.ZOrder = this.ZOrder;
            t.StartMs = this.StartMs;
            t.EndMs = this.EndMs;
            // warning, maybe TODO, if changed afterwards references will all be changed
            t.Attributes = this.Attributes;
            t.totalCurrentMs = this.totalCurrentMs;

            return t;
        }

    }
}
