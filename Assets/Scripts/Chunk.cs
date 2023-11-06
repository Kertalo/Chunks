using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    // создаем коробки в этом чанке
    public void CreateObjects(Vector3[] positions)
    {
        foreach (Vector3 position in positions)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.SetParent(transform);
            cube.transform.localPosition = position;
        }
    }
}
