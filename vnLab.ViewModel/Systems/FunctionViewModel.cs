namespace vnLab.ViewModel.Systems
{
    public class FunctionViewModel
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Url { get; set; }
        public int SortOrder { get; set; }
        public string? ParentId { get; set; }
        public string? Icon { get; set; }
    }

    public class FunctionTreeViewModel
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Url { get; set; }
        public int SortOrder { get; set; }
        public string? ParentId { get; set; }
        public string? Icon { get; set; }
        public List<FunctionTreeViewModel>? Children { get; set; }
    }
}