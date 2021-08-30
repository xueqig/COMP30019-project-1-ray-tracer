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
        }

        /// <summary>
        /// Given a ray, determine whether the ray hits the object
        /// and if so, return relevant hit data (otherwise null).
        /// </summary>
        /// <param name="ray">Ray data</param>
        /// <returns>Ray hit data, or null if no hit</returns>
        public RayHit Intersect(Ray ray)
        {
            // Write your code here...
            for (int i = 0; i < this.faces.Count; i++)
            {
                try
                {
                    // Console.WriteLine(this.vertices[this.faces[i][0]]);
                    Vector3 v0 = this.vertices[this.faces[i][0] - 1] + new Vector3(0, -0.9, 2);
                    Vector3 v1 = this.vertices[this.faces[i][1] - 1] + new Vector3(0, -0.9, 2);
                    Vector3 v2 = this.vertices[this.faces[i][2] - 1] + new Vector3(0, -0.9, 2);
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
    }

}
