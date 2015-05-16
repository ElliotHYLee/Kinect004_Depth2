using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Coordinate
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeNui();
        }

        KinectSensor nui = null;

        void InitializeNui()
        {
            nui = KinectSensor.KinectSensors[0];

            nui.ColorStream.Enable();
            nui.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(nui_ColorFrameReady);
            nui.DepthStream.Enable();
            nui.DepthStream.Range = DepthRange.Near;
            nui.SkeletonStream.Enable();
            nui.SkeletonStream.EnableTrackingInNearRange = true;

            nui.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(nui_AllFramesReady);


        }

        void nui_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame ImagePara = e.OpenColorImageFrame())
            {
                if (ImagePara == null) return;
                byte[] ImageBits = new byte[ImagePara.PixelDataLength];
                ImagePara.CopyPixelDataTo(ImageBits);

                BitmapSource src = null;

                src = BitmapSource.Create(ImagePara.Width, ImagePara.Height, 96, 96, PixelFormats.Bgr32, null, ImageBits, ImagePara.Width * ImagePara.BytesPerPixel);
                image1.Source = src;
            }
        }

        void nui_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            using (SkeletonFrame sf = e.OpenSkeletonFrame())
            {
                if (sf == null) return;

                Skeleton[] skeletonData = new Skeleton[sf.SkeletonArrayLength];
                sf.CopySkeletonDataTo(skeletonData);

                using (DepthImageFrame depthImagePara = e.OpenDepthImageFrame())
                {
                    Console.WriteLine("im here1");
                    if (depthImagePara != null)
                    {
                        Console.WriteLine("im here2");
                        foreach (Skeleton sd in skeletonData)
                        {
                            if (sd.TrackingState == SkeletonTrackingState.Tracked)
                            {
                                Console.WriteLine("im here3");
                                Joint joint = sd.Joints[JointType.HandLeft];
                                DepthImagePoint depthPoint;
                                depthPoint = depthImagePara.MapFromSkeletonPoint(joint.Position);
                                Point point = new Point( (int) (image1.Width *depthPoint.X/depthImagePara.Width), (int)(image1.Height *depthPoint.Y/depthImagePara.Height));
                                textBlock1.Text = string.Format("X:{0:0.00} Y:{1:0.00} Z:{2:0.00}m", point.X, point.Y, joint.Position.Z);
                                Canvas.SetLeft(ellipse1, point.X - ellipse1.Width / 2);
                                Canvas.SetTop(ellipse1, point.Y - ellipse1.Height / 2);
                            }
                        }
                    }
                }
            }
        }

        private void btn1_Click(object sender, RoutedEventArgs e)
        {
            nui.Start();
        }



    }
}
