@setupContainer
@setupCosmosDBKeys

Feature: Configure database level throughput
	In order to maximize the cost efficiency of my repositories
	As a developer
	I want to be able to configure database level throughput

Scenario: Create repositories with database level throughput
	When I create a repository ("SharedThroughPutDb", "Collection1") with database throughput 400 RU/s
	And I create a repository ("SharedThroughPutDb", "Collection2") with database throughput 400 RU/s
	Then it should create a database called "SharedThroughPutDb" with the following collections
	| Collection name | Throughput   |
	| Collection1     | NotSpecified |
	| Collection2     | NotSpecified |
	And the database called "SharedThroughPutDb" should have the throughput 400 RU/s

Scenario: Create repositories and increase the database level throughput
	When I create a repository ("SharedThroughPutDb", "Collection1") with database throughput 400 RU/s
	And I create a repository ("SharedThroughPutDb", "Collection2") with database throughput 800 RU/s
	Then it should create a database called "SharedThroughPutDb" with the following collections
	| Collection name | Throughput   |
	| Collection1     | NotSpecified |
	| Collection2     | NotSpecified |
	And the database called "SharedThroughPutDb" should have the throughput 800 RU/s

Scenario: Create repositories and decrease the database level throughput
	When I create a repository ("SharedThroughPutDb", "Collection1") with database throughput 800 RU/s
	And I create a repository ("SharedThroughPutDb", "Collection2") with database throughput 400 RU/s
	Then it should create a database called "SharedThroughPutDb" with the following collections
	| Collection name | Throughput   |
	| Collection1     | NotSpecified |
	| Collection2     | NotSpecified |
	And the database called "SharedThroughPutDb" should have the throughput 800 RU/s

Scenario: Create a repository with custom throughput in a database with shared throughput
	When I create a repository ("SharedThroughPutDb", "Collection1") with database throughput 400 RU/s
	And  I create a repository ("SharedThroughPutDb", "Collection2") with collection throughput 800 RU/s
	Then it should create a database called "SharedThroughPutDb" with the following collections
	| Collection name | Throughput   |
	| Collection1     | NotSpecified |
	| Collection2     | 800          |
	And the database called "SharedThroughPutDb" should have the throughput 400 RU/s

Scenario: Create a repository with database throughput in a database without shared throughput
	When I create a repository ("NotSharedThroughPutDb", "Collection1") with collection throughput 800 RU/s
	And I create a repository ("NotSharedThroughPutDb", "Collection2") with database throughput 400 RU/s
	Then it should throw a 'System.InvalidOperationException'

Scenario: Set the database offer throughput to a valid value
	Given I create a repository ("SharedThroughPutDb", "Collection1") with database throughput 400 RU/s
	When I set the database offer throughput for the repository ("SharedThroughPutDb", "Collection1") to 1000 ru/s
	Then the database called "SharedThroughPutDb" should have the throughput 1000 RU/s

Scenario: Set the offer throughput to a value which too low
	Given I create a repository ("SharedThroughPutDb", "Collection1") with database throughput 400 RU/s
	When I set the database offer throughput for the repository ("SharedThroughPutDb", "Collection1") to 100 ru/s
	Then it should throw a 'Microsoft.Azure.Documents.DocumentClientException, Microsoft.Azure.DocumentDB.Core'
	And the database called "SharedThroughPutDb" should have the throughput 400 RU/s