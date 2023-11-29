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
        public Vector3 Center { get; set; }
        public float XAngle { get; set; }
        public float YAngle { get; set; }
        public float ZAngle { get; set; }

        public Pivot(Vector3 center, float xAngle, float yAngle, float zAngle)
        {
            Center = center;
            XAngle = xAngle;
            YAngle = yAngle;
            ZAngle = zAngle;
        }

        public Pivot(Vector3 center)
        {
            Center = center;
            XAngle = 0;
            YAngle = 0;
            ZAngle = 0;
        }
        public void Move(Vector3 v)
        {
            Center += v;
        }

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

        public Vector3 XAxis()
        {
            return VectorMath.Rotate(VectorMath.Rotate(VectorMath.Rotate(Vector3.UnitX, XAngle, Axis.X), YAngle, Axis.Y), ZAngle, Axis.Z);
        }

        public Vector3 YAxis()
        {
            return VectorMath.Rotate(VectorMath.Rotate(VectorMath.Rotate(Vector3.UnitY, XAngle, Axis.X), YAngle, Axis.Y), ZAngle, Axis.Z);
        }

        public Vector3 ZAxis()
        {
            return VectorMath.Rotate(VectorMath.Rotate(VectorMath.Rotate(Vector3.UnitZ, XAngle, Axis.X), YAngle, Axis.Y), ZAngle, Axis.Z);
        }

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

        /*
        public Vector3 ToGlobalCoords(Vector3 local)
        {
            return Vector3.Transform(local, GlobalCoordsMatrix) + Center;
        }
        */
        public Vector3 ToLocalCoords(Vector3 global)
        {
            return Vector3.Transform(global - Center, LocalCoordsMatrix());
        }
        
        public Pivot Clone()
        {
            return new Pivot(Center, XAngle, YAngle, ZAngle);
        }
    }
}
