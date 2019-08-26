// <copyright file="MemoryTraversalWriter.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Extensions.CosmosDb.GremlinQuery.Internal
{
    using System.IO;
    using System.Threading.Tasks;

    /// <summary>
    /// A traversal writer over a memory stream.
    /// </summary>
    /// <remarks>
    /// The writer is for one-time use. Once you have converted to a string, the underlying
    /// writer is disposed.
    /// </remarks>
    public class MemoryTraversalWriter : ITraversalWriter
    {
        private readonly StreamWriter streamWriter = new StreamWriter(new MemoryStream());

        /// <inheritdoc/>
        public Task WriteAsync(string traversalContent)
        {
            return this.streamWriter.WriteAsync(traversalContent);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.streamWriter.Dispose();
        }

        /// <summary>
        /// Read the string from the writer.
        /// </summary>
        /// <returns>The string that was written to the writer.</returns>
        /// <remarks>
        /// This reads the string and disposes of the underlying writer.
        /// </remarks>
        public string ReadString()
        {
            this.streamWriter.Flush();

            // This must be OK because the base stream is a memory stream
            this.streamWriter.BaseStream.Seek(0, SeekOrigin.Begin);
            using (var streamReader = new StreamReader(this.streamWriter.BaseStream))
            {
                string result = streamReader.ReadToEnd();
                this.streamWriter.Dispose();
                return result;
            }
        }
    }
}
