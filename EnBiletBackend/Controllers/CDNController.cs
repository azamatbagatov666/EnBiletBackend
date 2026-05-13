using Microsoft.AspNetCore.Mvc;
using System.Data.Common;
using System.Xml.Linq;
using EnBiletBackend.Attributes;
using EnBiletBackend.Models;
using EnBiletBackend.Services;
using System;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.DataProtection;

namespace EnBiletBackend.Controllers

{
    [ApiController]
    public class CDNController : Controller
    {
        private readonly IConfiguration _config;
        public CDNController(
            AuthenticationService authenticationService,
            IConfiguration config
        )
        {
            _config = config;
        }


        [TheAuthorize]
        [HttpPost("get-image-upload-sas")]
        public IActionResult GetImageUploadSas()
        {
            var accountName = "cocukakli";
            var containerName = "public-images";

            var accountKey = _config["CDN:CDNKey"];

            var credential = new StorageSharedKeyCredential(accountName, accountKey);

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerName,
                Resource = "c",
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(1)
            };

            sasBuilder.SetPermissions(BlobContainerSasPermissions.Write | BlobContainerSasPermissions.Create);

            var sasToken = sasBuilder.ToSasQueryParameters(credential).ToString();

            return Ok(new
            {
                uploadUrl = $"https://{accountName}.blob.core.windows.net/{containerName}",
                sasToken
            });
        }

        [TheAuthorize]
        [HttpPost("delete-image")]
        public async Task<IActionResult> DeleteImage([FromBody] DELETEIMAGEREQUEST request)
        {
            if (request.ImageKeys == null || request.ImageKeys.Count == 0)
                return BadRequest("ImageKeys is required.");

            var accountName = "cocukakli";
            var containerName = "public-images";
            var accountKey = _config["CDN:CDNKey"];

            var credential = new StorageSharedKeyCredential(accountName, accountKey);
            var blobServiceClient = new BlobServiceClient(
                new Uri($"https://{accountName}.blob.core.windows.net"),
                credential
            );

            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            var deleted = new List<string>();
            var notFound = new List<string>();

            foreach (var key in request.ImageKeys)
            {
                var blobClient = containerClient.GetBlobClient(key);
                var result = await blobClient.DeleteIfExistsAsync();

                if (result.Value)
                    deleted.Add(key);
                else
                    notFound.Add(key);
            }

            return Ok(new
            {
                message = "Delete operation completed.",
                deleted,
                notFound
            });
        }

    }
}
