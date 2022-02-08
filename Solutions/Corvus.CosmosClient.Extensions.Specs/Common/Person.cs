// <copyright file="Person.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.CosmosClient.Extensions.Specs.Common
{
    using System;
    using System.Collections.Generic;

    internal class Person : IEquatable<Person>
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string? Name { get; set; }

        public DateTimeOffset? DateOfBirth { get; set; }

        public static bool operator ==(Person? left, Person? right)
        {
            return EqualityComparer<Person>.Default.Equals(left, right);
        }

        public static bool operator !=(Person? left, Person? right)
        {
            return !(left == right);
        }

        public override bool Equals(object? obj)
        {
            return this.Equals(obj as Person);
        }

        public bool Equals(Person? other)
        {
            return other != null &&
                   this.Id == other.Id &&
                   this.Name == other.Name &&
                   this.DateOfBirth.Equals(other.DateOfBirth);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Name, this.Id, this.DateOfBirth);
        }
    }
}