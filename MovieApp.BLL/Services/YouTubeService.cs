using System.Text.RegularExpressions;

namespace MovieApp.BLL.Services
{
    public class YouTubeService
    {
        public static string ConvertToEmbedUrl(string youtubeUrl)
        {
            if (string.IsNullOrWhiteSpace(youtubeUrl))
                return string.Empty;

            var videoId = ExtractVideoId(youtubeUrl);
            if (string.IsNullOrEmpty(videoId))
                return youtubeUrl;

            return $"https://www.youtube.com/embed/{videoId}";
        }

        public static bool IsValidYouTubeUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            var videoId = ExtractVideoId(url);
            return !string.IsNullOrEmpty(videoId);
        }

        public static string ExtractVideoId(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return string.Empty;

            var patterns = new[]
            {
                @"(?:youtube\.com\/watch\?v=|youtu\.be\/|youtube\.com\/embed\/)([a-zA-Z0-9_-]{11})",
                @"youtube\.com\/watch\?.*v=([a-zA-Z0-9_-]{11})"
            };

            foreach (var pattern in patterns)
            {
                var match = Regex.Match(url, pattern);
                if (match.Success && match.Groups.Count > 1)
                {
                    return match.Groups[1].Value;
                }
            }

            if (Regex.IsMatch(url, @"^[a-zA-Z0-9_-]{11}$"))
            {
                return url;
            }

            return string.Empty;
        }
    }
}

