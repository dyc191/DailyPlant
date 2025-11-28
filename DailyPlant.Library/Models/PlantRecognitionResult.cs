using System.Text.Json;
using System.Text.Json.Serialization;

namespace DailyPlant.Library.Models;

public class PlantRecognitionResult
{
    public long LogId { get; set; }
    public List<PlantItem> Result { get; set; }
}

public class PlantItem
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("score")]
    [JsonConverter(typeof(DoubleConverter))]
    public double Score { get; set; }
    
    [JsonPropertyName("baike_info")]
    public BaikeInfo BaikeInfo { get; set; }
}

public class BaikeInfo
{
    [JsonPropertyName("image_url")]
    public string ImageUrl { get; set; }
    
    [JsonPropertyName("description")]
    public string Description { get; set; }
}

public class DoubleConverter : JsonConverter<double>
{
    public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        try
        {
            if (reader.TokenType == JsonTokenType.Number)
                return reader.GetDouble();
            else if (reader.TokenType == JsonTokenType.String)
                if (double.TryParse(reader.GetString(), out double result))
                    return result;
            return 0.0;
        }
        catch
        {
            return 0.0;
        }
    }

    public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}