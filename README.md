# AssemblyLoadContext Plugin Model

Example of using AssemblyLoadContext as a plugin model where plugins are dynamically loaded and unloaded. The key to this scenario is making sure that the application and plugin assemblies agree on the same `IPlugin` type. There are a few strategies used in this project:

## Simple AssemblyLoadContext

```csharp
#define SIMPLEALC
```

In this example the `AssemblyLoadContext` type is created directly and the plugin is loaded via `LoadFromAssemblyPath`. This works because there is no special loading logic in this `AssemblyLoadContext`. That means when the plugin DLL accesses the `IPlugin` type it will not be present in the plugin `AssemblyLoadContext`, the runtime will then look in the default `AssemblyLoadContext` and find the type there.

## Custom AssemblyLoadContext

```csharp
#define CUSTOMALC
```

This particular example uses a custom `AssemblyLoadContext` that hooks the `Load` method. In the case the `Plugin.Types` assembly is loaded it will just return the `Assembly` loaded in the default context. This accomplishes the same thing as the simple example. The difference is that in a more complex plugin environment, particularly one where they can load their own dependencies, an application will likely need custom resolution logic and that needs to make sure it handles cases like this.

## Documentation

- [AssemblyLoadContext overview](https://learn.microsoft.com/en-us/dotnet/core/dependency-loading/understanding-assemblyloadcontext)
- [Managed loading algorithm](https://learn.microsoft.com/en-us/dotnet/core/dependency-loading/loading-managed)
