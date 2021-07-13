# Release notes for Corvus.Extensions.CosmosDb v3.

## v3.0

Breaking changes:

### Replacement and migration of deprecated Azure SDK libraries

There were two issues in v2.0. First, there were references to two deprecated libraries (`Microsoft.Azure.KeyVault` and `Microsoft.Azure.Services.AppAuthentication`). Second, these libraries had been referenced from the wrong place: `Corvus.Extensions.CosmosClient`. This didn't actually use them, so it never made any sense for it to depend on them—it was only really `Corvus.Testing.CosmosDb.SpecFlow` that needed them. So we've made two sets of related changes:

* `Corvus.Extensions.CosmosClient` no longer references these components:
  * `Microsoft.Azure.KeyVault`
  * `Microsoft.Azure.Services.AppAuthentication`
* `Corvus.Testing.CosmosDb.SpecFlow` also doesn't reference the components listed above, and instead references their replacements:
  * `Azure.Security.KeyVault.Secrets`
  * `Azure.Identity`

