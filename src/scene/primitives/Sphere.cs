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
            // Calculate the distance from the origin to the point on the ray halfway between the 2 intersection points
            Vector3 L = this.center - ray.Origin;
            double tca = L.Dot(ray.Direction);

            if (tca < 0)
            {
                return null;
            }

            double d2 = L.Dot(L) - tca * tca;
            if (d2 > this.radius * this.radius)
            {
                return null;
            }

            // Calculate intersection point
            double thc = Math.Sqrt(this.radius * this.radius - d2);
            double t0 = tca - thc;
            double t1 = tca + thc;

            if (t0 > t1)
            {
                double temp = t0;
                t0 = t1;
                t1 = temp;
            };

            if (t0 <= 0)
            {
                t0 = t1; // if t0 is negative, let's use t1 instead 
                if (t0 <= 0)
                {
                    return null; // both t0 and t1 are negative 
                }
            }

            Vector3 position = ray.Origin + t0 * ray.Direction;

            // Calculate the normal at this position
            Vector3 normal = (position - this.center).Normalized();

            return new RayHit(position, normal, ray.Direction, this.material);
        }

        /// <summary>
        /// The material of the sphere.
        /// </summary>
        public Material Material { get { return this.material; } }
    }

}
