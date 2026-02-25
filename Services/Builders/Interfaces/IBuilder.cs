using MAPSAI.Core.Models;
using MAPSAI.Models;
using System.Collections.ObjectModel;

namespace MAPSAI.Services.Builders.Interfaces
{
    public interface IBuilder
    {
        public const string Connector = " --> ";
        public const string newLine = "<br>";

        public string Build(ObservableCollection<UserStory> stories);
    }
}