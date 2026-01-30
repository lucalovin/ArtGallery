#!/bin/bash
# Bash script to remove build artifacts from git tracking
# Run this from the repository root
# Usage: bash cleanup-build-artifacts.sh

echo "🧹 Cleaning up build artifacts from git..."
echo ""

# Remove .dll files
echo "Removing .dll files..."
git ls-files "*.dll" | xargs -r git rm --cached

# Remove .pdb files
echo "Removing .pdb files..."
git ls-files "*.pdb" | xargs -r git rm --cached

# Remove bin directories
echo "Removing bin/ directories..."
git ls-files "**/bin/**" | xargs -r git rm --cached

# Remove obj directories
echo "Removing obj/ directories..."
git ls-files "**/obj/**" | xargs -r git rm --cached

# Remove cache files
echo "Removing .cache files..."
git ls-files "*.cache" | xargs -r git rm --cached

# Remove AssemblyInfo files
echo "Removing .AssemblyInfo.cs files..."
git ls-files "*.AssemblyInfo.cs" | xargs -r git rm --cached

# Remove sourcelink files
echo "Removing .sourcelink.json files..."
git ls-files "*.sourcelink.json" | xargs -r git rm --cached

echo ""
echo "✅ Done! Build artifacts removed from git tracking."
echo ""
echo "📝 Next steps:"
echo "   1. git add .gitignore art-gallery-dw-bi-app-backend/.gitignore"
echo "   2. git commit -m \"Remove build artifacts from tracking\""
echo "   3. git push"
echo ""
echo "ℹ️  Files are still on your disk, just not tracked by git anymore."
