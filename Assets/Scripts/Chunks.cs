using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using TreeEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class Chunks : MonoBehaviour
{
    class Position // ����� ��� ������������ ������
    {
        public int x, z;

        public Position(int x, int z)
        {
            this.x = x;
            this.z = z;
        }

        public override bool Equals(object o)
        {
            if (o is Position p)
                return x == p.x && z == p.z;
            return false;
        }

        public override int GetHashCode()
        {
            return (x.ToString() + z.ToString()).GetHashCode();
        }
    }

    [SerializeField] private GameObject chunkPrefab;
    [SerializeField] private Transform player;

    // ������� ������ ������������ �� ������ (1 -> 9 ������, 2 -> 25 ������ � �.�.)
    [SerializeField] private int countChunks;
    
    enum Direction { Up, Down, Left, Right }; // ��� ����������� ������

    private float chunkLength = 10; // ����� �����
    
    private int minCountObjects = 1; // ��� � ���� ���-�� ������� �� ����� �����
    private int maxCountObjects = 4;

    private Position currentPosition; // ������� ������� ������

    private Dictionary<Position, Vector3[]> allChunks = new(); // ��� �����, ������� �� ���������
    private Dictionary<Position, GameObject> chunksInGame = new(); // �����, ������� �� ����� ������

    private GameObject CreateChunk(Position position) // ������� ���� � ������� �������
    {
        GameObject chunk = Instantiate(chunkPrefab, new(position.x * chunkLength,
            0, position.z * chunkLength), transform.rotation, transform);
        chunk.GetComponent<Chunk>().CreateObjects(allChunks[position]);
        chunksInGame[position] = chunk;
        return chunk;
    }

    // ������� ��������� ������������ ������� � ���������� � allChunks ��� ��������� �������
    private void AddChunkToDictionary(Position position)
    {
        var rand = new System.Random();
        Vector3[] objects = new Vector3[rand.Next(minCountObjects, maxCountObjects + 1)];
        for (int i = 0; i < objects.Length; i++)
            objects[i] = new Vector3((float)(chunkLength * (rand.NextDouble() - 0.5)),
                0, (float)(chunkLength * (rand.NextDouble() - 0.5)));
        allChunks[position] = objects;
    }

    private void Start()
    {
        currentPosition = new(0, 0);

        for (int i = -countChunks; i <= countChunks; i++) // ������� ��������� �����
            for (int j = -countChunks; j <= countChunks; j++)
            {
                Position p = new(i, j);
                AddChunkToDictionary(p);
                CreateChunk(p);
            }
    }

    private void Update()
    {
        if (player == null)
            return;

        // ������� ����� Position, � ������������ ��������� ���������� ������, ����� ���� �� �������
        Position position = new((int)((player.position.x > 0 ? 1 : -1) *
            (Math.Abs(player.position.x) + (chunkLength / 2)) / chunkLength),
            (int)((player.position.z > 0 ? 1 : -1) *
            (Math.Abs(player.position.z) + (chunkLength / 2)) / chunkLength));

        if (currentPosition == position) // ���� �� �������� � ���� �� �����, �� return
            return;

        Direction dir; // ������ �����������, � ������� �� ����������
        if (position.x > currentPosition.x)
            dir = Direction.Right;
        else if (position.x < currentPosition.x)
            dir = Direction.Left;
        else if (position.z > currentPosition.z)
            dir = Direction.Up;
        else
            dir = Direction.Down;
        currentPosition = position; // �������� ������� ������� �� �����

        List<Position> needToDestroyChunks = new(); // �����, ������� ����� ����������
        List<Position> needToCreateChunks = new(); // �����, ������� ����� �������

        // ������� �����, ������� ����� �������. ����� ������ � ������������ ������ ����, ������� ����� �������
        foreach (var chunk in chunksInGame)
        {
            if (dir == Direction.Up && chunk.Key.z == currentPosition.z - countChunks - 1)
            {
                needToCreateChunks.Add(new(chunk.Key.x, currentPosition.z + countChunks));
                needToDestroyChunks.Add(chunk.Key);
            }
            if (dir == Direction.Down && chunk.Key.z == currentPosition.z + countChunks + 1)
            {
                needToCreateChunks.Add(new(chunk.Key.x, currentPosition.z - countChunks));
                needToDestroyChunks.Add(chunk.Key);
            }
            if (dir == Direction.Left && chunk.Key.x == currentPosition.x + countChunks + 1)
            {
                needToCreateChunks.Add(new(currentPosition.x - countChunks, chunk.Key.z));
                needToDestroyChunks.Add(chunk.Key);
            }
            if (dir == Direction.Right && chunk.Key.x == currentPosition.x - countChunks - 1)
            {
                needToCreateChunks.Add(new(currentPosition.x + countChunks, chunk.Key.z));
                needToDestroyChunks.Add(chunk.Key);
            }
        }
        foreach (var destroyChunk in needToDestroyChunks) // ������� �����
        {
            Destroy(chunksInGame[destroyChunk]);
            chunksInGame.Remove(destroyChunk);
        }

        foreach (var createChunk in needToCreateChunks) // ������� �����
        {
            if (!allChunks.ContainsKey(createChunk))
                AddChunkToDictionary(createChunk);
            CreateChunk(createChunk);
        }
    }
}
