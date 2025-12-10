<script setup>
import { useTheme } from './composables/useTheme'
import ThemeToggle from './components/ThemeToggle.vue'

// Initialize theme
useTheme()
</script>

<template>
  <div class="app-wrapper">
    <div class="theme-toggle-container">
      <ThemeToggle />
    </div>
    <router-view />
  </div>
</template>

<style lang="scss" scoped>
@import './styles/variables.scss';
@import './styles/mixins.scss';

.app-wrapper {
  width: 100%;
  height: 100%;
  display: flex;
  flex-direction: column;
}

.theme-toggle-container {
  position: fixed;
  top: var(--spacing-md);
  right: var(--spacing-md);
  z-index: 500;

  @include respond-to("md") {
    top: var(--spacing-sm);
    right: var(--spacing-sm);
  }
}
</style>

<style>
html, body, #app {
  margin: 0;
  padding: 0;
  width: 100%;
  height: 100%;
  overflow-x: hidden;
}

#app {
  width: 100%;
  height: 100%;
}
</style>
