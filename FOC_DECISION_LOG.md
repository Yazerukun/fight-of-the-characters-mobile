# FOC REBORN MOBILE — ARCHITECTURAL DECISION LOG (ADR)
## Formal Record of Strategic Engineering Decisions

**Project:** FOC REBORN MOBILE  
**Lead Architect & Studio Manager:** JIN  
**Last Updated:** 2026-11-11  

---

## ADR INDEX

* **ADR-001:** Adoption of Dual-Identity Architecture & Studio Leadership Protocol
* **ADR-002:** Dual-Engine Strategy (W3AndroidEngine Primary, Unity 6 LTS Preserved Fallback)
* **ADR-003:** Strict Host Storage Redirection to Drive D:
* **ADR-004:** Mathematical Verification of 4 Hero Skeletons over Synthetic Mocks
* **ADR-005:** Zero-Fabrication Reporting & Multi-Tier Evidence Labels

---

### ADR-001: Adoption of Dual-Identity Architecture & Studio Leadership Protocol
* **Date:** 2026-11-11
* **Status:** `ACCEPTED`
* **Context:** The project requires both deep low-level systems engineering (binary parsing, memory profiling, collision matrices, bytecode VMs) and high-level game production leadership (milestone tracking, risk mitigation, asset inventory, cross-tool orchestration).
* **Decision:** JIN operates simultaneously under two unified identities:
  1. **Principal Systems Architect:** Responsible for binary fidelity, memory safety, collision math, zero-floating-point drift, and build stability.
  2. **Lead Game Developer & Studio Manager:** Responsible for roadmap execution, gating criteria, asset pipeline governance, and clear bilingual developer communication.
* **Consequences:** Eliminates passive assistant behavior; ensures proactive issue preemption, military-grade documentation, and uncompromised technical rigor.

---

### ADR-002: Dual-Engine Strategy (W3AndroidEngine Primary, Unity 6 LTS Preserved Fallback)
* **Date:** 2026-11-11
* **Status:** `ACCEPTED`
* **Context:** Rebuilding Fight of Characters from scratch in Unity requires manually recreating 32,000+ lines of custom JASS triggers and complex hero spell logic. Conversely, W3AndroidEngine natively interprets Warcraft III maps, but its stability and full JASS API coverage on Android mobile are unverified.
* **Decision:** Designate **W3AndroidEngine** as the primary engine candidate to be vetted through an 8-gate empirical testing framework. Concurrently, preserve the existing **Unity 6 LTS URP** project at `D:\FIGHT OF THE CHARACTERS\FOC-Reborn-Mobile` (and mirrored on GitHub) as an authoritative fallback and asset baseline.
* **Consequences:** Maximizes upside (potential 10x development acceleration via native map execution) while establishing a zero-risk fail-safe (Unity fallback with verified DAE rigs, materials, and prefabs).

---

### ADR-003: Strict Host Storage Redirection to Drive D:
* **Date:** 2026-11-11
* **Status:** `ACCEPTED`
* **Context:** Host Drive C: has less than 450 MB of available disk space, while Drive D: possesses over 69 GB of free space. Standard Gradle executions default to writing Gigabytes of caches to `%USERPROFILE%\.gradle` on Drive C:, which would instantly crash the host OS.
* **Decision:**
  1. Create a dedicated engine folder on Drive D: (`D:\W3AndroidEngine`).
  2. Redirect all Gradle caches explicitly to `D:\W3AndroidEngine\gradle-cache` via `--gradle-user-home`.
  3. Set build temporary folders to `D:\W3AndroidEngine\downloads`.
  4. Prohibit any tool installation or asset extraction onto Drive C:.
* **Consequences:** Eliminates host disk space exhaustion risks; allows builds and dependency caching to operate smoothly on Drive D:.

---

### ADR-004: Mathematical Verification of 4 Hero Skeletons over Synthetic Mocks
* **Date:** 2026-11-10
* **Status:** `ACCEPTED`
* **Context:** Previous conversion tests utilized mock assertions or generic dummy objects that falsely reported successful rig creation without validating real skeletal deformation.
* **Decision:** Implement a deterministic MDX-to-DAE pipeline that directly reads MDX binary structures (`GEOS`, `BONE`, `PIVT`, `SEQS`, `ATCH`) and mathematically validates:
  - 100% vertex correspondence with Mean Absolute Positional Error ($\text{MAE} \le 0.000034$).
  - Exact bone hierarchies and inverse bind matrix orthogonality ($\text{Error} = 0.0$).
  - Full bone weight normalization ($\sum w = 1.0$).
  - Authentic animation interval slicing without synthetic keyframe extrapolation.
* **Consequences:** Guarantees that hero models (Ichigo, Saber, Rock Lee, Luffy) maintain their authentic Warcraft III identity without visual degradation or rig deformation bugs.

---

### ADR-005: Zero-Fabrication Reporting & Multi-Tier Evidence Labels
* **Date:** 2026-11-11
* **Status:** `ACCEPTED`
* **Context:** To ensure full transparency and auditability by external engineers or peer AI reviewers, all status claims must be backed by concrete proof.
* **Decision:** Enforce mandatory evidence labels on all deliverables:
  - `[VERIFIED BY SOURCE]`: Inspected raw source code or asset file directly.
  - `[VERIFIED BY STATIC TEST]`: Executed static script or mathematical proof with recorded output.
  - `[VERIFIED BY BUILD]`: Successfully compiled via compiler / build system.
  - `[VERIFIED ON DEVICE]`: Executed on physical Android mobile hardware.
  - `[STATIC FILE PRESENT]`: File exists on filesystem but has not been executed at runtime.
  - `[REPORTED BUT UNVERIFIED]`: Claimed in documentation or previous sessions but unproven.
  - `[BLOCKED]`: Prerequisite tool or condition is missing.
  - `[NOT TESTED]`: Feature or gate has not yet been executed.
* **Consequences:** Eliminates false completion claims; provides unambiguous clarity on project reality.
