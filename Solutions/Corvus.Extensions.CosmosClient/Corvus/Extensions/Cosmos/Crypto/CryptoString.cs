// <copyright file="CryptoString.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Extensions.Cosmos.Crypto
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// Provides useful functions for string generation.
    /// </summary>
    public static class CryptoString
    {
        /// <summary>
        /// Generates a random base-64 encoded string given a specific number of bytes of entropy.
        /// </summary>
        /// <param name="bytesOfEntropy">The number of bytes of entropy.</param>
        /// <returns>A base64-encoded string based on the provided number of bytes of entropy.</returns>
        public static string RandomBase64String(int bytesOfEntropy = 32)
        {
            if (bytesOfEntropy < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bytesOfEntropy), ExceptionMessages.CryptoString_RandomString_LengthCannotBeLessThanZero);
            }

            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] buf = new byte[bytesOfEntropy];
                rng.GetBytes(buf);
                return Convert.ToBase64String(buf);
            }
        }

        /// <summary>
        /// Generates a random string of a specific length containing the specified characters.
        /// </summary>
        /// <param name="length">The length of the string required.</param>
        /// <param name="allowedChars">The characters from which the string is to be formed.</param>
        /// <returns>A randomly generated string.</returns>
        /// <remarks>Allowed characters defaults to <c>A-Z</c>, <c>a-z</c>, and <c>0-9</c>.</remarks>
        public static string RandomString(int length = 32, string allowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789")
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), ExceptionMessages.CryptoString_RandomString_LengthCannotBeLessThanZero);
            }

            if (string.IsNullOrEmpty(allowedChars))
            {
                throw new ArgumentException(ExceptionMessages.CryptoString_RandomString_AllowedCharsCannotBeEmpty, nameof(allowedChars));
            }

            const int byteSize = 0x100;
            char[] allowedCharSet = new HashSet<char>(allowedChars).ToArray();

            if (allowedCharSet.Length > byteSize)
            {
                throw new ArgumentException(string.Format(ExceptionMessages.CryptoString_RandomString_AllowedCharsCannotContainMoreThanCharacters, byteSize, allowedCharSet.Length));
            }

            // Guid.NewGuid and System.Random are not particularly random. By using a
            // cryptographically-secure random number generator, the caller is always
            // protected, regardless of use.
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                var result = new StringBuilder();
                byte[] buf = new byte[128];
                while (result.Length < length)
                {
                    rng.GetBytes(buf);
                    for (int i = 0; i < buf.Length && result.Length < length; ++i)
                    {
                        // Divide the byte array into allowedCharSet-sized groups. If the
                        // random value falls into the last group and the last group is
                        // too small to choose from the entire allowedCharSet, ignore
                        // the value in order to minimize biasing the result.
                        int outOfRangeStart = byteSize - (byteSize % allowedCharSet.Length);
                        if (outOfRangeStart <= buf[i])
                        {
                            continue;
                        }

                        result.Append(allowedCharSet[buf[i] % allowedCharSet.Length]);
                    }
                }

                return result.ToString();
            }
        }
    }
}
