<template>
  <div class="login-container">
    <div class="login-card surface-card surface-card--padded">
      <div class="logo-section">
        <img src="/tooltrack-logo.png" alt="ToolTrackPro Logo" class="logo-image" />
        <p class="subtitle">Smart Tool Tracking System</p>
      </div>

      <transition name="fade">
        <div v-if="infoMessage" class="info-message">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M13 16h-1v-4h-1"/>
            <circle cx="12" cy="12" r="10"/>
          </svg>
          {{ infoMessage }}
        </div>
      </transition>

      <!-- RFID Login Mode (Default) -->
      <div v-if="loginMode === 'rfid'" class="rfid-login-section">
        <div class="rfid-prompt">
          <div class="rfid-scanner-visual">
            <!-- Signal waves animation -->
            <div class="signal-waves" :class="{ 'active': rfidConnected }">
              <div class="wave wave-1"></div>
              <div class="wave wave-2"></div>
              <div class="wave wave-3"></div>
            </div>
            <div class="rfid-icon-container" :class="{ 'scanning': rfidConnected, 'connecting': isConnecting }">
              <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
                <rect x="2" y="6" width="20" height="12" rx="2"/>
                <path d="M12 12m-2 0a2 2 0 1 0 4 0a2 2 0 1 0 -4 0"/>
                <path d="M6 10h.01M6 14h.01"/>
              </svg>
            </div>
          </div>
          <h2 class="rfid-title">Tap Your ID Card</h2>
          <p class="rfid-subtitle">
            <template v-if="isConnecting">
              <span class="connecting-text">Connecting to scanner...</span>
            </template>
            <template v-else-if="rfidConnected">
              <span class="ready-text">
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                  <polyline points="20 6 9 17 4 12"/>
                </svg>
                Ready - Place your card on the scanner
              </span>
            </template>
            <template v-else-if="connectionError">
              <span class="error-text">{{ connectionError }}</span>
            </template>
            <template v-else>
              Initializing scanner connection...
            </template>
          </p>
        </div>

        <div class="login-mode-switch">
          <span class="switch-text">or</span>
          <button type="button" class="switch-btn" @click="switchToCredentials">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"/>
              <polyline points="22,6 12,13 2,6"/>
            </svg>
            Login with email &amp; password
          </button>
        </div>
      </div>

      <!-- Credential Login Mode -->
      <form v-else @submit.prevent="handleLogin" class="login-form">
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
            placeholder="Enter your email"
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
          <div class="password-input-wrapper">
            <input
              id="password"
              v-model="password"
              :type="showPassword ? 'text' : 'password'"
              required
              placeholder="Enter your password"
              autocomplete="current-password"
            />
            <button type="button" class="password-toggle" @click="showPassword = !showPassword" :title="showPassword ? 'Hide password' : 'Show password'">
              <svg v-if="showPassword" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/>
                <circle cx="12" cy="12" r="3"/>
              </svg>
              <svg v-else width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24"/>
                <line x1="1" y1="1" x2="23" y2="23"/>
              </svg>
            </button>
          </div>
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

        <div class="login-mode-switch">
          <span class="switch-text">or</span>
          <button type="button" class="switch-btn" @click="switchToRfid">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
              <rect x="2" y="6" width="20" height="12" rx="2"/>
              <path d="M12 12m-2 0a2 2 0 1 0 4 0a2 2 0 1 0 -4 0"/>
            </svg>
            Login with RFID card
          </button>
        </div>
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
    </div>
  </div>
</template>

<script>
import { ref, onMounted, onBeforeUnmount } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '../stores/authStore'
import api from '../services/api'
import { 
  initLoginSignalR, 
  joinScannerLoginGroup, 
  onRfidLoginSuccess, 
  onLoginFailed,
  offRfidLoginSuccess,
  offLoginFailed,
  closeLoginSignalR 
} from '../services/loginSignalr'

export default {
  name: 'Login',
  setup() {
    const email = ref('')
    const password = ref('')
    const error = ref('')
    const loading = ref(false)
    const showPassword = ref(false)
    const router = useRouter()
    const route = useRoute()
    const authStore = useAuthStore()
    const infoMessage = ref('')
    
    // RFID login state
    const loginMode = ref('rfid') // 'rfid' or 'credentials'
    const rfidConnected = ref(false)
    const isConnecting = ref(false)
    const connectionError = ref('')
    const scannerDeviceId = ref(null)
    const scannerName = ref('')

    const POC_SCANNER_NAME = 'Pris-1'

    const switchToCredentials = () => {
      loginMode.value = 'credentials'
    }

    const switchToRfid = () => {
      loginMode.value = 'rfid'
      // Re-attempt connection if not already connected
      if (!rfidConnected.value && !isConnecting.value) {
        initializeScanner()
      }
    }

    const initializeScanner = async () => {
      isConnecting.value = true
      connectionError.value = ''
      rfidConnected.value = false

      try {
        // POC: use a single known scanner name and resolve its deviceId via anonymous endpoint
        const { data: scanner } = await api.get(`/scanners/by-name/${encodeURIComponent(POC_SCANNER_NAME)}`)
        scannerName.value = scanner.name
        scannerDeviceId.value = scanner.deviceId

        // Initialize SignalR connection for login events
        await initLoginSignalR()
        await joinScannerLoginGroup(scanner.deviceId)

        // Set up event handlers
        onRfidLoginSuccess(handleRfidLoginSuccess)
        onLoginFailed(handleRfidLoginFailed)

        rfidConnected.value = true
        isConnecting.value = false
        error.value = ''
      } catch (err) {
        console.error('Failed to initialize scanner:', err)
        isConnecting.value = false
        rfidConnected.value = false
        if (err.response?.status === 404) {
          connectionError.value = err.response?.data?.message || `Scanner '${POC_SCANNER_NAME}' not found`
        } else {
          connectionError.value = 'Failed to connect to scanner. Please try again.'
        }
      }
    }

    const handleRfidLoginSuccess = (data) => {
      console.log('RFID login success:', data)
      
      // Use setRfidLoginData to store auth data with scanner info
      // This will check if PIN verification is required
      const result = authStore.setRfidLoginData(data)

      // Clean up SignalR listeners
      cleanupRfidListeners()
      
      // Check if PIN verification is required
      if (result.requiresPinVerification) {
        console.log('PIN verification required, redirecting to PIN entry')
        router.push('/verify-pin')
        return
      }
      
      // No PIN required - proceed with normal login flow
      const hasAdminRole = data.roleIds && data.roleIds.includes(1)
      if (hasAdminRole) {
        router.push('/dashboard')
      } else {
        router.push('/kiosk')
      }
    }

    const handleRfidLoginFailed = (data) => {
      console.log('RFID login failed:', data)
      connectionError.value = data.Error || 'RFID login failed. Please try again.'
      // Keep scanner connected for next scan attempt
    }

    const cleanupRfidListeners = () => {
      offRfidLoginSuccess()
      offLoginFailed()
      closeLoginSignalR()
    }

    const handleLogin = async () => {
      error.value = ''
      loading.value = true

      try {
        // Login without scanner - scanner selection happens on the next page
        await authStore.login(email.value, password.value)
        // Always redirect to scanner selection page after credential login
        router.push('/select-scanner')
      } catch (err) {
        // Log full error for debugging
        console.error('Login error response:', err?.response ?? err)
        const serverMessage = err.response?.data?.message
        error.value = serverMessage || 'Login failed. Please check your credentials.'
        // Clear form inputs on login failure
        email.value = ''
        password.value = ''
      } finally {
        loading.value = false
      }
    }

    onMounted(() => {
      // Auto-initialize scanner connection for RFID login
      initializeScanner()
    })

    onBeforeUnmount(() => {
      cleanupRfidListeners()
    })

    return {
      email,
      password,
      error,
      loading,
      showPassword,
      handleLogin,
      infoMessage,
      loginMode,
      rfidConnected,
      isConnecting,
      connectionError,
      switchToCredentials,
      switchToRfid
    }
  }
}
</script>

<style scoped>
.login-container {
  display: flex;
  justify-content: center;
  align-items: center;
  width: 100%;
  min-height: 100vh;
  min-height: 100dvh;
  height: auto;
  background: linear-gradient(135deg, var(--primary-dark) 0%, var(--primary-light) 50%, var(--accent-green) 100%);
  padding: max(1rem, env(safe-area-inset-top)) 1rem max(1rem, env(safe-area-inset-bottom));
  margin: 0;
  box-sizing: border-box;
  overflow-y: auto;
  -webkit-overflow-scrolling: touch;
}

.login-card {
  width: 100%;
  max-width: 440px;
  margin: 1rem auto;
  animation: slideUp 0.4s ease-out;
}

@media (max-width: 600px) {
  .login-container {
    align-items: flex-start;
    padding: max(0.75rem, env(safe-area-inset-top)) 0.75rem max(0.75rem, env(safe-area-inset-bottom));
  }

  .login-card {
    margin: 0.75rem auto;
  }
}

/* Short screens (e.g., landscape / keyboard open): prioritize scrollability */
@media (max-height: 720px) {
  .login-container {
    align-items: flex-start;
  }
}

/* RFID Login Section Styles */
.rfid-login-section {
  text-align: center;
}

.rfid-prompt {
  padding: 1.5rem 0;
}

/* Scanner Visual Container */
.rfid-scanner-visual {
  position: relative;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 160px;
  height: 160px;
  margin-bottom: 1.5rem;
}

/* Signal Waves Animation */
.signal-waves {
  position: absolute;
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
}

.wave {
  position: absolute;
  border: 2px solid rgba(30, 144, 255, 0.3);
  border-radius: 50%;
  opacity: 0;
}

.signal-waves.active .wave {
  animation: ripple 2.5s ease-out infinite;
}

.wave-1 {
  width: 100px;
  height: 100px;
  animation-delay: 0s;
}

.wave-2 {
  width: 100px;
  height: 100px;
  animation-delay: 0.8s;
}

.wave-3 {
  width: 100px;
  height: 100px;
  animation-delay: 1.6s;
}

@keyframes ripple {
  0% {
    width: 100px;
    height: 100px;
    opacity: 0.6;
    border-color: rgba(30, 144, 255, 0.6);
  }
  100% {
    width: 160px;
    height: 160px;
    opacity: 0;
    border-color: rgba(30, 144, 255, 0);
  }
}

.rfid-icon-container {
  position: relative;
  z-index: 2;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 100px;
  height: 100px;
  background: linear-gradient(135deg, var(--primary-dark) 0%, var(--primary-light) 100%);
  border-radius: 50%;
  color: white;
  box-shadow: 0 8px 32px rgba(30, 144, 255, 0.3);
  transition: all 0.3s ease;
}

.rfid-icon-container.connecting {
  animation: pulse-slow 1.5s ease-in-out infinite;
}

.rfid-icon-container.scanning {
  animation: pulse-glow 2s ease-in-out infinite;
  box-shadow: 0 0 40px rgba(30, 144, 255, 0.5), 0 8px 32px rgba(30, 144, 255, 0.3);
}

@keyframes pulse-slow {
  0%, 100% {
    transform: scale(1);
    opacity: 0.7;
  }
  50% {
    transform: scale(1.02);
    opacity: 1;
  }
}

@keyframes pulse-glow {
  0%, 100% {
    transform: scale(1);
    box-shadow: 0 0 30px rgba(30, 144, 255, 0.4), 0 8px 32px rgba(30, 144, 255, 0.3);
  }
  50% {
    transform: scale(1.03);
    box-shadow: 0 0 50px rgba(30, 144, 255, 0.6), 0 12px 48px rgba(30, 144, 255, 0.4);
  }
}

.rfid-title {
  font-size: 1.5rem;
  font-weight: 700;
  color: var(--text-primary);
  margin: 0 0 0.75rem;
}

.rfid-subtitle {
  font-size: 0.95rem;
  color: var(--text-secondary);
  margin: 0 0 1rem;
  min-height: 1.5em;
}

.rfid-subtitle .connecting-text {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  color: var(--text-tertiary);
  animation: pulse-text 1.5s ease-in-out infinite;
}

@keyframes pulse-text {
  0%, 100% { opacity: 0.6; }
  50% { opacity: 1; }
}

.rfid-subtitle .ready-text {
  display: inline-flex;
  align-items: center;
  gap: 0.4rem;
  color: var(--accent-green);
  font-weight: 500;
}

.rfid-subtitle .error-text {
  color: var(--error-color);
}

.login-mode-switch {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.75rem;
  margin-top: 1.5rem;
  padding-top: 1.5rem;
  border-top: 1px solid var(--border-color);
}

.switch-text {
  font-size: 0.85rem;
  color: var(--text-tertiary);
  text-transform: uppercase;
  letter-spacing: 1px;
}

.switch-btn {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  background: transparent;
  border: 2px solid var(--border-color);
  color: var(--text-secondary);
  padding: 0.75rem 1.25rem;
  border-radius: 8px;
  font-size: 0.9rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
}

.switch-btn:hover {
  border-color: var(--primary-light);
  color: var(--primary-light);
  background: rgba(30, 144, 255, 0.05);
}

.hint {
  display: block;
  margin-top: 0.4rem;
  font-size: 0.8rem;
  color: var(--text-tertiary);
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

.info-message {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  background: rgba(30, 144, 255, 0.06);
  color: var(--primary-dark);
  padding: 0.9rem 1rem;
  border-radius: 10px;
  margin-bottom: 1rem;
  border: 1px solid rgba(30, 144, 255, 0.12);
  font-size: 0.95rem;
  font-weight: 600;
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

.password-input-wrapper {
  position: relative;
  display: flex;
  align-items: center;
}

.password-input-wrapper input {
  width: 100%;
  padding-right: 2.5rem;
}

.password-toggle {
  position: absolute;
  right: 1rem;
  background: none;
  border: none;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--text-tertiary);
  padding: 0.5rem;
  transition: color 0.2s ease;
}

.password-toggle:hover {
  color: var(--primary-light);
}

.password-toggle svg {
  width: 20px;
  height: 20px;
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
