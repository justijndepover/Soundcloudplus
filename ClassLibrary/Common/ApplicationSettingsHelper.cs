using System;
using System.Diagnostics;
using Windows.Storage;
using Newtonsoft.Json;

namespace ClassLibrary.Common
{
    public class ApplicationSettingsHelper
    {
        private static readonly ApplicationDataContainer LocalSettings = ApplicationData.Current.LocalSettings;
        private static readonly ApplicationDataContainer RoamingSettings = ApplicationData.Current.RoamingSettings;
        /// <summary>
        /// Function to read a setting value and clear it after reading it
        /// </summary>
        public static object ReadResetSettingsValue(string key)
        {
            Debug.WriteLine("Key: " + key);
            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
            {
                return null;
            }
            else
            {
                var value = ApplicationData.Current.LocalSettings.Values[key];
                ApplicationData.Current.LocalSettings.Values.Remove(key);
                return value;
            }
        }
        public static object ReadResetSettingsValue<T>(string key)
        {
            if (!LocalSettings.Values.ContainsKey(key))
            {
                return default(T);
            }
            string value = LocalSettings.Values[key] as string;
            LocalSettings.Values.Remove(key);
            try
            {
                return JsonConvert.DeserializeObject<T>(value);
            }
            catch (Exception)
            {
                return value;
            }
        }
        public static object ReadRoamingSettingsValue<T>(string key)
        {
            if (!RoamingSettings.Values.ContainsKey(key))
            {
                return default(T);
            }
            string value = (string) RoamingSettings.Values[key];
            try
            {
                return JsonConvert.DeserializeObject<T>(value);
            }
            catch (Exception)
            {
                return value;
            }
        }
        public static object ReadLocalSettingsValue<T>(string key)
        {
            if (!LocalSettings.Values.ContainsKey(key))
            {
                return default(T);
            }
            string value = (string)LocalSettings.Values[key];
            try
            {
                return JsonConvert.DeserializeObject<T>(value);
            }
            catch (Exception)
            {
                return value;
            }
        }

        /// <summary>
        /// Save a key value pair in settings. Create if it doesn't exist
        /// </summary>
        public static void SaveRoamingSettingsValue(string key, object value)
        {
            value = JsonConvert.SerializeObject(value);
            if (!RoamingSettings.Values.ContainsKey(key))
            {
                RoamingSettings.Values.Add(key, value);
            }
            else
            {
                RoamingSettings.Values[key] = value;
            }
        }
        public static void SaveLocalSettingsValue(string key, object value)
        {
            value = JsonConvert.SerializeObject(value);
            if (!LocalSettings.Values.ContainsKey(key))
            {
                LocalSettings.Values.Add(key, value);
            }
            else
            {
                LocalSettings.Values[key] = value;
            }
        }
        public static void SaveSettingsValue(string key, object value)
        {
            Debug.WriteLine(key + ":" + (value == null ? "null" : value.ToString()));

            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
            {
                ApplicationData.Current.LocalSettings.Values.Add(key, value);
            }
            else
            {
                ApplicationData.Current.LocalSettings.Values[key] = value;
            }
        }
        public static void DeleteLocalSettingsValue(string key)
        {
            if (LocalSettings.Values.ContainsKey(key))
            {
                LocalSettings.Values.Remove(key);
            }
        }
        public static void DeleteRoamingSettingsValue(string key)
        {
            if (RoamingSettings.Values.ContainsKey(key))
            {
                RoamingSettings.Values.Remove(key);
            }
        }
    }
}
