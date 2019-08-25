@setupContainer
@setupCosmosDBSqlClient

Feature: Get an entity from a Cosmos DB collection
	In order to persist POCO entities
	As a developer
	I want to be able to get an entity from a Cosmos DB collection

Scenario: Get an entity with a null ID
	When I get an entity with ID 'null'
	Then it should throw a 'System.ArgumentNullException'
	And the ArgumentNullException applies to the parameter 'id'

	
Scenario: Get a document with a null ID
	When I get a document with ID 'null'
	Then it should throw a 'System.ArgumentNullException'
	And the ArgumentNullException applies to the parameter 'id'

Scenario: Read a document with a null ID
	When I read a document with ID 'null'
	Then it should throw a 'System.ArgumentNullException'
	And the ArgumentNullException applies to the parameter 'id'

Scenario: Get an entity instance with a null ID
	When I get an entity instance with ID 'null'
	Then it should throw a 'System.ArgumentNullException'
	And the ArgumentNullException applies to the parameter 'id'

Scenario: Get an entity by ID that has not been stored
	When I get an entity with ID 'newguid'
	Then it should throw a 'Microsoft.Azure.Documents.DocumentClientException, Microsoft.Azure.DocumentDB.Core'
	And the DocumentClientException should have an HTTP status code of 'NotFound'

Scenario: Get a document by ID that has not been stored
	When I get a document with ID 'newguid'
	Then it should throw a 'Microsoft.Azure.Documents.DocumentClientException, Microsoft.Azure.DocumentDB.Core'
	And the DocumentClientException should have an HTTP status code of 'NotFound'

Scenario: Read a document by ID that has not been stored
	When I read a document with ID 'newguid'
	Then it should throw a 'Microsoft.Azure.Documents.DocumentClientException, Microsoft.Azure.DocumentDB.Core'
	And the DocumentClientException should have an HTTP status code of 'NotFound'

Scenario: Get an entity instance by ID that has not been stored
	When I get an entity instance with ID 'newguid'
	Then it should throw a 'Microsoft.Azure.Documents.DocumentClientException, Microsoft.Azure.DocumentDB.Core'
	And the DocumentClientException should have an HTTP status code of 'NotFound'

Scenario: Get an entity by ID that has been stored
	Given I store the entities
	| Id                                   | Name        | Some value |
	| 3d4e9bb3-b55e-473a-b482-330638cbe680 | Test entity | 3          |
	When I get an entity with ID '3d4e9bb3-b55e-473a-b482-330638cbe680'
	Then it should match the entity
	| Id                                   | Name        | Some value |
	| 3d4e9bb3-b55e-473a-b482-330638cbe680 | Test entity | 3          |

Scenario: Read a document by ID that has been stored
	Given I store the entities
	| Id                                   | Name        | Some value |
	| 3d4e9bb3-b55e-473a-b482-330638cbe680 | Test entity | 3          |
	When I read a document with ID '3d4e9bb3-b55e-473a-b482-330638cbe680'
	Then it should match the document response
	| Id                                   | Name        | Some value |
	| 3d4e9bb3-b55e-473a-b482-330638cbe680 | Test entity | 3          |

Scenario: Get a document by ID that has been stored
	Given I store the entities
	| Id                                   | Name        | Some value |
	| 3d4e9bb3-b55e-473a-b482-330638cbe680 | Test entity | 3          |
	When I get a document with ID '3d4e9bb3-b55e-473a-b482-330638cbe680'
	Then the document should match the storage result

Scenario: Get an entity instance by ID that has been stored
	Given I store the entities
	| Id                                   | Name        | Some value |
	| 3d4e9bb3-b55e-473a-b482-330638cbe680 | Test entity | 3          |
	When I get an entity instance with ID '3d4e9bb3-b55e-473a-b482-330638cbe680'
	Then the entity instance should match the storage result

