using System;
using System.Collections.Generic;

namespace RayTracer
{
    /// <summary>
    /// Class to represent a ray traced scene, including the objects,
    /// light sources, and associated rendering logic.
    /// </summary>
    static class Constants
    {
        public const double fov = Math.PI / 6; // Field-of-view = 60 degree
        public const int MaxDepth = 10;
        public const double Offset = 0.0000000001;
    }

    public class Scene
    {
        private SceneOptions options;
        private ISet<SceneEntity> entities;
        private ISet<PointLight> lights;

        /// <summary>
        /// Construct a new scene with provided options.
        /// </summary>
        /// <param name="options">Options data</param>
        public Scene(SceneOptions options = new SceneOptions())
        {
            this.options = options;
            this.entities = new HashSet<SceneEntity>();
            this.lights = new HashSet<PointLight>();
        }

        /// <summary>
        /// Add an entity to the scene that should be rendered.
        /// </summary>
        /// <param name="entity">Entity object</param>
        public void AddEntity(SceneEntity entity)
        {
            this.entities.Add(entity);
        }

        /// <summary>
        /// Add a point light to the scene that should be computed.
        /// </summary>
        /// <param name="light">Light structure</param>
        public void AddPointLight(PointLight light)
        {
            this.lights.Add(light);
        }

        /// <summary>
        /// Render the scene to an output image. This is where the bulk
        /// of your ray tracing logic should go... though you may wish to
        /// break it down into multiple functions as it gets more complex!
        /// </summary>
        /// <param name="outputImage">Image to store render output</param>
        public void Render(Image outputImage)
        {
            // Begin writing your code here...
            for (int y = 0; y < outputImage.Height; y++)
            {
                for (int x = 0; x < outputImage.Width; x++)
                {
                    Color color = new Color(0, 0, 0);
                    // Implement anti-aliasing and increase image resolution
                    double AAMultiplier = options.AAMultiplier;
                    for (int j = 0; j < AAMultiplier; j++)
                    {
                        for (int i = 0; i < AAMultiplier; i++)
                        {
                            double size = 1 / AAMultiplier;
                            Ray ray = CameraRay(x + i * size, y + j * size, outputImage);
                            SceneEntity entity = FirstEntityHit(ray);
                            if (entity == null)
                            {
                                continue;
                            }

                            RayHit hit = entity.Intersect(ray);
                            color += CastRay(hit, entity, new Color(0, 0, 0), 0);
                        }
                    }
                    color /= (AAMultiplier * AAMultiplier);
                    outputImage.SetPixel(x, y, color);
                }
            }
        }

        private Color CastRay(RayHit hit, SceneEntity entity, Color color, int depth)
        {
            if (depth > Constants.MaxDepth)
            {
                return NormalizeColor(color);
            }

            if (entity.Material.Type == Material.MaterialType.Reflective)
            {
                Vector3 reflectDirection = hit.Reflect();
                // Offset ray origin slightly away from the surface to prevent premature hit
                Vector3 origin = hit.Position + Constants.Offset * reflectDirection;
                Ray ray = new Ray(origin, reflectDirection);
                SceneEntity newEntity = FirstEntityHit(ray);
                if (newEntity != null)
                {
                    RayHit newHit = newEntity.Intersect(ray);
                    depth++;
                    return CastRay(newHit, newEntity, color, depth);
                }
                return NormalizeColor(color);
            }
            else if (entity.Material.Type == Material.MaterialType.Refractive)
            {
                double kr = Fresnel(hit, entity.Material);

                // Reflect
                Vector3 reflectDirection = hit.Reflect();
                Vector3 reflectOrigin = hit.Position + Constants.Offset * reflectDirection;
                Ray reflectRay = new Ray(reflectOrigin, reflectDirection);
                SceneEntity reflectEntity = FirstEntityHit(reflectRay);
                Color reflectColor = new Color(0, 0, 0);
                if (reflectEntity != null)
                {
                    RayHit reflectHit = reflectEntity.Intersect(reflectRay);
                    depth++;
                    reflectColor = CastRay(reflectHit, reflectEntity, reflectColor, depth);
                }

                // Refract
                Vector3 refractDirection = hit.Refract(entity.Material);
                Vector3 refractOrigin = hit.Position + Constants.Offset * refractDirection;
                Ray refractRay = new Ray(refractOrigin, refractDirection);
                SceneEntity refractEntity = FirstEntityHit(refractRay);
                Color refractColor = new Color(0, 0, 0);
                if (refractEntity != null)
                {
                    RayHit refractHit = refractEntity.Intersect(refractRay);
                    depth++;
                    refractColor = CastRay(refractHit, refractEntity, refractColor, depth);
                }

                color = reflectColor * kr + refractColor * (1 - kr);
                return NormalizeColor(color);
            }
            else
            {
                foreach (PointLight light in this.lights)
                {
                    Vector3 L = (light.Position - hit.Position).Normalized();
                    // Check if light is blocked
                    if (!LightIsBlocked(hit.Position, light.Position))
                    {
                        color += entity.Material.Color * light.Color * hit.Normal.Dot(L);
                    }
                }
                return NormalizeColor(color);
            }
        }

        private double Fresnel(RayHit hit, Material material)
        {
            double kr;
            double cosi = hit.Incident.Dot(hit.Normal);
            double etai = 1;
            double etat = material.RefractiveIndex;
            if (cosi > 0)
            {
                double temp = etai;
                etai = etat;
                etat = temp;
            }
            // Compute sini using Snell's law
            double sint = etai / etat * Math.Sqrt(Math.Max(0, 1 - cosi * cosi));
            // Total internal reflection
            if (sint >= 1)
            {
                kr = 1;
            }
            else
            {
                double cost = Math.Sqrt(Math.Max(0, 1 - sint * sint));
                cosi = Math.Abs(cosi);
                double Rs = ((etat * cosi) - (etai * cost)) / ((etat * cosi) + (etai * cost));
                double Rp = ((etai * cosi) - (etat * cost)) / ((etai * cosi) + (etat * cost));
                kr = (Rs * Rs + Rp * Rp) / 2;
            }
            return kr;
        }

        private SceneEntity FirstEntityHit(Ray ray)
        {
            SceneEntity firstEntity = null;
            double minDistanceSq = Double.MaxValue;
            foreach (SceneEntity entity in this.entities)
            {
                RayHit hit = entity.Intersect(ray);
                if (hit != null)
                {
                    double distanceSq = (hit.Position - ray.Origin).LengthSq();
                    if (distanceSq < minDistanceSq && distanceSq != 0)
                    {
                        minDistanceSq = distanceSq;
                        firstEntity = entity;
                    }
                }
            }
            return firstEntity;
        }

        private Boolean LightIsBlocked(Vector3 hitPosition, Vector3 lightPosition)
        {
            Vector3 hitToLight = lightPosition - hitPosition;
            Vector3 direction = hitToLight.Normalized();
            Vector3 origin = hitPosition + Constants.Offset * direction;
            // Fire another ray from hit point to light source
            Ray ray = new Ray(origin, direction);
            foreach (SceneEntity entity in this.entities)
            {
                RayHit hit = entity.Intersect(ray);
                // Check if the ray hits a closer surface to make sure ray does not go past the light
                if (hit != null && (hit.Position - hitPosition).LengthSq() < hitToLight.LengthSq())
                {
                    return true;
                }
            }
            return false;
        }

        private Color NormalizeColor(Color color)
        {
            return new Color(Math.Max(color.R, 0), Math.Max(color.G, 0), Math.Max(color.B, 0));
        }

        private Ray CameraRay(double x, double y, Image outputImage)
        {
            // Normalise the pixel space and pixels in the middles
            double pixelX = (x + 0.5) / outputImage.Width;
            double pixelY = (y + 0.5) / outputImage.Height;
            double pixelZ = 1.0;

            // Make it between -1 and 1
            pixelX = (pixelX * 2) - 1;
            pixelY = 1 - (pixelY * 2);

            // Scale y axis wrt the aspect ratio
            double aspectRatio = (double)outputImage.Width / outputImage.Height;
            pixelX = pixelX * Math.Tan(Constants.fov);
            pixelY = pixelY * (Math.Tan(Constants.fov) / aspectRatio);

            Vector3 direction = new Vector3(pixelX, pixelY, pixelZ).Normalized();

            return new Ray(new Vector3(0, 0, 0), direction);
        }
    }
}
