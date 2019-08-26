// <copyright file="ByGraphTraversal.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Extensions.CosmosDb.GremlinQuery.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Corvus.ContentHandling.Json;
    using Newtonsoft.Json;

    /// <summary>
    /// A gremlin graph traversal.
    /// </summary>
    /// <typeparam name="TStart">The start of the traversal.</typeparam>
    /// <typeparam name="TEnd">The end of the traversal.</typeparam>
    internal class ByGraphTraversal<TStart, TEnd> : GraphTraversal<TStart, TEnd>
    {
        private readonly string key;

        /// <summary>
        /// Initializes a new instance of the <see cref="ByGraphTraversal{TStart, TEnd}" /> class.
        /// </summary>
        /// <param name="client">The Gremlin client for which this is a traversal.</param>
        /// <param name="parent">The parent of this traversal.</param>
        public ByGraphTraversal(ICosmosDbGremlinClient client, ITraversal parent)
            : base(client, parent)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ByGraphTraversal{TStart, TEnd}"/> class.
        /// </summary>
        /// <param name="client">The Gremlin client for which this is a traversal.</param>
        /// <param name="parent">The parent of this traversal.</param>
        /// <param name="key">The key to use for comparison.</param>
        public ByGraphTraversal(ICosmosDbGremlinClient client, ITraversal parent, Expression<Func<TEnd, string>> key)
            : this(client, parent, GetPropertyNameForExpression(client, key))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ByGraphTraversal{TStart, TEnd}" /> class.
        /// </summary>
        /// <param name="client">The Gremlin client for which this is a traversal.</param>
        /// <param name="parent">The parent of this traversal.</param>
        /// <param name="key">The key to use for comparison.</param>
        public ByGraphTraversal(ICosmosDbGremlinClient client, ITraversal parent, string key)
            : base(client, parent)
        {
            this.key = key;
        }

        /// <inheritdoc />
        public override async Task WriteAsync(ITraversalWriter writer, Dictionary<string, object> bindings)
        {
            if (this.Parent != null)
            {
                await writer.WriteAsync(".").ConfigureAwait(false);
            }

            await writer.WriteAsync("by(").ConfigureAwait(false);

            if (!string.IsNullOrEmpty(this.key))
            {
                string name = GetBindingName(bindings);
                bindings.Add(name, this.key);
                await writer.WriteAsync(name).ConfigureAwait(false);
            }
            else
            {
                // NOP
            }

            await writer.WriteAsync(")").ConfigureAwait(false);
            await base.WriteAsync(writer, bindings).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the json property name for the given property expression.
        /// </summary>
        /// <typeparam name="TEntityType">The type of the entity declaring the property.</typeparam>
        /// <param name="client">The Gremlin client.</param>
        /// <param name="propertyExpression">The expression for a property.</param>
        /// <returns>The string for the property name.</returns>
        /// <remarks>This attempts to consider camel casing and <see cref="JsonPropertyAttribute"/>, but will
        /// fail for more complex schema.</remarks>
        private static string GetPropertyNameForExpression<TEntityType>(ICosmosDbGremlinClient client, Expression<Func<TEntityType, string>> propertyExpression)
        {
            var lambdaExpression = propertyExpression as LambdaExpression;
            MemberExpression memberExpression = lambdaExpression.GetMemberExpression();
            return memberExpression.Member.GetPredictedMemberName(client.JsonSerializerSettings);
        }
    }
}