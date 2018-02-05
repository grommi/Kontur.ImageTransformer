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
using System.IO;

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
            link newR = (r ,g, b) => (byte)Math.Min(((r * .393) + (g * .769) + (b * .189)), 255);
            link newG = (r, g, b) => (byte)Math.Min(((r * .349) + (g * .686) + (b * .168)), 255); 
            link newB = (r, g, b) => (byte)Math.Min(((r * .272) + (g * .534) + (b * .131)), 255); 
            
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
                //bmp = (Bitmap)Image.FromStream(listenerContext.Request.InputStream);
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
           
            if ((xTo > xFrom) && (yTo > yFrom))
            {
                Bitmap img = new Bitmap(xTo - xFrom, yTo - yFrom, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                var bd = bmp.LockBits(
                    new Rectangle(0, 0,bmp.Width,bmp.Height),
                    ImageLockMode.ReadOnly,
                     System.Drawing.Imaging.PixelFormat.Format32bppArgb
                );
                var bdi = img.LockBits(
                    new Rectangle(0, 0, img.Width, img.Height),
                    ImageLockMode.ReadWrite,
                     System.Drawing.Imaging.PixelFormat.Format32bppArgb
                );
                var buffer = new byte[bd.Stride * bd.Height];                
                Marshal.Copy(bd.Scan0, buffer, 0, buffer.Length);
                byte[] bufWithFilter = new byte[bdi.Stride * bdi.Height];
                Marshal.Copy(bdi.Scan0, bufWithFilter, 0, bufWithFilter.Length);
                int l = buffer.Length >> 2;
                int counter;
                int counter2;

                if (isBlackAndWhite)
                {
                    
                    for (int i = yFrom; i < yTo; i++)
                    {
                        for (int j = xFrom; j < xTo; j++)
                        {

                            counter = i * bd.Stride + (j << 2);
                            counter2 = (i - yFrom) * ((xTo - xFrom)<<2) + ((j - xFrom) << 2);
                            bufWithFilter[counter2 + 2] = bufWithFilter[counter2 + 1] = bufWithFilter[counter2] = blueLink(buffer[counter + 2], buffer[counter + 1], buffer[counter]);
                            bufWithFilter[counter2 + 3] = buffer[counter + 3];
                        }
                        
                    }
                }
                else
                {

                    for (int i = yFrom; i < yTo; i++)
                    {
                        for (int j = xFrom; j < xTo; j++)
                        {

                            counter = i * bd.Stride + (j << 2);
                            counter2 = (i - yFrom) * ((xTo - xFrom) << 2) + ((j - xFrom) << 2);
                            bufWithFilter[counter2 + 2] = redLink(buffer[counter + 2], buffer[counter + 1], buffer[counter]);
                            bufWithFilter[counter2 + 1] = greenLink(buffer[counter + 2], buffer[counter + 1], buffer[counter]);
                            bufWithFilter[counter2] = blueLink(buffer[counter + 2], buffer[counter + 1], buffer[counter]);
                            bufWithFilter[counter2 + 3] = buffer[counter + 3];
                        }

                    }
                }
                
                Marshal.Copy(bufWithFilter, 0, bdi.Scan0, bufWithFilter.Length);
                     
                img.UnlockBits(bdi);
                bmp.UnlockBits(bd);
                //img.Save("newpng.png", System.Drawing.Imaging.ImageFormat.Png);
                //MemoryStream inMemory = new MemoryStream();
                img.Save(listenerContext.Response.OutputStream, System.Drawing.Imaging.ImageFormat.Png);
                //byte[] buffr = inMemory.GetBuffer();
                //inMemory.Close();
                //listenerContext.Response.OutputStream.Write(buffr, 0, buffr.Length);


                //Console.WriteLine("200 OK");
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
