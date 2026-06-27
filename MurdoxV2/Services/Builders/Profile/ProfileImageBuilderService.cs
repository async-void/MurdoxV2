using Humanizer;
using Microsoft.Extensions.Logging;
using MurdoxV2.Utilities.Drawing;
using SkiaSharp;

namespace MurdoxV2.Services.Builders.Profile
{
    public sealed class ProfileImageBuilderService(ILogger<ProfileImageBuilderService> logger,
        HttpClient client)
    {
        private readonly ILogger<ProfileImageBuilderService> _logger = logger;
        private readonly HttpClient _httpClient = client;

        public async Task<MemoryStream> BuildProfileImageAsync(MemberProfileDTO  memberData)
        {
            var bitmap = new SKBitmap(800, 350);
            using var canvas = new SKCanvas(bitmap);

            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "MurdoxV2/1.0");

            var avatarBytes = await _httpClient.GetByteArrayAsync(memberData.MemberAvatarUrl);
            using var memAvatar = SKBitmap.Decode(avatarBytes);

            using var resizedAvatar = new SKBitmap(220, 220);
            using (var resizedCanvas = new SKCanvas(resizedAvatar))
            {
                resizedCanvas.DrawBitmap(memAvatar, new SKRect(0,0, memAvatar.Width, memAvatar.Height), new SKRect(0,0, 220, 220));
            }

            using var cardBorder = new SKPaint
            {
                Color = SKColor.Parse("#1a1a1e"),
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 6
            };
            canvas.DrawRoundRect(new SKRect(0, 0, 800, 300), 16, 16, cardBorder);
            using var bgPaint = DrawingUtilities.DrawGradientBackgound("#2F3136", "#454950");
            canvas.DrawRoundRect(new SKRect(6, 6, 794, 294), 14, 14, bgPaint);

            using var path = new SKPath();
            path.AddCircle(150, 150, 110);
            canvas.Save();
            canvas.ClipPath(path, SKClipOperation.Intersect, antialias: true);
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

            //==============================//
            //FONT
            //==============================//
            var fontBoldPath = Path.Combine(AppContext.BaseDirectory, "Fonts", "Roboto-Bold.ttf");
            var fontItalicPath = Path.Combine(AppContext.BaseDirectory, "Fonts", "Roboto_SemiCondensed-Light.ttf");
            //=============================//
            //FONT COLORS
            //=============================//
            var greyBase = SKColors.LightGray;
            var primaryColor = SKColor.Parse("#5addcb");
            var lighterPrimary = SKColor.Parse("#8FE7DB");
            //=============================//
            //FONT STYLE
            //============================//
            using var fontBold = SKTypeface.FromFile(fontBoldPath);
            using var fontItalic = SKTypeface.FromFile(fontItalicPath);
            using var boldFont = new SKFont(fontBold, 36);
            using var italicFont = new SKFont(fontItalic, 22);
            using var memNameFont = DrawingUtilities.GetFittedFont(fontBold, memberData.NickName ?? "UNKOWN", maxWidth: 440, startSize: 36f);
            //============================//
            //FONT PAINT
            //============================//
            using var titlePaint = new SKPaint { Color = primaryColor, IsAntialias = true };
            using var boldPaint = new SKPaint { Color = greyBase, IsAntialias = true };
            using var italicPaint = new SKPaint { Color = greyBase, IsAntialias = true };

            //=============================//
            //DRAW THE INFO
            //============================//
            canvas.DrawText($"{memberData.GlobalUsername}", 380, 80, memNameFont, titlePaint);
            canvas.DrawText($"ID: {memberData.MemberId}", 380, 110, italicFont, italicPaint);
            canvas.DrawText($"Created: {memberData.CreatedAt.Humanize()}", 380, 140, italicFont, italicPaint);
            canvas.DrawText($"Joined: {memberData.JoinedGuildAt.Humanize()}", 380, 170, italicFont, italicPaint);
            canvas.DrawText($"XP: {memberData.XP}", 380, 200, italicFont, italicPaint);
            canvas.DrawText($"Balance: ${memberData.Bank?.Balance ?? 0.00}", 380, 230, italicFont, italicPaint);
            canvas.DrawText($"Verified: {memberData.VerifiedStatus ?? "UNKOWN"}", 380, 260, italicFont, italicPaint);


            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 85);
            var stream = new MemoryStream();
            data.SaveTo(stream);
            stream.Position = 0;
            bitmap.Dispose();
            return stream;
        }
    }
}
