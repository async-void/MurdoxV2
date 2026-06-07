namespace MurdoxV2.Features.ScamDetection
{
    public sealed class ImageIngestionResult(bool success, byte[] imageBytes, string errorMessage)
    {
        public bool Success { get; } = success;
        public byte[] ImageBytes { get; } = imageBytes;
        public string ErrorMessage { get; } = errorMessage;

        public static ImageIngestionResult Ok(byte[] bytes) =>
            new(true, bytes, null);
        public static ImageIngestionResult Fail(string error) =>
            new(false, null, error);
    }
}
