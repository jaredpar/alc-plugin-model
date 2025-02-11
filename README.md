# AssemblyLoadContext Plugin Model

Example of using AssemblyLoadContext as a plugin model where plugins are dynamically loaded and unloaded.

The key to this example is making sure that the application and the plugin assemblies all agree on the same `IPlugin` type. This is done by using the same `Assembly` instance for both the default `AssemblyLoadContext` and all of the ones created to manage the plugins.

This particular example uses a custom `AssemblyLoadContext` type but it can likely be accomplished by hooking the resolve event.
