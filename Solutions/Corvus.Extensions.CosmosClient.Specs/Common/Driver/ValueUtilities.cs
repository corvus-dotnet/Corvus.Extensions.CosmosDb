namespace Corvus.Extensions.CosmosClient.Specs.Common.Driver
{
    using System;

    internal static class ValueUtilities
    {
        /// <summary>
        /// Gets a nullable string from a string
        /// </summary>
        /// <param name="nullableString">The string value, or the string <c>null</c> for null.</param>
        /// <returns></returns>
        internal static string GetNullableString(string nullableString)
        {
            return nullableString == "null" ? null : nullableString;
        }

        /// <summary>
        /// Gets a nullable DateTimeOffset from a string
        /// </summary>
        /// <param name="nullableDateTimeOffsetString">The string value that can be parsed to a <see cref="DateTimeOffset"/>, or the string <c>null</c> for null.</param>
        /// <returns></returns>
        internal static DateTimeOffset? GetNullableDateTimeOffset(string nullableDateTimeOffsetString)
        {
            return nullableDateTimeOffsetString == "null" ? null : (DateTimeOffset?)DateTimeOffset.Parse(nullableDateTimeOffsetString);
        }
    }
}
