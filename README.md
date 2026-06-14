# Bounteous.Azure

Shared Azure helpers for .NET — thin, fluent wrappers over the Azure SDK for
Key Vault secrets and Blob Storage, with first-class dependency-injection
support. Targets **.NET 10**.

## Install

```bash
dotnet add package Bounteous.Azure
```

## Key Vault

`IKeyVault` reads secrets from an Azure Key Vault. It authenticates with
`DefaultAzureCredential` unless you supply your own `TokenCredential`.

```csharp
using Bounteous.Azure.Secrets;

IKeyVault keyVault = new KeyVault()
    .WithVaultName("my-vault");           // https://my-vault.vault.azure.net

// Raw string secret
string apiKey = await keyVault.GetKeyAsync("api-key");

// Strongly-typed secret (deserialized from JSON)
MySettings settings = await keyVault.GetKeyAsync<MySettings>("my-settings");
```

Supply an explicit credential when needed:

```csharp
var keyVault = new KeyVault()
    .WithVaultName("my-vault")
    .WithCredentials(new ManagedIdentityCredential());
```

### Dependency injection

```csharp
using Bounteous.Azure.IoC;

services.AddKeyVault();   // registers IKeyVault -> KeyVault (scoped)
```

## Blob Storage

`IBlobStorage` saves and reads objects as JSON blobs. The target container is
created if it does not already exist.

```csharp
using Bounteous.Azure.Storage;

IBlobStorage storage = await new BlobStorage()
    .ForAccount("mystorageaccount")       // https://mystorageaccount.blob.core.windows.net
    .ForContainer("documents");

await storage.SaveAsync("order-123", order);
var order = await storage.ReadAsync<Order>("order-123");
```

As with Key Vault, authentication defaults to `DefaultAzureCredential` and can
be overridden via `WithCredentials(...)`.

## License

Internal Bounteous shared component.
