using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ACG_1
{
    public class Pivot
    {
        // Центр пивота (точку, 
        // относительно которой выполняются операции перемещения и вращения).
        public Vector3 Center { get; set; }
        
        // Углы поворота вокруг осей 
        public float XAngle { get; set; }
        public float YAngle { get; set; }
        public float ZAngle { get; set; }

        // Создает объект Pivot и инициализирует его центр и углы поворота вокруг
        // осей X, Y и Z значениями, переданными в аргументах.
        public Pivot(Vector3 center, float xAngle, float yAngle, float zAngle)
        {
            Center = center;
            XAngle = xAngle;
            YAngle = yAngle;
            ZAngle = zAngle;
        }

        // Создает объект Pivot и инициализирует его центр, а углы устанавливает в 0
        public Pivot(Vector3 center)
        {
            Center = center;
            XAngle = 0;
            YAngle = 0;
            ZAngle = 0;
        }
        
        // Перемещает центр пивота на вектор v
        public void Move(Vector3 v)
        {
            Center += v;
        }

        // Выполняет вращение пивота на заданный угол вокруг указанной оси (X, Y, или Z). 
        public void Rotate(float angle, Axis axis)
        {
            switch (axis)
            {
                case Axis.X:
                    XAngle += angle;
                    break;
                case Axis.Y:
                    YAngle += angle;
                    break;
                case Axis.Z:
                    ZAngle += angle;
                    break;
            }
        }

        // Возвращают направления осей X, Y и Z пивота после применения всех углов поворота.
        public Vector3 XAxis()
        {
            return VectorMath.Rotate(VectorMath.Rotate(VectorMath.Rotate(Vector3.UnitX, XAngle, Axis.X), YAngle, Axis.Y), ZAngle, Axis.Z);
            //return VectorMath.Rotate(VectorMath.Rotate(Vector3.UnitX, XAngle, Axis.X), YAngle, Axis.Y);
        }
        public Vector3 YAxis()
        {
            return VectorMath.Rotate(VectorMath.Rotate(VectorMath.Rotate(Vector3.UnitY, XAngle, Axis.X), YAngle, Axis.Y), ZAngle, Axis.Z);
            //return (VectorMath.Rotate(VectorMath.Rotate(Vector3.UnitY, XAngle, Axis.X), YAngle, Axis.Y));
        }
        public Vector3 ZAxis()
        {
            return VectorMath.Rotate(VectorMath.Rotate(VectorMath.Rotate(Vector3.UnitZ, XAngle, Axis.X), YAngle, Axis.Y), ZAngle, Axis.Z);
        }

        // Возвращает 4x4 матрицу трансформации (модельную матрицу), которая представляет
        // собой комбинацию всех преобразований (перемещение, вращение) пивота.
        public Matrix4x4 ModelMatrix()
        {
            Vector3 xAxis = XAxis();
            Vector3 yAxis = YAxis();
            Vector3 zAxis = ZAxis();
            return new Matrix4x4(
                xAxis.X, xAxis.Y, xAxis.Z, 0,
                yAxis.X, yAxis.Y, yAxis.Z, 0,
                zAxis.X, zAxis.Y, zAxis.Z, 0,
                Center.X, Center.Y, Center.Z, 1.0f
            );
        }

        // Возвращает 4x4 матрицу, которая используется для преобразования координат
        // из глобальных в локальные системы координат пивота.
        public Matrix4x4 LocalCoordsMatrix()
        {
            Vector3 xAxis = XAxis();
            Vector3 yAxis = YAxis();
            Vector3 zAxis = ZAxis();
            return new Matrix4x4(
                xAxis.X, yAxis.X, yAxis.X, 0,
                xAxis.Y, yAxis.Y, yAxis.Y, 0,
                xAxis.Z, yAxis.Z, yAxis.Z, 0,
                0, 0, 0, 1
            );
        }
        
        // Выполняет обратное преобразование координат из глобальных в локальные
        // системы координат пивота
        public Vector3 ToLocalCoords(Vector3 global)
        {
            return Vector3.Transform(global - Center, LocalCoordsMatrix());
        }
        
        // создает и возвращает новый объект Pivot, который является копией текущего пивота,
        // со всеми его параметрами (центр и углы поворота).
        public Pivot Clone()
        {
            return new Pivot(Center, XAngle, YAngle, ZAngle);
        }
    }
}
