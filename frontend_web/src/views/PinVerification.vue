<template>
  <div class="login-container">
    <div class="login-card surface-card surface-card--padded">
      <div class="logo-section">
        <img src="/tooltrack-logo.png" alt="ToolTrackPro Logo" class="logo-image" />
        <p class="subtitle">PIN Verification Required</p>
      </div>

      <div class="pin-section">
        <div class="pin-prompt">
          <div class="pin-icon-container">
            <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
              <rect x="3" y="11" width="18" height="11" rx="2" ry="2"/>
              <path d="M7 11V7a5 5 0 0 1 10 0v4"/>
              <circle cx="12" cy="16" r="1"/>
            </svg>
          </div>
          <h2 class="pin-title">Enter Your PIN</h2>
          <p class="pin-subtitle">
            Welcome back, <strong>{{ userName }}</strong>
          </p>
        </div>

        <form @submit.prevent="handleVerifyPin" class="pin-form">
          <div class="pin-input-group">
            <div class="pin-inputs">
              <input
                v-for="(digit, index) in 4"
                :key="index"
                ref="pinInputs"
                type="password"
                inputmode="numeric"
                pattern="[0-9]"
                maxlength="1"
                class="pin-digit"
                :class="{ 'error': hasError }"
                v-model="pinDigits[index]"
                @input="handlePinInput($event, index)"
                @keydown="handlePinKeydown($event, index)"
                @paste="handlePaste($event)"
                :disabled="loading || locked"
                autocomplete="off"
              />
            </div>
            <p v-if="remainingAttempts !== null && remainingAttempts > 0" class="attempts-warning">
              {{ remainingAttempts }} attempt{{ remainingAttempts === 1 ? '' : 's' }} remaining
            </p>
          </div>

          <button type="submit" class="verify-btn" :disabled="!isComplete || loading || locked">
            <span v-if="!loading">Verify PIN</span>
            <span v-else class="loading-spinner">
              <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <circle cx="12" cy="12" r="10" opacity="0.25"/>
                <path d="M12 2a10 10 0 0 1 10 10" stroke-linecap="round">
                  <animateTransform attributeName="transform" type="rotate" from="0 12 12" to="360 12 12" dur="1s" repeatCount="indefinite"/>
                </path>
              </svg>
              Verifying...
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

        <div class="login-mode-switch">
          <span class="switch-text">or</span>
          <button type="button" class="switch-btn" @click="cancelAndGoBack">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M19 12H5M12 19l-7-7 7-7"/>
            </svg>
            Use different login method
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, computed, onMounted, nextTick, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/authStore'

export default {
  name: 'PinVerification',
  setup() {
    const router = useRouter()
    const authStore = useAuthStore()
    
    const pinDigits = ref(['', '', '', ''])
    const pinInputs = ref([])
    const loading = ref(false)
    const error = ref('')
    const hasError = ref(false)
    const remainingAttempts = ref(null)
    const locked = ref(false)

    const userName = computed(() => {
      if (authStore.pendingMfaData) {
        return authStore.pendingMfaData.name || 'User'
      }
      return 'User'
    })

    const isComplete = computed(() => {
      return pinDigits.value.every(d => d !== '')
    })

    const handlePinInput = (event, index) => {
      const value = event.target.value
      
      // Only allow digits
      if (!/^\d*$/.test(value)) {
        pinDigits.value[index] = ''
        return
      }

      // Clear error on new input
      if (hasError.value) {
        hasError.value = false
        error.value = ''
      }

      // Move to next input if a digit was entered
      if (value && index < 3) {
        nextTick(() => {
          pinInputs.value[index + 1]?.focus()
        })
      }
    }

    const handlePinKeydown = (event, index) => {
      // Handle backspace
      if (event.key === 'Backspace' && !pinDigits.value[index] && index > 0) {
        nextTick(() => {
          pinInputs.value[index - 1]?.focus()
        })
      }
      
      // Handle arrow keys
      if (event.key === 'ArrowLeft' && index > 0) {
        pinInputs.value[index - 1]?.focus()
      }
      if (event.key === 'ArrowRight' && index < 3) {
        pinInputs.value[index + 1]?.focus()
      }
    }

    const handlePaste = (event) => {
      event.preventDefault()
      const pastedText = event.clipboardData.getData('text')
      const digits = pastedText.replace(/\D/g, '').slice(0, 4).split('')
      
      digits.forEach((digit, index) => {
        if (index < 4) {
          pinDigits.value[index] = digit
        }
      })
      
      // Focus the appropriate input
      const focusIndex = Math.min(digits.length, 3)
      nextTick(() => {
        pinInputs.value[focusIndex]?.focus()
      })
    }

    const handleVerifyPin = async () => {
      if (!isComplete.value || loading.value || locked.value) return

      loading.value = true
      error.value = ''
      hasError.value = false

      const pin = pinDigits.value.join('')

      try {
        const result = await authStore.verifyPin(pin)

        if (result.success) {
          // Login complete - redirect based on role
          const hasAdminRole = authStore.user?.role === 'Admin'
          if (hasAdminRole) {
            router.push('/dashboard')
          } else {
            router.push('/kiosk')
          }
        } else {
          // PIN verification failed
          hasError.value = true
          error.value = result.error || 'Incorrect PIN'
          remainingAttempts.value = result.remainingAttempts ?? null
          
          if (result.locked) {
            locked.value = true
            // Redirect back to login after a short delay
            setTimeout(() => {
              authStore.clearPendingMfa()
              router.push('/login')
            }, 3000)
          } else {
            // Clear PIN inputs for retry
            pinDigits.value = ['', '', '', '']
            nextTick(() => {
              pinInputs.value[0]?.focus()
            })
          }
        }
      } catch (err) {
        console.error('PIN verification error:', err)
        error.value = 'An error occurred. Please try again.'
        hasError.value = true
      } finally {
        loading.value = false
      }
    }

    // Auto-submit when PIN is fully entered (4 digits)
    watch(isComplete, (complete) => {
      if (!complete) return
      if (loading.value || locked.value) return

      // Let v-model settle and avoid fighting focus transitions
      nextTick(() => {
        handleVerifyPin()
      })
    })

    const cancelAndGoBack = () => {
      authStore.clearPendingMfa()
      router.push('/login')
    }

    onMounted(() => {
      // Redirect to login if there's no pending MFA data
      if (!authStore.pendingMfaData) {
        router.push('/login')
        return
      }

      // Focus first PIN input
      nextTick(() => {
        pinInputs.value[0]?.focus()
      })
    })

    return {
      pinDigits,
      pinInputs,
      loading,
      error,
      hasError,
      remainingAttempts,
      locked,
      userName,
      isComplete,
      handlePinInput,
      handlePinKeydown,
      handlePaste,
      handleVerifyPin,
      cancelAndGoBack
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
  margin-bottom: 2rem;
}

.logo-image {
  height: 100px;
  width: auto;
  margin-bottom: 1rem;
}

.subtitle {
  color: var(--accent-gray);
  margin: 0;
  font-size: 1rem;
  font-weight: 500;
}

/* PIN Section Styles */
.pin-section {
  text-align: center;
}

.pin-prompt {
  padding: 1rem 0;
}

.pin-icon-container {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 80px;
  height: 80px;
  background: linear-gradient(135deg, var(--primary-dark) 0%, var(--primary-light) 100%);
  border-radius: 50%;
  color: white;
  margin-bottom: 1rem;
  box-shadow: 0 8px 32px rgba(30, 144, 255, 0.3);
}

.pin-title {
  font-size: 1.5rem;
  font-weight: 700;
  color: var(--text-primary);
  margin: 0 0 0.5rem;
}

.pin-subtitle {
  font-size: 0.95rem;
  color: var(--text-secondary);
  margin: 0;
}

.pin-subtitle strong {
  color: var(--primary-light);
}

/* PIN Input Styles */
.pin-form {
  margin-top: 1.5rem;
}

.pin-input-group {
  margin-bottom: 1.5rem;
}

.pin-inputs {
  display: flex;
  justify-content: center;
  gap: 0.75rem;
}

.pin-digit {
  width: 56px;
  height: 64px;
  text-align: center;
  font-size: 1.5rem;
  font-weight: 700;
  padding: 0;
  line-height: 64px;
  -webkit-appearance: none;
  appearance: none;
  border: 2px solid var(--border-color);
  border-radius: 12px;
  background: var(--bg-primary);
  color: var(--text-primary);
  transition: all 0.2s ease;
  caret-color: var(--primary-light);
}

.pin-digit:focus {
  outline: none;
  border-color: var(--primary-light);
  box-shadow: 0 0 0 4px rgba(30, 144, 255, 0.15);
}

.pin-digit.error {
  border-color: var(--error-color);
  animation: shake 0.4s ease-in-out;
}

@keyframes shake {
  0%, 100% { transform: translateX(0); }
  20%, 60% { transform: translateX(-6px); }
  40%, 80% { transform: translateX(6px); }
}

.pin-digit:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.attempts-warning {
  margin-top: 0.75rem;
  font-size: 0.85rem;
  color: var(--warning-color, #f59e0b);
  font-weight: 500;
}

/* Verify Button */
.verify-btn {
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

.verify-btn:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(30, 144, 255, 0.4);
}

.verify-btn:active:not(:disabled) {
  transform: translateY(0);
}

.verify-btn:disabled {
  opacity: 0.7;
  cursor: not-allowed;
  transform: none;
}

.loading-spinner {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

/* Error Message */
.error-message {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  background: rgba(255, 107, 107, 0.1);
  color: var(--error-color);
  padding: 1rem;
  border-radius: 10px;
  margin-top: 1rem;
  font-size: 0.9rem;
  font-weight: 500;
  border: 2px solid var(--error-color);
}

/* Mode Switch */
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

/* Transitions */
.fade-enter-active, .fade-leave-active {
  transition: all 0.3s ease;
}

.fade-enter-from, .fade-leave-to {
  opacity: 0;
  transform: translateY(-10px);
}

/* Responsive */
@media (max-width: 600px) {
  .login-container {
    align-items: flex-start;
    padding: max(0.75rem, env(safe-area-inset-top)) 0.75rem max(0.75rem, env(safe-area-inset-bottom));
  }

  .login-card {
    margin: 0.75rem auto;
  }

  .pin-digit {
    width: 48px;
    height: 56px;
    font-size: 1.25rem;
    line-height: 56px;
  }

  .logo-image {
    height: 80px;
  }
}

@media (max-width: 400px) {
  .pin-inputs {
    gap: 0.5rem;
  }

  .pin-digit {
    width: 44px;
    height: 52px;
    font-size: 1.1rem;
    border-radius: 8px;
    line-height: 52px;
  }
}
</style>
