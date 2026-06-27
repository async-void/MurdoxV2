using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Utilities.Drawing
{
    public static class DrawingUtilities
    {
        #region GET FITTED FONT
        public static SKFont GetFittedFont(SKTypeface typeface, string text, float maxWidth, float startSize, float minSize = 18f)
        {
            float size = startSize;
            while (size > minSize)
            {
                using var testFont = new SKFont(typeface, size);
                if (testFont.MeasureText(text) <= maxWidth)
                    return new SKFont(typeface, size);

                size -= 2f;
            }
            return new SKFont(typeface, minSize);
        }
        #endregion

        #region DRAW XP BAR
        public static void DrawXpBar(SKCanvas canvas, SKTypeface font, int currentXp, int xpForNextLevel, int level,
            int x = 320, int y = 180, int width = 440, int height = 24)
        {
            float progress = xpForNextLevel > 0 ? Math.Clamp((float)currentXp / xpForNextLevel, 0f, 1f) : 0f;
            float radius = height / 2f;

            // Background track
            using var trackPaint = new SKPaint { Color = SKColor.Parse("#40444B"), IsAntialias = true };
            canvas.DrawRoundRect(new SKRect(x, y, x + width, y + height), radius, radius, trackPaint);

            // Fill
            float fillWidth = width * progress;
            if (fillWidth > 0)
            {
                using var fillPaint = new SKPaint { Color = SKColor.Parse("7160e8"), IsAntialias = true };

                // Clip to track shape so the fill respects rounded corners even when partial
                using var trackPath = new SKPath();
                trackPath.AddRoundRect(new SKRect(x, y, x + width, y + height), radius, radius);
                canvas.Save();
                canvas.ClipPath(trackPath, antialias: true);
                canvas.DrawRect(new SKRect(x, y, x + fillWidth, y + height), fillPaint);
                canvas.Restore();
            }

            // XP text above bar
            using var xpFont = new SKFont(font, 18);
            using var xpPaint = new SKPaint { Color = SKColors.LightGray, IsAntialias = true };
            canvas.DrawText($"Level {level}  •  {currentXp} / {xpForNextLevel} XP", x, y - 10, xpFont, xpPaint);
        }
        #endregion

        #region DRAW GRADIENT BACKGROUND

        public static SKPaint DrawGradientBackgound(string color1, string color2)
        {
            var colors = new SKColor[] { SKColor.Parse(color1), SKColor.Parse(color2) };
            var shader = SKShader.CreateLinearGradient
            (
                new SKPoint(0, 0),
                new SKPoint(800, 350),
                colors, null,
                SKShaderTileMode.Clamp
            );
            return new SKPaint
            {
                Shader = shader,
                IsAntialias = true
            };
        }

        #endregion
    }
}
