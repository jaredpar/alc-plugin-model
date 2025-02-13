﻿// See https://aka.ms/new-console-template for more information

#define SIMPLEALC
// #define CUSTOMALC

using System.Runtime.Loader;
using Plugin.Types;

PluginData? pluginData = null;
while (true)
{
    Console.WriteLine("Choose an option:");
    Console.WriteLine("1. Load Plugin1");
    Console.WriteLine("2. Load Plugin2");
    if (pluginData is not null)
    {
        Console.WriteLine("3. Run Plugin");
        Console.WriteLine("4. Unload Plugin");
    }

    var data = Console.ReadLine();
    if (string.IsNullOrEmpty(data) || !int.TryParse(data.AsSpan(0, 1), out var option))
    {
        Console.WriteLine("Invalid option. Entere a single digit");
        continue;
    }

    switch (option)
    {
        case 1:
            pluginData?.Dispose();
            pluginData = PluginData.Create("Plugin1.dll");
            break;
        case 2:
            pluginData?.Dispose();
            pluginData = PluginData.Create("Plugin2.dll");
            break;
        case 3:
            Console.WriteLine($"Running {pluginData!.Plugins.Count} plugins");
            foreach (var plugin in pluginData.Plugins)
            {
                plugin.Run();
            }
            break;
        case 4:
            pluginData?.Dispose();
            pluginData = null;
            break;
        default:
            Console.WriteLine("Invalid option");
            break;
    }
}

internal sealed partial class PluginData(
    AssemblyLoadContext loadContext,
    List<IPlugin> plugins) : IDisposable
{
    public List<IPlugin> Plugins { get; } = plugins;
    public AssemblyLoadContext LoadContext { get; } = loadContext;

    public void Dispose()
    {
        Plugins.Clear();
        LoadContext.Unload();
    }

    public static partial PluginData Create(string pluginName);
}

#if SIMPLEALC

// This option uses a simple AssemblyLoadContext that relies on default loading logic
// to unify the assembly for IPlugin

internal sealed partial class PluginData
{
    public static partial PluginData Create(string pluginName)
    {
        var pluginPath = Path.Combine(Path.GetDirectoryName(typeof(Program).Assembly.Location)!, pluginName);

        // This works because there is no load logic for the created AssemblyLoadContext. That 
        // means any load request from the plugin DLL will be forwarded to the default context
        // where the plugin DLL is already loaded
        var loadContext = new AssemblyLoadContext(pluginName, isCollectible: true);

        var pluginAssembly = loadContext.LoadFromAssemblyPath(pluginPath);
        var plugins = new List<IPlugin>();
        foreach (var type in pluginAssembly.GetTypes())
        {
            if (typeof(IPlugin).IsAssignableFrom(type))
            {
                var plugin = (IPlugin)Activator.CreateInstance(type)!;
                plugins.Add(plugin);
            }
        }

        return new PluginData(loadContext, plugins);
    }
}

#elif CUSTOMALC

// This option uses a custom AssemblyLoadContext that ensures that the plugin and application agree
// on the IPlugin interface. A custom AssemblyLoadContext is likely going to be necessary if plugins
// can do custom loading like using a utility library

internal sealed partial class PluginData
{
    public static partial PluginData Create(string pluginName)
    {
        var pluginPath = Path.Combine(Path.GetDirectoryName(typeof(Program).Assembly.Location)!, pluginName);
        var pluginTypeAssembly = typeof(IPlugin).Assembly;
        var loadContext = new AssemblyLoadContext(pluginName, isCollectible: true);
        var pluginAssembly = loadContext.LoadFromAssemblyPath(pluginPath);
        var plugins = new List<IPlugin>();
        foreach (var type in pluginAssembly.GetTypes())
        {
            if (typeof(IPlugin).IsAssignableFrom(type))
            {
                var plugin = (IPlugin)Activator.CreateInstance(type)!;
                plugins.Add(plugin);
            }
        }

        return new PluginData(loadContext, plugins);
    }
}

internal sealed class PluginAssemblyLoadContext(Assembly pluginAssembly) 
    : AssemblyLoadContext(pluginAssembly.GetName().Name, isCollectible: true)
{
    internal Assembly PluginAssembly { get; } = pluginAssembly;

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        // This ensures that the plugin and application are using the same Plugin.Types
        // assembly and hence agree on the IPlugin interface
        if (assemblyName.Name == PluginAssembly.GetName().Name)
        {
            return PluginAssembly;
        }

        return null;
    }
}

#else

#error "Pick an option"

#endif