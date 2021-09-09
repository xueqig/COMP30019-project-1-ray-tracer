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
        private List<Vector3> vertices;
        private List<Vector3> vertexNormals;
        private List<List<int>> faces;
        private Vector3 offset;
        private double scale;
        private Material material;
        private Vector3 center; // Bounding sphere center
        private double radius; // Bounding sphere radium 

        /// <summary>
        /// Construct a new OBJ model.
        /// </summary>
        /// <param name="objFilePath">File path of .obj</param>
        /// <param name="offset">Vector each vertex should be offset by</param>
        /// <param name="scale">Uniform scale applied to each vertex</param>
        /// <param name="material">Material applied to the model</param>
        public ObjModel(string objFilePath, Vector3 offset, double scale, Material material)
        {
            this.offset = offset;
            this.scale = scale;
            this.material = material;

            // Here's some code to get you started reading the file...
            this.vertices = new List<Vector3>();
            this.vertexNormals = new List<Vector3>();
            this.faces = new List<List<int>>();

            string[] lines = File.ReadAllLines(objFilePath);
            for (int i = 0; i < lines.Length; i++)
            {
                String[] line = lines[i].Split(" ");
                if (line[0] == "v")
                {
                    double v0 = Double.Parse(line[1]);
                    double v1 = Double.Parse(line[2]);
                    double v2 = Double.Parse(line[3]);
                    Vector3 vertex = new Vector3(v0, v1, v2) * scale + offset;
                    this.vertices.Add(vertex);
                }
                else if (line[0] == "vn")
                {
                    double vn0 = Double.Parse(line[1]);
                    double vn1 = Double.Parse(line[2]);
                    double vn2 = Double.Parse(line[3]);
                    Vector3 vertexNormal = new Vector3(vn0, vn1, vn2);
                    this.vertexNormals.Add(vertexNormal);
                }
                else if (line[0] == "f")
                {
                    List<int> face = new List<int>();
                    face.Add(Int32.Parse(line[1].Split("/")[0]));
                    face.Add(Int32.Parse(line[2].Split("/")[0]));
                    face.Add(Int32.Parse(line[3].Split("/")[0]));
                    this.faces.Add(face);
                }
            }

            // Calculate center and radius of bounding sphere
            Vector3 minVector = this.minVector();
            Vector3 maxVector = this.maxVector();
            double sv0 = (minVector.X + maxVector.X) / 2;
            double sv1 = (minVector.Y + maxVector.Y) / 2;
            double sv2 = (minVector.Z + maxVector.Z) / 2;
            this.center = new Vector3(sv0, sv1, sv2);
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
            // Check bounding sphere intersection
            SceneEntity sphere = new Sphere(this.center, this.radius, this.material);
            if (sphere.Intersect(ray) == null)
            {
                return null;
            }

            // Check triangle mesh intersection
            double minDistanceSq = Double.MaxValue;
            RayHit firstHit = null;
            for (int i = 0; i < this.faces.Count; i++)
            {
                try
                {
                    Vector3 v0 = this.vertices[this.faces[i][0] - 1];
                    Vector3 v1 = this.vertices[this.faces[i][1] - 1];
                    Vector3 v2 = this.vertices[this.faces[i][2] - 1];

                    Vector3 n0 = this.vertexNormals[this.faces[i][0] - 1];
                    Vector3 n1 = this.vertexNormals[this.faces[i][1] - 1];
                    Vector3 n2 = this.vertexNormals[this.faces[i][2] - 1];

                    SceneEntity triangle = new Triangle(v0, v1, v2, this.material);
                    // RayHit hit = triangle.Intersect(ray);

                    RayHit hit = rayTriangleIntersect(ray, v0, v1, v2, n0, n1, n2);
                    if (hit != null)
                    {
                        double distanceSq = (hit.Position - ray.Origin).LengthSq();
                        if (distanceSq < minDistanceSq && distanceSq != 0)
                        {
                            minDistanceSq = distanceSq;
                            firstHit = hit;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return firstHit;
        }

        /// <summary>
        /// The material attached to this object.
        /// </summary>
        public Material Material { get { return this.material; } }

        private RayHit rayTriangleIntersect(Ray ray, Vector3 v0, Vector3 v1, Vector3 v2, Vector3 n0, Vector3 n1, Vector3 n2)
        {
            // Write your code here...
            // Calculate normal of the plane
            Vector3 v0v1 = v1 - v0;
            Vector3 v0v2 = v2 - v0;
            Vector3 normal = v0v1.Cross(v0v2).Normalized();

            // Check if ray and plane are parallel
            double NorDotDir = normal.Dot(ray.Direction);
            if (Math.Abs(NorDotDir) < double.Epsilon)
            {
                return null;
            }

            // Computer t
            double t = (v0 - ray.Origin).Dot(normal) / NorDotDir;

            // Check if the triangle is in behind the ray
            if (t <= 0)
            {
                return null;
            }

            // Compute the intersection point 
            Vector3 position = ray.Origin + t * ray.Direction;

            // Check if intersection point is inside the triangle 
            Vector3 v0p = position - v0;
            double dot0 = normal.Dot(v0v1.Cross(v0p));

            Vector3 v1v2 = v2 - v1;
            Vector3 v1p = position - v1;
            double dot1 = normal.Dot(v1v2.Cross(v1p));

            Vector3 v2v0 = v0 - v2;
            Vector3 v2p = position - v2;
            double dot2 = normal.Dot(v2v0.Cross(v2p));

            if (dot0 < 0 || dot1 < 0 || dot2 < 0)
            {
                return null;
            }

            // Compute hit normal by using vertex normals
            Vector3 pvec = ray.Direction.Cross(v0v2);
            double det = v0v1.Dot(pvec);
            double invDet = 1 / det;
            Vector3 tvec = ray.Origin - v0;
            double u = tvec.Dot(pvec) * invDet;
            Vector3 qvec = tvec.Cross(v0v1);
            double v = ray.Direction.Dot(qvec) * invDet;
            Vector3 hitNormal = (1 - u - v) * n0 + u * n1 + v * n2;

            return new RayHit(position, hitNormal, ray.Direction, this.material);
        }
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
