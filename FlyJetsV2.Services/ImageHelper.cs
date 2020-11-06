using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace FlyJetsV2.Services
{
    public class ImageHelper
    {

        public static string GetThumbNailImage(string originalImagePath)
        {

            if (File.Exists(originalImagePath))
            {
                // Load image.
                Image image = Image.FromFile(originalImagePath);

                string thumbNailFileName = "thumb_" + Path.GetFileName(originalImagePath);
                // Compute thumbnail size.
                Size thumbnailSize = GetThumbnailSize(image);

                // Get thumbnail.
                Image thumbnail = image.GetThumbnailImage(thumbnailSize.Width,
                    thumbnailSize.Height, null, IntPtr.Zero);

                // Save thumbnail.
                thumbnail.Save(Path.GetDirectoryName(originalImagePath) + "\\" + thumbNailFileName);
                thumbnail.Dispose();
                image.Dispose();

                return thumbNailFileName;
            }
            else
            {
                throw new FileNotFoundException("The provided file is not found", originalImagePath);
            }
        }

        public static Size GetThumbnailSize(Image original)
        {
            // Maximum size of any dimension.
            const int maxPixels = 41;

            // Width and height.
            int originalWidth = original.Width;
            int originalHeight = original.Height;

            // Compute best factor to scale entire image based on larger dimension.
            double factor;
            if (originalWidth > originalHeight)
            {
                factor = (double)maxPixels / originalWidth;
            }
            else
            {
                factor = (double)maxPixels / originalHeight;
            }

            // Return thumbnail size.
            return new Size((int)(originalWidth * factor), (int)(originalHeight * factor));
        }

    }
}
