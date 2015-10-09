using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Devices.Bluetooth.Advertisement;
using Newtonsoft.Json;

namespace ClassLibrary.API
{
    public class ApiProxy
    {
        public string BaseEndpointUrl { get; } = "https://api.soundcloud.com";

        public async Task<ApiResponse> RequestTask(HttpMethod method, string endpoint, object body = null,
            object queryStringPairs = null)
        {
            string jsonBody = null;
            if (method != HttpMethod.Get && body != null)
            {
                jsonBody = Newtonsoft.Json.JsonConvert.SerializeObject(body);
            }

            Dictionary<String, String> dictQueryStringPairs = null;
            if (queryStringPairs != null && queryStringPairs.GetType() == typeof (Dictionary<String, String>))
            {
                dictQueryStringPairs = (Dictionary<String, String>) queryStringPairs;
                queryStringPairs = null;
            }

            //prepare querystrings 
            string queryString = null;
            if (queryStringPairs != null)
            {
                var properties = queryStringPairs.GetType().GetRuntimeProperties();

                List<string> keyValuePairs = (from property in properties
                    let val = property.GetValue(queryStringPairs)
                    select String.Format("{0}={1}", property.Name, val)).ToList();
                queryString = String.Join("&", keyValuePairs);
            }
            else if (dictQueryStringPairs != null)
            {
                List<String> keyValuePairs =
                    (from pair in dictQueryStringPairs select String.Format("{0}={1}", pair.Key, pair.Value)).ToList();
                queryString = String.Join("&", keyValuePairs);
            }
            if (method == HttpMethod.Get)
            {
                return await Get(endpoint, queryString);
            }
            else if (method == HttpMethod.Post)
            {
                //return await Post(endpoint, queryString, jsonBody);
            }
            else if (method == HttpMethod.Put)
            {
                //return await Put(endpoint, queryString, jsonBody);
            }
            else if (method == HttpMethod.Delete)
            {
                //return await Delete(endpoint, queryString);
            }
            return null;
        }

        private async Task<ApiResponse> Get(string endpoint, string queryString)
        {
            ApiResponse apiResponse = new ApiResponse();
            string finalEndpoint = (BaseEndpointUrl + endpoint) + (!String.IsNullOrEmpty(queryString) ? "?" + queryString : "");
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("client_id", "bb45a30915dd5b2e04cf203b0f257c09");
                client.DefaultRequestHeaders.Add("client_secret", "85a0ba75066c4bd5fb14142b16c21d8a");

                HttpResponseMessage response = await client.GetAsync(finalEndpoint);
                apiResponse.StatusCode = response.StatusCode;
                if (response.IsSuccessStatusCode)
                {
                    apiResponse.Data = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());                  
                }
            }
            return apiResponse;
        }
    }
}