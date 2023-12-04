using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StbImageSharp;
using System.Numerics;

namespace ACG_1
{
    public class Texture
    {
        // Хранит данные пикселей диффузной карты 
        public byte[] DiffuseMap { get; protected set; }
        // Содержит данные пикселей карты нормалей 
        public float[] NormalMap { get; protected set; }
        // Содержит данные пикселей карты бликов 
        public byte[] SpecularMap { get; protected set; }
        // Хранит текстурные координаты, используемые для нанесения текстур на 3D-поверхности.
        public Vector2[] TextureCoords { get; protected set; }
        // Индексы, связанные с текстурами.
        public int[] TextureIndexes { get; protected set; }
        // Содержат размеры загруженных карт текстур.
        public int Width { get; protected set; }
        public int Height { get; protected set; }
        // Содержит путь к файлам текстур.
        public String Path { get; protected set; }
        
        // Принимает путь к файлу, текстурные координаты и индексы текстур в качестве параметров.
        // Инициализирует свойства и пытается загрузить данные текстур из указанных файлов.
        public Texture(String path, Vector2[] textureCoords, int[] textureIdx) 
        {
            Path = path;
            TextureCoords = textureCoords;
            TextureIndexes = textureIdx;
            DiffuseMap = Array.Empty<byte>();
            NormalMap = Array.Empty<float>();
            SpecularMap = Array.Empty<byte>();
            Width = 0;
            Height = 0;
            LoadFromFile(path);
        }
        
        // Загружает данные текстур из изображений (диффузной, нормальной, бликовой) по указанному пути.
        // Преобразует данные пикселей в соответствующий формат (массив байтов или массив чисел с плавающей запятой для нормальных карт).
        // Устанавливает свойства Width и Height на основе загруженных размеров изображений.
        public void LoadFromFile(string path)
        {
            StbImage.stbi_set_flip_vertically_on_load(1);

            // Here we open a stream to the file and pass it to StbImageSharp to load.
            using (Stream stream = File.OpenRead("shovel_diffuse.png"))
            {
                ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlue);
                DiffuseMap = image.Data;
                Width = image.Width;
                Height = image.Height;
            }
            using (Stream stream = File.OpenRead("shovel_normal_map.png"))
            {
                ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlue);
                byte[] normalMapByte = image.Data;
                NormalMap = new float[normalMapByte.Length];
                for (int i = 0; i < normalMapByte.Length; i++) {
                    NormalMap[i] = normalMapByte[i] / 255f * 2 - 1;
                }
            }
            using (Stream stream = File.OpenRead("shovel_mrao.png"))
            {
                ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlue);
                SpecularMap = image.Data;
            }
        }

        public int[] GetDiffuseMapColor(float u, float v) {
            int idx = ((int)(v * Height) * Width + (int)(u * Width)) * 3;
            return new int[] { DiffuseMap[idx], DiffuseMap[idx + 1], DiffuseMap[idx + 2] };
        }

        public float GetSpecularMapCoef(float u, float v)
        {
            int idx = ((int)(v * Height) * Width + (int)(u * Width)) * 3;
            return SpecularMap[idx] / 255f;
        }
        
        public Vector3 GetNormal(float u, float v) {
            int idx = ((int)(v * Height) * Width + (int)(u * Width)) * 3;
            return new Vector3(NormalMap[idx], NormalMap[idx + 1], NormalMap[idx + 2]);
        }
    }
}
