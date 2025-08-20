using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class SceneTransitioner : MonoBehaviour
{
    private static GameObject fadeSpherePrefab;
    private static GameObject activeFadeSphere;
    private static Material fadeMaterial;
    private static bool isFading;
    [SerializeField] private float fadeSpeed = 1.5f;
    [SerializeField] private string firstScene;
    [SerializeField] private string secondScene;
    [SerializeField] private string thirdScene;

    private void Awake()
    {
        if (fadeSpherePrefab == null) InitializeFadeSystem();
        // Make this object persistent
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (!isFading && activeFadeSphere != null) activeFadeSphere.SetActive(false);
    }

    public void GoToFirstScene()
    {
        if (!isFading) StartCoroutine(FadeAndLoadScene(firstScene));
    }

    public void GoToSecondScene()
    {
        if (!isFading) StartCoroutine(FadeAndLoadScene(secondScene));
    }

    public void GoToThirdScene()
    {
        if (!isFading) StartCoroutine(FadeAndLoadScene(thirdScene));
    }

    // I don't use this, but it's generally better
    public void GoToScene(string sceneName)
    {
        if (!isFading) StartCoroutine(FadeAndLoadScene(sceneName));
    }

    private void InitializeFadeSystem()
    {
        // Create a material for the fade effect
        fadeMaterial = new Material(Shader.Find("Standard"));
        fadeMaterial.SetFloat("_Mode", 3);
        fadeMaterial.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
        fadeMaterial.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
        fadeMaterial.SetInt("_ZWrite", 0);
        fadeMaterial.DisableKeyword("_ALPHATEST_ON");
        fadeMaterial.EnableKeyword("_ALPHABLEND_ON");
        fadeMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        fadeMaterial.renderQueue = 3000;
        fadeMaterial.color = Color.black;

        // Create the fade sphere prefab
        fadeSpherePrefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        fadeSpherePrefab.name = "FadeSphere";
        fadeSpherePrefab.GetComponent<MeshRenderer>().material = fadeMaterial;

        // Make the sphere inverse-normals (render on inside)
        var meshFilter = fadeSpherePrefab.GetComponent<MeshFilter>();
        var normals = meshFilter.mesh.normals;
        for (var i = 0; i < normals.Length; i++) normals[i] = -normals[i];
        meshFilter.mesh.normals = normals;

        // Optimize the sphere for the fade effect
        fadeSpherePrefab.GetComponent<SphereCollider>().enabled = false;
        fadeSpherePrefab.transform.localScale = new Vector3(100000f, 100000f, 100000f);

        // Hide the prefab
        fadeSpherePrefab.SetActive(false);
        DontDestroyOnLoad(fadeSpherePrefab);
    }

    private void CreateFadeSphere()
    {
        if (activeFadeSphere == null)
        {
            activeFadeSphere = Instantiate(fadeSpherePrefab);
            DontDestroyOnLoad(activeFadeSphere);
        }

        activeFadeSphere.SetActive(true);
        UpdateFadeSpherePosition();
    }

    private void UpdateFadeSpherePosition()
    {
        if (activeFadeSphere != null)
        {
            var mainCamera = Camera.main;
            if (mainCamera != null) activeFadeSphere.transform.position = mainCamera.transform.position;
        }
    }

    private IEnumerator FadeAndLoadScene(string sceneToLoad)
    {
        isFading = true;
        CreateFadeSphere();
        yield return StartCoroutine(FadeOut());

        SceneManager.LoadScene(sceneToLoad);
        CreateFadeSphere();
        UpdateFadeSpherePosition();
        yield return StartCoroutine(FadeIn());

        if (activeFadeSphere != null) activeFadeSphere.SetActive(false);

        isFading = false;
    }

    private IEnumerator FadeOut()
    {
        float elapsedTime = 0;
        var currentColor = fadeMaterial.color;

        while (elapsedTime < fadeSpeed)
        {
            elapsedTime += Time.deltaTime;
            var alpha = Mathf.Clamp01(elapsedTime / fadeSpeed);
            fadeMaterial.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
            UpdateFadeSpherePosition();
            yield return null;
        }
    }

    private IEnumerator FadeIn()
    {
        float elapsedTime = 0;
        var currentColor = fadeMaterial.color;

        while (elapsedTime < fadeSpeed)
        {
            elapsedTime += Time.deltaTime;
            var alpha = 1 - Mathf.Clamp01(elapsedTime / fadeSpeed);
            fadeMaterial.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
            UpdateFadeSpherePosition();
            yield return null;
        }
    }
}