/* Shared Component Factory Functions */
/* All components render HTML strings using design tokens from design-tokens.css */

/* CATEGORY GRADIENTS MAP — Must match DESIGN-SYSTEM-v2.md */
export const CATEGORY_GRADIENTS = {
  'Backend': 'linear-gradient(135deg, #4F46E5 0%, #7C3AED 100%)',
  'Frontend': 'linear-gradient(135deg, #0EA5E9 0%, #06B6D4 100%)',
  'DevOps': 'linear-gradient(135deg, #F97316 0%, #FB923C 100%)',
  'Mobile': 'linear-gradient(135deg, #EC4899 0%, #F43F5E 100%)',
  'Data Science': 'linear-gradient(135deg, #6366F1 0%, #A78BFA 100%)',
  'AI/ML': 'linear-gradient(135deg, #10B981 0%, #34D399 100%)',
};

/* ========== ATOMS ========== */

/**
 * createButton - Render a button atom
 * @param {string} variant - 'primary' | 'secondary' | 'ghost' | 'nav-ghost' | 'nav-cta' | 'card-action' | 'cta-white'
 * @param {string} size - 'sm' | 'md' | 'lg'
 * @param {string} label - Button text
 * @param {Object} options - { disabled, loading, fullWidth, ariaLabel, onClick }
 * @returns {string} HTML string
 */
export function createButton(variant = 'primary', size = 'md', label = '', options = {}) {
  const { disabled = false, loading = false, fullWidth = false, ariaLabel = label, onClick = null } = options;
  const classes = [
    'btn',
    `btn-${variant}`,
    `btn-${size}`,
    fullWidth ? 'btn-full-width' : '',
  ].filter(Boolean).join(' ');

  const onClickAttr = onClick ? ` onclick="${onClick}"` : '';
  const disabledAttr = disabled || loading ? ' disabled' : '';

  return `<button class="${classes}" aria-label="${ariaLabel}"${disabledAttr}${onClickAttr}>
    ${loading ? '<span class="spinner spinner-sm"></span>' : label}
  </button>`;
}

/**
 * createBadge - Render a badge atom
 * @param {string} label - Badge text
 * @param {string} color - Hex color code (e.g. '#4F46E5')
 * @returns {string} HTML string
 */
export function createBadge(label = '', color = '#F43F5E') {
  return `<span class="badge" style="background-color: ${color};">${label}</span>`;
}

/**
 * createSpinner - Render a spinner atom
 * @param {string} size - 'sm' | 'md' | 'lg'
 * @param {string} ariaLabel - Accessibility label
 * @returns {string} HTML string
 */
export function createSpinner(size = 'sm', ariaLabel = 'Loading') {
  return `<span class="spinner spinner-${size}" role="status" aria-label="${ariaLabel}"></span>`;
}

/**
 * createSectionTag - Render a section tag (eyebrow label)
 * @param {string} label - Tag text
 * @param {string} icon - Emoji or icon character (optional)
 * @returns {string} HTML string
 */
export function createSectionTag(label = '', icon = '') {
  const iconHtml = icon ? `<span class="section-tag-icon">${icon}</span>` : '';
  return `<div class="section-tag">${iconHtml}<span>${label}</span></div>`;
}

/* ========== MOLECULES ========== */

/**
 * createCourseCard - Render a course card molecule
 * @param {Object} data - { courseId, title, instructorName, category, categoryColor, thumbnailGradient, thumbnailEmoji, rating, reviewCount, rank }
 * @param {Function} onViewClick - Callback when view button clicked
 * @returns {string} HTML string
 */
export function createCourseCard(data = {}, onViewClick = null) {
  const {
    courseId = 0,
    title = 'Untitled Course',
    instructorName = 'Unknown Instructor',
    category = 'Backend',
    categoryColor = '#4F46E5',
    thumbnailGradient = CATEGORY_GRADIENTS['Backend'],
    thumbnailEmoji = '📚',
    rating = 0,
    reviewCount = 0,
    rank = null,
  } = data;

  const rankBadge = rank ? `<span class="badge" style="background-color: #F59E0B;">#{#${rank}</span>` : '';
  const onClickAttr = onViewClick ? ` onclick="${onViewClick}(${courseId})"` : '';

  return `<div class="card course-card">
    <div class="course-card-thumbnail" style="background: ${thumbnailGradient};">
      <span style="font-size: 3rem;">${thumbnailEmoji}</span>
      <span class="course-card-badge">${createBadge(category, categoryColor)}</span>
      ${rankBadge ? `<span class="course-card-rank">${rankBadge}</span>` : ''}
    </div>
    <h3 class="course-card-title">${title}</h3>
    <p class="course-card-instructor">${instructorName}</p>
    <div class="course-card-rating">
      <span>⭐ ${rating.toFixed(1)}</span>
      <span>(${reviewCount} reviews)</span>
    </div>
    <div class="course-card-action">
      ${createButton('card-action', 'sm', 'View Course', { onClick: `viewCourse_${courseId}` })}
    </div>
  </div>`;
}

/**
 * createFeatureCard - Render a feature card molecule
 * @param {Object} data - { icon, title, description, accentColor }
 * @returns {string} HTML string
 */
export function createFeatureCard(data = {}) {
  const {
    icon = '✨',
    title = 'Feature Title',
    description = 'Feature description goes here.',
    accentColor = 'rose', // 'rose' | 'sky' | 'amber'
  } = data;

  return `<div class="card feature-card feature-card-${accentColor}">
    <div class="feature-card-icon">${icon}</div>
    <h3 class="feature-card-title">${title}</h3>
    <p class="feature-card-description">${description}</p>
  </div>`;
}

/**
 * createStatCard - Render a stat card molecule
 * @param {Object} data - { value, label, valueColor }
 * @returns {string} HTML string
 */
export function createStatCard(data = {}) {
  const {
    value = '0',
    label = 'Stat Label',
    valueColor = 'rose', // 'rose' | 'sky' | 'amber' | 'emerald'
  } = data;

  return `<div class="card stat-card stat-card-${valueColor}">
    <div class="stat-card-value">${value}</div>
    <div class="stat-card-label">${label}</div>
  </div>`;
}

/**
 * createPreviewCard - Render a preview card molecule
 * @param {Object} data - { icon, title, description, badgeLabel, badgeBg, badgeText }
 * @returns {string} HTML string
 */
export function createPreviewCard(data = {}) {
  const {
    icon = '📱',
    title = 'Preview Title',
    description = 'Preview description goes here.',
    badgeLabel = 'New',
    badgeBg = 'bg-rose-light',
    badgeText = 'text-rose',
  } = data;

  return `<div class="card preview-card">
    <div class="preview-card-icon">${icon}</div>
    <h3 class="preview-card-title">${title}</h3>
    <p class="preview-card-description">${description}</p>
    <span class="preview-card-badge" style="background-color: var(--color-rose-light); color: var(--color-rose);">
      ${badgeLabel}
    </span>
  </div>`;
}

/* ========== ORGANISMS ========== */

/**
 * createNavbar - Render navbar organism
 * @param {Array<string>} buttonHtmls - Array of HTML button strings
 * @param {string} brandText - Navbar brand/logo text
 * @returns {string} HTML string
 */
export function createNavbar(buttonHtmls = [], brandText = 'Tool Lending') {
  const buttonsHtml = buttonHtmls.join('');
  return `<nav class="navbar">
    <div class="navbar-brand">${brandText}</div>
    <div class="navbar-menu">${buttonsHtml}</div>
  </nav>`;
}

/**
 * createFooter - Render footer organism
 * @returns {string} HTML string
 */
export function createFooter() {
  return `<footer class="footer">
    <div class="footer-container">
      <div class="footer-section">
        <div class="footer-brand">Tool Lending</div>
        <p style="font-size: 0.875rem; line-height: 1.5; margin-top: 1rem; color: rgba(255, 255, 255, 0.7);">
          Empowering communities through shared resources.
        </p>
      </div>
      <div class="footer-section">
        <h3>Platform</h3>
        <ul>
          <li><a href="#dashboard">Dashboard</a></li>
          <li><a href="#tools">Browse Tools</a></li>
          <li><a href="#requests">My Requests</a></li>
          <li><a href="#profile">Profile</a></li>
        </ul>
      </div>
      <div class="footer-section">
        <h3>Company</h3>
        <ul>
          <li><a href="#about">About Us</a></li>
          <li><a href="#privacy">Privacy Policy</a></li>
          <li><a href="#terms">Terms of Service</a></li>
          <li><a href="#contact">Contact</a></li>
        </ul>
      </div>
    </div>
  </footer>`;
}

/**
 * createChatbotFab - Render chatbot FAB (floating action button) organism
 * @param {Object} options - { isAuthenticated, isRateLimited, cooldownLabel, onClick }
 * @returns {string} HTML string
 */
export function createChatbotFab(options = {}) {
  const {
    isAuthenticated = false,
    isRateLimited = false,
    cooldownLabel = '',
    onClick = null,
  } = options;

  const icon = isAuthenticated ? '💬' : '🔒';
  const title = isAuthenticated ? (isRateLimited ? 'Rate Limited' : 'Chat with AI') : 'Sign in to chat';
  const disabled = !isAuthenticated || isRateLimited;
  const badge = !isAuthenticated ? '<div class="chatbot-fab-badge">🔒</div>' :
    (isRateLimited ? '<div class="chatbot-fab-badge">⏱</div>' : '');
  const onClickAttr = onClick && !disabled ? ` onclick="${onClick}"` : '';
  const disabledAttr = disabled ? ' disabled' : '';

  return `<button class="chatbot-fab" title="${title}"${onClickAttr}${disabledAttr} aria-label="${title}">
    ${icon}
    ${badge}
  </button>`;
}

/* ========== LAYOUTS ========== */

/**
 * createPublicLayout - Render the public layout shell
 * @param {string} navbarHtml - Navbar HTML
 * @param {string} mainHtml - Main content HTML
 * @param {string} footerHtml - Footer HTML
 * @param {string} fabHtml - Chatbot FAB HTML (optional)
 * @returns {string} HTML string
 */
export function createPublicLayout(navbarHtml = '', mainHtml = '', footerHtml = '', fabHtml = '') {
  return `<div class="public-layout">
    <header class="public-layout-header">${navbarHtml}</header>
    <main class="public-layout-main container-app">${mainHtml}</main>
    <footer class="public-layout-footer">${footerHtml}</footer>
    ${fabHtml}
  </div>`;
}

/* ========== UTILITIES ========== */

/**
 * createGrid - Render a grid wrapper for card layouts
 * @param {Array<string>} cardHtmls - Array of HTML card strings
 * @param {number} columns - Number of columns (3 or 4)
 * @returns {string} HTML string
 */
export function createGrid(cardHtmls = [], columns = 3) {
  const gridClass = columns === 4 ? 'grid-cols-4' : 'grid-cols-3';
  const cardsHtml = cardHtmls.join('');
  return `<div class="${gridClass} gap-4">${cardsHtml}</div>`;
}

/**
 * createSection - Render a section with optional background
 * @param {string} content - Section HTML content
 * @param {string} background - 'hero' | '1' | '2' | '3'
 * @param {string} padding - 'lg' | 'sm'
 * @returns {string} HTML string
 */
export function createSection(content = '', background = 'hero', padding = 'lg') {
  const bgClass = background === 'hero' ? 'bg-section-hero' : `bg-section-${background}`;
  const padClass = padding === 'sm' ? 'section-pad-sm' : 'section-pad';
  return `<section class="${bgClass} ${padClass}">${content}</section>`;
}
