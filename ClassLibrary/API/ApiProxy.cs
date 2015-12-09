using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Security.Authentication.Web;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Enough.Storage;
using Newtonsoft.Json;

namespace ClassLibrary.API
{
    public class ApiProxy
    {
        public static string NewBasepoint = "https://api-v2.soundcloud.com";
        public static string OldBasepoint = "https://api.soundcloud.com";

        public async Task<ApiResponse> RequestTask(HttpMethod method, string endpoint, object body = null,
            object queryStringPairs = null, object clientHeaders = null, bool useNewBasepoint = true)
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

                var keyValuePairs = (from property in properties
                    let val = property.GetValue(queryStringPairs)
                    select String.Format("{0}={1}", property.Name, val)).ToList();
                queryString = String.Join("&", keyValuePairs);
            }
            else if (dictQueryStringPairs != null)
            {
                var keyValuePairs =
                    (from pair in dictQueryStringPairs select String.Format("{0}={1}", pair.Key, pair.Value)).ToList();
                queryString = String.Join("&", keyValuePairs);
            }
            if (method == HttpMethod.Get)
            {
                return await Get(endpoint, queryString, clientHeaders, useNewBasepoint);
            }
            if (method == HttpMethod.Post)
            {
                return await Post(endpoint, queryString, body, clientHeaders);
            }
            if (method == HttpMethod.Put)
            {
                //return await Put(endpoint, queryString, jsonBody);
            }
            else if (method == HttpMethod.Delete)
            {
                //return await Delete(endpoint, queryString);
            }
            return null;
        }

        private async Task<ApiResponse> Get(string endpoint, string queryString, object queryHeaders = null, bool useNewBasepoint = true)
        {
            var apiResponse = new ApiResponse();
            string finalEndpoint;
            if (useNewBasepoint)
            {
                finalEndpoint = (NewBasepoint + endpoint) + (!String.IsNullOrEmpty(queryString) ? "?" + queryString : "");
            }
            else
            {
                finalEndpoint = (OldBasepoint + endpoint) + (!String.IsNullOrEmpty(queryString) ? "?" + queryString : "");
            }
            using (var client = new HttpClient())
            {
                if (queryHeaders != null)
                {
                    var properties = queryHeaders.GetType().GetRuntimeProperties();
                    foreach (var property in properties)
                    {
                        var val = property.GetValue(queryHeaders);
                        client.DefaultRequestHeaders.Add(property.Name, val.ToString());
                    }
                }
                try
                {
                    HttpResponseMessage response = await client.GetAsync(finalEndpoint);
                    if (response.IsSuccessStatusCode)
                    {
                        apiResponse.Succes = true;
                        apiResponse.Data = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());                  
                    }
                    else
                    {
                        apiResponse.Succes = false;
                        await StorageHelper.DeleteObjectAsync<string>("code");
                        await StorageHelper.DeleteObjectAsync<string>("token");
                    }
                }
                catch (Exception)
                {
                    MessageDialog md = new MessageDialog("Please check your internet connection", "Sorry, we encountered an error");
                    md.Commands.Add(new UICommand("Close", Action));
                    await md.ShowAsync();
                }
            }
            return apiResponse;
        }

        private void Action(IUICommand command)
        {
            Application.Current.Exit();
        }

        private async Task<ApiResponse> Post(string endpoint, string queryString, object body, object queryHeaders = null)
        {
            var apiResponse = new ApiResponse();
            var finalEndpoint = (NewBasepoint + endpoint) + (!String.IsNullOrEmpty(queryString) ? "?" + queryString : "");
            using (var client = new HttpClient())
            {
                using (var multipartFormDataContent = new MultipartFormDataContent())
                {
                    if (queryHeaders != null)
                {
                    var properties = queryHeaders.GetType().GetRuntimeProperties();
                    foreach (var property in properties)
                    {
                        var val = property.GetValue(queryHeaders);
                        client.DefaultRequestHeaders.Add(property.Name, Uri.EscapeDataString(val.ToString()));
                    }
                }
                if (body != null)
                {
                    var properties = body.GetType().GetRuntimeProperties();
                    foreach (var property in properties)
                    {
                        var value = property.GetValue(body);
                        multipartFormDataContent.Add(new StringContent(value.ToString()), '"' + property.Name + '"');
                    }
                }
                var response = await client.PostAsync(finalEndpoint, multipartFormDataContent);
                if (response.IsSuccessStatusCode)
                {
                    apiResponse.Succes = true;
                    apiResponse.Data = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
                }
                else
                {
                    apiResponse.Succes = false;
                }
                }
            }
            return apiResponse;
        }

        public async Task<bool> Authenticate()
        {
            var callbackUrl = new Uri("https://soundcloud.com/soundcloud-callback.html"); //WebAuthenticationBroker.GetCurrentApplicationCallbackUri();
            var requestUrl = new UriBuilder("https://soundcloud.com/connect")
            {
                Query =
                    "client_id=776ca412db7b101b1602c6a67b1a0579&response_type=code_and_token&scope=non-expiring&display=popup&redirect_uri=" +
                    callbackUrl
            };
            try
            {
            var webAuthenticationResult = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, requestUrl.Uri, callbackUrl);
            if (webAuthenticationResult.ResponseStatus == WebAuthenticationStatus.Success && !String.IsNullOrWhiteSpace(webAuthenticationResult.ResponseData))
            {
                var response = webAuthenticationResult.ResponseData;
                var code = Regex.Split(response, "code=")[1].Split('&')[0].Split('#')[0];
                var token = Regex.Split(response, "access_token=")[1].Split('&')[0].Split('#')[0];
                await StorageHelper.SaveObjectAsync(code, "code");
                await StorageHelper.SaveObjectAsync(token, "token");
                return true;
            }
            return false;

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }
    }
}