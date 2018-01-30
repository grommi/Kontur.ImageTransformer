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
    class HardLogic : IHandlerLogic
    {
        public int HandleGrayscale(int x, int y, int w, int h, HttpListenerContext listenerContext)
        {
            throw new NotImplementedException();
        }

        public int HandleSepia(int x, int y, int w, int h, HttpListenerContext listenerContext)
        {
            throw new NotImplementedException();
        }

        public int HandleThreshold(int x, int y, int w, int h, int threshold, HttpListenerContext listenerContext)
        {
            throw new NotImplementedException();
        }
        /*private int handleWithFilter(int x, int y, int w, int h, HttpListenerContext listenerContext, link redLink, link greenLink, link blueLink, bool isBlackAndWhite)
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
                    new Rectangle(0, 0, bmp.Width, bmp.Height),
                    ImageLockMode.ReadOnly,
                     System.Drawing.Imaging.PixelFormat.Format32bppArgb
                );

                var buffer = new byte[bd.Stride * bd.Height];
                Marshal.Copy(bd.Scan0, buffer, 0, buffer.Length);
                byte[] bufWithFilter = new byte[4 * (xTo - xFrom) * (yTo - yFrom)];
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
                            counter2 = (i - yFrom) * ((xTo - xFrom) << 2) + ((j - xFrom) << 2);
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
                var bdi = img.LockBits(
                    new Rectangle(0, 0, img.Width, img.Height),
                    ImageLockMode.WriteOnly,
                     System.Drawing.Imaging.PixelFormat.Format32bppArgb
                );
                Marshal.Copy(bufWithFilter, 0, bdi.Scan0, bufWithFilter.Length);
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
        }*/
    }
}
