namespace Client.Models
{
    public class NavViewModel
    {
        public string Title { get; set; }
        public string Controller { get; set; }
        public string? Action { get; set; } = "Index";
        public string? Icon { get; set; } = "fa-chart-line";

        public List<NavViewModel>? Children { get; set; } = new();

        public bool HasChildren => (Children != null && Children.Count > 0);
    }
}
