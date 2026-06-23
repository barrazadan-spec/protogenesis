using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.V5
{
    /// <summary>
    /// Lightweight procedural morphology layer for V5 cells. It adds readable biological silhouettes
    /// without requiring imported art: flagella, capsule halos, fimbrias, cilia, hyphae and thylakoid bands.
    /// It is intentionally visual-only and can be disabled safely.
    /// </summary>
    [DisallowMultipleComponent]
    public class V5CellMorphologyRenderer : MonoBehaviour
    {
        private V5CellEntity cell;
        private readonly List<LineRenderer> lines = new List<LineRenderer>(48);
        private SpriteRenderer capsuleHalo;
        private SpriteRenderer thylakoidCore;
        private static Material lineMaterial;
        private static Sprite circleSprite;
        private float nextRefresh;
        private int lastSignature = -1;

        private void Awake()
        {
            cell = GetComponent<V5CellEntity>();
            if (lineMaterial == null)
            {
                Shader shader = Shader.Find("Sprites/Default");
                if (shader != null) lineMaterial = new Material(shader);
            }
            if (circleSprite == null) circleSprite = V5ProceduralSprites.CreateCircleSprite(64);
            EnsureSpriteChild(ref capsuleHalo, "morph_capsule_halo", 0);
            EnsureSpriteChild(ref thylakoidCore, "morph_thylakoid_core", 1);
        }

        private void LateUpdate()
        {
            if (cell == null) return;
            if (Time.time >= nextRefresh || Signature() != lastSignature)
            {
                nextRefresh = Time.time + 0.25f;
                Rebuild();
            }
            Animate();
        }

        private int Signature()
        {
            int sig = (int)cell.EvolutionPath * 131 + (int)cell.Domain * 17 + cell.Structures.Count;
            for (int i = 0; i < cell.Structures.Count; i++) sig = sig * 31 + (int)cell.Structures[i];
            if (cell.IsPlayerOwned)
            {
                V5GameManager gm = V5GameManager.Instance;
                if (gm != null && gm.Adaptations != null)
                {
                    sig = sig * 31 + gm.Adaptations.Installed.Count;
                    for (int i = 0; i < gm.Adaptations.Installed.Count; i++) sig = sig * 31 + (int)gm.Adaptations.Installed[i];
                }
            }
            return sig;
        }

        private void Rebuild()
        {
            lastSignature = Signature();
            int needed = RequiredLines();
            EnsureLineCount(needed);
            for (int i = 0; i < lines.Count; i++) lines[i].gameObject.SetActive(i < needed);

            Color baseColor = V5EvolutionLibrary.ColorForPath(cell.EvolutionPath);
            if (cell.EvolutionPath == V5EvolutionPath.Uncommitted) baseColor = V5Colors.LUCA;
            if (!cell.IsPlayerOwned) baseColor = Color.Lerp(baseColor, Color.red, 0.35f);

            bool capsule = cell.HasStructure(V5StructureId.Capsule) ||
                           cell.HasStructure(V5StructureId.PeptidoglycanWall) ||
                           cell.HasStructure(V5StructureId.CelluloseWall) ||
                           HasAdaptation(V5AdaptationId.PolysaccharideCapsule) ||
                           HasAdaptation(V5AdaptationId.BacterialWall) ||
                           HasAdaptation(V5AdaptationId.CelluloseWall) ||
                           HasAdaptation(V5AdaptationId.SilicaFrustule) ||
                           HasAdaptation(V5AdaptationId.ExtremophileMembrane);
            capsuleHalo.gameObject.SetActive(capsule);
            if (capsule)
            {
                bool heavyShell = cell.HasStructure(V5StructureId.Capsule) || HasAdaptation(V5AdaptationId.PolysaccharideCapsule) || HasAdaptation(V5AdaptationId.SilicaFrustule);
                capsuleHalo.color = new Color(baseColor.r, baseColor.g, baseColor.b, heavyShell ? 0.20f : 0.12f);
                capsuleHalo.transform.localScale = Vector3.one * (heavyShell ? 1.55f : 1.28f);
            }

            bool thylakoid = cell.HasStructure(V5StructureId.Thylakoid) ||
                              cell.HasStructure(V5StructureId.MicroalgalChloroplast) ||
                              cell.HasPhotosynthesis ||
                              HasAdaptation(V5AdaptationId.ProkaryoticThylakoid) ||
                              HasAdaptation(V5AdaptationId.Chloroplast);
            thylakoidCore.gameObject.SetActive(thylakoid);
            if (thylakoid)
            {
                thylakoidCore.color = new Color(0.20f, 0.85f, 0.24f, 0.35f);
                thylakoidCore.transform.localScale = new Vector3(0.72f, 0.28f, 1f);
                thylakoidCore.transform.localRotation = Quaternion.Euler(0f, 0f, 22f);
            }

            int index = 0;
            if (cell.HasStructure(V5StructureId.BacterialFlagellum) || HasAdaptation(V5AdaptationId.BacterialFlagellum))
            {
                ConfigureLine(lines[index++], baseColor, 0.055f, 9, 1.35f, 0f, false);
            }
            if (cell.HasStructure(V5StructureId.EukaryoticFlagellum) || HasAdaptation(V5AdaptationId.EukaryoticFlagellum))
            {
                ConfigureLine(lines[index++], baseColor, 0.070f, 14, 2.0f, 180f, true);
            }
            if (cell.HasStructure(V5StructureId.Fimbriae) || HasAnyAdaptation(V5AdaptationId.PiliFimbriae, V5AdaptationId.BasicAdhesin, V5AdaptationId.ColonialAdhesin))
            {
                for (int i = 0; i < 16; i++) ConfigureSpine(lines[index++], baseColor, i, 16, 0.26f, 0.025f);
            }
            if (cell.HasStructure(V5StructureId.Cilia) || HasAdaptation(V5AdaptationId.Cilia))
            {
                for (int i = 0; i < 24; i++) ConfigureSpine(lines[index++], baseColor, i, 24, 0.38f, 0.030f);
            }
            if (cell.HasStructure(V5StructureId.CoronaCilia))
            {
                for (int i = 0; i < 18; i++) ConfigureSpine(lines[index++], baseColor, i, 18, 0.55f, 0.036f);
            }
            if (cell.HasStructure(V5StructureId.InvasiveHypha) || HasAnyAdaptation(V5AdaptationId.FungalHypha, V5AdaptationId.ExtracellularEnzymes))
            {
                ConfigureHypha(lines[index++], baseColor, -34f, 1.85f);
                ConfigureHypha(lines[index++], baseColor, 28f, 1.65f);
                ConfigureHypha(lines[index++], baseColor, 86f, 1.25f);
            }
            if (cell.HasStructure(V5StructureId.MucilageMatrix) || HasAnyAdaptation(V5AdaptationId.SlimePlasmodium, V5AdaptationId.ChemicalMemory))
            {
                ConfigureHypha(lines[index++], baseColor, -72f, 1.25f);
                ConfigureHypha(lines[index++], baseColor, -18f, 1.75f);
                ConfigureHypha(lines[index++], baseColor, 42f, 1.55f);
                ConfigureHypha(lines[index++], baseColor, 118f, 1.35f);
            }
            if (HasAdaptation(V5AdaptationId.Pseudopods))
            {
                ConfigureHypha(lines[index++], baseColor, -128f, 0.95f);
                ConfigureHypha(lines[index++], baseColor, -8f, 1.05f);
                ConfigureHypha(lines[index++], baseColor, 112f, 0.95f);
            }
            if (cell.HasStructure(V5StructureId.PiercingStylet))
            {
                ConfigureLine(lines[index++], baseColor, 0.045f, 2, 1.45f, 0f, false);
            }
        }

        private int RequiredLines()
        {
            int count = 0;
            if (cell.HasStructure(V5StructureId.BacterialFlagellum) || HasAdaptation(V5AdaptationId.BacterialFlagellum)) count += 1;
            if (cell.HasStructure(V5StructureId.EukaryoticFlagellum) || HasAdaptation(V5AdaptationId.EukaryoticFlagellum)) count += 1;
            if (cell.HasStructure(V5StructureId.Fimbriae) || HasAnyAdaptation(V5AdaptationId.PiliFimbriae, V5AdaptationId.BasicAdhesin, V5AdaptationId.ColonialAdhesin)) count += 16;
            if (cell.HasStructure(V5StructureId.Cilia) || HasAdaptation(V5AdaptationId.Cilia)) count += 24;
            if (cell.HasStructure(V5StructureId.CoronaCilia)) count += 18;
            if (cell.HasStructure(V5StructureId.InvasiveHypha) || HasAnyAdaptation(V5AdaptationId.FungalHypha, V5AdaptationId.ExtracellularEnzymes)) count += 3;
            if (cell.HasStructure(V5StructureId.MucilageMatrix) || HasAnyAdaptation(V5AdaptationId.SlimePlasmodium, V5AdaptationId.ChemicalMemory)) count += 4;
            if (HasAdaptation(V5AdaptationId.Pseudopods)) count += 3;
            if (cell.HasStructure(V5StructureId.PiercingStylet)) count += 1;
            return count;
        }

        private void Animate()
        {
            if (thylakoidCore != null && thylakoidCore.gameObject.activeSelf)
            {
                float pulse = 1f + Mathf.Sin(Time.time * 2.4f) * 0.08f;
                thylakoidCore.transform.localScale = new Vector3(0.72f * pulse, 0.28f, 1f);
            }
            if (capsuleHalo != null && capsuleHalo.gameObject.activeSelf)
            {
                float pulse = 1f + Mathf.Sin(Time.time * 1.2f) * 0.035f;
                capsuleHalo.transform.localScale = capsuleHalo.transform.localScale.normalized * pulse;
            }
        }

        private void EnsureSpriteChild(ref SpriteRenderer sr, string childName, int sortingOffset)
        {
            Transform t = transform.Find(childName);
            if (t == null)
            {
                GameObject go = new GameObject(childName);
                go.transform.SetParent(transform, false);
                t = go.transform;
            }
            sr = t.GetComponent<SpriteRenderer>();
            if (sr == null) sr = t.gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = circleSprite;
            sr.sortingOrder = sortingOffset - 2;
            sr.gameObject.SetActive(false);
        }

        private bool HasAdaptation(V5AdaptationId id)
        {
            if (cell == null || !cell.IsPlayerOwned) return false;
            V5GameManager gm = V5GameManager.Instance;
            return gm != null && gm.Adaptations != null && gm.Adaptations.Has(id);
        }

        private bool HasAnyAdaptation(params V5AdaptationId[] ids)
        {
            if (ids == null) return false;
            for (int i = 0; i < ids.Length; i++)
            {
                if (HasAdaptation(ids[i])) return true;
            }
            return false;
        }

        private void EnsureLineCount(int count)
        {
            while (lines.Count < count)
            {
                GameObject go = new GameObject("morph_line_" + lines.Count.ToString("00"));
                go.transform.SetParent(transform, false);
                LineRenderer lr = go.AddComponent<LineRenderer>();
                lr.useWorldSpace = false;
                lr.material = lineMaterial;
                lr.numCapVertices = 4;
                lr.numCornerVertices = 4;
                lr.sortingOrder = -1;
                lines.Add(lr);
            }
        }

        private void ConfigureLine(LineRenderer lr, Color color, float width, int points, float length, float angle, bool sinusoidal)
        {
            lr.positionCount = points;
            lr.startWidth = width;
            lr.endWidth = width * 0.45f;
            lr.startColor = new Color(color.r, color.g, color.b, 0.82f);
            lr.endColor = new Color(color.r, color.g, color.b, 0.35f);
            float radius = 0.52f;
            Quaternion rot = Quaternion.Euler(0f, 0f, angle);
            for (int i = 0; i < points; i++)
            {
                float t = points <= 1 ? 0f : i / (float)(points - 1);
                float x = -radius - t * length;
                float y = sinusoidal ? Mathf.Sin(t * Mathf.PI * 2.4f + Time.time * 5.0f) * 0.13f : Mathf.Sin(t * Mathf.PI * 6f + Time.time * 8f) * 0.035f;
                lr.SetPosition(i, rot * new Vector3(x, y, 0f));
            }
        }

        private void ConfigureSpine(LineRenderer lr, Color color, int index, int total, float length, float width)
        {
            float a = index * Mathf.PI * 2f / Mathf.Max(1, total);
            Vector3 start = new Vector3(Mathf.Cos(a) * 0.50f, Mathf.Sin(a) * 0.50f, 0f);
            Vector3 end = new Vector3(Mathf.Cos(a) * (0.50f + length), Mathf.Sin(a) * (0.50f + length), 0f);
            lr.positionCount = 2;
            lr.startWidth = width;
            lr.endWidth = width * 0.55f;
            lr.startColor = new Color(color.r, color.g, color.b, 0.58f);
            lr.endColor = new Color(color.r, color.g, color.b, 0.20f);
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
        }

        private void ConfigureHypha(LineRenderer lr, Color color, float angle, float length)
        {
            lr.positionCount = 5;
            lr.startWidth = 0.055f;
            lr.endWidth = 0.025f;
            lr.startColor = new Color(color.r, color.g, color.b, 0.75f);
            lr.endColor = new Color(color.r, color.g, color.b, 0.18f);
            Quaternion rot = Quaternion.Euler(0f, 0f, angle);
            lr.SetPosition(0, rot * new Vector3(0.45f, 0f, 0f));
            lr.SetPosition(1, rot * new Vector3(0.75f, 0.06f, 0f));
            lr.SetPosition(2, rot * new Vector3(1.05f, -0.04f, 0f));
            lr.SetPosition(3, rot * new Vector3(length, 0.08f, 0f));
            lr.SetPosition(4, rot * new Vector3(length + 0.25f, -0.12f, 0f));
        }
    }
}
