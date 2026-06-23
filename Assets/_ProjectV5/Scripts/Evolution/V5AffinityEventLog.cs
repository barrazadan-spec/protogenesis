using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.V5
{
    public struct V5AffinityEvent
    {
        public V5EvolutionPath path;
        public float points;
        public string reason;
        public string source;
        public float time;

        public V5AffinityEvent(V5EvolutionPath path, float points, string reason, string source, float time)
        {
            this.path = path;
            this.points = points;
            this.reason = reason;
            this.source = source;
            this.time = time;
        }
    }

    public class V5AffinityEventLog : MonoBehaviour, IV5RunResettable
    {
        public string LastMessage = "Historial de afinidad listo.";
        public int Count { get { return events.Count; } }

        private readonly List<V5AffinityEvent> events = new List<V5AffinityEvent>(96);
        private readonly Dictionary<string, float> throttle = new Dictionary<string, float>(64);

        public void ResetForNewRun()
        {
            events.Clear();
            throttle.Clear();
            LastMessage = "Historial de afinidad listo.";
        }

        public void AddEvent(V5EvolutionPath path, float points, string reason, string source)
        {
            if (!V5RosterBalance.IsPlayablePath(path) || points <= 0f) return;
            V5GameManager gm = V5GameManager.Instance;
            float now = gm != null ? gm.ElapsedSeconds : Time.time;
            string key = path + "|" + source + "|" + reason;
            float last;
            if (throttle.TryGetValue(key, out last) && now - last < V5Balance.AffinityEventMinInterval) return;
            throttle[key] = now;

            V5AffinityEvent e = new V5AffinityEvent(path, Mathf.Clamp(points, 0.5f, 18f), reason, source, now);
            events.Add(e);
            while (events.Count > V5Balance.AffinityEventCapacity) events.RemoveAt(0);
            LastMessage = "+" + e.points.ToString("0") + " " + e.path + ": " + e.reason;
        }

        public float ScoreBonus(V5EvolutionPath path)
        {
            V5GameManager gm = V5GameManager.Instance;
            float now = gm != null ? gm.ElapsedSeconds : Time.time;
            float total = 0f;
            for (int i = 0; i < events.Count; i++)
            {
                V5AffinityEvent e = events[i];
                if (e.path != path) continue;
                float age = Mathf.Max(0f, now - e.time);
                if (age > V5Balance.AffinityEventMemorySeconds) continue;
                float retention = Mathf.Lerp(1f, V5Balance.AffinityEventMinimumRetention, age / V5Balance.AffinityEventMemorySeconds);
                total += e.points * retention;
            }
            return Mathf.Min(total, V5Balance.AffinityEventScoreCap);
        }

        public string RouteSummary(V5EvolutionPath path, int maxItems)
        {
            int found = 0;
            string text = "";
            for (int i = events.Count - 1; i >= 0 && found < maxItems; i--)
            {
                V5AffinityEvent e = events[i];
                if (e.path != path) continue;
                if (found > 0) text += " | ";
                text += "+" + e.points.ToString("0") + " " + e.reason;
                found++;
            }
            return found > 0 ? text : "sin eventos recientes";
        }

        public string RecentSummary(int maxItems)
        {
            int found = 0;
            string text = "";
            for (int i = events.Count - 1; i >= 0 && found < maxItems; i--)
            {
                V5AffinityEvent e = events[i];
                if (found > 0) text += " | ";
                text += e.path + " +" + e.points.ToString("0") + " " + e.reason;
                found++;
            }
            return found > 0 ? text : "sin eventos de afinidad todavia";
        }

        public void RecordStructure(V5CellEntity cell, V5StructureId id)
        {
            if (cell == null || !cell.IsPlayerOwned) return;
            string name = V5EvolutionLibrary.GetStructure(id).displayName;
            switch (id)
            {
                case V5StructureId.BacterialFlagellum: AddEvent(V5EvolutionPath.Bacteria, 8f, name, "structure"); break;
                case V5StructureId.Plasmid: AddEvent(V5EvolutionPath.Bacteria, 10f, name, "structure"); break;
                case V5StructureId.Fimbriae:
                    AddEvent(V5EvolutionPath.Bacteria, 10f, name, "structure");
                    AddEvent(V5EvolutionPath.Fungus, 4f, "adhesion/biofilm", "structure");
                    break;
                case V5StructureId.Capsule:
                case V5StructureId.PeptidoglycanWall:
                    AddEvent(V5EvolutionPath.Archaea, 8f, name, "structure");
                    AddEvent(V5EvolutionPath.Cyanobacteria, 4f, name, "structure");
                    break;
                case V5StructureId.Thylakoid:
                    AddEvent(V5EvolutionPath.Cyanobacteria, 14f, name, "structure");
                    AddEvent(V5EvolutionPath.Microalga, 6f, name, "structure");
                    break;
                case V5StructureId.Lysosome:
                    AddEvent(V5EvolutionPath.Amoeba, 14f, name, "structure");
                    AddEvent(V5EvolutionPath.Rotifer, 6f, name, "structure");
                    break;
                case V5StructureId.StorageVacuole:
                    AddEvent(V5EvolutionPath.Amoeba, 5f, name, "structure");
                    AddEvent(V5EvolutionPath.Ciliate, 5f, name, "structure");
                    AddEvent(V5EvolutionPath.SlimeMold, 4f, name, "structure");
                    break;
                case V5StructureId.EukaryoticFlagellum:
                    AddEvent(V5EvolutionPath.Flagellate, 14f, name, "structure");
                    AddEvent(V5EvolutionPath.Nematode, 5f, name, "structure");
                    break;
                case V5StructureId.Cilia:
                    AddEvent(V5EvolutionPath.Ciliate, 14f, name, "structure");
                    break;
                case V5StructureId.MicroalgalChloroplast:
                    AddEvent(V5EvolutionPath.Microalga, 16f, name, "structure");
                    break;
                case V5StructureId.InvasiveHypha:
                    AddEvent(V5EvolutionPath.Fungus, 14f, name, "structure");
                    AddEvent(V5EvolutionPath.SlimeMold, 5f, name, "structure");
                    break;
                case V5StructureId.MucilageMatrix:
                    AddEvent(V5EvolutionPath.SlimeMold, 16f, name, "structure");
                    break;
                case V5StructureId.CoronaCilia:
                    AddEvent(V5EvolutionPath.Rotifer, 14f, name, "structure");
                    AddEvent(V5EvolutionPath.Ciliate, 5f, name, "structure");
                    break;
                case V5StructureId.PiercingStylet:
                    AddEvent(V5EvolutionPath.Nematode, 16f, name, "structure");
                    break;
                case V5StructureId.Cuticle:
                    AddEvent(V5EvolutionPath.Nematode, 7f, name, "structure");
                    AddEvent(V5EvolutionPath.Rotifer, 7f, name, "structure");
                    break;
                case V5StructureId.Catalase:
                    AddEvent(V5EvolutionPath.Archaea, 6f, name, "structure");
                    AddEvent(V5EvolutionPath.Cyanobacteria, 5f, name, "structure");
                    break;
            }
        }

        public void RecordMetabolism(V5CellEntity cell, V5MetabolismType type)
        {
            if (cell == null || !cell.IsPlayerOwned) return;
            if (type == V5MetabolismType.Fermentation) AddEvent(V5EvolutionPath.Bacteria, 12f, "fermentacion activa", "metabolism");
            else if (type == V5MetabolismType.Chemolithotrophy) AddEvent(V5EvolutionPath.Archaea, 14f, "quimiolitotrofia", "metabolism");
            else if (type == V5MetabolismType.Photosynthesis)
            {
                AddEvent(V5EvolutionPath.Cyanobacteria, 12f, "fotosintesis", "metabolism");
                AddEvent(V5EvolutionPath.Microalga, 7f, "fotosintesis", "metabolism");
            }
            else if (type == V5MetabolismType.Respiration)
            {
                AddEvent(V5EvolutionPath.Amoeba, 5f, "respiracion eucariota", "metabolism");
                AddEvent(V5EvolutionPath.Flagellate, 5f, "respiracion eucariota", "metabolism");
                AddEvent(V5EvolutionPath.Ciliate, 5f, "respiracion eucariota", "metabolism");
            }
        }

        public void RecordGene(V5GeneId gene)
        {
            if (gene == V5GeneId.Fermentation) AddEvent(V5EvolutionPath.Bacteria, 10f, "gen Fermentation", "gene");
            else if (gene == V5GeneId.Chemolithotrophy) AddEvent(V5EvolutionPath.Archaea, 12f, "gen Chemolithotrophy", "gene");
            else if (gene == V5GeneId.Photosynthesis)
            {
                AddEvent(V5EvolutionPath.Cyanobacteria, 10f, "gen Photosynthesis", "gene");
                AddEvent(V5EvolutionPath.Microalga, 8f, "gen Photosynthesis", "gene");
            }
            else if (gene == V5GeneId.Motility)
            {
                AddEvent(V5EvolutionPath.Flagellate, 10f, "gen Motility", "gene");
                AddEvent(V5EvolutionPath.Nematode, 5f, "gen Motility", "gene");
            }
            else if (gene == V5GeneId.Secretion)
            {
                AddEvent(V5EvolutionPath.Fungus, 8f, "gen Secretion", "gene");
                AddEvent(V5EvolutionPath.Bacteria, 6f, "gen Secretion", "gene");
            }
            else if (gene == V5GeneId.Recognition)
            {
                AddEvent(V5EvolutionPath.Amoeba, 7f, "gen Recognition", "gene");
                AddEvent(V5EvolutionPath.Rotifer, 6f, "gen Recognition", "gene");
            }
            else if (gene == V5GeneId.Adhesion)
            {
                AddEvent(V5EvolutionPath.Fungus, 9f, "gen Adhesion", "gene");
                AddEvent(V5EvolutionPath.SlimeMold, 9f, "gen Adhesion", "gene");
                AddEvent(V5EvolutionPath.Bacteria, 6f, "gen Adhesion", "gene");
            }
            else if (gene == V5GeneId.RapidDivision) AddEvent(V5EvolutionPath.Bacteria, 9f, "division rapida", "gene");
            else if (gene == V5GeneId.Symbiosis) AddEvent(V5EvolutionPath.Microalga, 6f, "simbiosis", "gene");
        }

        public void RecordGerminal(V5EvolutionPath targetPath, string displayName)
        {
            AddEvent(targetPath, 8f, "producida " + displayName, "germinal");
        }

        public void RecordCellMode(V5CellEntity cell, V5CellModeId mode)
        {
            if (cell == null || !cell.IsPlayerOwned) return;
            V5EvolutionPath primary = cell.EvolutionPath;
            if (!V5RosterBalance.IsPlayablePath(primary))
            {
                V5EvolutionAffinityResult best = V5EvolutionAffinitySystem.BestRoute(cell);
                primary = best.path;
            }
            if (V5RosterBalance.IsPlayablePath(primary)) AddEvent(primary, 1.2f, "modo " + V5CellModeLibrary.Get(mode).displayName, "mode");

            if (mode == V5CellModeId.Gather)
            {
                AddEvent(V5EvolutionPath.Microalga, 0.8f, "conducta recolectora", "mode");
                AddEvent(V5EvolutionPath.SlimeMold, 0.8f, "rastreo de recursos", "mode");
            }
            else if (mode == V5CellModeId.Defend)
            {
                AddEvent(V5EvolutionPath.Archaea, 0.8f, "homeostasis defensiva", "mode");
                AddEvent(V5EvolutionPath.Fungus, 0.8f, "anclaje territorial", "mode");
            }
            else if (mode == V5CellModeId.Scout)
            {
                AddEvent(V5EvolutionPath.Flagellate, 1.3f, "exploracion movil", "mode");
                AddEvent(V5EvolutionPath.Nematode, 0.7f, "lectura de corredores", "mode");
            }
            else if (mode == V5CellModeId.Colonize)
            {
                AddEvent(V5EvolutionPath.Bacteria, 1.1f, "presion colonial", "mode");
                AddEvent(V5EvolutionPath.Fungus, 0.9f, "red territorial", "mode");
                AddEvent(V5EvolutionPath.SlimeMold, 0.9f, "matriz movil", "mode");
            }
            else if (mode == V5CellModeId.Hunt)
            {
                AddEvent(V5EvolutionPath.Amoeba, 1.1f, "conducta predatoria", "mode");
                AddEvent(V5EvolutionPath.Flagellate, 1.0f, "raideo movil", "mode");
                AddEvent(V5EvolutionPath.Nematode, 0.9f, "persecucion lineal", "mode");
            }
            else if (mode == V5CellModeId.Recover)
            {
                AddEvent(V5EvolutionPath.Archaea, 1.0f, "recuperacion extrema", "mode");
            }
        }

        public void RecordColonization(V5CellEntity cell)
        {
            if (cell == null || !cell.IsPlayerOwned) return;
            V5EvolutionPath path = cell.EvolutionPath;
            if (path == V5EvolutionPath.Uncommitted)
            {
                if (cell.HasBiofilm) path = V5EvolutionPath.Bacteria;
                else if (cell.HasMucilage) path = V5EvolutionPath.SlimeMold;
                else if (cell.HasStructure(V5StructureId.InvasiveHypha)) path = V5EvolutionPath.Fungus;
                else if (cell.HasPhotosynthesis) path = V5EvolutionPath.Cyanobacteria;
            }
            if (V5RosterBalance.IsPlayablePath(path)) AddEvent(path, 2.5f, "colonizacion sostenida", "colonize");
            if (cell.HasBiofilm) AddEvent(V5EvolutionPath.Bacteria, 2f, "biofilm activo", "colonize");
            if (cell.HasMucilage) AddEvent(V5EvolutionPath.SlimeMold, 2f, "mucilago activo", "colonize");
            if (cell.HasStructure(V5StructureId.InvasiveHypha)) AddEvent(V5EvolutionPath.Fungus, 2f, "hifas activas", "colonize");
        }

        public void RecordEnvironment(V5CellEntity cell, float light, float oxygen, float toxins, float acidity, float detritus)
        {
            if (cell == null || !cell.IsPlayerOwned) return;
            if (light > 0.58f)
            {
                AddEvent(V5EvolutionPath.Cyanobacteria, 2.2f, "vida en luz alta", "environment");
                AddEvent(V5EvolutionPath.Microalga, 2.2f, "vida en luz alta", "environment");
            }
            if (toxins > 0.35f || acidity > 0.66f) AddEvent(V5EvolutionPath.Archaea, 2.8f, "vida en zona extrema", "environment");
            if (detritus > 0.38f)
            {
                AddEvent(V5EvolutionPath.Fungus, 2.2f, "detritus recurrente", "environment");
                AddEvent(V5EvolutionPath.SlimeMold, 2.2f, "detritus recurrente", "environment");
            }
            if (oxygen > 0.48f && cell.Domain == V5CellDomain.Eukaryote)
            {
                AddEvent(V5EvolutionPath.Amoeba, 1.8f, "oxigeno eucariota", "environment");
                AddEvent(V5EvolutionPath.Flagellate, 1.8f, "oxigeno eucariota", "environment");
            }
        }

        public void RecordCombat(V5CellEntity attacker, string label, bool killed)
        {
            if (attacker == null || !attacker.IsPlayerOwned) return;
            float killBonus = killed ? 4f : 0f;
            if (label == "digest")
            {
                AddEvent(V5EvolutionPath.Amoeba, 3f + killBonus, killed ? "presa fagocitada" : "fagocitosis en combate", "combat");
                AddEvent(V5EvolutionPath.Rotifer, 1.5f, "filtracion/fagocitosis", "combat");
            }
            else if (label == "pierce") AddEvent(V5EvolutionPath.Nematode, 3f + killBonus, killed ? "presa perforada" : "perforacion", "combat");
            else if (label == "filter") AddEvent(V5EvolutionPath.Rotifer, 3f + killBonus, killed ? "swarm filtrado" : "filtracion", "combat");
            else if (label == "net")
            {
                AddEvent(V5EvolutionPath.Fungus, 2f + killBonus * 0.5f, "red digestiva", "combat");
                AddEvent(V5EvolutionPath.SlimeMold, 2f + killBonus * 0.5f, "red de mucilago", "combat");
            }
            else if (label == "tox") AddEvent(V5EvolutionPath.Bacteria, 2f + killBonus * 0.5f, "toxinacion", "combat");
            else if (attacker.Stats.speed > 2.5f) AddEvent(V5EvolutionPath.Flagellate, 1.5f + killBonus * 0.5f, "hostigamiento movil", "combat");
        }
    }
}
