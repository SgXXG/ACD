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

        public float GetPolygonBrightness(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 LS)
        {
            float brightness = 0;
            Vector3 triangleCenter = new Vector3((v1.X + v2.X + v3.X) / 3.0f, (v1.Y + v2.Y + v3.Y) / 3.0f, (v1.Z + v2.Z + v3.Z) / 3.0f);
            Vector3 lightingVector = Vector3.Normalize(LS - triangleCenter);

            Vector3 normal = VectorMath.GetNormal(v1, v2, v3);

            brightness = VectorMath.Dot(lightingVector, normal);
            if ((brightness < 0) || (float.IsNaN(brightness))) brightness = 0;
            return brightness;
        }
    }
}
