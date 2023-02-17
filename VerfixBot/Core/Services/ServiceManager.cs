namespace VerfixMusic.Core.Managers;

using Microsoft.Extensions.DependencyInjection;
using System;

public class ServiceManager
{
    public static IServiceProvider Provider { get; private set; }

    public static void SetProvider(ServiceCollection collection)
        => Provider = collection.BuildServiceProvider();

    public static T GetService<T>() where T : new()
        => Provider.GetRequiredService<T>();
}
