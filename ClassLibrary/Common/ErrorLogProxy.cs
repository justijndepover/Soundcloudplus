using System;
using System.Diagnostics;
using System.Net.Http;
using Windows.UI.Notifications;

namespace ClassLibrary.Common
{
    public class ErrorLogProxy
    {
        public static async void LogError(string message)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // ReSharper disable once AccessToDisposedClosure
                    HttpResponseMessage response = await client.GetAsync("http://arnvanhoutte.be/api/soundcloud?error=" + message);
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

        public static void NotifyErrorInDebug(string message)
        {
            if ((bool)ApplicationSettingsHelper.ReadLocalSettingsValue<bool>("DebugModeEnabled"))
            {
                UpdateToastMessage(message);
            }
        }
        public static void NotifyError(string message)
        {
                UpdateToastMessage(message);
        }
        private static void UpdateToastMessage(string message)
        {
            var template = ToastTemplateType.ToastImageAndText02;
            var xml = ToastNotificationManager.GetTemplateContent(template);
            xml.DocumentElement.SetAttribute("launch", "Args");

            var node = xml.CreateTextNode(message);
            var elements = xml.GetElementsByTagName("text");
            elements[1].AppendChild(node);

            var toast = new ToastNotification(xml);
            var notifier = ToastNotificationManager.CreateToastNotifier("App");
            notifier.Show(toast);
        }
    }
}
