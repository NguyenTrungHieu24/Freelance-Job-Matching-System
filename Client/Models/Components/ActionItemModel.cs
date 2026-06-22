namespace Client.Models.Components
{
    public class ActionItemModel
    {
        public string Title { get; set; } = null!;
        public string? Icon { get; set; }
        public string? Url { get; set; }
        public string? AjaxUrl { get; set; }
        public string? ConfirmMessage { get; set; }
        public string? ExtraMessage { get; set; }
        public string? Handle { get; set; }
        public string? CssClass { get; set; }

        public bool Show { get; set; } = true;

        public string? OnClick { get; set; }
    }
}
