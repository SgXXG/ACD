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
        public byte[] DiffuseMap { get; protected set; }
        public float[] NormalMap { get; protected set; }
        public byte[] SpecularMap { get; protected set; }
        public Vector2[] TextureCoords { get; protected set; }
        public int[] TextureIndexes { get; protected set; }
        public int Width { get; protected set; }
        public int Height { get; protected set; }
        public String Path { get; protected set; }
        
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
        
        public void LoadFromFile(string path)
        {
            StbImage.stbi_set_flip_vertically_on_load(1);

            // Here we open a stream to the file and pass it to StbImageSharp to load.
            using (Stream stream = File.OpenRead(path + "\\shovel_diffuse.png"))
            {
                ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlue);
                DiffuseMap = image.Data;
                Width = image.Width;
                Height = image.Height;
            }
            using (Stream stream = File.OpenRead(path + "\\shovel_normal_map.png"))
            {
                ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlue);
                byte[] normalMapByte = image.Data;
                NormalMap = new float[normalMapByte.Length];
                for (int i = 0; i < normalMapByte.Length; i++) {
                    NormalMap[i] = normalMapByte[i] / 255f * 2 - 1;
                }
            }
            using (Stream stream = File.OpenRead(path + "\\shovel_mrao.png"))
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
