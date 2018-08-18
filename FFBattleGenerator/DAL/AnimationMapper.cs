using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using FFBattleGenerator.BLL;

namespace FFBattleGenerator.DAL
{
    public class AnimationMapper
    {

        public static AnimationsWrapper ReadAnimations(string xmlFile)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(xmlFile);

            Dictionary<string, AnimationDef> animationDefinitions = ReadAnimation(doc);
            Dictionary<string, CharSet> charsets = ReadCharSets(doc);
            Dictionary<string, TextDef> textDefinitions = ReadTexts(doc, charsets);
            Dictionary<string, UsageGroup> groupDefinitions = ReadGroups(doc, animationDefinitions, charsets, textDefinitions);

            Flow flow = ReadFlow(doc, animationDefinitions, charsets, textDefinitions, groupDefinitions);

            int width = int.Parse(doc.SelectSingleNode("/animations").Attributes["width"].Value);
            int height = int.Parse(doc.SelectSingleNode("/animations").Attributes["height"].Value);

            AnimationsWrapper aw = new AnimationsWrapper()
            {
                AnimationDefinitions = animationDefinitions,
                CharSets = charsets,
                Flow = flow,
                Width = width,
                Height = height
            };
            return aw;
        }

        private static Dictionary<string, CharSet> ReadCharSets(XmlDocument doc)
        {
            XmlNodeList charsets = doc.SelectNodes("//charset");

            Dictionary<string, CharSet> sets = new Dictionary<string, CharSet>();

            foreach (XmlNode cs in charsets)
            {
                CharSet charset = new CharSet()
                {
                    Name = cs.Attributes["name"].Value
                };
                XmlNodeList chars = cs.SelectNodes("char");
                foreach (XmlNode cd in chars)
                {
                    CharDef chardef = new CharDef()
                    {
                        Name = cd.Attributes["name"].Value,
                        X = int.Parse(cd.Attributes["x"].Value),
                        Y = int.Parse(cd.Attributes["y"].Value),
                        Width = int.Parse(cd.Attributes["width"].Value),
                        Height = int.Parse(cd.Attributes["height"].Value),
                        Image = cd.Attributes["img"].Value
                    };
                    charset.Characters.Add(chardef.Name[0], chardef);
                }

                sets.Add(charset.Name, charset);
            }
            return sets;
        }

        private static Dictionary<string, TextDef> ReadTexts(XmlDocument doc, Dictionary<string, CharSet> charsets)
        {
            XmlNodeList txt = doc.SelectNodes("//text");

            Dictionary<string, TextDef> txts = new Dictionary<string, TextDef>();

            foreach (XmlNode t in txt)
            {
                TextDef text = new TextDef()
                {
                    Name = t.Attributes["name"].Value,
                    Value = t.Attributes["value"].Value
                };
                //CharSet def = (from cs in charsets
                //               where cs.Name == t.Attributes["charset"].Value
                //               select cs).ToList()[0];
                //text.CharacterSet = def;
                text.CharacterSet = charsets[t.Attributes["charset"].Value];
                txts.Add(text.Name, text);
            }
            return txts;
        }

        private static Dictionary<string, AnimationDef> ReadAnimation(XmlDocument doc)
        {
            XmlNodeList animations = doc.SelectNodes("//animation");

            Dictionary<string, AnimationDef> animationDefinitions = new Dictionary<string, AnimationDef>();
            foreach (XmlNode n in animations)
            {
                AnimationDef a = new AnimationDef()
                {
                    Name = n.Attributes["name"].Value,
                    Sequence = n.Attributes["sequence"].Value.Split(','),
                };
                if (n.Attributes["randomSeqStart"] == null)
                    a.RandomSeqStart = false;
                else
                    a.RandomSeqStart = n.Attributes["randomSeqStart"].Value.Equals("true", StringComparison.InvariantCultureIgnoreCase);

                XmlNodeList frames = n.SelectNodes("frame");
                foreach (XmlNode f in frames)
                {
                    FrameDef fd = new FrameDef()
                    {
                        X = int.Parse(f.Attributes["x"].Value),
                        Y = int.Parse(f.Attributes["y"].Value),
                        Image = f.Attributes["img"].Value,
                        Width = int.Parse(f.Attributes["width"].Value),
                        Height = int.Parse(f.Attributes["height"].Value),
                        Delay = int.Parse(f.Attributes["delay"].Value),
                    };
                    if (f.Attributes["basex"] == null)
                        fd.BaseX = 0;
                    else
                        fd.BaseX = int.Parse(f.Attributes["basex"].Value);

                    if (f.Attributes["basey"] == null)
                        fd.BaseY = 0;
                    else
                        fd.BaseY = int.Parse(f.Attributes["basey"].Value);

                    a.Frames.Add(fd);
                }
                animationDefinitions.Add(a.Name, a);
            }
            return animationDefinitions;
        }

        private static Dictionary<string, UsageGroup> ReadGroups(XmlDocument doc, Dictionary<string, AnimationDef> animDefs, Dictionary<string, CharSet> charDefs, Dictionary<string, TextDef> textDefs)
        {
            XmlNodeList groups = doc.SelectNodes("//group");

            Dictionary<string, UsageGroup> groupDefinitions = new Dictionary<string, UsageGroup>();
            foreach (XmlNode n in groups)
            {
                UsageGroup g = new UsageGroup()
                {
                    Name = n.Attributes["name"].Value,
                    // don't assign a parent to objects yet, it will be assigned at useGroup
                    Objects = ReadUsage(animDefs, charDefs, textDefs, groupDefinitions, n, null)
                };

                groupDefinitions.Add(g.Name, g);
            }
            return groupDefinitions;
        }

        private static Flow ReadFlow(XmlDocument doc, Dictionary<string, AnimationDef> animDefs, Dictionary<string, CharSet> charDefs, Dictionary<string, TextDef> textDefs, Dictionary<string, UsageGroup> groupDefs)
        {
            XmlNode flow = doc.SelectNodes("//flow")[0];
            Flow f = new Flow();

            XmlNodeList times = flow.SelectNodes("time");
            foreach (XmlNode time in times)
            {
                string description;
                if (time.Attributes["description"] == null)
                    description = "";
                else
                    description = time.Attributes["description"].Value;

                TimeBlock tb = new TimeBlock()
                {
                    From = int.Parse(time.Attributes["from"].Value),
                    To = int.Parse(time.Attributes["to"].Value),
                    Description = description
                };

                tb.Objects.AddRange(ReadUsage(animDefs, charDefs, textDefs, groupDefs, time, tb));

                //XmlNodeList useAnim = time.SelectNodes("useAnim");
                //foreach (XmlNode use in useAnim)
                //{


                // tb.Animations.Add(a);
                //}


                //XmlNodeList useText = time.SelectNodes("useText");
                //foreach (XmlNode use in useText)
                //{


                //tb.Texts.Add(txt);
                //}


                f.TimeBlocks.Add(tb);
            }

            return f;
        }

        private static List<Renderable> ReadUsage(Dictionary<string, AnimationDef> animDefs, Dictionary<string, CharSet> charsets, Dictionary<string, TextDef> textDefs, Dictionary<string, UsageGroup> groupDefs, XmlNode time, TimeBlock parent)
        {
            List<Renderable> objects = new List<Renderable>();

            for (int i = 0; i < time.ChildNodes.Count; i++)
            {
                XmlNode use = time.ChildNodes[i];

                if (use.Name.ToLower() == "useanim")
                {
                    //AnimationDef aDef = (from ad in animDefs
                    //                     where ad.Name == use.Attributes["name"].Value
                    //                     select ad).ToList()[0];

                    Animation a = new Animation(parent, animDefs[use.Attributes["name"].Value])
                    {
                        X = int.Parse(use.Attributes["x"].Value),
                        Y = int.Parse(use.Attributes["y"].Value)
                    };
                    ReadRenderableParams(i, use, a);

                    // read animation attributes
                    a.Attributes = ReadUsageAttributes(use);

                    objects.Add(a);
                }
                else if (use.Name.ToLower() == "usetext")
                {
                    //TextDef tDef = (from t in textDefs
                    //                where t.Name == use.Attributes["name"].Value
                    //                select t).ToList()[0];

                    TextDef tdef;
                    if (use.Attributes["text"] != null && use.Attributes["charset"] != null)
                    {
                        tdef = new TextDef()
                        {
                            CharacterSet = charsets[use.Attributes["charset"].Value],
                            Value = use.Attributes["text"].Value
                        };
                    }
                    else
                    {
                        tdef = textDefs[use.Attributes["name"].Value];
                    }

                    Text txt = new Text(parent, tdef)
                    {
                        X = int.Parse(use.Attributes["x"].Value),
                        Y = int.Parse(use.Attributes["y"].Value),
                    };
                    ReadRenderableParams(i, use, txt);

                    // read text attributes
                    txt.Attributes = ReadUsageAttributes(use);

                    objects.Add(txt);
                }
                else if (use.Name.ToLower() == "usegroup")
                {
                    UsageGroup g = groupDefs[use.Attributes["name"].Value];
                    foreach (Renderable o in g.Objects)
                    {
                        Renderable oClone = o.Clone();
                        oClone.ParentBlock = parent;
                        objects.Add(oClone);
                    }
                }
            }
            return objects;
        }

        private static void ReadRenderableParams(int i, XmlNode use, Renderable a)
        {
            if (use.Attributes["startMs"] == null)
                a.StartMs = 0;
            else
                a.StartMs = int.Parse(use.Attributes["startMs"].Value);

            if (use.Attributes["endMs"] == null)
                a.EndMs = 0;
            else
                a.EndMs = int.Parse(use.Attributes["endMs"].Value);

            a.Alpha = 1;

            if (use.Attributes["zorder"] == null)
                a.ZOrder = i;
            else
                a.ZOrder = int.Parse(use.Attributes["zorder"].Value);

            if (use.Attributes["category"] == null)
                a.Category = System.Drawing.Color.Empty;
            else
                a.Category = System.Drawing.Color.FromName(use.Attributes["category"].Value);

        }

        private static List<AnimAttribute> ReadUsageAttributes(XmlNode use)
        {
            List<AnimAttribute> animAttributes = new List<AnimAttribute>();
            foreach (XmlNode attribute in use.ChildNodes)
            {
                if (attribute.Name == "attr")
                {
                    AnimAttribute aa = new AnimAttribute()
                    {
                        Name = attribute.Attributes["name"].Value
                    };

                    foreach (XmlAttribute att in attribute.Attributes)
                    {
                        if (att.Name == "startMs")
                            aa.StartMs = int.Parse(att.Value);
                        else
                            aa.Properties.Add(att.Name, att.Value);
                    }

                    animAttributes.Add(aa);
                }
            }
            return animAttributes;
        }
    }
}
