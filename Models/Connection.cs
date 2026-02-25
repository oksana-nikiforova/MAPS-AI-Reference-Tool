namespace MAPSAI.Models
{
    public class Connection
    {
        private const string DEFAULT_TEXT = "";

        public Connection(string id, string source, string target, string text = DEFAULT_TEXT)
        {
            ID = id;
            Source = source;
            Target = target;
            Text = text;
        }

        public string ID { get; set; }
        public string Source { get; set; }
        public string Target { get; set; }
        public string Text { get; set; }
    }
}
