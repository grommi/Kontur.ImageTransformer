using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Kontur.ImageTransformer
{
    interface IHandlerLogic
    {

        int HandleGrayscale(int x, int y, int w,int h, HttpListenerContext listenerContext);
        int HandleSepia(int x, int y, int w, int h, HttpListenerContext listenerContext);
        int HandleThreshold(int x, int y, int w, int h, int threshold, HttpListenerContext listenerContext);
    }
}
