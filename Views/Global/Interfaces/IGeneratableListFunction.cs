using System.Collections;
using MAPSAI.Services.AI;
using MAPSAI.Services.Builders;

namespace MAPSAI.Views.Global.Interfaces
{
    public interface IGeneratableListFunction
    {
        Task<IEnumerable> GenerateAsync(ListEntryService listEntryService);
    }
}
