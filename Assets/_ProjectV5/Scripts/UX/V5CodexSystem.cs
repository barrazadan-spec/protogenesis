using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.V5
{
    public class V5CodexSystem : MonoBehaviour, IV5RunResettable
    {
        public readonly List<string> Entries = new List<string>();
        public readonly List<string> RecentUnlocks = new List<string>();

        public void BeginRun()
        {
            Unlock("LUCA", "Toda run comienza como una protocelula neutra antes de elegir metabolismo, dominio e identidad.");
            Unlock("Division celular", "La produccion de unidades ocurre cuando una celula se divide y reparte recursos, estructuras y memoria del linaje.");
            Unlock("Genoma adaptativo", "El progreso principal ocurre instalando adaptaciones: cada una deja huella en identidad, cuerpo, produccion y amenazas.");
        }

        void IV5RunResettable.ResetForNewRun() => BeginRun();

        public void ObserveCell(V5CellEntity cell)
        {
            if (cell == null) return;
            if (cell.EvolutionPath != V5EvolutionPath.Uncommitted)
                Unlock(cell.EvolutionPath.ToString(), "Ruta observada: " + cell.EvolutionPath + ". Su rol emerge desde adaptaciones, identidad, estructuras heredadas y conducta.");
            if (cell.Domain == V5CellDomain.Prokaryote)
                Unlock("Dominio procariota", "Rapido, barato, territorial y orientado a biofilm, toxinas o ambientes extremos.");
            if (cell.Domain == V5CellDomain.Eukaryote)
                Unlock("Dominio eucariota", "Mas caro y lento, pero con organelos, depredacion y rutas complejas.");

            V5GameManager gm = V5GameManager.Instance;
            if (gm != null && gm.Identity != null && gm.Identity.Identity != V5IdentityId.LUCA)
                ObserveIdentity(gm.Identity);
        }

        public void ObserveStructure(V5StructureId id)
        {
            V5StructureDefinition def = V5EvolutionLibrary.GetStructure(id);
            Unlock(def.displayName, def.description);
        }

        public void ObserveAdaptation(V5AdaptationDefinition def)
        {
            if (def == null) return;
            string text = def.description;
            AddClause(ref text, def.identityEffect);
            AddClause(ref text, def.productionEffect);
            AddClause(ref text, def.bodyEffect);
            if (def.routeHint != V5EvolutionPath.Uncommitted)
                AddClause(ref text, "Ruta afin: " + def.routeHint + ". " + V5BiologyCanon.RouteDesignNote(def.routeHint));
            if (!string.IsNullOrEmpty(def.naturalCounters))
                AddClause(ref text, "Counters naturales: " + def.naturalCounters + ".");
            Unlock("Adaptacion: " + def.displayName, text);
        }

        public void ObserveIdentity(V5IdentityRecognizer identity)
        {
            if (identity == null || identity.Identity == V5IdentityId.LUCA) return;
            string text = identity.Reason;
            V5AdaptationId[] canon = V5BiologyCanon.AdaptationsForIdentity(identity.Identity);
            if (canon != null && canon.Length > 0)
                AddClause(ref text, "Canon: " + V5BiologyCanon.AdaptationListText(canon) + ".");
            if (identity.EvolutionPath != V5EvolutionPath.Uncommitted)
                AddClause(ref text, V5BiologyCanon.RouteDesignNote(identity.EvolutionPath));
            Unlock("Identidad: " + identity.DisplayName, text);
        }

        public void Unlock(string title, string text)
        {
            string entry = title + " - " + text;
            string prefix = title + " -";
            for (int i = 0; i < Entries.Count; i++)
                if (Entries[i].StartsWith(prefix)) return;

            Entries.Add(entry);
            RecentUnlocks.Add(title);
            while (RecentUnlocks.Count > 5) RecentUnlocks.RemoveAt(0);
            V5GameManager gm = V5GameManager.Instance;
            if (gm != null && gm.Hud != null) gm.Hud.Toast("Codex: " + title);
        }

        private void AddClause(ref string text, string clause)
        {
            if (string.IsNullOrEmpty(clause)) return;
            if (!string.IsNullOrEmpty(text) && !text.EndsWith(" ")) text += " ";
            text += clause;
        }
    }
}
