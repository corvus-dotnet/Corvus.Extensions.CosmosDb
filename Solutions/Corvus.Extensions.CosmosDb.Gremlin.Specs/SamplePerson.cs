// <copyright file="SamplePerson.cs" company="Endjin">
// Copyright (c) Endjin. All rights reserved.
// </copyright>

namespace Endjin.GraphRepository.Specs
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// An example person.
    /// </summary>
    public class SamplePerson : IEquatable<SamplePerson>
    {
        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the date of birth.
        /// </summary>
        public DateTimeOffset DateOfBirth { get; set; }

        /// <summary>
        /// Gets or sets the rating of the person.
        /// </summary>
        public int Rating { get; set; }

        /// <summary>
        /// Gets a value used to work around the fact that CosmosDB won't let us use the ID as the partition id.
        /// </summary>
        public string Partition => this.Id;

        public override bool Equals(object obj)
        {
            return this.Equals(obj as SamplePerson);
        }

        public bool Equals(SamplePerson other)
        {
            return other != null &&
                   this.Id == other.Id &&
                   this.FirstName == other.FirstName &&
                   this.LastName == other.LastName &&
                   this.DateOfBirth.Equals(other.DateOfBirth) &&
                   this.Rating == other.Rating;
        }

        public override int GetHashCode()
        {
            int hashCode = -1348590051;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.Id);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.FirstName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.LastName);
            hashCode = hashCode * -1521134295 + EqualityComparer<DateTimeOffset>.Default.GetHashCode(this.DateOfBirth);
            hashCode = hashCode * -1521134295 + this.Rating.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(SamplePerson left, SamplePerson right)
        {
            return EqualityComparer<SamplePerson>.Default.Equals(left, right);
        }

        public static bool operator !=(SamplePerson left, SamplePerson right)
        {
            return !(left == right);
        }
    }
}
