"""
Inserta 'object X = null;' después de cada línea '// TODO: Primordia — var X = ...'
para que el código compile. El null hace que los null-checks fallen silenciosamente.
"""
import os, re, glob

BASE = r"C:\Users\barra\OneDrive\Desktop\PROYECTO PROTOGENESIS IA\Assets\Scripts"

# Pattern: // TODO: Primordia — var NAME = ...
TODO_VAR = re.compile(r'//\s*TODO:\s*Primordia\s*[—\-]+\s*var\s+(\w+)\s*=')

def process_file(filepath):
    with open(filepath, 'r', encoding='utf-8', errors='replace') as f:
        lines = f.readlines()

    changed = False
    new_lines = []
    i = 0
    while i < len(lines):
        line = lines[i]
        m = TODO_VAR.search(line)
        if m:
            varname = m.group(1)
            new_lines.append(line)
            # Check if next non-empty line already declares this var
            next_idx = i + 1
            while next_idx < len(lines) and lines[next_idx].strip() == '':
                next_idx += 1

            already_declared = False
            if next_idx < len(lines):
                next_stripped = lines[next_idx].strip()
                # Already has a stub?
                if re.match(r'object\s+' + re.escape(varname) + r'\s*=', next_stripped):
                    already_declared = True

            if not already_declared:
                indent = len(line) - len(line.lstrip())
                prefix = ' ' * indent
                new_lines.append(f"{prefix}object {varname} = null; // Primordia migration stub\n")
                changed = True
        else:
            new_lines.append(line)
        i += 1

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
