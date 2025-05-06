// This program has been developed by students from the bachelor Computer Science at Utrecht
// University within the Software Project course.
// 
// © Copyright Utrecht University (Department of Information and Computing Sciences)

using System.Text.Json;
using Xunit;
using RAiD.Net.Helpers;

namespace RAiD.Net.Tests.Helpers
{
    public class RAiDDateConverterTests
    {
        private readonly RAiDDateConverter _converter;
        private readonly JsonSerializerOptions _options;

        public RAiDDateConverterTests()
        {
            _converter = new();
            _options = new()
            {
                Converters = { _converter },
            };
        }

        [Fact]
        public void ConvertsFullDateFormat()
        {
            const string json = "\"2025-08-28\"";
            DateTime result = JsonSerializer.Deserialize<DateTime>(json, _options);

            Assert.Equal(2025, result.Year);
            Assert.Equal(8, result.Month);
            Assert.Equal(28, result.Day);
        }

        [Fact]
        public void ConvertsYearMonthFormat()
        {
            const string json = "\"2025-08\"";
            DateTime result = JsonSerializer.Deserialize<DateTime>(json, _options);

            Assert.Equal(2025, result.Year);
            Assert.Equal(8, result.Month);
            Assert.Equal(1, result.Day);
        }

        [Fact]
        public void ConvertsYearOnlyFormat()
        {
            const string json = "\"2025\"";
            DateTime result = JsonSerializer.Deserialize<DateTime>(json, _options);

            Assert.Equal(2025, result.Year);
            Assert.Equal(1, result.Month);
            Assert.Equal(1, result.Day);
        }

        [Fact]
        public void ThrowsExceptionForNullString()
        {
            const string json = "null";

            Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<DateTime>(json, _options));
        }

        [Fact]
        public void ThrowsExceptionForInvalidFormatString()
        {
            const string json = "\"invalid-date\"";

            Assert.Throws<FormatException>(() => JsonSerializer.Deserialize<DateTime>(json, _options));
        }

        [Fact]
        public void SerializesDateTimeToFullDateFormat()
        {
            DateTime date = new(2025, 8, 28, 0 ,0,0, DateTimeKind.Utc);
            string result = JsonSerializer.Serialize(date, _options);

            Assert.Equal("\"2025-08-28\"", result);
        }

        [Fact]
        public void SerializesDateTimeWithTimeComponentCorrectly()
        {
            DateTime date = new(2025, 8, 28, 13, 45, 30, DateTimeKind.Utc);
            string result = JsonSerializer.Serialize(date, _options);

            Assert.Equal("\"2025-08-28\"", result);
        }
        
        [Fact]
        public void ThrowsExceptionForEmptyString()
        {
            const string json = "\"\"";

            Assert.Throws<FormatException>(() => JsonSerializer.Deserialize<DateTime>(json, _options));
        }
        
        [Fact]
        public void ThrowsExceptionForUnexpectedLength()
        {
            const string json = "\"2025-8\"";

            Assert.Throws<FormatException>(() => JsonSerializer.Deserialize<DateTime>(json, _options));
        }
    }
}