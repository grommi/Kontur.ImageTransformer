using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Kontur.ImageTransformer
{
    class HardLogic : IHandlerLogic
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int HandleGrayscale(int x, int y, int w, int h, HttpListenerContext listenerContext)
        {
            link grayscale = (r, g, b) => (byte)((r + g + b) / 3);
            return handleWithFilter(x, y, w, h, listenerContext, grayscale, grayscale, grayscale, true);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int HandleSepia(int x, int y, int w, int h, HttpListenerContext listenerContext)
        {
            link newR = (r, g, b) => (byte)Math.Min(((r * .393) + (g * .769) + (b * .189)), 255);
            link newG = (r, g, b) => (byte)Math.Min(((r * .349) + (g * .686) + (b * .168)), 255);
            link newB = (r, g, b) => (byte)Math.Min(((r * .272) + (g * .534) + (b * .131)), 255);

            return handleWithFilter(x, y, w, h, listenerContext, newR, newG, newB, false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int HandleThreshold(int x, int y, int w, int h, int thresholdValue, HttpListenerContext listenerContext)
        {

            link threshold = (r, g, b) => (byte)(((r + g + b) / 3 >= (255 * thresholdValue / 100)) ? 255 : 0);
            return handleWithFilter(x, y, w, h, listenerContext, threshold, threshold, threshold, true);

        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int handleWithFilter(int x, int y, int w, int h, HttpListenerContext listenerContext, link redLink, link greenLink, link blueLink, bool isBlackAndWhite)
        {
            Console.WriteLine("start");
            PngBitmapDecoder decoder = new PngBitmapDecoder(listenerContext.Request.InputStream,BitmapCreateOptions.PreservePixelFormat,BitmapCacheOption.OnDemand);
            BitmapSource bmpSourse = decoder.Frames[0];
            int leftPoint = (w >= 0) ? x : x + w;
            int upperPoint = (h >= 0) ? y : y + h;
            int absW = Math.Abs(w);
            int absH = Math.Abs(h);
            int xFrom = Math.Max(leftPoint, 0);
            int yFrom = Math.Max(upperPoint, 0);
            int xTo = Math.Min(leftPoint + absW, (int)bmpSourse.PixelWidth);
            int yTo = Math.Min(upperPoint + absH, (int)bmpSourse.PixelHeight);
            int oldStride = (int)bmpSourse.PixelWidth;
            byte[] buf = new byte[4 * ((xTo - xFrom) + (yTo - yFrom) * oldStride)];
            try
            {                
                bmpSourse.CopyPixels(new System.Windows.Int32Rect(xFrom, yFrom, xTo - xFrom, yTo - yFrom), buf, 4 * (int)bmpSourse.PixelWidth, 0);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                Console.Read();
            }
            int width = (xTo - xFrom);
            int height = (yTo - yFrom);
            int stride = width * 4;
            byte[] buf2 = new byte[4 * width * height];
            byte oldB, oldG, oldR;
            for (int i= 0; i < height; i += 1)
            {
                for(int j =0;j<(stride);j+=4)
                {
                    oldB = buf[4*i*oldStride+j];
                    oldG = buf[4*i * oldStride + j + 1];
                    oldR = buf[4*i * oldStride + j + 2];
                    
                    buf2[i*stride+j] = blueLink(oldR, oldG, oldB);
                    buf2[i * stride + j + 1] = greenLink(oldR, oldG, oldB);
                    buf2[i * stride + j + 2] = redLink(oldR, oldG, oldB);
                    buf2[i * stride + j+3]= buf[4*i * oldStride + j + 3];
                }
            }
                
           
            
            BitmapSource image = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgra32, null, buf2, stride);
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Interlace = PngInterlaceOption.On;
            encoder.Frames.Add(BitmapFrame.Create(image));
            MemoryStream inMemory = new MemoryStream();
            encoder.Save(inMemory);
            byte[] buffer = inMemory.GetBuffer();
            listenerContext.Response.OutputStream.Write(buffer, 0, buffer.Length);
            //FileStream fs= new FileStream("out.png", FileMode.OpenOrCreate);
            //fs.Write(buffer, 0, buffer.Length);
            //fs.Close();

            Console.WriteLine("end");
            return 200;
        }

        //delegate Color filter(Color c);
        delegate byte link(byte r, byte g, byte b);

        
    }
    

    
}
