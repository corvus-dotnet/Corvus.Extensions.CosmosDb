@setupContainer
@setupCosmosDBSqlClient

Feature: Delete an entity from a Cosmos DB collection
	In order to persist POCO entities
	As a developer
	I want to be able to delete an entity from a Cosmos DB collection

Scenario: Delete an entity with a null ID
	When I delete an entity with ID 'null'
	Then it should throw a 'System.ArgumentNullException'
	And the ArgumentNullException applies to the parameter 'id'


Scenario: Delete an entity with an ID that has not been stored
	When I delete an entity with ID 'newguid'
	Then it should throw a 'Microsoft.Azure.Documents.DocumentClientException, Microsoft.Azure.DocumentDB.Core'
	And the DocumentClientException should have an HTTP status code of 'NotFound'

Scenario: Delete an entity with an ID that has been stored
	Given I store the entities
	| Id                                   | Name        | Some value |
	| 3d4e9bb3-b55e-473a-b482-330638cbe680 | Test entity | 3          |
	And I delete an entity with ID '3d4e9bb3-b55e-473a-b482-330638cbe680'
	When I get an entity with ID '3d4e9bb3-b55e-473a-b482-330638cbe680'
	Then it should throw a 'Microsoft.Azure.Documents.DocumentClientException, Microsoft.Azure.DocumentDB.Core'
	And the DocumentClientException should have an HTTP status code of 'NotFound'