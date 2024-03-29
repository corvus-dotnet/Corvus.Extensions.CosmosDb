# Release notes for Corvus.Extensions.CosmosDb v4.

## v4.1

NuGet upgrades:

* `Microsoft.Azure.Cosmos` 3.25.0 -> 3.26.1
* `Azure.Security.KeyVault.Secrets` 4.2.0 -> 4.3.0
* `Corvus.Identity.Azure` 3.0.0 -> 3.1.0

## v4.0

Breaking changes:

### .NET 6.0

As of v4.0, we target .NET 6.0. Applications requiring older versions of .NET will need to use v3.x of these libraries

### Package name, namespace, and code split changes

The library names were inconsistent with other parts of Corvus, and since we also wanted to change the split of functionality to avoid forcing a dependency on Newtonsoft Json.NET, we have renamed and repartitioned the libraries:

* `Corvus.CosmosClient.Extensions` (formerly `Corvus.Extensions.CosmosClient`)
  * the `ICosmosClientBuilderFactory` type (and also the related new `ICosmosOptionsFactory`)
  * the `ForEachAsync` extension methods for `Container`
* `Corvus.CosmosClient.NewtonSoft.Json.Extensions` (formerly part of `Corvus.Extensions.CosmosClient`)
  * Json.NET-specific implementation of `ICosmosClientBuilderFactory` and `ICosmosOptionsFactory`
* `Corvus.Testing.CosmosDb.SpecFlow` no change—this naming was already consistent with various other `Corvus.Testing.*` libraries

### Support for `Azure.Core` identity in `ICosmosClientBuilderFactory`

This interface has a new method, which would be a breaking change for implementors, but not consumers. We add an overload that supports passing of `Azure.Core`-style credentials to enable access token based authentication as an alternative to an access key.

New features

### Support for `CosmosClientOptions` through `ICosmosOptionsFactory`

The `ICosmosClientBuilderFactory` enabled a class to provided `CosmosClientBuilder`. In typical use, we'd register this in DI with an implementation that performs JSON serialization customization. The problem with this is that there are two mechanisms for configuring `CosmosClient` and no way to map between them. `ICosmosClientBuilderFactory` supports the "fluent" API (`CosmosClientBuilder`) but not the `CosmosClientOptions` class. the new `ICosmosOptionsFactory` provides the same functionality as `ICosmosClientBuilderFactory`, but in a way that works for applications that want to use `CosmosClientOptions`.
