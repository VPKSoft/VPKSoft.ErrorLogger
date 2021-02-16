<#
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
#>

Write-Output "Init NuGet push..."

$output_file = ".\CryptEnvVar.exe"

$download_url = "https://www.vpksoft.net/toolset/CryptEnvVar.exe"

Write-Output "Download file:  $download_url ..."
(New-Object System.Net.WebClient).DownloadFile($download_url, $output_file)
Write-Output "Download done."

# application parameters..
$application = "."
$environment_cryptor = "CryptEnvVar.exe"

# create the digital signature..
$arguments = @("-s", $Env:SECRET_KEY, "-e", "CERT_1;CERT_2;CERT_3;CERT_4;CERT_5;CERT_6;CERT_7;CERT_8", "-f", "C:\vpksoft.pfx", "-w", "80", "-i", "-v")
& (-join($application, "\", $environment_cryptor)) $arguments

#create nuget.config file..
$arguments = @("-s", $Env:SECRET_KEY, "-e", "NUGET_CONFIG", "-f", ".\nuget.config", "-w", "80", "-i", "-v")
& (-join($application, "\", $environment_cryptor)) $arguments

# register the certificate to the CI image..
$certpw=ConvertTo-SecureString $Env:PFX_PASS –asplaintext –force 
Import-PfxCertificate -FilePath "C:\vpksoft.pfx" -CertStoreLocation Cert:\LocalMachine\My -Password $certpw | Out-Null

# sign and push the NuGet packages..
if ([string]::IsNullOrEmpty($Env:CIRCLE_PR_NUMBER)) # dont push on PR's..
{
    $files = Get-ChildItem $Env:CIRCLE_WORKING_DIRECTORY -r -Filter .\*VPKSoft.ErrorLogger*.nupkg # use the mask to discard possible third party packages..
    for ($i = 0; $i -lt $files.Count; $i++) 
    { 
        $file = $files[$i].FullName

        # sign the NuGet packages.
	    Write-Output (-join("Signing package: ", $file, " ..."))

        $arguments = @("sign", $file, "-CertificatePath", "C:\vpksoft.pfx", "-Timestamper", "http://timestamp.comodoca.com", "-CertificatePassword", $Env:PFX_PASS)

        nuget.exe $arguments > null 2>&1

	    Write-Output (-join("Package signed: ", $file, "."))

        # push the NuGet packges..
        $nuget_api = "https://api.nuget.org/v3/index.json"
        #$nuget_api = "https://apiint.nugettest.org/v3/index.json"
        $nuget_packages_api = "https://nuget.pkg.github.com/VPKSoft/index.json"

	    Write-Output (-join("Pushing NuGet:", $file, " ..."))
        
        # To nuget.org..
        $arguments = @("push", $file, $Env:NUGET_APIKEY, "-Source", $nuget_api, "-SkipDuplicate")
        #$args = @("push", $file, $Env:NUGET_TEST_APIKEY, "-Source", $nuget_api, "-SkipDuplicate")
        nuget.exe $arguments

        # To GitHub packages..
        $arguments = @("push", $file, $Env:NUGET_PACKAGES_API, "-Source", $nuget_packages_api, "-SkipDuplicate")
        nuget.exe $arguments

	    Write-Output (-join("Pushing done:", $file, "."))
    }
    Write-Output "NuGet push finished."
}
else
{
    Write-Output (-join("PR detected, no package publish: ", $Env:CIRCLE_PR_NUMBER))
}
