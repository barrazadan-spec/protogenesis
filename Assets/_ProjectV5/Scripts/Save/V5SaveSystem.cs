using System;
using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.V5
{
    [Serializable]
    public class V5SaveData
    {
        public float version;
        public float elapsed;
        public V5ScenarioId scenario;
        public V5GeneId ring1;
        public V5GeneId ring2;
        public V5GeneId ring3;
        public V5GeneId ring4;
        public List<V5AdaptationId> adaptations = new List<V5AdaptationId>();
        public List<V5SavedCell> cells = new List<V5SavedCell>();
        public V5EnvironmentSnapshot environment;
        public List<V5LineageUpgradeId> lineageUpgrades = new List<V5LineageUpgradeId>();
    }

    [Serializable]
    public class V5SavedCell
    {
        public float x, y;
        public bool player;
        public V5CellRole role;
        public V5CellDomain domain;
        public V5EvolutionPath path;
        public V5MetabolismType metabolism;
        public V5LineageRole lineageRole;
        public V5FunctionalCasteId functionalCaste;
        public V5ResourceWallet resources;
        public V5CellStats stats;
        public List<V5StructureId> structures = new List<V5StructureId>();
    }

    public class V5SaveSystem : MonoBehaviour
    {
        public string LastMessage = "";
        private const string QuickSaveKey = "ProtogenesisV5QuickSave";

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F5)) QuickSave();
            if (Input.GetKeyDown(KeyCode.F9)) QuickLoad();
        }

        public bool HasSave()
        {
            return PlayerPrefs.HasKey(QuickSaveKey);
        }

        public void QuickSave()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null) return;
            V5SaveData data = new V5SaveData();
            data.version = V5Balance.SaveVersion;
            data.elapsed = gm.ElapsedSeconds;
            data.scenario = gm.ScenarioId;
            if (gm.Genes != null)
            {
                data.ring1 = gm.Genes.Ring1;
                data.ring2 = gm.Genes.Ring2;
                data.ring3 = gm.Genes.Ring3;
                data.ring4 = gm.Genes.Ring4;
            }
            if (gm.Adaptations != null)
            {
                for (int i = 0; i < gm.Adaptations.Installed.Count; i++) data.adaptations.Add(gm.Adaptations.Installed[i]);
            }
            AddCells(data, gm.PlayerCells);
            AddCells(data, gm.NonPlayerCells);
            if (gm.Environment != null) data.environment = gm.Environment.CreateSnapshot();
            if (gm.LineageUpgrades != null) data.lineageUpgrades = gm.LineageUpgrades.GetUnlocked();
            string json = JsonUtility.ToJson(data, true);
            PlayerPrefs.SetString(QuickSaveKey, json);
            PlayerPrefs.Save();
            LastMessage = "Run guardada: " + data.cells.Count + " células, t=" + gm.ElapsedSeconds.ToString("0") + "s";
            if (gm.Hud != null) gm.Hud.Toast(LastMessage);
        }

        public bool QuickLoad()
        {
            if (!HasSave())
            {
                LastMessage = "No hay quicksave V5.";
                return false;
            }
            string json = PlayerPrefs.GetString(QuickSaveKey, "");
            if (string.IsNullOrEmpty(json))
            {
                LastMessage = "Quicksave vacío.";
                return false;
            }
            V5SaveData data = JsonUtility.FromJson<V5SaveData>(json);
            return Load(data);
        }

        public bool Load(V5SaveData data)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.CellFactory == null || data == null)
            {
                LastMessage = "No se pudo cargar la run.";
                return false;
            }

            gm.ClearCellsForLoad();
            gm.ElapsedSeconds = data.elapsed;
            gm.ScenarioId = data.scenario;
            gm.Phase = V5GamePhase.Primordial;
            gm.Paused = false;
            if (gm.Genes != null)
            {
                gm.Genes.Ring1 = data.ring1;
                gm.Genes.Ring2 = data.ring2;
                gm.Genes.Ring3 = data.ring3;
                gm.Genes.Ring4 = data.ring4;
                gm.Genes.LastMessage = "Genes restaurados desde quicksave.";
            }
            if (gm.Adaptations != null) gm.Adaptations.RestoreInstalled(data.adaptations);

            List<V5CellEntity> loaded = new List<V5CellEntity>();
            for (int i = 0; i < data.cells.Count; i++)
            {
                V5CellEntity c = gm.CellFactory.SpawnFromSave(data.cells[i]);
                if (c != null) loaded.Add(c);
            }

            for (int i = 0; i < loaded.Count; i++)
            {
                if (loaded[i] != null && loaded[i].IsPlayerOwned && loaded[i].Role != V5CellRole.Mother) loaded[i].Mother = gm.MotherCell;
            }
            if (gm.Selection != null) gm.Selection.ClearSelection();
            if (gm.MotherCell != null && gm.Selection != null) gm.Selection.AddSelection(gm.MotherCell);
            if (gm.Scenario != null) gm.Scenario.BeginScenario(data.scenario);
            if (gm.Environment != null && data.environment != null) gm.Environment.ApplySnapshot(data.environment);
            if (gm.LineageUpgrades != null) gm.LineageUpgrades.RestoreUnlocked(data.lineageUpgrades);
            if (gm.Identity != null) gm.Identity.Recalculate("load");

            LastMessage = "Run cargada: " + loaded.Count + " células.";
            if (gm.Hud != null) gm.Hud.Toast(LastMessage);
            return true;
        }

        private void AddCells(V5SaveData data, IReadOnlyList<V5CellEntity> cells)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                V5CellEntity c = cells[i];
                if (c == null) continue;
                V5SavedCell s = new V5SavedCell();
                s.x = c.transform.position.x;
                s.y = c.transform.position.y;
                s.player = c.IsPlayerOwned;
                s.role = c.Role;
                s.domain = c.Domain;
                s.path = c.EvolutionPath;
                s.metabolism = c.Metabolism;
                s.lineageRole = c.LineageRole;
                s.functionalCaste = c.FunctionalCaste;
                s.resources = c.Resources;
                s.stats = c.Stats;
                s.structures.AddRange(c.Structures);
                data.cells.Add(s);
            }
        }
    }
}
