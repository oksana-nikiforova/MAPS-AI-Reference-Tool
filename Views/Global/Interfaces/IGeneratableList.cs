using MAPSAI.Services.AI;
using MAPSAI.Services.Builders;
using System.Collections;
using System.Collections.ObjectModel;

namespace MAPSAI.Views.Global.Interfaces
{
    public interface IGeneratableList<T>: IGeneratableListFunction
    {
        public ObservableCollection<T> Collection { get; set; }
        string PromptKey { get; set; }
        string ListName { get; set; }
        string Instruction { get; set; }
        new Task<IEnumerable> GenerateAsync(ListEntryService listEntryService);
    }
}
