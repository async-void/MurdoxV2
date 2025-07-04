using MurdoxV2.Enums;

namespace MurdoxV2.Models
{
    public class SystemError<T>
    {
        public int Id { get; set; }
        public string? ErrorMessage { get; set; }
        public T? CreatedBy { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
        public ErrorType ErrorType { get; set; }

    }
}
