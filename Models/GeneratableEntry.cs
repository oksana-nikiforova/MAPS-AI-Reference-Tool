using MAPSAI.Services.AI;
using MAPSAI.Views.Global.Interfaces;
using System.ComponentModel;

namespace MAPSAI.Models
{
    public class GeneratableEntry : INotifyPropertyChanged, IGeneratableEntry
    {
        public GeneratableEntry(string name, string instruction, string promptkey, string metaPromptKey, int maxTokens)
        { 
            Name = name;
            Instruction = instruction;
            PromptKey = promptkey;
            MetaPromptKey = metaPromptKey;
            MaxTokens = maxTokens;
        }

        public int MaxTokens { get; set; } = 0;

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        private string _instruction = string.Empty;
        public string Instruction
        {
            get => _instruction;
            set
            {
                if (_instruction != value)
                {
                    _instruction = value;
                    OnPropertyChanged(nameof(Instruction));
                }
            }
        }

        // Persisted
        public string PromptKey { get; set; } = "";
        public string MetaPromptKey { get; set; } = "";

        private string _text = string.Empty;
        public string Text
        {
            get => _text;
            set
            {
                if (_text != value)
                {
                    _text = value;
                    OnPropertyChanged(nameof(Text));
                }
            }
        }

        private bool _isUsed = false;
        public bool IsUsed
        {
            get => _isUsed;
            set
            {
                if (_isUsed != value)
                {
                    _isUsed = value;
                    OnPropertyChanged(nameof(IsUsed));
                }
            }
        }

        private bool _isGenerating = false;
        public bool IsGenerating
        {
            get => _isGenerating;
            set
            {
                if (_isGenerating != value)
                {
                    _isGenerating = value;
                    OnPropertyChanged(nameof(IsGenerating));
                }
            }
        }

        public async Task<string> GenerateAsync(
            ListEntryService listEntryService)
        {
            IsGenerating = true;
            Text = string.Empty;

            try
            {
                Text = string.Empty;

                Text = await listEntryService.SendPromptRequestAsync(PromptKey, Name, Instruction);
            }
            finally
            {
                IsGenerating = false;
            }

            return Text;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
