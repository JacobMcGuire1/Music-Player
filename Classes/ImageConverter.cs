using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Music_thing.Classes
{
    public class ImageConverter : JsonConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var uri = (string)reader.Value;
            // convert base64 to byte array, put that into memory stream and feed to image
            BitmapImage bitmapImage = new BitmapImage(new Uri(uri));

            return bitmapImage;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var image = (BitmapImage)value;

            int h = image.DecodePixelHeight;
            int w = image.DecodePixelWidth;
            // save to memory stream in original format
            
            writer.WriteValue(image.UriSource);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(BitmapImage);
        }
    }
}
