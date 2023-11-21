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

        FastBitmap fastBuffer;
        BitmapSource bitmapSource;

        Camera camera;
        Primitive model;

        // Выполняется инициализация основных параметров
        public MainWindow()
        {
            InitializeComponent();
            window.KeyDown += Grid_KeyDown;

            screenWidth = (int)window.Width;
            screenHeight = (int)window.Height;

            fastBuffer = new FastBitmap((int)screenWidth, (int)screenHeight);

            float fNear = 0.1f;
            float fFar = 120000.0f;
            float fFov = 70.0f; 
            float fFovRad = fFov / 180.0f * MathF.PI;

            model = ParseObjFile("Porsche_911_GT2.obj");

            camera = new Camera(new Vector3(0, 0, modelSizeY * 5.0f), 0,0,0, fFovRad,fNear, fFar, screenWidth, screenHeight);

            float axesLenght = Math.Max(Math.Max(modelSizeX, modelSizeY), modelSizeZ) * 0.5f;

            DateTime t1, t2;
            t1 = DateTime.Now;
            Render();
            t2 = DateTime.Now;
            var diff = t2.Subtract(t1).Milliseconds;

        }

        // Обработчик события нажатия клавиши,
        // реализовано управление камерой (перемещение и вращение) при нажатии клавиш.
        private void Grid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.ToString().Equals("W"))
            {
                camera.Move(new Vector3(0, 0, -modelSizeX * 0.1f));
                Render();
            }
            if (e.Key.ToString().Equals("A"))
            {
                camera.Move(new Vector3(-modelSizeX * 0.1f, 0, 0));
                Render();
            }
            if (e.Key.ToString().Equals("S"))
            {
                camera.Move(new Vector3(0, 0, modelSizeX * 0.1f));
                Render();
            }
            if (e.Key.ToString().Equals("D"))
            {
                camera.Move(new Vector3(modelSizeX * 0.1f, 0, 0));
                Render();
            }
            
            if (e.Key == Key.Up)
            {
                camera.Rotate(0.1f, Axis.X);
                Render();
            }
            if (e.Key == Key.Right)
            {
                camera.Rotate(-0.1f, Axis.Y);
                Render();
            }
            if (e.Key == Key.Down)
            {
                camera.Rotate(-0.1f, Axis.X);
                Render();
            }
            if (e.Key == Key.Left)
            {
                camera.Rotate(0.1f, Axis.Y);
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
            
            DrawObject(model, camera);
        }

        // Метод для отрисовки 3D-объектов в сцене.
        public void DrawObject(Primitive primitive, Camera camera)
        {           
            Matrix4x4 viewMatrix = camera.ViewMatrix();
            Matrix4x4 resultMatrix = Matrix4x4.Multiply(Matrix4x4.Multiply(Matrix4x4.Multiply(primitive.Pivot.ModelMatrix(), camera.ViewMatrix()), camera.ProjectionMatrix), camera.ViewportTransformMatrix);

            for (int i = 0; i < primitive.Indexes.Length; i += 3)
            {
                // Индексы вершин полигона
                var i1 = primitive.Indexes[i];
                var i2 = primitive.Indexes[i + 1];
                var i3 = primitive.Indexes[i + 2];
                // Вершины полигона
                var v1 = primitive.LocalVertices[i1 - 1];
                var v2 = primitive.LocalVertices[i2 - 1];
                var v3 = primitive.LocalVertices[i3 - 1];
                // Рисование полигона
                DrawPolygon(v1, v2, v3, resultMatrix, viewMatrix);
            }
            bitmapSource = Convert(fastBuffer.Bitmap);
            image.Source = bitmapSource;
        }

        // Метод для рисования полигона на экране.
        public void DrawPolygon(Vector3 v1, Vector3 v2, Vector3 v3, Matrix4x4 resultMatrix, Matrix4x4 viewMatrix)
        {
            float z1 = VectorMath.Transform(v1, viewMatrix).Z;
            float z2 = VectorMath.Transform(v2, viewMatrix).Z;
            float z3 = VectorMath.Transform(v3, viewMatrix).Z;

            if ((z1 > -camera.Znear) || (z2 > -camera.Znear) || (z3 > -camera.Znear)) return;
        
            var p1 = VectorMath.Transform(v1, resultMatrix);
            var p2 = VectorMath.Transform(v2, resultMatrix);
            var p3 = VectorMath.Transform(v3, resultMatrix);
            
            // Рисование полигона
            if (isPointInScreen(p1) && isPointInScreen(p2) && isPointInScreen(p3)) {
                BresenhamLine(System.Drawing.Color.Black, (int)p1.X, (int)p1.Y, (int)p2.X, (int)p2.Y);
                BresenhamLine(System.Drawing.Color.Black, (int)p2.X, (int)p2.Y, (int)p3.X, (int)p3.Y);
                BresenhamLine(System.Drawing.Color.Black, (int)p3.X, (int)p3.Y, (int)p1.X, (int)p1.Y);
            }
        }

        // Метод для проверки, находится ли точка внутри экрана.
        public bool isPointInScreen(Vector3 p) {
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

        // Алгоритм Брезенхэма для рисования линии между точками.
        public void BresenhamLine(System.Drawing.Color color, int x1, int y1, int x2, int y2)
        {
            int dx = Math.Abs(x2 - x1);
            int sx = x1 < x2 ? 1 : -1;
            int dy = -Math.Abs(y2 - y1);
            int sy = y1 < y2 ? 1 : -1;
            int error = dx + dy;

            while (true)
            {
                printPixel(x1, y1, color);
                if ((x1 == x2) && (y1 == y2)) break;
                int e2 = 2 * error;
                if (e2 >= dy)
                {
                    if (x1 == x2) break;
                    error = error + dy;
                    x1 = x1 + sx;
                }
                if (e2 <= dx)
                {
                    if (y1 == y2) break;
                    error = error + dx;
                    y1 = y1 + sy;
                }
            }
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
                    idx.Add(int.Parse(elements.ElementAt(1).Split("/").ElementAt(0)));
                    idx.Add(int.Parse(elements.ElementAt(2).Split("/").ElementAt(0)));
                    idx.Add(int.Parse(elements.ElementAt(3).Split("/").ElementAt(0)));
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
