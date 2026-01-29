# PowerShell script to remove .dll and .pdb files from git tracking
# Run this from your git repository root

Write-Host "Cleaning up .dll and .pdb files from git cache..." -ForegroundColor Yellow
Write-Host ""

# First, let's see what files would be affected
Write-Host "Files currently tracked by git (.dll and .pdb):" -ForegroundColor Cyan
git ls-files | Select-String -Pattern "\.(dll|pdb)$"

Write-Host ""
Write-Host "Do you want to remove these files from git tracking? (y/n)" -ForegroundColor Yellow
$response = Read-Host

if ($response -eq 'y' -or $response -eq 'Y') {
    Write-Host ""
    Write-Host "Removing .dll files from git cache..." -ForegroundColor Green
    git ls-files | Select-String -Pattern "\.dll$" | ForEach-Object { 
        $file = $_.Line
        Write-Host "  Removing: $file"
        git rm --cached "$file" 2>&1 | Out-Null
    }
    
    Write-Host "Removing .pdb files from git cache..." -ForegroundColor Green
    git ls-files | Select-String -Pattern "\.pdb$" | ForEach-Object { 
        $file = $_.Line
        Write-Host "  Removing: $file"
        git rm --cached "$file" 2>&1 | Out-Null
    }
    
    Write-Host ""
    Write-Host "Done! These files are now unstaged and will be ignored in future commits." -ForegroundColor Green
    Write-Host "You can now commit these changes with:" -ForegroundColor Cyan
    Write-Host '  git commit -m "Remove .dll and .pdb files from tracking"' -ForegroundColor White
} else {
    Write-Host "Operation cancelled." -ForegroundColor Red
}
