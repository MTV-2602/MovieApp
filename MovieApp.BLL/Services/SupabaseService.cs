using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace MovieApp.BLL.Services
{
    public class SupabaseService
    {
        private readonly string _supabaseUrl;
        private readonly string _supabaseKey;
        private readonly string _bucketName;

        public SupabaseService()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            _supabaseUrl = config["Supabase:Url"] ?? throw new Exception("Supabase URL not configured");
            _supabaseKey = config["Supabase:Key"] ?? throw new Exception("Supabase Key not configured");
            _bucketName = config["Supabase:BucketName"] ?? "posters";
        }

        public async Task<string> UploadPosterAsync(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found", filePath);

            var extension = Path.GetExtension(filePath);
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var contentType = GetContentType(filePath);

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("apikey", _supabaseKey);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_supabaseKey}");

            var fileBytes = await File.ReadAllBytesAsync(filePath);
            
            using var content = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(fileBytes);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
            content.Add(fileContent, "file", uniqueFileName);

            var storagePath = $"posters/{uniqueFileName}";
            var encodedPath = Uri.EscapeDataString(storagePath);
            var url = $"{_supabaseUrl}/storage/v1/object/{_bucketName}/{encodedPath}";
            var response = await client.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to upload to Supabase: {error}");
            }

            return $"{_supabaseUrl}/storage/v1/object/public/{_bucketName}/{storagePath}";
        }

        public async Task<string> UploadPosterFromStreamAsync(Stream stream, string fileName)
        {
            var extension = Path.GetExtension(fileName);
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var contentType = GetContentType(fileName);

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("apikey", _supabaseKey);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_supabaseKey}");

            var content = new StreamContent(stream);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

            var storagePath = $"posters/{uniqueFileName}";
            var encodedPath = Uri.EscapeDataString(storagePath);
            var url = $"{_supabaseUrl}/storage/v1/object/{_bucketName}/{encodedPath}";
            var response = await client.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to upload to Supabase: {error}");
            }

            return $"{_supabaseUrl}/storage/v1/object/public/{_bucketName}/{storagePath}";
        }

        public async Task<string> UploadVideoAsync(string filePath, string bucketName = "videos")
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found", filePath);

            var fileInfo = new FileInfo(filePath);
            const long maxSizeBytes = 100 * 1024 * 1024; // 100MB
            
            if (fileInfo.Length > maxSizeBytes)
            {
                var sizeMB = fileInfo.Length / (1024.0 * 1024.0);
                throw new ArgumentException($"Video file quá lớn ({sizeMB:F2} MB). Giới hạn: 100 MB.");
            }

            var extension = Path.GetExtension(filePath);
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var contentType = GetVideoContentType(filePath);

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("apikey", _supabaseKey);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_supabaseKey}");

            var fileBytes = await File.ReadAllBytesAsync(filePath);
            
            using var content = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(fileBytes);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
            content.Add(fileContent, "file", uniqueFileName);

            var storagePath = $"trailers/{uniqueFileName}";
            var encodedPath = Uri.EscapeDataString(storagePath);
            var url = $"{_supabaseUrl}/storage/v1/object/{bucketName}/{encodedPath}";
            var response = await client.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to upload video to Supabase: {error}");
            }

            return $"{_supabaseUrl}/storage/v1/object/public/{bucketName}/{storagePath}";
        }

        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLower();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => "image/jpeg"
            };
        }

        private string GetVideoContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLower();
            return extension switch
            {
                ".mp4" => "video/mp4",
                ".webm" => "video/webm",
                ".ogg" => "video/ogg",
                ".mov" => "video/quicktime",
                ".avi" => "video/x-msvideo",
                _ => "video/mp4"
            };
        }
    }
}

