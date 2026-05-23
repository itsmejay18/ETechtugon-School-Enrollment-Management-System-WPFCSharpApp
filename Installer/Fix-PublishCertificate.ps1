param(
    [string]$ProjectPath = ".\School-Management-System\School-Management-System.csproj",
    [string]$PfxPath = ".\School-Management-System\Properties\ClickOnce\SchoolManagementSystem_ClickOnce.pfx",
    [string]$Subject = "CN=School Management System ClickOnce",
    [string]$Password = $env:SMS_CLICKONCE_CERT_PASSWORD
)

$ErrorActionPreference = "Stop"

$projectFullPath = (Resolve-Path $ProjectPath).Path
if ([string]::IsNullOrWhiteSpace($Password)) {
    throw "Certificate password is required. Pass -Password <strong-password> or set SMS_CLICKONCE_CERT_PASSWORD."
}

$pfxDir = Split-Path $PfxPath -Parent
if (-not (Test-Path $pfxDir)) {
    New-Item -ItemType Directory -Force -Path $pfxDir | Out-Null
}

$cert = New-SelfSignedCertificate -Type CodeSigningCert -Subject $Subject -CertStoreLocation "Cert:\CurrentUser\My" -NotAfter (Get-Date).AddYears(5)
$securePassword = ConvertTo-SecureString -String $Password -AsPlainText -Force
Export-PfxCertificate -Cert $cert -FilePath $PfxPath -Password $securePassword | Out-Null

[xml]$xml = Get-Content -Path $projectFullPath
$ns = New-Object System.Xml.XmlNamespaceManager($xml.NameTable)
$ns.AddNamespace("msb", "http://schemas.microsoft.com/developer/msbuild/2003")

$rootPropGroup = $xml.SelectSingleNode("/msb:Project/msb:PropertyGroup[1]", $ns)
if (-not $rootPropGroup) {
    throw "Failed to locate project PropertyGroup in csproj."
}

function Set-OrCreateNode($parent, $name, $value) {
    $node = $parent.SelectSingleNode("msb:$name", $ns)
    if (-not $node) {
        $node = $xml.CreateElement($name, "http://schemas.microsoft.com/developer/msbuild/2003")
        [void]$parent.AppendChild($node)
    }
    $node.InnerText = $value
}

$projectDir = Split-Path $projectFullPath -Parent
$pfxFullPath = (Resolve-Path $PfxPath).Path
$projectUri = New-Object System.Uri(($projectDir.TrimEnd('\') + '\'))
$pfxUri = New-Object System.Uri($pfxFullPath)
$relativePfx = [System.Uri]::UnescapeDataString($projectUri.MakeRelativeUri($pfxUri).ToString()).Replace('/', '\')
Set-OrCreateNode $rootPropGroup "GenerateManifests" "true"
Set-OrCreateNode $rootPropGroup "SignManifests" "true"
Set-OrCreateNode $rootPropGroup "ManifestKeyFile" $relativePfx
Set-OrCreateNode $rootPropGroup "ManifestCertificateThumbprint" $cert.Thumbprint
Set-OrCreateNode $rootPropGroup "PublishWizardCompleted" "true"

$xml.Save($projectFullPath)

Write-Host "Created ClickOnce certificate and updated project settings:"
Write-Host "  Thumbprint: $($cert.Thumbprint)"
Write-Host "  PFX: $((Resolve-Path $PfxPath).Path)"
