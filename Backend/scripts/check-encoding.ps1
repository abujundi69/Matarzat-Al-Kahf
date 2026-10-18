[CmdletBinding()]
param(
    [string]$RepositoryRoot
)

$ErrorActionPreference = 'Stop'
if ([string]::IsNullOrWhiteSpace($RepositoryRoot)) {
    $scriptDirectory = [IO.Path]::GetDirectoryName($MyInvocation.MyCommand.Path)
    $backendDirectory = [IO.Directory]::GetParent($scriptDirectory).FullName
    $RepositoryRoot = [IO.Directory]::GetParent($backendDirectory).FullName
}
$repositoryFullPath = [IO.Path]::GetFullPath(
    (Resolve-Path -LiteralPath $RepositoryRoot).Path
).TrimEnd([IO.Path]::DirectorySeparatorChar, [IO.Path]::AltDirectorySeparatorChar)
$strictUtf8 = [Text.UTF8Encoding]::new($false, $true)
$sourceExtensions = [Collections.Generic.HashSet[string]]::new(
    [StringComparer]::OrdinalIgnoreCase
)
@(
    '.config', '.cs', '.csproj', '.css', '.csv', '.env', '.html', '.json',
    '.jsx', '.md', '.ps1', '.sql', '.sln', '.ts', '.tsx', '.txt', '.xml',
    '.yaml', '.yml'
) | ForEach-Object { [void]$sourceExtensions.Add($_) }

$excludedDirectories = [Collections.Generic.HashSet[string]]::new(
    [StringComparer]::OrdinalIgnoreCase
)
@(
    '.git', '.vs', 'bin', 'dist', 'node_modules', 'obj'
) | ForEach-Object { [void]$excludedDirectories.Add($_) }

$failures = [Collections.Generic.List[string]]::new()
$filesChecked = 0

function Test-ExcludedPath {
    param([string]$FullName)

    $relativePath = [IO.Path]::GetFullPath($FullName).Substring(
        $repositoryFullPath.Length
    ).TrimStart([IO.Path]::DirectorySeparatorChar, [IO.Path]::AltDirectorySeparatorChar)
    foreach ($segment in ($relativePath -split '[\\/]')) {
        if ($excludedDirectories.Contains($segment)) {
            return $true
        }
    }

    return $false
}

Get-ChildItem -LiteralPath $RepositoryRoot -Recurse -File |
    Where-Object {
        $sourceExtensions.Contains($_.Extension) -and
        -not (Test-ExcludedPath -FullName $_.FullName)
    } |
    ForEach-Object {
        $filesChecked++
        $relativePath = [IO.Path]::GetFullPath($_.FullName).Substring(
            $repositoryFullPath.Length
        ).TrimStart([IO.Path]::DirectorySeparatorChar, [IO.Path]::AltDirectorySeparatorChar)

        try {
            $text = $strictUtf8.GetString([IO.File]::ReadAllBytes($_.FullName))
        }
        catch [Text.DecoderFallbackException] {
            $failures.Add("$relativePath`: invalid UTF-8 byte sequence")
            return
        }

        if ($text.Contains([char]0xFFFD)) {
            $failures.Add("$relativePath`: contains the Unicode replacement character U+FFFD")
        }

        # Common UTF-8 decoded as Windows-1252/Latin-1, including Arabic mojibake.
        if ($text -match '(?:\u00C3[\u0080-\u00BF]|\u00C2[\u0080-\u00BF]|\u00D8[\u0080-\u00BF]|\u00D9[\u0080-\u00BF]|\u00E2[\u0080-\u00BF]|\u00EF\u00BF\u00BD)') {
            $failures.Add("$relativePath`: contains a likely mojibake sequence")
        }
    }

if ($failures.Count -gt 0) {
    Write-Error ("Encoding validation failed:`n - " + ($failures -join "`n - "))
}

Write-Host "Encoding validation passed for $filesChecked source/configuration files."
