<template>
  <div class="users-container">
    <div class="users-header">
      <h2>User Management</h2>
      <button @click="showRegisterForm = true" class="btn btn-primary">Add New User</button>
    </div>

    <div v-if="showRegisterForm" class="register-form-modal">
      <div class="form-card">
        <h3>Register New User</h3>
        <form @submit.prevent="registerUser" class="register-form">
          <input
            v-model="newUser.email"
            type="email"
            placeholder="Email"
            required
            class="form-input"
          />
          <input
            v-model="newUser.password"
            type="password"
            placeholder="Password"
            required
            class="form-input"
          />
          <input
            v-model="newUser.name"
            type="text"
            placeholder="First Name"
            required
            class="form-input"
          />
          <input
            v-model="newUser.lastname"
            type="text"
            placeholder="Last Name"
            required
            class="form-input"
          />
          <div class="form-buttons">
            <button type="submit" class="btn btn-primary">Register</button>
            <button type="button" @click="showRegisterForm = false" class="btn btn-secondary">
              Cancel
            </button>
          </div>
        </form>
      </div>
    </div>

    <div v-if="loading" class="loading">Loading users...</div>

    <div v-else class="users-grid">
      <div v-for="user in users" :key="user.id" class="user-card">
        <div class="user-role">{{ user.role }}</div>
        <h4>{{ user.name }} {{ user.lastname }}</h4>
        <p><strong>Email:</strong> {{ user.email }}</p>
        <p><strong>RFID:</strong> {{ user.rfidUid || 'Not assigned' }}</p>
        <div class="user-footer">
          <small>ID: {{ user.id }}</small>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, onMounted } from 'vue'
import api from '../services/api'

export default {
  name: 'AdminUserManagement',
  setup() {
    const users = ref([])
    const loading = ref(true)
    const showRegisterForm = ref(false)
    const newUser = ref({
      email: '',
      password: '',
      name: '',
      lastname: ''
    })

    const fetchUsers = async () => {
      try {
        const response = await api.get('/items') // This would typically be /users endpoint
        users.value = response.data
      } catch (err) {
        console.error('Failed to fetch users:', err)
      } finally {
        loading.value = false
      }
    }

    const registerUser = async () => {
      try {
        await api.post('/auth/register', newUser.value)
        newUser.value = { email: '', password: '', name: '', lastname: '' }
        showRegisterForm.value = false
        fetchUsers()
      } catch (err) {
        console.error('Registration failed:', err)
      }
    }

    onMounted(() => {
      fetchUsers()
    })

    return {
      users,
      loading,
      showRegisterForm,
      newUser,
      registerUser
    }
  }
}
</script>

<style scoped>
.users-container {
  max-width: 1200px;
}

.users-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
}

.users-header h2 {
  margin: 0;
  color: #2c3e50;
}

.btn {
  padding: 0.75rem 1.5rem;
  border: none;
  border-radius: 4px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s;
}

.btn-primary {
  background: #27ae60;
  color: white;
}

.btn-primary:hover {
  background: #229954;
}

.btn-secondary {
  background: #95a5a6;
  color: white;
}

.btn-secondary:hover {
  background: #7f8c8d;
}

.register-form-modal {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
}

.form-card {
  background: white;
  padding: 2rem;
  border-radius: 8px;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.2);
  width: 100%;
  max-width: 400px;
}

.form-card h3 {
  margin: 0 0 1.5rem;
  color: #2c3e50;
}

.register-form {
  display: grid;
  gap: 1rem;
}

.form-input {
  padding: 0.75rem;
  border: 1px solid #bdc3c7;
  border-radius: 4px;
  font-size: 0.9rem;
}

.form-input:focus {
  outline: none;
  border-color: #3498db;
  box-shadow: 0 0 0 3px rgba(52, 152, 219, 0.1);
}

.form-buttons {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
  margin-top: 1rem;
}

.loading {
  text-align: center;
  color: #7f8c8d;
  padding: 3rem;
}

.users-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 1.5rem;
}

.user-card {
  background: white;
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  padding: 1.5rem;
  position: relative;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.user-role {
  position: absolute;
  top: 1rem;
  right: 1rem;
  padding: 0.35rem 0.75rem;
  border-radius: 20px;
  font-size: 0.75rem;
  font-weight: 600;
  background: #d4edda;
  color: #155724;
}

.user-card h4 {
  margin: 0 0 1rem;
  color: #2c3e50;
}

.user-card p {
  margin: 0.5rem 0;
  color: #34495e;
  font-size: 0.9rem;
}

.user-footer {
  margin-top: 1rem;
  padding-top: 1rem;
  border-top: 1px solid #ecf0f1;
  color: #95a5a6;
  font-size: 0.8rem;
}
</style>
