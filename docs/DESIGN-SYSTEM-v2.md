# Design System — Variant B (Soft Rounded / Consumer)
## Angular + Tailwind CSS · Living Reference

> **Agent instruction:** This document is a MAP, not a build guide.
> The foundation described here is already implemented in the codebase (TASK-06 complete).
> Your job is to USE existing pieces — never rebuild them.
> Before writing any template code, look up the correct component in Section 4 and use it.

---

## 1. Where Things Live

| Layer | Location in repo |
|---|---|
| Tailwind config (all tokens) | `frontend/tailwind.config.js` |
| Global styles + utilities | `frontend/src/styles.scss` |
| All shared components | `frontend/src/app/shared/` |
| SharedModule (import this) | `frontend/src/app/shared/shared.module.ts` |
| Public layout shell | `frontend/src/app/layouts/public-layout/` |
| Category gradient map | `frontend/src/app/shared/components/molecules/app-course-card/app-course-card.component.ts` → `CATEGORY_GRADIENTS` |

**Every feature module must import `SharedModule`.** Never copy-paste a component's template inline.

---

## 2. Design Tokens (Tailwind)

All tokens are defined in `tailwind.config.js`. Use the Tailwind class name — never use raw hex values.

### Colors

| Tailwind class | Value | When to use |
|---|---|---|
| `bg-rose` / `text-rose` | `#F43F5E` | Primary CTA buttons, active states, brand accent |
| `bg-rose-dark` | `#E11D48` | Button hover state only |
| `bg-rose-light` / `text-rose-light` | `#FFF1F3` | Light backgrounds, badge fill, card hover fill |
| `bg-rose-mid` | `#FECDD3` | Subtle borders, tints |
| `text-amber` / `bg-amber` | `#F59E0B` | Warnings, streaks, lock badge, stat accent |
| `text-sky` / `bg-sky` | `#0EA5E9` | Info states, feature card accent (col 2/5) |
| `text-ink` | `#18181B` | All primary headings and body text |
| `text-ink-2` | `#52525B` | Secondary text, descriptions |
| `text-ink-3` | `#A1A1AA` | Muted text, metadata, placeholders |
| `bg-border` / `border-border` | `#F4F4F5` | Lightest dividers |
| `bg-border-2` / `border-border-2` | `#E4E4E7` | Standard component borders |
| `bg-bg` | `#FAFAFA` | Page background, section alternation |
| `bg-ink` | `#18181B` | Footer background |

### Border Radius

| Tailwind class | Value | Mandatory usage |
|---|---|---|
| `rounded-pill` | `100px` | **All buttons** — no exceptions |
| `rounded-card` | `24px` | **All cards**, modals, section blocks |
| `rounded-sm` | `12px` | Tooltips, small floating elements |
| `rounded-xs` | `6px` | Inline status chips only |

### Shadows

| Tailwind class | Use case |
|---|---|
| `shadow-xs` | Resting card state |
| `shadow-sm` | Navbar, preview cards |
| `shadow-md` | Tooltips, dropdowns |
| `shadow-lg` | Hovered card state |
| `shadow-rose` | Primary CTA button hover |
| `shadow-rose-lg` | Chatbot FAB hover |
| `shadow-fab` | Chatbot FAB resting |

### Layout Utilities (defined in `styles.scss`)

| Class | Expands to |
|---|---|
| `container-app` | `max-w-[1120px] mx-auto px-7` |
| `section-pad` | `py-20` |
| `section-pad-sm` | `py-12` |

---

## 3. Design Rules (Quick Reference)

| Rule | Requirement |
|---|---|
| Buttons | Always `rounded-pill` — never any other radius |
| Cards | Always `rounded-card` (24px) |
| Section backgrounds | Alternate `bg-bg` and `bg-white`; hero = `bg-bg`, next = `bg-white`, etc. |
| Transition speed | Max `duration-[200ms]` for cards, `duration-150` for buttons |
| Font | `font-sans` only — no serif |
| Spacing | Tailwind default 4px scale only — no arbitrary `px-[13px]` style values |
| Inline styles | Only permitted for dynamic values that cannot be expressed in Tailwind (e.g., `[style.background]="gradient"`) |

---

## 4. Shared Component Reference

Import `SharedModule` in your feature module, then use these selectors directly in templates.

---

### `<app-button>`

**Path:** `shared/components/atoms/app-button/`

| Input | Type | Options | Default |
|---|---|---|---|
| `variant` | `string` | `primary` · `secondary` · `ghost` · `nav-ghost` · `nav-cta` · `card-action` · `cta-white` | `primary` |
| `size` | `string` | `sm` · `md` · `lg` | `md` |
| `disabled` | `boolean` | — | `false` |
| `loading` | `boolean` | Shows spinner, blocks click | `false` |
| `fullWidth` | `boolean` | Adds `w-full` | `false` |

**Output:** `(clicked)` — void

**Back button content pattern (required):**
- Do not use raw arrow text like `← Back to ...` directly.
- For all back buttons, render icon + label using the shared utility classes:
  - `back-label`
  - `back-label-icon`
- This keeps vertical alignment and spacing consistent across browsers and fonts.

```html
<app-button variant="ghost" size="sm" (clicked)="onBack()">
  <span class="back-label">
    <svg aria-hidden="true" class="back-label-icon" viewBox="0 0 20 20" fill="none">
      <path d="M15.8333 10H4.16663" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" />
      <path d="M8.33329 14.1667L4.16663 10L8.33329 5.83334" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" />
    </svg>
    <span>Back to Dashboard</span>
  </span>
</app-button>
```

**Variant visual summary:**

| Variant | Background | Text | Use when |
|---|---|---|---|
| `primary` | `bg-rose` | white | Main CTA (hero, section bottom) |
| `secondary` | `bg-white` | `text-ink`, rose border on hover | Secondary action alongside primary |
| `ghost` | transparent | `text-ink-2` | Nav text links |
| `nav-ghost` | `bg-border` | `text-ink` | Navbar "Log In" button |
| `nav-cta` | `bg-rose` | white | Navbar "Get Started" button |
| `card-action` | `bg-rose-light` | `text-rose` | Full-width action inside a card |
| `cta-white` | `bg-white` | `text-rose` | Button placed on a colored background |

```html
<!-- Examples -->
<app-button variant="primary" size="lg" (clicked)="onStart()">🚀 Start Learning Free</app-button>
<app-button variant="secondary" size="lg" (clicked)="onBrowse()">Explore Catalog</app-button>
<app-button variant="nav-cta" size="sm" routerLink="/login">Get Started 🚀</app-button>
<app-button variant="card-action" size="sm" (clicked)="viewCourse.emit(id)">View Course</app-button>
<app-button variant="primary" size="lg" [loading]="isLoading" [fullWidth]="true" (clicked)="onSignIn()">
  Sign in with Google
</app-button>
```

---

### `<app-section-tag>`

**Path:** `shared/components/atoms/app-section-tag/`

Renders the rose pill eyebrow label above every section title.

| Input | Type |
|---|---|
| `icon` | `string` (emoji, optional) |
| `label` | `string` (required) |

```html
<app-section-tag icon="⭐" label="Top Rated" />
<app-section-tag icon="✨" label="Why EdTech" />
<app-section-tag label="Platform Features" />
```

---

### `<app-badge>`

**Path:** `shared/components/atoms/app-badge/`

Small pill badge, white text, colored background. Used on course card thumbnails.

| Input | Type |
|---|---|
| `label` | `string` |
| `color` | hex string (e.g. `'#4F46E5'`) |

```html
<app-badge label="Backend" color="#4F46E5" />
```

---

### `<app-spinner>`

**Path:** `shared/components/atoms/app-spinner/`

Animated loading ring. Used automatically inside `<app-button [loading]="true">`. Also use for page-level loading states.

| Input | Type | Default |
|---|---|---|
| `size` | `'sm' \| 'md' \| 'lg'` | `'sm'` |

```html
<app-spinner size="md" />
```

---

### `<app-course-card>`

**Path:** `shared/components/molecules/app-course-card/`

Complete course card with thumbnail, category badge, rank badge, rating row, and action button.

| Input | Type | Notes |
|---|---|---|
| `courseId` | `number` | |
| `title` | `string` | |
| `instructorName` | `string` | |
| `category` | `string` | Must match a key in `CATEGORY_GRADIENTS` |
| `categoryColor` | `string` (hex) | Badge background |
| `thumbnailGradient` | `string` | Use `CATEGORY_GRADIENTS[course.category]` |
| `thumbnailEmoji` | `string` | |
| `rating` | `number` | |
| `reviewCount` | `number` | |
| `rank` | `number \| null` | Pass `null` to hide rank badge |

**Output:** `(viewCourse)` — emits `courseId: number`

**Category gradient map** — import from the component file, do not redeclare:
```typescript
import { CATEGORY_GRADIENTS } from '../../shared/components/molecules/app-course-card/app-course-card.component';
```

```html
<app-course-card
  *ngFor="let course of courses; let i = index"
  [courseId]="course.id"
  [title]="course.title"
  [instructorName]="course.instructorName"
  [category]="course.category"
  [categoryColor]="course.categoryColor"
  [thumbnailGradient]="CATEGORY_GRADIENTS[course.category]"
  [thumbnailEmoji]="course.emoji"
  [rating]="course.rating"
  [reviewCount]="course.reviewCount"
  [rank]="i + 1"
  (viewCourse)="onViewCourse($event)" />
```

---

### `<app-feature-card>`

**Path:** `shared/components/molecules/app-feature-card/`

Card with a 3px colored top border accent.

| Input | Type | Notes |
|---|---|---|
| `icon` | `string` | emoji |
| `title` | `string` | |
| `description` | `string` | |
| `accentColor` | `'rose' \| 'sky' \| 'amber'` | Assign by grid position: 1&4=rose, 2&5=sky, 3&6=amber |

```html
<app-feature-card icon="🤖" title="In-Lesson AI Tutor"
  description="..." accentColor="rose" />
```

---

### `<app-stat-card>`

**Path:** `shared/components/molecules/app-stat-card/`

| Input | Type | Notes |
|---|---|---|
| `value` | `string` | e.g. `'50,000+'` |
| `label` | `string` | e.g. `'Active Students'` |
| `valueColor` | `'rose' \| 'sky' \| 'amber' \| 'emerald'` | Assign slot 1=rose, 2=sky, 3=amber, 4=emerald |

---

### `<app-preview-card>`

**Path:** `shared/components/molecules/app-preview-card/`

Used in hero section to show real app state examples.

| Input | Type |
|---|---|
| `icon` | `string` |
| `title` | `string` |
| `description` | `string` |
| `badgeLabel` | `string` |
| `badgeBg` | Tailwind class string (e.g. `'bg-rose-light'`) |
| `badgeText` | Tailwind class string (e.g. `'text-rose'`) |

---

### `<app-navbar>`

**Path:** `shared/components/organisms/app-navbar/`

Floating pill navbar. Sticky top. Projects nav buttons via `ng-content`.

```html
<app-navbar>
  <app-button variant="ghost"     size="sm" routerLink="/courses">Explore Catalog</app-button>
  <app-button variant="nav-ghost" size="sm" routerLink="/login">Log In</app-button>
  <app-button variant="nav-cta"   size="sm" routerLink="/login">Get Started 🚀</app-button>
</app-navbar>
```

**Do not project anything other than `<app-button>` elements into navbar.**

---

### `<app-footer>`

**Path:** `shared/components/organisms/app-footer/`

Dark footer. No inputs. Static platform links. Drop it in and it works.

```html
<app-footer />
```

---

### `<app-chatbot-fab>`

**Path:** `shared/components/organisms/app-chatbot-fab/`

Fixed bottom-right AI chatbot button. Position is `fixed` — do not wrap in any positioned container.

| Input | Type | Default |
|---|---|---|
| `isAuthenticated` | `boolean` | `false` |
| `isRateLimited` | `boolean` | `false` |
| `cooldownLabel` | `string` | `''` |

**Output:** `(openChat)` — emits void (only fires when authenticated and not rate limited)

**Behavior:**
- Guest → shows 🔒 badge → click redirects to `/login`
- Authenticated → no badge → click emits `(openChat)`
- Rate limited → shows ⏱ badge → click does nothing (cooldown shown in tooltip)

```html
<app-chatbot-fab
  [isAuthenticated]="(currentUser$ | async) !== null"
  (openChat)="onOpenChat()" />
```

**Always placed in the layout shell (`public-layout.component.html`), never inside a page component.**

---

## 5. Layout Shell

All public pages render inside `PublicLayoutComponent`. The shell provides navbar, footer, and chatbot FAB automatically. Page components only implement their own section content.

**Shell location:** `frontend/src/app/layouts/public-layout/public-layout.component.html`

**Viewport height contract (required):**
- Footer must stick to the bottom on short pages (no large blank area below footer).
- Implement shell wrapper as `min-h-screen flex flex-col`.
- Main content area must be `flex-1`.
- Page-level shells inside `main` should use `h-full` (not `min-h-screen`) unless truly fullscreen behavior is required.

**Routing pattern:**
```typescript
const routes: Routes = [
  {
    path: '',
    component: PublicLayoutComponent,
    children: [
      { path: '',       component: HomePageComponent  },
      { path: 'login',  component: LoginPageComponent },
      { path: 'courses', component: CourseCatalogComponent },
      // Add new public pages here as children of PublicLayoutComponent
    ],
  },
];
```

---

## 6. Section Background Alternation

Sections on the same page must alternate backgrounds. Start after hero with `bg-bg`.

| Section order | Background |
|---|---|
| Hero | `bg-bg` |
| Section 1 (after hero) | `bg-white` |
| Section 2 | `bg-bg` |
| Section 3 | `bg-white` |
| CTA banner wrapper | `bg-bg` (the inner rounded block carries the gradient) |
| Footer | `bg-ink` (always) |

---

## 7. CTA Banner Pattern

```html
<div class="px-4 py-6 bg-bg">
  <div class="bg-gradient-to-br from-rose via-rose-dark to-[#9F1239]
              rounded-[32px] px-12 py-[72px]
              grid grid-cols-[1fr_auto] gap-12 items-center">
    <div>
      <h2 class="text-white mb-3">Heading</h2>
      <p class="text-white/75 text-base leading-relaxed">Supporting copy.</p>
    </div>
    <div class="flex flex-col items-center gap-3 whitespace-nowrap">
      <app-button variant="cta-white" size="lg" (clicked)="onCta()">
        CTA Label →
      </app-button>
      <p class="text-[0.76rem] text-white/55">Sub-note text</p>
    </div>
  </div>
</div>
```

---

## 8. Grid Patterns

| Context | Class |
|---|---|
| 3-column cards (features, courses, preview) | `grid grid-cols-3 gap-4` |
| 4-column stat cards | `grid grid-cols-4 gap-4` |
| Footer | `grid grid-cols-[2fr_1fr_1fr] gap-12` |

Standard card grid gap is always `gap-4` (16px).

---

## 9. Animation Classes

| Interaction | Tailwind |
|---|---|
| Button hover lift | `hover:-translate-y-0.5` |
| Button scale | `hover:scale-[1.03]` |
| Card hover lift | `hover:-translate-y-1` |
| All interactive elements | `transition-all duration-150` |
| Cards | `transition-all duration-200` |
| FAB hover | `hover:-translate-y-[3px] hover:scale-[1.06]` |
| Online pulse dot | `animate-pulse-ring` (defined in `styles.scss`) |

---

## 10. What Agents Must NOT Do

| ❌ Forbidden | ✅ Correct |
|---|---|
| Raw `<button>` in any page template | `<app-button>` |
| `style="color: #F43F5E"` inline | Use `text-rose` Tailwind class |
| `border-radius: 24px` in SCSS | Use `rounded-card` |
| Rebuild course card layout in a page | Import and use `<app-course-card>` |
| Place `<app-navbar>` inside a page component | It lives in `PublicLayoutComponent` only |
| Use `position: fixed` in a page component for the FAB | `<app-chatbot-fab>` handles its own positioning |
| Add new routes outside `PublicLayoutComponent` children (for public pages) | Add as a child route |
| Import individual components directly instead of `SharedModule` | `imports: [SharedModule]` in your module |
| Redeclare `CATEGORY_GRADIENTS` in a new file | Import from `app-course-card.component.ts` |
