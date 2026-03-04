# RayTracer
**Physically Based Renderer**

Learning Roadmap --- C# / WPF

*A step-by-step guide from ray casting to full PBR*

**Milestone 1 --- Ray Caster**

The foundation. Renders colored spheres with flat shading and no
lighting --- the \'hello world\' of ray tracing.

**What you build**

-   Ray--sphere intersection math

-   A pinhole camera that shoots rays through pixels

-   PPM file output (plain text RGB --- trivially simple format)

**What to learn**

-   Vectors: dot product, cross product, normalization

-   Parametric rays: P(t) = O + t·D

-   Ray-sphere intersection: solving the quadratic equation

-   How a pinhole camera model works

**WPF**

Not needed yet. Write output to a .ppm file --- any image viewer can
open it. Keep focus on the math.

**Milestone 2 --- Lighting & Shading**

Spheres with diffuse shading, shadows, and a point light. First
recognizable 3D render.

**What you build**

-   Surface normal computation

-   Shadow rays (does anything block the light?)

-   Lambertian (diffuse) shading model

**What to learn**

-   Surface normals and how to compute them analytically

-   The rendering equation --- conceptual overview

-   Lambertian reflectance --- why it looks \'matte\'

-   Difference between local and global illumination

**Milestone 3 --- Path Tracing Core**

Soft shadows, color bleeding, indirect lighting --- the first physically
correct image. This is the heart of PBR.

**What you build**

-   Recursive ray bouncing

-   Monte Carlo integration

-   Russian roulette ray termination

-   Proper random number generator (System.Random or better)

**What to learn**

-   Monte Carlo integration --- why randomness gives correct integrals

-   The rendering equation formally (Kajiya 1986)

-   Probability density functions (PDFs)

-   Why many samples per pixel are needed (noise vs. convergence)

-   Importance sampling --- conceptual introduction

**WPF --- Introduce here**

A live preview window with WriteableBitmap lets you watch the image
converge sample by sample. Wire in Parallel.For over scan lines for a
near-free speedup that scales with CPU cores.

**Milestone 4 --- Materials**

Glass, mirrors, brushed metal. The classic Cornell Box scene is the
target render.

**What you build**

-   Perfect specular reflection (reflect() function)

-   Dielectric refraction --- Snell\'s law + Fresnel equations

-   A material system / abstraction layer

**What to learn**

-   Snell\'s law and index of refraction

-   Fresnel equations --- why glass is reflective at grazing angles

-   Schlick\'s approximation (cheap, good-enough Fresnel)

-   BRDFs --- what they are and why they matter

-   Energy conservation in materials

**Milestone 5 --- Acceleration & Meshes**

Complex 3D scenes with thousands of triangles at interactive-ish speeds.

**What you build**

-   Ray-triangle intersection (Möller--Trumbore algorithm)

-   OBJ file loader

-   Bounding Volume Hierarchy (BVH)

**What to learn**

-   Why naive O(n) per ray is unacceptably slow

-   BVH construction and traversal

-   AABB (axis-aligned bounding box) intersection

-   Surface area heuristic (SAH) for better BVH splits

**Milestone 6 --- Physically Based Materials (PBR)**

Photorealistic surfaces: rough metals, coated plastics, skin-like
scattering.

**What you build**

-   Cook-Torrance microfacet BRDF

-   GGX normal distribution function

-   Importance sampling for microfacets

-   Metalness / roughness material model

**What to learn**

-   Microfacet theory --- surfaces as collections of tiny mirrors

-   GGX / Trowbridge-Reitz distribution

-   Geometry shadowing/masking (Smith G term)

-   Importance sampling BRDFs to reduce noise

-   Physical difference between metals and dielectrics

**Milestone 7 --- Advanced Light Transport**

Caustics, participating media (fog/volumes), subsurface scattering.

**What you build**

-   Multiple importance sampling (MIS)

-   Direct light sampling

-   Volumetric rendering / homogeneous media

-   (Optional) Bidirectional path tracing

**What to learn**

-   Multiple importance sampling --- combining strategies optimally

-   The balance heuristic (Veach & Guibas)

-   Volume rendering equation

-   Phase functions (Henyey-Greenstein)

-   Why bidirectional methods help with difficult light paths (caustics)

**Suggested Resources**

  ------------------- ---------------------------- ----------------------
  **Milestone**       **Resource**                 **Format**

  M1--M3              Ray Tracing in One Weekend   Free online / PDF
                      (Shirley)                    

  M3--M4              Physically Based Rendering   pbrt.org --- free
                      (PBRT)                       online

  M5                  PBRT Chapter 4               Primitives & BVH

  M6                  Disney PBR course notes      Burley --- PDF
                      (2012)                       

  M6                  Naty Hoffman \'Background\'  SIGGRAPH slides
                      talk                         

  M7                  Veach thesis on MIS          Stanford --- PDF

  M7                  PBRT Chapters 14--15         Light transport
  ------------------- ---------------------------- ----------------------

**C# & WPF Implementation Notes**

**Core Type Design**

Use struct for hot-path types --- stack allocated, no GC pressure:

> public readonly struct Vector3 { \... }
>
> public readonly struct Ray { \... }
>
> public readonly struct Color { \... }

Write your own Vector3 initially --- understanding every operation is
the point. System.Numerics.Vector3 exists but saves nothing at this
stage.

**Suggested Project Structure**

> RayTracer/
>
> Core/
>
> Vector3.cs
>
> Ray.cs
>
> Color.cs
>
> Scene/
>
> IHittable.cs
>
> Sphere.cs
>
> HittableList.cs
>
> Materials/
>
> IMaterial.cs
>
> Output/
>
> PpmWriter.cs
>
> Renderer.cs

**Parallelism (Milestone 3+)**

Near-zero-effort speedup that scales with all CPU cores:

> Parallel.For(0, imageHeight, y =\> {
>
> for (int x = 0; x \< imageWidth; x++) {
>
> // render pixel (x, y)
>
> }
>
> });

Each row is independent, so this is trivially safe. Use thread-local
Random instances --- System.Random is not thread-safe.

**WPF Live Preview (Milestone 3+)**

Use WriteableBitmap as a render target. Update it incrementally as
samples accumulate:

> var bitmap = new WriteableBitmap(width, height, 96, 96,
> PixelFormats.Rgb24, null);
>
> // After each pass:
>
> bitmap.WritePixels(rect, pixelBuffer, stride, 0);

Run the renderer on a background Task and dispatch UI updates via
Dispatcher.InvokeAsync.

**Performance Tips**

-   Use Span\<T\> in the BVH for cache-friendly traversal (Milestone 5)

-   Prefer readonly struct + in parameters to avoid defensive copies

-   Profile with dotnet-trace or Visual Studio\'s CPU profiler before
    optimizing

-   Avoid LINQ in the hot path --- it allocates

-   Consider ArrayPool\<T\> for temporary buffers in inner loops
