using System.Xml.Linq;
using Windows.Data.Xml.Dom;
using ClassLibrary.Models;

namespace TilesAndNotifications.Services
{
    public class TileService
    {

        public static XmlDocument CreateTiles(Track t)
        {
            XDocument xDoc = new XDocument(
                new XElement("tile", new XAttribute("version", 3),
                    new XElement("visual",
                        // Small Tile
                        new XElement("binding",
                            new XAttribute("branding", "name"),
                            new XAttribute("displayName", "Soundcloud Plus"),
                            new XAttribute("template", "TileSmall"),
                            new XElement("group",
                                new XElement("subgroup",
                                new XElement("text", t.Title,
                                    new XAttribute("hint-style", "captionsubtle"),
                                    new XAttribute("hint-wrap", true),
                                    new XAttribute("hint-maxLines", 3))
                                )
                            )
                        ),
                        // Medium Tile
                        new XElement("binding",
                            new XAttribute("branding", "name"),
                            new XAttribute("displayName", "Soundcloud Plus"),
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
                        // Medium Tile
                        new XElement("binding",
                            new XAttribute("branding", "name"),
                            new XAttribute("displayName", "Soundcloud Plus"),
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
                    //// Wide Tile
                    //new XElement("binding",
                    //    new XAttribute("branding", "name"),
                    //    new XAttribute("displayName", "Soundcloud Plus"),
                    //    new XAttribute("template", "TileWide"),
                    //    new XElement("group",
                    //        new XElement("subgroup",
                    //            new XElement("text", t.Title,
                    //                new XAttribute("hint- style", "caption"))
                    //        //new XElement("text", t.Title,
                    //        //    new XAttribute("hint- style", "captionsubtle"),
                    //        //    new XAttribute("hint-wrap", true),
                    //        //    new XAttribute("hint-maxLines", 3))
                    //        //new XElement("text", primaryTile.message2, 
                    //        //    new XAttribute("hint- style", "captionsubtle"),
                    //        //    new XAttribute("hint-wrap", true), 
                    //        //    new XAttribute("hint-maxLines", 3))
                    //        ),
                    //        new XElement("subgroup",
                    //            new XAttribute("hint-weight", 15),
                    //        new XElement("image",
                    //            new XAttribute("placement", "inline"),
                    //            new XAttribute("src", "Assets/StoreLogo.png"))
                    //        )
                    //    )
                    //)
                    )
                )
            );
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xDoc.ToString());
            return xmlDoc;
        }
    }
}
