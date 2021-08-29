using System;
using System.Collections.Generic;

namespace RayTracer
{
    /// <summary>
    /// Class to represent a ray traced scene, including the objects,
    /// light sources, and associated rendering logic.
    /// </summary>
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
            // // Begin writing your code here...
            // int width = outputImage.Width;
            // int height = outputImage.Height;
            // int numPixels = width * height;
            // Color white = new Color(255, 255, 255);

            // for (int pid = 0; pid < numPixels; pid++)
            // {
            //     outputImage.SetPixel(pid, white);
            // }

            // Stage 1.3 - Fire a ray to each pixel
            Vector3 origin = new Vector3(0, 0, 0);
            Color black = new Color(0, 0, 0);

            // Relate pixel space to world space 
            // Loop through the pixels and normalise the pixel space
            for (int y = 0; y < outputImage.Height; y++)
            {
                for (int x = 0; x < outputImage.Width; x++)
                {
                    Ray ray = CameraRay(x, y, outputImage);
                    SceneEntity entity = FirstEntityHit(ray);
                    if (entity == null)
                    {
                        outputImage.SetPixel(x, y, black);
                        continue;
                    }

                    RayHit hit = entity.Intersect(ray);
                    outputImage.SetPixel(x, y, CastRay(hit, entity, new Color(0, 0, 0), 0));
                }
            }
        }
        private Color CastRay(RayHit hit, SceneEntity entity, Color color, int depth)
        {
            int maxDepth = 5;

            if (depth > maxDepth)
            {
                return NormalizeColor(color);
            }

            if (entity.Material.Type == Material.MaterialType.Reflective)
            {
                Ray ray = new Ray(hit.Position, hit.Reflect());
                SceneEntity newEntity = FirstEntityHit(ray);

                if (newEntity != null)
                {
                    RayHit newHit = newEntity.Intersect(ray);
                    depth += 1;
                    return CastRay(newHit, newEntity, color, depth);
                }
                return NormalizeColor(color);
            }
            else if (entity.Material.Type == Material.MaterialType.Refractive)
            {
                Ray ray = new Ray(hit.Position + 0.0000000001 * hit.Incident, hit.Refract(entity.Material));
                SceneEntity newEntity = FirstEntityHit(ray);

                if (newEntity != null)
                {
                    RayHit newHit = newEntity.Intersect(ray);
                    depth += 1;
                    return CastRay(newHit, newEntity, color, depth);
                }
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
            double offset = 0.0000000001;
            // Fire another ray from hit point to light source
            // Add offset to ray origin
            Ray ray = new Ray(hitPosition + offset * direction, direction);
            foreach (SceneEntity entity in this.entities)
            {
                RayHit hit = entity.Intersect(ray);
                // Check if the ray hits a closer surface
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

        private Ray CameraRay(int x, int y, Image outputImage)
        {
            // Normalise the pixel space and pixels in the middles
            double pixelX = (x + 0.5) / outputImage.Width;
            double pixelY = (y + 0.5) / outputImage.Height;
            double pixelZ = 1.0;

            // Make it between -1 and 1
            pixelX = (pixelX * 2) - 1;
            pixelY = 1 - (pixelY * 2);

            // Apply fov = 60 degree to x and y
            // Scale y axis wrt the aspect ratio
            double aspectRatio = (double)outputImage.Width / outputImage.Height;
            pixelX = pixelX * Math.Tan(Math.PI / 6);
            pixelY = pixelY * (Math.Tan(Math.PI / 6) / aspectRatio);

            Vector3 direction = new Vector3(pixelX, pixelY, pixelZ).Normalized();

            return new Ray(new Vector3(0, 0, 0), direction);
        }
    }
}
