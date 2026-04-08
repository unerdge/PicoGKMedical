# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

PicoGKMedical is a computational geometry engine for medical and engineering applications, built on top of the PicoGK voxel geometry library. The solution contains three independent projects targeting .NET 10.0.

## Build and Run Commands

```bash
# Build entire solution
dotnet build BoneConstruct.sln

# Build individual projects
dotnet build BoneConstruct/BoneConstruct.csproj
dotnet build MagicCube/MagicCube.csproj
dotnet build MatrixLearning/MatrixLearning.csproj

# Run projects
dotnet run --project BoneConstruct/
dotnet run --project MagicCube/
dotnet run --project MatrixLearning/
```

## Projects

**BoneConstruct** - Main computational geometry engine with shape construction framework and example applications (e.g., HelixHeatX heat exchanger design).

**MagicCube** - Standalone magic square solver using depth-first search.

**MatrixLearning** - Simple Matrix4x4 operations demo.

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
- **MatrixLearning:** System.Numerics only

## Code Organization

- Implicit usings enabled (no need for common System namespaces)
- Nullable reference types enabled
- Main namespace: `Leap71.ShapeKernel` for geometry framework
- Application code in `BoneConstruct` namespace
