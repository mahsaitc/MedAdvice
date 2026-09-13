# One-time export: pulls every image still stored as varbinary(max) in the old
# MedAdviceDb (pre path-refactor) out to files under wwwroot/uploads/<folder>/,
# and writes apply_image_paths.sql with the UPDATE statements to run afterwards
# (after `dotnet ef database update` has added the *Path columns).
#
# Safe to re-run: it always re-exports and regenerates apply_image_paths.sql.

Add-Type -AssemblyName System.Data

$ServerInstance = ".\MAHSASQL"
$Database       = "MedAdviceDb"
$ProjectRoot    = "C:\Users\EMDAD RAYANEH\Desktop\claude\myprojects\MedAdvice"
$WwwRoot        = Join-Path $ProjectRoot "MedAdvice\wwwroot\uploads"
$SqlOutPath     = Join-Path $ProjectRoot "apply_image_paths.sql"

$Mappings = @(
    @{ Table = "adviceCategories";     IdCol = "Id"; BlobCol = "AdviceCategoryPicture"; PathCol = "AdviceCategoryPicturePath"; Folder = "advices" }
    @{ Table = "Advices";              IdCol = "Id"; BlobCol = "AdviceHeaderImage";      PathCol = "AdviceHeaderImagePath";      Folder = "advices" }
    @{ Table = "AdviceImages";         IdCol = "Id"; BlobCol = "Adviceimg";              PathCol = "AdviceimgPath";              Folder = "advices" }
    @{ Table = "blogCategories";       IdCol = "Id"; BlobCol = "BlogCategoryPicture";    PathCol = "BlogCategoryPicturePath";    Folder = "blogs" }
    @{ Table = "Blogs";                IdCol = "Id"; BlobCol = "BlogHeaderImage";        PathCol = "BlogHeaderImagePath";        Folder = "blogs" }
    @{ Table = "BlogImages";           IdCol = "Id"; BlobCol = "Blogimg";                PathCol = "BlogimgPath";                Folder = "blogs" }
    @{ Table = "Doctors";              IdCol = "Id"; BlobCol = "DrProfileImage";         PathCol = "DrProfileImagePath";         Folder = "doctors" }
    @{ Table = "DoctorImages";         IdCol = "Id"; BlobCol = "Doctorimg";              PathCol = "DoctorimgPath";              Folder = "doctors" }
    @{ Table = "DoctorSpacialities";   IdCol = "Id"; BlobCol = "DrSpacialityPicture";    PathCol = "DrSpacialityPicturePath";    Folder = "doctors" }
    @{ Table = "HomepageContents";     IdCol = "Id"; BlobCol = "HeroImage";              PathCol = "HeroImagePath";              Folder = "homepage" }
    @{ Table = "ProductImages";        IdCol = "Id"; BlobCol = "img";                    PathCol = "imgPath";                    Folder = "products" }
)

function Get-Extension([byte[]]$bytes) {
    if ($bytes.Length -ge 3 -and $bytes[0] -eq 0xFF -and $bytes[1] -eq 0xD8 -and $bytes[2] -eq 0xFF) { return ".jpg" }
    if ($bytes.Length -ge 8 -and $bytes[0] -eq 0x89 -and $bytes[1] -eq 0x50 -and $bytes[2] -eq 0x4E -and $bytes[3] -eq 0x47) { return ".png" }
    return ".jpg"
}

$connectionString = "Server=$ServerInstance;Database=$Database;Trusted_Connection=True;"
$connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
$connection.Open()

$sqlLines = New-Object System.Collections.Generic.List[string]
$sqlLines.Add("SET NOCOUNT ON;")

$grandTotal = 0
$grandExported = 0

foreach ($m in $Mappings) {
    $folderPath = Join-Path $WwwRoot $m.Folder
    New-Item -ItemType Directory -Path $folderPath -Force | Out-Null

    $cmdText = "SELECT [$($m.IdCol)] AS RowId, [$($m.BlobCol)] AS Blob FROM [$($m.Table)] WHERE [$($m.BlobCol)] IS NOT NULL"
    $cmd = New-Object System.Data.SqlClient.SqlCommand($cmdText, $connection)
    $reader = $cmd.ExecuteReader()

    $tableTotal = 0
    $tableExported = 0

    while ($reader.Read()) {
        $tableTotal++
        $rowId = $reader["RowId"]
        $blob = $reader["Blob"]
        if ($blob -eq [DBNull]::Value -or $blob.Length -eq 0) { continue }

        $ext = Get-Extension $blob
        $fileName = [guid]::NewGuid().ToString("N") + $ext
        $filePath = Join-Path $folderPath $fileName
        [System.IO.File]::WriteAllBytes($filePath, $blob)

        $webPath = "/uploads/$($m.Folder)/$fileName"
        $webPathEscaped = $webPath.Replace("'", "''")
        $sqlLines.Add("UPDATE [$($m.Table)] SET [$($m.PathCol)] = N'$webPathEscaped' WHERE [$($m.IdCol)] = $rowId;")
        $tableExported++
    }
    $reader.Close()

    Write-Host "$($m.Table).$($m.BlobCol): $tableTotal rows with data, $tableExported exported"
    $grandTotal += $tableTotal
    $grandExported += $tableExported
}

$connection.Close()

[System.IO.File]::WriteAllLines($SqlOutPath, $sqlLines, [System.Text.Encoding]::UTF8)

Write-Host ""
Write-Host "TOTAL: $grandExported of $grandTotal images exported."
Write-Host "Files written under: $WwwRoot\<folder>\"
Write-Host "UPDATE script written to: $SqlOutPath"
Write-Host ""
Write-Host "Next: run 'dotnet ef database update' (in MedAdvice\MedAdvice), then apply the UPDATE script with sqlcmd."
