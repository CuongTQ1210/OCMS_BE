using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using OCMS_Services.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace OCMS_Services.Service
{
    public class BlobService : IBlobService
    {
        private readonly BlobServiceClient _blobServiceClient;

        public BlobService(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;
        }

        #region UploadFileAsync
        public async Task<string> UploadFileAsync(string containerName, string blobName, Stream fileStream, string contentType = null)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync();
            var blobClient = containerClient.GetBlobClient(blobName);

            // Đặt Content-Type và Content-Disposition
            var blobHttpHeaders = new BlobHttpHeaders
            {
                ContentType = contentType ?? GetContentTypeFromExtension(blobName),
                ContentDisposition = "inline" // Đảm bảo hiển thị trực tiếp
            };

            await blobClient.UploadAsync(fileStream, new BlobUploadOptions
            {
                HttpHeaders = blobHttpHeaders
            });

            return blobClient.Uri.ToString();
        }
        #endregion

        #region DeleteFileAsync
        public async Task DeleteFileAsync(string certificateFileUrl)
        {
            Uri uri = new Uri(certificateFileUrl);
            string containerName = uri.Segments[1].TrimEnd('/');
            string blobName = string.Join("", uri.Segments.Skip(2));

            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            // Delete the blob
            await blobClient.DeleteIfExistsAsync();
        }
        #endregion

        #region GenerateSasToken
        public async Task<string> GenerateSasTokenForBlobAsync(string blobUrl, TimeSpan validityPeriod, string permissions = "r")
        {
            // Xóa query string và giải mã URL
            UriBuilder uriBuilder = new UriBuilder(blobUrl);
            uriBuilder.Query = null;
            Uri uri = uriBuilder.Uri;

            // Lấy container và blob name
            string containerName = uri.Segments[1].TrimEnd('/');
            string rawBlobName = string.Join("", uri.Segments.Skip(2));
            string blobName = HttpUtility.UrlDecode(rawBlobName);

            // Khởi tạo BlobClient
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            // Tạo SAS token với các tham số
            BlobSasBuilder sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerName,
                BlobName = blobName,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.Add(validityPeriod),
            };

            // Đặt quyền
            sasBuilder.SetPermissions(permissions switch
            {
                "r" => BlobSasPermissions.Read,
                "w" => BlobSasPermissions.Write,
                _ => throw new ArgumentException("Invalid permissions")
            });

            // Tạo SAS URI
            var sasUri = blobClient.GenerateSasUri(sasBuilder);

            return sasUri.Query;
        }       

        public async Task<string> GetBlobUrlWithSasTokenAsync(string blobUrl, TimeSpan validityPeriod, string permissions = "r")
        {
            // Loại bỏ SAS token từ URL (nếu có)
            var cleanBlobUrl = RemoveSasTokenFromUrl(blobUrl);

            // Tạo SAS token mới
            var sasToken = await GenerateSasTokenForBlobAsync(cleanBlobUrl, validityPeriod, permissions);

            return $"{cleanBlobUrl}{sasToken}";
        }
        #endregion

        #region GetBlobUrlWithoutSasToken
        public string GetBlobUrlWithoutSasToken(string blobUrl)
        {
            return RemoveSasTokenFromUrl(blobUrl);
        }
        #endregion

        #region Helper Methods
        // Hàm hỗ trợ lấy Content-Type dựa trên phần mở rộng
        private string GetContentTypeFromExtension(string blobName)
        {
            var extension = Path.GetExtension(blobName).ToLower();
            return extension switch
            {
                ".html" => "text/html",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".pdf" => "application/pdf",
                _ => "application/octet-stream"
            };
        }
        private BlobSasPermissions GetBlobSasPermissions(string permissions)
        {
            BlobSasPermissions sasPermissions = 0; // Initialize with 0 as 'None' is not defined in BlobSasPermissions.

            if (permissions.Contains("r")) sasPermissions |= BlobSasPermissions.Read;
            if (permissions.Contains("w")) sasPermissions |= BlobSasPermissions.Write;
            if (permissions.Contains("d")) sasPermissions |= BlobSasPermissions.Delete;
            if (permissions.Contains("l")) sasPermissions |= BlobSasPermissions.List;
            if (permissions.Contains("a")) sasPermissions |= BlobSasPermissions.Add;
            if (permissions.Contains("c")) sasPermissions |= BlobSasPermissions.Create;
            if (permissions.Contains("t")) sasPermissions |= BlobSasPermissions.Tag;

            return sasPermissions;
        }

        private string RemoveSasTokenFromUrl(string url)
        {
            int signatureIndex = url.IndexOf("?sv=");
            if (signatureIndex == -1)
                signatureIndex = url.IndexOf("?");

            return signatureIndex < 0 ? url : url.Substring(0, signatureIndex);
        }
        #endregion
    }
}
