﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ClassLibrary.Common
{
    public class ErrorLogProxy
    {
        public ErrorLogProxy(string message)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // ReSharper disable once AccessToDisposedClosure
                    HttpResponseMessage response = AsyncHelper.RunSync(() => client.GetAsync("http://arnvanhoutte.be/api/soundcloud?error=" + message));
                    if (response.IsSuccessStatusCode)
                    {
                        Debug.WriteLine("good job");
                    }
                    else
                    {
                        Debug.WriteLine("bad job");
                    }
                }
            }
            catch (Exception)
            {
                Debug.WriteLine("Yer fucked kiddo");
            }
        }

        public ErrorLogProxy()
        {
            
        }
    }
}
