<template>
  <div class="login-container">
    <div class="login-card">
      <h1>RFID Warehouse</h1>
      <p class="subtitle">Tool Management System</p>

      <form @submit.prevent="handleLogin" class="login-form">
        <div class="form-group">
          <label for="email">Email</label>
          <input
            id="email"
            v-model="email"
            type="email"
            required
            placeholder="admin@warehouse.com"
          />
        </div>

        <div class="form-group">
          <label for="password">Password</label>
          <input
            id="password"
            v-model="password"
            type="password"
            required
            placeholder="Enter your password"
          />
        </div>

        <button type="submit" class="login-btn" :disabled="loading">
          {{ loading ? 'Logging in...' : 'Login' }}
        </button>
      </form>

      <div v-if="error" class="error-message">
        {{ error }}
      </div>

      <div class="test-credentials">
        <p><strong>Test Credentials:</strong></p>
        <p>Admin: admin@warehouse.com / password123</p>
        <p>User: john.doe@warehouse.com / password123</p>
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
        router.push('/dashboard')
      } catch (err) {
        error.value = err.response?.data?.message || 'Login failed. Please try again.'
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
  min-height: 100vh;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}

.login-card {
  background: white;
  padding: 2.5rem;
  border-radius: 8px;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.2);
  width: 100%;
  max-width: 400px;
}

.login-card h1 {
  text-align: center;
  color: #2c3e50;
  margin: 0 0 0.5rem;
  font-size: 1.8rem;
}

.subtitle {
  text-align: center;
  color: #7f8c8d;
  margin: 0 0 2rem;
  font-size: 0.9rem;
}

.login-form {
  margin-bottom: 1.5rem;
}

.form-group {
  margin-bottom: 1.5rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  color: #2c3e50;
  font-weight: 600;
}

.form-group input {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid #bdc3c7;
  border-radius: 4px;
  font-size: 1rem;
  transition: border-color 0.3s;
  box-sizing: border-box;
}

.form-group input:focus {
  outline: none;
  border-color: #667eea;
  box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
}

.login-btn {
  width: 100%;
  padding: 0.75rem;
  background-color: #667eea;
  color: white;
  border: none;
  border-radius: 4px;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  transition: background-color 0.3s;
}

.login-btn:hover:not(:disabled) {
  background-color: #5568d3;
}

.login-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.error-message {
  background-color: #ffe5e5;
  color: #c0392b;
  padding: 0.75rem;
  border-radius: 4px;
  margin-bottom: 1rem;
  font-size: 0.9rem;
}

.test-credentials {
  background-color: #f0f4ff;
  padding: 1rem;
  border-radius: 4px;
  font-size: 0.85rem;
  color: #2c3e50;
}

.test-credentials p {
  margin: 0.3rem 0;
}

.test-credentials strong {
  display: block;
  margin-bottom: 0.5rem;
}
</style>
