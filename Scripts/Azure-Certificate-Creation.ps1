$clearPassword = "P@ssw0rd!"
$password = $clearPassword | ConvertTo-SecureString -AsPlainText -Force

$cert = New-PnPAzureCertificate -CommonName "StarterKitProvisioning" -ValidYears 5 -Out C:\temp\StarterKitProvisioning.pfx -CertificatePassword $password
$cert.KeyCredentials | clip
$cert.Thumbprint | clip

$cert = Get-PnPAzureCertificate -CertificatePath C:\temp\SPProvisioningApp.pfx -CertificatePassword $password
$cert.KeyCredentials | clip
$cert.Thumbprint | clip
