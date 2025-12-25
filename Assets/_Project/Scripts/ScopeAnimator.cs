using UnityEngine;

public class ScopeAnimator : MonoBehaviour
{
    [SerializeField] private SystemModel system;
    [SerializeField] private float scrollSpeed = 0.2f;
    [SerializeField] private float minEmission = 0.3f;
    [SerializeField] private float maxEmission = 1.5f;

    private Renderer rend;
    private Material mat;
    private Vector2 baseOffset;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        if (rend == null) return;

        // material instance so we don't edit the shared one
        mat = rend.material;
        baseOffset = mat.mainTextureOffset;
    }

    private void Update()
    {
        if (mat == null) return;

        float x = baseOffset.x + Time.time * scrollSpeed;
        mat.mainTextureOffset = new Vector2(x, baseOffset.y);

        if (system != null && mat.HasProperty("_EmissionColor"))
        {
            float t = Mathf.InverseLerp(system.flowRange.x, system.flowRange.y, system.flow);
            float intensity = Mathf.Lerp(minEmission, maxEmission, t);
            Color baseColor = mat.GetColor("_Color");
            mat.SetColor("_EmissionColor", baseColor * intensity);
        }
    }
}
