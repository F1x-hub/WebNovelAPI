using System.Text.Json;

namespace BasicWebNovelAPI.Model.Errors
{
    public class ErrorDetail
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ErrorId { get; set; } = string.Empty;
        public string RequestId { get; set; } = string.Empty;
        public string Detail { get; set; } = string.Empty;
        public DateTime TtimeSpan { get; set; } = DateTime.Now;
        public string StackTrace { get; set; } = string.Empty;

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                WriteIndented = true,
            });
        }
    }
}
