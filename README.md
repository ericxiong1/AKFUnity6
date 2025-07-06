# Anisotropic Kuwahara Filter for the Universal Render Pipeline

A real-time Anisotropic Kuwahara Filter implemented in Unity 6 for edge-preserving image abstraction and stylization. Ideal for achieving painterly or toon-like effects while retaining sharp structure along image edges.

This code is built off of Acerola's [Anisotropic Kuwahara Filter](https://github.com/GarrettGunnell/Post-Processing) ported to Unity 6000.1.8f1 URP.

The implementation is based around the Scriptable Render Pipeline and Renderer Features, as this filter has multiple passes (which frustratingly, base URP does not support).

# Installation
Clone repository or download as .zip.

Drag and drop the 2 scripts and shader anywhere within the Project menu of your project.

Assign AnisotropicKuwaharaRenderFeature.cs to your Universal Renderer Data Inspector.

# Example

## With

![image](https://github.com/user-attachments/assets/b9148560-878c-4327-99c1-146b7d289e02)


## Without

![image](https://github.com/user-attachments/assets/4ef909fb-f46b-4b18-b8f7-e3cf76633fc7)
