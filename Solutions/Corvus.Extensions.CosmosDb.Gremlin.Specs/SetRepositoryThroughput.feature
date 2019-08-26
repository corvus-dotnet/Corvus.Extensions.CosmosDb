@setupContainer
@setupGraphRepository

Feature: Set offer throughput
	In order to manage the throughput and billing levels
	As a developer
	I want to be able to scale the Client throughput up and down


Scenario: Set the offer throughput to a valid value
	When I set the offer throughput to 1000 ru/s
	Then the result should be 1000 ru/s

Scenario: Set the offer throughput to a value which too low
	Given I set the offer throughput to 1000 ru/s
	When I set the offer throughput to 100 ru/s
	Then the result should be 1000 ru/s