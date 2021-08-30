using System.IO;
using System;
using System.Collections.Generic;

namespace RayTracer
{
    /// <summary>
    /// Add-on option C. You should implement your solution in this class template.
    /// </summary>
    public class ObjModel : SceneEntity
    {
        private Material material;
        private List<Vector3> vertices;
        private List<List<int>> faces;
        private Vector3 center;
        private double radius;

        /// <summary>
        /// Construct a new OBJ model.
        /// </summary>
        /// <param name="objFilePath">File path of .obj</param>
        /// <param name="offset">Vector each vertex should be offset by</param>
        /// <param name="scale">Uniform scale applied to each vertex</param>
        /// <param name="material">Material applied to the model</param>
        public ObjModel(string objFilePath, Vector3 offset, double scale, Material material)
        {
            this.material = material;

            // Here's some code to get you started reading the file...
            this.vertices = new List<Vector3>();
            this.faces = new List<List<int>>();

            string[] lines = File.ReadAllLines(objFilePath);
            for (int i = 0; i < lines.Length; i++)
            {
                String[] line = lines[i].Split(" ");
                if (line[0] == "v")
                {
                    Vector3 vertex = new Vector3(Double.Parse(line[1]), Double.Parse(line[2]), Double.Parse(line[3]));
                    this.vertices.Add(vertex);
                }
                else if (line[0] == "f")
                {
                    // f 1//1 2//2 3//3
                    List<int> face = new List<int>();
                    face.Add(Int32.Parse(line[1].Split("//")[0]));
                    face.Add(Int32.Parse(line[2].Split("//")[0]));
                    face.Add(Int32.Parse(line[3].Split("//")[0]));
                    this.faces.Add(face);
                }
            }
            Vector3 minVector = this.minVector() * 0.35 + new Vector3(0, -0.9, 2);
            Vector3 maxVector = this.maxVector() * 0.35 + new Vector3(0, -0.9, 2);
            this.center = new Vector3((minVector.X + maxVector.X) / 2, (minVector.Y + maxVector.Y) / 2, (minVector.Z + maxVector.Z) / 2);
            this.radius = (maxVector - minVector).Length() / 2;
        }

        /// <summary>
        /// Given a ray, determine whether the ray hits the object
        /// and if so, return relevant hit data (otherwise null).
        /// </summary>
        /// <param name="ray">Ray data</param>
        /// <returns>Ray hit data, or null if no hit</returns>
        public RayHit Intersect(Ray ray)
        {
            // Create bounding sphere
            SceneEntity sphere = new Sphere(this.center, this.radius, this.material);
            if (sphere.Intersect(ray) == null)
            {
                return null;
            }

            for (int i = 0; i < this.faces.Count; i++)
            {
                try
                {
                    Vector3 v0 = this.vertices[this.faces[i][0] - 1] * 0.35 + new Vector3(0, -0.9, 2);
                    Vector3 v1 = this.vertices[this.faces[i][1] - 1] * 0.35 + new Vector3(0, -0.9, 2);
                    Vector3 v2 = this.vertices[this.faces[i][2] - 1] * 0.35 + new Vector3(0, -0.9, 2);
                    SceneEntity triangle = new Triangle(v0, v1, v2, this.material);

                    if (triangle.Intersect(ray) != null)
                    {
                        return triangle.Intersect(ray);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return null;
        }

        /// <summary>
        /// The material attached to this object.
        /// </summary>
        public Material Material { get { return this.material; } }

        private Vector3 minVector()
        {
            double minX = Double.MaxValue;
            double minY = Double.MaxValue;
            double minZ = Double.MaxValue;
            for (int i = 0; i < this.vertices.Count; i++)
            {
                if (this.vertices[i].X < minX)
                {
                    minX = this.vertices[i].X;
                }
                if (this.vertices[i].Y < minY)
                {
                    minY = this.vertices[i].Y;
                }
                if (this.vertices[i].Z < minZ)
                {
                    minZ = this.vertices[i].Z;
                }
            }
            return new Vector3(minX, minY, minZ);
        }
        private Vector3 maxVector()
        {
            double maxX = Double.MinValue;
            double maxY = Double.MinValue;
            double maxZ = Double.MinValue;
            for (int i = 0; i < this.vertices.Count; i++)
            {
                if (this.vertices[i].X > maxX)
                {
                    maxX = this.vertices[i].X;
                }
                if (this.vertices[i].Y > maxY)
                {
                    maxY = this.vertices[i].Y;
                }
                if (this.vertices[i].Z > maxZ)
                {
                    maxZ = this.vertices[i].Z;
                }
            }
            return new Vector3(maxX, maxY, maxZ);
        }
    }

}
