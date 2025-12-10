import { ref, watch, onMounted } from 'vue'

// Theme state
const currentTheme = ref('light')

/**
 * useTheme - Composable for managing light/dark mode theme
 * Persists theme preference to localStorage
 * Applies data-theme attribute to document root
 */
export function useTheme() {
  // Initialize theme from localStorage or system preference
  const initializeTheme = () => {
    // Check localStorage first
    const storedTheme = localStorage.getItem('theme')
    if (storedTheme) {
      currentTheme.value = storedTheme
      applyTheme(storedTheme)
      return
    }

    // Check system preference
    if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
      currentTheme.value = 'dark'
      applyTheme('dark')
    } else {
      currentTheme.value = 'light'
      applyTheme('light')
    }
  }

  // Apply theme to DOM and localStorage
  const applyTheme = (theme) => {
    if (theme === 'dark') {
      document.documentElement.setAttribute('data-theme', 'dark')
      localStorage.setItem('theme', 'dark')
      currentTheme.value = 'dark'
    } else {
      document.documentElement.removeAttribute('data-theme')
      localStorage.setItem('theme', 'light')
      currentTheme.value = 'light'
    }
  }

  // Toggle between light and dark mode
  const toggleTheme = () => {
    const newTheme = currentTheme.value === 'light' ? 'dark' : 'light'
    applyTheme(newTheme)
  }

  // Set specific theme
  const setTheme = (theme) => {
    if (theme === 'light' || theme === 'dark') {
      applyTheme(theme)
    }
  }

  // Get current theme
  const getTheme = () => currentTheme.value

  // Check if dark mode is active
  const isDark = () => currentTheme.value === 'dark'

  // Initialize on mount
  onMounted(() => {
    initializeTheme()

    // Listen to system theme changes
    if (window.matchMedia) {
      const darkModeQuery = window.matchMedia('(prefers-color-scheme: dark)')
      darkModeQuery.addEventListener('change', (e) => {
        if (!localStorage.getItem('theme')) {
          applyTheme(e.matches ? 'dark' : 'light')
        }
      })
    }
  })

  return {
    currentTheme,
    toggleTheme,
    setTheme,
    getTheme,
    isDark,
    initializeTheme
  }
}
