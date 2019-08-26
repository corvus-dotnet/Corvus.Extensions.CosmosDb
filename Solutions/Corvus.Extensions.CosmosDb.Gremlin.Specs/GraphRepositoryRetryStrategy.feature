Feature: GraphRepositoryRetryStrategy
	In order to handle 439 responses from cosmos db
	As a developer
	I want to honor the retry-after header

Scenario: No exception thrown
	Given I execute a gremlin query
	And I have configured my retry options with MaxRetryAttemptsOnThrottledRequests of 5 and MaxRetryWaitTimeInSeconds of 2
	When The query is successful
	Then The operation will not be retried

Scenario: Exception with retry-after header honors retry
	Given I execute a gremlin query
	And I have configured my retry options with MaxRetryAttemptsOnThrottledRequests of 5 and MaxRetryWaitTimeInSeconds of 2
	When The query throws an exception with a status code of 429 and a retry-after header of 0:00:00:01.0000000
	Then The operation will be retried after 1000 millisecond(s)

Scenario: Exception with retry-after header exceeds MaxRetryWaitTimeInSeconds
	Given I execute a gremlin query
	And I have configured my retry options with MaxRetryAttemptsOnThrottledRequests of 5 and MaxRetryWaitTimeInSeconds of 1
	When The query throws an exception with a status code of 429 and a retry-after header of 0:00:00:01.1000000
	Then The operation will not be retried
	Then A GremlinClientException will be thrown

Scenario: Retries exceed MaxRetryAttemptsOnThrottledRequests
	Given I execute a gremlin query
	And I have configured my retry options with MaxRetryAttemptsOnThrottledRequests of 5 and MaxRetryWaitTimeInSeconds of 1
	When The query throws 6 consecutive exceptions with a status code of 429 and a retry-after header of 0:00:00:00.0010000
	Then the operation will be retried 5 times
	And A GremlinClientException will be thrown

Scenario: Transient exceptions use backoff retry
	Given I execute a gremlin query
	When A transient exception is thrown 3 times
	Then the operation will be retried 3 times

Scenario: Transient exceptions thrown more than 5 times
	Given I execute a gremlin query
	When A transient exception is thrown 6 times
	Then the operation will be retried 5 times
	And the transient exception will be thrown

