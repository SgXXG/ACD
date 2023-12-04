using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ACG_1
{
    public class Primitive : Object3D
    {
        //Локальные вершины
        public Vector3[] LocalVertices { get; protected set; }
        //Индексы вершин
        public int[] VerticesIndexes { get; protected set; }
        //Индексы нормалей
        public int[] NormalsIndexes { get; protected set; }
        //Нормали вершин
        public Vector3[] Normals { get; protected set; }

        public Texture Texture { get; protected set; }

        public Primitive()
        {
            LocalVertices = Array.Empty<Vector3>();
            VerticesIndexes = Array.Empty<int>();
            NormalsIndexes = Array.Empty<int>();
            Normals = Array.Empty<Vector3>();
        }
        public Primitive(Vector3[] lv, int[] verticesIdx, Vector3[] normals, int[] normalsIdx, Texture texture, Pivot p)
        {
            VerticesIndexes = verticesIdx;
            LocalVertices = lv;
            Normals = normals;
            NormalsIndexes = normalsIdx;
            Texture = texture;
            Pivot = p;
        }

        public override void Move(Vector3 v)
        {
            Pivot.Move(v);
        }

        public override void Rotate(float angle, Axis axis)
        {
            Pivot.Rotate(angle, axis);
        }

        public void Scale(float k)
        {
            for (int i = 0; i < LocalVertices.Length; i++)
                LocalVertices[i] *= k;
        }
    }
}
