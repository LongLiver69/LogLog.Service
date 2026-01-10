using Microsoft.AspNetCore.Mvc;
using Minio;
using Minio.DataModel.Args;

namespace LogLog.Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IMinioClient _minio;
        private readonly IConfiguration _config;

        public FileController(IMinioClient minio, IConfiguration config)
        {
            _minio = minio;
            _config = config;
        }

        [HttpPost("upload-url")]
        public async Task<IActionResult> GetUploadUrl([FromBody] string fileName)
        {
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            var extension = Path.GetExtension(fileName);
            var objectName = $"{fileNameWithoutExt}_{Guid.NewGuid()}{extension}";

            var url = await _minio.PresignedPutObjectAsync(
                new PresignedPutObjectArgs()
                    .WithBucket(_config["MinIO:Bucket"])
                    .WithObject(objectName)
                    .WithExpiry(60 * 2) // 2 phút
            );

            return Ok(new { objectName, url });
        }

        [HttpGet("download-url/{objectName}")]
        public async Task<IActionResult> GetDownloadUrl(string objectName)
        {
            var url = await _minio.PresignedGetObjectAsync(
                new PresignedGetObjectArgs()
                    .WithBucket(_config["MinIO:Bucket"])
                    .WithObject(objectName)
                    .WithExpiry(60 * 2) // 2 phút
            );

            return Ok(new { url });
        }
    }
}
