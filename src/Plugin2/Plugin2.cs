using Plugin.Types;
namespace Plugin2;

public class Plugin2 : IPlugin
{
    public void Run() => Console.WriteLine("Plugin2 is running");
}

public class Plugin3: IPlugin
{
    public void Run() => Console.WriteLine("Plugin3 is running");
}
