@setupContainer
@setupGraphRepository

Feature: Filter by Has
	In order to execute graph queries
	As a developer
	I want to be able to fold the results of the traversal

Scenario: Filter to a label 
	Given I add the following vertices to the graph with label 'Person'
	| Id                                   | First name | Last name | DateOfBirth | Rating |
	| 02df3455-be17-4975-b7e5-1cc3229e14d5 | Barry      | Took      | 1928/06/19  | 5      |
	| 785e3630-1c14-490a-b4c2-948de1d7190b | Marty      | Feldman   | 1934/07/08  | 6      |
	When I get the nodes with label 'Person' as a traversal called 'traversal1'
	Then I should be able to get the following vertices from the traversal called 'traversal1'
	| Id                                   | First name | Last name | DateOfBirth | Rating |
	| 02df3455-be17-4975-b7e5-1cc3229e14d5 | Barry      | Took      | 1928/06/19  | 5      |
	| 785e3630-1c14-490a-b4c2-948de1d7190b | Marty      | Feldman   | 1934/07/08  | 6      |

Scenario: Filter to a property which exists
Given I add the following vertices to the graph with label 'Person'
	| Id                                   | First name | Last name | DateOfBirth | Rating |
	| 02df3455-be17-4975-b7e5-1cc3229e14d5 | Barry      | Took      | 1928/06/19  | 5      |
	| 785e3630-1c14-490a-b4c2-948de1d7190b | Marty      | Feldman   | 1934/07/08  | 6      |
	When I get the nodes with property 'firstName' as a traversal called 'traversal1'
	Then I should be able to get the following vertices from the traversal called 'traversal1'
	| Id                                   | First name | Last name | DateOfBirth | Rating |
	| 02df3455-be17-4975-b7e5-1cc3229e14d5 | Barry      | Took      | 1928/06/19  | 5      |
	| 785e3630-1c14-490a-b4c2-948de1d7190b | Marty      | Feldman   | 1934/07/08  | 6      |

Scenario: Filter to a property which does not exist
Given I add the following vertices to the graph with label 'Person'
	| Id                                   | First name | Last name | DateOfBirth | Rating |
	| 02df3455-be17-4975-b7e5-1cc3229e14d5 | Barry      | Took      | 1928/06/19  | 5      |
	| 785e3630-1c14-490a-b4c2-948de1d7190b | Marty      | Feldman   | 1934/07/08  | 6      |
	When I get the nodes with property 'nonExistent' as a traversal called 'traversal1'
	Then the traversal called 'traversal1' should be empty

Scenario: Filter to a property with a predicate
Given I add the following vertices to the graph with label 'Person'
	| Id                                   | First name | Last name | DateOfBirth | Rating |
	| 02df3455-be17-4975-b7e5-1cc3229e14d5 | Barry      | Took      | 1928/06/19  | 5      |
	| 785e3630-1c14-490a-b4c2-948de1d7190b | Marty      | Feldman   | 1934/07/08  | 6      |
	When I get the nodes with property 'rating' as a traversal called 'traversal1' with the predicate between(6,7)
	Then I should be able to get the following vertices from the traversal called 'traversal1'
	| Id                                   | First name | Last name | DateOfBirth | Rating |
	| 785e3630-1c14-490a-b4c2-948de1d7190b | Marty      | Feldman   | 1934/07/08  | 6      |

Scenario: Filter to a property with a predicate which produces no results
Given I add the following vertices to the graph with label 'Person'
	| Id                                   | First name | Last name | DateOfBirth | Rating |
	| 02df3455-be17-4975-b7e5-1cc3229e14d5 | Barry      | Took      | 1928/06/19  | 5      |
	| 785e3630-1c14-490a-b4c2-948de1d7190b | Marty      | Feldman   | 1934/07/08  | 6      |
	When I get the nodes with property 'rating' as a traversal called 'traversal1' with the predicate inside(6,8)
	Then the traversal called 'traversal1' should be empty

