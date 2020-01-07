using Amazon.Textract;
using Amazon.Textract.Model;
using Microsoft.Extensions.Configuration;
using SharedLibrary;
using SharedLibrary.Interfaces;
using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace OCRRecognizerService
{
    public class AWSTExtractService : IRecognizerService
    {
        private IConfiguration _configuration;

        public AWSTExtractService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<Harvest> Recognize(string filePathName)
        {
            Logger.LogDebug(">> Recognize");

            var client = new AmazonTextractClient(
                _configuration.GetSection("AccessKey").Value,
                _configuration.GetSection("SecretAccessKey").Value,
                Amazon.RegionEndpoint.GetBySystemName(_configuration.GetSection("Region").Value));

            Amazon.Textract.Model.Document MyDocument;

            int imageWidth = 0;
            int imageHeight = 0;

            using (Image image = Image.FromFile(filePathName))
            {
                imageWidth = image.Width >= 2560 ? 2560 : image.Width;
                imageHeight = image.Height > 1080 ? 1080 : image.Height;

                #region Negative
                /*

                Bitmap newBitmap = new Bitmap(image.Width, image.Height);

                Graphics graphics = Graphics.FromImage(newBitmap);

                ColorMatrix colorMatrix = new ColorMatrix(
                   new float[][]
                  {
                 new float[] {.3f, .3f, .3f, 0, 0},
                 new float[] {.59f, .59f, .59f, 0, 0},
                 new float[] {.11f, .11f, .11f, 0, 0},
                 new float[] {0, 0, 0, 1, 0},
                 new float[] {0, 0, 0, 0, 1}
                  });

                ImageAttributes attributes = new ImageAttributes();
                attributes.SetColorMatrix(colorMatrix);

                graphics.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height),
                   0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);

                graphics.Dispose();
                */

                #endregion

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    image.Save(memoryStream, image.RawFormat);

                    MyDocument = new Amazon.Textract.Model.Document()
                    {
                        Bytes = memoryStream
                    };
                }
            }

            var request = new AnalyzeDocumentRequest() { Document = MyDocument, FeatureTypes = new List<string>() { "TABLES" } };
            var result = await client.AnalyzeDocumentAsync(request);

            var specie = "";
            var furType = "";

            float trophyRating = 0;
            float quickKill = 0;
            float integrity = 0;
            float score = 0;

            bool ocrPaseFailure = false;

            if (result.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                try
                {
                    Logger.LogTrace(System.Text.Json.JsonSerializer.Serialize(result));

                    float specieMinLeft = (100f + ((imageWidth - 1920) / 2)) / imageWidth;
                    float specieMinTop = (150f + ((imageHeight - 1080) / 2)) / imageHeight;
                    float specieMaxTop = (230f + ((imageHeight - 1080) / 2)) / imageHeight;

                    float furTypeMinLeft = (300f + ((imageWidth - 1920) / 2)) / imageWidth;
                    float furTypeMinTop = (295f + ((imageHeight - 1080) / 2)) / imageHeight;

                    float trophyRatingMinLeft = (300f + ((imageWidth - 1920) / 2)) / imageWidth;
                    float trophyRatingMinTop = (500f + ((imageHeight - 1080) / 2)) / imageHeight;

                    float quickKillMinLeft = (300f + ((imageWidth - 1920) / 2)) / imageWidth;
                    float quickKillMinTop = (550f + ((imageHeight - 1080) / 2)) / imageHeight;

                    float integrityMinLeft = (300f + ((imageWidth - 1920) / 2)) / imageWidth;
                    float integrityMinTop = (585f + ((imageHeight - 1080) / 2)) / imageHeight;

                    float scoreMinLeft = (475f + ((imageWidth - 1920) / 2)) / imageWidth;
                    float scoreMinTop = (680f + ((imageHeight - 1080) / 2)) / imageHeight;
                    float scoreMaxRight = (600f + ((imageWidth - 1920) / 2)) / imageWidth;

                    try
                    {
                        specie = result.Blocks.FindAll(b => b.BlockType.Value == "LINE" &&
                        ((b.Geometry.BoundingBox.Left + b.Geometry.BoundingBox.Width) < scoreMaxRight)).Find(b =>
                        b.Geometry.BoundingBox.Left > specieMinLeft &&
                        b.Geometry.BoundingBox.Top > specieMinTop &&
                        specieMaxTop > b.Geometry.BoundingBox.Top
                        ).Text;
                    }
                    catch (Exception e)
                    {
                        ocrPaseFailure = true;

                        Logger.LogError(e);
                    }

                    try
                    {
                        furType = result.Blocks.FindAll(b => b.BlockType.Value == "LINE" &&
                        ((b.Geometry.BoundingBox.Left + b.Geometry.BoundingBox.Width) < scoreMaxRight)).Find(b =>
                        (b.Geometry.BoundingBox.Left > furTypeMinLeft) &&
                        (b.Geometry.BoundingBox.Top > furTypeMinTop)
                        ).Text;
                    }
                    catch (Exception e)
                    {
                        ocrPaseFailure = true;

                        Logger.LogError(e);
                    }

                    try
                    {
                        trophyRating = float.Parse(result.Blocks.FindAll(b => b.BlockType.Value == "LINE" &&
                        ((b.Geometry.BoundingBox.Left + b.Geometry.BoundingBox.Width) < scoreMaxRight)).Find(b =>
                        (b.Geometry.BoundingBox.Left > trophyRatingMinLeft) &&
                        (b.Geometry.BoundingBox.Top > trophyRatingMinTop)
                        ).Text);
                    }
                    catch (Exception e)
                    {
                        ocrPaseFailure = true;

                        Logger.LogError(e);
                    }

                    try
                    {
                        quickKill = float.Parse(result.Blocks.FindAll(b => b.BlockType.Value == "LINE" &&
                            ((b.Geometry.BoundingBox.Left + b.Geometry.BoundingBox.Width) < scoreMaxRight)).Find(b =>
                            (b.Geometry.BoundingBox.Left > quickKillMinLeft) &&
                            (b.Geometry.BoundingBox.Top > quickKillMinTop)
                            ).Text.Replace("%", ""));
                    }
                    catch (Exception e)
                    {
                        ocrPaseFailure = true;

                        Logger.LogError(e);
                    }

                    try
                    {
                        integrity = float.Parse(result.Blocks.FindAll(b => b.BlockType.Value == "LINE" &&
                        ((b.Geometry.BoundingBox.Left + b.Geometry.BoundingBox.Width) < scoreMaxRight)).Find(b =>
                        (b.Geometry.BoundingBox.Left > integrityMinLeft) &&
                        (b.Geometry.BoundingBox.Top > integrityMinTop)
                        ).Text.Replace("%", ""));
                    }
                    catch (Exception e)
                    {
                        ocrPaseFailure = true;

                        Logger.LogError(e);
                    }

                    try
                    {
                        score = float.Parse(result.Blocks.FindAll(b => b.BlockType.Value == "LINE" &&
                        ((b.Geometry.BoundingBox.Left + b.Geometry.BoundingBox.Width) < scoreMaxRight)).Find(b =>
                        (b.Geometry.BoundingBox.Left > scoreMinLeft) &&
                        (b.Geometry.BoundingBox.Top > scoreMinTop)).Text);
                    }
                    catch (Exception e)
                    {
                        ocrPaseFailure = true;

                        Logger.LogError(e);
                    }
                }
                catch (Exception e)
                {
                    ocrPaseFailure = true;

                    Logger.LogError(e);
                }
            }

            return new Harvest() { FurType = furType, IntegrityBonus = integrity, QuickKillBonus = quickKill, Score = score, Specie = specie, TrophyRating = trophyRating, RequiresCheck = ocrPaseFailure };
        }
    }
}
