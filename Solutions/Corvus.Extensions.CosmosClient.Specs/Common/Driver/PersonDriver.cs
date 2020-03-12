// <copyright file="PersonDriver.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Extensions.CosmosClient.Specs.Common.Driver
{
    using System.Collections.Generic;
    using System.Linq;
    using TechTalk.SpecFlow;

    internal static class PersonDriver
    {
        /// <summary>
        /// Create a sample people from example data.
        /// </summary>
        /// <param name="table">The table from which to create the people. This should have columns <c>Id</c>, <c>Name</c> (which is nullable) and <c>DateOfBirth</c>.</param>
        /// <param name="context">The scenario context (or null if you do not wish to set the value into the context).</param>
        /// <param name="keyToSet">The key to set in the scenario context (or null if you do not wish to set the value into the context).</param>
        /// <returns>The example person created.</returns>
        internal static IList<Person> CreatePeople(Table table, ScenarioContext context = null, string keyToSet = null)
        {
            var people = table.Rows.Select(
                row => new Person
                {
                    Id = row["Id"],
                    Name = ValueUtilities.GetNullableString(row["Name"]),
                    DateOfBirth = ValueUtilities.GetNullableDateTimeOffset(row["DateOfBirth"]).Value,
                }).ToList();

            if (context != null && keyToSet != null)
            {
                context.Set(people, keyToSet);
            }

            return people;
        }

        /// <summary>
        /// Create a sample person from example data.
        /// </summary>
        /// <param name="id">The id of the person.</param>
        /// <param name="name">The name, or "null" for a null value.</param>
        /// <param name="dateOfBirth">The date of birth.</param>
        /// <param name="context">The scenario context (or null if you do not wish to set the value into the context).</param>
        /// <param name="keyToSet">The key to set in the scenario context (or null if you do not wish to set the value into the context).</param>
        /// <returns>The example person created.</returns>
        internal static Person CreatePerson(string id, string name, string dateOfBirth, ScenarioContext context = null, string keyToSet = null)
        {
            var person =
                new Person
                {
                    Id = id,
                    Name = ValueUtilities.GetNullableString(name),
                    DateOfBirth = ValueUtilities.GetNullableDateTimeOffset(dateOfBirth).Value,
                };

            if (context != null && keyToSet != null)
            {
                context.Set(person, keyToSet);
            }

            return person;
        }

        /// <summary>
        /// Gets an ordered list of Person objects based on the indices in a table.
        /// </summary>
        /// <param name="source">The source list.</param>
        /// <param name="indices">A table with a single <c>Index</c> column which defines the indices of the items in the source table.</param>
        /// <returns>A list of Person objects.</returns>
        internal static IList<Person> GetPeopleFromIndices(IList<Person> source, Table indices)
        {
            return indices.Rows.Select(row => source[int.Parse(row["Index"]) - 1]).ToList();
        }
    }
}
