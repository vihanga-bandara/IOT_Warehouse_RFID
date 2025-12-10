<template>
  <div class="theme-toggle">
    <button
      @click="toggle"
      :aria-label="isDark ? 'Switch to light mode' : 'Switch to dark mode'"
      class="toggle-btn"
      :title="isDark ? 'Light Mode' : 'Dark Mode'"
    >
      <svg v-if="isDark" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
        <!-- Sun icon for dark mode (clicking switches to light) -->
        <circle cx="12" cy="12" r="5"/>
        <line x1="12" y1="1" x2="12" y2="3"/>
        <line x1="12" y1="21" x2="12" y2="23"/>
        <line x1="4.22" y1="4.22" x2="5.64" y2="5.64"/>
        <line x1="18.36" y1="18.36" x2="19.78" y2="19.78"/>
        <line x1="1" y1="12" x2="3" y2="12"/>
        <line x1="21" y1="12" x2="23" y2="12"/>
        <line x1="4.22" y1="19.78" x2="5.64" y2="18.36"/>
        <line x1="18.36" y1="5.64" x2="19.78" y2="4.22"/>
      </svg>
      <svg v-else width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
        <!-- Moon icon for light mode (clicking switches to dark) -->
        <path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z"/>
      </svg>
      <span class="tooltip">{{ isDark ? 'Light' : 'Dark' }}</span>
    </button>
  </div>
</template>

<script>
import { computed } from 'vue'
import { useTheme } from '../composables/useTheme'

export default {
  name: 'ThemeToggle',
  setup() {
    const { toggleTheme, isDark } = useTheme()

    return {
      toggle: toggleTheme,
      isDark: computed(() => isDark())
    }
  }
}
</script>

<style scoped lang="scss">
@import '../styles/variables.scss';
@import '../styles/mixins.scss';

.theme-toggle {
  position: relative;
}

.toggle-btn {
  @include btn-icon;
  background: var(--bg-secondary);
  color: var(--text-primary);
  border: 2px solid var(--border-color);
  position: relative;

  &:hover {
    background: var(--border-color);
    color: var(--primary-light);
    transform: scale(1.1);
  }

  &:active {
    transform: scale(0.95);
  }

  svg {
    width: 100%;
    height: 100%;
    stroke-linecap: round;
    stroke-linejoin: round;
  }

  .tooltip {
    @include visually-hidden;
    position: absolute;
    bottom: -30px;
    left: 50%;
    transform: translateX(-50%);
    background: var(--bg-secondary);
    color: var(--text-primary);
    padding: var(--spacing-xs) var(--spacing-sm);
    border-radius: var(--radius-sm);
    font-size: var(--text-xs);
    font-weight: var(--weight-semibold);
    white-space: nowrap;
    opacity: 0;
    transition: opacity var(--transition-fast);
    pointer-events: none;
  }

  &:hover .tooltip {
    opacity: 1;
    @include visually-hidden;
    position: absolute;
    display: inline-block;
  }
}
</style>
