# Quick cleanup script - removes .dll, .pdb, and obj/bin files from git cache
# Run without confirmation prompts

Write-Host "Removing binary files from git tracking..." -ForegroundColor Yellow

# Remove all .dll files from git cache
git ls-files "*.dll" | ForEach-Object { git rm --cached "$_" 2>$null }

# Remove all .pdb files from git cache  
git ls-files "*.pdb" | ForEach-Object { git rm --cached "$_" 2>$null }

# Remove all files in bin/ and obj/ directories
git ls-files "**/bin/**" | ForEach-Object { git rm --cached "$_" 2>$null }
git ls-files "**/obj/**" | ForEach-Object { git rm --cached "$_" 2>$null }

# Remove cache files
git ls-files "*.cache" | ForEach-Object { git rm --cached "$_" 2>$null }

# Remove .AssemblyInfo.cs auto-generated files
git ls-files "*.AssemblyInfo.cs" | ForEach-Object { git rm --cached "$_" 2>$null }

# Remove sourcelink.json files
git ls-files "*.sourcelink.json" | ForEach-Object { git rm --cached "$_" 2>$null }

Write-Host ""
Write-Host "Done! Files removed from git tracking." -ForegroundColor Green
Write-Host "These files are still on disk but won't be tracked by git." -ForegroundColor Cyan
Write-Host ""
Write-Host "Now run:" -ForegroundColor Yellow
Write-Host '  git add .gitignore' -ForegroundColor White
Write-Host '  git commit -m "Remove build artifacts from tracking"' -ForegroundColor White
