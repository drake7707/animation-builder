using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FFBattleGenerator.BLL;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using ImageQuantization;

namespace FFBattleGenerator
{

    public partial class Form1 : Form
    {
        //private List<string> images = new List<string>();
        //private List<int> delays = new List<int>();

        private string xmlFile;
        private AnimationManager anim = null;
        //private int curMs = 0;
        private int msLastUpdated = 0;
        private int frameCount = 0;

        public Form1()
        {
            InitializeComponent();
           
        }

        private void btnDraw_Click(object sender, EventArgs e)
        {
            if (anim == null) return;

            picPreview.Image = AnimationDrawer.GetImageFromFrame(anim, chkDisplayDebugInfo.Checked);
            DrawZoom(mousex, mousey);
        }


        private void tmrAnimation_Tick(object sender, EventArgs e)
        {
            if (anim.CurrentMs < 0 || anim.CurrentMs > anim.Flow.TotalDuration)
            {
                tmrAnimation.Enabled = false;
                btnStop.Text = "Start";
            }
            //if (curMs > anim.Flow.TotalDuration)
            //    tmrAnimation.Enabled = false;

            bool needToUpdate = anim.NextFrame(25);
            
            if (needToUpdate)
            {

                picPreview.Image = AnimationDrawer.GetImageFromFrame(anim, chkDisplayDebugInfo.Checked);
                DrawZoom(mousex, mousey);
                picPreview.Refresh();

                msLastUpdated = anim.CurrentMs;
                frameCount++;
            }

            lblStatus.Text = anim.CurrentMs.ToString() + " " + msLastUpdated.ToString() + " frames: " + frameCount;
            lblDescription.Text = anim.CurrentTimeBlock.Description;
           // UpdateAnimList();

        }

        private void UpdateAnimList()
        {
            if (anim == null) return;


            if (anim.CurrentTimeBlock != oldTimeBlock)
                lstOverview.Items.Clear();
            int count = 0;
            lstOverview.BeginUpdate();
            foreach (Renderable o in anim.CurrentTimeBlock.Objects)
            {
                ListViewItem itm = null;
                if (o is Animation)
                {
                    Animation a = (Animation)o;
                    itm = new ListViewItem(new string[] {
                            "A",
                            a.Definition.Name,
                            a.StartMs.ToString(),
                            (a.StartMs + a.TotalTime).ToString(),
                            a.X.ToString(),
                            a.Y.ToString(),
                            a.Alpha.ToString(),
                            a.Started ? "Y" : "N",
                            a.Finished ? "Y" : "N"
                        });
                    itm.BackColor = (a.Started && !a.Finished) ? Color.LightGreen : Color.White;
                }
                else if (o is Text)
                {
                    Text t = (Text)o;
                    itm = new ListViewItem(new string[] {
                            "T",
                            t.Definition.Name,
                            t.StartMs.ToString(),
                            (t.StartMs + t.TotalTime).ToString(),
                            t.X.ToString(),
                            t.Y.ToString(),
                            t.Alpha.ToString(),
                            t.Started ? "Y" : "N",
                            t.Finished ? "Y" : "N",
                            t.TextString 
                        });
                    itm.BackColor = (t.Started && !t.Finished) ? Color.LightGreen : Color.White;
                }

                if (anim.CurrentTimeBlock != oldTimeBlock)
                    lstOverview.Items.Add(itm);
                else
                    lstOverview.Items[count] = itm;

                count++;
            }
            if (anim.CurrentTimeBlock != oldTimeBlock)
                oldTimeBlock = anim.CurrentTimeBlock;
            lstOverview.EndUpdate();
        }
        private TimeBlock oldTimeBlock = null;

        private void btnReload_Click(object sender, EventArgs e)
        {
            if (xmlFile == null) return;

            ResetAnimation();

            btnStop.Text = "Start";
            btnStop_Click(btnStop, EventArgs.Empty);
        }

        private void ResetAnimation()
        {
            anim = new AnimationManager(xmlFile);

            // set the relative path to where the xml file is, otherwise it won't load the images correctly
            Environment.CurrentDirectory = System.IO.Path.GetDirectoryName(xmlFile);

            timeline.Animation = anim;
            picPreview.Width = anim.Width;
            picPreview.Height = anim.Height;
            msLastUpdated = 0;
            frameCount = 0;
        }

        private void btnSaveGif_Click(object sender, EventArgs e)
        {
            if (xmlFile == null) return;

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Title = "Save animation as...";
                sfd.Filter = "*.gif|*.gif";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    //SaveGif(sfd.FileName);
                    System.Threading.Thread t = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(SaveGif));
                    t.Start(sfd.FileName);
                }
            }
        }

        private void SaveGif(object filename)
        {
            List<string> framePaths = new List<string>();
            List<int> delays = new List<int>();

            if (!Directory.Exists("output"))
                Directory.CreateDirectory("output");

            AnimationManager anim = new AnimationManager(xmlFile);
            long ms = 0;
            int FRAME_MS = 50;
            long lastUpdateMs = 0;

            long lastProgressRefresh = DateTime.Now.Ticks;

            while (ms < anim.Flow.TotalDuration)
            {
                bool needToUpdate = anim.NextFrame(FRAME_MS);

                if (needToUpdate)
                {

                    Image img = AnimationDrawer.GetImageFromFrame(anim, false);
                    string framePath = "output\\frame" + framePaths.Count + ".gif";

                    OctreeQuantizer quantizer = new OctreeQuantizer(255, 4);
                    using (Bitmap quantized = quantizer.Quantize(img))
                    {
                        quantized.Save(framePath, ImageFormat.Gif);
                    }

                    framePaths.Add(framePath);
                    int delay = (int)((ms - lastUpdateMs) / 10);
                    if (delay <= 0)
                        delay = 1;
                    delays.Add(delay);
                    lastUpdateMs = ms;

                    if (DateTime.Now.Ticks - lastProgressRefresh > 10000000)
                    {
                        SetSaveGifProgress("Generating frames", (float)ms / (float)anim.Flow.TotalDuration);
                        lastProgressRefresh = DateTime.Now.Ticks;
                    }

                }

                ms += FRAME_MS;
            }

            GifCreator.GifCreator gifCreator = new GifCreator.GifCreator();

            GifCreator.GifCreator.ProgressHandler progressHandler = new GifCreator.GifCreator.ProgressHandler(GifCreator_Progress);
            gifCreator.Progress += progressHandler;
            gifCreator.CreateAnimatedGif(framePaths, delays, filename.ToString());
            gifCreator.Progress -= progressHandler;

            SetSaveGifProgress("Save complete", 1f);
        }

        void GifCreator_Progress(float perc)
        {
            SetSaveGifProgress("Saving animated gif file", perc);
        }

        public void SetSaveGifProgress(string action, float perc)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new System.Threading.ThreadStart(() =>
                {
                    lblStatus.Text = action;
                    barStatus.Value = (int)(barStatus.Minimum + perc * (barStatus.Maximum - barStatus.Minimum));
                }));
            }
            else
            {
                lblStatus.Text = action;
                barStatus.Value = (int)(barStatus.Minimum + perc * (barStatus.Maximum - barStatus.Minimum));
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (xmlFile == null) return;

            if (btnStop.Text == "Stop")
            {
                tmrAnimation.Enabled = false;
                btnStop.Text = "Start";
            }
            else
            {
                tmrAnimation.Enabled = true;
                btnStop.Text = "Stop";
            }
        }


        private void picPreview_MouseMove(object sender, MouseEventArgs e)
        {

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                mousex = e.X;
                mousey = e.Y;

                DrawZoom(mousex, mousey);
            }
        }

        private void DrawZoom(int x, int y)
        {
            if (picPreview.Image != null)
            {

                int zoom = 8;

                Bitmap b = new Bitmap(picZoom.Width, picZoom.Height);
                using (Graphics g = Graphics.FromImage(b))
                {
                    ImageAttributes ia = new ImageAttributes();

                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                    g.DrawImage(picPreview.Image, new Rectangle(0, 0, picZoom.Width, picZoom.Height),/* new Rectangle(*/x - picZoom.Width / (2 * zoom), y - picZoom.Height / (2 * zoom), picZoom.Width / zoom, picZoom.Height / zoom, GraphicsUnit.Pixel);
                }
                picZoom.Image = b;
            }
        }

        private int mousex;
        private int mousey;

        private void timeline_CursorPositionChanged(int toMs)
        {
         //   ResetAnimation();
            //curMs = toMs;

            //int ms = 0;
            //while (ms < curMs)
            //{
            //    anim.NextFrame(25);
            //    ms += 25;
            //}
            if (anim.CurrentMs < toMs)
            {
                while (anim.CurrentMs < toMs)
                    anim.NextFrame(25);
            }
            else
            {
                while (anim.CurrentMs > toMs)
                    anim.NextFrame(-25);
            }

            lblStatus.Text = anim.CurrentMs.ToString() + " " + msLastUpdated.ToString() + " frames: " + frameCount;
//            UpdateAnimList();

            btnDraw_Click(null, EventArgs.Empty);
        }

        private void btnUpdateAnimlist_Click(object sender, EventArgs e)
        {
            UpdateAnimList();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "*.xml|*.xml";
                if(ofd.ShowDialog(this) == DialogResult.OK)
                {
                    xmlFile = ofd.FileName;
                    ResetAnimation();
                    
                }
            }
             
        }
    }
}
