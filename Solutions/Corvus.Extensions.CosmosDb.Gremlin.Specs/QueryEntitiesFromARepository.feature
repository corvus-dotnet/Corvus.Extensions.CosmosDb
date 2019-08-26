@setupContainer
@setupGraphRepository
@setup10SampleEntities

Feature: Query entities from a client
	In order to persist POCO entities
	As a developer
	I want to be able to query entities from a client

Scenario: Query entities with a null specification
	When I query entities with a null specification
	Then it should throw a 'System.ArgumentNullException'
	And the ArgumentNullException applies to the parameter 'sqlQuerySpec'

Scenario: Query entities with a null query string
	When I query entities with a null query string
	Then it should throw a 'System.ArgumentNullException'
	And the ArgumentNullException applies to the parameter 'queryText'
	
Scenario: Query all entities with a query string
	When I query entities with the text 'SELECT * FROM Entities e'
	Then there should be as many entities in the result set as in the set that was stored
	And each result should have a matching entity in the set that was stored
		
Scenario: Query all entities with a query string and paging
	When I query 4 pages of entities with the text 'SELECT * FROM Entities e' and a page size of 3
    Then there should be between 1 and 3 entities in each page
	And the total number of entities across the pages should be the same as in the set that was stored
	And each page of results should have matching entities in the set that was stored

Scenario: Query all entities with a query string and paging by continuation token
	When I query 4 pages of entities by continuation token with the text 'SELECT * FROM Entities e' and a page size of 3
    Then there should be between 1 and 3 entities in each page
	And the total number of entities across the pages should be the same as in the set that was stored
	And each page of results should have matching entities in the set that was stored

Scenario: Query entities with particular values
	When I query entities with the text 'SELECT * FROM Entities e WHERE e.someValue IN ({buildDistinctValueParameters})' and supply distinctValueParameters
	Then there should be as many entities in the result set as in the set that was stored
	And each result should have a matching entity in the set that was stored

Scenario: Query entities with particular values with paging
	When I query 4 pages of entities with a page size of 3 with the text 'SELECT * FROM Entities e WHERE e.someValue IN ({buildDistinctValueParameters})' and supply distinctValueParameters
    Then there should be between 1 and 3 entities in each page
	And the total number of entities across the pages should be the same as in the set that was stored
	And each page of results should have matching entities in the set that was stored

Scenario: Query all entities with particular values and paging by continuation token
	When I query 4 pages of entities by continuation token with a page size of 3 with the text 'SELECT * FROM Entities e WHERE e.someValue IN ({buildDistinctValueParameters})' and supply distinctValueParameters
    Then there should be between 1 and 3 entities in each page
	And the total number of entities across the pages should be the same as in the set that was stored
	And each page of results should have matching entities in the set that was stored


Scenario: Query entities with a particular value that has been stored
	When I query entities with the text 'SELECT * FROM Entities e WHERE e.someValue = @value' and supply a value from a stored entity
	Then the results should match the entities stored with that value

Scenario: Query entities with a particular value that has not been stored
	When I query entities with the text 'SELECT * FROM Entities e WHERE e.someValue = @value' and supply a value that is not stored
	Then there should be no entities in the response
