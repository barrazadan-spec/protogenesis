using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.V5
{
    public enum V5ThreatArchetype
    {
        Drifter,
        Hunter,
        Bloomer,
        Grazer,
        Acidophile,
        PhageCloud
    }

    public class V5ThreatEcologySystem : MonoBehaviour, IV5RunResettable
    {
        public bool ShowPanel;
        public bool AdaptiveThreats = true;
        public float ThreatPressure;
        public float AwakeningScore;
        public string LastMessage = "Z: ecología de amenazas. Las amenazas mutan según el ambiente y tu dominancia.";
        private readonly Dictionary<int, V5ThreatArchetype> archetypes = new Dictionary<int, V5ThreatArchetype>();
        private readonly HashSet<int> tuned = new HashSet<int>();
        private GUIStyle box;
        private GUIStyle title;
        private GUIStyle button;
        private float tick;
        private float spawnTimer;

        private void Start() => V5PanelRouter.Register("Amenazas", () => ShowPanel, v => ShowPanel = v);

        public void ResetForNewRun()
        {
            ThreatPressure = 0f;
            AwakeningScore = 0f;
            LastMessage = "Ecosistema latente: la gota observa tu primera biologia.";
            archetypes.Clear();
            tuned.Clear();
            tick = 0f;
            spawnTimer = 0f;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Z)) { if (!ShowPanel) V5PanelRouter.CloseOthers("Amenazas"); ShowPanel = !ShowPanel; }
            if (!AdaptiveThreats) return;
            tick += Time.deltaTime;
            spawnTimer += Time.deltaTime;
            if (tick >= 1.0f)
            {
                tick = 0f;
                UpdateThreatPressure();
                TuneExistingThreats();
            }
            float interval = Mathf.Lerp(58f, 26f, Mathf.Clamp01(AwakeningScore));
            if (spawnTimer >= interval)
            {
                spawnTimer = 0f;
                TrySpawnAdaptiveThreat();
            }
        }

        public V5ThreatArchetype GetArchetype(V5CellEntity enemy)
        {
            if (enemy == null) return V5ThreatArchetype.Drifter;
            int id = enemy.GetInstanceID();
            V5ThreatArchetype a;
            if (archetypes.TryGetValue(id, out a)) return a;
            a = PickArchetype(enemy);
            archetypes[id] = a;
            return a;
        }

        private void UpdateThreatPressure()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null) return;
            AwakeningScore = V5EcologySpawnPolicy.EcologicalAwakening(gm);
            float colonization = gm.Environment != null ? gm.Environment.AverageColonization() : 0f;
            float enemies = Mathf.Clamp01(gm.NonPlayerCells.Count / 35f);
            float time = Mathf.Clamp01(gm.ElapsedSeconds / 1800f);
            float cells = Mathf.Clamp01(gm.PlayerCells.Count / 18f);
            ThreatPressure = Mathf.Clamp01((time * 0.16f + cells * 0.17f + colonization * 0.26f + enemies * 0.14f + AwakeningScore * 0.45f) * Mathf.Lerp(0.35f, 1f, AwakeningScore));
        }

        private void TuneExistingThreats()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null) return;
            for (int i = 0; i < gm.NonPlayerCells.Count; i++)
            {
                V5CellEntity enemy = gm.NonPlayerCells[i];
                if (enemy == null) continue;
                V5ThreatArchetype a = GetArchetype(enemy);
                if (!tuned.Contains(enemy.GetInstanceID()))
                {
                    ApplyArchetypeStats(enemy, a);
                    tuned.Add(enemy.GetInstanceID());
                }
                ApplyArchetypeBehaviour(enemy, a);
            }
        }

        private V5ThreatArchetype PickArchetype(V5CellEntity enemy)
        {
            V5GameManager gm = V5GameManager.Instance;
            V5BiomeSystem biomes = gm != null ? gm.Biomes : null;
            V5BiomeType b = biomes != null ? biomes.SampleAt(enemy.transform.position).biome : V5BiomeType.NeutralBroth;
            if (b == V5BiomeType.ToxicBloom || b == V5BiomeType.AcidFrontier) return V5ThreatArchetype.Acidophile;
            if (b == V5BiomeType.PhoticShelf || b == V5BiomeType.OxygenBloom) return V5ThreatArchetype.Bloomer;
            if (b == V5BiomeType.DetritusField) return V5ThreatArchetype.Grazer;
            if (enemy.EvolutionPath == V5EvolutionPath.Amoeba || enemy.EvolutionPath == V5EvolutionPath.Flagellate || enemy.EvolutionPath == V5EvolutionPath.Rotifer || enemy.EvolutionPath == V5EvolutionPath.Nematode) return V5ThreatArchetype.Hunter;
            if (Random.value < 0.12f) return V5ThreatArchetype.PhageCloud;
            return V5ThreatArchetype.Drifter;
        }

        private void ApplyArchetypeStats(V5CellEntity enemy, V5ThreatArchetype a)
        {
            if (enemy == null) return;
            switch (a)
            {
                case V5ThreatArchetype.Hunter:
                    enemy.Stats.speed *= 1.22f;
                    enemy.Stats.physicalDamagePerSecond += 1.4f;
                    enemy.Stats.sensorRange += 3f;
                    enemy.Directive = V5Directive.Attack;
                    break;
                case V5ThreatArchetype.Bloomer:
                    enemy.Stats.colonizationPower += 0.9f;
                    enemy.Stats.atpPerSecond += 0.5f;
                    enemy.Directive = V5Directive.Colonize;
                    break;
                case V5ThreatArchetype.Grazer:
                    enemy.Stats.synthesisRate *= 1.3f;
                    enemy.Stats.speed *= 0.94f;
                    enemy.Directive = V5Directive.Farm;
                    break;
                case V5ThreatArchetype.Acidophile:
                    enemy.Stats.toxinResistance += 0.45f;
                    enemy.Stats.phTolerance = 0.78f;
                    enemy.Stats.chemicalDamagePerSecond += 0.8f;
                    break;
                case V5ThreatArchetype.PhageCloud:
                    enemy.Stats.radius *= 0.72f;
                    enemy.Stats.speed *= 1.6f;
                    enemy.Stats.physicalDamagePerSecond += 0.8f;
                    enemy.Stats.currentHp *= 0.65f;
                    break;
                default:
                    enemy.Stats.speed *= 1.03f;
                    break;
            }
        }

        private void ApplyArchetypeBehaviour(V5CellEntity enemy, V5ThreatArchetype a)
        {
            if (enemy == null) return;
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null) return;
            if (a == V5ThreatArchetype.Bloomer || a == V5ThreatArchetype.Acidophile)
            {
                enemy.Directive = V5Directive.Colonize;
                if (gm.Environment != null)
                {
                    float toxin = a == V5ThreatArchetype.Acidophile ? 0.0035f : 0.0012f;
                    float acid = a == V5ThreatArchetype.Acidophile ? 0.004f : 0f;
                    gm.Environment.ModifyArea(enemy.transform.position, 1.8f, -0.0005f, 0f, 0.0008f, toxin, acid, -0.0015f, 0f);
                }
            }
            else if (a == V5ThreatArchetype.Grazer && gm.Environment != null)
            {
                gm.Environment.ModifyArea(enemy.transform.position, 1.4f, -0.003f, 0f, 0f, 0.0005f, 0f, -0.001f, 0.001f);
            }
        }

        private void TrySpawnAdaptiveThreat()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.CellFactory == null || gm.Environment == null) return;
            if (AwakeningScore < 0.18f) return;
            if (gm.ElapsedSeconds < V5EcologySpawnPolicy.HostileGraceSeconds(gm.ScenarioId) && AwakeningScore < 0.42f) return;
            int cap = 3 + Mathf.RoundToInt(AwakeningScore * 18f);
            if (gm.NonPlayerCells.Count > cap) return;
            if (Random.value > 0.10f + AwakeningScore * 0.32f + ThreatPressure * 0.24f) return;

            Vector2 pos = Random.insideUnitCircle.normalized * Random.Range(gm.Environment.MapRadius * 0.35f, gm.Environment.MapRadius * 0.88f);
            V5EvolutionPath path = PickPathForResponse(gm);
            V5CellEntity enemy = gm.CellFactory.SpawnNeutral(pos, path);
            if (enemy == null) return;
            bool hostile = AwakeningScore > 0.48f || ThreatPressure > 0.45f || gm.ScenarioId == V5ScenarioId.PredatorBloom;
            if (hostile && enemy.GetComponent<V5EnemyBrain>() == null) enemy.gameObject.AddComponent<V5EnemyBrain>();
            if (!hostile)
            {
                enemy.Directive = Random.value < 0.5f ? V5Directive.Explore : V5Directive.Colonize;
                enemy.Stats.physicalDamagePerSecond *= 0.35f;
                enemy.Stats.sensorRange *= 0.65f;
            }
            V5ThreatArchetype a = PickArchetype(enemy);
            archetypes[enemy.GetInstanceID()] = a;
            ApplyArchetypeStats(enemy, a);
            if (!hostile)
            {
                enemy.Directive = Random.value < 0.5f ? V5Directive.Explore : V5Directive.Colonize;
                enemy.AttackTarget = null;
                enemy.Stats.physicalDamagePerSecond *= 0.30f;
                enemy.Stats.chemicalDamagePerSecond *= 0.45f;
                enemy.Stats.sensorRange *= 0.70f;
            }
            LastMessage = (hostile ? "Amenaza adaptativa: " : "Vida emergente: ") + a + " / " + path + " | causa: " + EcologicalCause(gm);
            if (gm.Hud != null) gm.Hud.Toast(LastMessage);
        }

        private V5EvolutionPath PickPathForResponse(V5GameManager gm)
        {
            V5CellEntity mother = gm != null ? gm.MotherCell : null;
            V5AdaptationSystem adaptations = gm != null ? gm.Adaptations : null;
            if (adaptations != null)
            {
                if (HasAny(adaptations, V5AdaptationId.ProkaryoticThylakoid, V5AdaptationId.Chloroplast, V5AdaptationId.CelluloseWall, V5AdaptationId.SilicaFrustule))
                    return Random.value < 0.68f ? V5EvolutionPath.Cyanobacteria : V5EvolutionPath.Microalga;
                if (HasAny(adaptations, V5AdaptationId.SlimePlasmodium, V5AdaptationId.FungalHypha, V5AdaptationId.ExtracellularEnzymes, V5AdaptationId.ChemicalMemory))
                    return Random.value < 0.55f ? V5EvolutionPath.SlimeMold : V5EvolutionPath.Fungus;
                if (HasAny(adaptations, V5AdaptationId.Lysosome, V5AdaptationId.Pseudopods))
                    return Random.value < 0.58f ? V5EvolutionPath.Flagellate : V5EvolutionPath.Ciliate;
                if (HasAny(adaptations, V5AdaptationId.BacterialFlagellum, V5AdaptationId.EukaryoticFlagellum, V5AdaptationId.Cilia))
                    return Random.value < 0.55f ? V5EvolutionPath.Flagellate : V5EvolutionPath.Amoeba;
                if (HasAny(adaptations, V5AdaptationId.ProtonPump, V5AdaptationId.ExtremophileMembrane, V5AdaptationId.CatalaseROS))
                    return Random.value < 0.60f ? V5EvolutionPath.Archaea : V5EvolutionPath.Bacteria;
                if (HasAny(adaptations, V5AdaptationId.BacterialWall, V5AdaptationId.PolysaccharideCapsule, V5AdaptationId.BasicAdhesin, V5AdaptationId.PiliFimbriae))
                    return Random.value < 0.60f ? V5EvolutionPath.Bacteria : V5EvolutionPath.Flagellate;
            }
            if (mother != null)
            {
                if (mother.HasStructure(V5StructureId.Thylakoid) || mother.HasStructure(V5StructureId.MicroalgalChloroplast) || mother.Metabolism == V5MetabolismType.Photosynthesis)
                    return Random.value < 0.68f ? V5EvolutionPath.Cyanobacteria : V5EvolutionPath.Microalga;
                if (mother.HasStructure(V5StructureId.MucilageMatrix) || mother.HasStructure(V5StructureId.InvasiveHypha))
                    return Random.value < 0.55f ? V5EvolutionPath.SlimeMold : V5EvolutionPath.Fungus;
                if (mother.HasStructure(V5StructureId.BacterialFlagellum) || mother.HasStructure(V5StructureId.EukaryoticFlagellum) || mother.HasStructure(V5StructureId.Cilia))
                    return Random.value < 0.55f ? V5EvolutionPath.Flagellate : V5EvolutionPath.Amoeba;
                if (mother.Metabolism == V5MetabolismType.Chemolithotrophy || mother.Metabolism == V5MetabolismType.Fermentation)
                    return Random.value < 0.60f ? V5EvolutionPath.Archaea : V5EvolutionPath.Bacteria;
            }
            if (ThreatPressure < 0.28f) return Random.value < 0.65f ? V5EvolutionPath.Bacteria : V5EvolutionPath.Cyanobacteria;
            if (ThreatPressure < 0.58f) return Random.value < 0.45f ? V5EvolutionPath.Flagellate : V5EvolutionPath.Amoeba;
            float r = Random.value;
            if (r < 0.22f) return V5EvolutionPath.Amoeba;
            if (r < 0.42f) return V5EvolutionPath.Flagellate;
            if (r < 0.58f) return V5EvolutionPath.SlimeMold;
            if (r < 0.74f) return V5EvolutionPath.Rotifer;
            if (r < 0.88f) return V5EvolutionPath.Nematode;
            if (r < 0.96f) return V5EvolutionPath.Tardigrade;
            return V5EvolutionPath.Fungus;
        }

        private string EcologicalCause(V5GameManager gm)
        {
            if (gm == null) return "ecosistema";
            V5AdaptationSystem a = gm.Adaptations;
            if (a != null)
            {
                if (HasAny(a, V5AdaptationId.ProkaryoticThylakoid, V5AdaptationId.Chloroplast, V5AdaptationId.CelluloseWall, V5AdaptationId.SilicaFrustule)) return "exceso fotosintetico";
                if (HasAny(a, V5AdaptationId.SlimePlasmodium, V5AdaptationId.FungalHypha, V5AdaptationId.ExtracellularEnzymes, V5AdaptationId.ChemicalMemory)) return "red territorial";
                if (HasAny(a, V5AdaptationId.Lysosome, V5AdaptationId.Pseudopods)) return "depredacion activa";
                if (HasAny(a, V5AdaptationId.BacterialFlagellum, V5AdaptationId.EukaryoticFlagellum, V5AdaptationId.Cilia)) return "movilidad alta";
                if (HasAny(a, V5AdaptationId.ProtonPump, V5AdaptationId.ExtremophileMembrane, V5AdaptationId.CatalaseROS)) return "metabolismo extremo";
                if (HasAny(a, V5AdaptationId.BacterialWall, V5AdaptationId.PolysaccharideCapsule, V5AdaptationId.BasicAdhesin, V5AdaptationId.PiliFimbriae)) return "biofilm bacteriano";
            }
            if (gm.Identity != null && gm.Identity.Identity != V5IdentityId.LUCA) return gm.Identity.DisplayName;
            return "despertar " + (AwakeningScore * 100f).ToString("0") + "%";
        }

        private bool HasAny(V5AdaptationSystem adaptations, params V5AdaptationId[] ids)
        {
            if (adaptations == null || ids == null) return false;
            for (int i = 0; i < ids.Length; i++)
                if (adaptations.Has(ids[i])) return true;
            return false;
        }

        private void OnGUI()
        {
            if (!ShowPanel) return;
            EnsureStyles();
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null) return;
            Rect r = new Rect(10, Screen.height - 360, 430, 260);
            GUI.Box(r, "", box);
            GUI.Label(new Rect(r.x + 12, r.y + 10, r.width - 24, 24), "ECOLOGÍA DE AMENAZAS", title);
            GUI.Label(new Rect(r.x + 12, r.y + 38, r.width - 24, 24), "Despertar: " + (AwakeningScore * 100f).ToString("0") + "% | Presión: " + (ThreatPressure * 100f).ToString("0") + "% | NPC: " + gm.NonPlayerCells.Count);
            AdaptiveThreats = GUI.Toggle(new Rect(r.x + 12, r.y + 66, 220, 24), AdaptiveThreats, "Amenazas adaptativas");
            if (GUI.Button(new Rect(r.x + 250, r.y + 64, 150, 28), "Forzar amenaza", button)) TrySpawnAdaptiveThreat();
            GUI.Label(new Rect(r.x + 12, r.y + 96, r.width - 24, 42), LastMessage);
            float y = r.y + 144;
            int shown = 0;
            for (int i = 0; i < gm.NonPlayerCells.Count && shown < 6; i++)
            {
                V5CellEntity e = gm.NonPlayerCells[i];
                if (e == null) continue;
                GUI.Label(new Rect(r.x + 20, y, r.width - 40, 20), e.EvolutionPath + " — " + GetArchetype(e) + " HP " + e.Stats.currentHp.ToString("0"));
                y += 20;
                shown++;
            }
        }

        private void EnsureStyles()
        {
            if (box != null) return;
            box = new GUIStyle(GUI.skin.box);
            box.normal.background = Texture2D.whiteTexture;
            box.normal.textColor = Color.white;
            title = new GUIStyle(GUI.skin.label);
            title.fontStyle = FontStyle.Bold;
            title.normal.textColor = Color.white;
            button = new GUIStyle(GUI.skin.button);
        }
    }
}
