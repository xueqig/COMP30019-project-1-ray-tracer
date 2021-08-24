using System;

namespace RayTracer
{
    /// <summary>
    /// Class to represent an (infinite) plane in a scene.
    /// </summary>
    public class Sphere : SceneEntity
    {
        private Vector3 center;
        private double radius;
        private Material material;

        /// <summary>
        /// Construct a sphere given its center point and a radius.
        /// </summary>
        /// <param name="center">Center of the sphere</param>
        /// <param name="radius">Radius of the spher</param>
        /// <param name="material">Material assigned to the sphere</param>
        public Sphere(Vector3 center, double radius, Material material)
        {
            this.center = center;
            this.radius = radius;
            this.material = material;
        }

        /// <summary>
        /// Determine if a ray intersects with the sphere, and if so, return hit data.
        /// </summary>
        /// <param name="ray">Ray to check</param>
        /// <returns>Hit data (or null if no intersection)</returns>
        public RayHit Intersect(Ray ray)
        {
            // Write your code here...
            Vector3 origin = new Vector3(0, 0, 0);
            // Calculate the distance from the origin to the point on the ray halfway between the 2 intersection points
            Vector3 L = this.center - origin;
            double tc = L.Dot(ray.Direction);

            if (tc < 0)
            {
                return null;
            }

            double d = Math.Sqrt(tc * tc - L.LengthSq());
            if (d > this.radius)
            {
                return null;
            }

            // Calculate intersection point
            double t1c = Math.Sqrt(this.radius * this.radius - d * d);
            double t1 = tc - t1c;
            Vector3 position = origin + t1 * ray.Direction;

            // Calculate the normal at this position
            Vector3 normal = position - this.center;

            return new RayHit(position, normal, ray.Direction, this.material);
        }

        /// <summary>
        /// The material of the sphere.
        /// </summary>
        public Material Material { get { return this.material; } }
    }

}
