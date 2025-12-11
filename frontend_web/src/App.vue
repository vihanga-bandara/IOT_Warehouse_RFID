<script setup>
import { useTheme } from './composables/useTheme'

// Initialize theme
useTheme()
</script>

<template>
  <div class="app-wrapper">
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
