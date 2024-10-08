using System.IO;

namespace GameClient;

/// <summary>
/// Provides utility methods for working with paths.
/// </summary>
internal static class PathUtils
{
    /// <summary>
    /// Gets the absolute path of the specified path.
    /// </summary>
    /// <param name="path">The path to get the absolute path of.</param>
    /// <returns>The absolute path of the specified path.</returns>
    public static string GetAbsolutePath(string path)
    {
        string assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;

        path = path.Replace('/', Path.DirectorySeparatorChar);
        path = path.Replace('\\', Path.DirectorySeparatorChar);

        path = Path.Combine(Path.GetDirectoryName(assemblyPath)!, path);

        return Path.GetFullPath(path);
    }
}
