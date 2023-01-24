 
using System.Drawing; 

namespace OpenAI_for_Grasshopper
{
    public static class Utils
    { 
        public static Image Base64ToImage(string base64String)
        {
            if (base64String == null)
                return null;
            byte[] imageBytes = Convert.FromBase64String(base64String);
            using (var ms = new MemoryStream(imageBytes, 0, imageBytes.Length)) {
                Image image = Image.FromStream(ms, true);
                return image;
            }
        }
        public static string ImageToBase64(Image image, System.Drawing.Imaging.ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream()) {
                // Convert Image to byte[]
                image.Save(ms, format);
                byte[] imageBytes = ms.ToArray();

                // Convert byte[] to base 64 string
                string base64String = Convert.ToBase64String(imageBytes);
                return base64String;
            }
        }
    }
}
