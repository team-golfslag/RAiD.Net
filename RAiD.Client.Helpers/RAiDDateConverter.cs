using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RAiD.Client.Helpers;

public class RAiDDateConverter : JsonConverter<DateTime>
{
    /// <summary>
    /// Converts a date string in the format "yyyy-MM-dd; yyyy-MM; yyyy" to a DateTime object.
    /// example "2025-08-28; 2025-08; 2025"
    /// </summary>
    /// <param name="reader">The JSON reader.</param>
    /// <param name="typeToConvert">The type to convert to.</param>
    /// <param name="options">The JSON serializer options.</param>
    /// <returns>A DateTime object representing the date.</returns>
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? dateString = reader.GetString();
        if (string.IsNullOrEmpty(dateString))
            return DateTime.MinValue;

        string[] dateParts = dateString.Split(';');
        if (dateParts.Length == 0)
            return DateTime.MinValue;

        // Parse the first part of the date string
        if (DateTime.TryParseExact(dateParts[0], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None,
            out DateTime date))
            return date;

        throw new FormatException("Invalid date format");
    }

    /// <summary>
    /// Converts a DateTime object to a JSON string in the format "yyyy-MM-dd; yyyy-MM; yyyy".
    /// example "2025-08-28; 2025-08; 2025"
    /// </summary>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="value">The DateTime object to convert.</param>
    /// <param name="options">The JSON serializer options.</param>
    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("yyyy-MM-dd; yyyy-MM; yyyy"));
    }
}
