using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace Kontur.ImageTransformer
{
    class MediumLogic : IHandlerLogic
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int HandleGrayscale(int x, int y, int w, int h, HttpListenerContext listenerContext)
        {            
            link grayscale = (r, g, b) => (byte)((r + g+ b) / 3);
            return handleWithFilter(x, y, w, h, listenerContext, grayscale, grayscale, grayscale,true);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int HandleSepia(int x, int y, int w, int h, HttpListenerContext listenerContext)
        {
            link newR = (r ,g, b) => (byte)Math.Min((int)((r * .393) + (g * .769) + (b * .189)), 255);
            link newG = (r, g, b) => (byte)Math.Min((int)((r * .349) + (g * .686) + (b * .168)), 255); 
            link newB = (r, g, b) => (byte)Math.Min((int)((r * .272) + (g * .534) + (b * .131)), 255); 
            
            return handleWithFilter(x, y, w, h, listenerContext, newR,newG,newB,false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int HandleThreshold(int x, int y, int w, int h, int thresholdValue, HttpListenerContext listenerContext)
        {           
            
            link threshold = (r, g, b) =>  (byte)(((r + g + b) / 3 >= (255 * thresholdValue / 100)) ? 255 : 0);
            return handleWithFilter(x, y, w, h, listenerContext, threshold, threshold, threshold,true);

        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int handleWithFilter(int x, int y, int w, int h, HttpListenerContext listenerContext, link redLink,link greenLink,link blueLink,bool isBlackAndWhite)
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
                if (isBlackAndWhite)
                    for (int i = 0; i < l; i++)
                        buffer[(i << 2) + 2] = buffer[(i << 2) + 1] = buffer[i << 2] = blueLink(buffer[(i << 2) + 2], buffer[(i << 2) + 1], buffer[i << 2]);
                else                
                    for (int i = 0; i < l; i++)
                    {
                        buffer[i << 2] = blueLink(buffer[(i << 2) + 2], buffer[(i << 2) + 1], buffer[i << 2]);
                        buffer[(i << 2) + 1] = greenLink(buffer[(i << 2) + 2], buffer[(i << 2) + 1], buffer[i << 2]);
                        buffer[(i << 2) + 2] = redLink(buffer[(i << 2) + 2], buffer[(i << 2) + 1], buffer[i << 2]);
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

                //img.Save("newpng.png", System.Drawing.Imaging.ImageFormat.Png);
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
        delegate byte link(byte r,byte g,byte b);

        private int HandleWithFilter(int x, int y, int w, int h, HttpListenerContext listenerContext)
        {
                    
            return 429;
        }
    }
}
