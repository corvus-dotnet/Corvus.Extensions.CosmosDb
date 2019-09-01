
Feature: EntityInstanceFeature
	In order to obtain the ETag for stored entity instances
	As a developer
	I want to be able to read a POCO written to a CosmosDB container as an EntityInstance


Scenario Outline: Serialize a POCO to a document with _etag and deserialize to an EntityInstance
	Given I create a Person with Name "<Name>" and DateOfBirth "<DateOfBirth>" called "SamplePerson"
	And I serialize the Person "SamplePerson" to a document called "SampleDocument" with ETag "<ETag>"
	When I deserialize the document called "SampleDocument" to an EntityInstance called "Result"
	Then the EntityInstance called "Result" should have an Entity with Name "<Name>" and DateOfBirth "<DateOfBirth>" and an ETag "<ETag>"

	Examples: 
	| Name  | DateOfBirth | ETag   |
	| Henry | 1969-04-13  | 01AFE3 |
	| null  | 1963-11-23  | null   |

Scenario Outline: Round-trip serialize an EntityInstance
	Given I create a Person with Name "<Name>" and DateOfBirth "<DateOfBirth>" called "SamplePerson"
	And I create an EntityInstance for the Person called "SamplePerson" with ETag "<ETag>" called "SampleInstance"
	And I serialize the EntityInstance called "SampleInstance" to a document called "SampleDocument"
	When I deserialize the document called "SampleDocument" to an EntityInstance called "Result"
	Then the EntityInstance called "Result" should have an Entity with Name "<Name>" and DateOfBirth "<DateOfBirth>" and an ETag "<ETag>"

	Examples: 
	| Name  | DateOfBirth | ETag   |
	| Henry | 1969-04-13  | 01AFE3 |
	| null  | 1963-11-23  | null   |
