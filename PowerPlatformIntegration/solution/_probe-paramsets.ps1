Import-Module 'C:\PSModules\Rnwood.Dataverse.Data.PowerShell\3.0.3\Rnwood.Dataverse.Data.PowerShell.psd1' -Force
$alias = Get-Alias Connect-DataverseConnection
"Alias resolves to: $($alias.ResolvedCommand.Name)"
$cmd = $alias.ResolvedCommand
foreach ($ps in $cmd.ParameterSets) {
  "== $($ps.Name)"
  foreach ($p in $ps.Parameters) {
    $mand = if ($p.IsMandatory) { ' [mandatory]' } else { '' }
    "   $($p.Name)$mand"
  }
}
