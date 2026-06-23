"""
Limpia referencias a scripts deprecados en archivos .cs del proyecto Primordia.
Estrategia: comentar líneas que solo referencian clases deprecadas.
"""
import os, re, glob

BASE = r"C:\Users\barra\OneDrive\Desktop\PROYECTO PROTOGENESIS IA\Assets\Scripts"
DEPRECATED_DIR = os.path.join(BASE, "_Deprecated")

# Clases deprecadas (nombres exactos)
DEPRECATED_CLASSES = [
    "CAP", "CAPStats", "ProtobacteriaSystem", "ProtobacteriaSelectUI",
    "MorphogenesisVisualizer", "OrganismStateVisualizer",
    "PhenotypeSystem", "PhenotypeType",
    "ChemicalSignalSystem", "MicroscopeModeManager",
    "EcologicalDirector", "GranOxigenacion",
    "EcosystemEventScheduler", "EcosystemPopulation",
    "CampaignManager", "MissionData",
    "CounterVisualizationSystem"
]

# Namespaces que solo contienen scripts deprecados (eliminar using entero)
DEPRECATED_NAMESPACES = [
    "Protogenesis.Campaign",
]

# Clases NO deprecadas que comparten namespace con deprecadas
# (para no eliminar el using completo si estas siguen siendo usadas)
NON_DEPRECATED_IN_MIXED_NS = {
    "Protogenesis.Player": ["EraManager"],
    "Protogenesis.Ecosystem": ["EcosystemManager", "ZoneManager", "BiomeZone", "ResourceNode", "ZoneType"],
    "Protogenesis.Rendering": ["ZoomManager", "MapVisualizer"],
    "Protogenesis.UI": ["ResourceHUD", "SlotInstallUI", "DivisionHintUI", "EvolutionHUD",
                        "EraProgressUI", "OrganelleUI", "AbilityUI", "GeneticTreeUI",
                        "PostMortemUI", "AlertSystem", "EnvironmentMonitor"],
}

# Build regex para detectar referencias directas a clases deprecadas
# Matchea: ClassName.Xxx, new ClassName, GetComponent<ClassName>, typeof(ClassName),
# ClassName varName, (ClassName), [SerializeField] ClassName
CLASS_PATTERN = re.compile(
    r'\b(' + '|'.join(re.escape(c) for c in DEPRECATED_CLASSES) + r')\b'
)

# Usando statements a eliminar completamente
NS_USING_PATTERN = {
    ns: re.compile(r'^\s*using\s+' + re.escape(ns) + r'\s*;\s*$')
    for ns in DEPRECATED_NAMESPACES
}

def contains_non_deprecated(line, non_deprecated_list):
    for cls in non_deprecated_list:
        if cls in line:
            return True
    return False

def process_file(filepath):
    with open(filepath, 'r', encoding='utf-8', errors='replace') as f:
        lines = f.readlines()

    changed = False
    new_lines = []

    for line in lines:
        original = line
        stripped = line.strip()

        # Skip already commented lines
        if stripped.startswith('//') or stripped.startswith('/*'):
            new_lines.append(line)
            continue

        # 1. Remove pure deprecated namespace usings
        remove_line = False
        for ns, pat in NS_USING_PATTERN.items():
            if pat.match(line):
                new_lines.append('// [Primordia] ' + line.lstrip())
                changed = True
                remove_line = True
                break
        if remove_line:
            continue

        # 2. Check if line references a deprecated class
        if CLASS_PATTERN.search(line):
            # Check if it's a using statement for a mixed namespace
            using_match = re.match(r'^\s*using\s+(Protogenesis\.\w+)\s*;\s*$', line)
            if using_match:
                ns = using_match.group(1)
                if ns in NON_DEPRECATED_IN_MIXED_NS:
                    # Keep the using — the namespace has non-deprecated classes too
                    new_lines.append(line)
                    continue
                else:
                    # Unknown/pure deprecated namespace using
                    new_lines.append('// [Primordia] ' + line.lstrip())
                    changed = True
                    continue

            # It's a code line — comment it out with TODO
            indent = len(line) - len(line.lstrip())
            prefix = line[:indent]
            new_lines.append(prefix + '// TODO: Primordia — ' + line.lstrip())
            changed = True
            continue

        new_lines.append(line)

    if changed:
        with open(filepath, 'w', encoding='utf-8') as f:
            f.writelines(new_lines)
        return True
    return False


# Process all .cs files except _Deprecated
files = glob.glob(os.path.join(BASE, '**', '*.cs'), recursive=True)
modified = []
for f in files:
    if '_Deprecated' in f:
        continue
    if process_file(f):
        modified.append(f.replace(BASE + os.sep, ''))

print(f"Modified {len(modified)} files:")
for m in sorted(modified):
    print(f"  {m}")
