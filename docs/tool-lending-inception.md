# Inception Sprint – Analysis & Design Document
## Tool Lending Platform

---

## 0. Overview

This document is the output of **Iteration 0 – Inception Sprint** for the Tool Lending Platform. It contains **global analysis & design artifacts** (no code) and serves as the architectural "map" for future implementation sprints.

**System**: Tool Lending Platform  
**Problem**: Enable neighbors to lend tools to each other, reducing costs and fostering community.  
**Architectural Style**: **Layered Architecture** – clear separation of concerns (Presentation → API → Application → Domain → Infrastructure) with SQLite for data persistence.

---

## 1. Assumptions & Open Issues

### Assumptions

- **30-minute demo scope**: Proto is feature-complete but not production-ready (no performance tuning, minimal error handling).
- **Single-tenant**: No multi-organization support; one neighborhood/region per deployment.
- **No background jobs**: All operations (approval, notifications) are synchronous request-response.
- **Resource-level authorization**: Users can only approve/deny/edit their own tools; no admin role for prototype.
- **Minimal state tracking**: BorrowRequest has three states: PendingApproval, Approved, Denied. No explicit "Returned" state tracked.
- **SQLite is sufficient**: No need for Redis, message queues, or advanced caching for ~50 concurrent users.
- **Image handling**: Simple file uploads; stored in local filesystem or embedded as base64 (no CDN/blob storage).
- **No notifications**: Changes to request status are visible on dashboard refresh; no email/SMS alerts for prototype.

### Open Issues / Decisions for Later

- **Image persistence strategy**: Will images be stored in filesystem, database, or cloud storage post-prototype?
- **Concurrency handling**: How to handle race conditions if two users approve the last quantity simultaneously?
- **Tool return workflow**: Should prototype track when/if borrowed tools are returned, or assume implicit return?
- **Search & filtering**: Browse tools page – should support filtering by tool name, owner, or both? (Not specified in SRS.)
- **Session management**: Session timeout and token refresh strategy (not detailed in SRS §5.2).

---

## 2. Actor Identification

### 2.1 Stakeholders (Non-Runtime)

| Stakeholder | Interest / Concern |
|---|---|
| University / Instructors | Evaluate architecture completeness, real-world applicability for 30-min demo |
| Development Team | Clear architecture and design to implement within constraints |
| Future Maintainers | Understanding of design decisions and extension points |

### 2.2 Human Actors

| Actor | Role Description | Primary Goals |
|---|---|---|
| **Neighbor (Tool Owner)** | Authenticated user who owns and lends tools | Upload tools; approve/deny borrow requests; track lending inventory |
| **Neighbor (Borrower)** | Authenticated user who borrows tools from others | Browse available tools; request to borrow; view borrowed tools |
| **User (Dual Role)** | Any user can be both owner and borrower | Manage both lending and borrowing activities |

### 2.3 System Actors (External & Internal Systems)

| Actor / System | Type | Interaction Description |
|---|---|---|
| Web Browser | External | User interface; HTTP requests to ASP.NET Core backend |
| SQLite Database | Internal | Persistent data storage; queried by Application Layer |

### System Context

```
┌─────────────────────────────────────────────────────────┐
│                   Tool Lending Platform                 │
│  ┌─────────────────────────────────────────────────────┤
│  │ Presentation Layer (HTML/CSS/JS)                    │
│  ├─────────────────────────────────────────────────────┤
│  │ API Layer (ASP.NET Core Controllers)                │
│  ├─────────────────────────────────────────────────────┤
│  │ Application Layer (Use Case Handlers)               │
│  ├─────────────────────────────────────────────────────┤
│  │ Domain Layer (Business Logic)                       │
│  ├─────────────────────────────────────────────────────┤
│  │ Infrastructure Layer (Repositories, DB Adapters)    │
│  └─────────────────────────────────────────────────────┘
         │                                    │
         │                                    │
      ▼ ▲                                  ▼ ▲
   Neighbor Users (Browsers)          SQLite DB
      (50 concurrent)                 (Local File)
```

---

## 3. Domain Model – Conceptual

### 3.1 Domain Entities & Relationships

| Entity | Description |
|---|---|
| **User** | Represents a person using the platform; can own and borrow tools |
| **Tool** | Represents an item that a user owns and is willing to lend to others |
| **BorrowRequest** | Represents a transaction/agreement: a borrower requests to use a tool from an owner |

### 3.2 Relationship Overview

```
User 1 ──── * Tool
     (as owner)

User 1 ──── * BorrowRequest
     (as borrower)

Tool 1 ──── * BorrowRequest
     (owns)

User 1 ──── * BorrowRequest
     (as owner/approver, via Tool ownership)
```

### 3.3 Conceptual Class Diagram (PlantUML)

```
┌───────────────────────────────────────┐
│              User                     │
├───────────────────────────────────────┤
│ - userId: ID                          │
│ - username: string                    │
│ - password: hashed(string)            │
│ - createdAt: timestamp                │
├───────────────────────────────────────┤
│ + register(username, password)        │
│ + login(username, password): Token    │
│ + createTool(name, qty, image)        │
│ + approveBorrowRequest(requestId)     │
│ + denyBorrowRequest(requestId)        │
│ + requestBorrowTool(toolId)           │
└───────────────────────────────────────┘
           │ owns             │ borrows
           │                  │
    1      │ *          1     │ *
┌──────────────────┐   ┌──────────────────┐
│      Tool        │   │ BorrowRequest    │
├──────────────────┤   ├──────────────────┤
│ - toolId: ID     │   │ - reqId: ID      │
│ - ownerId: ID    │   │ - toolId: ID(FK) │
│ - name: string   │   │ - borrowerId: ID │
│ - qty: int       │   │ - status: enum   │
│ - imageUrl: url  │   │ - reqDate: ts    │
│ - createdAt: ts  │   │ - apprvDate: ts  │
├──────────────────┤   └──────────────────┘
│ + edit()         │
│ + delete()       │
│ + decreaseQty()  │
└──────────────────┘
```

---

## 4. BCE Class Identification

### 4.1 BCE Overview

- **Boundary**: Web UI pages and REST API endpoints where actors interact with the system.
- **Control**: Application services that orchestrate use-case flows (no business logic; delegates to domain/entities).
- **Entity**: Domain objects aligned with the conceptual model (User, Tool, BorrowRequest).

### 4.2 BCE Matrix by Use Case

#### UC-1: User Registration

**Boundary**
- RegisterationPage (HTML form)
- POST /api/auth/register

**Control**
- AuthenticationService

**Entity**
- User

---

#### UC-2: User Login

**Boundary**
- LoginPage (HTML form)
- POST /api/auth/login

**Control**
- AuthenticationService

**Entity**
- User

---

#### UC-3: Upload Tool

**Boundary**
- UploadToolPage (HTML form with file input)
- POST /api/tools

**Control**
- ToolService

**Entity**
- Tool
- User (as owner)

---

#### UC-4: Browse & Request Borrow

**Boundary**
- BrowseToolsPage (HTML list/grid)
- GET /api/tools
- POST /api/tools/{id}/borrow-requests

**Control**
- ToolService
- BorrowRequestService

**Entity**
- Tool
- BorrowRequest
- User

---

#### UC-5: Approve/Deny Borrow Request

**Boundary**
- OwnerDashboardPage (request list + actions)
- PATCH /api/borrow-requests/{id}/approve
- PATCH /api/borrow-requests/{id}/deny

**Control**
- BorrowRequestService

**Entity**
- BorrowRequest
- Tool
- User

---

#### UC-6: View Owned Tools Dashboard

**Boundary**
- OwnerDashboardPage (inventory + pending requests)
- GET /api/tools/owned
- GET /api/borrow-requests/pending

**Control**
- ToolService
- BorrowRequestService

**Entity**
- Tool
- BorrowRequest

---

#### UC-7: View Borrowed Tools Dashboard

**Boundary**
- BorrowerDashboardPage (borrowed items list)
- GET /api/borrow-requests/approved

**Control**
- BorrowRequestService

**Entity**
- BorrowRequest
- Tool

---

### 4.3 Global BCE Catalog

#### Boundary Classes / Endpoints

| Class / Endpoint | Responsibility |
|---|---|
| **RegisterationPage** | HTML form for user registration |
| **LoginPage** | HTML form for user login |
| **UploadToolPage** | HTML form to upload tool (name, quantity, image) |
| **BrowseToolsPage** | HTML list of available tools; request-to-borrow button |
| **OwnerDashboardPage** | Show owned tools inventory + pending borrow requests with approve/deny actions |
| **BorrowerDashboardPage** | Show approved borrow requests (borrowed tools) |
| **POST /api/auth/register** | Endpoint to create user account |
| **POST /api/auth/login** | Endpoint to authenticate user |
| **POST /api/tools** | Endpoint to upload a tool |
| **GET /api/tools** | Endpoint to list available tools (qty > 0, not owned by user) |
| **GET /api/tools/owned** | Endpoint to list user's own tools |
| **POST /api/tools/{id}/borrow-requests** | Endpoint to request to borrow a tool |
| **GET /api/borrow-requests/pending** | Endpoint to list pending requests for user's tools |
| **GET /api/borrow-requests/approved** | Endpoint to list user's approved borrows |
| **PATCH /api/borrow-requests/{id}/approve** | Endpoint to approve a borrow request |
| **PATCH /api/borrow-requests/{id}/deny** | Endpoint to deny a borrow request |

#### Control Classes

| Class | Responsibility |
|---|---|
| **AuthenticationService** | Register, login, password hashing, session management |
| **ToolService** | Create, read, edit, delete tools; query available tools; decrease quantity |
| **BorrowRequestService** | Create borrow request; approve/deny; list pending/approved requests |

#### Entity Classes

| Class | Responsibility |
|---|---|
| **User** | Domain entity representing a user; handles password hashing, authentication state |
| **Tool** | Domain entity representing a tool; tracks owner, name, quantity, image |
| **BorrowRequest** | Domain entity representing a borrow transaction; tracks status lifecycle |

---

## 5. State Machine Diagrams

### 5.1 BorrowRequest State Machine

#### Transition Table

| Current State | Trigger | Guard / Condition | Next State | Action / Side Effects |
|---|---|---|---|---|
| **PendingApproval** | Owner clicks "Approve" | Tool qty > 0 | **Approved** | Decrease tool qty by 1; add to borrower's approved list |
| **PendingApproval** | Owner clicks "Deny" | (none) | **Denied** | Remove from pending; notify borrower (via UI) |
| **PendingApproval** | Borrower cancels request | (none) | **Cancelled** | Remove request (optional for proto) |
| **Approved** | (end state for proto) | — | — | Borrower has tool; awaiting return (not tracked in proto) |
| **Denied** | (terminal state) | — | — | Request closed |
| **Cancelled** | (terminal state) | — | — | Request closed |

#### PlantUML State Diagram

```
[*] --> PendingApproval
PendingApproval --> Approved : Owner approves\n[qty > 0]\n/ qty -= 1
PendingApproval --> Denied : Owner denies / (notify)
PendingApproval --> Cancelled : Borrower cancels / (optional)
Approved --> [*]
Denied --> [*]
Cancelled --> [*]
```

### 5.2 User State Machine (Simplified)

For the prototype, User has a simple lifecycle:

| Current State | Trigger | Guard | Next State | Action |
|---|---|---|---|---|
| **Unregistered** | User submits registration form | Username unique, password valid | **Active** | Account created, session started |
| **Active** | User logs in | Credentials correct | **LoggedIn** | Session token issued |
| **Active** | User logs out | (none) | **LoggedOut** | Session destroyed |
| **LoggedIn** | Session expires | (none) | **Active** | User must re-login |

For prototype, we assume users stay **Active**; no account suspension or deletion.

---

## 6. Architecture Design

### 6.1 Architectural Style & Rationale

**Style**: **Layered Architecture**

**Rationale**:
- **Clear separation of concerns**: Each layer has a single responsibility (presentation, API routing, business logic, data access).
- **Easy to understand**: Ideal for a 30-minute prototype and a university audience; no complex event-driven or microservices overhead.
- **Simple to implement**: Straightforward to map from SRS features to code modules.
- **SQLite integration**: Lightweight persistence; fits naturally into the Infrastructure Layer.
- **Scalability**: Although layered, can scale horizontally by adding load balancers and read replicas later (post-prototype).

This style aligns with the SRS technology preferences (ASP.NET Core + HTML/CSS/JS + SQLite).

### 6.2 Layers and Responsibilities

```
┌────────────────────────────────────────────────────────┐
│ 1. PRESENTATION LAYER                                  │
│    - HTML pages, CSS styling, client-side JS          │
│    - Form validation, DOM manipulation                │
│    - HTTP requests to API Layer                       │
├────────────────────────────────────────────────────────┤
│ 2. API LAYER (ASP.NET Core Controllers)               │
│    - REST endpoints (POST, GET, PATCH)                │
│    - Input validation, error responses                │
│    - Authentication checks (session/token)            │
│    - Request → Application Layer delegation           │
├────────────────────────────────────────────────────────┤
│ 3. APPLICATION LAYER (Services)                        │
│    - Use-case orchestration                           │
│    - Transaction boundaries                           │
│    - Calls Domain Layer and Infrastructure Layer      │
│    - Examples: AuthenticationService, ToolService     │
├────────────────────────────────────────────────────────┤
│ 4. DOMAIN LAYER (Business Logic & Entities)           │
│    - Core domain entities: User, Tool, BorrowRequest  │
│    - Business rules (e.g., qty must be > 0 to lend)   │
│    - No infrastructure dependencies                   │
├────────────────────────────────────────────────────────┤
│ 5. INFRASTRUCTURE LAYER                               │
│    - Database access (Repositories)                   │
│    - SQLite adapters                                  │
│    - File storage (images)                            │
│    - External service adapters (if any)               │
└────────────────────────────────────────────────────────┘
```

### 6.3 Technology & Integration View

#### Technologies by Layer

| Layer | Primary Technologies |
|---|---|
| **Presentation** | HTML5, CSS3, Vanilla JavaScript (or lightweight framework like Alpine.js) |
| **API** | ASP.NET Core (C#); REST controllers; built-in dependency injection |
| **Application** | C# services; LINQ for queries |
| **Domain** | C# classes (User, Tool, BorrowRequest); business logic methods |
| **Infrastructure** | SQLite (via ADO.NET or Entity Framework Core); file system for images |

#### Component Communication

```
Browser (HTML/JS)
    ↕ HTTP (JSON)
ASP.NET Core REST API
    ↕ (method calls)
Application Services
    ↕ (method calls)
Domain Entities + Repositories
    ↕ (SQL queries)
SQLite Database
```

#### Data Flow Example: Approve Borrow Request

```
1. User clicks "Approve" on BrowseToolsPage
2. JS sends PATCH /api/borrow-requests/{id}/approve
3. API Controller (BorrowRequestController) receives request
4. Controller calls BorrowRequestService.ApproveBorrowRequest(id)
5. Service retrieves BorrowRequest + Tool from Repository
6. Service validates: is requester the tool owner?
7. Service calls Tool.DecreaseQuantity()
8. Service updates BorrowRequest status → Approved
9. Service persists changes via Repository.Save()
10. Repository executes UPDATE SQL on SQLite
11. Response returned to Browser; Dashboard refreshes
```

---

## 7. Database Design – Full ERD

### 7.1 Tables & Columns

#### Table: Users

```
- id (PK, INT, auto-increment)
- username (VARCHAR(255), unique, not null)
- password_hash (VARCHAR(255), not null) [bcrypt]
- created_at (DATETIME, default CURRENT_TIMESTAMP)
- updated_at (DATETIME, default CURRENT_TIMESTAMP)
```

#### Table: Tools

```
- id (PK, INT, auto-increment)
- owner_id (FK → Users.id, not null)
- name (VARCHAR(255), not null)
- quantity (INT, not null, default 1) [must be >= 0]
- image_url (VARCHAR(512)) [path or base64 reference; can be null]
- created_at (DATETIME, default CURRENT_TIMESTAMP)
- updated_at (DATETIME, default CURRENT_TIMESTAMP)
```

#### Table: BorrowRequests

```
- id (PK, INT, auto-increment)
- tool_id (FK → Tools.id, not null)
- borrower_id (FK → Users.id, not null)
- status (VARCHAR(20), enum: 'PendingApproval', 'Approved', 'Denied')
- requested_at (DATETIME, default CURRENT_TIMESTAMP)
- approved_at (DATETIME, nullable) [populated when status → Approved]
```

### 7.2 Relationships

| Relationship | Type | Description |
|---|---|---|
| Users 1 — * Tools | One-to-Many | Each user owns zero or more tools; each tool has one owner |
| Users 1 — * BorrowRequests | One-to-Many | Each user makes zero or more borrow requests as borrower |
| Tools 1 — * BorrowRequests | One-to-Many | Each tool has zero or more borrow requests |
| Users 1 — * Tools (via ownership) 1 — * BorrowRequests | Indirect | Tool owner can approve requests for their tools |

**Cascade Rules:**
- If a User is deleted: cascade delete their Tools and BorrowRequests (soft-delete preferred for audit, but hard-delete acceptable for prototype).
- If a Tool is deleted: cascade delete its BorrowRequests.

### 7.3 Indexing Strategy (for performance)

```
- Tools: index on (owner_id, quantity) → faster "my tools" and "available tools" queries
- BorrowRequests: index on (tool_id, status) → faster "pending requests for my tools" queries
- BorrowRequests: index on (borrower_id, status) → faster "my borrowed tools" queries
- Users: unique index on username → enforce uniqueness
```

---

## 8. Security Design

### 8.1 Authentication

**Mechanism**: Session-based with username/password.

**Flow**:
1. User submits username + password on LoginPage.
2. API endpoint `/api/auth/login` receives credentials.
3. AuthenticationService hashes input password and compares to stored hash (bcrypt).
4. On match: create session token (e.g., JWT or ASP.NET Core session cookie).
5. Return token to client; client includes in subsequent requests (Authorization header or cookie).
6. API middleware validates token on each request; reject if missing or expired.

**Where Enforced**: 
- API Layer: Middleware checks Authorization header or session cookie before routing to controllers.
- Application Layer: Services assume authenticated user context.

### 8.2 Authorization & RBAC

**Roles** (for prototype):
- **Authenticated User**: Can register, login, upload tools, browse, request borrow, approve/deny own tools' requests.
- **No Admin role**: Not in scope for 30-minute prototype.

**Authorization Rules** (Resource-Level):

| Action | Rule |
|---|---|
| View own tools | User must be owner of the tool |
| Edit/Delete tool | User must be owner |
| Approve/Deny borrow request | Authenticated user must be the owner of the tool in question |
| Request borrow | Authenticated user must NOT be the tool owner |
| View own borrowed tools | Authenticated user is the borrower in the request |

**RBAC Anti-Patterns to Avoid**:
- **BOLA (Broken Object Level Authorization)**: Validate that logged-in user owns the tool/request before allowing edit/approve/deny. **Mitigation**: Every endpoint checks ownership.
- **Privilege Escalation**: No stored "role" field; infer permissions from resource ownership only.

### 8.3 Data Protection & Privacy

**Sensitive Data**:
- **Passwords**: Must be hashed using bcrypt with appropriate salt rounds (≥10).
- **Session tokens**: Store in secure, HttpOnly cookies (if session-based) or in Authorization header (if JWT).
- **User identities**: No PII other than username collected in prototype.

**Logging Strategy**:
- Log authentication attempts (failed logins) for security audit.
- Avoid logging passwords or session tokens.
- Log approval/denial of borrow requests for audit trail.

**Storage**:
- All data persisted in SQLite (local file, no network transfer for prototype).
- HTTPS recommended for production (TLS encrypts data in transit).

### 8.4 Threats & Mitigations

| Threat | Severity | Mitigation |
|---|---|---|
| **SQL Injection** | High | Use parameterized queries (Entity Framework Core or prepared statements); never concatenate SQL strings |
| **XSS (Cross-Site Scripting)** | High | Sanitize user input before rendering in HTML; use templating engine that escapes by default; validate file uploads |
| **CSRF (Cross-Site Request Forgery)** | Medium | Use CSRF tokens on forms; ASP.NET Core includes built-in CSRF protection |
| **Weak Passwords** | Medium | Enforce minimum password length (≥6 characters per SRS); consider adding complexity requirements post-prototype |
| **Session Hijacking** | Medium | Use secure, HttpOnly cookies; enforce HTTPS; set session timeout |
| **Unauthorized Access (BOLA)** | Medium | Validate resource ownership on every API call; never trust user input for authorization |
| **File Upload Attacks** | Low (for proto) | Validate file type (image only); enforce size limit (5MB per SRS); store outside webroot if possible |

---

## 9. Deployment & Infrastructure (High-Level)

For prototype:
- **Single machine deployment**: ASP.NET Core app + SQLite on one host.
- **No load balancing**: Acceptable for ~50 concurrent users.
- **Data backup**: Copy SQLite file to external storage post-demo.
- **Future scaling**: Separate app servers + PostgreSQL + Redis (post-prototype).

---

## 10. Summary & Next Steps

### Key Design Decisions

1. **Layered Architecture**: Clear separation; easy to understand and implement.
2. **SQLite Database**: Lightweight; no setup required; suitable for prototype.
3. **Session-Based Auth**: Simple; no external identity provider needed.
4. **Resource-Level Authorization**: Only tool owners can approve their own requests.
5. **Minimal State Tracking**: BorrowRequest has 3 states; no return workflow for proto.

### Artifacts for Next Sprint (Detailed Design)

- **Flow Design Sheets**: Sequence diagrams for each use case.
- **API Contract (OpenAPI/Swagger)**: Detailed endpoint specifications.
- **Database Schema SQL**: Create table statements.
- **Error Handling Strategy**: Exception types and error codes.
- **Testing Strategy**: Unit tests, integration tests, E2E tests.

### Success Criteria

✅ All 5 core features working (registration, upload, browse, approve, dashboards)  
✅ Database persists data across requests  
✅ Authorization checks prevent unauthorized actions  
✅ Demo completes within 30 minutes  
✅ Code is readable and maintainable for future extensions  

---
