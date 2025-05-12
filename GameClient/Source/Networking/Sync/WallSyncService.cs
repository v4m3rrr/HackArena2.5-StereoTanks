using GameLogic;

namespace GameClient.Networking;

/// <summary>
/// Synchronizes wall and border sprites with the wall grid in the game logic.
/// </summary>
/// <param name="parent">The grid component containing wall logic state.</param>
/// <param name="getWallGrid">A function to retrieve the current wall sprite grid.</param>
/// <param name="setWallGrid">An action to set a new wall sprite grid.</param>
/// <param name="borderWalls">The list of border wall sprites to synchronize.</param>
internal sealed class WallSyncService(
    GridComponent parent,
    Func<Sprites.Wall.WallWithLogic?[,]> getWallGrid,
    Action<Sprites.Wall.WallWithLogic?[,]> setWallGrid,
    List<Sprites.Wall.Border> borderWalls)
    : ISyncService
{
    /// <inheritdoc/>
    public void Sync()
    {
        var logicGrid = parent.Logic.WallGrid;
        var wallGrid = getWallGrid();

        if (wallGrid.GetLength(0) != logicGrid.GetLength(0) || wallGrid.GetLength(1) != logicGrid.GetLength(1))
        {
            wallGrid = new Sprites.Wall.WallWithLogic?[logicGrid.GetLength(0), logicGrid.GetLength(1)];
            setWallGrid(wallGrid);
        }

        for (int x = 0; x < logicGrid.GetLength(0); x++)
        {
            for (int y = 0; y < logicGrid.GetLength(1); y++)
            {
                var logicWall = logicGrid[x, y];
                if (logicWall is null)
                {
                    wallGrid[x, y] = null;
                }
                else if (wallGrid[x, y] is null)
                {
#if STEREO
                    wallGrid[x, y] = Sprites.Wall.FromType(logicWall, parent);
#else
                    wallGrid[x, y] = new Sprites.Wall.Solid(logicWall, parent);
#endif
                }
                else
                {
                    wallGrid[x, y]!.UpdateLogic(logicWall);
                }
            }
        }

        borderWalls.Clear();
        for (int i = 0; i < parent.Logic.Dim; i++)
        {
            borderWalls.Add(new Sprites.Wall.Border(i, -1, parent));
            borderWalls.Add(new Sprites.Wall.Border(i, parent.Logic.Dim, parent));
            borderWalls.Add(new Sprites.Wall.Border(-1, i, parent));
            borderWalls.Add(new Sprites.Wall.Border(parent.Logic.Dim, i, parent));
        }
    }
}
