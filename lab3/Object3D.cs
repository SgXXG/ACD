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
        public Pivot Pivot { get; protected set; }
        public abstract void Move(Vector3 v);
        public abstract void Rotate(float angle, Axis axis);
    }
}
