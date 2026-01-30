# Git Repository Cleanup Instructions

## Problem
Your git repository is currently tracking build artifacts (.dll files, bin/obj folders) that should be ignored.

## Solution

### Step 1: Run the Quick Cleanup Script (PowerShell)

Open PowerShell in the **art-gallery-dw-bi-app-backend** directory and run:

```powershell
cd art-gallery-dw-bi-app-backend
.\quick-cleanup.ps1
```

This will remove all .dll, .pdb, and obj/bin files from git tracking.

### Step 2: Commit the Changes

```bash
git add .gitignore
git add art-gallery-dw-bi-app-backend/.gitignore
git commit -m "Remove build artifacts from git tracking and update .gitignore"
```

### Step 3: Verify It Worked

Check that no .dll files are staged:

```bash
git status
```

You should see only the .gitignore files and the removal of the binary files.

### Step 4: Push to Remote (if needed)

```bash
git push origin main
```

---

## Alternative: Manual Cleanup (if script doesn't work)

If the PowerShell script doesn't work, you can manually run these commands:

```bash
# Navigate to repository root
cd c:\_dev\vuejs\project\art-gallery-dw-bi-app

# Remove .dll files
git rm --cached -r **/*.dll

# Remove .pdb files
git rm --cached -r **/*.pdb

# Remove bin/obj directories
git rm --cached -r **/bin/**
git rm --cached -r **/obj/**

# Remove cache files
git rm --cached -r **/*.cache

# Remove auto-generated AssemblyInfo files
git rm --cached -r **/*.AssemblyInfo.cs

# Remove sourcelink files
git rm --cached -r **/*.sourcelink.json

# Commit the changes
git add .gitignore art-gallery-dw-bi-app-backend/.gitignore
git commit -m "Remove build artifacts from git tracking"
```

---

## What These Commands Do

- `git rm --cached` - Removes files from git tracking but **keeps them on your disk**
- The files will still exist locally, but won't be committed anymore
- Future builds will regenerate these files, and they'll be ignored by git

---

## Verification

After cleanup, run:

```bash
git ls-files | grep "\.dll$"
git ls-files | grep "\.pdb$"
```

Both commands should return empty results (no .dll or .pdb files tracked).

---

## Important Notes

1. **Don't delete the .gitignore files** - They're correctly configured
2. **The files stay on your disk** - Only removed from git tracking
3. **Future commits won't include these files** - They're now properly ignored
4. **All team members should run this** - If you're working with a team
