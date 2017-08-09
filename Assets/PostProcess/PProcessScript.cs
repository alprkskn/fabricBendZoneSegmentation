using UnityEngine;
using System.Collections;
 
[ExecuteInEditMode]
public class PProcessScript : MonoBehaviour {
 
	public float intensity;
	private Material material;
    private Camera camera;

    // Creates a private material used to the effect
    void Awake ()
	{
        camera = GetComponent<Camera>();
		material = new Material( Shader.Find("Hidden/BWDiffuse") );
        camera.depthTextureMode = DepthTextureMode.DepthNormals;

	}
	
	// Postprocess the image
	void OnRenderImage (RenderTexture source, RenderTexture destination)
	{
		//if (intensity == 0)
		//{
		//	Graphics.Blit (source, destination);
		//	return;
		//}
 
		material.SetFloat("_bwBlend", intensity);
		Graphics.Blit (source, destination, material);
	}
}