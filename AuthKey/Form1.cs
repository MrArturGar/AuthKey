using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Emgu;
using Emgu.Util;
using Emgu.CV;
using Emgu.CV.Util;
using Emgu.CV.Structure;

namespace AuthKey
{
    public partial class Form1 : Form
    {
        private VideoCapture capture = null;
        private double frames;
        private double framesCounter;
        private double fps;

        private bool play = false;
        private static CascadeClassifier classifier = new CascadeClassifier("haarcascade_frontalface_alt_tree.xml");



        public Form1()
        {
            InitializeComponent();
        }

        private Image<Bgr, byte> Find(Image<Bgr, byte> image)
        {
            MCvObjectDetection[] regions;

            using (HOGDescriptor descriptor = new HOGDescriptor())
            {
                descriptor.SetSVMDetector(HOGDescriptor.GetDefaultPeopleDetector());

                regions = descriptor.DetectMultiScale(image);
            }


            foreach (MCvObjectDetection pesh in regions)
            {
                image.Draw(pesh.Rect, new Bgr(Color.Red), 3);

                CvInvoke.PutText(image, "Face", new Point(pesh.Rect.X, pesh.Rect.Y), Emgu.CV.CvEnum.FontFace.HersheyPlain, 1, new MCvScalar(255, 255, 255), 2);
            }


            Rectangle[] faces = classifier.DetectMultiScale(image, 1.4, 0);
            foreach (Rectangle face in faces)
            {
                using (Graphics graphics = Graphics.FromImage(image.Bitmap))
                {
                    using (Pen pen = new Pen(Color.Yellow, 3))
                    {
                        graphics.DrawRectangle(pen, face);
                    }
                }
            }


            return image;
        }

        private async void ReadFrames()
        {
            Mat m = new Mat();

            while (play && framesCounter< frames)
            {
                framesCounter++;

                capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.PosFrames, framesCounter);

                capture.Read(m);

                pictureBox1.Image = m.Bitmap;

                pictureBox2.Image = Find(m.ToImage<Bgr, byte>()).Bitmap;

                labelStatus.Text = $"{framesCounter} / {frames}";

                await Task.Delay(1000 / Convert.ToInt16(fps));
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult result = openFileDialog1.ShowDialog();
                
                if (result == DialogResult.OK)
                {
                    capture = new VideoCapture(openFileDialog1.FileName);
                    Mat mat = new Mat();
                    capture.Read(mat);

                    pictureBox1.Image = mat.Bitmap;

                    fps = capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Fps);

                    frames = capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameCount);

                    framesCounter = 1;
                }

            }
            catch (Exception ex)
            {
                ShowException(ex);
            }
        }

        private void ShowException(Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void findToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (capture == null)
                    throw new Exception("Video ivalid");

                play = true;

                ReadFrames();
            }
            catch (Exception ex)
            {
                ShowException(ex);
            }
        }

        private void pauseButton_Click(object sender, EventArgs e)
        {
            try
            {
                play = false;
            }
            catch (Exception ex)
            {
                ShowException(ex);
            }

        }

        private void resumeButton_Click(object sender, EventArgs e)
        {

            try
            {
                findToolStripMenuItem_Click(null, null);
            }
            catch (Exception ex)
            {
                ShowException(ex);
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                framesCounter -= Convert.ToDouble(numericUpDown1.Value);
            }
            catch (Exception ex)
            {
                ShowException(ex);
            }

        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            try
            {
                framesCounter += Convert.ToDouble(numericUpDown1.Value);

            }
            catch (Exception ex)
            {
                ShowException(ex);
            }

        }
    }
}
