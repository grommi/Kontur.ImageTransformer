using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Kontur.ImageTransformer
{
    class SimpleLogic : IHandlerLogic
    {
        
        public int HandleGrayscale(int x, int y, int w, int h, HttpListenerContext listenerContext)
        {
            int intensity;
            filter grayscale = c => Color.FromArgb(intensity = (c.R+c.G+c.B)/3,intensity,intensity);
            return handleWithFilter(x, y, w, h, listenerContext, grayscale);            
        }
        
        public int HandleSepia(int x, int y, int w, int h, HttpListenerContext listenerContext)
        {
            link newR = c => Math.Min((int)((c.R * .393) + (c.G * .769) + (c.B * .189)),255);
            link newG = c => Math.Min((int)((c.R * .349) + (c.G * .686) + (c.B * .168)), 255);
            link newB = c => Math.Min((int)((c.R * .272) + (c.G * .534) + (c.B * .131)), 255);
            filter sepia = c => Color.FromArgb(newR(c),newG(c), newB(c));
            return handleWithFilter(x, y, w, h, listenerContext, sepia);            
        }

        
        public int HandleThreshold(int x, int y, int w, int h, int thresholdValue, HttpListenerContext listenerContext)
        {

            int newRGB;
            filter threshold = c => Color.FromArgb(newRGB = ((c.R + c.G + c.B) >= 255 * thresholdValue / 100) ? 255 : 0,newRGB,newRGB);
            return handleWithFilter(x, y, w, h, listenerContext, threshold);
            
        }
        private int handleWithFilter(int x, int y, int w, int h, HttpListenerContext listenerContext, filter filtr)
        {
            Bitmap bm;
            try
            {
                 bm = new Bitmap(listenerContext.Request.InputStream);
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
            int xTo = Math.Min(leftPoint + absW, bm.Width);
            int yTo = Math.Min(upperPoint + absH, bm.Height);
            Bitmap image = new Bitmap(xTo - xFrom, yTo - yFrom);
            if ((xTo > xFrom)&& (yTo > yFrom)){
                for (int i = xFrom; i < xTo; i++)
                {
                    for (int j = yFrom; j < yTo; j++)
                    {
                        Color oldColor = bm.GetPixel(i, j);
                        Color newColor = filtr(oldColor);
                        image.SetPixel(i - xFrom, j - yFrom, newColor);
                    }
                }
                image.Save(listenerContext.Response.OutputStream, System.Drawing.Imaging.ImageFormat.Png);
                return 200;
            }
            else{
                return 204;
            }
        }
        
        delegate Color filter(Color c);
        delegate int link(Color c);

    }
}
