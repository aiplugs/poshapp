@{
    RootModule        = 'PSModule.psm1'
    ModuleVersion     = '2.2.4.1'
    GUID              = '1d73a601-4a6c-43c5-ba3f-619b18bbb404'
    Author            = 'Microsoft Corporation'
    CompanyName       = 'Microsoft Corporation'
    Copyright         = '(c) Microsoft Corporation. All rights reserved.'
    Description       = 'PowerShell module with commands for discovering, installing, updating and publishing the PowerShell artifacts like Modules, DSC Resources, Role Capabilities and Scripts.'
    PowerShellVersion = '3.0'
    FormatsToProcess  = 'PSGet.Format.ps1xml'
FunctionsToExport = @(
	'Find-Command',
	'Find-DSCResource',
	'Find-Module',
	'Find-RoleCapability',
	'Find-Script',
	'Get-CredsFromCredentialProvider',
	'Get-InstalledModule',
	'Get-InstalledScript',
	'Get-PSRepository',
	'Install-Module',
	'Install-Script',
	'New-ScriptFileInfo',
	'Publish-Module',
	'Publish-Script',
	'Register-PSRepository',
	'Save-Module',
	'Save-Script',
	'Set-PSRepository',
	'Test-ScriptFileInfo',
	'Uninstall-Module',
	'Uninstall-Script',
	'Unregister-PSRepository',
	'Update-Module',
	'Update-ModuleManifest',
	'Update-Script',
	'Update-ScriptFileInfo')

    VariablesToExport = 'PSGetPath'
    AliasesToExport   = @('inmo', 'fimo', 'upmo', 'pumo')
    FileList          = @('PSModule.psm1',
        'PSGet.Format.ps1xml',
        'PSGet.Resource.psd1')
    RequiredModules   = @(@{ModuleName = 'PackageManagement'; ModuleVersion = '1.4.4' })
    PrivateData       = @{
        "PackageManagementProviders"           = 'PSModule.psm1'
        "SupportedPowerShellGetFormatVersions" = @('1.x', '2.x')
        PSData                                 = @{
            Tags         = @('Packagemanagement',
                'Provider',
                'PSEdition_Desktop',
                'PSEdition_Core',
                'Linux',
                'Mac')
            ProjectUri   = 'https://go.microsoft.com/fwlink/?LinkId=828955'
            LicenseUri   = 'https://go.microsoft.com/fwlink/?LinkId=829061'
            ReleaseNotes = @'
### 2.2.4.1
- Remove catalog file

### 2.2.3
- Update `HelpInfoUri` to point to the latest content (#560)
- Improve discovery of usable nuget.exe binary (Thanks bwright86!) (#558)

### 2.2.2
Bug Fix

- Update casing of DscResources output

### 2.2.1
Bug Fix

- Allow DscResources to work on case sensitive platforms (#521)
- Fix for failure to return credential provider when using private feeds (#521)

## 2.2
Bug Fix

- Fix for prompting for credentials when passing in -Credential parameter when using Register-PSRepository

## 2.1.5
New Features

- Add and remove nuget based repositories as a nuget source when nuget client tool is installed (#498)

Bug Fix

- Fix for 'Failed to publish module' error thrown when publishing modules (#497)

## 2.1.4
- Fixed hang while publishing some packages (#478)

## 2.1.3
New Features

- Added -Scope parameter to Update-Module (Thanks @lwajswaj!) (#471)
- Added -Exclude parameter to Publish-Module (Thanks @Benny1007!) (#191)
- Added -SkipAutomaticTags parameter to Publish-Module (Thanks @awickham10!) (#452)

Bug Fix

- Fixed issue with finding modules using macOS and .NET Core 3.0

## 2.1.2

New Feature

- Added support for registering repositories with special characters

## 2.1.1

- Fix DSC resource folder structure

## 2.1.0

Breaking Change

- Default installation scope for Update-Module and Update-Script has changed to match Install-Module and Install-Script. For Windows PowerShell (version 5.1 or below), the default scope is AllUsers when running in an elevated session, and CurrentUser at all other times.
  For PowerShell version 6.0.0 and above, the default installation scope is always CurrentUser. (#421)

Bug Fixes

- Update-ModuleManifest no longer clears FunctionsToExport, AliasesToExport, nor NestModules (#415 & #425) (Thanks @pougetat and @tnieto88!)
- Update-Module no longer changes repository URL (#407)
- Update-ModuleManifest no longer preprends 'PSGet_' to module name (#403) (Thanks @ThePoShWolf)
- Update-ModuleManifest now throws error and fails to update when provided invalid entries (#398) (Thanks @pougetat!)
- Ignore files no longer being included when uploading modules (#396)

New Features

- New DSC resource, PSRepository (#426) (Thanks @johlju!)
- Piping of PS respositories (#420)
- utf8 support for .nuspec (#419)

## 2.0.4

Bug Fix
* Remove PSGallery availability checks (#374)

## 2.0.3

Bug fixes and Improvements
* Fix CommandAlreadyAvailable error for PackageManagement module (#333)
* Remove trailing whitespace when value is not provided for Get-PSScriptInfoString (#337) (Thanks @thomasrayner)
* Expanded aliases for improved readability (#338) (Thanks @lazywinadmin)
* Improvements for Catalog tests (#343)
* Fix Update-ScriptInfoFile to preserve PrivateData (#346) (Thanks @tnieto88)
* Import modules with many commands faster (#351)

New Features
* Tab completion for -Repository parameter (#339) and for Publish-Module -Name (#359) (Thanks @matt9ucci)

## 2.0.1

Bug fixes
- Resolved Publish-Module doesn't report error but fails to publish module (#316)
- Resolved CommandAlreadyAvailable error while installing the latest version of PackageManagement module (#333)

## 2.0.0

Breaking Change
- Default installation scope for Install-Module, Install-Script, and Install-Package has changed. For Windows PowerShell (version 5.1 or below), the default scope is AllUsers when running in an elevated session, and CurrentUser at all other times.
  For PowerShell version 6.0.0 and above, the default installation scope is always CurrentUser.

## 1.6.7

Bug fixes
- Resolved Install/Save-Module error in PSCore 6.1.0-preview.4 on Ubuntu 18.04 OS (WSL/Azure) (#313)
- Updated error message in Save-Module cmdlet when the specified path is not accessible (#313)
- Added few additional verbose messages (#313)

## 1.6.6

Dependency Updates
* Add dependency on version 4.1.0 or newer of NuGet.exe
* Update NuGet.exe bootstrap URL to https://aka.ms/psget-nugetexe

Build and Code Cleanup Improvements
* Improved error handling in network connectivity tests.

Bug fixes
- Change Update-ModuleManifest so that prefix is not added to CmdletsToExport.
- Change Update-ModuleManifest so that parameters will not reset to default values.
- Specify AllowPrereleseVersions provider option only when AllowPrerelease is specified on the PowerShellGet cmdlets.

## 1.6.5

New features
* Allow Pester/PSReadline installation when signed by non-Microsoft certificate (#258)
  - Whitelist installation of non-Microsoft signed Pester and PSReadline over Microsoft signed Pester and PSReadline.

Build and Code Cleanup Improvements
* Splitting of functions (#229) (Thanks @Benny1007)
  - Moves private functions into respective private folder.
  - Moves public functions as defined in PSModule.psd1 into respective public folder.
  - Removes all functions from PSModule.psm1 file.
  - Dot sources the functions from PSModule.psm1 file.
  - Uses Export-ModuleMember to export the public functions from PSModule.psm1 file.

* Add build step to construct a single .psm1 file (#242) (Thanks @Benny1007)
  - Merged public and private functions into one .psm1 file to increase load time performance.

Bug fixes
- Fix null parameter error caused by MinimumVersion in Publish-PackageUtility (#201)
- Change .ExternalHelp link from PSGet.psm1-help.xml to PSModule-help.xml in PSModule.psm1 file (#215)
- Change Publish-* to allow version comparison instead of string comparison (#219)
- Ensure Get-InstalledScript -RequiredVersion works when versions have a leading 0 (#260)
- Add positional path to Save-Module and Save-Script (#264, #266)
- Ensure that Get-AuthenticodePublisher verifies publisher and that installing or updating a module checks for approprite catalog signature (#272)
- Update HelpInfoURI to 'http://go.microsoft.com/fwlink/?linkid=855963' (#274)


## 1.6.0

New features
* Prerelease Version Support (#185)
  - Implemented prerelease versions functionality in PowerShellGet cmdlets.
  - Enables publishing, discovering, and installing the prerelease versions of modules and scripts from the PowerShell Gallery.
  - [Documentation](https://docs.microsoft.com/en-us/powershell/gallery/psget/module/PrereleaseModule)

* Enabled publish cmdlets on PWSH and Nano Server (#196)
  - Dotnet command version 2.0.0 or newer should be installed by the user prior to using the publish cmdlets on PWSH and Windows Nano Server.
  - Users can install the dotnet command by following the instructions specified at https://aka.ms/dotnet-install-script.
  - On Windows, users can install the dotnet command by running *Invoke-WebRequest -Uri 'https://dot.net/v1/dotnet-install.ps1' -OutFile '.\dotnet-install.ps1'; & '.\dotnet-install.ps1' -Channel Current -Version '2.0.0'*
  - Publish cmdlets on Windows PowerShell supports using the dotnet command for publishing operations.

Breaking Change
- PWSH: Changed the installation location of AllUsers scope to the parent of $PSHOME instead of $PSHOME. It is the SHARED_MODULES folder on PWSH.

Bug fixes
- Update HelpInfoURI to 'https://go.microsoft.com/fwlink/?linkid=855963' (#195)
- Ensure MyDocumentsPSPath path is correct (#179) (Thanks @lwsrbrts)


## 1.5.0.0

New features
* Added support for modules requiring license acceptance (#150)
  - [Documentation](https://docs.microsoft.com/en-us/powershell/gallery/psget/module/RequireLicenseAcceptance)

* Added version for REQUIREDSCRIPTS (#162)
  - Enabled following scenarios for REQUIREDSCRIPTS
    - [1.0] - RequiredVersion
    - [1.0,2.0] - Min and Max Version
    - (,1.0] - Max Version
    - 1.0 - Min Version

Bug fixes
* Fixed empty version value in nuspec (#157)


## 1.1.3.2
* Disabled PowerShellGet Telemetry on PS Core as PowerShell Telemetry APIs got removed in PowerShell Core beta builds. (#153)
* Fixed for DateTime format serialization issue. (#141)
* Update-ModuleManifest should add ExternalModuleDependencies value as a collection. (#129)

## 1.1.3.1

New features
* Added `PrivateData` field to ScriptFileInfo. (#119)

Bug fixes

## 1.1.2.0

Bug fixes

## 1.1.1.0

Bug fixes

## 1.1.0.0

* Initial release from GitHub.
* PowerShellCore support.

## For full history of release notes see changelog:
https://github.com/PowerShell/PowerShellGet/blob/master/CHANGELOG.md
'@
        }
    }

    HelpInfoURI       = 'http://go.microsoft.com/fwlink/?linkid=2113539'
}


# SIG # Begin signature block
# MIIjgwYJKoZIhvcNAQcCoIIjdDCCI3ACAQExDzANBglghkgBZQMEAgEFADB5Bgor
# BgEEAYI3AgEEoGswaTA0BgorBgEEAYI3AgEeMCYCAwEAAAQQH8w7YFlLCE63JNLG
# KX7zUQIBAAIBAAIBAAIBAAIBADAxMA0GCWCGSAFlAwQCAQUABCDo35X1DFIsUHA9
# IY3qr24j5UgRclulr67aKMfMS58uVaCCDYEwggX/MIID56ADAgECAhMzAAABh3IX
# chVZQMcJAAAAAAGHMA0GCSqGSIb3DQEBCwUAMH4xCzAJBgNVBAYTAlVTMRMwEQYD
# VQQIEwpXYXNoaW5ndG9uMRAwDgYDVQQHEwdSZWRtb25kMR4wHAYDVQQKExVNaWNy
# b3NvZnQgQ29ycG9yYXRpb24xKDAmBgNVBAMTH01pY3Jvc29mdCBDb2RlIFNpZ25p
# bmcgUENBIDIwMTEwHhcNMjAwMzA0MTgzOTQ3WhcNMjEwMzAzMTgzOTQ3WjB0MQsw
# CQYDVQQGEwJVUzETMBEGA1UECBMKV2FzaGluZ3RvbjEQMA4GA1UEBxMHUmVkbW9u
# ZDEeMBwGA1UEChMVTWljcm9zb2Z0IENvcnBvcmF0aW9uMR4wHAYDVQQDExVNaWNy
# b3NvZnQgQ29ycG9yYXRpb24wggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIB
# AQDOt8kLc7P3T7MKIhouYHewMFmnq8Ayu7FOhZCQabVwBp2VS4WyB2Qe4TQBT8aB
# znANDEPjHKNdPT8Xz5cNali6XHefS8i/WXtF0vSsP8NEv6mBHuA2p1fw2wB/F0dH
# sJ3GfZ5c0sPJjklsiYqPw59xJ54kM91IOgiO2OUzjNAljPibjCWfH7UzQ1TPHc4d
# weils8GEIrbBRb7IWwiObL12jWT4Yh71NQgvJ9Fn6+UhD9x2uk3dLj84vwt1NuFQ
# itKJxIV0fVsRNR3abQVOLqpDugbr0SzNL6o8xzOHL5OXiGGwg6ekiXA1/2XXY7yV
# Fc39tledDtZjSjNbex1zzwSXAgMBAAGjggF+MIIBejAfBgNVHSUEGDAWBgorBgEE
# AYI3TAgBBggrBgEFBQcDAzAdBgNVHQ4EFgQUhov4ZyO96axkJdMjpzu2zVXOJcsw
# UAYDVR0RBEkwR6RFMEMxKTAnBgNVBAsTIE1pY3Jvc29mdCBPcGVyYXRpb25zIFB1
# ZXJ0byBSaWNvMRYwFAYDVQQFEw0yMzAwMTIrNDU4Mzg1MB8GA1UdIwQYMBaAFEhu
# ZOVQBdOCqhc3NyK1bajKdQKVMFQGA1UdHwRNMEswSaBHoEWGQ2h0dHA6Ly93d3cu
# bWljcm9zb2Z0LmNvbS9wa2lvcHMvY3JsL01pY0NvZFNpZ1BDQTIwMTFfMjAxMS0w
# Ny0wOC5jcmwwYQYIKwYBBQUHAQEEVTBTMFEGCCsGAQUFBzAChkVodHRwOi8vd3d3
# Lm1pY3Jvc29mdC5jb20vcGtpb3BzL2NlcnRzL01pY0NvZFNpZ1BDQTIwMTFfMjAx
# MS0wNy0wOC5jcnQwDAYDVR0TAQH/BAIwADANBgkqhkiG9w0BAQsFAAOCAgEAixmy
# S6E6vprWD9KFNIB9G5zyMuIjZAOuUJ1EK/Vlg6Fb3ZHXjjUwATKIcXbFuFC6Wr4K
# NrU4DY/sBVqmab5AC/je3bpUpjtxpEyqUqtPc30wEg/rO9vmKmqKoLPT37svc2NV
# BmGNl+85qO4fV/w7Cx7J0Bbqk19KcRNdjt6eKoTnTPHBHlVHQIHZpMxacbFOAkJr
# qAVkYZdz7ikNXTxV+GRb36tC4ByMNxE2DF7vFdvaiZP0CVZ5ByJ2gAhXMdK9+usx
# zVk913qKde1OAuWdv+rndqkAIm8fUlRnr4saSCg7cIbUwCCf116wUJ7EuJDg0vHe
# yhnCeHnBbyH3RZkHEi2ofmfgnFISJZDdMAeVZGVOh20Jp50XBzqokpPzeZ6zc1/g
# yILNyiVgE+RPkjnUQshd1f1PMgn3tns2Cz7bJiVUaqEO3n9qRFgy5JuLae6UweGf
# AeOo3dgLZxikKzYs3hDMaEtJq8IP71cX7QXe6lnMmXU/Hdfz2p897Zd+kU+vZvKI
# 3cwLfuVQgK2RZ2z+Kc3K3dRPz2rXycK5XCuRZmvGab/WbrZiC7wJQapgBodltMI5
# GMdFrBg9IeF7/rP4EqVQXeKtevTlZXjpuNhhjuR+2DMt/dWufjXpiW91bo3aH6Ea
# jOALXmoxgltCp1K7hrS6gmsvj94cLRf50QQ4U8Qwggd6MIIFYqADAgECAgphDpDS
# AAAAAAADMA0GCSqGSIb3DQEBCwUAMIGIMQswCQYDVQQGEwJVUzETMBEGA1UECBMK
# V2FzaGluZ3RvbjEQMA4GA1UEBxMHUmVkbW9uZDEeMBwGA1UEChMVTWljcm9zb2Z0
# IENvcnBvcmF0aW9uMTIwMAYDVQQDEylNaWNyb3NvZnQgUm9vdCBDZXJ0aWZpY2F0
# ZSBBdXRob3JpdHkgMjAxMTAeFw0xMTA3MDgyMDU5MDlaFw0yNjA3MDgyMTA5MDla
# MH4xCzAJBgNVBAYTAlVTMRMwEQYDVQQIEwpXYXNoaW5ndG9uMRAwDgYDVQQHEwdS
# ZWRtb25kMR4wHAYDVQQKExVNaWNyb3NvZnQgQ29ycG9yYXRpb24xKDAmBgNVBAMT
# H01pY3Jvc29mdCBDb2RlIFNpZ25pbmcgUENBIDIwMTEwggIiMA0GCSqGSIb3DQEB
# AQUAA4ICDwAwggIKAoICAQCr8PpyEBwurdhuqoIQTTS68rZYIZ9CGypr6VpQqrgG
# OBoESbp/wwwe3TdrxhLYC/A4wpkGsMg51QEUMULTiQ15ZId+lGAkbK+eSZzpaF7S
# 35tTsgosw6/ZqSuuegmv15ZZymAaBelmdugyUiYSL+erCFDPs0S3XdjELgN1q2jz
# y23zOlyhFvRGuuA4ZKxuZDV4pqBjDy3TQJP4494HDdVceaVJKecNvqATd76UPe/7
# 4ytaEB9NViiienLgEjq3SV7Y7e1DkYPZe7J7hhvZPrGMXeiJT4Qa8qEvWeSQOy2u
# M1jFtz7+MtOzAz2xsq+SOH7SnYAs9U5WkSE1JcM5bmR/U7qcD60ZI4TL9LoDho33
# X/DQUr+MlIe8wCF0JV8YKLbMJyg4JZg5SjbPfLGSrhwjp6lm7GEfauEoSZ1fiOIl
# XdMhSz5SxLVXPyQD8NF6Wy/VI+NwXQ9RRnez+ADhvKwCgl/bwBWzvRvUVUvnOaEP
# 6SNJvBi4RHxF5MHDcnrgcuck379GmcXvwhxX24ON7E1JMKerjt/sW5+v/N2wZuLB
# l4F77dbtS+dJKacTKKanfWeA5opieF+yL4TXV5xcv3coKPHtbcMojyyPQDdPweGF
# RInECUzF1KVDL3SV9274eCBYLBNdYJWaPk8zhNqwiBfenk70lrC8RqBsmNLg1oiM
# CwIDAQABo4IB7TCCAekwEAYJKwYBBAGCNxUBBAMCAQAwHQYDVR0OBBYEFEhuZOVQ
# BdOCqhc3NyK1bajKdQKVMBkGCSsGAQQBgjcUAgQMHgoAUwB1AGIAQwBBMAsGA1Ud
# DwQEAwIBhjAPBgNVHRMBAf8EBTADAQH/MB8GA1UdIwQYMBaAFHItOgIxkEO5FAVO
# 4eqnxzHRI4k0MFoGA1UdHwRTMFEwT6BNoEuGSWh0dHA6Ly9jcmwubWljcm9zb2Z0
# LmNvbS9wa2kvY3JsL3Byb2R1Y3RzL01pY1Jvb0NlckF1dDIwMTFfMjAxMV8wM18y
# Mi5jcmwwXgYIKwYBBQUHAQEEUjBQME4GCCsGAQUFBzAChkJodHRwOi8vd3d3Lm1p
# Y3Jvc29mdC5jb20vcGtpL2NlcnRzL01pY1Jvb0NlckF1dDIwMTFfMjAxMV8wM18y
# Mi5jcnQwgZ8GA1UdIASBlzCBlDCBkQYJKwYBBAGCNy4DMIGDMD8GCCsGAQUFBwIB
# FjNodHRwOi8vd3d3Lm1pY3Jvc29mdC5jb20vcGtpb3BzL2RvY3MvcHJpbWFyeWNw
# cy5odG0wQAYIKwYBBQUHAgIwNB4yIB0ATABlAGcAYQBsAF8AcABvAGwAaQBjAHkA
# XwBzAHQAYQB0AGUAbQBlAG4AdAAuIB0wDQYJKoZIhvcNAQELBQADggIBAGfyhqWY
# 4FR5Gi7T2HRnIpsLlhHhY5KZQpZ90nkMkMFlXy4sPvjDctFtg/6+P+gKyju/R6mj
# 82nbY78iNaWXXWWEkH2LRlBV2AySfNIaSxzzPEKLUtCw/WvjPgcuKZvmPRul1LUd
# d5Q54ulkyUQ9eHoj8xN9ppB0g430yyYCRirCihC7pKkFDJvtaPpoLpWgKj8qa1hJ
# Yx8JaW5amJbkg/TAj/NGK978O9C9Ne9uJa7lryft0N3zDq+ZKJeYTQ49C/IIidYf
# wzIY4vDFLc5bnrRJOQrGCsLGra7lstnbFYhRRVg4MnEnGn+x9Cf43iw6IGmYslmJ
# aG5vp7d0w0AFBqYBKig+gj8TTWYLwLNN9eGPfxxvFX1Fp3blQCplo8NdUmKGwx1j
# NpeG39rz+PIWoZon4c2ll9DuXWNB41sHnIc+BncG0QaxdR8UvmFhtfDcxhsEvt9B
# xw4o7t5lL+yX9qFcltgA1qFGvVnzl6UJS0gQmYAf0AApxbGbpT9Fdx41xtKiop96
# eiL6SJUfq/tHI4D1nvi/a7dLl+LrdXga7Oo3mXkYS//WsyNodeav+vyL6wuA6mk7
# r/ww7QRMjt/fdW1jkT3RnVZOT7+AVyKheBEyIXrvQQqxP/uozKRdwaGIm1dxVk5I
# RcBCyZt2WwqASGv9eZ/BvW1taslScxMNelDNMYIVWDCCFVQCAQEwgZUwfjELMAkG
# A1UEBhMCVVMxEzARBgNVBAgTCldhc2hpbmd0b24xEDAOBgNVBAcTB1JlZG1vbmQx
# HjAcBgNVBAoTFU1pY3Jvc29mdCBDb3Jwb3JhdGlvbjEoMCYGA1UEAxMfTWljcm9z
# b2Z0IENvZGUgU2lnbmluZyBQQ0EgMjAxMQITMwAAAYdyF3IVWUDHCQAAAAABhzAN
# BglghkgBZQMEAgEFAKCBrjAZBgkqhkiG9w0BCQMxDAYKKwYBBAGCNwIBBDAcBgor
# BgEEAYI3AgELMQ4wDAYKKwYBBAGCNwIBFTAvBgkqhkiG9w0BCQQxIgQgeNZFb46c
# rEzcl+lZUT1v9/cMMO/aquad3yMAPbo5qmcwQgYKKwYBBAGCNwIBDDE0MDKgFIAS
# AE0AaQBjAHIAbwBzAG8AZgB0oRqAGGh0dHA6Ly93d3cubWljcm9zb2Z0LmNvbTAN
# BgkqhkiG9w0BAQEFAASCAQC2jXx1WO7sehMVpvRz1Jlyns0iahpVwYkcqZ0qf5yY
# vunO5cxdz5mMTOQ80F+VzLlFh+eRoFE5sC4i0V8hp/IMD8S9A2ZaeNVAQWpCnThP
# rF1vRsC9NofI2p+HzhTth0kVXmpXdLPMKGsa7ENcu50Tqvq9j0XNKejZWtlTOgbt
# /0uY+gLQofczqX5rXhzjja7Y3GWpqxj/OpjJz7adv1tSlFeIEkQBgHP6L+rsOKuF
# 8Sc8oxtGwzMfjEREnAI/BSLK9LKSS7eNa17NdSK/kihyLPT9MF9eJi3ZPVItzCnN
# WymKh5Sfy5RAoUOroALGJb3rzTM7EDtXHRrZPootdosDoYIS4jCCEt4GCisGAQQB
# gjcDAwExghLOMIISygYJKoZIhvcNAQcCoIISuzCCErcCAQMxDzANBglghkgBZQME
# AgEFADCCAVEGCyqGSIb3DQEJEAEEoIIBQASCATwwggE4AgEBBgorBgEEAYRZCgMB
# MDEwDQYJYIZIAWUDBAIBBQAEIELg/LjV/6JRKY+h6JIk6MHsOFIJAnDcIZBxg9mb
# RxmHAgZegg5zDW0YEzIwMjAwNDIyMjAyNjQxLjEzNFowBIACAfSggdCkgc0wgcox
# CzAJBgNVBAYTAlVTMQswCQYDVQQIEwJXQTEQMA4GA1UEBxMHUmVkbW9uZDEeMBwG
# A1UEChMVTWljcm9zb2Z0IENvcnBvcmF0aW9uMS0wKwYDVQQLEyRNaWNyb3NvZnQg
# SXJlbGFuZCBPcGVyYXRpb25zIExpbWl0ZWQxJjAkBgNVBAsTHVRoYWxlcyBUU1Mg
# RVNOOjA4NDItNEJFNi1DMjlBMSUwIwYDVQQDExxNaWNyb3NvZnQgVGltZS1TdGFt
# cCBTZXJ2aWNloIIOOTCCBPEwggPZoAMCAQICEzMAAAEJfoK9HnvTYSIAAAAAAQkw
# DQYJKoZIhvcNAQELBQAwfDELMAkGA1UEBhMCVVMxEzARBgNVBAgTCldhc2hpbmd0
# b24xEDAOBgNVBAcTB1JlZG1vbmQxHjAcBgNVBAoTFU1pY3Jvc29mdCBDb3Jwb3Jh
# dGlvbjEmMCQGA1UEAxMdTWljcm9zb2Z0IFRpbWUtU3RhbXAgUENBIDIwMTAwHhcN
# MTkxMDIzMjMxOTE0WhcNMjEwMTIxMjMxOTE0WjCByjELMAkGA1UEBhMCVVMxCzAJ
# BgNVBAgTAldBMRAwDgYDVQQHEwdSZWRtb25kMR4wHAYDVQQKExVNaWNyb3NvZnQg
# Q29ycG9yYXRpb24xLTArBgNVBAsTJE1pY3Jvc29mdCBJcmVsYW5kIE9wZXJhdGlv
# bnMgTGltaXRlZDEmMCQGA1UECxMdVGhhbGVzIFRTUyBFU046MDg0Mi00QkU2LUMy
# OUExJTAjBgNVBAMTHE1pY3Jvc29mdCBUaW1lLVN0YW1wIFNlcnZpY2UwggEiMA0G
# CSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQC4wyEQZQIHGgkuQ/1UnrdT7jela35b
# XCpB9jYSlc+bFiXDs1LLX1Z79nkL4ZUfj+wtOrN7OyEqXV2fgiwdi0uZ/W31ozc6
# OTcY3gF+yGp0ZPTCA463zSdBCSpHpGG6c7XyYXig8cRPQuO7Rv5dFpxpPlDypMty
# 1+OlgFcZUYoMSQabW4QUu87yM3hZ7MTuTLZsuKx7+dDzJxIAbGwecCNSsPd0D2zE
# /WwR+LCInse+4UFrrYYPwJKsPMifO3UvmCF7Ld/rmyLQbGdrR6xwXMmzc4HBBOT5
# wyta6Op0CYdnUensxOJ/qgENw/fNTWPXfggms8DLsOJthTYrG2QkDSr3AgMBAAGj
# ggEbMIIBFzAdBgNVHQ4EFgQUpaSSc0yDQvxCcYjn1KjvNj9uSUYwHwYDVR0jBBgw
# FoAU1WM6XIoxkPNDe3xGG8UzaFqFbVUwVgYDVR0fBE8wTTBLoEmgR4ZFaHR0cDov
# L2NybC5taWNyb3NvZnQuY29tL3BraS9jcmwvcHJvZHVjdHMvTWljVGltU3RhUENB
# XzIwMTAtMDctMDEuY3JsMFoGCCsGAQUFBwEBBE4wTDBKBggrBgEFBQcwAoY+aHR0
# cDovL3d3dy5taWNyb3NvZnQuY29tL3BraS9jZXJ0cy9NaWNUaW1TdGFQQ0FfMjAx
# MC0wNy0wMS5jcnQwDAYDVR0TAQH/BAIwADATBgNVHSUEDDAKBggrBgEFBQcDCDAN
# BgkqhkiG9w0BAQsFAAOCAQEAOFiHA4sHR0uQEq6TTC5G/8luBryoOQ+kFuJU5iXA
# TSXe0BrdSVzlKq3qkE6EHvrcXFgzl1KHLFi2bgsh8JiPlDLHfLmfTkFNxLEHr35M
# FTPwa9J3U4afrCk7aYsYIE0JsiDF3+RY24HHh6Sw0njIQ1K8yH5PC5+evkj+lh5k
# 6mhQf472m8Vc/fLPPtOsdyeczOEw5citXv1zUINJWwHy2m3eQl6ulxA3sgYpAzdm
# +NQtf/oi0yQ6QmkQSmd+rpbgk6tqi1j/iOg0ECRmmK0wtvfaEvjwxU67Ykxwyg18
# 8kRLhAAz6d7/S/FGrq+v07zCVJxxr0ZEoCtaTFl7zJ/qaDCCBnEwggRZoAMCAQIC
# CmEJgSoAAAAAAAIwDQYJKoZIhvcNAQELBQAwgYgxCzAJBgNVBAYTAlVTMRMwEQYD
# VQQIEwpXYXNoaW5ndG9uMRAwDgYDVQQHEwdSZWRtb25kMR4wHAYDVQQKExVNaWNy
# b3NvZnQgQ29ycG9yYXRpb24xMjAwBgNVBAMTKU1pY3Jvc29mdCBSb290IENlcnRp
# ZmljYXRlIEF1dGhvcml0eSAyMDEwMB4XDTEwMDcwMTIxMzY1NVoXDTI1MDcwMTIx
# NDY1NVowfDELMAkGA1UEBhMCVVMxEzARBgNVBAgTCldhc2hpbmd0b24xEDAOBgNV
# BAcTB1JlZG1vbmQxHjAcBgNVBAoTFU1pY3Jvc29mdCBDb3Jwb3JhdGlvbjEmMCQG
# A1UEAxMdTWljcm9zb2Z0IFRpbWUtU3RhbXAgUENBIDIwMTAwggEiMA0GCSqGSIb3
# DQEBAQUAA4IBDwAwggEKAoIBAQCpHQ28dxGKOiDs/BOX9fp/aZRrdFQQ1aUKAIKF
# ++18aEssX8XD5WHCdrc+Zitb8BVTJwQxH0EbGpUdzgkTjnxhMFmxMEQP8WCIhFRD
# DNdNuDgIs0Ldk6zWczBXJoKjRQ3Q6vVHgc2/JGAyWGBG8lhHhjKEHnRhZ5FfgVSx
# z5NMksHEpl3RYRNuKMYa+YaAu99h/EbBJx0kZxJyGiGKr0tkiVBisV39dx898Fd1
# rL2KQk1AUdEPnAY+Z3/1ZsADlkR+79BL/W7lmsqxqPJ6Kgox8NpOBpG2iAg16Hgc
# sOmZzTznL0S6p/TcZL2kAcEgCZN4zfy8wMlEXV4WnAEFTyJNAgMBAAGjggHmMIIB
# 4jAQBgkrBgEEAYI3FQEEAwIBADAdBgNVHQ4EFgQU1WM6XIoxkPNDe3xGG8UzaFqF
# bVUwGQYJKwYBBAGCNxQCBAweCgBTAHUAYgBDAEEwCwYDVR0PBAQDAgGGMA8GA1Ud
# EwEB/wQFMAMBAf8wHwYDVR0jBBgwFoAU1fZWy4/oolxiaNE9lJBb186aGMQwVgYD
# VR0fBE8wTTBLoEmgR4ZFaHR0cDovL2NybC5taWNyb3NvZnQuY29tL3BraS9jcmwv
# cHJvZHVjdHMvTWljUm9vQ2VyQXV0XzIwMTAtMDYtMjMuY3JsMFoGCCsGAQUFBwEB
# BE4wTDBKBggrBgEFBQcwAoY+aHR0cDovL3d3dy5taWNyb3NvZnQuY29tL3BraS9j
# ZXJ0cy9NaWNSb29DZXJBdXRfMjAxMC0wNi0yMy5jcnQwgaAGA1UdIAEB/wSBlTCB
# kjCBjwYJKwYBBAGCNy4DMIGBMD0GCCsGAQUFBwIBFjFodHRwOi8vd3d3Lm1pY3Jv
# c29mdC5jb20vUEtJL2RvY3MvQ1BTL2RlZmF1bHQuaHRtMEAGCCsGAQUFBwICMDQe
# MiAdAEwAZQBnAGEAbABfAFAAbwBsAGkAYwB5AF8AUwB0AGEAdABlAG0AZQBuAHQA
# LiAdMA0GCSqGSIb3DQEBCwUAA4ICAQAH5ohRDeLG4Jg/gXEDPZ2joSFvs+umzPUx
# vs8F4qn++ldtGTCzwsVmyWrf9efweL3HqJ4l4/m87WtUVwgrUYJEEvu5U4zM9GAS
# inbMQEBBm9xcF/9c+V4XNZgkVkt070IQyK+/f8Z/8jd9Wj8c8pl5SpFSAK84Dxf1
# L3mBZdmptWvkx872ynoAb0swRCQiPM/tA6WWj1kpvLb9BOFwnzJKJ/1Vry/+tuWO
# M7tiX5rbV0Dp8c6ZZpCM/2pif93FSguRJuI57BlKcWOdeyFtw5yjojz6f32WapB4
# pm3S4Zz5Hfw42JT0xqUKloakvZ4argRCg7i1gJsiOCC1JeVk7Pf0v35jWSUPei45
# V3aicaoGig+JFrphpxHLmtgOR5qAxdDNp9DvfYPw4TtxCd9ddJgiCGHasFAeb73x
# 4QDf5zEHpJM692VHeOj4qEir995yfmFrb3epgcunCaw5u+zGy9iCtHLNHfS4hQEe
# gPsbiSpUObJb2sgNVZl6h3M7COaYLeqN4DMuEin1wC9UJyH3yKxO2ii4sanblrKn
# QqLJzxlBTeCG+SqaoxFmMNO7dDJL32N79ZmKLxvHIa9Zta7cRDyXUHHXodLFVeNp
# 3lfB0d4wwP3M5k37Db9dT+mdHhk4L7zPWAUu7w2gUDXa7wknHNWzfjUeCLraNtvT
# X4/edIhJEqGCAsswggI0AgEBMIH4oYHQpIHNMIHKMQswCQYDVQQGEwJVUzELMAkG
# A1UECBMCV0ExEDAOBgNVBAcTB1JlZG1vbmQxHjAcBgNVBAoTFU1pY3Jvc29mdCBD
# b3Jwb3JhdGlvbjEtMCsGA1UECxMkTWljcm9zb2Z0IElyZWxhbmQgT3BlcmF0aW9u
# cyBMaW1pdGVkMSYwJAYDVQQLEx1UaGFsZXMgVFNTIEVTTjowODQyLTRCRTYtQzI5
# QTElMCMGA1UEAxMcTWljcm9zb2Z0IFRpbWUtU3RhbXAgU2VydmljZaIjCgEBMAcG
# BSsOAwIaAxUACsG8ux1nIgl0fkctgBa2jzpieACggYMwgYCkfjB8MQswCQYDVQQG
# EwJVUzETMBEGA1UECBMKV2FzaGluZ3RvbjEQMA4GA1UEBxMHUmVkbW9uZDEeMBwG
# A1UEChMVTWljcm9zb2Z0IENvcnBvcmF0aW9uMSYwJAYDVQQDEx1NaWNyb3NvZnQg
# VGltZS1TdGFtcCBQQ0EgMjAxMDANBgkqhkiG9w0BAQUFAAIFAOJK3wkwIhgPMjAy
# MDA0MjIyMzE5MzdaGA8yMDIwMDQyMzIzMTkzN1owdDA6BgorBgEEAYRZCgQBMSww
# KjAKAgUA4krfCQIBADAHAgEAAgIDAjAHAgEAAgIRmjAKAgUA4kwwiQIBADA2Bgor
# BgEEAYRZCgQCMSgwJjAMBgorBgEEAYRZCgMCoAowCAIBAAIDB6EgoQowCAIBAAID
# AYagMA0GCSqGSIb3DQEBBQUAA4GBABfeNkW1Z+mYZxX+YbBcOFel+Y4X2ZGTFVn5
# vRb1jJr6yWN1rb/fJyxfJywCp0hGgidj9ft24oRzG/BjQJmoZ+4jLWN3jlub9l4n
# TABnzLFDvFX5zKViJqWZibECLNT26n7ZkWcztbzNS/uyeCB01zK4b3tExgDqE3Ws
# VeBKZA5HMYIDDTCCAwkCAQEwgZMwfDELMAkGA1UEBhMCVVMxEzARBgNVBAgTCldh
# c2hpbmd0b24xEDAOBgNVBAcTB1JlZG1vbmQxHjAcBgNVBAoTFU1pY3Jvc29mdCBD
# b3Jwb3JhdGlvbjEmMCQGA1UEAxMdTWljcm9zb2Z0IFRpbWUtU3RhbXAgUENBIDIw
# MTACEzMAAAEJfoK9HnvTYSIAAAAAAQkwDQYJYIZIAWUDBAIBBQCgggFKMBoGCSqG
# SIb3DQEJAzENBgsqhkiG9w0BCRABBDAvBgkqhkiG9w0BCQQxIgQgH++mtXOb9id3
# 0JW1bCA/8JOJff5ze7cuAEk5siOjmDEwgfoGCyqGSIb3DQEJEAIvMYHqMIHnMIHk
# MIG9BCCCVPhhBhtKMjxiE2/c3YdDcB3+1eTbswVjXf+epZ1SjzCBmDCBgKR+MHwx
# CzAJBgNVBAYTAlVTMRMwEQYDVQQIEwpXYXNoaW5ndG9uMRAwDgYDVQQHEwdSZWRt
# b25kMR4wHAYDVQQKExVNaWNyb3NvZnQgQ29ycG9yYXRpb24xJjAkBgNVBAMTHU1p
# Y3Jvc29mdCBUaW1lLVN0YW1wIFBDQSAyMDEwAhMzAAABCX6CvR5702EiAAAAAAEJ
# MCIEICtrVv/gtwOkMzJv2s7JpViyc7/VUcGA0KUWhI1R7H/LMA0GCSqGSIb3DQEB
# CwUABIIBAFY34TBAuH7lCRR5pkvxfqbfBeRJ/f3OlTWD8pwWkTpHiYiqlFKoifJ3
# 18WpwUWmq7Utt1nmbLtGJ5/t9+gWJIEXT7BQrfuq1PEgo92z65vJAaBy3gxYq5Yc
# 0Cw6HM0F5FN4pZfWGyIZVeUur/M776fPTWwyRl7gME4hKXEwzlHBxpsyvBXzBeAN
# /kZ0OvwQlFB/zUUMTkw/rOa3/ZvTOnpYyOIDjp7P1v75hT14Jr7bbqQEnFKjn9nI
# 0xMCt3m+QAyK4qU50njYi5MtQ9uSKw7pX74k6DWQ9PirkBk+t/VfoVzJtDdLm8rs
# eNzyMPTRMQf/fhFvyerF/tTG4SMMt78=
# SIG # End signature block
