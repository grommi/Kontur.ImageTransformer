using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Kontur.ImageTransformer
{
    class MediumLogic : IHandlerLogic
    {

        public int HandleGrayscale(int x, int y, int w, int h, HttpListenerContext listenerContext)
        {
            int intensity;
            filter grayscale = c => Color.FromArgb(c.A, intensity = (c.R + c.G + c.B) / 3, intensity, intensity);
            return handleWithFilter(x, y, w, h, listenerContext, grayscale);
        }

        public int HandleSepia(int x, int y, int w, int h, HttpListenerContext listenerContext)
        {
            link newR = c => Math.Min((int)((c.R * .393) + (c.G * .769) + (c.B * .189)), 255);
            link newG = c => Math.Min((int)((c.R * .349) + (c.G * .686) + (c.B * .168)), 255);
            link newB = c => Math.Min((int)((c.R * .272) + (c.G * .534) + (c.B * .131)), 255);
            filter sepia = c => Color.FromArgb(c.A, newR(c), newG(c), newB(c));
            return handleWithFilter(x, y, w, h, listenerContext, sepia);
        }


        public int HandleThreshold(int x, int y, int w, int h, int thresholdValue, HttpListenerContext listenerContext)
        {
            Console.WriteLine(thresholdValue);
            int newRGB;
            filter threshold = c => Color.FromArgb(c.A, newRGB = ((c.R + c.G + c.B) / 3 >= (255 * thresholdValue / 100)) ? 255 : 0, newRGB, newRGB);
            return handleWithFilter(x, y, w, h, listenerContext, threshold);

        }
        private int handleWithFilter(int x, int y, int w, int h, HttpListenerContext listenerContext, filter filtr)
        {
            
            Bitmap bmp;
            try
            {
                bmp = new Bitmap(listenerContext.Request.InputStream);
            }
            catch
            {
                return 400;
            }

            int leftPoint = (w >= 0) ? x : x + w;
            int upperPoint = (h >= 0) ? y : y + h;
            int absW = Math.Abs(w);
            int absH = Math.Abs(h);
            int xFrom = Math.Max(leftPoint, 0);
            int yFrom = Math.Max(upperPoint, 0);
            int xTo = Math.Min(leftPoint + absW, bmp.Width);
            int yTo = Math.Min(upperPoint + absH, bmp.Height);
            Bitmap img = new Bitmap(xTo - xFrom, yTo - yFrom, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            if ((xTo > xFrom) && (yTo > yFrom))
            {        
                
                var bd = bmp.LockBits(
                    new Rectangle(xFrom, yFrom, xTo-xFrom, yTo-yFrom),
                    ImageLockMode.ReadOnly,
                     System.Drawing.Imaging.PixelFormat.Format32bppArgb
                );               
                
                var buffer = new byte[bd.Stride * bd.Height];                
                Marshal.Copy(bd.Scan0, buffer, 0, buffer.Length);
                
                int l = buffer.Length >> 2;
                for (int i = 0; i < l; i++)
                {
                    Color c = filtr( Color.FromArgb(buffer[(i << 2) + 3], buffer[(i << 2) + 2], buffer[(i << 2) + 1], buffer[i << 2]));
                    buffer[i << 2] = c.B;
                    buffer[(i << 2) + 1] = c.G;
                    buffer[(i << 2) + 2] = c.R;
                    //byte intencity = (byte)((buffer[i << 2] + buffer[(i << 2) + 1] + buffer[(i << 2) + 2])/3);
                    //buffer[i << 2] = buffer[(i << 2) + 1] = buffer[(i << 2) + 2] = intencity;
                }
                var bdi = img.LockBits(
                    new Rectangle(xFrom, yFrom, xTo - xFrom, yTo - yFrom),
                    ImageLockMode.WriteOnly,
                     System.Drawing.Imaging.PixelFormat.Format32bppArgb
                );
                Marshal.Copy(buffer, 0, bdi.Scan0, buffer.Length);
                //Bitmap img = new Bitmap(xTo - xFrom, yTo - yFrom, (xTo - xFrom)<<2, System.Drawing.Imaging.PixelFormat.Format32bppArgb,bd.Scan0);                

                img.Save("newpng.png", System.Drawing.Imaging.ImageFormat.Png);
                //listenerContext.Response.ContentEncoding = listenerContext.Request.ContentEncoding;
                img.Save(listenerContext.Response.OutputStream, System.Drawing.Imaging.ImageFormat.Png);
                //listenerContext.Response.AppendHeader("ContentType", "image/png");
                //listenerContext.Response.ContentType = "image/png";
                img.UnlockBits(bdi);
                bmp.UnlockBits(bd);
                bmp.Dispose();
                img.Dispose();
                return 200;
            }
            else
            {
                return 204;
            }
        }

        delegate Color filter(Color c);
        delegate int link(Color c);

        private int HandleWithFilter(int x, int y, int w, int h, HttpListenerContext listenerContext)
        {
                    
            return 429;
        }
    }
}
