using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace GameClient.DebugConsoleItems;

/// <summary>
/// Represents an attribute that initializes a command.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
internal sealed class CommandAttribute : Attribute, ICommand
{
    private string? name;
    private CommandGroupAttribute? group;
    private MethodInfo? methodInfo;
    private Delegate? action;
    private ArgumentAttribute[]? arguments;

    private bool isInitialized;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandAttribute"/> class.
    /// </summary>
    public CommandAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandAttribute"/> class.
    /// </summary>
    /// <param name="description">The description of the command.</param>
    public CommandAttribute(string description)
    {
        this.Description = description;
    }

    /// <summary>
    /// Gets or sets the name of the command.
    /// </summary>
    public string Name
    {
        get => this.isInitialized
            ? this.name ?? this.methodInfo!.Name
            : throw new InvalidOperationException("The command has not been initialized.");
        set => this.name = value;
    }

    /// <summary>
    /// Gets or sets the description of the command.
    /// </summary>
    public string Description { get; set; } = "No description provided.";

    /// <summary>
    /// Gets or sets a value indicating whether the command is case sensitive.
    /// </summary>
    public bool CaseSensitive { get; set; } = false;

    /// <summary>
    /// Gets the arguments of the command.
    /// </summary>
    public ArgumentAttribute[] Arguments => this.isInitialized
        ? this.arguments!
        : throw new InvalidOperationException("The command has not been initialized.");

    /// <summary>
    /// Gets the group of the command.
    /// </summary>
    public CommandGroupAttribute? Group => this.isInitialized
        ? this.group
        : throw new InvalidOperationException("The command has not been initialized.");

    /// <summary>
    /// Gets the method info of the command.
    /// </summary>
    public MethodInfo MethodInfo => this.isInitialized
        ? this.methodInfo!
        : throw new InvalidOperationException("The command has not been initialized.");

    /// <summary>
    /// Gets the action of the command.
    /// </summary>
    public Delegate Action => this.isInitialized
        ? this.action!
        : throw new InvalidOperationException("The command has not been initialized.");

    /// <summary>
    /// Initializes the command.
    /// </summary>
    /// <param name="method">The method to create the command from.</param>
    /// <param name="group">The group that the command belongs to.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the command has already been initialized
    /// or the method does not have a <see cref="CommandAttribute"/>.
    /// </exception>
    public void Initialize(MethodInfo method, CommandGroupAttribute? group)
    {
        if (this.isInitialized)
        {
            throw new InvalidOperationException("The command has already been initialized.");
        }

        if (method.GetCustomAttribute<CommandAttribute>() is null)
        {
            throw new InvalidOperationException("The method does not have a CommandAttribute.");
        }

        this.group = group;
        this.methodInfo = method;
        this.action = CreateDelegate(method);
        this.arguments = ArgumentAttribute.GetAllFromMethod(method);

        this.isInitialized = true;
    }

    private static Delegate CreateDelegate(MethodInfo method)
    {
        Type[]? parameters = method.GetParameters().Select(p => p.ParameterType).ToArray();
        Type delegateType;

        if (method.ReturnType == typeof(void))
        {
            delegateType = Expression.GetActionType(parameters);
        }
        else
        {
            var parameterTypes = parameters.Append(method.ReturnType).ToArray();
            delegateType = Expression.GetFuncType(parameterTypes);
        }

        return Delegate.CreateDelegate(delegateType, method);
    }
}
