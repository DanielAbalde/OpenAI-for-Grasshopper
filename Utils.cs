
using System.Drawing;
using static OpenAI.GPT3.ObjectModels.Models;

namespace OpenAI_for_Grasshopper
{
    public enum ModelSubject { TextCompletion, CodeCompletion, ChatCompletion, ImageGeneration, FineTunning, Embedding, SpeechToText, Moderation }

    public static class Utils
    {
        private static Dictionary<ModelSubject, string[]> _models;

        public static Dictionary<ModelSubject, string[]> Models {
            get {
                if(_models == null) {
                    _models = new Dictionary<ModelSubject, string[]>() {
                        { ModelSubject.TextCompletion, new string[]{ "text-davinci-003", "text-davinci-002", "code-davinci-edit-001" } },
                        { ModelSubject.CodeCompletion, new string[]{ "code-davinci-002", "code-cushman-001", "code-davinci-edit-001" } },
                        { ModelSubject.ChatCompletion, new string[]{ "gpt-3.5-turbo", "gpt-3.5-turbo-0301" } },
                        { ModelSubject.ImageGeneration, new string[]{ "dall-e" } },
                        { ModelSubject.FineTunning, new string[]{ "davinci", "curie", "babbage", "ada" } },
                        { ModelSubject.Embedding, new string[]{ "text-embedding-ada-002" } },
                        { ModelSubject.SpeechToText, new string[]{ "whisper-1" } },
                        { ModelSubject.Moderation, new string[]{ "text-moderation-latest", "text-moderation-stable" } }
                    };
                }
                return _models;
            }
        }

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
