@setupContainer
@setupCosmosDBSqlClient

Feature: Store an entity in a Cosmos DB collection
	In order to persist POCO entities
	As a developer
	I want to be able to store an entity in a Cosmos DB collection

Scenario: Store a null entity
	When I store a null entity
	Then it should throw a 'System.ArgumentNullException'
	And the ArgumentNullException applies to the parameter 'item'

Scenario: Store an entity
	When I store an entity
	| Id      | Name  | Some value |
	| newguid | Hello | 7          |
	Then the result should match
	| Id      | Name  | Some value |
	| newguid | Hello | 7          |

Scenario: Insert an entity that does not exist
	When I insert an entity
	| Id                                   | Name  | Some value |
	| 3976796e-9ff5-4ee9-aeea-ac2dceb96af6 | Hello | 7          |
	Then the result should match
	| Id                                   | Name  | Some value |
	| 3976796e-9ff5-4ee9-aeea-ac2dceb96af6 | Hello | 7          |

Scenario: Insert an entity that already exists
	Given I insert an entity
	| Id                                   | Name  | Some value |
	| 7f3a63ad-0756-4376-9515-33ab4c3d92fc | Hello | 7          |
	When I insert an entity
	| Id                                   | Name  | Some value |
	| 7f3a63ad-0756-4376-9515-33ab4c3d92fc | World | 8          |
	Then it should throw a 'Microsoft.Azure.Documents.DocumentClientException, Microsoft.Azure.DocumentDB.Core'
	And the DocumentClientException should have an HTTP status code of 'Conflict'

Scenario: Update an entity using the update method
	Given I store an entity
	| Id                                   | Name  | Some value |
	| 1f504446-e049-42dc-b005-99fbd77e79b7 | First | 1          |
	And I save the ETag as 'FirstVersion'
	And I store an entity
	| Id                                   | Name   | Some value |
	| 1f504446-e049-42dc-b005-99fbd77e79b7 | Second | 2          |
	And I save the ETag as 'SecondVersion'
	When I update an entity with the ETag 'SecondVersion'
	| Id                                   | Name  | Some value |
	| 1f504446-e049-42dc-b005-99fbd77e79b7 | Third | 3          |
	Then the result should match
	| Id                                   | Name  | Some value |
	| 1f504446-e049-42dc-b005-99fbd77e79b7 | Third | 3          |

Scenario: Update an entity with the wrong etag using the update method
	Given I store an entity
	| Id                                   | Name  | Some value |
	| be7883da-62ed-4680-bf49-195ba532bf98 | First | 1          |
	And I save the ETag as 'FirstVersion'
	And I store an entity
	| Id                                   | Name   | Some value |
	| be7883da-62ed-4680-bf49-195ba532bf98 | Second | 2          |
	And I save the ETag as 'SecondVersion'
	When I update an entity with the ETag 'FirstVersion'
	| Id                                   | Name  | Some value |
	| be7883da-62ed-4680-bf49-195ba532bf98 | Third | 3          |
	Then it should throw a 'Microsoft.Azure.Documents.DocumentClientException, Microsoft.Azure.DocumentDB.Core'
	And the DocumentClientException should have an HTTP status code of 'PreconditionFailed'

Scenario: Update an entity with the correct etag
	Given I store an entity
	| Id                                   | Name  | Some value |
	| 1f504446-e049-42dc-b005-99fbd77e79b7 | First | 1          |
	And I save the ETag as 'FirstVersion'
	And I store an entity
	| Id                                   | Name   | Some value |
	| 1f504446-e049-42dc-b005-99fbd77e79b7 | Second | 2          |
	And I save the ETag as 'SecondVersion'
	When I store an entity with the ETag 'SecondVersion'
	| Id                                   | Name  | Some value |
	| 1f504446-e049-42dc-b005-99fbd77e79b7 | Third | 3          |
	Then the result should match
	| Id      | Name  | Some value |
	| 1f504446-e049-42dc-b005-99fbd77e79b7 | Third | 3          |

Scenario: Update an entity with the wrong etag
	Given I store an entity
	| Id                                   | Name  | Some value |
	| be7883da-62ed-4680-bf49-195ba532bf98 | First | 1          |
	And I save the ETag as 'FirstVersion'
	And I store an entity
	| Id                                   | Name   | Some value |
	| be7883da-62ed-4680-bf49-195ba532bf98 | Second | 2          |
	And I save the ETag as 'SecondVersion'
	When I store an entity with the ETag 'FirstVersion'
	| Id                                   | Name  | Some value |
	| be7883da-62ed-4680-bf49-195ba532bf98 | Third | 3          |
	Then it should throw a 'Microsoft.Azure.Documents.DocumentClientException, Microsoft.Azure.DocumentDB.Core'
	And the DocumentClientException should have an HTTP status code of 'PreconditionFailed'