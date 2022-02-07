Feature: EntityInstance
	In order to obtain the ETag for stored entity instances
	As a developer
	I want to be able to read a POCO written to a CosmosDB container as an EntityInstance

Scenario Outline: Serialize a POCO to a document with _etag and deserialize to an EntityInstance
	Given I create a Person with Id "<Id>", Name "<Name>" and DateOfBirth "<DateOfBirth>" called "SamplePerson"
	And I serialize the Person "SamplePerson" to a document called "SampleDocument" with ETag "<ETag>"
	When I deserialize the document called "SampleDocument" to an EntityInstance called "Result"
	Then the EntityInstance called "Result" should have an Entity with Id "<Id>", Name "<Name>" and DateOfBirth "<DateOfBirth>" and an ETag "<ETag>"

	Examples:
		| Id                                   | Name  | DateOfBirth | ETag   |
		| C79B34BA-20E8-4422-A6E5-5F03B1A062F5 | Henry | 1969-04-13  | 01AFE3 |
		| 0C04904C-1679-4801-BA3C-2B92455B6533 | null  | 1963-11-23  | null   |

Scenario Outline: Round-trip serialize an EntityInstance
	Given I create a Person with Id "<Id>", Name "<Name>" and DateOfBirth "<DateOfBirth>" called "SamplePerson"
	And I create an EntityInstance for the Person called "SamplePerson" with ETag "<ETag>" called "SampleInstance"
	And I serialize the EntityInstance called "SampleInstance" to a document called "SampleDocument"
	When I deserialize the document called "SampleDocument" to an EntityInstance called "Result"
	Then the EntityInstance called "Result" should have an Entity with Id "<Id>", Name "<Name>" and DateOfBirth "<DateOfBirth>" and an ETag "<ETag>"

	Examples:
		| Id                                   | Name  | DateOfBirth | ETag   |
		| C79B34BA-20E8-4422-A6E5-5F03B1A062F5 | Henry | 1969-04-13  | 01AFE3 |
		| 0C04904C-1679-4801-BA3C-2B92455B6533 | null  | 1963-11-23  | null   |

Scenario Outline: Compare EntityInstances for equality
	Given I create a Person with Id "<LeftId>", Name "<LeftName>" and DateOfBirth "<LeftDateOfBirth>" called "SamplePersonLeft"
	Given I create a Person with Id "<RightId>", Name "<RightName>" and DateOfBirth "<RightDateOfBirth>" called "SamplePersonRight"
	And I create an EntityInstance for the Person called "SamplePersonLeft" with ETag "<LeftETag>" called "LeftInstance"
	And I create an EntityInstance for the Person called "SamplePersonRight" with ETag "<RightETag>" called "RightInstance"
	Then the Equals comparison of the EntityInstance called "LeftInstance" with the EntityInstance called "RightInstance" should be "<Equal>"
	And the Equals comparison of the EntityInstance called "LeftInstance" with the EntityInstance called "RightInstance" as an object should be "<Equal>"
	And the Equals comparison of the EntityInstance called "LeftInstance" with a null EntityInstance should be "false"
	And the Equals comparison of the EntityInstance called "LeftInstance" with a null object should be "false"
	And the == comparison of the EntityInstance called "LeftInstance" with the EntityInstance called "RightInstance" should be "<Equal>"
	And the != comparison of the EntityInstance called "LeftInstance" with the EntityInstance called "RightInstance" should be not "<Equal>"

	Examples:
		| LeftId                               | LeftName | LeftDateOfBirth | LeftETag | RightId                              | RightName | RightDateOfBirth | RightETag | Equal |
		| C79B34BA-20E8-4422-A6E5-5F03B1A062F5 | Henry    | 1969-04-13      | 01AFE3   | C79B34BA-20E8-4422-A6E5-5F03B1A062F5 | Henry     | 1969-04-13       | 01AFE3    | true  |
		| 0C04904C-1679-4801-BA3C-2B92455B6533 | null     | 1963-11-23      | null     | 0C04904C-1679-4801-BA3C-2B92455B6533 | null      | 1963-11-23       | null      | true  |
		| 6C781E3A-EC13-40BD-83B6-B3DFD7B478A5 | Henry    | 1963-11-23      | null     | 6C781E3A-EC13-40BD-83B6-B3DFD7B478A5 | null      | 1963-11-23       | null      | false |
		| 0874114E-6A73-467A-843A-67CF92B0916E | null     | 1963-11-23      | null     | 0874114E-6A73-467A-843A-67CF92B0916E | Henry     | 1963-11-23       | null      | false |
		| C9E51AB5-D027-43B4-9CAE-2D0D1AB67FB8 | Henry    | 1963-11-23      | 01AFE3   | C9E51AB5-D027-43B4-9CAE-2D0D1AB67FB8 | Henry     | 1963-11-23       | null      | false |
		| 26952722-08AB-4351-AA55-5EDEF780761A | Henry    | 1963-11-23      | null     | 26952722-08AB-4351-AA55-5EDEF780761A | Henry     | 1963-11-23       | 01AFE3    | false |
		| 62CEF04D-026F-40AD-B4D2-0E3BE5D9A1C7 | Henry    | 1971-04-22      | 01AFE3   | 62CEF04D-026F-40AD-B4D2-0E3BE5D9A1C7 | Henry     | 1969-04-13       | 01AFE3    | false |