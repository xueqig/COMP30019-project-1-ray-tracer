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

            // Relate pixel space to world space 
            // Loop through the pixels and normalise the pixel space
            for (int y = 0; y < outputImage.Height; y++)
            {
                for (int x = 0; x < outputImage.Width; x++)
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

                    Ray ray = new Ray(origin, direction);

                    outputImage.SetPixel(x, y, new Color(0, 0, 0));
                    double t = 0;
                    foreach (SceneEntity entity in this.entities)
                    {
                        RayHit hit = entity.Intersect(ray);
                        if (hit != null)
                        {
                            // We got a hit with this entity!
                            // The colour of the entity is entity.Material.Color
                            // Apply diffuse
                            double t1 = hit.Position.LengthSq();
                            if (t == 0 || t1 < t)
                            {
                                Color finalColor = new Color(0, 0, 0);
                                foreach (PointLight light in this.lights)
                                {
                                    Vector3 L = (light.Position - hit.Position).Normalized();
                                    Color color = entity.Material.Color * light.Color * hit.Normal.Dot(L);
                                    finalColor += color;
                                }
                                if (finalColor.R < 0)
                                {
                                    finalColor = new Color(0, finalColor.G, finalColor.B);
                                }
                                if (finalColor.G < 0)
                                {
                                    finalColor = new Color(finalColor.R, 0, finalColor.B);
                                }
                                if (finalColor.B < 0)
                                {
                                    finalColor = new Color(finalColor.R, finalColor.G, 0);
                                }
                                outputImage.SetPixel(x, y, finalColor);
                            }
                        }
                    }
                }
            }
        }

    }
}

// https://www.scratchapixel.com/lessons/3d-basic-rendering/ray-tracing-generating-camera-rays/generating-camera-rays