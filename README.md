# PicoGKMedical

**Intelligent Software for Personalized Artificial Bone Design Based on PicoGK**

A computational geometry platform for medical device design, enabling personalized orthopedic implant design from patient CT/MRI scans. Built on the PicoGK implicit geometry modeling kernel from Leap71.

## Project Overview

PicoGKMedical is an innovation and entrepreneurship training project by students from Xidian University. The platform bridges the gap between medical imaging and personalized device manufacturing by automating the generation of biomechanically optimized, patient-specific bone implants and scaffolds.

**Key Innovation:** Convert medical imaging data → patient-specific geometric models → manufacturable designs in hours (vs. weeks of manual modeling)

### Team

- **Project Lead:** 雍能杰 (Yong Nengjie)
- **Team Members:** 华城, 王墨林, 张珂溢, 赵子祺
- **Advisor:** 任爱锋 (Ren Aifeng)
- **Organization:** Xidian University (西安电子科技大学) School of Innovation & Entrepreneurship
- **Project Duration:** Nov 2025 - Jun 2027

### Clinical & Market Value

- **Clinical Impact:** Precision bone matching, biomorphic porous structures, optimized load transfer
- **Market Opportunity:** Global orthopedic market (billion-dollar scale), shift toward personalized 3D-printed implants
- **Technical Edge:** Automated design reduces specialist bottlenecks; enables real-time iteration with surgeons
- **Partnerships:** Collaboration with Heiyan Medical (黑焰医疗) for manufacturing validation and hospital integration

## Projects in This Solution

### 1. **BoneConstruct** (Core Platform)
Main computational geometry engine for orthopedic implant design. Loads patient bone geometry from STL files (CT reconstructions), applies biomechanical optimization, generates porous lattice structures, and exports manufacturing-ready models.

**Key Features:**
- CT-to-implant workflow automation
- Biomechanical constraint satisfaction
- Multi-scale porous structure design (bone-compatible pore sizes)
- Real-time 3D preview with PicoGK
- STL import/export

### 2. **3DBoxCanvasDesigner** (Secondary Platform)
Parametric 3D packaging box design system with dieline generation and PDF export. Demonstrates extensibility of the geometric framework to non-medical domains (industrial packaging, logistics).

**Key Features:**
- 6 box types: TuckEnd, Mailer, CorrugatedRSC, AutoLockBottom, PillowBox, RigidBox
- Real wall thickness simulation
- 2D dieline generation for manufacturing
- PDF export with manufacturing annotations

### 3. **FluidSimulation** (Experimental)
Aerodynamic simulation module exploring fluid flow geometry preprocessing. Supports future integration with CFD solvers (e.g., OpenFOAM) for specialized engineering applications.

**Key Features:**
- Voxel-based fluid domain geometry
- OpenVDB field export
- Boundary condition extraction

### 4. **Leap71_Repo_Display** (Showcase)
Demonstration and repository showcase application. Displays geometric models and project repositories for collaborative reference.

## Getting Started

### Requirements

- **.NET 10.0** runtime
- **PicoGK v1.7.7.4** NuGet package (BoneConstruct, FluidSimulation)
- **PdfSharp v6.2.0** (3DBoxCanvasDesigner)
- **Avalonia UI framework** (3DBoxCanvasDesigner GUI)

### Build & Run

```bash
# Build entire solution
dotnet build BoneConstruct.sln

# Build individual projects
dotnet build BoneConstruct/BoneConstruct.csproj
dotnet build 3DBoxCanvasDesigner/3DBoxCanvasDesigner.csproj
dotnet build FluidSimulation/FluidSimulation.csproj
dotnet build Leap71_Repo_Display/Leap71_Repo_Display.csproj

# Run projects
dotnet run --project BoneConstruct/
dotnet run --project 3DBoxCanvasDesigner/        # Launches GUI
dotnet run --project FluidSimulation/
dotnet run --project Leap71_Repo_Display/
```

### Example: BoneConstruct Workflow

1. Place a patient bone STL file in `BoneConstruct/BoneSTL/`
2. Update the file path in `Program.cs`
3. Run the application
4. PicoGK displays the bone in 3D, applies design algorithms
5. Output implant STL saved to `Output/BoneImplant.stl`
6. Send to manufacturing (3D printing, milling, etc.)

## Architecture

### ShapeKernel Framework (Leap71.ShapeKernel namespace)

Core geometry framework used across all projects:

**BaseShape Hierarchy:**
- Abstract `BaseShape` with `voxConstruct()` method returning voxel geometry
- Interfaces: `ISurfaceBaseShape`, `ISpineBaseShape`, `IMeshBaseShape`, `ILatticeBaseShape`
- Primitives: Box, Cylinder, Cone, Sphere, Pipe, Ring, Lens, etc.

**LocalFrame System:**
- Manages 3D position and local coordinate axes (X, Y, Z)
- Enables parametric transformations, rotations, mirroring

**Modulation System:**
- `SurfaceModulation` (2D) and `LineModulation` (1D) for parametric deformation
- Applies to radius, thickness, density profiles during construction

**Key Subsystems:**
- `Splines/` - Curve/surface interpolation (ControlPointSpline, CylindricalControlSpline)
- `Functions/` - Boolean operations, lattice generation, voxel operations
- `Utilities/` - Math helpers (Bisection, GridOperations, MeshUtility, VecOperations)
- `Visualizations/` - Color palettes and mesh coloring for 3D preview

### Execution Model

```csharp
// PicoGK initialization with resolution and main function
PicoGK.Library.Go(0.3f, () => Run());

// Inside Run():
// 1. Load STL or construct geometry
// 2. Apply biological/mechanical constraints
// 3. Generate voxel field via ShapeKernel shapes
// 4. PicoGK renders 3D preview
// 5. Export result (STL, VDB, etc.)
```

## Code Organization

- **Implicit usings enabled** - No need for common `System` namespaces
- **Nullable reference types enabled** - Strict null-safety
- **Main namespace:** `Leap71.ShapeKernel` for geometry framework
- **Application namespace:** `BoneConstruct` for medical device logic
- **Box designer namespace:** `BoxCanvasDesigner` for packaging platform

## Development & Testing

See `CLAUDE.md` for:
- **Critical API guidelines** for PicoGK and PdfSharp usage
- **Color/transparency** handling in 3D previews
- **PDF font setup** requirements (PdfSharp 6.x)
- **Testing checklist** after code modifications

## Project Status & Roadmap

**Current Phase:** V1.0 MVP Development (medical implant design core)

**Completed:**
- ✅ ShapeKernel parametric framework
- ✅ BoneConstruct STL import pipeline
- ✅ Basic implant design algorithms
- ✅ 3D preview with PicoGK
- ✅ 3DBoxCanvasDesigner platform launch

**In Progress:**
- Biomechanical constraint refinement (bone stress adaptation)
- Multi-material design support
- Hospital trial integration with Heiyan Medical

**Planned (Phase 2):**
- Porous structure optimization (surface area, load distribution)
- Real-time surgical planning UI
- FDA/CFDA regulatory documentation support
- Integration with CAD/CAM manufacturing pipelines

## Contact & Collaboration

For questions, collaboration requests, or hospital partnerships:
- **Project Lead:** 雍能杰 (2468910580@qq.com, +86 18281975038)
- **Organization:** Xidian University Innovation & Entrepreneurship Program
- **Partners:** Heiyan Medical (黑焰医疗)

## License & Acknowledgments

Built on:
- **PicoGK** - Implicit geometry kernel by Leap71 (https://github.com/leap71/PicoGK)
- **Avalonia** - Cross-platform UI framework
- **PdfSharp** - PDF generation library

This project is part of the College Student Innovation and Entrepreneurship Training Program (大学生创新创业训练计划) at Xidian University.

---

**Last Updated:** 2026/04/14
