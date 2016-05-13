using System;
using System.Xml.Linq;
using Windows.Data.Xml.Dom;
using Windows.UI.StartScreen;
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
