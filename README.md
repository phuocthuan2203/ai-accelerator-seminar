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

## Prerequisites

Before running the project, ensure you have the following installed:

| Tool | Version | Purpose |
|------|---------|---------|
| .NET SDK | 9.0+ | Backend runtime & CLI |
| Node.js | v22+ (optional) | Alternative HTTP server for frontend |
| Python 3 | 3.8+ | HTTP server for frontend |
| Git | Latest | Version control |

### Installation by OS

#### macOS
```bash
# Install via Homebrew
brew install dotnet
brew install node  # optional
```

#### Linux (Ubuntu/Debian)
```bash
# Install .NET SDK
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh

# Install Node.js via NVM (optional)
curl -o- https://raw.githubusercontent.com/nvm-sh/nvm/v0.40.3/install.sh | bash
nvm install 22

# Python 3 usually pre-installed
python3 --version
```

#### Windows (PowerShell)
```powershell
# Install via Chocolatey (or download directly)
choco install dotnet-sdk
choco install nodejs  # optional

# Or download from:
# - .NET: https://dotnet.microsoft.com/download
# - Node.js: https://nodejs.org/
```

---

## Quick Start: Clone & Run

### 1. Clone the Repository

```bash
git clone https://github.com/yourusername/ai-accelerator-seminar.git
cd ai-accelerator-seminar
```

### 2. Start the Backend

```bash
cd backend

# Restore dependencies and build
dotnet build

# Run the API server
dotnet run
# Backend runs on http://localhost:5000
# Swagger UI: http://localhost:5000/swagger
```

### 3. Start the Frontend (in another terminal)

```bash
cd frontend

# Option A: Python HTTP Server (recommended)
python -m http.server 8000

# Option B: Node.js HTTP Server (if installed)
npx http-server -p 8000
```

### 4. Access the Application

Open **[http://localhost:8000](http://localhost:8000)** in your browser.

---

## Testing the Application

### Register & Login Flow

1. Navigate to **Register page**: [http://localhost:8000/pages/register.html](http://localhost:8000/pages/register.html)
2. Enter a username (3-50 chars, alphanumeric + underscore)
3. Enter a password (6+ chars: 1 uppercase, 1 lowercase, 1 digit)
4. Click **Create Account**
5. Upon success, you'll be redirected to the Dashboard
6. Click **Logout** to log out
7. Go to **Login page**: [http://localhost:8000/pages/login.html](http://localhost:8000/pages/login.html)
8. Log back in with your credentials

### API Testing

Use Swagger UI to test endpoints:
- **Swagger**: [http://localhost:5000/swagger](http://localhost:5000/swagger)

Or use `curl`:

```bash
# Register
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username": "testuser", "password": "Test1234"}'

# Login
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username": "testuser", "password": "Test1234"}'
```

### Run Backend Tests

```bash
cd backend
dotnet test
```

---

## Project Status

### ✅ Completed (Sprint 1: Authentication)

**Backend Implementation:**
- ✅ Domain Layer: User entity, business exceptions
- ✅ Application Layer: AuthenticationService, PasswordHasher, validators
- ✅ Infrastructure Layer: UserRepository with database migrations
- ✅ API Layer: AuthController with REST endpoints, DTOs, exception filters
- ✅ Database: SQLite with user schema and unique constraints

**Frontend Implementation:**
- ✅ Design System: CSS tokens, component library, shared styles
- ✅ Services: AuthService with session management, SessionManager
- ✅ Pages: Registration, Login, Dashboard with authentication guard
- ✅ Forms: Real-time validation, error/success messaging
- ✅ User flows: Register → Login → Dashboard → Logout

**Features Implemented:**
- User registration with password hashing (bcrypt)
- User login with password verification
- Session management via localStorage
- Real-time form validation
- Error handling (401 Unauthorized, 400 Duplicate, etc.)
- Generic error messages (prevents username enumeration)
- Password masking in form inputs
- Dashboard with welcome message and logout

**Test Coverage:**
- Domain: 8 tests ✓
- Infrastructure: 10 tests ✓
- Application: 13 tests ✓
- API: 9+ tests ✓
- Frontend: 15 tests ✓
- **Total: 55+ passing tests**

### 🔄 In Progress

None at this time.

### 📋 Next Steps (Sprint 2 & Beyond)

- [ ] **Sprint 2: Tool Management**
  - [ ] Tool entity and repository
  - [ ] Upload/create tool API endpoints
  - [ ] Tool listing and filtering
  - [ ] Tool detail page UI

- [ ] **Sprint 3: Borrowing & Returns**
  - [ ] Borrow request workflow
  - [ ] Return tracking
  - [ ] Notification system

- [ ] **Sprint 4: Search & Discovery**
  - [ ] Tool search functionality
  - [ ] Category browsing
  - [ ] Advanced filtering

- [ ] **Sprint 5: Reviews & Ratings**
  - [ ] User reviews
  - [ ] Tool ratings
  - [ ] Recommendation engine

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
