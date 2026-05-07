# ai-accelerator-seminar

Tool Lending Platform — A full-stack demo application for the AI Accelerator Seminar.

---

## Project Structure Reference

### Tech Stack

| Component | Technology | Version |
|-----------|-----------|---------|
| **Backend** | ASP.NET Core | 9.0 LTS |
| **Database** | PostgreSQL 16 (with pgvector) | 16 |
| **Frontend** | Vanilla HTML/CSS/JS | ES Modules |

### Frontend Stack

| Decision | Choice | Notes |
|----------|--------|-------|
| Framework | Vanilla HTML/CSS/JS | No framework overhead |
| Styling system | CSS Variables (design tokens) | All colors/radii/shadows defined in `design-tokens.css` |
| Language | JavaScript (ES Modules) | No build step required |
| Build tool | None | Static files served directly from `frontend/` |
| Folder structure | `frontend/css/`, `frontend/js/`, `frontend/pages/` | See Frontend Folder Structure below |

### Layers

| Layer | Location | Responsibility |
|-------|----------|-----------------|
| Domain | `backend/Domain/` | Entities, value objects, business invariants, exceptions |
| Application | `backend/Application/` | Use-case services, interfaces, application orchestration |
| Infrastructure | `backend/Infrastructure/` | Database context, migrations, repository implementations |
| API | `backend/Api/` | Controllers, DTOs, dependency injection, middleware |
| Frontend | `frontend/` | HTML, CSS, JavaScript client code |

### Frontend Folder Structure

```
frontend/
├── css/
│   ├── design-tokens.css     # All CSS custom properties (colors, radii, shadows)
│   ├── styles.css            # Global layout utilities and base styles
│   └── components.css        # Shared component styles (atoms, molecules, organisms)
├── js/
│   ├── app.js                # Application initialization
│   ├── auth.js               # AuthService placeholder
│   └── components.js         # Component factory functions + CATEGORY_GRADIENTS
├── pages/
│   ├── login.html            # Login page (filled in TASK-06)
│   └── register.html         # Registration page (filled in TASK-06)
└── index.html                # Main entry point with layout shell
```

### Commands

| Task | Command | Location |
|------|---------|----------|
| **Build backend** | `dotnet build` | From `backend/` directory |
| **Clean backend** | `dotnet clean` | From `backend/` directory |
| **Run backend** | `dotnet run` | From `backend/Api/` directory (port 5000) |
| **Run backend tests** | `dotnet test` | From `backend/` directory |
| **Add DB migration** | `dotnet ef migrations add {MigrationName} --project Infrastructure --startup-project Api` | From `backend/` directory |
| **Apply DB migration** | `dotnet ef database update --project Infrastructure --startup-project Api` | From `backend/` directory |
| **Serve frontend** | Python: `python -m http.server 8000` or Node: `npx http-server -p 8000` | From `frontend/` directory |

### Design System

All UI components and design tokens are defined in:
- **Design Tokens**: `frontend/css/design-tokens.css` (colors, radii, shadows)
- **Global Styles**: `frontend/css/styles.css` (layout utilities, animations)
- **Component Styles**: `frontend/css/components.css` (atoms, molecules, organisms)
- **Component Factories**: `frontend/js/components.js` (vanilla JS functions to render components)
- **Reference**: `docs/DESIGN-SYSTEM-v2.md` (complete component API and rules)

### Shared Components

All UI pages must import and use components from `frontend/css/` and `frontend/js/components.js`. Never rebuild components or hardcode colors/spacing.

**Available Components:**

Atoms: button (7 variants), badge, spinner, section-tag  
Molecules: course-card, feature-card, stat-card, preview-card  
Organisms: navbar, footer, chatbot-fab  
Layouts: public-layout (navbar + footer + FAB shell)  

---

## Running the Project

### 1. Start Backend

```bash
cd backend
dotnet build
dotnet run --project Api
# Backend runs on http://localhost:5000
# Swagger UI: http://localhost:5000/swagger
```

### 2. Start Frontend (in another terminal)

```bash
cd frontend
python -m http.server 8000
# Frontend runs on http://localhost:8000
```

### 3. Access the Application

Open [http://localhost:8000](http://localhost:8000) in your browser.

---

## Design System Rules

All UI implementations must follow these rules:

| Rule | Requirement |
|------|------------|
| Buttons | Always use `--radius-pill` (100px) — no exceptions |
| Cards | Always use `--radius-card` (24px) |
| Section backgrounds | Alternate `bg-bg` and `bg-white` |
| Colors | Use only CSS variables (`var(--color-*)`) — no raw hex values in component CSS |
| Spacing | Use standard 4px scale — no arbitrary pixel values |
| Font | `font-family: system-ui, sans-serif` — no serif fonts |
| Animations | Max `transition-all duration-200` for cards, `duration-150` for buttons |
| Inline styles | Only for dynamic values that cannot be expressed in CSS variables |

---

## Sprint 1: User Registration & Login (UC-1, UC-2)

See `docs/sprint1-authen/PROGRESS.md` for current status and task breakdown.

---
