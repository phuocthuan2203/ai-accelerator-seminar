# TASK-00 — Design System Implementation

## Context

This task implements the design system blueprint from `docs/DESIGN-SYSTEM-v2.md` for your specific project.

The Design System v2 defines a complete component library, design tokens, and UI rules. Your job is to translate that blueprint into actual, runnable code using your project's frontend stack.

This is a one-time setup task. After completion, all UI tasks will reuse these components and tokens — never rebuilding them.

---

## Prerequisites

Before starting this task, **you must provide the frontend stack details.** Without these, the agent cannot proceed.

Update `README.md → Project Structure Reference` with a new "Frontend Stack" section:

| Decision | Your Choice | Notes |
|----------|------------|-------|
| Framework | (Angular / React / Vue / Svelte / other?) | Determines component syntax |
| Styling system | (Tailwind CSS / CSS Modules / styled-components / SCSS / other?) | Determines how design tokens are defined |
| Language | (TypeScript / JavaScript) | Determines syntax and tooling |
| Build tool | (Angular CLI / Vite / Create React App / Webpack / other?) | Determines dev commands |
| Folder structure | (Already defined in README.md Project Structure Reference) | Where frontend code lives |

**Example:**
```
Framework: React 18
Styling: Tailwind CSS v3.4
Language: TypeScript
Build tool: Vite
Folder: frontend/src/
```

---

## Design References

Read these before implementing:

| Document | Section | Purpose |
|----------|---------|---------|
| `docs/DESIGN-SYSTEM-v2.md` | Sections 1–10 | Component specs, tokens, rules |
| `README.md` | Project Structure Reference | Your folder structure + frontend stack |

---

## Task Workflow

This task follows a 6-step process. Execute each step in order.

### Step 1: Understand the Design System Blueprint

Read `docs/DESIGN-SYSTEM-v2.md` completely. It describes:

- **Atoms** (Section 4): `app-button`, `app-badge`, `app-spinner`, `app-section-tag`
- **Molecules** (Section 4): `app-course-card`, `app-feature-card`, `app-stat-card`, `app-preview-card`
- **Organisms** (Section 4): `app-navbar`, `app-footer`, `app-chatbot-fab`
- **Design tokens** (Section 2): Colors, radius, shadows, spacing
- **Design rules** (Section 3): Buttons always `rounded-pill`, cards always `rounded-card`, no arbitrary values
- **Layout shell** (Section 5): `PublicLayoutComponent` with navbar, footer, FAB

### Step 2: Adapt Folder Structure

The Design System assumes this structure (Angular example):

```
frontend/src/app/
├── shared/
│   ├── components/
│   │   ├── atoms/       (app-button, app-badge, etc.)
│   │   ├── molecules/   (app-course-card, etc.)
│   │   └── organisms/   (app-navbar, app-footer, etc.)
│   └── shared.module.ts or shared.ts
├── layouts/
│   └── public-layout/
├── features/
│   ├── dashboard/
│   ├── login/
│   └── ...
└── styles.scss
```

**Your actual structure may differ.** Adapt the paths to match your project:

- **React**: Use `shared/` folder with `.jsx/.tsx` files instead of Angular components
- **Vue**: Use `shared/components/` with `.vue` files
- **Monolith**: Use `src/components/` instead of `frontend/src/app/shared/`

Document your final folder structure in a new "Frontend Folder Structure" subsection under README.md → Project Structure Reference.

### Step 3: Implement Design Tokens

Choose **one** approach based on your styling system:

#### Option A: Tailwind CSS (recommended)

If using Tailwind, create or update `frontend/tailwind.config.js`:

```javascript
module.exports = {
  theme: {
    extend: {
      colors: {
        rose: '#F43F5E',
        'rose-dark': '#E11D48',
        'rose-light': '#FFF1F3',
        'rose-mid': '#FECDD3',
        amber: '#F59E0B',
        sky: '#0EA5E9',
        ink: '#18181B',
        'ink-2': '#52525B',
        'ink-3': '#A1A1AA',
        border: '#F4F4F5',
        'border-2': '#E4E4E7',
        bg: '#FAFAFA',
      },
      borderRadius: {
        pill: '100px',
        card: '24px',
        sm: '12px',
        xs: '6px',
      },
      boxShadow: {
        xs: '0 1px 2px 0 rgba(0, 0, 0, 0.05)',
        sm: '0 1px 3px 0 rgba(0, 0, 0, 0.1)',
        md: '0 4px 6px -1px rgba(0, 0, 0, 0.1)',
        lg: '0 10px 15px -3px rgba(0, 0, 0, 0.1)',
        rose: '0 0 20px rgba(244, 63, 94, 0.3)',
        'rose-lg': '0 0 40px rgba(244, 63, 94, 0.4)',
        fab: '0 8px 16px rgba(0, 0, 0, 0.15)',
      },
    },
  },
};
```

#### Option B: CSS Variables

If using CSS Modules or plain CSS, create `frontend/src/styles/design-tokens.css`:

```css
:root {
  /* Colors */
  --color-rose: #F43F5E;
  --color-rose-dark: #E11D48;
  --color-rose-light: #FFF1F3;
  --color-amber: #F59E0B;
  --color-sky: #0EA5E9;
  --color-ink: #18181B;
  --color-ink-2: #52525B;
  --color-border: #F4F4F5;
  --color-bg: #FAFAFA;

  /* Border Radius */
  --radius-pill: 100px;
  --radius-card: 24px;
  --radius-sm: 12px;
  --radius-xs: 6px;

  /* Shadows */
  --shadow-xs: 0 1px 2px 0 rgba(0, 0, 0, 0.05);
  --shadow-sm: 0 1px 3px 0 rgba(0, 0, 0, 0.1);
  --shadow-lg: 0 10px 15px -3px rgba(0, 0, 0, 0.1);
  --shadow-rose: 0 0 20px rgba(244, 63, 94, 0.3);
}
```

### Step 4: Create Shared Components

For **each component in Section 4 of `DESIGN-SYSTEM-v2.md`**, create the component files:

#### Component Structure (Framework-Agnostic)

For each component, create:

1. **Component file** (TypeScript/JavaScript)
   - Define props/inputs
   - Implement behavior
   - Export the component

2. **Template file** (HTML/JSX/Vue template)
   - Use design tokens (Tailwind classes or CSS variables)
   - Never hardcode colors, spacing, or radius values
   - Reference Section 4 of DESIGN-SYSTEM-v2.md for exact inputs/outputs

3. **Style file** (optional, only if needed)
   - Component-specific styles only
   - Global/utility styles go in global styles file

#### Example: Implement `<app-button>` (Angular)

**File: `frontend/src/app/shared/components/atoms/app-button/app-button.component.ts`**

```typescript
import { Component, Input, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'app-button',
  standalone: true,
  templateUrl: './app-button.component.html',
  styleUrl: './app-button.component.scss',
})
export class AppButtonComponent {
  @Input() variant: 'primary' | 'secondary' | 'ghost' | 'nav-ghost' | 'nav-cta' | 'card-action' | 'cta-white' = 'primary';
  @Input() size: 'sm' | 'md' | 'lg' = 'md';
  @Input() disabled = false;
  @Input() loading = false;
  @Input() fullWidth = false;
  @Output() clicked = new EventEmitter<void>();

  onClick() {
    if (!this.disabled && !this.loading) {
      this.clicked.emit();
    }
  }

  get buttonClasses(): string {
    const baseClasses = 'rounded-pill font-sans transition-all duration-150';
    const sizeClasses = {
      sm: 'px-4 py-2 text-sm',
      md: 'px-6 py-3 text-base',
      lg: 'px-8 py-4 text-lg',
    }[this.size];
    const variantClasses = {
      primary: 'bg-rose text-white hover:bg-rose-dark',
      secondary: 'bg-white text-ink border border-rose hover:border-rose-dark',
      ghost: 'bg-transparent text-ink-2 hover:bg-border',
      'nav-ghost': 'bg-border text-ink hover:shadow-sm',
      'nav-cta': 'bg-rose text-white hover:shadow-rose',
      'card-action': 'bg-rose-light text-rose hover:bg-rose-mid',
      'cta-white': 'bg-white text-rose hover:shadow-sm',
    }[this.variant];
    const disabledClasses = this.disabled ? 'opacity-50 cursor-not-allowed' : '';
    const widthClasses = this.fullWidth ? 'w-full' : '';

    return `${baseClasses} ${sizeClasses} ${variantClasses} ${disabledClasses} ${widthClasses}`;
  }
}
```

**File: `frontend/src/app/shared/components/atoms/app-button/app-button.component.html`**

```html
<button
  [class]="buttonClasses"
  [disabled]="disabled || loading"
  (click)="onClick()"
  type="button"
>
  <ng-content></ng-content>
</button>
```

**Repeat this pattern for all 13 components listed in Section 4 of DESIGN-SYSTEM-v2.md:**
- Atoms (4): button, badge, spinner, section-tag
- Molecules (4): course-card, feature-card, stat-card, preview-card
- Organisms (3): navbar, footer, chatbot-fab
- Plus layout components (2): PublicLayoutComponent

### Step 5: Create Shared Module / Export System

**For Angular:**

Create `frontend/src/app/shared/shared.module.ts`:

```typescript
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AppButtonComponent } from './components/atoms/app-button/app-button.component';
import { AppBadgeComponent } from './components/atoms/app-badge/app-badge.component';
// ... import all 13 components

const COMPONENTS = [
  AppButtonComponent,
  AppBadgeComponent,
  // ... all components
];

@NgModule({
  imports: [CommonModule, ...COMPONENTS],
  exports: [...COMPONENTS],
})
export class SharedModule {}
```

**For React:**

Create `frontend/src/shared/index.ts`:

```typescript
export { AppButton } from './components/atoms/AppButton';
export { AppBadge } from './components/atoms/AppBadge';
// ... all components
```

**For Vue:**

Create `frontend/src/main.ts` or `frontend/src/shims.d.ts` to globally register shared components, or export them from `frontend/src/shared/index.ts`.

### Step 6: Create Global Styles

Create or update `frontend/src/styles.scss` (or equivalent for your styling system):

```scss
/* Layout utilities */
.container-app {
  max-width: 1120px;
  margin: 0 auto;
  padding: 0 28px; /* 7 * 4px */
}

.section-pad {
  padding: 80px 0; /* 20 * 4px */
}

.section-pad-sm {
  padding: 48px 0; /* 12 * 4px */
}

/* Back button styling */
.back-label {
  display: flex;
  align-items: center;
  gap: 8px;
}

.back-label-icon {
  width: 20px;
  height: 20px;
  flex-shrink: 0;
}

/* Animation: online pulse dot */
@keyframes pulse-ring {
  0% {
    box-shadow: 0 0 0 0 rgba(244, 63, 94, 0.7);
  }
  70% {
    box-shadow: 0 0 0 6px rgba(244, 63, 94, 0);
  }
  100% {
    box-shadow: 0 0 0 0 rgba(244, 63, 94, 0);
  }
}

.animate-pulse-ring {
  animation: pulse-ring 2s infinite;
}

/* Section background alternation */
.bg-section-hero {
  @apply bg-bg;
}

.bg-section-1 {
  @apply bg-white;
}

.bg-section-2 {
  @apply bg-bg;
}
```

---

## Affected Files

| File path | Action | Layer | Must-contain |
|-----------|--------|-------|--------------|
| `frontend/tailwind.config.js` (or CSS variables file) | CREATE | Config | All color/radius/shadow tokens from DESIGN-SYSTEM-v2.md §2 |
| `frontend/src/styles.scss` | CREATE | Global styles | `.container-app`, `.section-pad`, `.back-label`, animations |
| `frontend/src/app/shared/components/atoms/app-button/` | CREATE | Component (Atom) | `AppButtonComponent` with all 7 variants |
| `frontend/src/app/shared/components/atoms/app-badge/` | CREATE | Component (Atom) | `AppBadgeComponent` |
| `frontend/src/app/shared/components/atoms/app-spinner/` | CREATE | Component (Atom) | `AppSpinnerComponent` with size prop |
| `frontend/src/app/shared/components/atoms/app-section-tag/` | CREATE | Component (Atom) | `AppSectionTagComponent` |
| `frontend/src/app/shared/components/molecules/app-course-card/` | CREATE | Component (Molecule) | `AppCourseCardComponent` + `CATEGORY_GRADIENTS` export |
| `frontend/src/app/shared/components/molecules/app-feature-card/` | CREATE | Component (Molecule) | `AppFeatureCardComponent` |
| `frontend/src/app/shared/components/molecules/app-stat-card/` | CREATE | Component (Molecule) | `AppStatCardComponent` |
| `frontend/src/app/shared/components/molecules/app-preview-card/` | CREATE | Component (Molecule) | `AppPreviewCardComponent` |
| `frontend/src/app/shared/components/organisms/app-navbar/` | CREATE | Component (Organism) | `AppNavbarComponent` with ng-content for buttons |
| `frontend/src/app/shared/components/organisms/app-footer/` | CREATE | Component (Organism) | `AppFooterComponent` |
| `frontend/src/app/shared/components/organisms/app-chatbot-fab/` | CREATE | Component (Organism) | `AppChatbotFabComponent` with auth/rate-limit logic |
| `frontend/src/app/layouts/public-layout/` | CREATE | Layout | `PublicLayoutComponent` with min-h-screen flex layout |
| `frontend/src/app/shared/shared.module.ts` (or shared/index.ts) | CREATE | Module/Export | Exports all 13 components |

---

## Design Rule Checklist

| # | Exact rule (verbatim from DESIGN-SYSTEM-v2.md) | Source | Owner files | Proposed test name | Verification |
|---|-------|--------|-------------|-------------------|--------------|
| 1 | All buttons use `rounded-pill` (100px) radius. No exceptions. | §3 Design Rules | app-button | ButtonComponent_UsesRoundedPill_Always | Visual + CSS inspection |
| 2 | All cards use `rounded-card` (24px) radius. | §3 Design Rules | app-course-card, app-feature-card, app-stat-card, app-preview-card | CardComponent_UsesRoundedCard_Always | Visual + CSS inspection |
| 3 | Sections alternate `bg-bg` and `bg-white`. Hero = bg-bg, next = bg-white. | §6 | public-layout | SectionBackground_Alternates_Correctly | Visual inspection |
| 4 | Transition speed max `duration-[200ms]` for cards, `duration-150` for buttons. | §3 | All components | Transition_Speed_Correct | CSS inspection |
| 5 | No arbitrary padding/margin values. Use Tailwind scale (4px increments) or CSS variables. | §3 | All components | Spacing_NoArbitraryValues | CSS inspection |
| 6 | Only use colors from design tokens. No raw hex values. | §2 | All components | Colors_FromTokensOnly | CSS inspection |
| 7 | Only use font-sans. No serif fonts. | §3 | All components | Font_SansOnly | CSS inspection |
| 8 | Inline styles only for dynamic values (e.g., `[style.background]`). Not for static styling. | §3 | All components | InlineStyles_DynamicOnly | Code review |
| 9 | SharedModule exports all 13 components. Importable from one source. | §5 | shared.module.ts or index.ts | SharedModule_ExportsAll | Import test |
| 10 | CATEGORY_GRADIENTS map in app-course-card is importable and matches DESIGN-SYSTEM-v2.md §4. | §4 | app-course-card | CategoryGradients_ImportableAndCorrect | Import + values test |

---

## Edge Cases Handled

| # | Rule # | Trigger condition | Expected output / verification |
|---|--------|------------------|-------------------------------|
| 1 | 1 | Button with size="sm" and variant="primary" | Visual: small pill button with rose bg, proper padding |
| 2 | 2 | Card hover state triggered | Visual: card lifts (translate-y-1), shadow-lg applied, no glitch |
| 3 | 3 | Page has 4 sections (hero + 3 content sections) | Visual: bg-bg → bg-white → bg-bg → bg-white alternation correct |
| 4 | 9 | Feature module imports SharedModule, uses app-button in template | Functional: button renders and emits (clicked) on click |
| 5 | 10 | Component references CATEGORY_GRADIENTS[course.category] with unknown category | Functional: no console error; gradient is handled or defaults gracefully |

---

## Feature Flags

None for this task.

---

## Verification Steps (for this task)

### Automated Checks
- [ ] All components compile/build without errors
- [ ] SharedModule imports and exports work
- [ ] No TypeScript errors in component files

### Manual/Visual Checks
- [ ] Button component renders with all 7 variants correctly
- [ ] All cards use `rounded-card` (24px radius — measurable with dev tools)
- [ ] All buttons use `rounded-pill` (100px radius)
- [ ] Colors match design tokens exactly (use color picker)
- [ ] Spacing uses only 4px increments (dev tools measure)
- [ ] No inline styles except for dynamic content
- [ ] PublicLayoutComponent renders navbar, footer, FAB in correct positions
- [ ] Sections alternate background colors correctly on a test page

### Test Examples (write at least one per component)

```typescript
// app-button.component.spec.ts
describe('AppButtonComponent', () => {
  it('should apply "rounded-pill" class for all variants', () => {
    // Render button with each variant
    // Assert that buttonClasses includes 'rounded-pill'
  });

  it('should emit (clicked) when not disabled and not loading', () => {
    // Click button
    // Assert (clicked) emits exactly once
  });
});

// app-course-card.component.spec.ts
describe('AppCourseCardComponent', () => {
  it('should apply "rounded-card" class', () => {
    // Render card
    // Assert that template includes 'rounded-card' class
  });

  it('should render CATEGORY_GRADIENTS[category] correctly', () => {
    // Pass category='Backend'
    // Assert gradient is applied from CATEGORY_GRADIENTS
  });
});
```

---

## Definition of Done

This task is complete when:

- [ ] All 13 components (4 atoms + 4 molecules + 3 organisms + 2 layout) are created and compile
- [ ] Design tokens (colors, radius, shadows) are defined in either Tailwind config or CSS variables
- [ ] SharedModule or equivalent export system is set up and importable
- [ ] Global styles file (`styles.scss`) exists with layout utilities and animations
- [ ] PublicLayoutComponent renders navbar, footer, chatbot FAB correctly
- [ ] All rules in Design Rule Checklist are verified (visual + code inspection)
- [ ] No hardcoded color/spacing/radius values exist (all use tokens)
- [ ] At least one test per component type passes
- [ ] README.md is updated with Frontend Stack and Folder Structure sections
- [ ] DESIGN-SYSTEM-v2.md is marked as "already implemented" or "reference for this sprint"

