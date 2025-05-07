@perFeatureContainer
@setupCosmosDBKeys
@withSharedDatabase
@withHierarchicalPK
Feature: IterationExtensions with Hierarchical PK
	In order to operatore over the results of a query
	As a developer
	I want to be able to iterate the results of a query

Scenario: Iterate a collection with a synchronous action.
	Given that I create a Cosmos Container called "TestContainer"
	And I add a collection of Person objects called "People" to the Cosmos Container called "TestContainer"
		| Index | Tenant                               | Id                                   | Name    | DateOfBirth |
		| 1     | 598042b7-61eb-4813-a4fa-264f72ab49e9 | 22359BAE-D1A7-407F-B560-4FC62027C68E | Tom     | 1972-01-13  |
		| 2     | 598042b7-61eb-4813-a4fa-264f72ab49e9 | 36DBCA33-C10B-4802-A9AA-AB16822A9D25 | Dick    | 1984-05-17  |
		| 3     | 598042b7-61eb-4813-a4fa-264f72ab49e9 | 5BD6C25C-4846-4352-B069-CD75BCA7E41C | Harry   | 1991-10-06  |
		| 4     | 0f06465c-5639-47e2-906e-09c208360e37 | 6ED54E7C-D39C-4A2D-8781-91D15EC047F1 | Darrell | 1933-08-14  |
		| 5     | 0f06465c-5639-47e2-906e-09c208360e37 | EB7DF71C-762E-49D9-BC32-95C608EDE208 | Sally   | 1932-04-09  |
		| 6     | 0f06465c-5639-47e2-906e-09c208360e37 | DC62035F-039D-40EE-8307-BD77CE6FEC67 | Alicia  | 1933-06-12  |
	When I iterate the query "SELECT * FROM People p WHERE p.dateOfBirth < '1934-01-01T00:00:00.000000Z'" against the container called "TestContainer" with a synchronous action and store the Person objects seen in "PersonItemsSeen"
	Then the Person collection "PersonItemsSeen" should contain the following items from the Person collection "People"
		| Index |
		| 4     |
		| 5     |
		| 6     |

Scenario: Iterate a collection with a synchronous action and a batch size of 1
	Given that I create a Cosmos Container called "TestContainer"
	And I add a collection of Person objects called "People" to the Cosmos Container called "TestContainer"
		| Index | Tenant                               | Id                                   | Name    | DateOfBirth |
		| 1     | 598042b7-61eb-4813-a4fa-264f72ab49e9 | 22359BAE-D1A7-407F-B560-4FC62027C68E | Tom     | 1972-01-13  |
		| 2     | 598042b7-61eb-4813-a4fa-264f72ab49e9 | 36DBCA33-C10B-4802-A9AA-AB16822A9D25 | Dick    | 1984-05-17  |
		| 3     | 598042b7-61eb-4813-a4fa-264f72ab49e9 | 5BD6C25C-4846-4352-B069-CD75BCA7E41C | Harry   | 1991-10-06  |
		| 4     | 0f06465c-5639-47e2-906e-09c208360e37 | 6ED54E7C-D39C-4A2D-8781-91D15EC047F1 | Darrell | 1933-08-14  |
		| 5     | 0f06465c-5639-47e2-906e-09c208360e37 | EB7DF71C-762E-49D9-BC32-95C608EDE208 | Sally   | 1932-04-09  |
		| 6     | 0f06465c-5639-47e2-906e-09c208360e37 | DC62035F-039D-40EE-8307-BD77CE6FEC67 | Alicia  | 1933-06-12  |
	When I iterate the query "SELECT * FROM People p WHERE p.dateOfBirth < '1934-01-01T00:00:00.000000Z'" against the container called "TestContainer" with a batch size of "1" and a synchronous action and store the Person objects seen in "PersonItemsSeen"
	Then the Person collection "PersonItemsSeen" should contain the following items from the Person collection "People"
		| Index |
		| 4     |
		| 5     |
		| 6     |

Scenario: Iterate a collection with a synchronous action, a batch size of 1 and a max batch count of 2
	Given that I create a Cosmos Container called "TestContainer"
	And I add a collection of Person objects called "People" to the Cosmos Container called "TestContainer"
		| Index | Tenant                               | Id                                   | Name    | DateOfBirth |
		| 1     | 598042b7-61eb-4813-a4fa-264f72ab49e9 | 22359BAE-D1A7-407F-B560-4FC62027C68E | Tom     | 1972-01-13  |
		| 2     | 598042b7-61eb-4813-a4fa-264f72ab49e9 | 36DBCA33-C10B-4802-A9AA-AB16822A9D25 | Dick    | 1984-05-17  |
		| 3     | 598042b7-61eb-4813-a4fa-264f72ab49e9 | 5BD6C25C-4846-4352-B069-CD75BCA7E41C | Harry   | 1991-10-06  |
		| 4     | 0f06465c-5639-47e2-906e-09c208360e37 | 6ED54E7C-D39C-4A2D-8781-91D15EC047F1 | Darrell | 1933-08-14  |
		| 5     | 0f06465c-5639-47e2-906e-09c208360e37 | EB7DF71C-762E-49D9-BC32-95C608EDE208 | Sally   | 1932-04-09  |
		| 6     | 0f06465c-5639-47e2-906e-09c208360e37 | DC62035F-039D-40EE-8307-BD77CE6FEC67 | Alicia  | 1933-06-12  |
	When I iterate the query "SELECT * FROM People p WHERE p.dateOfBirth < '1934-01-01T00:00:00.000000Z'" against the container called "TestContainer" with a batch size of "1", a max batch count of "2" and a synchronous action and store the Person objects seen in "PersonItemsSeen"
	Then the Person collection "PersonItemsSeen" should contain the following items from the Person collection "People"
		| Index |
		| 4     |
		| 5     |

Scenario: Iterate a collection of Entity Instances with a synchronous action.
	Given that I create a Cosmos Container called "TestContainer"
	And I add a collection of Person objects called "People" to the Cosmos Container called "TestContainer"
		| Index | Tenant                               | Id                                   | Name    | DateOfBirth |
		| 1     | 598042b7-61eb-4813-a4fa-264f72ab49e9 | 22359BAE-D1A7-407F-B560-4FC62027C68E | Tom     | 1972-01-13  |
		| 2     | 598042b7-61eb-4813-a4fa-264f72ab49e9 | 36DBCA33-C10B-4802-A9AA-AB16822A9D25 | Dick    | 1984-05-17  |
		| 3     | 598042b7-61eb-4813-a4fa-264f72ab49e9 | 5BD6C25C-4846-4352-B069-CD75BCA7E41C | Harry   | 1991-10-06  |
		| 4     | 0f06465c-5639-47e2-906e-09c208360e37 | 6ED54E7C-D39C-4A2D-8781-91D15EC047F1 | Darrell | 1933-08-14  |
		| 5     | 0f06465c-5639-47e2-906e-09c208360e37 | EB7DF71C-762E-49D9-BC32-95C608EDE208 | Sally   | 1932-04-09  |
		| 6     | 0f06465c-5639-47e2-906e-09c208360e37 | DC62035F-039D-40EE-8307-BD77CE6FEC67 | Alicia  | 1933-06-12  |
	When I iterate the query "SELECT * FROM People p WHERE p.dateOfBirth < '1934-01-01T00:00:00.000000Z'" against the container called "TestContainer" with a synchronous action and store the Entity Instance of Person objects seen in "PersonItemsSeen"
	Then the Entity Instance of Person collection "PersonItemsSeen" should contain the following items from the Person collection "People"
		| Index |
		| 4     |
		| 5     |
		| 6     |

Scenario: Iterate a collection with an asynchronous action
	Given that I create a Cosmos Container called "TestContainer"
	And I add a collection of Person objects called "People" to the Cosmos Container called "TestContainer"
		| Index | Tenant                               | Id                                   | Name    | DateOfBirth |
		| 1     | 598042b7-61eb-4813-a4fa-264f72ab49e9 | 22359BAE-D1A7-407F-B560-4FC62027C68E | Tom     | 1972-01-13  |
		| 2     | 598042b7-61eb-4813-a4fa-264f72ab49e9 | 36DBCA33-C10B-4802-A9AA-AB16822A9D25 | Dick    | 1984-05-17  |
		| 3     | 598042b7-61eb-4813-a4fa-264f72ab49e9 | 5BD6C25C-4846-4352-B069-CD75BCA7E41C | Harry   | 1991-10-06  |
		| 4     | 0f06465c-5639-47e2-906e-09c208360e37 | 6ED54E7C-D39C-4A2D-8781-91D15EC047F1 | Darrell | 1933-08-14  |
		| 5     | 0f06465c-5639-47e2-906e-09c208360e37 | EB7DF71C-762E-49D9-BC32-95C608EDE208 | Sally   | 1932-04-09  |
		| 6     | 0f06465c-5639-47e2-906e-09c208360e37 | DC62035F-039D-40EE-8307-BD77CE6FEC67 | Alicia  | 1933-06-12  |
	When I iterate the query "SELECT * FROM People p WHERE p.dateOfBirth < '1934-01-01T00:00:00.000000Z'" against the container called "TestContainer" with an asynchronous action and store the Person objects seen in "PersonItemsSeen"
	Then the Person collection "PersonItemsSeen" should contain the following items from the Person collection "People"
		| Index |
		| 4     |
		| 5     |
		| 6     |

Scenario: Iterate a collection with an asynchronous action and a batch size of 1
	Given that I create a Cosmos Container called "TestContainer"
	And I add a collection of Person objects called "People" to the Cosmos Container called "TestContainer"
		| Index | Tenant                               | Id                                   | Name    | DateOfBirth |
		| 1     | 598042b7-61eb-4813-a4fa-264f72ab49e9 | 22359BAE-D1A7-407F-B560-4FC62027C68E | Tom     | 1972-01-13  |
		| 2     | 598042b7-61eb-4813-a4fa-264f72ab49e9 | 36DBCA33-C10B-4802-A9AA-AB16822A9D25 | Dick    | 1984-05-17  |
		| 3     | 598042b7-61eb-4813-a4fa-264f72ab49e9 | 5BD6C25C-4846-4352-B069-CD75BCA7E41C | Harry   | 1991-10-06  |
		| 4     | 0f06465c-5639-47e2-906e-09c208360e37 | 6ED54E7C-D39C-4A2D-8781-91D15EC047F1 | Darrell | 1933-08-14  |
		| 5     | 0f06465c-5639-47e2-906e-09c208360e37 | EB7DF71C-762E-49D9-BC32-95C608EDE208 | Sally   | 1932-04-09  |
		| 6     | 0f06465c-5639-47e2-906e-09c208360e37 | DC62035F-039D-40EE-8307-BD77CE6FEC67 | Alicia  | 1933-06-12  |
	When I iterate the query "SELECT * FROM People p WHERE p.dateOfBirth < '1934-01-01T00:00:00.000000Z'" against the container called "TestContainer" with a batch size of "1" and an asynchronous action and store the Person objects seen in "PersonItemsSeen"
	Then the Person collection "PersonItemsSeen" should contain the following items from the Person collection "People"
		| Index |
		| 4     |
		| 5     |
		| 6     |

Scenario: Iterate a collection with an asynchronous action, a batch size of 1 and a max batch count of 2
	Given that I create a Cosmos Container called "TestContainer"
	And I add a collection of Person objects called "People" to the Cosmos Container called "TestContainer"
		| Index | Tenant                               | Id                                   | Name    | DateOfBirth |
		| 1     | 598042b7-61eb-4813-a4fa-264f72ab49e9 | 22359BAE-D1A7-407F-B560-4FC62027C68E | Tom     | 1972-01-13  |
		| 2     | 598042b7-61eb-4813-a4fa-264f72ab49e9 | 36DBCA33-C10B-4802-A9AA-AB16822A9D25 | Dick    | 1984-05-17  |
		| 3     | 598042b7-61eb-4813-a4fa-264f72ab49e9 | 5BD6C25C-4846-4352-B069-CD75BCA7E41C | Harry   | 1991-10-06  |
		| 4     | 0f06465c-5639-47e2-906e-09c208360e37 | 6ED54E7C-D39C-4A2D-8781-91D15EC047F1 | Darrell | 1933-08-14  |
		| 5     | 0f06465c-5639-47e2-906e-09c208360e37 | EB7DF71C-762E-49D9-BC32-95C608EDE208 | Sally   | 1932-04-09  |
		| 6     | 0f06465c-5639-47e2-906e-09c208360e37 | DC62035F-039D-40EE-8307-BD77CE6FEC67 | Alicia  | 1933-06-12  |
	When I iterate the query "SELECT * FROM People p WHERE p.dateOfBirth < '1934-01-01T00:00:00.000000Z'" against the container called "TestContainer" with a batch size of "1", a max batch count of "2" and an asynchronous action and store the Person objects seen in "PersonItemsSeen"
	Then the Person collection "PersonItemsSeen" should contain the following items from the Person collection "People"
		| Index |
		| 4     |
		| 5     |

Scenario: Iterate a collection of Entity Instances with an asynchronous action.
	Given that I create a Cosmos Container called "TestContainer"
	And I add a collection of Person objects called "People" to the Cosmos Container called "TestContainer"
		| Index | Tenant                               | Id                                   | Name    | DateOfBirth |
		| 1     | 598042b7-61eb-4813-a4fa-264f72ab49e9 | 22359BAE-D1A7-407F-B560-4FC62027C68E | Tom     | 1972-01-13  |
		| 2     | 598042b7-61eb-4813-a4fa-264f72ab49e9 | 36DBCA33-C10B-4802-A9AA-AB16822A9D25 | Dick    | 1984-05-17  |
		| 3     | 598042b7-61eb-4813-a4fa-264f72ab49e9 | 5BD6C25C-4846-4352-B069-CD75BCA7E41C | Harry   | 1991-10-06  |
		| 4     | 0f06465c-5639-47e2-906e-09c208360e37 | 6ED54E7C-D39C-4A2D-8781-91D15EC047F1 | Darrell | 1933-08-14  |
		| 5     | 0f06465c-5639-47e2-906e-09c208360e37 | EB7DF71C-762E-49D9-BC32-95C608EDE208 | Sally   | 1932-04-09  |
		| 6     | 0f06465c-5639-47e2-906e-09c208360e37 | DC62035F-039D-40EE-8307-BD77CE6FEC67 | Alicia  | 1933-06-12  |
	When I iterate the query "SELECT * FROM People p WHERE p.dateOfBirth < '1934-01-01T00:00:00.000000Z'" against the container called "TestContainer" with an asynchronous action and store the Entity Instance of Person objects seen in "PersonItemsSeen"
	Then the Entity Instance of Person collection "PersonItemsSeen" should contain the following items from the Person collection "People"
		| Index |
		| 4     |
		| 5     |
		| 6     |