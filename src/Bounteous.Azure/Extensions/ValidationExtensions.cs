    using Bounteous.Core.Validations;

namespace Bounteous.Azure.Extensions
{
    public static class ValidationExtensions
    {
        public static void IsValid(this string value)
            => Validate.Begin().IsNotEmpty(value, nameof(value)).Check();

        public static void IsValid<T>(this T value) where T : class
            => Validate.Begin().IsNotNull(value, nameof(value)).Check();
    }
}