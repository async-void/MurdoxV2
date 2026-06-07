using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Features.ScamDetection
{
    public sealed class HttpImageIngestionService(HttpClient httpClient) : IImageIngestionService
    {
        private readonly HttpClient _httpClient = httpClient;

        public async Task<ImageIngestionResult> FetchAsync(ScamImageContext context)
        {
            try
            {
                var url = context.Attachment.Url;
                var bytes = await _httpClient.GetByteArrayAsync(url);

                if (bytes.Length > 0) 
                    return ImageIngestionResult.Ok(bytes);
                return ImageIngestionResult.Fail("Downloaded image is empty.");
            }
            catch (Exception ex)
            {
                return ImageIngestionResult.Fail($"Failed to download image: {ex.Message}");
            }
        }
    }
}
