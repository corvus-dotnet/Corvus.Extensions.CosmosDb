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

Scenario Outline: Compare EntityInstances for equality
	Given I create a Person with Name "<LeftName>" and DateOfBirth "<LeftDateOfBirth>" called "SamplePersonLeft"
	Given I create a Person with Name "<RightName>" and DateOfBirth "<RightDateOfBirth>" called "SamplePersonRight"
	And I create an EntityInstance for the Person called "SamplePersonLeft" with ETag "<LeftETag>" called "LeftInstance"
	And I create an EntityInstance for the Person called "SamplePersonRight" with ETag "<RightETag>" called "RightInstance"
	Then the Equals comparison of the EntityInstance called "LeftInstance" with the EntityInstance called "RightInstance" should be "<Equal>"
	And the Equals comparison of the EntityInstance called "LeftInstance" with the EntityInstance called "RightInstance" as an object should be "<Equal>"
	And the Equals comparison of the EntityInstance called "LeftInstance" with a null EntityInstance should be "false"
	And the Equals comparison of the EntityInstance called "LeftInstance" with a null object should be "false"
	And the == comparison of the EntityInstance called "LeftInstance" with the EntityInstance called "RightInstance" should be "<Equal>"
	And the != comparison of the EntityInstance called "LeftInstance" with the EntityInstance called "RightInstance" should be not "<Equal>"

	Examples:
		| LeftName | LeftDateOfBirth | LeftETag | RightName | RightDateOfBirth | RightETag | Equal		 |
		| Henry    | 1969-04-13      | 01AFE3   | Henry     | 1969-04-13       | 01AFE3    | true        |
		| null     | 1963-11-23      | null     | null      | 1963-11-23       | null      | true        |
		| Henry    | 1963-11-23      | null     | null      | 1963-11-23       | null      | false       |
		| null     | 1963-11-23      | null     | Henry     | 1963-11-23       | null      | false       |
		| Henry    | 1963-11-23      | 01AFE3   | Henry     | 1963-11-23       | null      | false       |
		| Henry    | 1963-11-23      | null     | Henry     | 1963-11-23       | 01AFE3    | false       |
		| Henry    | 1971-04-22      | 01AFE3   | Henry     | 1969-04-13       | 01AFE3    | false       |

