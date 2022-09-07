using System;
using System.Collections.Generic;
using UnityEngine;

public class GameBoard : MonoBehaviour
{
    [SerializeField]
    Transform ground = default;

    [SerializeField]
    GameTile tilePrefab = default;

    [SerializeField]
    bool showGrid, showPaths;

    [SerializeField]
    Texture2D gridTexture = default;

    Vector2Int size;

    GameTileContentFactory contentFactory;

    GameTile[] tiles;

    readonly Queue<GameTile> searchFrontier = new();

    public void Initialize(Vector2Int size, GameTileContentFactory contentFactory)
    {
        this.size = size;
        this.contentFactory = contentFactory;

        ground.localScale = new Vector3(size.x, size.y, 1);

        var offset2d = (Vector2)size * .5f - Vector2.one * .5f;
        var offset3d = new Vector3(offset2d.x, 0, offset2d.y);

        tiles = new GameTile[size.x * size.y];

        for (int i = 0, y = 0; y < size.y; ++y)
        {
            for (int x = 0; x < size.x; ++x, ++i)
            {
                var tile = tiles[i] = Instantiate(tilePrefab);
                tile.transform.SetParent(transform, false);
                tile.transform.localPosition = new Vector3(x, 0, y) - offset3d;

                if (x > 0)
                    GameTile.MakeEastWestNeighbors(tile, tiles[i - 1]);

                if (y > 0)
                    GameTile.MakeNorthSouthNeighbors(tile, tiles[i - size.x]);

                tile.IsAlternative = (x & 1) == 0;
                if ((y & 1) == 0) tile.IsAlternative = !tile.IsAlternative;

                tile.Content = contentFactory.Get(GameTile.GameTileContentType.Empty);
            }
        }

        ToggleDestination(tiles[tiles.Length / 2]);
    }

    bool FindPaths()
    {
        foreach (var tile in tiles)
        {
            if (tile.Content.Type == GameTile.GameTileContentType.Destination)
            {
                tile.BecomeDestination();
                searchFrontier.Enqueue(tile);
            }

            else tile.ClearPath();
        }

        if (searchFrontier.Count == 0) return false;

        while (searchFrontier.Count > 0)
        {
            var tile = searchFrontier.Dequeue();
            if (tile != null)
            {
                var growPathFunctions = new Func<GameTile>[]
                {
                    tile.GrowPathNorth,
                    tile.GrowPathSouth,
                    tile.GrowPathEast,
                    tile.GrowPathWest
                };

                if (!tile.IsAlternative) Array.Reverse(growPathFunctions);

                foreach (var growPathFunction in growPathFunctions)
                {
                    searchFrontier.Enqueue(growPathFunction());
                }
            }
        }

        foreach (var tile in tiles)
        {
            if (!tile.HasPath) return false;
        }

        if (showPaths)
            foreach (var tile in tiles)
            {
                tile.ShowPath();
            }

        return true;
    }

    public void ToggleDestination(GameTile tile)
    {
        if (tile.Content.Type == GameTile.GameTileContentType.Destination)
        {
            tile.Content = contentFactory.Get(GameTile.GameTileContentType.Empty);
            if (!FindPaths())
            {
                tile.Content = contentFactory.Get(GameTile.GameTileContentType.Destination);
                FindPaths();
            }
        }

        else if (tile.Content.Type == GameTile.GameTileContentType.Empty)
        {
            tile.Content = contentFactory.Get(GameTile.GameTileContentType.Destination);
            FindPaths();
        }
    }

    public void ToggleWall(GameTile tile)
    {
        if (tile.Content.Type == GameTile.GameTileContentType.Wall)
        {
            tile.Content = contentFactory.Get(GameTile.GameTileContentType.Empty);
            FindPaths();

        }

        else if (tile.Content.Type == GameTile.GameTileContentType.Empty)
        {
            tile.Content = contentFactory.Get(GameTile.GameTileContentType.Wall);
            if (!FindPaths())
            {
                tile.Content = contentFactory.Get(GameTile.GameTileContentType.Empty);
                FindPaths();
            }
        }
    }

    public GameTile GetTile(Ray ray)
    {
        if (Physics.Raycast(ray, out var hit))
        {
            var x = (int)(hit.point.x + size.x * .5f);
            var y = (int)(hit.point.z + size.y * .5f);

            if (x >= 0 && x < size.x && y >= 0 && y < size.y)
                return tiles[x + y * size.x];
        }

        return null;
    }

    public bool ShowPaths
    {
        get => showPaths;

        set
        {
            showPaths = value;

            foreach (var tile in tiles)
            {
                if (showPaths) tile.ShowPath();
                else tile.HidePath();
            }
        }
    }

    public bool ShowGrid
    {
        get => showGrid;

        set
        {
            showGrid = value;

            var material = ground.GetComponent<MeshRenderer>().material;

            if (showGrid)
            {
                material.mainTexture = gridTexture;
                material.SetTextureScale("_BaseMap", size);
            }

            else material.mainTexture = null;
        }
    }
}