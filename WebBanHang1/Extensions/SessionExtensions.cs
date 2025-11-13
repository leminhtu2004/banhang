using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace WebBanHang1.Extensions
{
    public static class SessionExtensions
    {
        // Phương thức mở rộng để lưu đối tượng vào session dưới dạng JSON
        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            session.SetString(key, JsonSerializer.Serialize(value, options));
        }

        // Phương thức mở rộng để lấy đối tượng từ session dưới dạng JSON
        public static T? GetObjectFromJson<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }
    }
}
