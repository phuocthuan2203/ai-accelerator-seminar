# Software Requirements Specification (SRS)
## Tool Lending Platform

---

## 1. Document Information

- **Project Name**: Tool Lending Platform
- **Version**: 1.0
- **Last Updated**: 2026-05-06
- **Author**: Prototype for 30-minute demo

---

## 2. Introduction

### 2.1 Purpose

This SRS defines the requirements for a web-based tool lending platform that enables neighbors to lend tools to each other instead of buying them individually. The document is intended for:
- Product owners and stakeholders
- Frontend and backend developers
- QA and testing teams

### 2.2 Scope

**In Scope:**
- User registration and login (username/password)
- Tool upload and management (name, quantity, image)
- Dashboard for tool owners to manage their lending inventory
- Dashboard for users to manage borrowed tools and approve lending requests
- Tool availability management based on quantity
- Approval workflow for borrowing requests

**Out of Scope:**
- Payment processing or financial transactions
- Identity verification or KYC
- Damage/loss liability handling
- Real-time chat or notifications
- Mobile app (web-only for prototype)

### 2.3 Definitions, Acronyms, and Abbreviations

| Term | Definition |
|------|-----------|
| Tool Owner | User who uploads and lends tools |
| Borrower | User who requests to borrow tools |
| Quantity | Number of available units of a tool ready to lend |
| Approval Workflow | Process where tool owner must approve a borrow request before lending |

---

## 3. Overall Description

### 3.1 Product Perspective

Standalone web application. Users access the platform through a web browser to manage their tool inventory and browse/request tools from other neighbors.

**Platform**: Web application (HTML/CSS/JavaScript frontend)

### 3.2 Target Users & Roles

| User Type | Goal | Typical Scenario |
|-----------|------|-----------------|
| **Neighbor (Tool Owner)** | Lend tools they own to reduce waste and help community | Uploads a power drill they rarely use; approves borrow requests from neighbors; tracks inventory |
| **Neighbor (Borrower)** | Borrow tools instead of buying them | Searches for a hammer; requests to borrow; picks up and returns when done |
| **Admin** | Moderate and manage platform *(optional for prototype)* | Reviews problematic listings or disputes *(future scope)* |

### 3.3 User Needs & Goals

- **Reduce cost**: Neighbors avoid buying tools they use infrequently
- **Build community**: Foster trust and connection among neighbors
- **Sustainability**: Reduce consumption and waste
- **Convenience**: Easy-to-use interface for uploading and browsing tools

### 3.4 Technology & Architecture Preferences

- **Frontend**: HTML, CSS, JavaScript (vanilla or lightweight framework)
- **Backend**: ASP.NET Core
- **Database**: Simple lightweight engine (e.g., SQLite for prototype)
- **Authentication**: Session-based (username/password)

### 3.5 Assumptions & Dependencies

- Users have reliable internet access
- Users trust each other (no formal identity verification for prototype)
- Tool owners accurately report quantity and tool condition
- Prototype does not include payment or insurance mechanisms
- No external API integrations required for MVP

---

## 4. Functional Requirements

### 4.1 Feature: User Registration & Login

**Description**: Users create accounts and authenticate to access the platform.

**User Stories**
- As a new user, I want to register with a username and password so that I can access the platform.
- As a registered user, I want to log in with my credentials so that I can manage my tools and browsing history.

**Acceptance Criteria**
- Given I am on the registration page, when I enter a unique username and password, then my account is created and I am redirected to login.
- Given I have an account, when I enter my correct username and password on the login page, then I am authenticated and redirected to my dashboard.
- Given I enter an incorrect password, when I click login, then an error message is displayed and I remain on the login page.
- Username must be unique; password must be at least 6 characters.

---

### 4.2 Feature: Tool Upload & Management

**Description**: Tool owners upload tools with name, quantity, and image for others to borrow.

**User Stories**
- As a tool owner, I want to upload a tool with a name, quantity, and image so that others can see what I'm willing to lend.
- As a tool owner, I want to view all tools I have uploaded so that I can manage my inventory.
- As a tool owner, I want to edit or delete a tool listing so that I can keep my inventory accurate.

**Acceptance Criteria**
- Given I am logged in, when I navigate to "Upload Tool" and fill in name, quantity, and upload an image, then the tool is saved and appears in my dashboard.
- Given I have uploaded a tool, when I view my tools dashboard, then I can see all tools I uploaded with their current quantity.
- Given a tool is listed, when I click "Edit", then I can change the name, quantity, or image and save changes.
- Given a tool is listed, when I click "Delete", then the tool is removed and no longer visible to borrowers.
- Quantity must be a positive integer; image file size limit is 5MB.

---

### 4.3 Feature: Tool Browsing & Requesting

**Description**: Users search and request to borrow tools from other tool owners.

**User Stories**
- As a borrower, I want to browse available tools in the platform so that I can find what I need.
- As a borrower, I want to request to borrow a tool so that the owner can approve or deny my request.

**Acceptance Criteria**
- Given I am logged in, when I navigate to the "Browse Tools" page, then I see all tools with quantity > 0 from other users.
- Given I view a tool listing, when I click "Request to Borrow", then a request is created with a status of "Pending Approval".
- Given I submit a borrow request, when the owner approves it, then the tool's quantity is reduced by 1 and I can see it in my "Borrowed Tools" dashboard.
- Only tools with quantity > 0 are visible; tool owners cannot see their own tools in the browse list.

---

### 4.4 Feature: Approval Workflow (Tool Owner Dashboard)

**Description**: Tool owners approve or deny borrow requests from other users.

**User Stories**
- As a tool owner, I want to see all pending borrow requests for my tools so that I can decide who to lend to.
- As a tool owner, I want to approve a borrow request so that the borrower can take the tool.
- As a tool owner, I want to deny a borrow request so that I can reject borrowers I'm uncomfortable with.

**Acceptance Criteria**
- Given I am a tool owner, when I view my "Pending Requests" dashboard, then I see all borrow requests for my tools with status "Pending Approval".
- Given a pending request, when I click "Approve", then the request status changes to "Approved", the tool quantity decreases by 1, and the borrower is notified.
- Given a pending request, when I click "Deny", then the request status changes to "Denied" and the borrower is notified.
- If tool quantity reaches 0 after approval, no further borrow requests can be made for that tool.

---

### 4.5 Feature: Borrowed Tools Dashboard (Borrower)

**Description**: Users track tools they are currently borrowing.

**User Stories**
- As a borrower, I want to see all tools I am currently borrowing so that I know what I have and when to return them.

**Acceptance Criteria**
- Given I am a borrower, when I view my "Borrowed Tools" dashboard, then I see all tools with status "Approved" that I requested.
- Each borrowed tool shows the owner's name, tool name, and approval date.

---

## 5. Non-Functional Requirements

### 5.1 Performance & Scalability

- Page load time: < 3 seconds for typical operations
- Support for initial prototype: ~50 concurrent users
- Database query response time: < 500ms

### 5.2 Security & Privacy

- Passwords stored using hashing (e.g., bcrypt)
- Session-based authentication with secure cookies
- No sensitive financial or identity data collected
- HTTPS recommended for production deployment

### 5.3 Reliability & Availability

- Target uptime for prototype: 99% during demo
- Data persistence using local SQLite database

### 5.4 Usability & Accessibility

- Responsive design for desktop browsers (mobile optimization optional)
- Clear navigation and intuitive workflows
- Form validation with helpful error messages

---

## 6. Data & Integrations

### 6.1 Data Model (Conceptual)

| Entity | Key Attributes |
|--------|----------------|
| **User** | UserID, Username, Password (hashed), CreatedAt |
| **Tool** | ToolID, OwnerID, Name, Quantity, ImageURL, CreatedAt |
| **BorrowRequest** | RequestID, ToolID, BorrowerID, Status (Pending/Approved/Denied), RequestDate, ApprovedDate |

### 6.2 Integrations

- **None required for prototype**: No external API integrations (payment, maps, auth services)

---

## 7. Risks & Open Questions

| Risk / Question | Impact | Mitigation |
|-----------------|--------|-----------|
| Tool owners may not return borrowed tools | High | Implement return tracking and review system (future) |
| Damaged or lost tools | High | Add liability agreement / deposit system (future) |
| Spam or fake accounts | Medium | Add email verification or rate limiting (future) |
| Scalability beyond 50 users | Low (prototype scope) | Migrate to production DB and optimize queries (future) |
| No way to contact borrowers | Medium | Add in-app messaging or notifications (future) |

---

## 8. Success Criteria for Prototype

- ✅ User can register and log in
- ✅ User can upload tools with name, quantity, and image
- ✅ User can browse and request tools
- ✅ Tool owner can approve/deny requests
- ✅ Quantity auto-decreases on approval
- ✅ Dashboard shows owned and borrowed tools
- ✅ Demo completed within 30 minutes with all features working

---
