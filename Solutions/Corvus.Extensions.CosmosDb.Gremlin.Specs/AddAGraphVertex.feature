@setupContainer
@setupGraphRepository

Feature: Add a graph vertex
	In order to execute graph queries
	As a developer
	I want to be able to add a vertex with properties to a client

Scenario: Add a vertex to the graph 
	When I add the following vertices to the graph with label 'Person'
	| Id                                   | First name | Last name | DateOfBirth |
	| 0a836426-3f98-47c7-9887-7b2af1055e8e | Barry      | Took      | 1928/06/19  |
	Then I should be able to get the following vertices
	| Id                                   | First name | Last name | DateOfBirth |
	| 0a836426-3f98-47c7-9887-7b2af1055e8e | Barry      | Took      | 1928/06/19  |

Scenario: Add a vertex to the graph that already exists 
	Given I add the following vertices to the graph with label 'Person'
	| Id                                   | First name | Last name | DateOfBirth |
	| 978a4987-f28d-42d5-a2dc-b2f00e9dd841 | Barry      | Took      | 1928/06/19  |
	When I add the following vertices to the graph with label 'Person'
	| Id                                   | First name | Last name | DateOfBirth |
	| 978a4987-f28d-42d5-a2dc-b2f00e9dd841 | Marty      | Feldman   | 1934/07/08  |
	Then it should throw an 'Corvus.Extensions.CosmosDb.GremlinClientException, Corvus.Extensions.CosmosDb.Gremlin.Abstractions'
	And the GremlinClientException should have an HTTP status code of 'Conflict'

Scenario: Update a vertex that already exists
	Given I add the following vertices to the graph with label 'Person'
	| Id                                   | First name | Last name | DateOfBirth |
	| 4457f710-19ad-486b-9bda-e2ff6cda3a27 | Barry      | Took      | 1928/06/19  |
	When I update the following vertices in the graph
	| Id                                   | First name | Last name | DateOfBirth |
	| 4457f710-19ad-486b-9bda-e2ff6cda3a27 | Marty      | Feldman   | 1934/07/08  |
	Then I should be able to get the following vertices
	| Id                                   | First name | Last name | DateOfBirth |
	| 4457f710-19ad-486b-9bda-e2ff6cda3a27 | Marty      | Feldman   | 1934/07/08  |

Scenario: Update a vertex that doesn't exist
	Given I add the following vertices to the graph with label 'Person'
	| Id                                   | First name | Last name | DateOfBirth |
	| cdc3b049-f194-4470-902f-ced00f26549f | Barry      | Took      | 1928/06/19  |
	When I update the following vertices in the graph
	| Id                                   | First name | Last name | DateOfBirth |
	| 2b9fa76b-0db5-4c20-9579-48141dcb9e26 | Marty      | Feldman   | 1934/07/08  |
	Then it should throw an 'Corvus.Extensions.CosmosDb.GremlinClientException, Corvus.Extensions.CosmosDb.Gremlin.Abstractions'
	And the GremlinClientException should have an HTTP status code of 'NotFound'
	And I should be able to get the following vertices
	| Id                                   | First name | Last name | DateOfBirth |
	| cdc3b049-f194-4470-902f-ced00f26549f | Barry      | Took      | 1928/06/19  |	
