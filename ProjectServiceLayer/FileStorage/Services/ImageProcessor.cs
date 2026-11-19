using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace FrontOfficeApplication.ProjectServiceLayer.FileStorage.Services
{
    public class ImageProcessor
    {
        public static bool IsImage( string contentType)
        {
            return contentType.Equals( "image/jpeg", StringComparison.OrdinalIgnoreCase) 
                || contentType.Equals( "image/png", StringComparison.OrdinalIgnoreCase)
                || contentType.Equals( "image/webp", StringComparison.OrdinalIgnoreCase);
        }

        public static async Task<byte[]> ProcessImageAsync( Stream input)
        {
            using var image = await Image.LoadAsync(input);

            const int maxWidth = 1200;
            const int maxHeight = 1200;

            image.Mutate(x =>
                x.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(
                        maxWidth,
                        maxHeight)
                }));

            await using var output = new MemoryStream();

            await image.SaveAsJpegAsync( output, new JpegEncoder { Quality = 80 });

            return output.ToArray();
        }
    }
}
