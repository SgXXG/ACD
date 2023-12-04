using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ACG_1
{
    public class Cube : Primitive
    {
        public Cube(Vector3 center, float sideLen)
        {
            float d = sideLen / 2.0f;
            LocalVertices = new Vector3[]
                {
                    new Vector3(center.X - d , center.Y - d, center.Z + d) , //1
                    new Vector3(center.X - d , center.Y - d, center.Z - d) , //2
                    new Vector3(center.X - d , center.Y + d, center.Z + d) , //3
                    new Vector3(center.X - d , center.Y + d, center.Z - d) , //4
                    new Vector3(center.X + d , center.Y - d, center.Z + d) , //5
                    new Vector3(center.X + d , center.Y - d, center.Z - d) , //6
                    new Vector3(center.X + d , center.Y + d, center.Z + d) , //7
                    new Vector3(center.X + d , center.Y + d, center.Z - d) , //8
                };

            VerticesIndexes = new int[]
                {
                    1,2,4 ,
                    1,3,4 ,
                    1,2,6 ,
                    1,5,6 ,
                    5,6,8 ,
                    5,7,8 ,
                    8,4,3 ,
                    8,7,3 ,
                    4,2,8 ,
                    2,8,6 ,
                    3,1,7 ,
                    1,7,5
                };

            Pivot = new Pivot(new Vector3(0, 0, 0));
        }
    }
}