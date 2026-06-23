"""
Comments out lines that access members of deprecated stub variables (typed as object).
"""
import re, glob, os

BASE = r"C:\Users\barra\OneDrive\Desktop\PROYECTO PROTOGENESIS IA\Assets\Scripts"

STUB_VARS = ['cap', 'ps', 'origin', 'merged', 'vis', 'species', 's']

def has_member_access(line):
    stripped = line.strip()
    if stripped.startswith('//'):
        return False
    for v in STUB_VARS:
        if re.search(r'\b' + re.escape(v) + r'\s*\.\s*\w+', line):
            return True
    return False

def process_file(filepath):
    with open(filepath, 'r', encoding='utf-8', errors='replace') as f:
        lines = f.readlines()

    changed = False
    new_lines = []
    for line in lines:
        if has_member_access(line):
            indent = len(line) - len(line.lstrip())
            prefix = ' ' * indent
            new_lines.append(prefix + '// TODO: Primordia — ' + line.lstrip())
            changed = True
        else:
            new_lines.append(line)

    if changed:
        with open(filepath, 'w', encoding='utf-8') as f:
            f.writelines(new_lines)
        return True
    return False

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
