using System.Diagnostics;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using MAPSAI.Models;
using System.Collections.ObjectModel;
using System.Net;
using System.Globalization;
using MAPSAI.Services.Builders;
using MAPSAI.Core.Models;
using MAPSAI.Services.Files.Models;

namespace MAPSAI.Services.Files
{

    public class XmlService
    {
        public XmlService()
        {

        }

        public XmlResponse ParseXml(string filePath)
        {
            try
            {
                XDocument doc = XDocument.Load(filePath);

                IEnumerable<XElement> elements = from c in doc.Descendants() select c;

                var UserStories = new ObservableCollection<UserStory>();

                foreach (XElement element in elements)
                {
                    IEnumerable<XAttribute> attributes = element.Attributes();

                    var styleAttr = element.Attribute("style");
                    var valueAttr = element.Attribute("value");
                    var idAttr = element.Attribute("id");

                    if (styleAttr is null || valueAttr is null || idAttr is null) continue;

                    if (styleAttr.Value.Contains("rounded=1"))
                    {
                        string cleanText = Regex.Replace(valueAttr.Value, "<[^>]*>", "").Trim();
                        cleanText = WebUtility.HtmlDecode(cleanText);

                        var split = Regex.Split(cleanText, "-{2,}");

                        if (split.Length < 2 || split is null || split[0] is null || split[1] is null)
                        {
                            if (valueAttr.Value.Contains("[external]"))
                            {
                                continue;
                            }
                            continue;
                        }

                        var story = split[0].Trim();
                        var user = split[1].Trim();

                        if (user.Contains("&"))
                        {
                            var users = user.Split('&', StringSplitOptions.RemoveEmptyEntries);

                            foreach (var singleUser in users)
                            {
                                UserStories.Add(new UserStory()
                                {
                                    ID = idAttr.Value,
                                    User = CultureInfo.CurrentCulture.TextInfo
                                        .ToTitleCase(singleUser.Trim().ToLower()),
                                    Story = story,
                                });
                            }
                        }
                        else
                        {
                            UserStories.Add(new UserStory()
                            {
                                ID = idAttr.Value,
                                User = CultureInfo.CurrentCulture.TextInfo
                                    .ToTitleCase(user.Trim().ToLower()),
                                Story = story,
                            });
                        }
                    }
                }

                foreach (var story in UserStories)
                {
                    foreach (XElement element in elements)
                    {
                        IEnumerable<XAttribute> attributes = element.Attributes();

                        var idAttr = element.Attribute("id");
                        var sourceAttr = element.Attribute("source");
                        var targetAttr = element.Attribute("target");

                        if (sourceAttr is null || targetAttr is null || idAttr is null) 
                            continue;

                        if (element.Attribute("edge")?.Value != "1")
                            continue;

                        if (story.ID == sourceAttr.Value)
                        {
                            var connection = new Connection(idAttr.Value, sourceAttr.Value, targetAttr.Value);

                            connection.Text = element.Attribute("value")?.Value;

                            story.Connections.Add(connection);
                        }

                    }

                    foreach (var connection in story.Connections)
                    {
                        foreach (var element in elements)
                        {
                            var styleAttr = element.Attribute("style");
                            var parentAttr = element.Attribute("parent");
                            var valueAttr = element.Attribute("value");

                            if (styleAttr is null || parentAttr is null || valueAttr is null) continue;

                            if (styleAttr.Value.Contains("edgeLabel;"))
                            {
                                if (connection.ID == parentAttr.Value)
                                {
                                    connection.Text = valueAttr.Value;
                                }
                            }
                        }
                    }
                }

                return new(true, null, UserStories);
            }
            catch (Exception ex) 
            {
                Debug.WriteLine(ex);
                return new(true, "Error occured while parsing xml!", null);
            }
        }
    }
}
