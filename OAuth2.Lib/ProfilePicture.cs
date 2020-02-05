using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Text;

namespace OfficeClip.OpenSource.OAuth2.Lib
{
    public class ProfilePicture
    {
        public string DirectLink { get; private set; }
        public string Base64String { get; private set; }
        public string ContentType { get; private set; }
        public string HtmlPart { get; private set; }
        public Image Image { get; private set; }

        public ProfilePicture(string link, bool isDirectLink)
        {
            if (isDirectLink)
            {
                DirectLink = link;
            }
            ExtractImageInformation(link);            
        }

        private void ExtractImageInformation(string link)
        {
            try
            {
                Base64String = ConvertImageURLToBase64(link);
                SetHtmlPart();
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not convert the profile picture url to base64: {ex.Message}");
            }
        }

        private void SetHtmlPart()
        {
            HtmlPart = $"<img alt=\"Profile Picture\" src=\"data:{ContentType};base64,{Base64String}\" />";
        }

        private string ConvertImageURLToBase64(string url)
        {
            StringBuilder _sb = new StringBuilder();

            byte[] _byte = this.GetImage(url);

            _sb.Append(Convert.ToBase64String(_byte, 0, _byte.Length));

            Image = ConvertByteArrayToImage(_byte);

            return _sb.ToString();
        }

        private byte[] GetImage(string url)
        {
            byte[] buf;

            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.ReadWriteTimeout = 30000;
                HttpWebResponse response = (HttpWebResponse)req.GetResponse();
                var stream = response.GetResponseStream();
                SetContentType(response);
                using (BinaryReader br = new BinaryReader(stream))
                {
                    int len = (int)(response.ContentLength);
                    buf = br.ReadBytes(len);
                    br.Close();
                }
                stream.Close();
                response.Close();
            }
            catch (Exception exp)
            {
                throw new Exception($"Issue with reading image from website: {exp.Message}");
            }

            return buf;
        }

        private void SetContentType(HttpWebResponse response)
        {
            for (int i = 0; i < response.Headers.Count; ++i)
            {
                if (response.Headers.Keys[i] == "Content-Type")
                    ContentType = response.Headers[i];
            }
        }

        private Image ConvertByteArrayToImage(byte[] byteArrayIn)
        {
            Image returnImage;
            using (MemoryStream ms = new MemoryStream(byteArrayIn))
            {
                returnImage = Image.FromStream(ms);
            }
            return returnImage;
        }

        private string ConvertImageToBase64(Image image)
        {
            using (MemoryStream m = new MemoryStream())
            {
                image.Save(m, ImageFormat.Jpeg);
                byte[] imageBytes = m.ToArray();

                // Convert byte[] to Base64 String
                string base64String = Convert.ToBase64String(imageBytes);
                return base64String;
            }
        }

        public void Resize (int pixel)
        {
            Image = ResizeImage(Image, pixel, pixel);
            Base64String = ConvertImageToBase64(Image);
            SetHtmlPart();
        }

        /// <summary>
        /// Resize the image to the specified width and height. 
        /// <see cref="http://stackoverflow.com/a/24199315/89256"/>
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        private static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
    }
}
