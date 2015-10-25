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
            // set a kinect senssor
            nui = KinectSensor.KinectSensors[0];
            // set colorstream
            nui.ColorStream.Enable();
            nui.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(nui_ColorFrameReady);
            // set a depth steam
            nui.DepthStream.Enable();
            nui.DepthStream.Range = DepthRange.Near;
            // set a skeleton steam
            nui.SkeletonStream.Enable();
            nui.SkeletonStream.EnableTrackingInNearRange = true;
            // for all frames
            nui.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(nui_AllFramesReady);


        }

        void nui_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            // do folowings when color stream comes in
            using (ColorImageFrame ImagePara = e.OpenColorImageFrame())
            {
                // return if the color stream is empty
                if (ImagePara == null) return;

                // put image stream's pixel data into byte array
                byte[] ImageBits = new byte[ImagePara.PixelDataLength];
                ImagePara.CopyPixelDataTo(ImageBits);
            
                // update image1
                BitmapSource src = null;
                src = BitmapSource.Create(ImagePara.Width, ImagePara.Height, 96, 96, PixelFormats.Bgr32, null, ImageBits, ImagePara.Width * ImagePara.BytesPerPixel);
                image1.Source = src;
            }
        }

        void nui_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            using (SkeletonFrame sf = e.OpenSkeletonFrame())
            {
                // return if empty
                if (sf == null) return;

                // prepare skeloton stream
                Skeleton[] skeletonData = new Skeleton[sf.SkeletonArrayLength];
                sf.CopySkeletonDataTo(skeletonData);

                using (DepthImageFrame depthImagePara = e.OpenDepthImageFrame())
                {
                    //Console.WriteLine("im here1");
                    
                    if (depthImagePara != null)
                    {
                        //Console.WriteLine("im here2");
                        

                        foreach (Skeleton sd in skeletonData)
                        {
                            if (sd.TrackingState == SkeletonTrackingState.Tracked)
                            {
                                //Console.WriteLine("im here3");
                                
                                //prepare joint
                                Joint leftHand = sd.Joints[JointType.HandLeft];
                                Joint rightHand = sd.Joints[JointType.HandRight];
                                Joint shoulderCenter = sd.Joints[JointType.Spine];


                                // a skelton's depth
                                DepthImagePoint depthPointLeftHand, depthPointRightHand, depthPointShoulderCenter;
                                depthPointLeftHand = depthImagePara.MapFromSkeletonPoint(leftHand.Position);
                                depthPointRightHand = depthImagePara.MapFromSkeletonPoint(rightHand.Position);
                                depthPointShoulderCenter = depthImagePara.MapFromSkeletonPoint(shoulderCenter.Position);

                                // skeleton's x,y position
                                Point pointLeftHand = new Point( (int) (image1.Width *depthPointLeftHand.X/depthImagePara.Width), (int)(image1.Height *depthPointLeftHand.Y/depthImagePara.Height));
                                Point pointRightHand = new Point((int)(image1.Width * depthPointRightHand.X / depthImagePara.Width), (int)(image1.Height * depthPointRightHand.Y / depthImagePara.Height));
                                Point pointShoulderCenter = new Point((int)(image1.Width * depthPointShoulderCenter.X / depthImagePara.Width), (int)(image1.Height * depthPointShoulderCenter.Y / depthImagePara.Height));

                                //  update textblock
                                txtLeftHand.Text = string.Format("X:{0:0.00} Y:{1:0.00} Z:{2:0.00}m", leftHand.Position.X, leftHand.Position.Y, leftHand.Position.Z);
                                txtRightHand.Text = string.Format("X:{0:0.00} Y:{1:0.00} Z:{2:0.00}m", rightHand.Position.X, rightHand.Position.Y, rightHand.Position.Z);
                                txtBody.Text = string.Format("X:{0:0.00} Y:{1:0.00} Z:{2:0.00}m", shoulderCenter.Position.X, shoulderCenter.Position.Y, shoulderCenter.Position.Z);

                                
                                // calculate and update distance
                                double lhx = leftHand.Position.X;
                                double lhy = leftHand.Position.Y;
                                double lhz = leftHand.Position.Z;
                                double rhx = rightHand.Position.X;
                                double rhy = rightHand.Position.Y;
                                double rhz = rightHand.Position.Z;
                                double bdx = shoulderCenter.Position.X;
                                double bdy = shoulderCenter.Position.Y;
                                double bdz = shoulderCenter.Position.Z;

                                double dlh = Math.Sqrt(Math.Pow(lhx, 2) + Math.Pow(lhy, 2) + Math.Pow(lhz, 2));
                                double drh = Math.Sqrt(Math.Pow(rhx, 2) + Math.Pow(rhy, 2) + Math.Pow(rhz, 2));
                                double dbd = Math.Sqrt(Math.Pow(bdx, 2) + Math.Pow(bdy, 2) + Math.Pow(bdz, 2));
                                double l2r = Math.Sqrt(Math.Pow(lhx-rhx, 2) + Math.Pow(lhy-rhy, 2) + Math.Pow(lhz-rhz, 2));

                                // update auxiliary textblock
                                txtDelta.Text = string.Format("X:{0:0.00} Y:{1:0.00} Z:{2:0.00}m", leftHand.Position.X - shoulderCenter.Position.X, leftHand.Position.Y - shoulderCenter.Position.Y, leftHand.Position.Z - shoulderCenter.Position.Z);
                                txtDistanceLeftHand.Text = dlh.ToString().Substring(0,4);
                                txtDistanceRightHand.Text = drh.ToString().Substring(0, 4);
                                txtDistanceBody.Text = dbd.ToString().Substring(0, 4);
                                txtDistanceL2R.Text = l2r.ToString().Substring(0, 4);
                                



                                // update circle's position
                                Canvas.SetLeft(ellipseLeftHand, pointLeftHand.X - ellipseLeftHand.Width / 2);
                                Canvas.SetTop(ellipseLeftHand, pointLeftHand.Y - ellipseLeftHand.Height / 2);

                                Canvas.SetLeft(ellipseRightHand, pointRightHand.X - ellipseRightHand.Width / 2);
                                Canvas.SetTop(ellipseRightHand, pointRightHand.Y - ellipseRightHand.Height / 2);

                                Canvas.SetLeft(ellipseBody, pointShoulderCenter.X - ellipseBody.Width / 2);
                                Canvas.SetTop(ellipseBody, pointShoulderCenter.Y - ellipseBody.Height / 2);

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
