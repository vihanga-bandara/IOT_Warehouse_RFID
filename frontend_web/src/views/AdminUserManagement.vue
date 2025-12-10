<template>
  <div class="users-page">
    <div class="users-container">
      <div class="page-header">
        <div>
          <h1>User Management</h1>
          <p class="page-subtitle">Add and manage warehouse users</p>
        </div>
        <button @click="showRegisterForm = true" class="add-user-btn">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M12 5v14M5 12h14"/>
          </svg>
          Add New User
        </button>
      </div>

      <nav class="admin-nav">
        <router-link to="/dashboard" class="nav-link">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/><polyline points="9 22 9 12 15 12 15 22"/>
          </svg>
          Dashboard
        </router-link>
        <router-link to="/admin/transactions" class="nav-link">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M3 12a9 9 0 1 0 18 0 9 9 0 0 0-18 0z"/><path d="M12 6v6l4 2"/>
          </svg>
          Transactions
        </router-link>
        <router-link to="/admin/users" class="nav-link active">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/>
          </svg>
          Users
        </router-link>
      </nav>

      <transition name="modal-fade">
        <div v-if="showRegisterForm" class="modal-overlay">
          <div class="form-modal" @click.stop>
            <button class="modal-close" @click="showRegisterForm = false">
              <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
              </svg>
            </button>
            <h2>Register New User</h2>
            <form @submit.prevent="registerUser" class="register-form">
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
                  v-model="newUser.email"
                  type="email"
                  placeholder="user@warehouse.com"
                  required
                  class="form-input"
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
                  v-model="newUser.password"
                  type="password"
                  placeholder="Enter password"
                  required
                  class="form-input"
                />
              </div>

              <div class="form-row">
                <div class="form-group">
                  <label for="name">
                    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                      <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/>
                    </svg>
                    First Name
                  </label>
                  <input
                    id="name"
                    v-model="newUser.name"
                    type="text"
                    placeholder="John"
                    required
                    class="form-input"
                  />
                </div>

                <div class="form-group">
                  <label for="lastname">Last Name</label>
                  <input
                    id="lastname"
                    v-model="newUser.lastname"
                    type="text"
                    placeholder="Doe"
                    required
                    class="form-input"
                  />
                </div>
              </div>

              <div class="form-buttons">
                <button type="submit" class="btn btn-primary">
                  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <polyline points="20 6 9 17 4 12"/>
                  </svg>
                  Register User
                </button>
                <button type="button" @click="showRegisterForm = false" class="btn btn-secondary">
                  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
                  </svg>
                  Cancel
                </button>
              </div>
            </form>
          </div>
        </div>
      </transition>

      <div v-if="loading" class="loading-state">
        <div class="spinner"></div>
        <p>Loading users...</p>
      </div>

      <div v-else class="users-section">
        <div class="users-grid">
          <div v-for="user in users" :key="user.id" class="user-card">
            <div class="user-badge">{{ user.role }}</div>
            <h3 class="user-name">{{ user.name }} {{ user.lastname }}</h3>
            <div class="user-details">
              <div class="detail-item">
                <span class="detail-icon">
                  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"/><polyline points="22,6 12,13 2,6"/>
                  </svg>
                </span>
                <span>{{ user.email }}</span>
              </div>
              <div class="detail-item">
                <span class="detail-icon">
                  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <path d="M3 4a1 1 0 0 1 1-1h16a1 1 0 0 1 1 1v2.586a1 1 0 0 1-.293.707l-6.414 6.414a1 1 0 0 0-.293.707V17l-4 4v-6.586a1 1 0 0 0-.293-.707L3.293 7.293A1 1 0 0 1 3 6.586V4z"/>
                  </svg>
                </span>
                <span class="mono">{{ user.rfidUid || 'Not assigned' }}</span>
              </div>
            </div>
            <div class="user-footer">
              <small>ID: {{ user.id }}</small>
            </div>
          </div>
        </div>

        <div v-if="users.length === 0" class="empty-state">
          <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
            <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/>
          </svg>
          <p>No users yet. Create one using the "Add New User" button.</p>
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
        const response = await api.get('/items')
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
.users-page {
  width: 100%;
  min-height: 100vh;
  background: var(--gradient-page-bg);
  padding: 2rem 1rem;
}

.users-container {
  max-width: 1400px;
  margin: 0 auto;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2.5rem;
  animation: slideDown 0.4s ease-out;
}

@keyframes slideDown {
  from {
    opacity: 0;
    transform: translateY(-20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.page-header h1 {
  color: var(--primary-dark);
  margin: 0 0 0.5rem;
  font-size: 2.5rem;
  font-weight: 800;
  letter-spacing: -0.5px;
}

.page-subtitle {
  color: var(--accent-gray);
  margin: 0;
  font-size: 1.1rem;
  font-weight: 500;
}

.add-user-btn {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.75rem 1.5rem;
  background: linear-gradient(135deg, var(--primary-dark) 0%, var(--primary-light) 100%);
  color: white;
  border: none;
  border-radius: 10px;
  font-weight: 700;
  font-size: 0.95rem;
  cursor: pointer;
  transition: all 0.3s ease;
  box-shadow: 0 4px 12px rgba(30, 144, 255, 0.3);
}

.add-user-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(30, 144, 255, 0.4);
}

.admin-nav {
  display: flex;
  gap: 0.5rem;
  margin-bottom: 2rem;
  border-bottom: 2px solid rgba(30, 144, 255, 0.1);
  padding-bottom: 1rem;
}

.nav-link {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.75rem 1.5rem;
  color: var(--accent-gray);
  text-decoration: none;
  border-bottom: 3px solid transparent;
  transition: all 0.3s ease;
  font-weight: 600;
  font-size: 0.95rem;
  border-radius: 8px 8px 0 0;
}

.nav-link svg {
  opacity: 0.7;
  transition: opacity 0.3s;
}

.nav-link:hover {
  color: var(--primary-light);
}

.nav-link:hover svg {
  opacity: 1;
}

.nav-link.active {
  color: var(--primary-light);
  border-bottom-color: var(--primary-light);
  background: rgba(30, 144, 255, 0.05);
}

.nav-link.active svg {
  opacity: 1;
}

@media (max-width: 600px) {
  .page-header {
    flex-direction: column;
    align-items: stretch;
    gap: 1rem;
  }

  .page-header h1 {
    font-size: 1.8rem;
  }

  .page-subtitle {
    font-size: 0.95rem;
  }

  .add-user-btn {
    width: 100%;
    justify-content: center;
  }

  .admin-nav {
    gap: 0;
    overflow-x: auto;
    -webkit-overflow-scrolling: touch;
  }
  
  .nav-link {
    padding: 0.6rem 1rem;
    font-size: 0.85rem;
    white-space: nowrap;
  }
}

@media (max-width: 400px) {
  .page-header h1 {
    font-size: 1.4rem;
  }

  .page-subtitle {
    font-size: 0.85rem;
  }
}

.modal-overlay {
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
  padding: 1rem;
  animation: fadeIn 0.3s ease-out;
}

@keyframes fadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}

.modal-fade-enter-active,
.modal-fade-leave-active {
  transition: all 0.3s ease;
}

.modal-fade-enter-from,
.modal-fade-leave-to {
  opacity: 0;
}

.form-modal {
  background: var(--bg-secondary);
  padding: 2.5rem;
  border-radius: 16px;
  box-shadow: 0 20px 60px rgba(0, 61, 107, 0.25);
  width: 100%;
  max-width: 500px;
  position: relative;
  animation: slideUp 0.3s ease-out;
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

.modal-close {
  position: absolute;
  top: 1rem;
  right: 1rem;
  background: none;
  border: none;
  cursor: pointer;
  color: var(--accent-gray);
  transition: all 0.3s ease;
  width: 40px;
  height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 8px;
}

.modal-close:hover {
  background: var(--border-color);
  color: var(--primary-dark);
}

.form-modal h2 {
  color: var(--primary-dark);
  margin: 0 0 1.5rem;
  font-size: 1.5rem;
  font-weight: 700;
}

.register-form {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.form-group label {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  color: var(--dark-text);
  font-weight: 600;
  font-size: 0.9rem;
}

.form-group label svg {
  opacity: 0.7;
  color: var(--primary-light);
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
}

@media (max-width: 500px) {
  .form-row {
    grid-template-columns: 1fr;
  }
}

.form-input {
  padding: 0.9rem 1rem;
  border: 2px solid var(--border-color);
  border-radius: 10px;
  font-size: 1rem;
  transition: all 0.3s ease;
  background: white;
  color: var(--dark-text);
  box-sizing: border-box;
}

.form-input:focus {
  outline: none;
  border-color: var(--primary-light);
  background: #fafbfc;
  box-shadow: 0 0 0 4px rgba(30, 144, 255, 0.1);
}

.form-input::placeholder {
  color: var(--text-tertiary);
}

@media (max-width: 600px) {
  .form-input {
    padding: 0.85rem 0.9rem;
    font-size: 16px;
  }
}

.form-buttons {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
  margin-top: 1rem;
}

.btn {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  padding: 0.9rem 1.5rem;
  border: none;
  border-radius: 10px;
  font-weight: 700;
  font-size: 0.9rem;
  cursor: pointer;
  transition: all 0.3s ease;
  text-decoration: none;
}

.btn-primary {
  background: linear-gradient(135deg, var(--primary-dark) 0%, var(--primary-light) 100%);
  color: white;
  box-shadow: 0 4px 12px rgba(30, 144, 255, 0.3);
}

.btn-primary:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(30, 144, 255, 0.4);
}

.btn-secondary {
  background: var(--border-color);
  color: var(--dark-text);
}

.btn-secondary:hover {
  background: #d0d5dd;
  color: var(--primary-dark);
}

.loading-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 3rem;
  background: var(--bg-secondary);
  border-radius: 16px;
  box-shadow: 0 4px 16px rgba(0, 61, 107, 0.08);
}

.spinner {
  width: 48px;
  height: 48px;
  border: 4px solid var(--border-color);
  border-top-color: var(--primary-light);
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin-bottom: 1rem;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.loading-state p {
  color: var(--accent-gray);
  font-weight: 600;
}

.users-section {
  animation: slideUp 0.4s ease-out;
}

.users-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
  gap: 1.5rem;
  margin-bottom: 2rem;
}

.user-card {
  background: white;
  border: 2px solid var(--border-color);
  border-radius: 12px;
  padding: 1.5rem;
  position: relative;
  overflow: hidden;
  transition: all 0.3s ease;
  box-shadow: 0 2px 8px rgba(0, 61, 107, 0.06);
}

.user-card::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 4px;
  background: linear-gradient(90deg, var(--primary-dark) 0%, var(--primary-light) 100%);
}

.user-card:hover {
  border-color: var(--primary-light);
  box-shadow: 0 8px 24px rgba(30, 144, 255, 0.15);
  transform: translateY(-2px);
}

.user-badge {
  display: inline-block;
  background: linear-gradient(135deg, var(--accent-green) 0%, #45a049 100%);
  color: white;
  padding: 0.4rem 0.8rem;
  border-radius: 6px;
  font-size: 0.75rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  margin-bottom: 0.8rem;
}

.user-name {
  color: var(--primary-dark);
  margin: 0 0 1rem;
  font-size: 1.2rem;
  font-weight: 700;
  line-height: 1.4;
}

.user-details {
  display: flex;
  flex-direction: column;
  gap: 0.8rem;
  margin-bottom: 1rem;
  padding-bottom: 1rem;
  border-bottom: 1px solid var(--border-color);
}

.detail-item {
  display: flex;
  align-items: center;
  gap: 0.6rem;
  font-size: 0.9rem;
  color: var(--dark-text);
}

.detail-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 28px;
  height: 28px;
  background: linear-gradient(135deg, rgba(30, 144, 255, 0.1) 0%, rgba(80, 200, 120, 0.1) 100%);
  border-radius: 6px;
  color: var(--primary-light);
  flex-shrink: 0;
}

.mono {
  font-family: 'Monaco', 'Menlo', monospace;
  font-size: 0.85rem;
  background: var(--bg-light);
  padding: 0.2rem 0.4rem;
  border-radius: 4px;
}

.user-footer {
  color: var(--accent-gray);
  font-size: 0.8rem;
  font-weight: 500;
}

.empty-state {
  text-align: center;
  color: var(--accent-gray);
  padding: 3rem 2rem;
  background: white;
  border-radius: 16px;
  border: 2px dashed var(--border-color);
  box-shadow: 0 4px 16px rgba(0, 61, 107, 0.08);
}

.empty-state svg {
  opacity: 0.5;
  margin-bottom: 1rem;
}

.empty-state p {
  margin: 0;
  font-size: 1.1rem;
  font-weight: 600;
}

@media (max-width: 600px) {
  .users-grid {
    grid-template-columns: 1fr;
  }

  .form-modal {
    padding: 2rem 1.5rem;
  }

  .form-buttons {
    grid-template-columns: 1fr;
  }
}
</style>
