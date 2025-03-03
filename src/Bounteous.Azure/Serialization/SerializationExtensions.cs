using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Bounteous.Core.Extensions;
using Microsoft.AspNetCore.Http;

namespace Bounteous.Azure.Serialization
{
    public static class SerializationExtensions
    {
        public static T FromHttpRequest<T>(this HttpRequest request)
            => new StreamReader(request.Body).ReadToEnd().FromJson<T>();

        public static async Task<T> FromJsonAsync<T>(this Stream stream)
            => await JsonSerializer.DeserializeAsync<T>(stream);
    }
}