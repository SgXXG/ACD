using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Globalization;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using System.Drawing;
using System.Timers;
using System.Numerics;
using System.Drawing.Imaging;
using System.Collections.Concurrent;

namespace ACG_1
{
    public partial class MainWindow : Window
    {
        int screenWidth;
        int screenHeight;
        float modelMaxX, modelMaxY, modelMaxZ;
        float modelMinX, modelMinY, modelMinZ;
        float modelSizeX, modelSizeY, modelSizeZ;

        FastBitmap fastBuffer;
        BitmapSource bitmapSource;
        float[] zBuffer;

        Camera camera;
        LightSource lightSource;
        Primitive model;
        Primitive axes;

        System.Drawing.Color ambientColor = System.Drawing.Color.DarkBlue;
        System.Drawing.Color diffuseColor = System.Drawing.Color.LightSteelBlue;
        System.Drawing.Color specularColor = System.Drawing.Color.LightYellow;

        public MainWindow()
        {
            InitializeComponent();
            window.KeyDown += Grid_KeyDown;

            screenWidth = (int)window.Width;
            screenHeight = (int)window.Height;

            fastBuffer = new FastBitmap((int)screenWidth, (int)screenHeight);
            zBuffer = new float[screenWidth * screenHeight];
            Array.Fill<float>(zBuffer, float.MinValue);

            float fNear = 0.1f;
            float fFar = 120000.0f;
            float fFov = 70.0f; // градусы
            //float fAspectRatio = screenHeight / (float)screenWidth;
            float fFovRad = fFov / 180.0f * MathF.PI;

            model = ParseObjFile("Porsche_911_GT2.obj");

            camera = new Camera(new Vector3(0, 0, modelSizeY * 5.0f), 0, 0, 0, fFovRad, fNear, fFar, screenWidth, screenHeight);

            lightSource = new LightSource(new Vector3(modelSizeX * 2, -modelSizeY * 3, modelSizeY * 5.0f));

            //float axesLenght = Math.Max(Math.Max(modelSizeX, modelSizeY), modelSizeZ) * 0.5f;
            //axes = new Axes(new Vector3(0, 0, 0), axesLenght);

            //DispatcherTimer dispatcherTimer = new DispatcherTimer();
            //dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            //dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 5);
            //dispatcherTimer.Start();

            DateTime t1, t2;
            t1 = DateTime.Now;
            Render();
            t2 = DateTime.Now;
            var diff = t2.Subtract(t1).Milliseconds;

        }

        private void Grid_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key.ToString().Equals("S"))
            {
                camera.Move(new Vector3(0, 0, modelSizeX * 0.5f));
                Render();
            }

            if (e.Key.ToString().Equals("W"))
            {
                camera.Move(new Vector3(0, 0, -modelSizeX * 0.5f));
                Render();
            }
            if (e.Key.ToString().Equals("A"))
            {
                camera.Move(new Vector3(-modelSizeX * 0.1f, 0, 0));
                Render();
            }

            if (e.Key.ToString().Equals("D"))
            {
                camera.Move(new Vector3(modelSizeX * 0.1f, 0, 0));
                Render();
            }

            if (e.Key == Key.Up)
            {
                camera.Rotate(0.2f, Axis.X);
                Render();
            }

            if (e.Key == Key.Down)
            {
                camera.Rotate(-0.2f, Axis.X);
                Render();
            }

            if (e.Key == Key.Left)
            {
                camera.Rotate(0.2f, Axis.Y);
                Render();
            }

            if (e.Key == Key.Right)
            {
                camera.Rotate(-0.2f, Axis.Y);
                Render();
            }
            if (e.Key == Key.Z)
            {
                model.Pivot.Rotate(-0.2f, Axis.Y);
                Render();
            }

            if (e.Key == Key.C)
            {
                model.Pivot.Rotate(0.2f, Axis.Y);
                Render();
            }
        }

        public void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            Render();
        }

        public void Render()
        {
            fastBuffer.Clear();
            Array.Fill<float>(zBuffer, float.MinValue);
            DrawObject(model, camera);
            //DrawObject(axes, camera);
        }

        public bool isPolygonVisible(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            // > 0 clockwise; < 0 counterclockwise
            return ((v1.X - v3.X) * (v2.Y - v1.Y) - (v2.X - v1.X) * (v1.Y - v3.Y)) <= 0;
        }

        public void DrawObject(Primitive primitive, Camera camera)
        {
            Matrix4x4 viewMatrix = camera.ViewMatrix();
            Matrix4x4 viewModelMatrix = Matrix4x4.Multiply(primitive.Pivot.ModelMatrix(), viewMatrix);
            Matrix4x4 resultMatrix = Matrix4x4.Multiply(Matrix4x4.Multiply(
                Matrix4x4.Multiply(primitive.Pivot.ModelMatrix(), viewMatrix), camera.ProjectionMatrix), camera.ViewportTransformMatrix);

            Matrix4x4 viewLightSourceMatrix = Matrix4x4.Multiply(lightSource.Pivot.ModelMatrix(), viewMatrix);
            Vector3 viewLS = VectorMath.Transform(lightSource.Pivot.Center, viewLightSourceMatrix);

            int plygonCount = primitive.VerticesIndexes.Length / 3;
            //for (int i = 0; i < plygonCount; i ++)
            Parallel.ForEach(Partitioner.Create(0, plygonCount), range =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    int ind = i * 3;
                    // вершины полигона
                    Vector3 v1 = primitive.LocalVertices[primitive.VerticesIndexes[ind] - 1];
                    Vector3 v2 = primitive.LocalVertices[primitive.VerticesIndexes[ind + 1] - 1];
                    Vector3 v3 = primitive.LocalVertices[primitive.VerticesIndexes[ind + 2] - 1];
                    // нормали вершин полигона
                    Vector3 n1 = primitive.Normals[primitive.NormalsIndexes[ind] - 1];
                    Vector3 n2 = primitive.Normals[primitive.NormalsIndexes[ind + 1] - 1];
                    Vector3 n3 = primitive.Normals[primitive.NormalsIndexes[ind + 2] - 1];

                    Vector3 view1 = VectorMath.Transform(v1, viewModelMatrix);
                    Vector3 view2 = VectorMath.Transform(v2, viewModelMatrix);
                    Vector3 view3 = VectorMath.Transform(v3, viewModelMatrix);

                    Vector3 normal1 = VectorMath.Transform(n1, viewModelMatrix);
                    Vector3 normal2 = VectorMath.Transform(n2, viewModelMatrix);
                    Vector3 normal3 = VectorMath.Transform(n3, viewModelMatrix);

                    //Преобразование нормали в координаты наблюдателя
                    Vector3 zero = VectorMath.Transform(Vector3.Zero, viewModelMatrix);
                    normal1 -= zero;
                    normal2 -= zero;
                    normal3 -= zero;

                    //if ((view1.Z > -camera.Znear) || (view2.Z > -camera.Znear) || (view3.Z > -camera.Znear)) continue;
                    if ((view1.Z > -camera.Znear) || (view2.Z > -camera.Znear) || (view3.Z > -camera.Znear)) continue;

                    Vector3 p1 = VectorMath.Transform(v1, resultMatrix);
                    Vector3 p2 = VectorMath.Transform(v2, resultMatrix);
                    Vector3 p3 = VectorMath.Transform(v3, resultMatrix);

                    //рисуем полигон
                    if (isPolygonVisible(p1, p2, p3))
                    {
                        if (!IsPointInScreen(p1) && !IsPointInScreen(p2) && !IsPointInScreen(p3)) { continue; }
                        DrawFilledTriangle(p1, p2, p3, view1, view2, view3, normal1, normal2, normal3, viewLS, ambientColor, diffuseColor, specularColor);
                    }
                    //}
                }
            });
            bitmapSource = Convert(fastBuffer.Bitmap);
            image.Source = bitmapSource;
        }

        public void DrawFilledTriangle(Vector3 p1, Vector3 p2, Vector3 p3,
                                        Vector3 view1, Vector3 view2, Vector3 view3,
                                        Vector3 n1, Vector3 n2, Vector3 n3,
                                        Vector3 LS,
                                        System.Drawing.Color ambientColor, System.Drawing.Color diffuseColor, System.Drawing.Color specularColor)
        {
            if (p2.Y < p1.Y)
            {
                (p2, p1) = (p1, p2);
                (n2, n1) = (n1, n2);
                (view2, view1) = (view1, view2);
            }
            if (p3.Y < p1.Y)
            {
                (p3, p1) = (p1, p3);
                (n3, n1) = (n1, n3);
                (view3, view1) = (view1, view3);
            }
            if (p3.Y < p2.Y)
            {
                (p3, p2) = (p2, p3);
                (n3, n2) = (n2, n3);
                (view3, view2) = (view2, view3);
            }

            //Интовые координаты точек на экране
            int t1_X = (int)p1.X;
            int t1_Y = (int)p1.Y;
            int t2_X = (int)p2.X;
            int t2_Y = (int)p2.Y;
            int t3_X = (int)p3.X;
            int t3_Y = (int)p3.Y;

            int total_height = t3_Y - t1_Y;

            //A_X, A_Y, B_X, B_Y координаты А и B на экране
            int y, xMax, xMin, A_X, A_Y, B_X, B_Y, segment_height, idx;
            Vector3 A, B, A_N, B_N, P, N, R, lightingVector, viewVector;
            float alpha, beta, phi, diffuseLighting, specularLighting;
            bool second_half;

            int ambientColorR = (int)(ambientColor.R * 0.1f);  // 0.1
            int ambientColorG = (int)(ambientColor.G * 0.1f);
            int ambientColorB = (int)(ambientColor.B * 0.1f);
            int diffuseColorR = (int)(diffuseColor.R * 0.91f);  // 0.6
            int diffuseColorG = (int)(diffuseColor.G * 0.91f);
            int diffuseColorB = (int)(diffuseColor.B * 0.91f);
            int specularColorR = (int)(specularColor.R * 0.6f);  // 0.3
            int specularColorG = (int)(specularColor.G * 0.6f);
            int specularColorB = (int)(specularColor.B * 0.6f);
            int r, g, b;

            System.Drawing.Color pixelColor;

            for (int i = 0; i < total_height; i++)
            {
                y = t1_Y + i;
                if (y >= screenHeight) continue;
                second_half = i > t2_Y - t1_Y || t2_Y == t1_Y;
                segment_height = second_half ? t3_Y - t2_Y : t2_Y - t1_Y;

                //Коэф интерполяции по всему треугольнику
                alpha = i / (float)total_height;
                A_X = t1_X + (int)((t3_X - t1_X) * alpha);
                //A_Y = t0_Y + (int)((t2_Y - t0_Y) * alpha);
                A = view1 + (view3 - view1) * alpha;
                A_N = n1 + (n3 - n1) * alpha;

                //Коэф интерполяции по сегменту
                beta = (float)(i - (second_half ? t2_Y - t1_Y : 0)) / segment_height; // be careful: with above conditions no division by zero here
                if (second_half)
                {
                    B_X = t2_X + (int)((t3_X - t2_X) * beta);
                    //B_Y = t1_Y + (int)((t2_Y - t1_Y) * beta);
                    B = view2 + (view3 - view2) * beta;
                    B_N = n2 + (n3 - n2) * beta;
                }
                else
                {
                    B_X = t1_X + (int)((t2_X - t1_X) * beta);
                    //B_Y = t1_Y + (int)((t1_Y - t1_Y) * beta);
                    B = view1 + (view2 - view1) * beta;
                    B_N = n1 + (n2 - n1) * beta;
                }

                if (A_X > B_X)
                {
                    (A_X, B_X) = (B_X, A_X);
                    (A, B) = (B, A);
                    (A_N, B_N) = (B_N, A_N);
                }

                if ((B_X <= 0) || (y < 0)) continue;
                xMax = B_X >= screenWidth ? screenWidth - 1 : B_X;
                xMin = A_X < 0 ? 0 : A_X;
                for (int x = xMin; x <= xMax; x++)
                {
                    //Коэф интерполяции на отрезке А_Х В_Х
                    phi = B_X == A_X ? 1.0f : (x - A_X) / (float)(B_X - A_X);
                    P = A + (B - A) * phi;
                    N = A_N + (B_N - A_N) * phi;
                    idx = x + y * screenWidth;
                    //P_Z = 1.0f / P_Z;
                    if (zBuffer[idx] < P.Z)
                    {
                        zBuffer[idx] = P.Z;

                        // diffuse lighting
                        lightingVector = Vector3.Normalize(LS - P);
                        diffuseLighting = VectorMath.Cross(lightingVector, N);
                        if ((diffuseLighting < 0) || (float.IsNaN(diffuseLighting))) diffuseLighting = 0;
                        if (diffuseLighting > 1.0f) diffuseLighting = 1.0f;

                        //specular lighting
                        viewVector = Vector3.Normalize(P - Vector3.Zero);
                        //R = lightingVector - 2 * diffuseLighting * N;
                        R = Vector3.Normalize(lightingVector - 2 * diffuseLighting * N);
                        specularLighting = VectorMath.Cross(R, viewVector);
                        if (specularLighting < 0) specularLighting = 0;
                        specularLighting = MathF.Pow(specularLighting, 4096f);

                        r = ambientColorR + (int)(diffuseColorR * diffuseLighting) + (int)(specularColorR * specularLighting);
                        g = ambientColorG + (int)(diffuseColorG * diffuseLighting) + (int)(specularColorG * specularLighting);
                        b = ambientColorB + (int)(diffuseColorB * diffuseLighting) + (int)(specularColorB * specularLighting);

                        // r = ambientColorR + (int)(specularColorR * specularLighting);
                        // g = ambientColorG + (int)(specularColorG * specularLighting);
                        // b = ambientColorB + (int)(specularColorB * specularLighting);

                        if (r > 255) r = 255;
                        if (g > 255) g = 255;
                        if (b > 255) b = 255;

                        pixelColor = System.Drawing.Color.FromArgb(r, g, b);
                        printPixel(x, y, pixelColor);
                    }
                }
            }
        }

        public bool IsPointInScreen(Vector3 p)
        {
            return (p.X >= 0) && (p.X < screenWidth) && (p.Y >= 0) && (p.Y < screenHeight);
        }

        public static BitmapSource Convert(System.Drawing.Bitmap bitmap)
        {
            var bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

            var bitmapSource = BitmapSource.Create(
                bitmapData.Width, bitmapData.Height,
                bitmap.HorizontalResolution, bitmap.VerticalResolution,
                PixelFormats.Bgra32, null,
                bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

            bitmap.UnlockBits(bitmapData);

            return bitmapSource;
        }

        public void printPixel(int x, int y, System.Drawing.Color color)
        {
            fastBuffer.SetPixel(x, y, color.ToArgb());
        }

        public Primitive ParseObjFile(String filename)
        {
            StreamReader reader = new StreamReader(filename);
            String line = "";
            List<String> elements = new List<string>();
            List<Vector3> lv = new List<Vector3>();
            List<int> verticesIdx = new List<int>();
            List<int> normalsIdx = new List<int>();
            List<Vector3> vn = new List<Vector3>();
            while ((line = reader.ReadLine()) != null)
            {
                elements = line.Split(" ").ToList();
                if (elements.ElementAt(0).Equals("v"))
                {

                    float x = float.Parse(elements.ElementAt(1), CultureInfo.InvariantCulture);
                    float y = float.Parse(elements.ElementAt(2), CultureInfo.InvariantCulture);
                    float z = float.Parse(elements.ElementAt(3), CultureInfo.InvariantCulture);
                    lv.Add(new Vector3(x, y, z));
                    if (x > modelMaxX) modelMaxX = x;
                    if (y > modelMaxY) modelMaxY = y;
                    if (z > modelMaxZ) modelMaxZ = z;
                    if (x < modelMinX) modelMinX = x;
                    if (y < modelMinY) modelMinY = y;
                    if (z < modelMinZ) modelMinZ = z;

                }
                else if (elements.ElementAt(0).Equals("f"))
                {

                    for (int i = 1; i < elements.Count - 1; i++)
                    {
                        verticesIdx.Add(int.Parse(elements.ElementAt(1).Split("/").ElementAt(0)));
                        verticesIdx.Add(int.Parse(elements.ElementAt(i).Split("/").ElementAt(0)));
                        verticesIdx.Add(int.Parse(elements.ElementAt(i + 1).Split("/").ElementAt(0)));

                        normalsIdx.Add(int.Parse(elements.ElementAt(1).Split("/").ElementAt(2)));
                        normalsIdx.Add(int.Parse(elements.ElementAt(i).Split("/").ElementAt(2)));
                        normalsIdx.Add(int.Parse(elements.ElementAt(i + 1).Split("/").ElementAt(2)));
                    }

                }
                else if (elements.ElementAt(0).Equals("vn"))
                {
                    float x = float.Parse(elements.ElementAt(1), CultureInfo.InvariantCulture);
                    float y = float.Parse(elements.ElementAt(2), CultureInfo.InvariantCulture);
                    float z = float.Parse(elements.ElementAt(3), CultureInfo.InvariantCulture);
                    vn.Add(new Vector3(x, y, z));
                }
            }
            modelSizeX = modelMaxX - modelMinX;
            modelSizeY = modelMaxY - modelMinY;
            modelSizeZ = modelMaxZ - modelMinZ;
            reader.Close();
            return new Primitive(lv.ToArray(), verticesIdx.ToArray(), vn.ToArray(), normalsIdx.ToArray(), new Pivot(new Vector3(0, 0, 0), 0, 0, 0));
        }
    }
}
