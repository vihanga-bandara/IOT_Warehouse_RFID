# SCSS Theming System & Dark Mode Guide

## Overview

This project now uses a **scalable SCSS architecture** with a complete theming system supporting dark/light mode. This makes UI updates significantly faster and the theme easily changeable.

## Architecture

### File Structure
```
frontend_web/src/
├── styles/
│   ├── variables.scss      # CSS variables & SCSS variables
│   ├── mixins.scss         # Reusable SCSS mixins
│   └── global.scss         # Global styles importing variables & mixins
├── composables/
│   └── useTheme.js         # Theme management composable
├── components/
│   └── ThemeToggle.vue     # Theme toggle button
└── views/
    └── *.vue               # Page components (can be refactored to use SCSS)
```

## Usage Guide

### 1. CSS Variables (For Styling)

All CSS variables are defined in `variables.scss` and available globally:

```scss
// Colors
--primary-dark
--primary-light
--accent-green
--accent-gray
--color-error
--color-warning
--color-success
--color-info

// Text
--text-primary
--text-secondary
--text-tertiary
--text-inverse

// Backgrounds
--bg-primary
--bg-secondary
--bg-tertiary
--bg-overlay

// Borders
--border-color
--border-color-light
--border-color-dark

// Shadows
--shadow-sm
--shadow-md
--shadow-lg
--shadow-xl

// Gradients
--gradient-primary
--gradient-accent
--gradient-warning
--gradient-page-bg

// Spacing
--spacing-xs (0.25rem)
--spacing-sm (0.5rem)
--spacing-md (1rem)
--spacing-lg (1.5rem)
--spacing-xl (2rem)
--spacing-2xl (2.5rem)

// Border Radius
--radius-sm (6px)
--radius-md (10px)
--radius-lg (12px)
--radius-xl (16px)

// Typography Scales
--text-xs, --text-sm, --text-base, --text-lg, --text-xl, --text-2xl, --text-3xl, --text-4xl
--weight-normal, --weight-medium, --weight-semibold, --weight-bold, --weight-extrabold

// Transitions
--transition-fast (0.2s ease)
--transition-normal (0.3s ease)
--transition-slow (0.4s ease)
```

### 2. Using CSS Variables in Vue Components

```vue
<template>
  <div class="card">
    <h2>Title</h2>
    <p>Content</p>
  </div>
</template>

<style scoped lang="scss">
.card {
  background: var(--bg-primary);
  color: var(--text-primary);
  border: 2px solid var(--border-color);
  border-radius: var(--radius-lg);
  padding: var(--spacing-lg);
  box-shadow: var(--shadow-md);
  transition: all var(--transition-normal);

  &:hover {
    border-color: var(--primary-light);
    box-shadow: var(--shadow-lg);
  }
}
</style>
```

### 3. Using SCSS Mixins

Import mixins at the top of your SCSS:

```vue
<style scoped lang="scss">
@import '../styles/mixins.scss';
@import '../styles/variables.scss';

// Use mixins for consistent patterns
.container {
  @include container;        // Max-width container with padding
}

.button-primary {
  @include btn-primary;      // Styled primary button
}

.card {
  @include card;             // Styled card with hover effect
}

.input {
  @include input-base;       // Styled input field
}

// Responsive design
.grid {
  @include grid-auto-fill(250px);  // Auto-fill grid layout
  
  @include respond-to("md") {
    @include grid-auto-fill(200px);
  }
}

// Flexbox utilities
.flex-header {
  @include flex-between;     // Flex with space-between
}

.centered {
  @include flex-col-center;  // Centered column flex
}

// Typography
.heading {
  @include heading(var(--text-2xl), var(--weight-bold));
}

.subtitle {
  @include subtitle;
}

// Animations
.card {
  @include animation-slide-up;
}

// Shadows
.component {
  @include shadow-md;
  @include shadow-hover;     // Hover effect with shadow
}
</style>
```

### 4. Responsive Design with Mixins

```scss
.grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: var(--spacing-lg);

  @include respond-to("md") {
    grid-template-columns: 1fr;
    gap: var(--spacing-md);
  }

  // Or use shorthand
  @include mobile {
    padding: var(--spacing-md);
  }

  @include tablet {
    font-size: var(--text-sm);
  }

  @include desktop {
    max-width: 1400px;
  }
}
```

### 5. Dark Mode Toggle

The theme toggle button appears in the top-right corner. Clicking it switches between light and dark mode, with preference stored in localStorage.

#### Programmatically Control Theme

```vue
<script setup>
import { useTheme } from '../composables/useTheme'

const { toggleTheme, setTheme, isDark, getTheme } = useTheme()

// Toggle between light and dark
toggleTheme()

// Set specific theme
setTheme('dark')
setTheme('light')

// Check current theme
if (isDark()) {
  console.log('Dark mode is active')
}

console.log(getTheme()) // 'light' or 'dark'
</script>
```

## Converting Existing Pages to SCSS

### Before (Inline CSS)
```vue
<style scoped>
.card {
  background: white;
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  padding: 1.5rem;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.card:hover {
  border-color: #3498db;
  box-shadow: 0 8px 24px rgba(30, 144, 255, 0.15);
}
</style>
```

### After (Using Mixins)
```vue
<style scoped lang="scss">
@import '../styles/mixins.scss';

.card {
  @include card;  // Automatically gets background, border, border-radius, padding, shadow, and hover effect!
}
</style>
```

## Creating Custom Themed Components

### Example: Custom Button Component

```vue
<template>
  <button :class="['custom-btn', variant]">
    <slot />
  </button>
</template>

<script setup>
const props = defineProps({
  variant: {
    type: String,
    default: 'primary', // primary, secondary, success, danger
    validator: (v) => ['primary', 'secondary', 'success', 'danger'].includes(v)
  }
})
</script>

<style scoped lang="scss">
@import '../styles/mixins.scss';

.custom-btn {
  @include btn-base;

  &.primary {
    @include btn-primary;
  }

  &.secondary {
    @include btn-secondary;
  }

  &.success {
    @include btn-success;
  }

  &.danger {
    @include btn-danger;
  }
}
</style>
```

## Changing the Theme Colors

To change the brand colors, edit `variables.scss`:

```scss
// Update these primary colors
$color-primary-dark: #003d6b;    // Change this
$color-primary-light: #1e90ff;   // Change this
$color-accent-green: #50c878;    // Change this
$color-accent-gray: #b8c5d6;     // Change this
```

The CSS variables will automatically update, and all components using CSS variables and mixins will reflect the new colors in both light and dark modes.

## Best Practices

### 1. Always Use CSS Variables for Colors
```scss
// ✅ Good
color: var(--text-primary);
background: var(--bg-secondary);
border: 2px solid var(--border-color);

// ❌ Avoid
color: #2c3e50;
background: #f8f9fa;
border: 2px solid #e8eef5;
```

### 2. Use Mixins for Common Patterns
```scss
// ✅ Good
.button {
  @include btn-primary;
}

.card {
  @include card;
}

// ❌ Avoid (manual styling)
.button {
  padding: 1rem 1.5rem;
  background: linear-gradient(...);
  color: white;
  border: none;
  border-radius: 10px;
  // ... etc
}
```

### 3. Use Responsive Mixins
```scss
// ✅ Good
.grid {
  @include grid-auto-fill(300px);
  
  @include mobile {
    @include grid-auto-fill(200px);
  }
}

// ❌ Avoid
.grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
}

@media (max-width: 600px) {
  .grid {
    grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
  }
}
```

### 4. Leverage Spacing Scale
```scss
// ✅ Good
.component {
  padding: var(--spacing-lg);
  margin-bottom: var(--spacing-md);
  gap: var(--spacing-sm);
}

// ❌ Avoid
.component {
  padding: 1.5rem;
  margin-bottom: 1rem;
  gap: 0.5rem;
}
```

## Available Mixins Reference

### Responsive
- `@include respond-to("sm")` - 400px breakpoint
- `@include respond-to("md")` - 600px breakpoint
- `@include respond-to("lg")` - 800px breakpoint
- `@include mobile` - Shorthand for 600px
- `@include tablet` - Shorthand for 800px
- `@include desktop` - For 1200px+

### Flexbox
- `@include flex-center` - Centered flex
- `@include flex-between` - Space-between flex
- `@include flex-col` - Column flex
- `@include flex-col-center` - Centered column

### Buttons
- `@include btn-base` - Base button styles
- `@include btn-primary` - Primary button
- `@include btn-secondary` - Secondary button
- `@include btn-success` - Success button
- `@include btn-danger` - Danger button
- `@include btn-icon` - Icon button (40x40px)

### Cards & Containers
- `@include card` - Card with hover effect
- `@include card-no-hover` - Card without hover
- `@include container` - Max-width container with padding
- `@include container-lg` - Large container

### Forms
- `@include input-base` - Input field styles
- `@include label` - Label styles
- `@include input-error` - Error state for inputs

### Badges
- `@include badge` - Generic badge
- `@include badge-success` - Success badge
- `@include badge-warning` - Warning badge
- `@include badge-error` - Error badge

### Shadows
- `@include shadow-sm` - Small shadow
- `@include shadow-md` - Medium shadow
- `@include shadow-lg` - Large shadow
- `@include shadow-xl` - Extra large shadow
- `@include shadow-hover` - Shadow on hover

### Animations
- `@include animation-slide-up` - Slide up animation
- `@include animation-slide-down` - Slide down animation
- `@include animation-fade-in` - Fade in animation
- `@include animation-pulse` - Pulse animation
- `@include animation-spin` - Spin animation

### Typography
- `@include heading($size, $weight)` - Heading with size/weight
- `@include subtitle` - Subtitle styles
- `@include text-truncate` - Single line truncate with ellipsis
- `@include text-clamp($lines)` - Multi-line truncate

### Utilities
- `@include visually-hidden` - Screen reader only
- `@include scrollbar-hide` - Hide scrollbars
- `@include custom-scrollbar` - Custom scrollbar styles
- `@include clearfix` - Clearfix for floats

## Performance Notes

- CSS variables have **zero performance overhead** compared to hardcoded values
- Dark mode toggle is instant (just attribute change)
- SCSS is compiled once at build time, no runtime cost
- Mixins are compiled into regular CSS, no runtime overhead

## Browser Support

- CSS Variables: All modern browsers (IE 11+ with fallbacks if needed)
- SCSS: Compiled to CSS at build time
- Dark mode: Works in all browsers with CSS variable support

## Next Steps for UI Updates

1. **New Page?** Copy the mixin pattern from existing pages
2. **New Component?** Use `@include card`, `@include btn-primary`, etc.
3. **New Colors?** Change `variables.scss` and all pages update automatically
4. **Mobile Responsive?** Use `@include mobile` or `@include respond-to("md")`
5. **Dark Mode?** Everything works automatically via CSS variables

## Example: Converting a Page to Use Mixins (5-10 min)

Before: 200+ lines of CSS
After: 50-80 lines of CSS using mixins

Average time to update: **5-10 minutes** instead of **30-45 minutes**
