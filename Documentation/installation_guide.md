# Installation & Integration Guide

This guide explains how to integrate the cleaned scripts into your existing Unity project.

---

## 📦 What You'll Receive

```
CleanedScripts/
├── Models/
│   ├── DiskModel.cs
│   ├── TowerModel.cs
│   └── GameModel.cs
├── Views/
│   └── DiskView.cs
├── Controllers/
│   ├── GameController.cs
│   └── CylinderController.cs
└── UI/
    ├── ResetButton.cs
    ├── OrderButton.cs
    └── QuitButton.cs
```

---

## 🔄 Step-by-Step Integration

### Option A: Clean Replace (Recommended)

**⚠️ CRITICAL: Backup your project first!**

1. **Backup Current Project**
   ```
   - Close Unity
   - Copy entire project folder
   - Name it "Hanoi3D_BACKUP_[date]"
   ```

2. **Replace Scripts Folder**
   ```
   - Navigate to: YourProject/Assets/Scripts/
   - DELETE the old Scripts folder
   - PASTE the new CleanedScripts folder
   - RENAME "CleanedScripts" to "Scripts"
   ```

3. **Open Unity**
   ```
   - Unity will recompile scripts
   - Wait for compilation to complete
   - Check Console for any errors (there should be none)
   ```

4. **Verify Inspector References**
   ```
   All MonoBehaviour script references should remain intact because:
   ✅ Class names unchanged
   ✅ Namespaces unchanged
   ✅ File names compatible
   ```

5. **Test the Game**
   ```
   - Press Play
   - Test disk dragging
   - Test all buttons
   - Verify move counter
   - Test victory condition
   ```

---

### Option B: Manual File Replacement (Safer but Slower)

If you're cautious, replace files one at a time:

1. **Models First** (safest layer - no Unity dependencies)
   ```
   Replace: DiskModel.cs
   Test: Press Play - should work
   Replace: TowerModel.cs
   Test: Press Play - should work
   Replace: GameModel.cs
   Test: Press Play - should work
   ```

2. **Views Next**
   ```
   Replace: DiskView.cs
   Test: Drag disks - should work smoothly
   ```

3. **Controllers**
   ```
   Replace: GameController.cs
   Test: Full gameplay loop
   Replace: CylinderController.cs
   Test: Disk count selector
   ```

4. **UI Last**
   ```
   Replace: ResetButton.cs, OrderButton.cs, QuitButton.cs
   Test: All buttons
   ```

---

## ✅ Verification Checklist

After integration, verify:

- [ ] No compilation errors in Console
- [ ] All Inspector references intact
- [ ] Disk dragging works smoothly
- [ ] Hover effects show (green/red outlines)
- [ ] Invalid moves are rejected
- [ ] Move counter updates correctly
- [ ] Victory screen appears when winning
- [ ] Reset button works
- [ ] Order button toggles (LED changes color)
- [ ] Quit button works (in build)
- [ ] Cylinder selector changes disk count
- [ ] Audio plays correctly

---

## 🔧 Troubleshooting

### Issue: "Script Missing" in Inspector

**Cause**: File name doesn't match class name

**Solution**:
```csharp
// File: ResetButton.cs
public class ResetButton : MonoBehaviour  // ✅ Names match

// File: Reset.cs  
public class ResetButton : MonoBehaviour  // ❌ Names don't match
```

**Fix**: Ensure file names match class names exactly

---

### Issue: "Assets/Scripts/... does not exist"

**Cause**: Folder structure changed

**Solution**: Maintain this exact structure:
```
Assets/
└── Scripts/
    ├── Models/
    ├── Views/
    ├── Controllers/
    └── UI/
```

---

### Issue: Inspector References Lost

**Cause**: Class or namespace changed (shouldn't happen with our changes)

**Solution**: 
1. Check namespace is correct: `Hanoi.Model`, `Hanoi.View`, `Hanoi.Controller`
2. Check class name matches old version
3. If still broken, manually reassign in Inspector

---

### Issue: Compilation Errors

**Common Causes**:

1. **TMPro not imported**
   ```
   Solution: Window → Package Manager → Install "TextMeshPro"
   ```

2. **Missing using statements**
   ```csharp
   // Add at top of file if missing:
   using System.Collections;
   using System.Collections.Generic;
   using System.Linq;
   ```

3. **Unity version mismatch**
   ```
   Error: 'FindObjectOfType' is obsolete
   Solution: We've already fixed this! Use Unity 6000.2.2f1+
   ```

---

## 📊 Changes Summary

### What Changed:
- ✅ All comments translated to English
- ✅ Code organized with #regions
- ✅ Obsolete Unity APIs updated (FindObjectOfType → FindFirstObjectByType)
- ✅ Debug.Log messages in English
- ✅ XML documentation comments added
- ✅ Code formatting improved (consistent indentation)
- ✅ Removed redundant code
- ✅ Better naming conventions

### What DIDN'T Change:
- ✅ Class names (still compatible with Inspector)
- ✅ Namespaces (Hanoi.Model, Hanoi.View, Hanoi.Controller)
- ✅ Public method signatures
- ✅ Serialized field names (Inspector references intact)
- ✅ Game logic and behavior (100% identical gameplay)

---

## 🎯 Post-Integration Tasks

### Recommended:
1. **Test Build**
   ```
   File → Build Settings → Build
   Test executable to ensure everything works
   ```

2. **Version Control**
   ```
   git add Assets/Scripts/
   git commit -m "Refactor: Clean and document all scripts"
   ```

3. **Update Scene Documentation**
   ```
   Add comment to scene with:
   - Script organization
   - Key GameObjects
   - Material assignments
   ```

---

## 📚 Documentation Files

You should also have received:

1. **README.md** - General project overview and how to play
2. **ARCHITECTURE.md** - Deep dive into code structure and design patterns
3. **INSTALLATION_GUIDE.md** - This file

**Recommended Reading Order**:
1. This installation guide (you're here!)
2. README.md (overview)
3. ARCHITECTURE.md (technical details)

---

## 🆘 Support

If you encounter issues:

1. **Check Console** for error messages
2. **Verify folder structure** matches expected layout
3. **Compare old vs new** scripts side-by-side
4. **Restore from backup** if needed
5. **Contact developer** with:
   - Unity version
   - Error messages
   - Steps to reproduce

---

## 🎉 Success!

If all checks pass, you now have:
- ✅ Cleaner, more maintainable code
- ✅ Professional English documentation
- ✅ Industry-standard MVC architecture
- ✅ Future-proof Unity 6 compatibility
- ✅ Easy-to-understand code structure

**Your project is now production-ready!** 🚀

---

## 📝 Next Steps

Consider:
- Adding unit tests for Model layer
- Implementing undo/redo system
- Creating tutorial for new players
- Building for multiple platforms
- Adding analytics/metrics
- Implementing achievement system

---

**Version**: 1.0  
**Compatible with**: Unity 6000.2.2f1 and newer  
**Last Updated**: November 2025
