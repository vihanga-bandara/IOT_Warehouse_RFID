<template>
  <div class="scanner-select-container">
    <div class="scanner-select-card surface-card surface-card--padded">
      <div class="logo-section">
        <img src="/tooltrack-logo.png" alt="ToolTrackPro Logo" class="logo-image" />
        <p class="subtitle">Select Your Scanner</p>
      </div>

      <div class="welcome-section">
        <div class="user-greeting">
          <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/>
            <circle cx="12" cy="7" r="4"/>
          </svg>
          <span>Welcome, <strong>{{ authStore.user?.name || 'User' }}</strong></span>
        </div>
      </div>

      <form @submit.prevent="handleBindScanner" class="scanner-form">
        <div class="form-group">
          <label for="scannerName">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <rect x="2" y="6" width="20" height="12" rx="2"/>
              <path d="M12 12m-2 0a2 2 0 1 0 4 0a2 2 0 1 0 -4 0"/>
            </svg>
            Scanner / Kiosk Name
          </label>
          <input
            id="scannerName"
            v-model="scannerName"
            type="text"
            :placeholder="isAdmin ? 'Enter scanner name (optional for admin)' : 'Enter scanner name'"
            :required="!isAdmin"
            autocomplete="off"
          />
          <small class="hint" v-if="!isAdmin">
            Check the label on your scanner for its name
          </small>
          <small class="hint" v-else>
            Optional for admins - skip to go directly to dashboard
          </small>
        </div>

        <transition name="fade">
          <div v-if="error" class="error-message">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <circle cx="12" cy="12" r="10"/>
              <line x1="15" y1="9" x2="9" y2="15"/>
              <line x1="9" y1="9" x2="15" y2="15"/>
            </svg>
            {{ error }}
          </div>
        </transition>

        <div class="button-group">
          <button type="submit" class="primary-btn" :disabled="loading">
            <span v-if="!loading">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <polyline points="20 6 9 17 4 12"/>
              </svg>
              Continue
            </span>
            <span v-else class="loading-spinner">
              <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <circle cx="12" cy="12" r="10" opacity="0.25"/>
                <path d="M12 2a10 10 0 0 1 10 10" stroke-linecap="round">
                  <animateTransform attributeName="transform" type="rotate" from="0 12 12" to="360 12 12" dur="1s" repeatCount="indefinite"/>
                </path>
              </svg>
              Connecting...
            </span>
          </button>

          <button 
            v-if="isAdmin" 
            type="button" 
            class="secondary-btn" 
            @click="skipToAdmin"
            :disabled="loading"
          >
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M5 12h14M12 5l7 7-7 7"/>
            </svg>
            Skip to Dashboard
          </button>
        </div>
      </form>

      <div class="logout-section">
        <button type="button" class="logout-btn" @click="handleLogout">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"/>
            <polyline points="16 17 21 12 16 7"/>
            <line x1="21" y1="12" x2="9" y2="12"/>
          </svg>
          Logout &amp; Return to Login
        </button>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/authStore'
import api from '../services/api'

export default {
  name: 'ScannerSelect',
  setup() {
    const scannerName = ref('')
    const error = ref('')
    const loading = ref(false)
    const router = useRouter()
    const authStore = useAuthStore()

    const isAdmin = computed(() => authStore.user?.role === 'Admin')

    const handleBindScanner = async () => {
      error.value = ''

      // For non-admins, scanner is required
      if (!isAdmin.value && !scannerName.value.trim()) {
        error.value = 'Please enter a scanner name to continue'
        return
      }

      // If scanner name is provided, validate and bind
      if (scannerName.value.trim()) {
        loading.value = true
        try {
          const response = await api.post('/auth/bind-scanner', {
            scannerName: scannerName.value.trim()
          })

          // Update auth store with scanner info
          authStore.scannerDeviceId = response.data.scannerDeviceId
          authStore.scannerName = response.data.scannerName
          localStorage.setItem('scannerDeviceId', response.data.scannerDeviceId)
          localStorage.setItem('scannerName', response.data.scannerName)

          // Navigate based on role
          if (isAdmin.value) {
            router.push('/dashboard')
          } else {
            router.push('/kiosk')
          }
        } catch (err) {
          console.error('Failed to bind scanner:', err)
          if (err.response?.status === 404) {
            error.value = err.response?.data?.message || `Scanner "${scannerName.value}" not found`
          } else {
            error.value = err.response?.data?.message || 'Failed to connect to scanner'
          }
        } finally {
          loading.value = false
        }
      } else {
        // Admin skipping scanner
        router.push('/dashboard')
      }
    }

    const skipToAdmin = () => {
      // Admin can skip scanner selection and go directly to dashboard
      router.push('/dashboard')
    }

    const handleLogout = () => {
      authStore.logout()
      router.push('/login')
    }

    return {
      scannerName,
      error,
      loading,
      isAdmin,
      authStore,
      handleBindScanner,
      skipToAdmin,
      handleLogout
    }
  }
}
</script>

<style scoped>
.scanner-select-container {
  display: flex;
  justify-content: center;
  align-items: center;
  width: 100vw;
  height: 100vh;
  background: linear-gradient(135deg, var(--primary-dark) 0%, var(--primary-light) 50%, var(--accent-green) 100%);
  padding: 0;
  margin: 0;
  box-sizing: border-box;
  position: fixed;
  top: 0;
  left: 0;
}

.scanner-select-card {
  width: 100%;
  max-width: 440px;
  margin: 1rem;
  animation: slideUp 0.4s ease-out;
}

@keyframes slideUp {
  from {
    opacity: 0;
    transform: translateY(30px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.logo-section {
  text-align: center;
  margin-bottom: 1.5rem;
}

.logo-image {
  height: 80px;
  width: auto;
  margin-bottom: 0.5rem;
}

.subtitle {
  color: var(--text-secondary);
  margin: 0;
  font-size: 1.1rem;
  font-weight: 600;
}

.welcome-section {
  margin-bottom: 1.5rem;
  padding: 1rem;
  background: linear-gradient(135deg, rgba(30, 144, 255, 0.08) 0%, rgba(50, 205, 50, 0.08) 100%);
  border-radius: 10px;
  border: 1px solid var(--border-color);
}

.user-greeting {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  color: var(--text-primary);
  font-size: 1rem;
}

.user-greeting svg {
  color: var(--primary-light);
}

.user-greeting strong {
  color: var(--primary-dark);
}

.scanner-form {
  margin-bottom: 1.5rem;
}

.form-group {
  margin-bottom: 1.5rem;
}

.form-group label {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin-bottom: 0.6rem;
  color: var(--text-primary);
  font-weight: 600;
  font-size: 0.9rem;
}

.form-group label svg {
  opacity: 0.7;
  color: var(--primary-light);
}

.form-group input {
  width: 100%;
  padding: 0.9rem 1rem;
  border: 2px solid var(--border-color);
  border-radius: 10px;
  font-size: 1rem;
  transition: all 0.3s ease;
  box-sizing: border-box;
  background: var(--bg-primary);
  color: var(--text-primary);
}

.form-group input:focus {
  outline: none;
  border-color: var(--primary-light);
  background: var(--bg-primary);
  box-shadow: 0 0 0 4px rgba(30, 144, 255, 0.1);
}

.form-group input::placeholder {
  color: var(--text-tertiary);
}

.hint {
  display: block;
  margin-top: 0.4rem;
  font-size: 0.8rem;
  color: var(--text-tertiary);
}

.button-group {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.primary-btn {
  width: 100%;
  padding: 1rem;
  background: linear-gradient(135deg, var(--primary-dark) 0%, var(--primary-light) 100%);
  color: white;
  border: none;
  border-radius: 10px;
  font-size: 1rem;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.3s ease;
  box-shadow: 0 4px 12px rgba(30, 144, 255, 0.3);
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
}

.primary-btn:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(30, 144, 255, 0.4);
}

.primary-btn:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

.secondary-btn {
  width: 100%;
  padding: 0.9rem;
  background: transparent;
  color: var(--text-secondary);
  border: 2px solid var(--border-color);
  border-radius: 10px;
  font-size: 0.95rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
}

.secondary-btn:hover:not(:disabled) {
  border-color: var(--primary-light);
  color: var(--primary-light);
  background: rgba(30, 144, 255, 0.05);
}

.secondary-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.loading-spinner {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.error-message {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  background: rgba(255, 107, 107, 0.1);
  color: var(--error-color);
  padding: 1rem;
  border-radius: 10px;
  margin-bottom: 1rem;
  font-size: 0.9rem;
  font-weight: 500;
  border: 2px solid var(--error-color);
}

.logout-section {
  padding-top: 1.5rem;
  border-top: 1px solid var(--border-color);
  text-align: center;
}

.logout-btn {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  background: transparent;
  border: none;
  color: var(--text-tertiary);
  font-size: 0.85rem;
  font-weight: 500;
  cursor: pointer;
  padding: 0.5rem 1rem;
  border-radius: 6px;
  transition: all 0.2s ease;
}

.logout-btn:hover {
  color: var(--error-color);
  background: rgba(255, 107, 107, 0.1);
}

.fade-enter-active,
.fade-leave-active {
  transition: all 0.3s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
  transform: translateY(-10px);
}

@media (max-width: 600px) {
  .scanner-select-card {
    margin: 0.5rem;
  }
  
  .logo-image {
    height: 60px;
  }
}
</style>
