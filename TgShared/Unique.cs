using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System.Diagnostics;
using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TgShared
{
    public static class Unique
    {
        public static async Task ProcessAndUploadAsync(Message message, UserSession session, ITelegramBotClient bot, AppSettings settings)
        {
            string ffmpegPath = settings.FfmpegPath;
            string ffprobePath = settings.FfprobePath;
            var uploadResults = new List<string>();
            string fileId = message.Video?.FileId ?? message.Document?.FileId;

            if (fileId == null)
            {
                await bot.SendMessage(message.Chat.Id, "❌ Не удалось получить файл. Убедитесь, что вы отправили видео или документ.");
                return;
            }

            var file = await bot.GetFile(fileId);

            string inputPath = $"temp_{DateTime.Now:yyyyMMdd_HHmmss}.mp4";

            using (var fileStream = new FileStream(inputPath, FileMode.Create))
            {
                await bot.DownloadFile(file.FilePath, fileStream);
            }

            double baseFps;
            await bot.SendMessage(message.Chat.Id, "⌛ Начинаем процесс уникализации...");
            try
            {
                baseFps = GetVideoFps(ffprobePath, inputPath);
                await bot.SendMessage(message.Chat.Id, $"🎥 Исходный FPS: {baseFps:F2}");
            }
            catch (Exception e)
            {
                await bot.SendMessage(message.Chat.Id, $"❌ Не удалось определить FPS: {e.Message}");
                File.Delete(inputPath);
                return;
            }

            var random = new Random();
            int cnt = 0;

            for (int i = 0; i < session.channels.Count(); i++)
            {
                double brightness = Math.Round(random.NextDouble() * 0.2 - 0.1, 2);
                double contrast = Math.Round(1.0 + random.NextDouble() * 0.2 - 0.1, 2);
                double fpsOffset = random.NextDouble() * 4 - 2;
                double newFps = Math.Round(Math.Max(1, baseFps + fpsOffset), 2);

                string brightnessStr = brightness.ToString("0.00", CultureInfo.InvariantCulture);
                string contrastStr = contrast.ToString("0.00", CultureInfo.InvariantCulture);
                string fpsStr = newFps.ToString("0.00", CultureInfo.InvariantCulture);

                string title = session.PendingTitle;
                string comment = "";
                string author = $"UploaderBot_{random.Next(1000, 9999)}";
                DateTime creationTime = DateTime.Now.AddDays(-random.Next(0, 30));
                string creationTimeIso = creationTime.ToString("yyyy-MM-ddTHH:mm:ss");

                string safeTimestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                string outputPath = $"unique_{i}_{safeTimestamp}.mp4";

                string videoFilter = $"eq=brightness={brightnessStr}:contrast={contrastStr},fps={fpsStr}";
                string args = $"-y -i \"{inputPath}\" -vf \"{videoFilter}\" " +
                              $"-metadata title=\"{title}\" -metadata comment=\"{comment}\" " +
                              $"-metadata author=\"{author}\" -metadata creation_time=\"{creationTimeIso}\" " +
                              $"-c:a copy \"{outputPath}\"";

                RunFFmpeg(ffmpegPath, args);

                if (!File.Exists(outputPath))
                {
                    await bot.SendMessage(message.Chat.Id, $"❌ Выходной файл не найден: {outputPath}");
                    continue;
                }
                var fileInfo = new FileInfo(outputPath);
                if (fileInfo.Length < 1024 * 50)
                {
                    await bot.SendMessage(message.Chat.Id, $"⚠️ Подозрительно маленький файл: {fileInfo.Length / 1024} KB");
                    continue;
                }
                
                try
                {
                    var YouTubeService = await AuthenticateAsync(session.channels[i]);
                    await UploadVideoAsync(YouTubeService, outputPath, title, comment);
                    uploadResults.Add($"✔️ {session.channels[i].ChannelName}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка загрузки: {ex}");
                    uploadResults.Add($"❌ {session.channels[i].ChannelName} — {ex.Message}");
                }

            }
            if (File.Exists(inputPath))
                File.Delete(inputPath);

            await bot.SendMessage(message.Chat.Id, $"✅ Уникализировано и загружено: {cnt} видео.");
            await bot.SendMessage(message.Chat.Id, "📊 Результаты загрузки:\n" + string.Join("\n", uploadResults));
        }
        private static void RunFFmpeg(string ffmpegPath, string args)
        {
            var psi = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = args,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (!string.IsNullOrEmpty(error))
            {
                Console.WriteLine("FFmpeg output:");
                Console.WriteLine(error);
            }
        }
        private static double GetVideoFps(string ffprobePath, string inputPath)
        {
            var psi = new ProcessStartInfo
            {
                FileName = ffprobePath,
                Arguments = $"-v 0 -of csv=p=0 -select_streams v:0 -show_entries stream=r_frame_rate \"{inputPath}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            string output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();

            if (output.Contains("/"))
            {
                var parts = output.Split('/');
                if (double.TryParse(parts[0], out double num) &&
                    double.TryParse(parts[1], out double den) && den != 0)
                {
                    return num / den;
                }
            }

            throw new Exception("Не удалось определить FPS.");
        }
        private static async Task<YouTubeService> AuthenticateAsync(YouTubeAcc acc)
        {
            var token = new TokenResponse
            {
                RefreshToken = acc.RefreshToken
            };

            var secrets = new ClientSecrets
            {
                ClientId = acc.ClientId,
                ClientSecret = acc.ClientSecret
            };

            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = secrets,
                Scopes = new[] { YouTubeService.Scope.YoutubeUpload }
            });

            var credential = new UserCredential(flow, acc.ChannelName, token);
            await credential.RefreshTokenAsync(CancellationToken.None);

            return new YouTubeService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "UniqueUploader"
            });
        }

        private static async Task UploadVideoAsync(YouTubeService svc, string filePath, string title, string description)
        {
            var video = new Google.Apis.YouTube.v3.Data.Video
            {
                Snippet = new VideoSnippet
                {
                    Title = title,
                    Description = description,
                    CategoryId = "22"
                },
                Status = new VideoStatus
                {
                    PrivacyStatus = "private",
                    MadeForKids = false
                }
            };

            using var fs = new FileStream(filePath, FileMode.Open);
            var req = svc.Videos.Insert(video, "snippet,status", fs, "video/mp4");
            await req.UploadAsync();
        }
    }
}
