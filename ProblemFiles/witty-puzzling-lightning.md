# Plan: PicoGK Simulation Applicability Analysis for Box Structural Design

## Context

The user has a product spec for a 3D packaging box design platform that requires physics algorithms (Section 5 of spec):
- McKee formula — corrugated box crush strength (BCT)
- Mullen burst strength
- Stack safety calculations
- Drop impact / G-value cushion protection

The `PicoGK_SimulationExample` repo demonstrates a fluid flow simulation setup using PicoGK voxels + OpenVDB fields. The question is: **can this simulation approach be applied to box structural simulation?**

No code changes. This is a pure analysis deliverable.

---

## Scope

Produce a written analysis covering:

1. What the simulation example actually does
2. Which parts of the box spec's physics requirements could use a similar approach
3. Which parts cannot (and why)
4. Recommended architecture for the physics engine

---

## Analysis

### What PicoGK_SimulationExample Does

The example is a **simulation pre-processor**, not a solver. It:
- Generates voxel geometry (fluid domain, solid domain, inlet patch) using PicoGK
- Extracts surface normals from voxel boundaries → velocity boundary conditions
- Packs geometry + physics fields (density, viscosity, velocity) into a single `.vdb` file
- A separate external solver (e.g. OpenFOAM) would consume that VDB file

It does **not** perform any actual physics computation. It is a geometry-to-solver bridge.

---

### Box Spec Physics Requirements vs. Simulation Approach

#### McKee Formula (BCT) — Section 5.1.1
```
BCT = 5.874 × ECT × CAL^0.508 × P^0.492
```
- This is a **closed-form empirical formula** — no simulation needed
- Inputs: material ECT value, caliper thickness, box perimeter
- Implementation: pure C# arithmetic, ~5 lines
- VDB/voxel approach: **not applicable**, adds zero value

#### Mullen Burst Strength — Section 5.1.3
- Lookup table: material grade → psi rating → max load
- Implementation: dictionary/enum lookup, ~10 lines
- VDB/voxel approach: **not applicable**

#### Stack Safety Factor — Section 5.1.2
```
MaxLayers = BCT / (BoxWeight × SafetyFactor)
```
- Derived from McKee result + safety factor table
- Implementation: arithmetic on McKee output
- VDB/voxel approach: **not applicable**

#### Drop Impact / G-value Cushion — Section 5.2
- Cushion curve lookup: foam type + thickness + static stress → transmitted G
- Implementation: interpolation table lookup
- VDB/voxel approach: **not applicable** for the lookup itself

**However** — if the user later wants to simulate actual stress distribution inside a box wall under compression (FEA-style), the VDB approach becomes relevant:
- Voxelize the box wall geometry (PicoGK already does this)
- Store material stiffness as a scalar field per voxel
- Export to an FEA solver (e.g. CalculiX, FEniCS) via VDB
- This mirrors exactly what the simulation example does for CFD

#### Printing Constraint Engine — Section 5.3
- Rule-based validation (min line width, trapping, CMYK)
- No simulation involved

---

### Verdict

| Physics Requirement | Simulation Approach Applicable? | Recommended Implementation |
|---|---|---|
| McKee BCT formula | No | Inline C# formula |
| Mullen burst lookup | No | Dictionary lookup |
| Stack safety factor | No | Arithmetic on BCT result |
| Cushion curve (G-value) | No | Interpolation table |
| Future: wall stress FEA | **Yes** | VDB export → external FEA solver (same pattern as example) |
| Future: drop simulation | **Yes** | VDB export → explicit dynamics solver |

---

### Recommended Architecture for Phase 2 Physics Engine

The spec's Phase 2 physics requirements are all **rule-based / formula-based**, not simulation-based. The right implementation is:

```
PhysicsEngine/
  McKeeCalculator.cs      — BCT = 5.874 × ECT × CAL^0.508 × P^0.492
  MullenTable.cs          — grade → psi → max weight lookup
  StackingAdvisor.cs      — MaxLayers = BCT / (weight × SF)
  CushionCurveTable.cs    — foam type × thickness × static stress → G
  PrintConstraintChecker.cs — min line width / trapping rules
```

The PicoGK simulation pattern (VDB field export) should be **reserved for a future FEA integration** if the user wants actual stress visualization inside box walls — not for the Phase 2 MVP physics.

---

## Files Referenced

- `E:\desktop_files\workplace\vscode\Csharp\LEAP71\Repositories\PicoGK_SimulationExample-main\...\src\SimpleFluidSimulationOutput.cs` — VDB field export pattern
- `E:\desktop_files\workplace\vscode\Csharp\LEAP71\Repositories\PicoGK_SimulationExample-main\...\src\SimpleFluidSimulationInput.cs` — VDB field import pattern
- `ProblemFiles\3D盒型画布设计器-功能分析与需求规格.txt` — Section 5 (physics algorithms)

---

## Deliverable

This analysis is the output. No code changes are made.
