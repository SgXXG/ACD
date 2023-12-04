using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ACG_1
{
    public class Camera : Object3D
    {
        public int ScreenWidth { get; set; }
        public int ScreenHeight { get; set; }
        // поле зрения камеры по оси Y в радианах
        public float FOV { get; private set; }
        // расстояние до ближней плоскости обзора камеры
        public float Znear { get; private set; }
        // расстояние до дальней плоскости обзора камеры
        public float Zfar { get; private set; }

        public Camera(Vector3 center, float xAngle, float yAngle, float zAngle, float fov, float znear, float zfar, int screenWidth, int screenHeight)
        {
            Pivot = new Pivot(center, xAngle, yAngle, zAngle);
            FOV = fov;
            Znear = znear;
            Zfar = zfar;
            ScreenWidth = screenWidth;
            ScreenHeight = screenHeight;
        }

        public override void Move(Vector3 v)
        {
            Pivot.Center = Vector3.Transform(Pivot.ToLocalCoords(Pivot.Center) + v, Pivot.ModelMatrix());
        }
        public override void Rotate(float angle, Axis axis)
        {
            Pivot.Rotate(angle, axis);
        }

        public Matrix4x4 ViewMatrix()
        {
            Vector3 xAxis = Pivot.XAxis();
            Vector3 yAxis = Pivot.YAxis();
            Vector3 zAxis = Pivot.ZAxis();
            return new Matrix4x4(
                xAxis.X, yAxis.X, zAxis.X, 0,
                xAxis.Y, yAxis.Y, zAxis.Y, 0,
                xAxis.Z, yAxis.Z, zAxis.Z, 0,
                -Vector3.Dot(xAxis, Pivot.Center), -Vector3.Dot(yAxis, Pivot.Center), -Vector3.Dot(zAxis, Pivot.Center), 1.0f
            );
        }

        public Matrix4x4 ProjectionMatrix => new Matrix4x4 //по методичке перевернуто
            (
                ScreenHeight / (float)ScreenWidth / MathF.Tan(FOV / 2.0f), 0, 0, 0,
                0, 1.0f / MathF.Tan(FOV / 2.0f), 0, 0,
                0, 0, Zfar / (Znear - Zfar), -1,
                0, 0, Zfar * Znear / (Znear - Zfar), 0
            );

        /*
        public Matrix4x4 ProjectionMatrix => new Matrix4x4 // из openGL 
                      (
                          ScreenHeight / (float)ScreenWidth / MathF.Tan(FOV / 2.0f), 0, 0, 0,
                          0, 1.0f / MathF.Tan(FOV / 2.0f), 0, 0,
                          0, 0, (Zfar + Znear) / (Zfar - Znear), 1.0f,
                          0, 0,  -2.0f * Zfar / (Zfar - Znear), 0
                      );
        */
       
        public Matrix4x4 ViewportTransformMatrix => new Matrix4x4
        (
            ScreenWidth / 2.0f, 0, 0, 0,
            0, -ScreenHeight / 2.0f, 0, 0,
            0, 0, 1.0f, 0,
            (ScreenWidth + 1.0f) / 2.0f, (ScreenHeight + 1.0f) / 2.0f, 0, 1.0f
        );
    }
}
