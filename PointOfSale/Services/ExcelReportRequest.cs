namespace UI.Services
{
    public class ExcelReportRequest
    {
        public ReportType ReportType { get; set; }

        public string Title { get; set; } = string.Empty;

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public object? Summary { get; set; }
        public object Data { get; set; } = null!;
    }
}