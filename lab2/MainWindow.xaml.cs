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

namespace ACG_1
{
    public partial class MainWindow : Window
    {
        int screenWidth;
        int screenHeight;
        float modelMaxX, modelMaxY, modelMaxZ;
        float modelMinX, modelMinY, modelMinZ;
        float modelSizeX, modelSizeY, modelSizeZ;

        Bitmap buffer;
        FastBitmap fastBuffer;
        BitmapSource bitmapSource;
        float[] zBuffer;
        System.Drawing.Color modelColor = System.Drawing.Color.WhiteSmoke;

        Camera camera;
        LightSource lightSource;
        Primitive model;
        Primitive axes;
        // Выполняется инициализация основных параметров
        public MainWindow()
        {
            InitializeComponent();
            window.KeyDown += Grid_KeyDown;
            
            screenWidth = (int)window.Width;
            screenHeight = (int)window.Height;

            buffer = new Bitmap((int)screenWidth, (int)screenHeight);
            fastBuffer = new FastBitmap((int)screenWidth, (int)screenHeight);

            zBuffer = new float[screenWidth * screenHeight];
            
            for (int i = 0; i < zBuffer.Length; ++i)
            {
                zBuffer[i] = float.MinValue;
            }

            float fNear = 0.1f;
            float fFar = 10000.0f;
            float fFov = 70.0f; // градусы
            float fFovRad = fFov / 180.0f * MathF.PI;

            model = ParseObjFile("Porsche_911_GT2.obj");
            camera = new Camera(new Vector3(0, 0, modelSizeY * 5.0f), 0,0,0, fFovRad,fNear, fFar, screenWidth, screenHeight);
    
            lightSource = new LightSource(new Vector3(modelSizeX * 3, modelSizeY * 2, modelSizeZ * 15.0f));
            
            float axesLenght = Math.Max(Math.Max(modelSizeX, modelSizeY), modelSizeZ) * 0.5f;

            DateTime t1, t2;
            t1 = DateTime.Now;
            Render();
            t2 = DateTime.Now;
            var diff = t2.Subtract(t1).Milliseconds;
        }

        // Обработчик события нажатия клавиши,
        // реализовано управление камерой (перемещение и вращение) при нажатии клавиш
        private void Grid_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key.ToString().Equals("S"))
            {
                camera.Move(new Vector3(0, 0, modelSizeX * 0.1f));
                Render();
            }

            if (e.Key.ToString().Equals("W"))
            {
                camera.Move(new Vector3(0, 0, -modelSizeX * 0.11f));
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

        // Метод обновления экрана по таймеру.
        public void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            Render();
        }

        // Метод для рендеринга 3D-сцены. Очищает буфер и рисует объекты.
        public void Render()
        {
            fastBuffer.Clear();
            for (int i = 0; i < zBuffer.Length; ++i)
            {
                zBuffer[i] = float.MinValue;
            }
            DrawObject(model, camera);
        }

        public bool isPolygonVisible(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            return ((v2.X - v1.X) * (v3.Y - v1.Y) - (v2.Y - v1.Y) * (v3.X - v1.X)) < 0;
        }

        // Метод для отрисовки 3D-объектов в сцене.
        public void DrawObject(Primitive primitive, Camera camera)
        {           
            Matrix4x4 viewMatrix = camera.ViewMatrix();
            Matrix4x4 viewModelMatrix = Matrix4x4.Multiply(primitive.Pivot.ModelMatrix(), viewMatrix);
            Matrix4x4 resultMatrix = Matrix4x4.Multiply(Matrix4x4.Multiply(Matrix4x4.Multiply(primitive.Pivot.ModelMatrix(), viewMatrix), camera.ProjectionMatrix), camera.ViewportTransformMatrix);
            
            Matrix4x4 viewLightSourceMatrix = Matrix4x4.Multiply(lightSource.Pivot.ModelMatrix(), viewMatrix);
            Vector3 LS = VectorMath.Transform(lightSource.Pivot.Center, viewLightSourceMatrix);
            
            for (int i = 0; i < primitive.Indexes.Length; i += 3)
            {
                // индексы вершин полигона
                var i1 = primitive.Indexes[i];
                var i2 = primitive.Indexes[i + 1];
                var i3 = primitive.Indexes[i + 2];
                // вершины полигона
                var v1 = primitive.LocalVertices[i1 - 1];
                var v2 = primitive.LocalVertices[i2 - 1];
                var v3 = primitive.LocalVertices[i3 - 1];
                // рисуем полигон
                DrawPolygon(v1, v2, v3, resultMatrix, viewModelMatrix, LS);
            }
            bitmapSource = Convert(fastBuffer.Bitmap);
            image.Source = bitmapSource;
        }

        // Метод для рисования полигона на экране.
        public void DrawPolygon(Vector3 v1, Vector3 v2, Vector3 v3, Matrix4x4 resultMatrix, Matrix4x4 viewModelMatrix, Vector3 LS)
        {
            Vector3 view1 = VectorMath.Transform(v1, viewModelMatrix);
            Vector3 view2 = VectorMath.Transform(v2, viewModelMatrix);
            Vector3 view3 = VectorMath.Transform(v3, viewModelMatrix);

            if ((view1.Z > -camera.Znear) || (view2.Z > -camera.Znear) || (view3.Z > -camera.Znear)) return;
        
            var p1 = VectorMath.Transform(v1, resultMatrix);
            var p2 = VectorMath.Transform(v2, resultMatrix);
            var p3 = VectorMath.Transform(v3, resultMatrix);
            //рисуем полигон
            if (isPolygonVisible(p1, p2, p3))
            {
                if (!IsPointInScreen(p1) && !IsPointInScreen(p2) && !IsPointInScreen(p3)) { return; }
                float brightness = lightSource.GetPolygonBrightness(view1, view2, view3, LS);
                System.Drawing.Color color = System.Drawing.Color.FromArgb(modelColor.A, (int)(modelColor.R * brightness), (int)(modelColor.G * brightness), (int)(modelColor.B * brightness));
                DrawFilledTriangle2(p1, p2, p3, view1, view2, view3, color);
            }
        }

        public void DrawFilledTriangle2(Vector3 v0, Vector3 v1, Vector3 v2,
                                        Vector3 view0, Vector3 view1, Vector3 view2,
                                        System.Drawing.Color color)
        {
            if (v1.Y < v0.Y)
            {
                Swap<Vector3>(ref v1, ref v0);
                Swap<Vector3>(ref view1, ref view0);
            }
            if (v2.Y < v0.Y)
            {
                Swap<Vector3>(ref v2, ref v0);
                Swap<Vector3>(ref view2, ref view0);
            }
            if (v2.Y < v1.Y)
            {
                Swap<Vector3>(ref v2, ref v1);
                Swap<Vector3>(ref view2, ref view1);
            }

            int t0_X = (int)v0.X;
            int t0_Y = (int)v0.Y;
            int t1_X = (int)v1.X;
            int t1_Y = (int)v1.Y;
            int t2_X = (int)v2.X;
            int t2_Y = (int)v2.Y;

            int total_height = t2_Y - t0_Y;

            int y, xMax, xMin, A_X, A_Y, B_X, B_Y, segment_height, idx;
            Vector3 A, B, P;
            float alpha, beta, phi;
            bool second_half;


            for (int i = 0; i < total_height; i++)
            {
                y = t0_Y + i;
                if (y >= screenHeight) return;
                second_half = i > t1_Y - t0_Y || t1_Y == t0_Y;
                segment_height = second_half ? t2_Y - t1_Y : t1_Y - t0_Y;

                alpha = i / (float)total_height;
                A_X = t0_X + (int)((t2_X - t0_X) * alpha);
                A_Y = t0_Y + (int)((t2_Y - t0_Y) * alpha);
                A = view0 + (view2 - view0) * alpha;

                beta = (float)(i - (second_half ? t1_Y - t0_Y : 0)) / segment_height; // be careful: with above conditions no division by zero here
                if (second_half)
                {
                    B_X = t1_X + (int)((t2_X - t1_X) * beta);
                    B_Y = t1_Y + (int)((t2_Y - t1_Y) * beta);
                    B = view1 + (view2 - view1) * beta;
                }
                else
                {
                    B_X = t0_X + (int)((t1_X - t0_X) * beta);
                    B_Y = t0_Y + (int)((t1_Y - t0_Y) * beta);
                    B = view0 + (view1 - view0) * beta;
                }

                if (A_X > B_X)
                {
                    (A_X, B_X) = (B_X, A_X);
                    (A_Y, B_Y) = (B_Y, A_Y);
                    (A, B) = (B, A);
                }

                if ((B_X <= 0) || (y < 0)) continue;
                xMax = B_X >= screenWidth ? screenWidth - 1 : B_X;
                xMin = A_X < 0 ? 0 : A_X;
                for (int x = xMin; x <= xMax; x++)
                {
                    phi = B_X == A_X ? 1.0f : (x - A_X) / (float)(B_X - A_X);
                    P = A + (B - A) * phi;
                    idx = x + y * screenWidth;
                    if (zBuffer[idx] < P.Z)
                    {
                        zBuffer[idx] = P.Z;
                        printPixel(x, y, color);
                    }
                }
            }
        }

        static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp;
            temp = lhs;
            lhs = rhs;
            rhs = temp;
        }

        // Метод для проверки, находится ли точка внутри экрана.
        public bool IsPointInScreen(Vector3 p) {
            return (p.X > 0) && (p.X < screenWidth) && (p.Y > 0) && (p.Y < screenHeight);
        }

        // Метод для преобразования объекта Bitmap в BitmapSource.
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

        // Метод для установки пикселя определенного цвета.
        public void printPixel(int x, int y, System.Drawing.Color color)
        {
            fastBuffer.SetPixel(x, y, color.ToArgb());
        }

        // Метод для парсинга файла .obj и создания 3D-примитива.
        public Primitive ParseObjFile(String filename) {
            StreamReader reader = new StreamReader(filename);
            String line = "";
            List<String> elements = new List<string>();
            List<Vector3> lv = new List<Vector3>();
            List<int> idx = new List<int>();
            while ((line = reader.ReadLine()) != null) {
                elements = line.Split(" ").ToList();
                if (elements.ElementAt(0).Equals("v")) {
                    
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

                } else if (elements.ElementAt(0).Equals("f"))
                {
                    for (int i = 1; i < elements.Count - 1; i++) {
                        idx.Add(int.Parse(elements.ElementAt(1).Split("/").ElementAt(0)));
                        idx.Add(int.Parse(elements.ElementAt(i).Split("/").ElementAt(0)));
                        idx.Add(int.Parse(elements.ElementAt(i + 1).Split("/").ElementAt(0)));
                    }
                }
            }
            modelSizeX = modelMaxX - modelMinX;
            modelSizeY = modelMaxY - modelMinY;
            modelSizeZ = modelMaxZ - modelMinZ;
            reader.Close();
            return new Primitive(lv.ToArray(), idx.ToArray(), new Pivot(new Vector3(0, 0, 0), 0, 0, 0));
        }
    }
}
