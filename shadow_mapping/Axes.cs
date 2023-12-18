using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace ACG_1
{
    class Axes : Primitive
    {
        public Axes(Vector3 center, float axesLen)
        {
            float fontSize = axesLen * 0.03f;
            LocalVertices = new Vector3[]
                {
                    new Vector3(0, 0, 0) , //1
                    new Vector3(axesLen, 0, 0) , //2
                    new Vector3(0, axesLen, 0) , //3
                    new Vector3(0, 0, axesLen) , //4
                    new Vector3(axesLen + fontSize, 0, fontSize) , //5
                    new Vector3(axesLen + fontSize * 2.0f, 0, -fontSize) , //6
                    new Vector3(axesLen + fontSize, 0, -fontSize) , //7
                    new Vector3(axesLen + fontSize * 2, 0, fontSize) , //8
                    new Vector3(0, axesLen + fontSize, fontSize) , //9
                    new Vector3(0, axesLen + fontSize * 1.5f, 0) , //10
                    new Vector3(0, axesLen + fontSize * 2.0f, fontSize) , //11
                    new Vector3(0, axesLen + fontSize * 1.5f, -fontSize) , //12
                    new Vector3(fontSize, 0, axesLen + fontSize) , //13
                    new Vector3(fontSize, 0, axesLen + fontSize * 2.0f) , //14
                    new Vector3(-fontSize, 0, axesLen + fontSize) , //15
                    new Vector3(-fontSize, 0, axesLen + fontSize * 2.0f) , //16
        };

            VerticesIndexes = new int[]
                {
                    1, 2, 1,
                    1, 3, 1,
                    1, 4, 1,
                    5, 6, 5,
                    7, 8, 7,
                    9, 10, 9,
                    11, 10, 11,
                    10, 12, 10,
                    13, 14, 13,
                    15, 16, 15,
                    14, 15, 14
                };

            Pivot = new Pivot(new Vector3(0, 0, 0));
        }

    }
}
