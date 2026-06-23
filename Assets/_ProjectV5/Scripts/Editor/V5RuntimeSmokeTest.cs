#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Protogenesis.V5.EditorTools
{
    public static class V5RuntimeSmokeTest
    {
        private const string ScenePath = "Assets/_ProjectV5/Scenes/V5_FullPrototype.unity";
        private static readonly List<string> runtimeErrors = new List<string>();
        private static double startedAt;
        private static double playStartedAt;
        private static bool sawPlayMode;
        private static bool finished;

        public static void RunFullPrototype()
        {
            finished = false;
            sawPlayMode = false;
            playStartedAt = 0;
            runtimeErrors.Clear();
            startedAt = EditorApplication.timeSinceStartup;

            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Application.logMessageReceived += OnLogMessage;
            EditorApplication.update += Tick;
            EditorApplication.isPlaying = true;
        }

        private static void Tick()
        {
            if (finished) return;

            if (EditorApplication.timeSinceStartup - startedAt > 35.0)
            {
                Finish(false, "Timeout entrando o validando Play Mode.");
                return;
            }

            if (!EditorApplication.isPlaying) return;

            if (!sawPlayMode)
            {
                sawPlayMode = true;
                playStartedAt = EditorApplication.timeSinceStartup;
                return;
            }

            if (EditorApplication.timeSinceStartup - playStartedAt < 4.0) return;

            ValidateRuntime();
        }

        private static void ValidateRuntime()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null) gm = Object.FindFirstObjectByType<V5GameManager>();

            bool ok = true;
            ok &= Check(gm != null, "V5GameManager existe.");
            if (gm == null)
            {
                Finish(false, "No se creo V5GameManager.");
                return;
            }

            ok &= Check(gm.Environment != null && gm.Environment.nutrients != null && gm.Environment.lightLevel != null, "EnvironmentGrid inicializado.");
            ok &= Check(gm.Resources != null && gm.Resources.Nodes.Count > 0, "ResourceSystem genero nodos.");
            ok &= Check(gm.CellFactory != null, "CellFactory conectado.");
            ok &= Check(gm.MotherCell != null && gm.MotherCell.IsPlayerOwned, "Celula madre creada.");
            ok &= Check(gm.PlayerCellCount() >= 1, "Registro de celulas jugador activo.");
            ok &= Check(gm.NonPlayerCells.Count > 0, "Amenazas/neutrales iniciales creados.");
            ok &= Check(gm.Selection != null && gm.Combat != null, "RTS selection/combat activos.");
            ok &= Check(gm.Squads != null, "Squad tactics activo.");
            ok &= Check(gm.Body != null && gm.Body.MaxSlots >= 6, "Cuerpo multicelular activo.");
            ok &= Check(gm.PlayableLoop != null, "Playable loop activo.");
            ok &= Check(gm.Biomes != null && gm.ThreatEcology != null, "Living battlefield systems activos.");
            ok &= Check(gm.Hud != null && gm.CameraController != null, "HUD y camara activos.");
            ok &= Check(V5EvolutionRoster.PrimaryRoutes.Length == 12 && !V5EvolutionRoster.IsPrimaryRoute(V5EvolutionPath.StemCell), "Roster principal sin rutas immune/legacy.");

            if (runtimeErrors.Count > 0)
            {
                ok = false;
                for (int i = 0; i < runtimeErrors.Count; i++) Debug.LogError("[V5Smoke] Runtime error capturado: " + runtimeErrors[i]);
            }

            Finish(ok, ok ? "Smoke test V5_FullPrototype OK." : "Smoke test V5_FullPrototype fallo.");
        }

        private static bool Check(bool condition, string message)
        {
            if (condition)
            {
                Debug.Log("[V5Smoke] OK: " + message);
                return true;
            }
            Debug.LogError("[V5Smoke] FAIL: " + message);
            return false;
        }

        private static void OnLogMessage(string condition, string stackTrace, LogType type)
        {
            if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert) return;
            if (condition.Contains("[V5Smoke]")) return;
            runtimeErrors.Add(condition);
        }

        private static void Finish(bool ok, string message)
        {
            if (finished) return;
            finished = true;
            Debug.Log("[V5Smoke] " + message);
            Application.logMessageReceived -= OnLogMessage;
            EditorApplication.update -= Tick;
            EditorApplication.isPlaying = false;
            EditorApplication.Exit(ok ? 0 : 1);
        }
    }
}
#endif
