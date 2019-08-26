@setupContainer
@setupGraphRepository

Feature: Folding results
	In order to execute graph queries
	As a developer
	I want to be able to fold the results of the traversal

Scenario: Fold a traversal to a list 
	Given I add the following vertices to the graph with label 'Person'
	| Id                                   | First name | Last name | DateOfBirth | Rating |
	| 02df3455-be17-4975-b7e5-1cc3229e14d5 | Barry      | Took      | 1928/06/19  | 5      |
	| 785e3630-1c14-490a-b4c2-948de1d7190b | Marty      | Feldman   | 1934/07/08  | 6      |
	And I get the nodes with label 'Person' as a traversal called 'traversal1'
	And I fold the traversal called 'traversal1' to a list traversal called 'traversal2'
	When I execute the list traversal called 'traversal2' and store the result in a list called 'list1'
	Then I should be able to get the following vertices from the list called 'list1'
	| Id                                   | First name | Last name | DateOfBirth | Rating |
	| 02df3455-be17-4975-b7e5-1cc3229e14d5 | Barry      | Took      | 1928/06/19  | 5      |
	| 785e3630-1c14-490a-b4c2-948de1d7190b | Marty      | Feldman   | 1934/07/08  | 6      |

Scenario: Fold a traversal to a list with no results
	Given I add the following vertices to the graph with label 'Person'
	| Id                                   | First name | Last name | DateOfBirth | Rating |
	| 02df3455-be17-4975-b7e5-1cc3229e14d5 | Barry      | Took      | 1928/06/19  | 5      |
	| 785e3630-1c14-490a-b4c2-948de1d7190b | Marty      | Feldman   | 1934/07/08  | 6      |
	And I get the nodes with label 'Cow' as a traversal called 'traversal1'
	And I fold the traversal called 'traversal1' to a list traversal called 'traversal2'
	When I execute the list traversal called 'traversal2' and store the result in a list called 'list1'
	Then the list called 'list1' should be empty
