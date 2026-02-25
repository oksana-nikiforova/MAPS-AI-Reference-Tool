using MAPSAI.Services.AI;
using MAPSAI.Services.Builders;

namespace MAPSAI.Views.Global.Interfaces
{
    public interface IGeneratableEntry
    {
        string Name { get; set; }
        string Instruction { get; set; }
        string PromptKey { get; set; }

        Task<string> GenerateAsync(ListEntryService listEntryService);
    }
}

