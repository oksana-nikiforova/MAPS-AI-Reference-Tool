using System.Collections;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using MAPSAI.Core.Models;
using MAPSAI.Models.AI;
using MAPSAI.Services.AI;
using MAPSAI.Services.Builders;
using MAPSAI.Views.Global.Interfaces;

namespace MAPSAI.Models
{
    public class ListItem<T> : IGeneratableList<T> where T : IListElement, new()
    {
        public ListItem() { }
        public ListItem(string name, string promptkey, string instruction, int maxTokens)
        {
            ListName = name;
            PromptKey = promptkey;
            Instruction = instruction;
            MaxTokens = maxTokens;
        }
        public string ListName { get; set; } = string.Empty;

        public int MaxTokens { get; set; } = 0;

        public string Instruction { get; set; } = string.Empty;

        public ObservableCollection<T> Collection { get; set; } = [];

        // Persisted
        public string PromptKey { get; set; } = "";

        public async Task<IEnumerable> GenerateAsync(ListEntryService listEntryService)
        {
            foreach (var item in Collection.ToList())
            {
                if (!item.IsUsed)
                {
                    Collection.Remove(item);
                }
            }

            var response = await listEntryService.SendPromptRequestAsync(PromptKey, ListName, Instruction);

            if (response is null || string.IsNullOrWhiteSpace(response))
            {
                return Collection;
            }

            var lines = response.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                                .Select(l => l.Trim())
                                .ToList();

            foreach (var line in lines)
            {
                var cleaned = line;
                if (!string.IsNullOrEmpty(cleaned) && cleaned[0] == '-')
                    cleaned = cleaned[1..];
                var newItem = new T { Text = cleaned };
                Collection.Add(newItem);
            }


            return Collection;
        }
    }

    public class StructuredListResponse
    {
        public List<string>? items { get; set; }
    }
}
