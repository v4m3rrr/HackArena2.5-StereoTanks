using System.Diagnostics;

namespace GameLogic.Networking;

/// <summary>
/// A class that provides extension methods for the <see cref="PacketType"/> enum.
/// </summary>
public static class PacketTypeExtensions
{
    /// <summary>
    /// Determines whether the packet type is part of the specified group.
    /// </summary>
    /// <param name="type">The packet type.</param>
    /// <param name="group">The packet type group.</param>
    /// <returns>
    /// <see langword="true"/> if the packet type is part of the specified group;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsGroup(this PacketType type, PacketType group)
    {
        Debug.Assert(((int)group & 0xF) == 0, "Provided group is not a group");
        return (int)type >> 4 == (int)group >> 4;
    }

    /// <summary>
    /// Determines whether the packet type has a payload.
    /// </summary>
    /// <param name="type">The packet type.</param>
    /// <returns>
    /// <see langword="true"/> if the packet type has a payload;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool HasPayload(this PacketType type)
    {
        return (type & PacketType.HasPayload) == PacketType.HasPayload;
    }
}
