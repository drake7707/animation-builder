using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FFBattleGenerator.BLL;

namespace FFBattleGenerator
{
    public partial class Timeline : Control
    {
        public Timeline()
        {
            InitializeComponent();

            backBuffer = new Bitmap(this.Width, this.Height);
            updateBackbuffer = true;
            SetStyle(ControlStyles.UserPaint | ControlStyles.Selectable, true);
            SetStyle(ControlStyles.ContainerControl, false);
            SetStyle(ControlStyles.ResizeRedraw, true);
            
        }

        public void SetCursor(int ms)
        {
            currentMs = ms;
            Invalidate();
        }


        private string tooltipText;
        private ToolTip tooltip = new ToolTip();

        public string TooltipText
        {
            get
            {
                return tooltipText;
            }
            set
            {
                tooltipText = value;
                tooltip.SetToolTip(this, tooltipText);
            }
        }

        private Image backBuffer;
        private bool updateBackbuffer;
        private int currentMs;

        private bool showLegend = true;
        public bool ShowLegend
        {
            get
            {
                return showLegend;
            }
            set
            {
                showLegend = value;
                updateBackbuffer = true;
                Invalidate();
            }
        }

        private AnimationManager anim;
        public AnimationManager Animation
        {
            get
            {
                return anim;
            }
            set
            {
                anim = value;
                if (anim != null)
                {
                    anim.TimeChanged += new AnimationManager.TimeChangedHanlder(anim_TimeChanged);
                    sortedRenderables = GetSortedByLevelRenderables();
                }
                Invalidate();
            }
        }

        //build a list of all animation/texts in the time block and stack them in levels if they overlap
        private List<List<Renderable>> sortedRenderables = null;

        void anim_TimeChanged(int ms)
        {
            currentMs = ms;
            Invalidate();
        }

        protected override void OnResize(EventArgs e)
        {
            backBuffer = new Bitmap(this.Width, this.Height);
            updateBackbuffer = true;

            base.OnResize(e);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            //base.OnPaintBackground(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (Animation == null)
            {
                e.Graphics.Clear(BackColor);
                e.Graphics.DrawString("Assign an animation manager to this control", Font, Brushes.Black, new PointF(0, 0));
            }
            else
            {
                int totalDuration = Animation.Flow.TotalDuration;

                if (updateBackbuffer)
                    DrawBackbuffer(totalDuration);

                e.Graphics.DrawImage(backBuffer, 0, 0);

                Pen p = new Pen(Color.Black);
                e.Graphics.DrawLine(p, (int)((float)currentMs / (float)totalDuration * Width), 0, (int)((float)currentMs / (float)totalDuration * Width), Height);
            }

            base.OnPaint(e);
        }

        private void DrawBackbuffer(int totalDuration)
        {
            int height = (ShowLegend ? Height - 20 : Height);

            using (Graphics g = Graphics.FromImage(backBuffer))
            
            {
                g.Clear(Color.White);

                for (int i = 0; i < Animation.Flow.TimeBlocks.Count; i++)
                {
                    TimeBlock tb = Animation.Flow.TimeBlocks[i];
                    int left = (int)(((float)tb.From / (float)totalDuration) * Width);
                    int right = (int)(((float)tb.To / (float)totalDuration) * Width);

                    Brush b = new SolidBrush(GetColorFrom(i));
                    g.FillRectangle(b, new Rectangle(left, 0, right - left + 1, height));
                }

                int blockHeight = (height / sortedRenderables.Count);
                for (int j = 0; j < sortedRenderables.Count; j++)
                {
                    foreach (Renderable r in sortedRenderables[j])
                    {
                        int currectRStartMs = r.ParentBlock.From + r.StartMs;
                        int currentREndMs = r.ParentBlock.From + r.StartMs + r.TotalTime;

                        int leftRender = (int)(((float)currectRStartMs / (float)totalDuration) * (Width - 1));
                        int rightRender = (int)(((float)currentREndMs / (float)totalDuration) * (Width - 1));

                        Brush fillColor = Brushes.Black;
                        if (r.Category == Color.Empty)
                        {
                            if (r is Animation)
                                fillColor = Brushes.Yellow;
                            else if (r is Text)
                                fillColor = Brushes.White;
                        }
                        else
                            fillColor = new SolidBrush(r.Category);

                        g.FillRectangle(fillColor, new Rectangle(leftRender, j * blockHeight, rightRender - leftRender, blockHeight));
                        g.DrawRectangle(Pens.Black, new Rectangle(leftRender, j * blockHeight, rightRender - leftRender, blockHeight));
                    }
                }

                if (ShowLegend)
                {
                    // draw legend
                    int curms = 0;
                    while (curms < totalDuration)
                    {
                        int offset = (int)(((float)curms / (float)totalDuration) * (Width - 1));
                        g.DrawLine(Pens.Black, new Point(offset, height), new Point(offset, height + 2));

                        SizeF strSize = g.MeasureString((curms / 1000).ToString(), Font);
                        g.DrawString((curms / 1000).ToString(), Font, Brushes.Black, new PointF(offset - strSize.Width / 2f, height + 3));
                        curms += 1000;
                    }
                }
            }
            updateBackbuffer = false;
        }

        public List<List<Renderable>> GetSortedByLevelRenderables()
        {
            List<List<Renderable>> sortedRenderables = new List<List<Renderable>>();

            foreach (TimeBlock tb in anim.Flow.TimeBlocks)
            {
                List<Renderable> tbObjects = new List<Renderable>(tb.Objects);
                tbObjects.Sort((o1, o2) => o1.ZOrder.CompareTo(o2.ZOrder));

                foreach (Renderable r in tbObjects)
                {
                    bool added = false;
                    int curLevel = 0;
                    while (!added)
                    {
                        if (curLevel >= sortedRenderables.Count)
                        {
                            List<Renderable> level = new List<Renderable>();
                            level.Add(r);
                            sortedRenderables.Add(level);
                            added = true;
                        }
                        else
                        {
                            int currentRStartMs = tb.From + r.StartMs;
                            int currentREndMs = tb.From + r.StartMs + r.TotalTime;

                            bool overlap = false;
                            foreach (Renderable addedR in sortedRenderables[curLevel])
                            {
                                int addedRStartMs = addedR.ParentBlock.From + addedR.StartMs;
                                int addedREndMs = addedR.ParentBlock.From + addedR.StartMs + addedR.TotalTime;

                                //if ((currentRStartMs > addedRStartMs && currentRStartMs < addedREndMs) ||
                                //    (currentREndMs > addedRStartMs && currentREndMs < addedREndMs) ||
                                //    (currentRStartMs == addedRStartMs && currentREndMs == addedREndMs))
                                if (!(currentRStartMs >= addedREndMs || currentREndMs <= addedRStartMs))
                                {
                                    overlap = true;
                                    break;
                                }
                            }
                            if (!overlap)
                            {
                                sortedRenderables[curLevel].Add(r);
                                added = true;
                            }
                        }
                        curLevel++;
                    }
                }
            }

            return sortedRenderables;
        }

        private Color GetColorFrom(int color)
        {
            switch (color)
            {
                case 0:
                    return Color.FromArgb(128, 128, 128);
                case 1:
                    return Color.FromArgb(0, 0, 255);
                case 2:
                    return Color.FromArgb(0, 255, 0);
                case 3:
                    return Color.FromArgb(255, 0, 0);
                case 4:
                    return Color.FromArgb(255, 255, 0);
                case 5:
                    return Color.FromArgb(0, 255, 255);
                case 6:
                    return Color.FromArgb(255, 0, 255);
                case 7:
                    return Color.FromArgb(128, 128, 255);
                case 8:
                    return Color.FromArgb(128, 255, 128);
                case 9:
                    return Color.FromArgb(255, 128, 128);
                case 10:
                    return Color.FromArgb(255, 255, 128);
                case 11:
                    return Color.FromArgb(128, 255, 255);
                case 12:
                    return Color.FromArgb(255, 128, 255);
                case 13:
                    return Color.FromArgb(0, 0, 128);
                case 14:
                    return Color.FromArgb(0, 128, 0);
                case 15:
                    return Color.FromArgb(128, 0, 0);
                case 16:
                    return Color.FromArgb(128, 128, 0);
                case 17:
                    return Color.FromArgb(0, 128, 128);
                case 18:
                    return Color.FromArgb(128, 0, 128);
                default:
                    return Color.FromArgb(192, 192, 192);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (anim == null) return;

            if (e.Button == MouseButtons.Left)
            {
                OnMouseUp(e);
                return;
            }

            int totalDuration = Animation.Flow.TotalDuration;

            int blockHeight = ((Height - sortedRenderables.Count) / sortedRenderables.Count);

            string newtext = "";

            for (int j = 0; j < sortedRenderables.Count; j++)
            {
                foreach (Renderable r in sortedRenderables[j])
                {
                    int currectRStartMs = r.ParentBlock.From + r.StartMs;
                    int currentREndMs = r.ParentBlock.From + r.StartMs + r.TotalTime;

                    int leftRender = (int)(((float)currectRStartMs / (float)totalDuration) * (Width - 1));
                    int rightRender = (int)(((float)currentREndMs / (float)totalDuration) * (Width - 1));

                    Rectangle rect = new Rectangle(leftRender, j * blockHeight, rightRender - leftRender, blockHeight);
                    if (e.X >= rect.Left && e.X <= rect.Right && e.Y >= rect.Top && e.Y <= rect.Bottom)
                    {
                        if (r is Animation)
                            newtext = ((Animation)r).Definition.Name + " [" + r.StartMs + "-" + (r.StartMs + r.TotalTime) + "]";
                        else if (r is Text)
                            newtext = ((Text)r).Definition.Value + " [" + r.StartMs + "-" + (r.StartMs + r.TotalTime) + "]";

                        if (tooltipText != newtext)
                            TooltipText = newtext;
                        return;
                    }
                }
            }

            TooltipText = "";

            base.OnMouseMove(e);
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left)
                currentMs -= 25;
            else if(e.KeyCode == Keys.Right)
                currentMs += 25;
            CursorPositionChangedHanlder temp = CursorPositionChanged;
            if (temp != null)
                temp(currentMs);

            base.OnKeyDown(e);
        }


        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Left ||
               keyData == Keys.Right)
            {
                OnKeyDown(new KeyEventArgs(keyData));
                return true;
            }
            else 
                return base.ProcessCmdKey(ref msg, keyData);
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (anim == null) return;

            this.Focus();

            int totalDuration = anim.Flow.TotalDuration;
            int ms = (int)(((float)e.X / (float)Width) * totalDuration);

            CursorPositionChangedHanlder temp = CursorPositionChanged;
            if (temp != null)
                temp(ms);

            base.OnMouseUp(e);
        }

        public delegate void CursorPositionChangedHanlder(int ms);
        public event CursorPositionChangedHanlder CursorPositionChanged;


    }
}
