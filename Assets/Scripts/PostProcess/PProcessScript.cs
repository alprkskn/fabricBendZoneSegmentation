using UnityEngine;
using System.Collections;
 
[ExecuteInEditMode]
public class PProcessScript : MonoBehaviour {
 
	public float intensity;
    public float angleThreshold = 80, depthWeight = 300;
    public int kernelRadius = 1;
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
		material.SetFloat("_angleThreshold", angleThreshold);
		material.SetFloat("_depthWeight", depthWeight);
        material.SetFloat("_kernelRadius", kernelRadius);
		Graphics.Blit (source, destination, material);
	}
}