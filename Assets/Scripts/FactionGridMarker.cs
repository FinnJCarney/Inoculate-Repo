using NUnit.Framework.Constraints;
using NUnit.Framework.Interfaces;
using System.Collections;
using System.Data.SqlTypes;
using UnityEngine;

public class FactionGridMarker : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        defaultPos = this.transform.localPosition;
        defaultRotation = this.transform.localRotation;
        spriteRenderer = GetComponent<SpriteRenderer>();
        neutralColor = spriteRenderer.color;
        defaultAlpha = neutralColor.a;
        baseColor = neutralColor;     
    }

    // Update is called once per frame
    void Update()
    {
        float timeValue = Time.time * 0.5f;
        if(faction == Faction.Clashing)
        {
            timeValue *= 2f;
        }

        float perlinValue = (Mathf.PerlinNoise(this.transform.position.x * 0.1f+ timeValue, this.transform.position.z * 0.1f + (timeValue * 1.2f)) * 2f) - 1f;
        perlinValue += adjustmentOffset;
        this.transform.localPosition = defaultPos + (Vector3.up * perlinValue * 0.66f);
        perlinValue *= 0.1f;
        spriteRenderer.color = baseColor + new Color(perlinValue, perlinValue, perlinValue, 0f);
    }

    public void SetFaction(Faction newFaction)
    {
        if(newFaction == faction)
        {
            return;
        }

        faction = newFaction;
        StopCoroutine(ChangeFaction());
        StartCoroutine(ChangeFaction());
    }

    private IEnumerator ChangeFaction()
    {
        Color newColor = Color.black;

        if (faction == Faction.Neutral && faction != Faction.Clashing)
        {
            newColor = neutralColor;
        }
        else if (faction == Faction.Clashing)
        {
            newColor = clashingCol;
        }
        else
        {
            newColor = LevelManager.lM.levelFactions[faction].color;

            float distanceModifier = 4f - Mathf.Pow(Vector2.Distance(new Vector2(this.transform.position.x, this.transform.position.z), LevelManager.lM.levelFactions[faction].mainPosition) / 9f, 3f);
            distanceModifier = Mathf.Clamp(distanceModifier, 1f, 3f);        
            newColor.a = defaultAlpha * distanceModifier;
        }

        for (float time = 0f; time < 0.5f; time += Time.deltaTime)
        {
            if(time < 0.1f)
            {
                adjustmentOffset -= (30f * Time.deltaTime);
            }
            else
            {
                adjustmentOffset += (7.5f * Time.deltaTime);
            }
            baseColor = Color.Lerp(baseColor, newColor, time / 0.5f);
            yield return null;
        }

        adjustmentOffset = 0f;
    }

    public Faction faction;

    private float adjustmentOffset;

    private Vector3 defaultPos;
    private Quaternion defaultRotation;
    private float defaultAlpha;
    private Color baseColor;
    private Color neutralColor;

    [SerializeField] private Color clashingCol;

    private SpriteRenderer spriteRenderer;
}
