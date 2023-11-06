using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using TreeEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class Chunks : MonoBehaviour
{
    class Position // класс для расположения игрока
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

    // сколько чанков отрисовывать от игрока (1 -> 9 чанков, 2 -> 25 чанков и т.д.)
    [SerializeField] private int countChunks;
    
    enum Direction { Up, Down, Left, Right }; // тип направления игрока

    private float chunkLength = 10; // длина чанка
    
    private int minCountObjects = 1; // мин и макс кол-во коробок на одном чанке
    private int maxCountObjects = 4;

    private Position currentPosition; // текущая позиция игрока

    private Dictionary<Position, Vector3[]> allChunks = new(); // все чанки, которые мы открывали
    private Dictionary<Position, GameObject> chunksInGame = new(); // чанки, которые мы видим сейчас

    private GameObject CreateChunk(Position position) // создаем чанк в текущей позиции
    {
        GameObject chunk = Instantiate(chunkPrefab, new(position.x * chunkLength,
            0, position.z * chunkLength), transform.rotation, transform);
        chunk.GetComponent<Chunk>().CreateObjects(allChunks[position]);
        chunksInGame[position] = chunk;
        return chunk;
    }

    // создаем случайное расположение коробок и записываем в allChunks для указанной позиции
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

        for (int i = -countChunks; i <= countChunks; i++) // создаем начальные чанки
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

        // создаем класс Position, в конструкторе указываем координаты игрока, перед этим их изменив
        Position position = new((int)((player.position.x > 0 ? 1 : -1) *
            (Math.Abs(player.position.x) + (chunkLength / 2)) / chunkLength),
            (int)((player.position.z > 0 ? 1 : -1) *
            (Math.Abs(player.position.z) + (chunkLength / 2)) / chunkLength));

        if (currentPosition == position) // если мы остались в этом же чанки, то return
            return;

        Direction dir; // узнаем направление, в которое мы сместились
        if (position.x > currentPosition.x)
            dir = Direction.Right;
        else if (position.x < currentPosition.x)
            dir = Direction.Left;
        else if (position.z > currentPosition.z)
            dir = Direction.Up;
        else
            dir = Direction.Down;
        currentPosition = position; // изменяем текущую позицию на новую

        List<Position> needToDestroyChunks = new(); // чанки, которые нужно уничтожить
        List<Position> needToCreateChunks = new(); // чанки, которые нужно создать

        // находим чанки, которые нужно удалить. Также делаем в соответствие другой чанк, который нужно создать
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
        foreach (var destroyChunk in needToDestroyChunks) // удаляем чанки
        {
            Destroy(chunksInGame[destroyChunk]);
            chunksInGame.Remove(destroyChunk);
        }

        foreach (var createChunk in needToCreateChunks) // создаем чанки
        {
            if (!allChunks.ContainsKey(createChunk))
                AddChunkToDictionary(createChunk);
            CreateChunk(createChunk);
        }
    }
}
