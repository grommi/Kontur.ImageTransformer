﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Net;

namespace Kontur.ImageTransformer
{
    static class Handler
    {
        /*public static  Handler instance ;
        private Handler()
        {
            NumberOfHandledRequests = 0;
            handlerLogic = new MediumLogic();
        }
        public static Handler GetInstance()
        {
            if (instance == null)
            {
                instance = new Handler();
            }
            return instance;
        }*/
        private static readonly IHandlerLogic handlerLogic = new HardLogic();
        public static volatile int NumberOfHandledRequests = 0;
        private const int maxNumberOfHandledRequests = 16;
        public static void Handle(HttpListenerContext listenerContext)
        {



            var request = listenerContext.Request;
            bool isCorrect = (request.HttpMethod == "POST") && (request.RawUrl.StartsWith("/process/"));
            string[] splitedUrl = request.RawUrl.Split('/');
            string filter = splitedUrl[2];
            int[] xYWH = splitedUrl[3].Split(',').Select(snum => int.Parse(snum)).ToArray();
            int x, y, w, h;
            x = xYWH[0]; y = xYWH[1]; w = xYWH[2]; h = xYWH[3];
            int code;
            switch (filter)
            {
                case "grayscale":
                    code = handlerLogic.HandleGrayscale(x, y, w, h, listenerContext);
                    returnWithCode(listenerContext, code);
                    break;
                case "sepia":
                    code = handlerLogic.HandleSepia(x, y, w, h, listenerContext);
                    returnWithCode(listenerContext, code);
                    break;
                default:
                    if (filter.StartsWith("threshold"))
                    {
                        try
                        {
                            int threshold = int.Parse(filter.Substring(10).Split(')')[0]);
                            code = handlerLogic.HandleThreshold(x, y, w, h, threshold, listenerContext);
                            returnWithCode(listenerContext, code);
                        }
                        catch
                        {
                            returnWithCode(listenerContext, 400);
                        }

                    }
                    else returnWithCode(listenerContext, 400);
                    break;
            }
            NumberOfHandledRequests--;

        }
        private static void returnWithCode(HttpListenerContext listenerContext, int code)
        {
            listenerContext.Response.StatusCode = code;
            
            //todo keep-alive?            
            listenerContext.Response.KeepAlive = listenerContext.Request.KeepAlive;
            listenerContext.Response.OutputStream.Close();
            listenerContext.Response.Close();
            
        }

    }
}
