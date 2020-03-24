using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class HouseGeneratorTest : MonoBehaviour
{
    public Tilemap map;
    public Tile room;
    public Tile hall;
    public Tile wall;

    public Vector2Int HouseSize = new Vector2Int(16,20);

    void Start()
    {
        Test();
    }

    public void Test()
    {
        map = map ?? GetComponent<Tilemap>() ?? gameObject.AddComponent<Tilemap>();

        HouseGenerator generator = new HouseGenerator(HouseSize.x, HouseSize.y);

        generator.Generate();

        (Queue<Rect> Rooms, Queue<Rect> Halls, Rect House) = generator.Drawables();

        foreach (Rect Hall in Halls)
        {
            for (int x = Hall.l; x <= Hall.r; ++x)
                for (int y = Hall.t; y <= Hall.b; ++y)
                    map.SetTile(new Vector3Int(x, y, 0), hall);
        }

        foreach (Rect Room in Rooms)
        {
            for (int x = Room.l + 1; x <= Room.r - 1; ++x)
                for (int y = Room.t + 1; y <= Room.b - 1; ++y)
                    map.SetTile(new Vector3Int(x, y, 0), room);

            foreach (Vector3Int v in Room.Edge())
                map.SetTile(v, wall);
        }

        foreach (Vector3Int v in House.Edge())
            map.SetTile(v, wall);
    }
}
