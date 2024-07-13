using System;
using System.Collections.Generic;
using System.Reflection;

namespace GameClient.DebugConsoleItems;

/// <summary>
/// Represents a command argument attribute.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
public sealed class ArgumentAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ArgumentAttribute"/> class.
    /// </summary>
    public ArgumentAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArgumentAttribute"/> class.
    /// </summary>
    /// <param name="description">The description of the argument.</param>
    public ArgumentAttribute(string description)
    {
        this.Description = description;
    }

    /// <summary>
    /// Gets the name of the argument.
    /// </summary>
    public string? Name { get; private set; }

    /// <summary>
    /// Gets the description of the argument.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Gets all argument attributes from the specified method.
    /// </summary>
    /// <param name="method">The method to get the argument attributes from.</param>
    /// <returns>The argument attributes of the specified method.</returns>
    public static ArgumentAttribute[] GetAllFromMethod(MethodInfo method)
    {
        var result = new List<ArgumentAttribute>();

        foreach (var parameter in method.GetParameters())
        {
            if (parameter.GetCustomAttribute<ArgumentAttribute>() is { } attribute)
            {
                attribute.Name ??= parameter.Name;
                attribute.Description ??= "No description provided.";
                result.Add(attribute);
            }
        }

        return result.ToArray();
    }
}