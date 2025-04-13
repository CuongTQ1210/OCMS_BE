using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Services.IService
{
    public interface IBlobService
    {
        Task<string> UploadFileAsync(string containerName, string blobName, Stream fileStream, string contentType);
        Task DeleteFileAsync(string blobFileUrl);
        Task<string> GenerateSasTokenForBlobAsync(string blobUrl, TimeSpan validityPeriod, string permissions = "r");
        Task<string> GetBlobUrlWithSasTokenAsync(string blobUrl, TimeSpan validityPeriod, string permissions = "r");
        string GetBlobUrlWithoutSasToken(string blobUrl);
    }
}
