using System;

namespace RayTracer
{
    /// <summary>
    /// Class to represent a triangle in a scene represented by three vertices.
    /// </summary>
    public class Triangle : SceneEntity
    {
        private Vector3 v0, v1, v2;
        private Material material;

        /// <summary>
        /// Construct a triangle object given three vertices.
        /// </summary>
        /// <param name="v0">First vertex position</param>
        /// <param name="v1">Second vertex position</param>
        /// <param name="v2">Third vertex position</param>
        /// <param name="material">Material assigned to the triangle</param>
        public Triangle(Vector3 v0, Vector3 v1, Vector3 v2, Material material)
        {
            this.v0 = v0;
            this.v1 = v1;
            this.v2 = v2;
            this.material = material;
        }

        /// <summary>
        /// Determine if a ray intersects with the triangle, and if so, return hit data.
        /// </summary>
        /// <param name="ray">Ray to check</param>
        /// <returns>Hit data (or null if no intersection)</returns>
        public RayHit Intersect(Ray ray)
        {
            // Write your code here...
            // Calculate normal of the plane
            Vector3 normal = (this.v1 - this.v0).Cross(this.v2 - this.v0).Normalized();

            // Check if the ray hits the plane
            // If the ray is perpendicular to normal, it will not hit the plane
            double rayDirectionDotNormal = normal.Dot(ray.Direction);
            if (Math.Abs(rayDirectionDotNormal) < double.Epsilon)
            {
                return null;
            }

            // If the ray hit the plane
            double t = (this.v0 - ray.Origin).Dot(normal) / rayDirectionDotNormal;

            // Check if triangle is in behind the ray
            if (t <= 0)
            {
                return null;
            }

            // Calculate position of the hit
            Vector3 position = ray.Origin + t * ray.Direction;

            // Check if position of the hit is inside the triangle 
            Vector3 v0v1 = this.v1 - this.v0;
            Vector3 v0p = position - this.v0;
            double dot0 = normal.Dot(v0v1.Cross(v0p));

            Vector3 v1v2 = this.v2 - this.v1;
            Vector3 v1p = position - this.v1;
            double dot1 = normal.Dot(v1v2.Cross(v1p));

            Vector3 v2v0 = this.v0 - this.v2;
            Vector3 v2p = position - this.v2;
            double dot2 = normal.Dot(v2v0.Cross(v2p));

            if (dot0 < 0 || dot1 < 0 || dot2 < 0)
            {
                return null;
            }

            return new RayHit(position, normal, ray.Direction, this.material);
        }

        /// <summary>
        /// The material of the triangle.
        /// </summary>
        public Material Material { get { return this.material; } }
    }

}
