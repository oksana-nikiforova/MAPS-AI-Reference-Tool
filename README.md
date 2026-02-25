# MAPS-AI-Reference-Tool
MAPS-AI - Model-Driven and AI-Assisted Generation of IT Project Scope and Planning Artefacts
MAPS-AI is a research prototype implementing a hybrid model-driven and generative AI-assisted methodology for early IT project initiation.
The tool transforms business process models into structured and traceable artefacts, including:
- UML use case diagrams
- Agile-ready user stories and product backlogs
- Project scope statements
- Draft IT Project Management Plans

Structural artefacts are generated through deterministic model-to-model transformations, while narrative elements (e.g., acceptance criteria, justification statements) are enriched using controlled generative AI under explicit constraints.

For experimentation purposes, an initial sample business process model file
Process_model_Thesis_selection_initial_short.drawio
is included in the repository.

The prototype demonstrates how formal modelling and AI can be integrated to ensure consistency, traceability, and standards alignment in early IT project phases.

# Local Setup and Deployment (Windows – .NET MAUI)

## Prerequisites

Install the following:

- **Windows 10 (version 19041+) or Windows 11**
- **Visual Studio 2022 or 2026**
  - Workloads:
    - **.NET Multi-platform App UI development**
    - **Desktop development with C++**
- **.NET SDK 9.0**

> Restart your machine after installing workloads if prompted.

---

## Clone the Repository

```bash
git clone https://github.com/oksana-nikiforova/MAPS-AI-Reference-Tool.git
```

Related scientific publications describing the methodology, transformation framework, and prototype validation are listed below:
- Nikiforova O., Grabis J., Pastor O., Babris K., Miļūne M.K., Bobkovs R. Model-based methodology for development of IT project management plan and scope using AI, Proc. RCIS, 2025, https://ceur-ws.org/Vol-3987/paper7.pdf;
- Blaževičs J.R., Nikiforova O., Pastor O. A Framework for Model-Driven AI-Assisted Generation of IT Project Management Plan and Scope Documents, Proc. FedCSIS, 2025, doi:10.15439/2025F8736;
- Miļūne M.K., Nikiforova O., Pastor O. Model Transformation-Based AI-Assisted Identification of Key Artifacts in IT Project Initiation Management, Proc. ITMS, 2025, doi:10.1109/ITMS67030.2025.11236707, IEEE explore;
- Nikiforova O., Bobkovs R., Pastor O. AI-Assisted Transformation of the Two-Hemisphere Model into Structured IT Project Backlog, Proc. ITMS, 2025, doi:10.1109/ITMS67030.2025.11236542, IEEE explore;
- Nikiforova O., Bobkovs R., Miļūne M.K., Babris K., Pastor O., Grabis J. MAPS-AI – A Tool for AI-Assisted Model-Driven Generation of IT Project Plan and Scope, LNCS 16361, 2026, doi:10.1007/978-3-032-12089-2_34.
