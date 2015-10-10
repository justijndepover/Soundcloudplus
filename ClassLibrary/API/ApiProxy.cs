using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;
using Enough.Storage;
using Newtonsoft.Json;
using HttpClient = System.Net.Http.HttpClient;
using HttpMethod = System.Net.Http.HttpMethod;
using HttpResponseMessage = System.Net.Http.HttpResponseMessage;

namespace ClassLibrary.API
{
    public class ApiProxy
    {
        public string BaseEndpointUrl { get; } = "https://api.soundcloud.com";

        public async Task<ApiResponse> RequestTask(HttpMethod method, string endpoint, object body = null,
            object queryStringPairs = null, object clientHeaders = null)
        {
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
                return await Get(endpoint, queryString, clientHeaders);
            }
            else if (method == HttpMethod.Post)
            {
                return await Post(endpoint, queryString, body, clientHeaders);
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

        private async Task<ApiResponse> Get(string endpoint, string queryString, object queryHeaders = null)
        {
            ApiResponse apiResponse = new ApiResponse();
            string finalEndpoint = (BaseEndpointUrl + endpoint) + (!String.IsNullOrEmpty(queryString) ? "?" + queryString : "");
            using (HttpClient client = new HttpClient())
            {
                if (queryHeaders != null)
                {
                    var properties = queryHeaders.GetType().GetRuntimeProperties();
                    foreach (var property in properties)
                    {
                        object val = property.GetValue(queryHeaders);
                        client.DefaultRequestHeaders.Add(property.Name, val.ToString());
                    }
                }

                HttpResponseMessage response = await client.GetAsync(finalEndpoint);
                apiResponse.StatusCode = response.StatusCode;
                if (response.IsSuccessStatusCode)
                {
                    apiResponse.Data = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());                  
                }
            }
            return apiResponse;
        }
        private async Task<ApiResponse> Post(string endpoint, string queryString, object body, object queryHeaders = null)
        {
            ApiResponse apiResponse = new ApiResponse();
            string finalEndpoint = (BaseEndpointUrl + endpoint) + (!String.IsNullOrEmpty(queryString) ? "?" + queryString : "");
            using (HttpClient client = new HttpClient())
            {
                using (MultipartFormDataContent multipartFormDataContent = new MultipartFormDataContent())
                {
                    if (queryHeaders != null)
                {
                    var properties = queryHeaders.GetType().GetRuntimeProperties();
                    foreach (var property in properties)
                    {
                        object val = property.GetValue(queryHeaders);
                        client.DefaultRequestHeaders.Add(property.Name, val.ToString());
                    }
                }
                if (body != null)
                {
                    var properties = body.GetType().GetRuntimeProperties();
                    foreach (var property in properties)
                    {
                        object value = property.GetValue(body);
                        multipartFormDataContent.Add(new StringContent(value.ToString()), '"' + property.Name + '"');
                    }
                }
                HttpResponseMessage response = await client.PostAsync(finalEndpoint, multipartFormDataContent);
                apiResponse.StatusCode = response.StatusCode;
                if (response.IsSuccessStatusCode)
                {
                    apiResponse.Data = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
                }
                }
            }
            return apiResponse;
        }

        public async Task<bool> Authenticate()
        {
            var callbackUrl = WebAuthenticationBroker.GetCurrentApplicationCallbackUri();
            WebAuthenticationResult webAuthenticationResult =
                await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, new Uri(BaseEndpointUrl + "/connect?client_id=776ca412db7b101b1602c6a67b1a0579&redirect_uri=" + callbackUrl + "&response_type=code_and_token&scope=non-expiring&display=popup&state="), callbackUrl);
            if (webAuthenticationResult.ResponseStatus == WebAuthenticationStatus.Success && webAuthenticationResult.ResponseData != null)
            {
                string response = webAuthenticationResult.ResponseData;
                string code = Regex.Split(response, "code=")[1].Split('&')[0].Split('#')[0];
                string token = Regex.Split(response, "access_token=")[1].Split('&')[0].Split('#')[0];
                await StorageHelper.SaveObjectAsync(code, "code");
                await StorageHelper.SaveObjectAsync(token, "token");
                return true;
            }
            return false;
        }
    }
}