﻿namespace Corvus.Extensions.CosmosClient.Specs.Common
{
    using System;
    using System.Collections.Generic;

    internal class Person : IEquatable<Person>
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Name { get; set; }

        public DateTimeOffset DateOfBirth { get; set; }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as Person);
        }

        public bool Equals(Person other)
        {
            return other != null &&
                   this.Id == other.Id &&
                   this.Name == other.Name &&
                   this.DateOfBirth.Equals(other.DateOfBirth);
        }

        public override int GetHashCode()
        {
            int hashCode = -605153749;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.Id);
            hashCode = hashCode * -1521134295 + EqualityComparer<DateTimeOffset>.Default.GetHashCode(this.DateOfBirth);
            return hashCode;
        }

        public static bool operator ==(Person left, Person right)
        {
            return EqualityComparer<Person>.Default.Equals(left, right);
        }

        public static bool operator !=(Person left, Person right)
        {
            return !(left == right);
        }
    }
}