using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace ClassLibrary.Common
{
    public class AppSettings
    {
        public async Task<string> ReadTextFileAsync(string path)
        {
            var folder = ApplicationData.Current.LocalFolder;
            var file = await folder.GetFileAsync(path);
            return await FileIO.ReadTextAsync(file);
        }

        public async void WriteTotextFileAsync(string fileName, string contents)
        {
            var folder = ApplicationData.Current.LocalFolder;
            var file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, contents);
        }

        public void SaveSettings(string key, string contents)
        {
            ApplicationData.Current.LocalSettings.Values[key] = contents;
        }

        public string LoadSettings(string key)
        {
            var settings = ApplicationData.Current.LocalSettings;
            return settings.Values[key].ToString();
        }
        public void SaveSettingsInContainer(string user, string key, string contents)
        {
            var localSetting = ApplicationData.Current.LocalSettings;

            localSetting.CreateContainer(user, ApplicationDataCreateDisposition.Always);

            if (localSetting.Containers.ContainsKey(user))
            {
                localSetting.Containers[user].Values[key] = contents;
            }
        }
    }

}
