// This program has been developed by students from the bachelor Computer Science at Utrecht
// University within the Software Project course.
// 
// © Copyright Utrecht University (Department of Information and Computing Sciences)

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RAiD.Net.Helpers;

public class RAiDDateConverter : JsonConverter<DateTime>
{
    /// <summary>
    /// Converts a date string in one of the following three formats "yyyy-MM-dd"; "yyyy-MM"; "yyyy" to a DateTime object.
    /// example "2025-08-28"; or "2025-08"; or "2025"
    /// </summary>
    /// <param name="reader">The JSON reader.</param>
    /// <param name="typeToConvert">The type to convert to.</param>
    /// <param name="options">The JSON serializer options.</param>
    /// <returns>A DateTime object representing the date.</returns>
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string dateString = reader.GetString() ?? throw new JsonException("Date string is null");

        DateTime dateTime = dateString.Length switch
        {
            7 => DateTime.ParseExact(dateString, "yyyy-MM", CultureInfo.InvariantCulture),
            4 => DateTime.ParseExact(dateString, "yyyy", CultureInfo.InvariantCulture),
            _ => DateTime.ParseExact(dateString, "yyyy-MM-dd", CultureInfo.InvariantCulture),
        };

        return dateTime;
    }

    /// <summary>
    /// Converts a DateTime object to a JSON string in one of the following three formats "yyyy-MM-dd"; "yyyy-MM"; "yyyy".
    /// example "2025-08-28"; or "2025-08"; or "2025"
    /// </summary>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="value">The DateTime object to convert.</param>
    /// <param name="options">The JSON serializer options.</param>
    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        string dateString = value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        writer.WriteStringValue(dateString);
    }
}
