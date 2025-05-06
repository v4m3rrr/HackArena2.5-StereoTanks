using GameLogic;
using MonoRivUI;

namespace GameClient.GameSceneComponents;

#if STEREO

/// <summary>
/// Represents a component that is associated with a team.
/// </summary>
/// <param name="team">The team the component is associated with.</param>
internal abstract class TeamBarComponent(Team team) : Component
{
    /// <summary>
    /// Gets the team the component is associated with.
    /// </summary>
    public Team Team { get; } = team;
}

#endif
