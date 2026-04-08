# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

PicoGKMedical is a computational geometry engine for medical and engineering applications, built on top of the PicoGK voxel geometry library. The solution contains four independent projects targeting .NET 10.0.

## Build and Run Commands

```bash
# Build entire solution
dotnet build BoneConstruct.sln

# Build individual projects
dotnet build BoneConstruct/BoneConstruct.csproj
dotnet build MagicCube/MagicCube.csproj
dotnet build MatrixLearning/MatrixLearning.csproj
dotnet build 3DBoxCanvasDesigner/3DBoxCanvasDesigner.csproj

# Run projects
dotnet run --project BoneConstruct/
dotnet run --project MagicCube/
dotnet run --project MatrixLearning/
dotnet run --project 3DBoxCanvasDesigner/
```

## Projects

**BoneConstruct** - Main computational geometry engine with shape construction framework and example applications (e.g., HelixHeatX heat exchanger design).

**MagicCube** - Standalone magic square solver using depth-first search.

**MatrixLearning** - Simple Matrix4x4 operations demo.

**3DBoxCanvasDesigner** - 3D packaging box parametric design system with dieline generation and PDF export.

## Architecture

### ShapeKernel Framework (Leap71.ShapeKernel namespace)

The core geometry engine is organized around a shape construction framework:

**BaseShape Hierarchy:**
- Abstract `BaseShape` base class with `voxConstruct()` method that returns voxel geometry
- Shapes implement interfaces: `ISurfaceBaseShape`, `ISpineBaseShape`, `IMeshBaseShape`, `ILatticeBaseShape`
- Primitive shapes in `BaseShapes/`: Box, Cylinder, Cone, Sphere, Pipe, Ring, Lens, etc.
- All shapes use voxel-based representation via PicoGK integration

**LocalFrame System:**
- `LocalFrame` manages position and local coordinate axes (X, Y, Z vectors)
- Used throughout shapes for spatial transformations: translation, rotation, mirroring
- Enables parametric positioning and orientation of geometry

**Modulation System:**
- `SurfaceModulation` (2D) and `LineModulation` (1D) for parametric shape deformation
- Applied to radius, dimensions, or other parameters during shape construction
- Located in `Modulations/` directory

**Key Subsystems:**
- `Splines/` - Curve and surface interpolation (ControlPointSpline, CylindricalControlSpline)
- `Functions/` - Shape operations (boolean ops, export, lattice generation, voxel functions)
- `Utilities/` - Math helpers (Bisection, GridOperations, MeshUtility, VecOperations)
- `Visualizations/` - Color palettes and mesh painting for geometry preview

### Execution Pattern

BoneConstruct uses PicoGK's execution model:
```csharp
PicoGK.Library.Go(0.5f, Example.Run);
```

The `Example.Run()` method contains the geometry construction logic. PicoGK manages the voxel field resolution and rendering.

## Dependencies

- **BoneConstruct & MagicCube:** PicoGK NuGet package (v1.7.7.4)
- **3DBoxCanvasDesigner:** PicoGK (v1.7.7.4) + PdfSharp (v6.2.0)
- **MatrixLearning:** System.Numerics only

## Code Organization

- Implicit usings enabled (no need for common System namespaces)
- Nullable reference types enabled
- Main namespace: `Leap71.ShapeKernel` for geometry framework
- Application code in `BoneConstruct` namespace
- Box designer code in `BoxCanvasDesigner` namespace

---

## CRITICAL: PicoGK API Usage Guidelines

### Always Check Source Code First

**MANDATORY:** Before using any PicoGK API, consult the source code at:
- `E:\desktop_files\workplace\vscode\Csharp\LEAP71\PicoGK-main`

**DO NOT assume API behavior.** Check:
1. Parameter order and types
2. Return values
3. Method overloads
4. Comments and documentation

### Color and Transparency

**PicoGK uses `ColorFloat` struct, NOT string colors.**

**Correct usage:**
```csharp
// Method 1: 8-digit hex string (RRGGBBAA)
ColorFloat color = new ColorFloat("FF6B3599");  // Orange with 60% opacity (99 = 153/255)

// Method 2: Float components (0.0-1.0)
ColorFloat color = new ColorFloat(1.0f, 0.42f, 0.21f, 0.6f);  // R, G, B, A

// Method 3: Constructor with transparency
ColorFloat baseColor = new ColorFloat("FF6B35");
ColorFloat transparentColor = new ColorFloat(baseColor, 0.6f);

// Apply to viewer
Library.oViewer().SetGroupMaterial(groupId, color, metallic, roughness);
```

**WRONG usage:**
```csharp
// ❌ This is WRONG - string is not accepted by SetGroupMaterial
Library.oViewer().SetGroupMaterial(0, "FF6B35", 0.0f, 1.0f);

// ❌ This is WRONG - 4th parameter is roughness, not transparency
Library.oViewer().SetGroupMaterial(0, color, 0.0f, 0.6f);  // 0.6 is roughness!
```

**Transparency in hex:**
- `FF` = fully opaque (255/255 = 1.0)
- `CC` = 80% opaque (204/255 ≈ 0.8)
- `99` = 60% opaque (153/255 ≈ 0.6)
- `80` = 50% opaque (128/255 = 0.5)
- `00` = fully transparent (0/255 = 0.0)

### SetGroupMaterial Parameters

```csharp
void SetGroupMaterial(
    int groupId,           // Group ID (0, 1, 2, ...)
    ColorFloat color,      // Color with embedded alpha
    float metallic,        // Metallic property (0.0-1.0)
    float roughness        // Surface roughness (0.0-1.0)
)
```

**Reference example from BoneConstruct:**
```csharp
// From ShapeKernel/Visualizations/ShPreviewFunctions.cs
ColorFloat clr = new ColorFloat(clrColor, fTransparency);
Library.oViewer().SetGroupMaterial(iNextGroupId, clr, fMetallic, fRoughness);
```

### PDF Export with PdfSharp 6.x

**MANDATORY:** Before using PdfSharp APIs, consult the source code or documentation:
- GitHub: https://github.com/empira/PDFsharp
- Documentation: https://docs.pdfsharp.net/
- Local NuGet cache: `C:\Users\unerdge\.nuget\packages\pdfsharp\6.2.0\`

**DO NOT assume API behavior.** PdfSharp 6.x has breaking changes from earlier versions:
1. Font handling requires IFontResolver implementation
2. XUnit implicit conversions are obsolete (use .Point, .Millimeter explicitly)
3. PDF standard fonts need XPdfFontOptions(PdfFontEncoding.WinAnsi)

**Font handling:**
- PdfSharp 6.x REQUIRES a FontResolver for all fonts (even built-in PDF fonts)
- Must implement IFontResolver and register via GlobalFontSettings.FontResolver
- Use built-in PDF fonts: `"Helvetica"`, `"Times-Roman"`, `"Courier"`
- DO NOT use `"Arial"` or other system font names without proper font resolver

**Correct usage:**
```csharp
// Register font resolver once at startup
if (GlobalFontSettings.FontResolver == null)
{
    GlobalFontSettings.FontResolver = new PdfFontResolver();
}

// Create fonts with PDF encoding
XFont font = new XFont("Helvetica", 12, XFontStyleEx.Regular, 
    new XPdfFontOptions(PdfFontEncoding.WinAnsi));
XFont boldFont = new XFont("Helvetica", 14, XFontStyleEx.Bold,
    new XPdfFontOptions(PdfFontEncoding.WinAnsi));
```

**WRONG usage:**
```csharp
// ❌ Will throw "No appropriate font found" exception
XFont font = new XFont("Helvetica", 12);  // Missing XPdfFontOptions

// ❌ Will throw NullReferenceException if FontResolver not registered
XFont font = new XFont("Arial", 12);
```

**XUnit usage:**
```csharp
// ✅ Correct - explicit property access
double x = page.Width.Point;
double y = page.Height.Millimeter;

// ❌ Obsolete - implicit conversion
double x = page.Width - 150;  // Warning CS0618
```

### Common Pitfalls

1. **Assuming string colors work** - Always use `ColorFloat`
2. **Confusing roughness with transparency** - Check parameter order
3. **Using system font names in PDF without FontResolver** - Must implement IFontResolver
4. **Not using XPdfFontOptions for PDF fonts** - Required in PdfSharp 6.x
5. **Not checking PicoGK source** - Always verify API before use
6. **Not checking PdfSharp documentation** - API changed significantly in 6.x

### Testing Requirements

After modifying PicoGK-related code:
1. **Compile** - `dotnet build`
2. **Run 3D preview** - Verify visual output
3. **Check console logs** - Look for PicoGK warnings
4. **Test PDF export** - Ensure files generate without errors

---

## 3DBoxCanvasDesigner Specific Notes

### Project Structure
```
3DBoxCanvasDesigner/
├── BoxParameters.cs          # Box parameter model
├── BoxGenerator.cs           # 3D geometry generator (Mesh-based)
├── Dieline/
│   ├── DielineData.cs       # 2D dieline data structures
│   └── DielineGenerator.cs  # Dieline generation logic
└── Export/
    └── PdfExporter.cs       # PDF export (uses Helvetica fonts)
```

### Current Implementation Status
- ✅ 6 box types: TuckEnd, Mailer, CorrugatedRSC, AutoLockBottom, PillowBox, RigidBox
- ✅ Real wall thickness (using ShapeKernel BaseBox + voxOffset)
- ✅ 2D dieline generation for all 6 types
- ✅ PDF export with FontResolver implementation
- ✅ Transparent 3D preview using Sh.PreviewVoxels

### Development Roadmap
See: `ProblemFiles/report/3_开发进度报告.md`

