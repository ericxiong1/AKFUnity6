# Anisotropic Kuwahara Filter for the Universal Render Pipeline

A real-time Anisotropic Kuwahara Filter implemented in Unity 6 for edge-preserving image abstraction and stylization. Ideal for achieving painterly or toon-like effects while retaining sharp structure along image edges.

This code is built off of Acerola's [Anisotropic Kuwahara Filter](https://github.com/GarrettGunnell/Post-Processing) ported to Unity 6000.1.8f1 URP.

The implementation is based around the Scriptable Render Pipeline and Renderer Features, as this filter has multiple passes (which frustratingly, base URP does not support).

# Installation
Clone repository or download as .zip.

Drag and drop the 2 scripts and shader anywhere within the Project menu of your project.

Assign AnisotropicKuwaharaRenderFeature.cs to your Universal Renderer Data Inspector.

# Example
![Screenshot 2025-07-05 222346](https://github.com/user-attachments/assets/4ea8f12b-56e2-4766-908e-65e8bf0cc40d)
