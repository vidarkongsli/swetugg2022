using System.Reflection;
using System.Runtime.CompilerServices;

namespace Kongsli.UrlShortener.Test.Helpers;

public static class EmbeddedResource
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static string ReadTestData<T>(string folder, string name)
    {
        var resourceName = $"{typeof(T).Namespace}.TestData.{folder}.{name}";
        using var stream = Assembly.GetCallingAssembly().GetManifestResourceStream(resourceName);
        if (stream == null) throw new Exception($"Embedded resource {resourceName} not found in assembly {typeof(T).Assembly.FullName}.");
        using var streamReader = new StreamReader(stream);
        return streamReader.ReadToEnd();
    }
}