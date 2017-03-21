Add-Type -AssemblyName System.IO.Compression.FileSystem
function Unzip
{
    param([string]$zipfile, [string]$outpath)
    [System.IO.Compression.ZipFile]::ExtractToDirectory($zipfile, $outpath)
}

$env:ELASTICSEARCH_INSTALL_DIR = "$pwd\.es"
Write-Host $env:ELASTICSEARCH_INSTALL_DIR
if(!(Test-Path -Path $env:ELASTICSEARCH_INSTALL_DIR )) {
  mkdir $env:ELASTICSEARCH_INSTALL_DIR -Force | Out-Null  
}

$env:ELASTICSEARCH_ARCHIVE_FILE = "$env:ELASTICSEARCH_INSTALL_DIR\elasticsearch-5.2.2.zip"
Write-Host $env:ELASTICSEARCH_ARCHIVE_FILE
if(!(Test-Path -Path $env:ELASTICSEARCH_ARCHIVE_FILE )) {
  (New-Object System.Net.WebClient).DownloadFile("https://artifacts.elastic.co/downloads/elasticsearch/elasticsearch-5.2.2.zip", $env:ELASTICSEARCH_ARCHIVE_FILE)
  Unzip $env:ELASTICSEARCH_ARCHIVE_FILE $env:ELASTICSEARCH_INSTALL_DIR
}

Write-Host "Start ES"
start-process "cmd.exe" "/c $env:ELASTICSEARCH_INSTALL_DIR\elasticsearch-5.2.2\bin\elasticsearch.bat"