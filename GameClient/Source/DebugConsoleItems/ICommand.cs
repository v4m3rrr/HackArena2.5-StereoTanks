namespace GameClient.DebugConsoleItems;

/// <summary>
/// Represents a command for the debug console.
/// </summary>
internal interface ICommand
{
    /// <summary>
    /// Gets the name of the command.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the description of the command.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets a value indicating whether the command is case sensitive.
    /// </summary>
    public bool CaseSensitive { get; }

    /// <summary>
    /// Gets the group of the command.
    /// </summary>
    public CommandGroupAttribute? Group { get; }

    /// <summary>
    /// Gets the full name of the command.
    /// </summary>
    /// <remarks>
    /// The full name of the command is the name of the command group
    /// followed by the name of the command separated by a space.
    /// </remarks>
    public string FullName => this.Group is null
        ? this.DisplayName
        : $"{(this.Group as ICommand).FullName} {this.DisplayName}";

    /// <summary>
    /// Gets the depth of the command.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The depth of the command is the number of command groups
    /// that the command belongs to.
    /// </para>
    /// <para>
    /// Zero means that the command does not belong to any command group.
    /// </para>
    /// </remarks>
    public int Depth => this.Group is null ? 0 : (this.Group as ICommand).Depth + 1;

    /// <summary>
    /// Gets the display name of the command.
    /// </summary>
    /// <remarks>
    /// The display name of the command is the name of the command
    /// in lowercase if the command is not case sensitive.
    /// </remarks>
    public string DisplayName => this.CaseSensitive ? this.Name : this.Name.ToLower();
}
