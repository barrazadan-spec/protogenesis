$word = New-Object -ComObject Word.Application
$word.Visible = $false

$doc1 = $word.Documents.Open('C:\Users\barra\OneDrive\Desktop\PROYECTO PROTOGENESIS IA\ProtogenesisPrimordia_GDD_v2_Definitivo.docx')
$doc1.Content.Text | Out-File 'C:\Users\barra\OneDrive\Desktop\PROYECTO PROTOGENESIS IA\GDD_v2.txt' -Encoding UTF8
$doc1.Close()

$doc2 = $word.Documents.Open('C:\Users\barra\OneDrive\Desktop\PROYECTO PROTOGENESIS IA\Primordia_Prompts_v2_Completo.docx')
$doc2.Content.Text | Out-File 'C:\Users\barra\OneDrive\Desktop\PROYECTO PROTOGENESIS IA\Prompts_v2.txt' -Encoding UTF8
$doc2.Close()

$word.Quit()
Write-Host "Done"
