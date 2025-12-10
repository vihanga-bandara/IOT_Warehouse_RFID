<template>
  <div class="login-container">
    <div class="login-card surface-card surface-card--padded">
      <div class="logo-section">
        <img src="/tooltrack-logo.png" alt="ToolTrackPro Logo" class="logo-image" />
        <p class="subtitle">Smart RFID Warehouse Management</p>
      </div>

      <form @submit.prevent="handleLogin" class="login-form">
        <div class="form-group">
          <label for="email">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"/>
              <polyline points="22,6 12,13 2,6"/>
            </svg>
            Email Address
          </label>
          <input
            id="email"
            v-model="email"
            type="email"
            required
            placeholder="your.email@company.com"
            autocomplete="email"
          />
        </div>

        <div class="form-group">
          <label for="password">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <rect x="3" y="11" width="18" height="11" rx="2" ry="2"/>
              <path d="M7 11V7a5 5 0 0 1 10 0v4"/>
            </svg>
            Password
          </label>
          <input
            id="password"
            v-model="password"
            type="password"
            required
            placeholder="Enter your password"
            autocomplete="current-password"
          />
        </div>

        <button type="submit" class="login-btn" :disabled="loading">
          <span v-if="!loading">Sign In</span>
          <span v-else class="loading-spinner">
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <circle cx="12" cy="12" r="10" opacity="0.25"/>
              <path d="M12 2a10 10 0 0 1 10 10" stroke-linecap="round">
                <animateTransform attributeName="transform" type="rotate" from="0 12 12" to="360 12 12" dur="1s" repeatCount="indefinite"/>
              </path>
            </svg>
            Signing in...
          </span>
        </button>
      </form>

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

      <div class="divider"></div>

      <div class="test-credentials">
        <div class="credentials-header">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <circle cx="12" cy="12" r="10"/>
            <line x1="12" y1="16" x2="12" y2="12"/>
            <line x1="12" y1="8" x2="12.01" y2="8"/>
          </svg>
          <strong>Demo Credentials</strong>
        </div>
        <div class="credential-item">
          <span class="badge badge-admin">Admin</span>
          <code>admin@warehouse.com / password123</code>
        </div>
        <div class="credential-item">
          <span class="badge badge-user">User</span>
          <code>john.doe@warehouse.com / password123</code>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/authStore'

export default {
  name: 'Login',
  setup() {
    const email = ref('')
    const password = ref('')
    const error = ref('')
    const loading = ref(false)
    const router = useRouter()
    const authStore = useAuthStore()

    const handleLogin = async () => {
      error.value = ''
      loading.value = true

      try {
        await authStore.login(email.value, password.value)
        // Redirect based on user role
        if (authStore.user?.role === 'Admin') {
          router.push('/dashboard')
        } else {
          router.push('/kiosk')
        }
      } catch (err) {
        error.value = err.response?.data?.message || 'Login failed. Please check your credentials.'
      } finally {
        loading.value = false
      }
    }

    return {
      email,
      password,
      error,
      loading,
      handleLogin
    }
  }
}
</script>

<style scoped>
.login-container {
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

.login-card {
  width: 100%;
  max-width: 440px;
  margin: 1rem;
  animation: slideUp 0.4s ease-out;
}

@media (max-width: 600px) {
  .login-card {
    padding: 2rem 1.5rem;
    margin: 0.5rem;
    border-radius: 12px;
  }
}

@media (max-width: 400px) {
  .login-card {
    padding: 1.5rem 1rem;
    margin: 0.25rem;
  }
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
  margin-bottom: 2.5rem;
}

.logo-image {
  height: 120px;
  width: auto;
  margin-bottom: 1rem;
  animation: bounce 0.6s ease-out;
}

@media (max-width: 600px) {
  .logo-image {
    height: 100px;
    margin-bottom: 0.8rem;
  }
}

@media (max-width: 400px) {
  .logo-image {
    height: 80px;
    margin-bottom: 0.6rem;
  }
}

@keyframes bounce {
  0% {
    transform: scale(0.8);
    opacity: 0;
  }
  50% {
    transform: scale(1.05);
  }
  100% {
    transform: scale(1);
    opacity: 1;
  }
}

.logo-icon {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 80px;
  height: 80px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border-radius: 20px;
  color: white;
  margin-bottom: 1rem;
  box-shadow: 0 8px 24px rgba(102, 126, 234, 0.4);
}

.login-card h1 {
  color: var(--dark-text);
  margin: 0 0 0.5rem;
  font-size: 2rem;
  font-weight: 700;
  letter-spacing: -0.5px;
}

.subtitle {
  color: var(--accent-gray);
  margin: 0;
  font-size: 1rem;
  font-weight: 500;
}

@media (max-width: 600px) {
  .login-card h1 {
    font-size: 1.6rem;
  }
  
  .subtitle {
    font-size: 0.9rem;
  }
}

@media (max-width: 400px) {
  .login-card h1 {
    font-size: 1.4rem;
  }
  
  .subtitle {
    font-size: 0.85rem;
  }
}

.login-form {
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
  color: var(--dark-text);
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

@media (max-width: 600px) {
  .form-group input {
    padding: 0.85rem 0.9rem;
    font-size: 16px;
  }
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

.login-btn {
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

@media (max-width: 600px) {
  .login-btn {
    padding: 0.9rem;
    font-size: 0.95rem;
  }
}

.login-btn:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(30, 144, 255, 0.4);
}

.login-btn:active:not(:disabled) {
  transform: translateY(0);
}

.login-btn:disabled {
  opacity: 0.7;
  cursor: not-allowed;
  transform: none;
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

.fade-enter-active, .fade-leave-active {
  transition: all 0.3s ease;
}

.fade-enter-from, .fade-leave-to {
  opacity: 0;
  transform: translateY(-10px);
}

.divider {
  height: 1px;
  background: linear-gradient(to right, transparent, var(--border-color), transparent);
  margin: 2rem 0 1.5rem;
}

.test-credentials {
  background: linear-gradient(135deg, var(--bg-light) 0%, var(--bg-lighter) 100%);
  padding: 1.2rem;
  border-radius: 10px;
  font-size: 0.85rem;
  border: 2px solid var(--border-color);
}

@media (max-width: 600px) {
  .test-credentials {
    padding: 1rem;
    font-size: 0.8rem;
  }
}

@media (max-width: 400px) {
  .test-credentials {
    padding: 0.8rem;
    font-size: 0.75rem;
  }
}

.credentials-header {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin-bottom: 1rem;
  color: var(--primary-dark);
}

.credentials-header svg {
  color: var(--primary-light);
}

.credentials-header strong {
  font-weight: 700;
  font-size: 0.9rem;
}

.credential-item {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.6rem;
  margin-bottom: 0.5rem;
  background: var(--bg-primary);
  border-radius: 6px;
  border: 1px solid var(--border-color);
}

.credential-item:last-child {
  margin-bottom: 0;
}

.credential-item code {
  font-family: 'Monaco', 'Menlo', monospace;
  font-size: 0.8rem;
  color: var(--text-primary);
  background: var(--bg-tertiary);
  padding: 0.2rem 0.5rem;
  border-radius: 4px;
  flex: 1;
}

.badge {
  display: inline-block;
  padding: 0.3rem 0.6rem;
  border-radius: 6px;
  font-weight: 700;
  font-size: 0.7rem;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  min-width: 60px;
  text-align: center;
}

.badge-admin {
  background: linear-gradient(135deg, var(--primary-dark) 0%, var(--primary-light) 100%);
  color: white;
}

.badge-user {
  background: linear-gradient(135deg, var(--accent-green) 0%, #45a049 100%);
  color: white;
}

@media (max-width: 500px) {
  .login-card {
    padding: 2rem 1.5rem;
  }
  
  .login-card h1 {
    font-size: 1.6rem;
  }
}
</style>
