using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace ACG_1
{
    public abstract class Object3D
    {
        // Представляет пивот (центр) объекта в трехмерном пространстве.
        // Пивот используется для определения положения и ориентации объекта.
        public Pivot Pivot { get; protected set; }
        
        // Представляет собой операцию перемещения объекта на вектор v в
        // трехмерном пространстве.
        public abstract void Move(Vector3 v);
        
        // Представляет собой операцию вращения объекта на указанный угол
        // angle вокруг указанной оси axis (которая может быть X, Y или Z).
        public abstract void Rotate(float angle, Axis axis);
    }
}
