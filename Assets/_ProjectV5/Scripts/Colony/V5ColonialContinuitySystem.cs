using UnityEngine;

namespace Protogenesis.V5
{
    public class V5ColonialContinuitySystem : MonoBehaviour, IV5RunResettable
    {
        public bool CrisisActive;
        public float CrisisTimeRemaining;
        public float LastScore = 100f;
        public string LastBand = "Estable";
        public string LastBreakdown = "Linaje intacto.";
        public V5CellEntity CurrentSuccessor;

        private float tickTimer;
        private float cryptobiosisPauseRemaining;

        public string StatusText
        {
            get
            {
                if (CrisisActive)
                {
                    string pause = cryptobiosisPauseRemaining > 0f ? " pausa cripto " + cryptobiosisPauseRemaining.ToString("0") + "s" : "";
                    return LastBand + " " + LastScore.ToString("0") + " / " + CrisisTimeRemaining.ToString("0") + "s" + pause;
                }
                return LastBand + " " + LastScore.ToString("0");
            }
        }

        public void ResetForNewRun()
        {
            CrisisActive = false;
            CrisisTimeRemaining = 0f;
            LastScore = 100f;
            LastBand = "Estable";
            LastBreakdown = "Linaje intacto.";
            CurrentSuccessor = null;
            cryptobiosisPauseRemaining = 0f;
            tickTimer = 0f;
        }

        private void Update()
        {
            if (!CrisisActive) return;
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.Phase == V5GamePhase.Victory || gm.Phase == V5GamePhase.Defeat) return;

            tickTimer += Time.deltaTime;
            if (tickTimer < 0.5f) return;
            tickTimer = 0f;
            TickContinuityCrisis(gm, 0.5f);
        }

        public bool TryHandleMotherDeath(V5CellEntity dyingMother)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || dyingMother == null) return false;

            ContinuityEvaluation eval = Evaluate(gm, dyingMother, null);
            LastScore = eval.score;
            LastBand = BandForScore(eval.score);
            LastBreakdown = eval.breakdown;
            CurrentSuccessor = eval.successor;

            if (eval.successor == null || eval.score < 20f)
            {
                LastBreakdown = "Colapso: sin sucesor viable. " + eval.breakdown;
                return false;
            }

            PromoteSuccessor(gm, dyingMother, eval.successor);
            if (eval.score >= 70f)
            {
                CrisisActive = false;
                CrisisTimeRemaining = 0f;
                LastBand = "Sucesion rapida";
            }
            else
            {
                CrisisActive = true;
                CrisisTimeRemaining = eval.score >= 40f ? 75f : 90f;
                cryptobiosisPauseRemaining = eval.successor.HasCryptobiosis ? 20f : 0f;
            }

            Toast("Continuidad colonial: " + LastBand + " (" + LastScore.ToString("0") + ")");
            return true;
        }

        private void TickContinuityCrisis(V5GameManager gm, float dt)
        {
            if (CurrentSuccessor == null || gm.MotherCell != CurrentSuccessor)
            {
                gm.Lose("colapso de continuidad colonial");
                return;
            }

            if (cryptobiosisPauseRemaining > 0f)
            {
                cryptobiosisPauseRemaining = Mathf.Max(0f, cryptobiosisPauseRemaining - dt);
            }
            else
            {
                CrisisTimeRemaining -= dt;
            }

            ContinuityEvaluation eval = Evaluate(gm, null, CurrentSuccessor);
            LastScore = eval.score;
            LastBand = BandForScore(eval.score);
            LastBreakdown = eval.breakdown;

            if (eval.score >= 70f)
            {
                EndCrisis("Continuidad estabilizada.");
                return;
            }

            if (eval.score < 20f)
            {
                gm.Lose("linaje colapso durante la crisis de continuidad");
                return;
            }

            if (CrisisTimeRemaining <= 0f)
            {
                if (eval.score >= 40f) EndCrisis("Crisis de continuidad superada.");
                else gm.Lose("sucesion fragmentaria no logro estabilizarse");
            }
        }

        private void EndCrisis(string message)
        {
            CrisisActive = false;
            CrisisTimeRemaining = 0f;
            cryptobiosisPauseRemaining = 0f;
            LastBand = "Estable";
            Toast(message);
        }

        private ContinuityEvaluation Evaluate(V5GameManager gm, V5CellEntity dyingMother, V5CellEntity preferred)
        {
            ContinuityEvaluation eval = new ContinuityEvaluation();
            eval.successor = preferred != null ? preferred : PickBestSuccessor(gm, dyingMother);
            if (eval.successor == null)
            {
                eval.score = 0f;
                eval.breakdown = "sin hija, nieta o unidad viable";
                return eval;
            }

            float score = 0f;
            string breakdown = "";
            V5CellEntity s = eval.successor;
            Vector2 anchor = s.transform.position;

            float successorScore = SuccessorBaseScore(s);
            score += successorScore;
            breakdown += "sucesor +" + successorScore.ToString("0") + "; ";

            int nearbyAllies = CountNearby(gm.PlayerCells, anchor, 8f, dyingMother);
            if (nearbyAllies >= 5)
            {
                score += 15f;
                breakdown += "cohesion +15; ";
            }
            else if (nearbyAllies >= 2)
            {
                score += 5f;
                breakdown += "cohesion +5; ";
            }
            else
            {
                score -= 10f;
                breakdown += "aislamiento -10; ";
            }

            if (HasAnyStructure(gm, V5StructureId.Fimbriae, V5StructureId.MucilageMatrix))
            {
                score += 15f;
                breakdown += "biofilm +15; ";
            }
            if (HasAnyAdaptation(gm, V5AdaptationId.BasicAdhesin, V5AdaptationId.PiliFimbriae, V5AdaptationId.ColonialAdhesin, V5AdaptationId.PersistentAdhesion))
            {
                score += 15f;
                breakdown += "adhesion adaptativa +15; ";
            }
            if (HasNetwork(gm, V5EvolutionPath.Fungus, V5StructureId.InvasiveHypha))
            {
                score += 20f;
                breakdown += "red hifal +20; ";
            }
            if (HasNetwork(gm, V5EvolutionPath.SlimeMold, V5StructureId.MucilageMatrix))
            {
                score += 20f;
                breakdown += "red movil +20; ";
            }
            if (HasAnyAdaptation(gm, V5AdaptationId.FungalHypha, V5AdaptationId.ExtracellularEnzymes))
            {
                score += 20f;
                breakdown += "red saprofita +20; ";
            }
            if (HasAnyAdaptation(gm, V5AdaptationId.SlimePlasmodium, V5AdaptationId.ChemicalMemory))
            {
                score += 20f;
                breakdown += "memoria mucilaginosa +20; ";
            }
            if (s.HasStructure(V5StructureId.Capsule) || s.HasStructure(V5StructureId.Cuticle))
            {
                score += 15f;
                breakdown += "quiste/cuticula +15; ";
            }
            if (HasAnyAdaptation(gm, V5AdaptationId.PolysaccharideCapsule, V5AdaptationId.ExtremophileMembrane, V5AdaptationId.CelluloseWall, V5AdaptationId.SilicaFrustule))
            {
                score += 15f;
                breakdown += "cubierta adaptativa +15; ";
            }
            if (s.HasCryptobiosis)
            {
                score += 10f;
                breakdown += "criptobiosis +10; ";
            }
            if (HasAnyAdaptation(gm, V5AdaptationId.SignalingCommunication, V5AdaptationId.CellDifferentiation))
            {
                score += 12f;
                breakdown += "senales/diferenciacion +12; ";
            }
            if (HasAnyAdaptation(gm, V5AdaptationId.BiologicalChampion))
            {
                score += 10f;
                breakdown += "campeon biologico +10; ";
            }

            float bankAtp = s.Resources.atp + (dyingMother != null ? dyingMother.Resources.atp : 0f);
            float bankBiomass = s.Resources.biomass + (dyingMother != null ? dyingMother.Resources.biomass : 0f);
            if (bankAtp >= 40f && bankBiomass >= 40f)
            {
                score += 10f;
                breakdown += "banco +10; ";
            }

            if (gm != null && gm.Body != null)
            {
                V5BodyState bodyState = gm.Body.CurrentState;
                if (bodyState == V5BodyState.Complete || bodyState == V5BodyState.Overloaded)
                { score += 15f; breakdown += "cuerpo estable +15; "; }
                else if (bodyState == V5BodyState.Partial)
                { score += 5f; breakdown += "cuerpo parcial +5; "; }
                else
                { score -= 10f; breakdown += "cuerpo perdido -10; "; }

                if (eval.successor != null && dyingMother != null)
                {
                    float bodyDist = Vector2.Distance(eval.successor.transform.position, dyingMother.transform.position);
                    if (bodyDist <= 5f && bodyState != V5BodyState.Exposed)
                    { score += 10f; breakdown += "sucesor protegido +10; "; }
                }
            }

            int nearbyThreats = CountNearby(gm.NonPlayerCells, anchor, 5f, null);
            if (nearbyThreats >= 2)
            {
                score -= 25f;
                breakdown += "amenazas -25; ";
            }
            else if (nearbyThreats == 1)
            {
                score -= 15f;
                breakdown += "amenaza -15; ";
            }

            if (s.Stats.stress > 90f)
            {
                score -= 30f;
                breakdown += "stress critico -30; ";
            }
            else if (s.Stats.stress > 70f)
            {
                score -= 15f;
                breakdown += "stress alto -15; ";
            }

            eval.score = Mathf.Clamp(score, 0f, 100f);
            eval.breakdown = breakdown;
            return eval;
        }

        private V5CellEntity PickBestSuccessor(V5GameManager gm, V5CellEntity dyingMother)
        {
            V5CellEntity best = null;
            float bestScore = -999f;
            for (int i = 0; i < gm.PlayerCells.Count; i++)
            {
                V5CellEntity c = gm.PlayerCells[i];
                if (c == null || c == dyingMother || c.Role == V5CellRole.Apex) continue;
                if (c.Stats.currentHp <= 0f) continue;
                float score = SuccessorBaseScore(c) + Mathf.Clamp01(c.Stats.currentHp / Mathf.Max(1f, c.Stats.maxHp)) * 10f - c.Stats.stress * 0.12f + c.Structures.Count * 1.5f;
                if (score > bestScore)
                {
                    bestScore = score;
                    best = c;
                }
            }
            return best;
        }

        private float SuccessorBaseScore(V5CellEntity cell)
        {
            if (cell == null) return 0f;
            if (cell.Role == V5CellRole.Daughter)
            {
                if (cell.LastInheritedStructureCount >= 2 || cell.Structures.Count >= 3 || cell.HasStructure(V5StructureId.StemPlasticity)) return 30f;
                return 25f;
            }
            if (cell.Role == V5CellRole.Granddaughter) return cell.Structures.Count >= 2 ? 15f : 10f;
            if (cell.EvolutionPath == V5EvolutionPath.Amoeba && cell.Stats.currentHp > cell.Stats.maxHp * 0.45f && cell.Stats.stress < 65f) return 20f;
            return 10f;
        }

        private void PromoteSuccessor(V5GameManager gm, V5CellEntity dyingMother, V5CellEntity successor)
        {
            successor.Role = V5CellRole.Mother;
            successor.Generation = 0;
            successor.Mother = successor;
            successor.Directive = V5Directive.Defend;
            successor.LineageRole = V5LineageRole.Generalist;
            successor.Stats.currentHp = Mathf.Max(successor.Stats.currentHp, successor.Stats.maxHp * 0.45f);
            gm.MotherCell = successor;

            for (int i = 0; i < gm.PlayerCells.Count; i++)
            {
                V5CellEntity c = gm.PlayerCells[i];
                if (c == null || c == dyingMother || c == successor) continue;
                if (c.Mother == dyingMother || c.Mother == null) c.Mother = successor;
                if (c.Directive == V5Directive.FollowMother || c.Directive == V5Directive.Defend) c.DirectiveTarget = successor.transform.position;
            }

            if (gm.Selection != null)
            {
                gm.Selection.ClearSelection();
                gm.Selection.AddSelection(successor);
            }
            if (gm.CameraController != null && (gm.CameraController.FollowTarget == null || gm.CameraController.FollowTarget == dyingMother.transform))
                gm.CameraController.FollowTarget = successor.transform;
            if (gm.Codex != null) gm.Codex.Unlock("Sucesion colonial", "La colonia sobrevivio a la muerte de la madre mediante un sucesor viable.");
        }

        private bool HasAnyStructure(V5GameManager gm, V5StructureId a, V5StructureId b)
        {
            for (int i = 0; i < gm.PlayerCells.Count; i++)
            {
                V5CellEntity c = gm.PlayerCells[i];
                if (c != null && (c.HasStructure(a) || c.HasStructure(b))) return true;
            }
            return false;
        }

        private bool HasNetwork(V5GameManager gm, V5EvolutionPath path, V5StructureId structure)
        {
            for (int i = 0; i < gm.PlayerCells.Count; i++)
            {
                V5CellEntity c = gm.PlayerCells[i];
                if (c != null && c.EvolutionPath == path && c.HasStructure(structure)) return true;
            }
            return false;
        }

        private bool HasAnyAdaptation(V5GameManager gm, params V5AdaptationId[] ids)
        {
            if (gm == null || gm.Adaptations == null || ids == null) return false;
            for (int i = 0; i < ids.Length; i++)
                if (gm.Adaptations.Has(ids[i])) return true;
            return false;
        }

        private int CountNearby(System.Collections.Generic.IReadOnlyList<V5CellEntity> cells, Vector2 center, float radius, V5CellEntity skip)
        {
            int count = 0;
            float r2 = radius * radius;
            for (int i = 0; i < cells.Count; i++)
            {
                V5CellEntity c = cells[i];
                if (c == null || c == skip || c.Stats.currentHp <= 0f) continue;
                if (((Vector2)c.transform.position - center).sqrMagnitude <= r2) count++;
            }
            return count;
        }

        private string BandForScore(float score)
        {
            if (score >= 70f) return "Sucesion rapida";
            if (score >= 40f) return "Crisis recuperable";
            if (score >= 20f) return "Supervivencia fragmentaria";
            return "Colapso";
        }

        private void Toast(string message)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm != null && gm.Hud != null) gm.Hud.Toast(message);
        }

        private struct ContinuityEvaluation
        {
            public V5CellEntity successor;
            public float score;
            public string breakdown;
        }
    }
}
