using System.Text.Json;

namespace BasicWebNovelAPI.Model.Errors
{
    public class ErrorDetail
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string ErrorId { get; set; }
        public string RequestId { get; set; }
        public string Detail { get; set; }
        public DateTime TtimeSpan { get; set; } = DateTime.Now;
        public string StackTrace { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                WriteIndented = true,
            });
        }
    }
}
