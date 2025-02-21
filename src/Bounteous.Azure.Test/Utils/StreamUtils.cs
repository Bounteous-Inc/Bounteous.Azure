using System.IO;
using Bounteous.Core.Extensions;

namespace Bounteous.Azure.Test.Utils
{
    public static class StreamUtils
    {
        public static Stream ToJsonStream<T>(this T subject) where T : class
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(subject.ToJson());
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}