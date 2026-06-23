using System.Collections.Generic;

namespace Protogenesis.V5
{
    public enum V5MvpRoute
    {
        None,
        Bacteria,
        Amoeba,
        PhotosyntheticProducer,
        Volvox
    }

    public static class V5MvpCanon
    {
        private static readonly V5AdaptationId[] bacteriaCore =
        {
            V5AdaptationId.BacterialWall,
            V5AdaptationId.BacterialFlagellum,
            V5AdaptationId.PiliFimbriae,
            V5AdaptationId.RapidDivision,
            V5AdaptationId.ProkaryoticThylakoid,
            V5AdaptationId.ExtremophileMembrane,
            V5AdaptationId.BasicAdhesin
        };

        private static readonly V5AdaptationId[] amoebaCore =
        {
            V5AdaptationId.Nucleus,
            V5AdaptationId.Mitochondria,
            V5AdaptationId.Lysosome,
            V5AdaptationId.Pseudopods,
            V5AdaptationId.ContractileVacuole,
            V5AdaptationId.Cilia
        };

        private static readonly V5AdaptationId[] producerCore =
        {
            V5AdaptationId.ProkaryoticThylakoid,
            V5AdaptationId.CatalaseROS,
            V5AdaptationId.Nucleus,
            V5AdaptationId.Chloroplast,
            V5AdaptationId.CelluloseWall,
            V5AdaptationId.SilicaFrustule
        };

        private static readonly V5AdaptationId[] volvoxCore =
        {
            V5AdaptationId.Nucleus,
            V5AdaptationId.Chloroplast,
            V5AdaptationId.EukaryoticFlagellum,
            V5AdaptationId.ColonialAdhesin,
            V5AdaptationId.PersistentAdhesion,
            V5AdaptationId.CellDifferentiation,
            V5AdaptationId.SignalingCommunication
        };

        private static readonly V5AdaptationId[] amoebaPrimer =
        {
            V5AdaptationId.BacterialFlagellum,
            V5AdaptationId.BacterialWall
        };

        private static readonly V5AdaptationId[] producerPrimer =
        {
            V5AdaptationId.BacterialWall
        };

        private static readonly V5AdaptationId[] volvoxPrimer =
        {
            V5AdaptationId.BasicAdhesin,
            V5AdaptationId.ProkaryoticThylakoid
        };

        private static readonly V5AdaptationId[] amoebaPlaytest =
        {
            V5AdaptationId.BacterialFlagellum,
            V5AdaptationId.BacterialWall,
            V5AdaptationId.Nucleus,
            V5AdaptationId.Mitochondria,
            V5AdaptationId.Lysosome,
            V5AdaptationId.Pseudopods,
            V5AdaptationId.ContractileVacuole,
            V5AdaptationId.Cilia
        };

        private static readonly V5AdaptationId[] producerPlaytest =
        {
            V5AdaptationId.ProkaryoticThylakoid,
            V5AdaptationId.BacterialWall,
            V5AdaptationId.CatalaseROS,
            V5AdaptationId.Nucleus,
            V5AdaptationId.Chloroplast,
            V5AdaptationId.CelluloseWall,
            V5AdaptationId.SilicaFrustule
        };

        private static readonly V5AdaptationId[] volvoxPlaytest =
        {
            V5AdaptationId.BasicAdhesin,
            V5AdaptationId.ProkaryoticThylakoid,
            V5AdaptationId.Nucleus,
            V5AdaptationId.Chloroplast,
            V5AdaptationId.EukaryoticFlagellum,
            V5AdaptationId.ColonialAdhesin,
            V5AdaptationId.PersistentAdhesion,
            V5AdaptationId.CellDifferentiation,
            V5AdaptationId.SignalingCommunication
        };

        private static readonly V5AdaptationId[] bacteriaBuildTargets =
        {
            V5AdaptationId.BacterialWall,
            V5AdaptationId.BacterialFlagellum,
            V5AdaptationId.PiliFimbriae,
            V5AdaptationId.RapidDivision
        };

        private static readonly V5AdaptationId[] amoebaBuildTargets =
        {
            V5AdaptationId.BacterialFlagellum,
            V5AdaptationId.BacterialWall,
            V5AdaptationId.Nucleus,
            V5AdaptationId.Lysosome,
            V5AdaptationId.Pseudopods
        };

        private static readonly V5AdaptationId[] producerBuildTargets =
        {
            V5AdaptationId.ProkaryoticThylakoid,
            V5AdaptationId.BacterialWall,
            V5AdaptationId.Nucleus,
            V5AdaptationId.Chloroplast,
            V5AdaptationId.CelluloseWall
        };

        private static readonly V5AdaptationId[] volvoxBuildTargets =
        {
            V5AdaptationId.BasicAdhesin,
            V5AdaptationId.ProkaryoticThylakoid,
            V5AdaptationId.Nucleus,
            V5AdaptationId.Chloroplast,
            V5AdaptationId.ColonialAdhesin,
            V5AdaptationId.CellDifferentiation
        };

        public static IReadOnlyList<V5AdaptationId> CoreAdaptations(V5MvpRoute route)
        {
            switch (route)
            {
                case V5MvpRoute.Bacteria: return bacteriaCore;
                case V5MvpRoute.Amoeba: return amoebaCore;
                case V5MvpRoute.PhotosyntheticProducer: return producerCore;
                case V5MvpRoute.Volvox: return volvoxCore;
                default: return System.Array.Empty<V5AdaptationId>();
            }
        }

        public static V5AdaptationId[] CoreAdaptationArray(V5MvpRoute route)
        {
            switch (route)
            {
                case V5MvpRoute.Bacteria: return bacteriaCore;
                case V5MvpRoute.Amoeba: return amoebaCore;
                case V5MvpRoute.PhotosyntheticProducer: return producerCore;
                case V5MvpRoute.Volvox: return volvoxCore;
                default: return System.Array.Empty<V5AdaptationId>();
            }
        }

        public static V5AdaptationId[] PrimerAdaptationArray(V5MvpRoute route)
        {
            switch (route)
            {
                case V5MvpRoute.Amoeba: return amoebaPrimer;
                case V5MvpRoute.PhotosyntheticProducer: return producerPrimer;
                case V5MvpRoute.Volvox: return volvoxPrimer;
                default: return System.Array.Empty<V5AdaptationId>();
            }
        }

        public static V5AdaptationId[] PlaytestAdaptationArray(V5MvpRoute route)
        {
            switch (route)
            {
                case V5MvpRoute.Bacteria: return bacteriaCore;
                case V5MvpRoute.Amoeba: return amoebaPlaytest;
                case V5MvpRoute.PhotosyntheticProducer: return producerPlaytest;
                case V5MvpRoute.Volvox: return volvoxPlaytest;
                default: return System.Array.Empty<V5AdaptationId>();
            }
        }

        public static V5AdaptationId[] BuildTargetArray(V5MvpRoute route)
        {
            switch (route)
            {
                case V5MvpRoute.Bacteria: return bacteriaBuildTargets;
                case V5MvpRoute.Amoeba: return amoebaBuildTargets;
                case V5MvpRoute.PhotosyntheticProducer: return producerBuildTargets;
                case V5MvpRoute.Volvox: return volvoxBuildTargets;
                default: return System.Array.Empty<V5AdaptationId>();
            }
        }

        public static int BuildTargetCount(V5MvpRoute route)
        {
            return BuildTargetArray(route).Length;
        }

        public static V5AdaptationId BuildTargetAt(V5MvpRoute route, int index)
        {
            V5AdaptationId[] targets = BuildTargetArray(route);
            return index >= 0 && index < targets.Length ? targets[index] : V5AdaptationId.None;
        }

        public static int BuildStage(V5MvpRoute route, V5AdaptationSystem adaptations)
        {
            V5AdaptationId[] targets = BuildTargetArray(route);
            if (adaptations == null || targets.Length == 0) return 0;

            int stage = 0;
            for (int i = 0; i < targets.Length; i++)
            {
                if (!adaptations.Has(targets[i])) break;
                stage++;
            }
            return stage;
        }

        public static float BuildProgress01(V5MvpRoute route, V5AdaptationSystem adaptations)
        {
            int count = BuildTargetCount(route);
            if (count <= 0) return 0f;
            return (float)BuildStage(route, adaptations) / count;
        }

        public static V5AdaptationId NextBuildTarget(V5MvpRoute route, V5AdaptationSystem adaptations)
        {
            V5AdaptationId[] targets = BuildTargetArray(route);
            if (adaptations == null || targets.Length == 0) return V5AdaptationId.None;

            for (int i = 0; i < targets.Length; i++)
                if (!adaptations.Has(targets[i])) return targets[i];
            return V5AdaptationId.None;
        }

        public static string BuildProgressText(V5MvpRoute route, V5AdaptationSystem adaptations)
        {
            int total = BuildTargetCount(route);
            if (route == V5MvpRoute.None || total == 0) return "Build MVP: sin ruta";

            int stage = BuildStage(route, adaptations);
            V5AdaptationDefinition next = V5AdaptationLibrary.Get(NextBuildTarget(route, adaptations));
            string nextText = next != null ? next.shortName : "build completo";
            return "Build " + DisplayName(route) + " " + stage + "/" + total + ": " +
                   BuildStageName(route, stage) + " | prox " + nextText;
        }

        public static string BuildTargetText(V5MvpRoute route)
        {
            V5AdaptationId[] targets = BuildTargetArray(route);
            if (targets.Length == 0) return "sin objetivos";

            string result = "";
            for (int i = 0; i < targets.Length; i++)
            {
                V5AdaptationDefinition def = V5AdaptationLibrary.Get(targets[i]);
                if (result.Length > 0) result += " > ";
                result += def != null ? def.shortName : targets[i].ToString();
            }
            return result;
        }

        public static string BuildStageName(V5MvpRoute route, int completedStage)
        {
            if (completedStage <= 0) return "semilla de identidad";

            switch (route)
            {
                case V5MvpRoute.Bacteria:
                    switch (completedStage)
                    {
                        case 1: return "membrana resistente";
                        case 2: return "swarm movil";
                        case 3: return "biofilm por contacto";
                        default: return "swarm de division rapida";
                    }
                case V5MvpRoute.Amoeba:
                    switch (completedStage)
                    {
                        case 1: return "puente movil";
                        case 2: return "cuerpo temprano listo";
                        case 3: return "eucariota cazadora";
                        case 4: return "digestor interno";
                        default: return "depredador por contacto";
                    }
                case V5MvpRoute.PhotosyntheticProducer:
                    switch (completedStage)
                    {
                        case 1: return "luz procariota";
                        case 2: return "base protegida";
                        case 3: return "salto eucariota";
                        case 4: return "cloroplasto online";
                        default: return "productor blindado";
                    }
                case V5MvpRoute.Volvox:
                    switch (completedStage)
                    {
                        case 1: return "adhesion inicial";
                        case 2: return "colonia luminica";
                        case 3: return "eucariota colonial";
                        case 4: return "fotosintesis interna";
                        case 5: return "cuerpo adhesivo";
                        default: return "cuerpo diferenciado";
                    }
                default:
                    return "sin build";
            }
        }

        public static V5MvpRoute CurrentRoute(V5GameManager gm)
        {
            if (gm == null) return V5MvpRoute.None;

            if (gm.Identity != null && gm.Identity.Identity != V5IdentityId.LUCA)
            {
                V5MvpRoute route = RouteForIdentity(gm.Identity.Identity);
                if (route != V5MvpRoute.None) return route;
            }

            if (gm.MotherCell != null)
            {
                V5MvpRoute route = RouteForPath(gm.MotherCell.EvolutionPath);
                if (route != V5MvpRoute.None) return route;
            }

            return BestRouteFromAdaptations(gm.Adaptations);
        }

        public static V5MvpRoute BestRouteFromAdaptations(V5AdaptationSystem adaptations)
        {
            V5MvpRoute best = V5MvpRoute.None;
            float bestScore = 0f;
            ScoreCandidate(V5MvpRoute.Bacteria, adaptations, ref best, ref bestScore);
            ScoreCandidate(V5MvpRoute.Amoeba, adaptations, ref best, ref bestScore);
            ScoreCandidate(V5MvpRoute.PhotosyntheticProducer, adaptations, ref best, ref bestScore);
            ScoreCandidate(V5MvpRoute.Volvox, adaptations, ref best, ref bestScore);
            return bestScore > 0f ? best : V5MvpRoute.None;
        }

        public static float Progress01(V5MvpRoute route, V5AdaptationSystem adaptations)
        {
            IReadOnlyList<V5AdaptationId> core = CoreAdaptations(route);
            if (adaptations == null || core.Count == 0) return 0f;
            return (float)InstalledCount(route, adaptations) / core.Count;
        }

        public static int InstalledCount(V5MvpRoute route, V5AdaptationSystem adaptations)
        {
            IReadOnlyList<V5AdaptationId> core = CoreAdaptations(route);
            if (adaptations == null || core.Count == 0) return 0;

            int count = 0;
            for (int i = 0; i < core.Count; i++)
                if (adaptations.Has(core[i])) count++;
            return count;
        }

        public static V5AdaptationId NextMissingCoreAdaptation(V5MvpRoute route, V5AdaptationSystem adaptations)
        {
            IReadOnlyList<V5AdaptationId> core = CoreAdaptations(route);
            if (adaptations == null || core.Count == 0) return V5AdaptationId.None;

            for (int i = 0; i < core.Count; i++)
                if (!adaptations.Has(core[i])) return core[i];
            return V5AdaptationId.None;
        }

        public static string ProgressText(V5MvpRoute route, V5AdaptationSystem adaptations)
        {
            IReadOnlyList<V5AdaptationId> core = CoreAdaptations(route);
            if (route == V5MvpRoute.None || core.Count == 0) return "MVP sin ruta";
            return DisplayName(route) + " " + InstalledCount(route, adaptations) + "/" + core.Count;
        }

        public static string CoreAdaptationText(V5MvpRoute route)
        {
            IReadOnlyList<V5AdaptationId> core = CoreAdaptations(route);
            if (core.Count == 0) return "ninguna";

            string result = "";
            for (int i = 0; i < core.Count; i++)
            {
                V5AdaptationDefinition def = V5AdaptationLibrary.Get(core[i]);
                string name = def != null && !string.IsNullOrEmpty(def.shortName) ? def.shortName : core[i].ToString();
                result += i == 0 ? name : ", " + name;
            }
            return result;
        }

        public static bool IsMvpCoreAdaptation(V5AdaptationId id)
        {
            return Contains(bacteriaCore, id) ||
                   Contains(amoebaCore, id) ||
                   Contains(producerCore, id) ||
                   Contains(volvoxCore, id);
        }

        public static V5MvpRoute RouteForAdaptation(V5AdaptationId id)
        {
            if (Contains(volvoxCore, id) && (id == V5AdaptationId.ColonialAdhesin ||
                                             id == V5AdaptationId.PersistentAdhesion ||
                                             id == V5AdaptationId.CellDifferentiation ||
                                             id == V5AdaptationId.SignalingCommunication))
                return V5MvpRoute.Volvox;

            if (Contains(amoebaCore, id) && (id == V5AdaptationId.Lysosome ||
                                             id == V5AdaptationId.Pseudopods ||
                                             id == V5AdaptationId.ContractileVacuole ||
                                             id == V5AdaptationId.Cilia ||
                                             id == V5AdaptationId.Mitochondria))
                return V5MvpRoute.Amoeba;

            if (Contains(producerCore, id) && (id == V5AdaptationId.ProkaryoticThylakoid ||
                                               id == V5AdaptationId.CatalaseROS ||
                                               id == V5AdaptationId.Chloroplast ||
                                               id == V5AdaptationId.CelluloseWall ||
                                               id == V5AdaptationId.SilicaFrustule))
                return V5MvpRoute.PhotosyntheticProducer;

            if (Contains(bacteriaCore, id)) return V5MvpRoute.Bacteria;
            return V5MvpRoute.None;
        }

        public static V5MvpRoute RouteForIdentity(V5IdentityId identity)
        {
            switch (identity)
            {
                case V5IdentityId.BacteriaSwarm:
                case V5IdentityId.Biofilm:
                    return V5MvpRoute.Bacteria;
                case V5IdentityId.Amoeba:
                case V5IdentityId.Ciliate:
                case V5IdentityId.ProtistBase:
                    return V5MvpRoute.Amoeba;
                case V5IdentityId.Cyanobacteria:
                case V5IdentityId.Microalga:
                case V5IdentityId.Diatom:
                    return V5MvpRoute.PhotosyntheticProducer;
                case V5IdentityId.VolvoxEarly:
                case V5IdentityId.VolvoxComplete:
                    return V5MvpRoute.Volvox;
                default:
                    return V5MvpRoute.None;
            }
        }

        public static V5MvpRoute RouteForPath(V5EvolutionPath path)
        {
            switch (path)
            {
                case V5EvolutionPath.Bacteria:
                    return V5MvpRoute.Bacteria;
                case V5EvolutionPath.Amoeba:
                case V5EvolutionPath.Ciliate:
                case V5EvolutionPath.Flagellate:
                    return V5MvpRoute.Amoeba;
                case V5EvolutionPath.Cyanobacteria:
                case V5EvolutionPath.Microalga:
                    return V5MvpRoute.PhotosyntheticProducer;
                default:
                    return V5MvpRoute.None;
            }
        }

        public static string DisplayName(V5MvpRoute route)
        {
            switch (route)
            {
                case V5MvpRoute.Bacteria: return "Bacteria";
                case V5MvpRoute.Amoeba: return "Ameba";
                case V5MvpRoute.PhotosyntheticProducer: return "Productor fotosintetico";
                case V5MvpRoute.Volvox: return "Volvox";
                default: return "Sin ruta MVP";
            }
        }

        public static string Fantasy(V5MvpRoute route)
        {
            switch (route)
            {
                case V5MvpRoute.Bacteria:
                    return "Sobrevivo por cantidad, velocidad y division.";
                case V5MvpRoute.Amoeba:
                    return "Cazo, fagocito y crezco.";
                case V5MvpRoute.PhotosyntheticProducer:
                    return "Transformo el mapa hasta que el ambiente pelea por mi.";
                case V5MvpRoute.Volvox:
                    return "Protejo germinales internas con un cuerpo especializado.";
                default:
                    return "Todavia no hay fantasia MVP consolidada.";
            }
        }

        public static string Objective(V5MvpRoute route)
        {
            switch (route)
            {
                case V5MvpRoute.Bacteria:
                    return "Swarm resistente: pared, movilidad, contacto y division para ocupar el mapa.";
                case V5MvpRoute.Amoeba:
                    return "Depredador flexible: cruza a Nucleo, digiere presas y gana control por contacto.";
                case V5MvpRoute.PhotosyntheticProducer:
                    return "Ingeniero de luz: tilacoide, catalasa y cloroplasto convierten ambiente en economia.";
                case V5MvpRoute.Volvox:
                    return "Cuerpo colonial: adhesion, luz y senales protegen una colonia especializada.";
                default:
                    return "Elige una ruta MVP para fijar una fantasia jugable.";
            }
        }

        private static bool Contains(V5AdaptationId[] list, V5AdaptationId id)
        {
            for (int i = 0; i < list.Length; i++)
                if (list[i] == id) return true;
            return false;
        }

        private static void ScoreCandidate(V5MvpRoute candidate, V5AdaptationSystem adaptations, ref V5MvpRoute best, ref float bestScore)
        {
            float score = Progress01(candidate, adaptations);
            if (score > bestScore)
            {
                bestScore = score;
                best = candidate;
            }
        }
    }
}
