# function replaceVariables($inputFile, $findString, $replaceString){
#     (Get-Content $inputFile) | foreach {$_.replace($findString,$replaceString)}
# }

# replaceVariables("C:\inetpub\wwwroot\ir.local\App_Config\Include\zzz\zz.IRD.Foundation\IRD.Foundation.Xdb.Enable.config", '#{XDB-Enable}', 'false');

$inputFile = 'C:\inetpub\wwwroot\ird930sc.dev.local\App_Config\Include\zzz\zz.IRD.Foundation\IRD.Foundation.Xdb.Enable.config';
$findString = '#{XDB-Enable}';
$replaceString = 'false';
(Get-Content $inputFile) | foreach {$_.replace($findString,$replaceString)}

###### TODO: Make this part of build.ps1 or turn Off directly in file for standalone.