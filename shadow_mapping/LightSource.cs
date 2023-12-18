using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ACG_1
{
    class LightSource : Object3D
    {
        public LightSource(Vector3 center) {
            Pivot = new Pivot(center);
        }
        public override void Move(Vector3 v)
        {
            Pivot.Move(v);
        }

        public override void Rotate(float angle, Axis axis)
        {
            Pivot.Rotate(angle, axis);
        }

        public float GetPolygonBrightness(Vector3 v1, Vector3 v2, Vector3 v3, Camera camera)
        {
            float brightness = 0;
            Vector3 triangleCenter = new Vector3((v1.X + v2.X + v3.X) / 3.0f, (v1.Y + v2.Y + v3.Y) / 3.0f, (v1.Z + v2.Z + v3.Z) / 3.0f);
            Vector3 lightingVector = Vector3.Transform(Pivot.Center, camera.ViewMatrix()) - triangleCenter;
            lightingVector = Vector3.Normalize(lightingVector);

            /*Vector3D A = new Vector3D(triangle.v1.X - triangle.v2.X, triangle.v1.Y - triangle.v2.Y, triangle.v1.Z - triangle.v2.Z);
            Vector3D B = new Vector3D(triangle.v2.X - triangle.v3.X, triangle.v2.Y - triangle.v3.Y, triangle.v2.Z - triangle.v3.Z);
            Vector3D normal = Vector3D.CrossProduct(A, B);
            normal.Normalize();*/

            Vector3 normal = VectorMath.GetNormal(v1, v2, v3);

            brightness = VectorMath.Cross(lightingVector, normal);
            if ((brightness < 0) || (float.IsNaN(brightness))) brightness = 0;
            return brightness;
        }
    }
}
