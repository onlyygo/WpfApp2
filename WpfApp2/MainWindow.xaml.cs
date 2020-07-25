using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.RightsManagement;
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

namespace WpfApp2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int height;
        private int width;
        private byte[] bigImage;
        private Stack st = new Stack();
        public MainWindow()
        {
            InitializeComponent();
            Stream imageStreamSource = new FileStream("C:/Users/WINDOWS10X/Desktop/20200718081251.jpg", FileMode.Open, FileAccess.Read, FileShare.Read);
            JpegBitmapDecoder decoder = new JpegBitmapDecoder(imageStreamSource, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
            BitmapSource bitmapSource = decoder.Frames[0];
            int h = bitmapSource.PixelHeight;
            int w = bitmapSource.PixelWidth;
            int rawStride = w * 3;
            byte[] rawImage = new byte[h * rawStride];
            bitmapSource.CopyPixels(rawImage, rawStride, 0);

            height = 810/12*12;
            width = 810/12*12;
            bigImage = new byte[height * width * 3];
            for (int i=0; i< height * width; i++) {
                int row2 = i / width;
                int col2 = i % width;
                int row1 = (int)(row2 * 1.0 / (height - 1) * (h-1));
                int col1 = (int)(col2 * 1.0 / (width - 1) * (w-1));
                bigImage[row2 * (width * 3) + col2 * 3 + 0] = rawImage[row1 * w * 3 + col1 * 3 + 0];
                bigImage[row2 * (width * 3) + col2 * 3 + 1] = rawImage[row1 * w * 3 + col1 * 3 + 1];
                bigImage[row2 * (width * 3) + col2 * 3 + 2] = rawImage[row1 * w * 3 + col1 * 3 + 2];
            }
            PixelFormat pf = PixelFormats.Bgr24;
            BitmapSource bitmap = BitmapSource.Create(width, height,
            96, 96, pf, null,
            bigImage, width * 3);
            // Draw the Image
            //Image myImage = new Image();
            myImage.Source = bitmap;
            myImage.Stretch = Stretch.None;
            myImage.Margin = new Thickness(20);
            st = new Stack();
        }

        static public bool is_zhi(int a) {
            for (int i=2; i<a;i++) {
                if (a%2==0) {
                    return false;
                }
            }
            return true;
        }
        static public void rotate_imageroi(ref byte[] img_ptr, int h, int w, int index) {
            int roih = h / 3 * 2;
            int roiw = w / 3 * 2;
            int roix = 0;
            int roiy = 0;
            switch (index) {
                case 1:
                    roix = w / 3;
                    roiy = 0;
                    break;
                case 2:
                    roiy = h / 3;
                    roix = 0;
                    break;
                case 3:
                    roix = w / 3;
                    roiy = h /3;
                    break;
            }
            rotate_roi(ref img_ptr, h, w, roix, roiy, roiw, roih);
        }
        static public void rotate_roi(ref byte[] img_ptr, int h, int w, int roix,int roiy, int roiw, int roih ) { 
            byte[] tmp_img = new byte[roiw * roih * 3];
            for (int row=0; row<roih;row++) {
                for (int col=0;col<roiw ;col++) {
                    int dsty = row - roih / 2;
                    int dstx = col - roiw / 2;
                    int srcx = dsty;
                    int srcy = -dstx;
                    int row2 = srcy + roih / 2;
                    int col2 = srcx + roiw / 2;
                    int row3 = row2 + roiy;
                    row3 = row3 > h - 1 ? h - 1 : row3;
                    int col3 = col2 + roix;
                    col3 = col3 > w - 1 ? w - 1 : col3;
                    tmp_img[row * roiw * 3 + col * 3 + 0] = img_ptr[row3 * w * 3 + col3 * 3 + 0];
                    tmp_img[row * roiw * 3 + col * 3 + 1] = img_ptr[row3 * w * 3 + col3 * 3 + 1];
                    tmp_img[row * roiw * 3 + col * 3 + 2] = img_ptr[row3 * w * 3 + col3 * 3 + 2];
                }
            }
            for (int row = 0; row < roih; row++)
            {
                for (int col = 0; col < roiw; col++)
                {
                    int row3 = row + roiy;
                    int col3 = col + roix;
                    img_ptr[row3 * w * 3 + col3 * 3 + 0] = tmp_img[row * roiw * 3 + col * 3 + 0];
                    img_ptr[row3 * w * 3 + col3 * 3 + 1] = tmp_img[row * roiw * 3 + col * 3 + 1];
                    img_ptr[row3 * w * 3 + col3 * 3 + 2] = tmp_img[row * roiw * 3 + col * 3 + 2];
                }
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            int flag = -1;
            switch (e.Key) {
                case Key.Q:
                    flag = 0;
                    break;
                case Key.W:
                    flag = 1;
                    break;
                case Key.A:
                    flag = 2;
                    break;
                case Key.S:
                    flag = 3;
                    break;
            }
            if (flag>=0) {
                st.Push(flag);
                st.Push(flag);
                st.Push(flag);
            }
            if (e.Key == Key.B && st.Count>0) {
                flag = (int)st.Pop();
            }
            if (flag >= 0) {
                rotate_imageroi(ref bigImage, height, width, flag);
                PixelFormat pf = PixelFormats.Bgr24;
                BitmapSource bitmap = BitmapSource.Create(width, height,
                96, 96, pf, null,
                bigImage, width * 3);
                myImage.Source = bitmap;
                myImage.Stretch = Stretch.None;
                myImage.Margin = new Thickness(20);
            }
        }
    }
}
