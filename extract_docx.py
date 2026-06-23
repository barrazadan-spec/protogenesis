import zipfile, re, sys

def extract_text(path):
    with zipfile.ZipFile(path) as z:
        with z.open('word/document.xml') as f:
            xml = f.read().decode('utf-8')
    # Strip XML tags, keep text
    text = re.sub(r'<w:br[^/]*/>', '\n', xml)
    text = re.sub(r'</w:p>', '\n', text)
    text = re.sub(r'<[^>]+>', '', text)
    # Decode XML entities
    text = text.replace('&amp;', '&').replace('&lt;', '<').replace('&gt;', '>').replace('&apos;', "'").replace('&quot;', '"')
    return text

for src, dst in [
    (r'C:\Users\barra\OneDrive\Desktop\PROYECTO PROTOGENESIS IA\ProtogenesisPrimordia_GDD_v2_Definitivo.docx',
     r'C:\Users\barra\OneDrive\Desktop\PROYECTO PROTOGENESIS IA\GDD_v2.txt'),
    (r'C:\Users\barra\OneDrive\Desktop\PROYECTO PROTOGENESIS IA\Primordia_Prompts_v2_Completo.docx',
     r'C:\Users\barra\OneDrive\Desktop\PROYECTO PROTOGENESIS IA\Prompts_v2.txt'),
]:
    text = extract_text(src)
    with open(dst, 'w', encoding='utf-8') as f:
        f.write(text)
    print(f"OK: {dst} ({len(text)} chars)")
