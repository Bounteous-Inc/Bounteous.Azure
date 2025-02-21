using System.IO;
using Bounteous.Core.Extensions;
using Microsoft.AspNetCore.Http;

namespace Bounteous.Azure.Serialization
{
    public static class SerializationExtensions
    {
        public static T FromHttpRequest<T>(this HttpRequest request)
            => new StreamReader(request.Body).ReadToEnd().FromJson<T>();
    }
}