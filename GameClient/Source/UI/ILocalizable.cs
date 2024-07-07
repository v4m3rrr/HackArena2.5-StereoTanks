using System;
using System.Collections.Generic;

namespace GameClient;

/// <summary>
/// Represents a localizable object.
/// </summary>
internal interface ILocalizable
{
    /// <summary>
    /// The list of references to the localizable objects.
    /// </summary>
    /// <remarks>
    /// The list contains weak references to the localizable objects.
    /// </remarks>
    private static readonly List<WeakReference<ILocalizable>> References = new();

    /// <summary>
    /// Refreshes all localizable objects.
    /// </summary>
    public static void RefreshAll()
    {
        for (int i = References.Count - 1; i >= 0; i--)
        {
            if (References[i].TryGetTarget(out ILocalizable? target))
            {
                target.Refresh();
            }
            else
            {
                References.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Adds a reference to the localizable object.
    /// </summary>
    /// <param name="reference">The reference to add.</param>
    public static void AddReference(ILocalizable reference)
    {
        References.Add(new WeakReference<ILocalizable>(reference));
    }

    /// <summary>
    /// Refreshes the localizable object.
    /// </summary>
    public void Refresh();
}
