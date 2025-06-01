# RedyllaDB

Redis + ScyllaDB = RedyllaDB :heart:

[![NuGet](https://img.shields.io/nuget/v/RedyllaDB.svg)](https://www.nuget.org/packages/RedyllaDB/)
[![Nuget](https://img.shields.io/nuget/dt/RedyllaDB.svg)](https://www.nuget.org/packages/RedyllaDB/)

RedyllaDB is a .NET client for ScyllaDB that mimics the API layer of [StackExchange.Redis](https://stackexchange.github.io/StackExchange.Redis/). The main goal is to provide a way to migrate basic Redis workloads to ScyllaDB with minimal changes to the existing codebase. 

## Getting started

To get started, please install ScyllaDB. For this tutorial, you can spin up a single-node Scylla cluster using the [docker-compose.yml](https://github.com/RedyllaDB/RedyllaDB/blob/main/docker-compose.yml#L14) file located in the root folder.

```yml
docker compose up -d
```

Next, create a new C# or F# project and add a reference to RedyllaDB package.

```bash
dotnet add package RedyllaDB 
```

Below is a basic example of how to use the API.

```csharp
using var scylla = RedyllaDB.ConnectionMultiplexer.Connect("localhost", "demo");
var db = scylla.GetDatabase();

// STRING type
await db.StringSetAsync("key", "value");
var value = await db.StringGetAsync("key");

// set TTL
await db.KeyExpireAsync("key", TimeSpan.FromMinutes(1.0));
var ttl = await db.KeyExpireTimeAsync("key");

await db.KeyDeleteAsync("key");
```