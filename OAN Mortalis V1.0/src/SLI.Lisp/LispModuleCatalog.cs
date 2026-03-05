using System.Reflection;

namespace SLI.Lisp;

public static class LispModuleCatalog
{
    public static IReadOnlyDictionary<string, string> LoadModules()
    {
        var assembly = typeof(LispModuleCatalog).Assembly;
        var resources = assembly.GetManifestResourceNames()
            .Where(name => name.EndsWith(".lisp", StringComparison.OrdinalIgnoreCase))
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToList();

        var modules = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var resource in resources)
        {
            using var stream = assembly.GetManifestResourceStream(resource) ??
                throw new InvalidOperationException($"Missing embedded Lisp module resource '{resource}'.");
            using var reader = new StreamReader(stream);
            var content = reader.ReadToEnd();
            var fileName = resource.Split('.').Reverse().Take(2).Reverse();
            var moduleName = string.Join(".", fileName);
            modules[moduleName] = content;
        }

        return modules;
    }
}
