using System;
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
        public ErrorLogProxy(Exception ex)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string exception = JsonConvert.SerializeObject(ex);

                    // ReSharper disable once AccessToDisposedClosure
                    HttpResponseMessage response = AsyncHelper.RunSync(() => client.PostAsync("http://arnvanhoutte.be/api/soundcloud?exception=" + exception, new StringContent("")));
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
