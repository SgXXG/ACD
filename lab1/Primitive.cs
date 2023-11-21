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
        public int[] Indexes { get; protected set; }

        public Primitive()
        {
            LocalVertices = new Vector3[0];
            Indexes = new int[0];
        }
        public Primitive(Vector3[] lv, int[] i, Pivot p)
        {
            Indexes = i;
            LocalVertices = lv;
            Pivot = p;
        }

        // Перемещает объект Primitive в пространстве на величину, заданную вектором v.
        public override void Move(Vector3 v)
        {
            Pivot.Move(v);
        }

        // Выполняет вращение объекта на заданный угол вокруг указанной оси (X, Y, или Z). 
        public override void Rotate(float angle, Axis axis)
        {
            Pivot.Rotate(angle, axis);

        }

        // Масштабирует объект, умножая все его локальные вершины на коэффициент k.
        public void Scale(float k)
        {
            for (int i = 0; i < LocalVertices.Length; i++)
                LocalVertices[i] *= k;
        }
    }
}
