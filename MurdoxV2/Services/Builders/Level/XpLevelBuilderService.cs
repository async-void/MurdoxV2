using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using MurdoxV2.Interfaces;
using MurdoxV2.Services.Level;
using MurdoxV2.Utilities.Drawing;
using SkiaSharp;

namespace MurdoxV2.Services.Builders.Level
{
    public sealed class XpLevelBuilderService(IMemberData memberDataService, ILevel levelService, ILogger<LevelService> logger)
    {
        private readonly IMemberData _memberDataService = memberDataService;
        private readonly ILevel _levelService = levelService;
        private readonly ILogger<LevelService> _logger = logger;
        public async Task<MemoryStream> BuildLevelBitmapAsync(DiscordMember member)
        {
            var bitmap = new SKBitmap(800, 300);
            using var canvas = new SKCanvas(bitmap);
            canvas.Clear(SKColor.Parse("#2F3136"));

            var memberXp = await _memberDataService.GetMemberXPAsync(member.Id, member.Guild.Id);
            _logger.LogInformation("{memberName} with id: {id} has XP : {xp}", member.Username, member.Id, memberXp);

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
            var avatarBytes = await httpClient.GetByteArrayAsync(member.AvatarUrl);
            using var avatarBmp = SKBitmap.Decode(avatarBytes);

            using var resizedAvatar = new SKBitmap(220, 220);
            using (var resizeCanvas = new SKCanvas(resizedAvatar))
            {
                resizeCanvas.DrawBitmap(avatarBmp, new SKRect(0, 0, avatarBmp.Width, avatarBmp.Height), new SKRect(0, 0, 220, 220));
            }

            using var path = new SKPath();
            path.AddCircle(150, 150, 110); 
            canvas.Save();
            canvas.ClipPath(path, antialias: true);
            canvas.DrawBitmap(resizedAvatar, 40, 40);
            canvas.Restore();

            using var border = new SKPaint
            {
                Color = SKColor.Parse("#7160e8"),
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 6
            };
            canvas.DrawCircle(150, 150, 110, border);

            var fontPath = Path.Combine(AppContext.BaseDirectory, "Fonts", "Roboto-Bold.ttf");
            using var font = SKTypeface.FromFile(fontPath);
            using var nameFont = DrawingUtilities.GetFittedFont(font, member.DisplayName, maxWidth: 440, startSize: 36f);
            using var idFont = new SKFont(font, 24);
            using var xpFont = new SKFont(font, 24);
            
            using var namePaint = new SKPaint { Color = SKColors.LightGray, IsAntialias = true};
            using var idPaint = new SKPaint { Color = SKColors.DarkSeaGreen, IsAntialias = true };
            using var xpPaint = new SKPaint { Color = SKColors.DarkSeaGreen, IsAntialias = true };
            canvas.DrawText(member.DisplayName, 320, 100, nameFont, namePaint);
            canvas.DrawText($"ID: {member.Id}", 320, 140, idFont, idPaint);
            //canvas.DrawText($"XP: {memberXp}", 320, 220, xpFont, xpPaint);

            var (level, currentXp, neededXp) = await _levelService.GetProgressAsync(memberXp);
            DrawingUtilities.DrawXpBar(canvas, font, currentXp: currentXp, xpForNextLevel: neededXp, level: level);

            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            var stream = new MemoryStream();
            data.SaveTo(stream);
            stream.Position = 0;
            //File.WriteAllBytes("debug.png", stream.ToArray());
            bitmap.Dispose();

            return stream;
        }
    }
}
