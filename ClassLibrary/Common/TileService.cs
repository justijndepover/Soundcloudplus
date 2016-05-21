using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Net.Http;
using System.Xml.Linq;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.UI.StartScreen;
using Windows.UI.Xaml.Media.Imaging;
using ClassLibrary.Models;

namespace ClassLibrary.Common
{
    public class TileService
    {
        public static XmlDocument CreateTiles(Track t)
        {
            XDocument xDoc = new XDocument(
                new XElement("tile", new XAttribute("version", 3),
                    new XElement("visual",
                        // Medium Tile
                        new XElement("binding",
                            new XAttribute("branding", "name"),
                            new XAttribute("displayName", "10Sound"),
                            new XAttribute("template", "TileMedium"),
                            new XElement("group",
                                new XElement("subgroup",
                                    new XElement("text", t.User.Username,
                                        new XAttribute("hint-style", "caption"),
                                        new XAttribute("hint-wrap", true),
                                        new XAttribute("hint-maxLines", 3)
                                    ),
                                    new XElement("text", t.Title,
                                        new XAttribute("hint-style", "captionsubtle"),
                                        new XAttribute("hint-wrap", true),
                                        new XAttribute("hint-maxLines", 3)
                                    )
                                )
                            )
                        ),
                        // Wide Tile
                        new XElement("binding",
                            new XAttribute("branding", "name"),
                            new XAttribute("displayName", "10Sound"),
                            new XAttribute("template", "TileWide"),
                            new XElement("group",
                                new XElement("subgroup",
                                    new XElement("text", t.User.Username,
                                        new XAttribute("hint-style", "caption"),
                                        new XAttribute("hint-wrap", true),
                                        new XAttribute("hint-maxLines", 3)
                                    ),
                                    new XElement("text", t.Title,
                                        new XAttribute("hint-style", "captionsubtle"),
                                        new XAttribute("hint-wrap", true),
                                        new XAttribute("hint-maxLines", 3)
                                    )
                                )
                            )
                        )
                    )
                )
            );
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xDoc.ToString());
            return xmlDoc;
        }
        public static async void CreateTileLinkedToPage(string title, string name, string[] arguments, string uri = "ms-appx:///Assets/10SoundBackground.png")
        {
            if (!uri.StartsWith("ms-appx:///"))
            {
                try
                {
                    var rootFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("10Sound\\Tilepics", CreationCollisionOption.OpenIfExists);

                    var coverpic_file = await rootFolder.CreateFileAsync(name, CreationCollisionOption.FailIfExists);
                    try
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            var response = await client.GetAsync(uri);
                            if (response.IsSuccessStatusCode)
                            {
                                Stream resStream = await response.Content.ReadAsStreamAsync();
                                using (var stream = await coverpic_file.OpenAsync(FileAccessMode.ReadWrite))
                                {
                                    await resStream.CopyToAsync(stream.AsStreamForWrite());
                                }
                                uri = "ms-appx:///Local/Tilepics" + name;
                            }
                        }
                    }
                    catch(Exception ex) //any exceptions happend while saving the picture
                    {
                        ErrorLogProxy.LogError(ex.ToString());
                        ErrorLogProxy.NotifyErrorInDebug(ex.ToString());
                    }
                }
                catch (Exception ex) //any exceptions happend while saving the picture
                {
                    ErrorLogProxy.LogError(ex.ToString());
                    ErrorLogProxy.NotifyErrorInDebug(ex.ToString());
                }
            }
            string argument = string.Join(",", arguments);
            SecondaryTile tile = new SecondaryTile(Guid.NewGuid().ToString(), name, title,
                new Uri(uri), TileSize.Default) {Arguments = argument };

            tile.VisualElements.ShowNameOnSquare150x150Logo = true;
            tile.VisualElements.ShowNameOnSquare310x310Logo = true;
            tile.VisualElements.ShowNameOnWide310x150Logo = true;

            await tile.RequestCreateAsync();
        }
    }
}
