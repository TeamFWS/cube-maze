using UnityEngine;

public class SkyManager : MonoBehaviour
{
    private static readonly int Rotation = Shader.PropertyToID("_Rotation");

    [SerializeField] private float skySpeed;

    private void Update()
    {
        RenderSettings.skybox.SetFloat(Rotation, Time.time * skySpeed);
    }
}