using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace OutfitReactions.Ai
{
    public sealed class OutfitVisionImage
    {
        public string MimeType { get; set; } = "image/png";
        public string Base64Data { get; set; } = "";
        public string Hash { get; set; } = "";
        public int Width { get; set; }
        public int Height { get; set; }

        // Optional SECOND view of the farmer (back-facing), so the AI can also see items that
        // only show from behind (wings, capes, backpacks, back of the hair). The front image
        // above stays the primary one used for pixel color reading; this one is shape-only.
        public string Base64DataBack { get; set; } = "";

        // Dominant hair color read directly from the rendered sprite pixels (works for
        // texture-painted hair where Fashion Sense reports no tint). Empty when unavailable.
        public bool HasHairColor { get; set; }
        public string HairColorName { get; set; } = "";
        public string HairColorHex { get; set; } = "";

        // Dominant hat/headwear color read directly from the rendered sprite pixels.
        // Used for Fashion Sense hats whose real color is painted into the texture
        // instead of exposed through the Fashion Sense tint API. Empty when unavailable.
        public bool HasHatColor { get; set; }
        public string HatColorName { get; set; } = "";
        public string HatColorHex { get; set; } = "";

        public bool IsUsable => !string.IsNullOrWhiteSpace(Base64Data) && !string.IsNullOrWhiteSpace(MimeType);

        public bool HasBackImage => !string.IsNullOrWhiteSpace(Base64DataBack);

        public string ToDataUri()
        {
            return "data:" + (string.IsNullOrWhiteSpace(MimeType) ? "image/png" : MimeType) + ";base64," + (Base64Data ?? "");
        }

        public string ToBackDataUri()
        {
            return "data:" + (string.IsNullOrWhiteSpace(MimeType) ? "image/png" : MimeType) + ";base64," + (Base64DataBack ?? "");
        }
    }

    internal sealed class OutfitVisionService
    {
        private const int ImageWidth = 256;
        private const int ImageHeight = 256;

        private readonly IMonitor monitor;

        public OutfitVisionService(IMonitor monitor)
        {
            this.monitor = monitor;
        }

        public bool TryCaptureFarmerAppearance(Farmer farmer, out OutfitVisionImage image, out string reason)
        {
            image = null;
            reason = "unknown reason";

            if (farmer == null)
            {
                reason = "farmer is null";
                return false;
            }

            if (Game1.graphics?.GraphicsDevice == null)
            {
                reason = "graphics device is unavailable";
                return false;
            }

            GraphicsDevice device = Game1.graphics.GraphicsDevice;
            RenderTarget2D target = null;
            SpriteBatch spriteBatch = null;
            RenderTargetBinding[] previousTargets = null;
            bool previousUiDrawingFlag = FarmerRenderer.isDrawingForUI;

            try
            {
                previousTargets = device.GetRenderTargets();
                target = new RenderTarget2D(device, ImageWidth, ImageHeight, false, SurfaceFormat.Color, DepthFormat.None);
                spriteBatch = new SpriteBatch(device);

                device.SetRenderTarget(target);
                device.Clear(Color.Transparent);

                FarmerRenderer.isDrawingForUI = true;
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);

                // Use the farmer's current rendered state so Fashion Sense/Harmony overlays can draw through
                // the same render path the game already uses. Draw at normal in-game scale on a transparent
                // canvas, with generous padding for hats, umbrellas, wings, hair, and tall custom pieces.
                // FRONT view (facing down = 2): shows face, glasses, bows, prints; this is also the view
                // used for pixel color reading, so it must be consistent regardless of where the player faces.
                Vector2 position = new(96f, 56f);
                farmer.FarmerRenderer.draw(
                    spriteBatch,
                    farmer.FarmerSprite.CurrentAnimationFrame,
                    farmer.FarmerSprite.CurrentFrame,
                    farmer.FarmerSprite.SourceRect,
                    position,
                    Vector2.Zero,
                    1f,
                    2,
                    Color.White,
                    0f,
                    4f,
                    farmer
                );

                spriteBatch.End();

                // Unbind the render target before reading it back into PNG bytes.
                if (previousTargets != null && previousTargets.Length > 0)
                    device.SetRenderTargets(previousTargets);
                else
                    device.SetRenderTarget(null);

                using MemoryStream stream = new();
                target.SaveAsPng(stream, ImageWidth, ImageHeight);
                byte[] pngBytes = stream.ToArray();
                if (pngBytes.Length <= 0)
                {
                    reason = "captured PNG was empty";
                    return false;
                }

                using SHA256 sha = SHA256.Create();
                byte[] hashBytes = sha.ComputeHash(pngBytes);

                image = new OutfitVisionImage
                {
                    MimeType = "image/png",
                    Base64Data = Convert.ToBase64String(pngBytes),
                    Hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant(),
                    Width = ImageWidth,
                    Height = ImageHeight
                };

                // Read the rendered pixels back and estimate the dominant hair color from the
                // top band of the sprite. This works for texture-painted hair (e.g. pink hair
                // a creator drew in), where Fashion Sense reports no tint to query.
                try
                {
                    Color[] pixels = new Color[ImageWidth * ImageHeight];
                    target.GetData(pixels);

                    if (TryEstimateHairColor(pixels, ImageWidth, ImageHeight, out Color hairColor))
                    {
                        image.HasHairColor = true;
                        image.HairColorName = ColorNamer.ClosestSimpleColorName(hairColor);
                        image.HairColorHex = "#" + hairColor.R.ToString("X2") + hairColor.G.ToString("X2") + hairColor.B.ToString("X2");
                    }

                    if (TryEstimateHatColor(pixels, ImageWidth, ImageHeight, out Color hatColor))
                    {
                        image.HasHatColor = true;
                        image.HatColorName = ColorNamer.ClosestSimpleColorName(hatColor);
                        image.HatColorHex = "#" + hatColor.R.ToString("X2") + hatColor.G.ToString("X2") + hatColor.B.ToString("X2");
                    }
                }
                catch (Exception ex)
                {
                    monitor?.Log("Hair/hat color pixel read failed: " + ex.Message, LogLevel.Trace);
                }

                // SECOND render: BACK view (facing up = 0), so the AI can also see items that only
                // show from behind (wings, capes, backpacks, back of hair). This image is used for
                // shape only; color always comes from the front image + text data.
                try
                {
                    RenderTarget2D backTarget = new(device, ImageWidth, ImageHeight, false, SurfaceFormat.Color, DepthFormat.None);
                    SpriteBatch backBatch = new(device);
                    try
                    {
                        device.SetRenderTarget(backTarget);
                        device.Clear(Color.Transparent);

                        FarmerRenderer.isDrawingForUI = true;
                        backBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
                        farmer.FarmerRenderer.draw(
                            backBatch,
                            farmer.FarmerSprite.CurrentAnimationFrame,
                            farmer.FarmerSprite.CurrentFrame,
                            farmer.FarmerSprite.SourceRect,
                            new Vector2(96f, 56f),
                            Vector2.Zero,
                            1f,
                            0,
                            Color.White,
                            0f,
                            4f,
                            farmer
                        );
                        backBatch.End();

                        if (previousTargets != null && previousTargets.Length > 0)
                            device.SetRenderTargets(previousTargets);
                        else
                            device.SetRenderTarget(null);

                        using MemoryStream backStream = new();
                        backTarget.SaveAsPng(backStream, ImageWidth, ImageHeight);
                        byte[] backBytes = backStream.ToArray();
                        if (backBytes.Length > 0)
                            image.Base64DataBack = Convert.ToBase64String(backBytes);
                    }
                    finally
                    {
                        backBatch.Dispose();
                        backTarget.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    monitor?.Log("Back-view capture failed (front view still used): " + ex.Message, LogLevel.Trace);
                }

                reason = "ok";
                return true;
            }
            catch (Exception ex)
            {
                reason = ex.Message;
                monitor?.Log(" Vision outfit capture failed: " + ex.Message, LogLevel.Trace);
                return false;
            }
            finally
            {
                try
                {
                    spriteBatch?.Dispose();
                }
                catch
                {
                    // ignore cleanup issues
                }

                try
                {
                    FarmerRenderer.isDrawingForUI = previousUiDrawingFlag;
                    if (previousTargets != null && previousTargets.Length > 0)
                        device.SetRenderTargets(previousTargets);
                    else
                        device.SetRenderTarget(null);
                }
                catch
                {
                    // ignore cleanup issues
                }

                try
                {
                    target?.Dispose();
                }
                catch
                {
                    // ignore cleanup issues
                }
            }
        }
        // Estimates the dominant hair color from the top band of the rendered sprite.
        // Strategy: find the opaque sprite's bounding box, look only at the top ~38% (the head
        // top, which is hair rather than face/skin/clothes), drop transparent, near-outline
        // (very dark), and near-white highlight pixels, then take the MODE of color-quantized
        // buckets (not the mean, which muddies pink/purple into brown). Returns the average of
        // the winning bucket for a precise value.
        private static bool TryEstimateHairColor(Color[] pixels, int width, int height, out Color result)
        {
            result = Color.Black;
            if (pixels == null || pixels.Length < width * height || width <= 0 || height <= 0)
                return false;

            // 1. Bounding box of opaque pixels.
            int minX = width, minY = height, maxX = -1, maxY = -1;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (pixels[y * width + x].A >= 128)
                    {
                        if (x < minX) minX = x;
                        if (x > maxX) maxX = x;
                        if (y < minY) minY = y;
                        if (y > maxY) maxY = y;
                    }
                }
            }

            if (maxX < minX || maxY < minY)
                return false;

            // 2. Top band of the bounding box = head/hair region.
            int boxHeight = maxY - minY + 1;
            int bandBottom = minY + Math.Max(1, (int)(boxHeight * 0.38));

            // 3. Histogram of quantized colors (5-bit-per-channel buckets) over valid pixels.
            Dictionary<int, int> counts = new();
            Dictionary<int, long[]> sums = new(); // bucket -> [rSum, gSum, bSum, n]

            for (int y = minY; y <= bandBottom && y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    Color c = pixels[y * width + x];
                    if (c.A < 200)
                        continue;

                    int brightness = (c.R + c.G + c.B) / 3;
                    if (brightness < 28)      // near-black outline
                        continue;
                    if (brightness > 248)     // near-white highlight/specular
                        continue;

                    int key = ((c.R >> 3) << 10) | ((c.G >> 3) << 5) | (c.B >> 3);
                    if (!counts.ContainsKey(key))
                    {
                        counts[key] = 0;
                        sums[key] = new long[4];
                    }
                    counts[key]++;
                    long[] s = sums[key];
                    s[0] += c.R; s[1] += c.G; s[2] += c.B; s[3]++;
                }
            }

            if (counts.Count == 0)
                return false;

            // 4. Winning bucket (mode) -> average its real pixels for a precise color.
            int bestKey = 0, bestCount = -1;
            foreach (var pair in counts)
            {
                if (pair.Value > bestCount)
                {
                    bestCount = pair.Value;
                    bestKey = pair.Key;
                }
            }

            long[] best = sums[bestKey];
            if (best[3] <= 0)
                return false;

            result = new Color((int)(best[0] / best[3]), (int)(best[1] / best[3]), (int)(best[2] / best[3]));
            return true;
        }

        // Estimates the dominant hat/headwear color from the very top of the rendered sprite.
        // This mirrors the hair pixel estimator: transparent canvas, opaque sprite bbox,
        // dark/white filtering, and mode of quantized colors. It is intentionally only used
        // as an authoritative prompt clue when the current detected change is a Fashion Sense
        // hat/headwear change, so it won't accidentally label normal hair as a hat.
        private static bool TryEstimateHatColor(Color[] pixels, int width, int height, out Color result)
        {
            result = Color.Black;
            if (pixels == null || pixels.Length < width * height || width <= 0 || height <= 0)
                return false;

            int minX = width, minY = height, maxX = -1, maxY = -1;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (pixels[y * width + x].A >= 128)
                    {
                        if (x < minX) minX = x;
                        if (x > maxX) maxX = x;
                        if (y < minY) minY = y;
                        if (y > maxY) maxY = y;
                    }
                }
            }

            if (maxX < minX || maxY < minY)
                return false;

            int boxHeight = maxY - minY + 1;
            int boxWidth = maxX - minX + 1;

            // Headwear usually sits in the top part of the farmer sprite. Use a slightly
            // narrower side padding than hair so tall accessories/wings don't dominate, but
            // keep enough width for wide hats, bows, and umbrellas attached to the head.
            int bandBottom = minY + Math.Max(1, (int)(boxHeight * 0.24));
            int padX = Math.Max(0, (int)(boxWidth * 0.08));
            int startX = minX + padX;
            int endX = maxX - padX;
            if (endX < startX)
            {
                startX = minX;
                endX = maxX;
            }

            Dictionary<int, int> counts = new();
            Dictionary<int, long[]> sums = new();

            for (int y = minY; y <= bandBottom && y <= maxY; y++)
            {
                for (int x = startX; x <= endX; x++)
                {
                    Color c = pixels[y * width + x];
                    if (c.A < 200)
                        continue;

                    int brightness = (c.R + c.G + c.B) / 3;
                    if (brightness < 28)      // near-black outline
                        continue;
                    if (brightness > 248)     // near-white highlight/specular
                        continue;

                    // Avoid vanilla/skin-ish face pixels if the top band overlaps forehead.
                    // This is only a soft guard; colored hats still pass normally.
                    bool likelySkin = c.R >= 175 && c.G >= 105 && c.G <= 220 && c.B >= 70 && c.B <= 190 && c.R > c.B + 20;
                    if (likelySkin)
                        continue;

                    int key = ((c.R >> 3) << 10) | ((c.G >> 3) << 5) | (c.B >> 3);
                    if (!counts.ContainsKey(key))
                    {
                        counts[key] = 0;
                        sums[key] = new long[4];
                    }

                    counts[key]++;
                    long[] s = sums[key];
                    s[0] += c.R; s[1] += c.G; s[2] += c.B; s[3]++;
                }
            }

            if (counts.Count == 0)
                return false;

            int bestKey = 0, bestCount = -1;
            foreach (var pair in counts)
            {
                if (pair.Value > bestCount)
                {
                    bestCount = pair.Value;
                    bestKey = pair.Key;
                }
            }

            long[] best = sums[bestKey];
            if (best[3] <= 0)
                return false;

            result = new Color((int)(best[0] / best[3]), (int)(best[1] / best[3]), (int)(best[2] / best[3]));
            return true;
        }
    }
}
