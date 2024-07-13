using System;
using System.Reflection;

namespace GameClient.DebugConsoleItems;

/// <summary>
/// Represents an attribute that initializes a command group.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
internal class CommandGroupAttribute : Attribute, ICommand
{
    private string? name;
    private CommandGroupAttribute? group;
    private Type? type;

    private bool isInitialized;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandGroupAttribute"/> class.
    /// </summary>
    public CommandGroupAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandGroupAttribute"/> class.
    /// </summary>
    /// <param name="description">The description of the command group.</param>
    public CommandGroupAttribute(string description)
    {
        this.Description = description;
    }

    /// <summary>
    /// Gets or sets the name of the command group.
    /// </summary>
    public string Name
    {
        get => this.isInitialized
            ? this.name ?? this.type!.Name
            : throw new InvalidOperationException("The command group has not been initialized.");
        set => this.name = value;
    }

    /// <summary>
    /// Gets or sets the description of the command group.
    /// </summary>
    public string Description { get; set; } = "No description provided.";

    /// <summary>
    /// Gets or sets a value indicating whether the command group is case sensitive.
    /// </summary>
    public bool CaseSensitive { get; set; } = false;

    /// <summary>
    /// Gets the group of the command.
    /// </summary>
    public CommandGroupAttribute? Group => this.isInitialized
        ? this.group
        : throw new InvalidOperationException("The command has not been initialized.");

    /// <summary>
    /// Gets the method info of the command.
    /// </summary>
    public Type Type => this.isInitialized
        ? this.type!
        : throw new InvalidOperationException("The command has not been initialized.");

    /// <summary>
    /// Initializes the command group.
    /// </summary>
    /// <param name="type">The type of the command group.</param>
    /// <param name="group">The group of the command group.</param>
    /// <exception cref="InvalidOperationException">
    /// The command has already been initialized
    /// or the type does not have a <see cref="CommandGroupAttribute"/>.
    /// </exception>
    public void Initialize(Type type, CommandGroupAttribute? group)
    {
        if (this.isInitialized)
        {
            throw new InvalidOperationException("The command has already been initialized.");
        }

        if (type.GetCustomAttribute<CommandGroupAttribute>() is null)
        {
            throw new InvalidOperationException("The type does not have a CommandGroupAttribute.");
        }

        this.group = group;
        this.type = type;
        this.isInitialized = true;
    }
}
