import { createApp } from 'vue'
import { createPinia } from 'pinia'
import { useTheme } from './composables/useTheme'
import './styles/global.scss'
import App from './App.vue'
import router from './router'

const app = createApp(App)
const pinia = createPinia()

app.use(pinia)
app.use(router)

// Initialize theme on app creation
const { initializeTheme } = useTheme()
initializeTheme()

app.mount('#app')
