@setupContainer
@setupGraphRepository

Feature: Add a graph edge
	In order to execute graph queries
	As a developer
	I want to be able to add edges with properties to a client

Scenario: Add an edge between two vertices
	Given I add the following vertices to the graph with label 'Person'
	| Id                                   | First name | Last name | DateOfBirth |
	| 050691a1-f9cf-44ca-b2e1-f2f436fc82b1 | Barry      | Took      | 1928/06/19  |
	| 3e3085d5-62ab-48e8-878b-d0da77e70dc9 | Marty      | Feldman   | 1934/07/08  |
	When I add the following edge to the graph
	| Start Id                             | End Id                               | Id                                   | Label      |
	| 050691a1-f9cf-44ca-b2e1-f2f436fc82b1 | 3e3085d5-62ab-48e8-878b-d0da77e70dc9 | abbb71e0-109d-445e-81b2-bc373882fd41 | works with |
	Then the following out traversals should exist
	| Start Id                             | End Id                               | Id                                   | Label      |
	| 050691a1-f9cf-44ca-b2e1-f2f436fc82b1 | 3e3085d5-62ab-48e8-878b-d0da77e70dc9 | abbb71e0-109d-445e-81b2-bc373882fd41 | works with |
	And the following in traversals should exist
	| Start Id                             | End Id                               | Id                                   | Label      |
	| 3e3085d5-62ab-48e8-878b-d0da77e70dc9 | 050691a1-f9cf-44ca-b2e1-f2f436fc82b1 | abbb71e0-109d-445e-81b2-bc373882fd41 | works with |
	
