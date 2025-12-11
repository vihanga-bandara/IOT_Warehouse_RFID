<template>
  <AdminShell
    title="User Management"
    subtitle="Add and manage warehouse users"
    active-tab="users"
  >
    <div class="users-page-content">
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

              <div class="form-group roles-group">
                <label class="roles-label">
                  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <circle cx="12" cy="7" r="4"/>
                    <path d="M5.5 21a6.5 6.5 0 0 1 13 0"/>
                  </svg>
                  Roles &amp; permissions
                </label>
                <div class="roles-options">
                  <label
                    v-for="role in availableRoles"
                    :key="role.id"
                    class="role-option"
                  >
                    <input
                      type="checkbox"
                      :value="role.id"
                      v-model="selectedRoleIds"
                    />
                    <div class="role-option-text">
                      <span class="role-name">{{ role.name }}</span>
                      <span class="role-description" v-if="role.description">{{ role.description }}</span>
                    </div>
                  </label>
                </div>
                <p class="roles-hint">Select one or both roles. At least one role is required.</p>
              </div>

              <div class="form-buttons">
                <button type="submit" class="button-primary">
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

      <transition name="modal-fade">
        <div v-if="showEditForm" class="modal-overlay">
          <div class="form-modal" @click.stop>
            <button class="modal-close" @click="showEditForm = false">
              <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
              </svg>
            </button>
            <h2>Edit User</h2>
            <form @submit.prevent="updateUser" class="register-form">
              <div class="form-group">
                <label for="edit-email">Email Address</label>
                <input
                  id="edit-email"
                  v-model="editForm.email"
                  type="email"
                  class="form-input"
                  :class="{ 'has-error': errors.email }"
                />
                <span v-if="errors.email" class="error-message">{{ errors.email }}</span>
              </div>

              <div class="form-group">
                <label for="edit-password">New Password (Optional)</label>
                <div class="password-input-wrapper">
                  <input
                    id="edit-password"
                    v-model="editForm.newPassword"
                    :type="showEditPassword ? 'text' : 'password'"
                    placeholder="Leave blank to keep current"
                    class="form-input"
                  />
                  <button type="button" class="password-toggle" @click="showEditPassword = !showEditPassword" :title="showEditPassword ? 'Hide password' : 'Show password'">
                    <svg v-if="showEditPassword" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                      <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/>
                      <circle cx="12" cy="12" r="3"/>
                    </svg>
                    <svg v-else width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                      <path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24"/>
                      <line x1="1" y1="1" x2="23" y2="23"/>
                    </svg>
                  </button>
                </div>
              </div>

              <div class="form-row">
                <div class="form-group">
                  <label for="edit-name">First Name</label>
                  <input
                    id="edit-name"
                    v-model="editForm.name"
                    type="text"
                    class="form-input"
                    :class="{ 'has-error': errors.name }"
                  />
                  <span v-if="errors.name" class="error-message">{{ errors.name }}</span>
                </div>

                <div class="form-group">
                  <label for="edit-lastname">Last Name</label>
                  <input
                    id="edit-lastname"
                    v-model="editForm.lastname"
                    type="text"
                    class="form-input"
                    :class="{ 'has-error': errors.lastname }"
                  />
                  <span v-if="errors.lastname" class="error-message">{{ errors.lastname }}</span>
                </div>
              </div>

              <div class="form-group roles-group">
                <label class="roles-label">Roles &amp; permissions</label>
                <div class="roles-options">
                  <label
                    v-for="role in availableRoles"
                    :key="role.id"
                    class="role-option"
                  >
                    <input
                      type="checkbox"
                      :value="role.id"
                      v-model="editForm.roleIds"
                    />
                    <div class="role-option-text">
                      <span class="role-name">{{ role.name }}</span>
                    </div>
                  </label>
                </div>
                <span v-if="errors.roles" class="error-message">{{ errors.roles }}</span>
              </div>

              <div class="form-buttons">
                <button type="submit" class="btn btn-primary">Update User</button>
                <button type="button" @click="showEditForm = false" class="btn btn-secondary">Cancel</button>
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
        <div class="filters-bar surface-card surface-card--padded">
          <div class="filter-group">
            <label for="user-filter">Filter users</label>
            <input
              id="user-filter"
              v-model="userFilter"
              type="text"
              placeholder="Search by name, email, role, or RFID"
              class="filter-input"
            />
          </div>
          <div class="filter-actions">
            <button class="clear-filter-btn button-ghost" @click="userFilter = ''" v-if="userFilter">
              Clear
            </button>
            <button @click="showRegisterForm = true" class="add-user-btn">
              <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M12 5v14M5 12h14"/>
              </svg>
              Add New User
            </button>
          </div>
        </div>

        <div class="users-grid">
          <div v-for="user in filteredUsers" :key="user.id" class="user-card">
            <div class="user-badges">
              <span 
                v-for="role in (user.roles || [])" 
                :key="role.id" 
                class="user-badge"
                :class="role.name.toLowerCase()"
              >
                {{ role.name }}
              </span>
            </div>
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
              <div class="detail-item">
                <span class="detail-icon">
                  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/>
                  </svg>
                </span>
                <span>{{ user.transactionCount || 0 }} transactions</span>
              </div>
            </div>
            <div class="user-footer">
              <button @click="openEditUser(user)" class="edit-user-btn button-ghost">
                Edit
              </button>
              <button @click="viewUserTransactions(user.id)" class="view-transactions-btn primary-action-btn">
                View Transactions
              </button>
            </div>
          </div>
        </div>

        <div v-if="!loading && users.length === 0" class="empty-state">
          <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
            <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/>
          </svg>
          <p>No users found. Create one using the "Add New User" button.</p>
        </div>
      </div>

    <!-- Transaction Modal -->
    <transition name="modal-fade">
      <div v-if="showTransactionModal" class="modal-overlay" @click="closeTransactionModal">
        <div class="transaction-modal" @click.stop>
          <button class="modal-close" @click="closeTransactionModal" aria-label="Close">
            <span class="modal-close-icon">×</span>
          </button>
          
          <div class="modal-header">
            <h2>{{ selectedUser?.name }} {{ selectedUser?.lastname }}'s Transactions</h2>
            <p class="modal-subtitle">{{ selectedUser?.email }}</p>
          </div>

          <div v-if="loadingTransactions" class="modal-loading">
            <div class="spinner"></div>
            <p>Loading transactions...</p>
          </div>

          <div v-else-if="userTransactions.length === 0" class="modal-empty">
            <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
              <path d="M3 12a9 9 0 1 0 18 0 9 9 0 0 0-18 0z"/><path d="M12 6v6l4 2"/>
            </svg>
            <p>No transactions found</p>
          </div>

          <div v-else class="transactions-list">
            <div v-for="transaction in userTransactions" :key="transaction.id" class="transaction-item">
              <div class="transaction-header">
                <div class="transaction-action" :class="actionClass(transaction.action)">
                  <svg v-if="actionClass(transaction.action) === 'borrow'" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <path d="M13 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V9z"/><polyline points="13 2 13 9 20 9"/>
                  </svg>
                  <svg v-else width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <path d="M1 4v6h6M23 20v-6h-6"/>
                  </svg>
                  <span>{{ actionLabel(transaction.action) }}</span>
                </div>
                <div class="transaction-date">
                  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/>
                  </svg>
                  {{ formatDate(transaction.timestamp) }}
                </div>
              </div>
              <div class="transaction-body">
                <div class="transaction-info">
                  <span class="info-label">Item:</span>
                  <span class="info-value">{{ transaction.itemName }}</span>
                </div>
                <div class="transaction-info">
                  <span class="info-label">Scanner:</span>
                  <span class="info-value">{{ transaction.deviceName }}</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </transition>

    </div>
  </AdminShell>
</template>

<script>
import { ref, computed, onMounted} from 'vue'
import api from '../services/api'
import AdminShell from '../components/AdminShell.vue'

export default {
  name: 'AdminUserManagement',
  components: {
    AdminShell
  },
  setup() {
    const users = ref([])
    const loading = ref(true)
    const userFilter = ref('')
    const showRegisterForm = ref(false)
    const showEditForm = ref(false)
    const showEditPassword = ref(false)
    const showTransactionModal = ref(false)
    const selectedUser = ref(null)
    const editingUser = ref(null)
    const userTransactions = ref([])
    const loadingTransactions = ref(false)
    const availableRoles = ref([
      {
        id: 2,
        name: 'User',
        description: 'Standard user access for borrowing and returning tools'
      },
      {
        id: 1,
        name: 'Admin',
        description: 'Full administrative access to dashboards and management tools'
      }
    ])
    const selectedRoleIds = ref([2])
    const newUser = ref({
      email: '',
      password: '',
      name: '',
      lastname: ''
    })
    const editForm = ref({
      email: '',
      name: '',
      lastname: '',
      roleIds: [],
      newPassword: ''
    })
    const errors = ref({})

    const fetchUsers = async () => {
      try {
        const response = await api.get('/auth/users')
        users.value = response.data
        console.log('Fetched users:', users.value)
      } catch (err) {
        console.error('Failed to fetch users:', err)
        console.error('Error details:', err.response)
      } finally {
        loading.value = false
      }
    }

    const fetchRoles = async () => {
      try {
        const response = await api.get('/auth/roles')
        const roles = Array.isArray(response.data) ? response.data : []
        if (roles.length) {
          availableRoles.value = roles.map(r => ({
            id: r.roleId,
            name: r.roleName,
            description: r.description || ''
          }))
        }
      } catch (err) {
        console.error('Failed to fetch roles:', err)
      }
    }

    const filteredUsers = computed(() => {
      const term = userFilter.value.trim().toLowerCase()
      if (!term) return users.value
      return users.value.filter(u => {
        const rolesStr = (u.roles || []).map(r => r.name).join(' ').toLowerCase()
        return (
          (u.name && u.name.toLowerCase().includes(term)) ||
          (u.lastname && u.lastname.toLowerCase().includes(term)) ||
          (u.email && u.email.toLowerCase().includes(term)) ||
          rolesStr.includes(term) ||
          (u.rfidUid && u.rfidUid.toLowerCase().includes(term))
        )
      })
    })

    const registerUser = async () => {
      try {
        if (!selectedRoleIds.value.length) {
          selectedRoleIds.value = [2]
        }

        await api.post('/auth/register', {
          ...newUser.value,
          roleIds: selectedRoleIds.value
        })

        newUser.value = { email: '', password: '', name: '', lastname: '' }
        selectedRoleIds.value = [2]
        showRegisterForm.value = false
        fetchUsers()
      } catch (err) {
        console.error('Registration failed:', err)
      }
    }

    const openEditUser = (user) => {
      editingUser.value = user
      editForm.value = {
        email: user.email,
        name: user.name,
        lastname: user.lastname,
        roleIds: (user.roles || []).map(r => r.id),
        newPassword: ''
      }
      errors.value = {}
      showEditForm.value = true
    }

    const validateForm = () => {
      const errs = {}
      if (!editForm.value.name?.trim()) errs.name = 'First name is required'
      if (!editForm.value.lastname?.trim()) errs.lastname = 'Last name is required'
      if (!editForm.value.email?.trim()) errs.email = 'Email is required'
      else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(editForm.value.email)) errs.email = 'Invalid email format'
      
      if (!editForm.value.roleIds || editForm.value.roleIds.length === 0) {
        errs.roles = 'At least one role is required'
      }
      
      errors.value = errs
      return Object.keys(errs).length === 0
    }

    const updateUser = async () => {
      if (!validateForm()) return

      try {
        await api.put(`/auth/users/${editingUser.value.id}`, editForm.value)
        showEditForm.value = false
        fetchUsers()
      } catch (err) {
        console.error('Update failed:', err)
        if (err.response?.data?.errors) {
           // Map backend validation errors if any
           // Assuming backend returns standard ASP.NET validation format
        }
      }
    }

    const actionLabel = (action) => {
      const a = (action || '').toLowerCase()
      if (a.includes('checkin') || a.includes('return')) return 'Returned'
      if (a.includes('checkout') || a.includes('borrow')) return 'Borrowed'
      return action || 'Borrowed'
    }

    const actionClass = (action) => {
      const a = (action || '').toLowerCase()
      if (a.includes('checkin') || a.includes('return')) return 'return'
      return 'borrow'
    }

    const viewUserTransactions = async (userId) => {
      console.log('Viewing transactions for user:', userId)
      selectedUser.value = users.value.find(u => u.id === userId)
      showTransactionModal.value = true
      loadingTransactions.value = true
      
      try {
        const response = await api.get(`/auth/users/${userId}/transactions`)
        userTransactions.value = response.data
      } catch (err) {
        console.error('Failed to fetch user transactions:', err)
        userTransactions.value = []
      } finally {
        loadingTransactions.value = false
      }
    }

    const closeTransactionModal = () => {
      showTransactionModal.value = false
      selectedUser.value = null
      userTransactions.value = []
    }

    const formatDate = (dateString) => {
      const date = new Date(dateString)
      return date.toLocaleString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
      })
    }

    onMounted(() => {
      fetchUsers()
      fetchRoles()
    })

    return {
      users,
      filteredUsers,
      userFilter,
      loading,
      showRegisterForm,
      showEditForm,
      showEditPassword,
      showTransactionModal,
      selectedUser,
      editingUser,
      userTransactions,
      loadingTransactions,
      availableRoles,
      selectedRoleIds,
      newUser,
      editForm,
      errors,
      registerUser,
      openEditUser,
      updateUser,
      actionLabel,
      actionClass,
      viewUserTransactions,
      closeTransactionModal,
      formatDate
    }
  }
}
</script>

<style scoped>
.users-page-content {
  width: 100%;
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

@media (max-width: 600px) {
  .add-user-btn {
    width: 100%;
    justify-content: center;
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

[data-theme="dark"] .form-group label {
  color: #cbd5e1;
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

[data-theme="dark"] .form-input {
  background: #1e293b;
  color: #e2e8f0;
  border-color: #334155;
}

[data-theme="dark"] .form-input:focus {
  background: #0f172a;
  border-color: var(--primary-light);
}

.form-input::placeholder {
  color: var(--text-tertiary);
}

.password-input-wrapper {
  position: relative;
  display: flex;
  align-items: center;
}

.password-input-wrapper .form-input {
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
  width: 18px;
  height: 18px;
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

.roles-group {
  margin-top: 0.5rem;
}

.roles-label {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  color: var(--dark-text);
  font-weight: 600;
  font-size: 0.9rem;
}

.roles-label svg {
  color: var(--primary-light);
}

.roles-options {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  margin-top: 0.25rem;
}

.role-option {
  display: flex;
  align-items: flex-start;
  gap: 0.75rem;
  padding: 0.6rem 0.8rem;
  border-radius: 10px;
  border: 1px solid var(--border-color);
  cursor: pointer;
  transition: all 0.2s ease;
  background: rgba(255, 255, 255, 0.9);
}

[data-theme="dark"] .role-option {
  background: #1e293b;
  border-color: #334155;
}

[data-theme="dark"] .role-name {
  color: #f1f5f9;
}

.role-option input {
  margin-top: 0.2rem;
}

.role-option-text {
  display: flex;
  flex-direction: column;
}

.role-name {
  font-weight: 600;
  color: var(--dark-text);
}

.role-description {
  font-size: 0.8rem;
  color: var(--text-tertiary);
}

.role-option:hover {
  border-color: var(--primary-light);
  box-shadow: 0 0 0 3px rgba(30, 144, 255, 0.1);
}

.roles-hint {
  margin-top: 0.25rem;
  font-size: 0.8rem;
  color: var(--text-tertiary);
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

.filters-bar {
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
  gap: 1rem;
  margin-bottom: 1.5rem;
}

.filter-group {
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
  flex: 1;
}

.filter-group label {
  font-size: 0.9rem;
  color: var(--dark-text);
  font-weight: 600;
}

[data-theme="dark"] .filter-group label {
  color: #cbd5e1;
}

.filter-input {
  padding: 0.75rem 0.85rem;
  border: 1px solid var(--border-color);
  border-radius: 10px;
  font-size: 0.95rem;
  background: white;
  color: var(--dark-text);
  transition: border-color 0.2s, box-shadow 0.2s;
}

.filter-input:focus {
  outline: none;
  border-color: var(--primary-light);
  box-shadow: 0 0 0 3px rgba(0, 125, 255, 0.15);
}

[data-theme="dark"] .filter-input {
  background: #1e293b;
  color: #e2e8f0;
  border-color: #334155;
}

.filter-actions {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.clear-filter-btn {
  align-self: center;
}

@media (max-width: 700px) {
  .filters-bar {
    flex-direction: column;
    align-items: stretch;
  }

  .clear-filter-btn {
    width: 100%;
  }
}

.users-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
  gap: 1.5rem;
  margin-bottom: 2rem;
}

.user-card {
  background: var(--card-bg, white);
  border: 2px solid var(--border-color);
  border-radius: 12px;
  padding: 1.5rem;
  position: relative;
  overflow: hidden;
  transition: all 0.3s ease;
  box-shadow: 0 2px 8px rgba(0, 61, 107, 0.06);
}

[data-theme="dark"] .user-card {
  background: #1e293b;
  border-color: #334155;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.3);
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

[data-theme="dark"] .user-card:hover {
  box-shadow: 0 8px 24px rgba(30, 144, 255, 0.3);
}

.user-badges {
  display: flex;
  gap: 0.5rem;
  margin-bottom: 0.8rem;
  flex-wrap: wrap;
}

.user-badge {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  padding: 0.35rem 0.9rem;
  border-radius: 999px;
  font-size: 0.8rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.35px;
  color: var(--badge-pill-text);
  background: var(--badge-neutral-bg);
  border: none;
  box-shadow: 0 6px 14px rgba(0, 0, 0, 0.1);
  transition: transform 0.2s ease;
}

.user-badge.admin {
  background: var(--badge-role-admin-gradient);
  color: var(--badge-role-admin-text);
}

.user-badge.user {
  background: var(--badge-role-user-gradient);
  color: var(--badge-role-user-text);
}

.user-badge:hover {
  transform: translateY(-1px);
}

.edit-user-btn {
  background: transparent;
  color: var(--primary-dark);
  border: 1px solid var(--border-color);
  padding: 0.4rem 0.75rem;
  border-radius: 8px;
  font-size: 0.85rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.18s ease;
  margin-right: 0.5rem;
  min-width: 72px;
}

.edit-user-btn:hover {
  background: var(--bg-light);
  border-color: var(--primary-light);
}

.user-name {
  color: var(--primary-dark);
  margin: 0 0 1rem;
  font-size: 1.2rem;
  font-weight: 700;
  line-height: 1.4;
}

[data-theme="dark"] .user-name {
  color: #f1f5f9;
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

[data-theme="dark"] .detail-item {
  color: #cbd5e1;
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
  display: flex;
  gap: 0.75rem;
  align-items: center;
  justify-content: flex-start;
  color: var(--accent-gray);
  font-size: 0.9rem;
  font-weight: 500;
  margin-top: 0.75rem;
}

.primary-action-btn {
  background: linear-gradient(135deg, var(--primary-dark) 0%, var(--primary-light) 100%);
  color: var(--text-inverse);
  border: none;
  padding: 0.6rem 1rem;
  border-radius: 10px;
  font-size: 0.95rem;
  font-weight: 700;
  cursor: pointer;
  box-shadow: 0 8px 20px rgba(30,144,255,0.12);
  transition: transform var(--transition-fast), box-shadow var(--transition-fast);
  min-width: 140px;
}

.primary-action-btn:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 10px 26px rgba(30,144,255,0.18);
}

[data-theme="dark"] .user-footer {
  color: #94a3b8;
}


/* Transaction Modal */
.transaction-modal {
  background: white;
  border-radius: 20px;
  padding: 0;
  max-width: 800px;
  width: 90%;
  max-height: 85vh;
  overflow: hidden;
  position: relative;
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
  animation: modalSlideIn 0.3s ease-out;
}

@keyframes modalSlideIn {
  from {
    opacity: 0;
    transform: translateY(-30px) scale(0.95);
  }
  to {
    opacity: 1;
    transform: translateY(0) scale(1);
  }
}

[data-theme="dark"] .transaction-modal {
  background: #1e293b;
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.6);
}

.modal-header {
  background: linear-gradient(135deg, #003D6B 0%, #005A9C 100%);
  color: white;
  padding: 2rem 2.5rem;
  border-bottom: none;
  position: relative;
}

.modal-header::after {
  content: '';
  position: absolute;
  bottom: 0;
  left: 0;
  right: 0;
  height: 4px;
  background: linear-gradient(90deg, rgba(255,255,255,0.1) 0%, rgba(255,255,255,0.3) 50%, rgba(255,255,255,0.1) 100%);
}

.modal-header h2 {
  color: white;
  font-size: 1.75rem;
  margin: 0 0 0.5rem;
  font-weight: 700;
  text-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.modal-subtitle {
  color: rgba(255, 255, 255, 0.9);
  font-size: 1rem;
  margin: 0;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.modal-subtitle::before {
  content: '';
  display: inline-block;
  width: 8px;
  height: 8px;
  background: #10b981;
  border-radius: 50%;
  box-shadow: 0 0 8px rgba(16, 185, 129, 0.6);
}

.modal-close {
  position: absolute;
  top: 1.25rem;
  right: 1.25rem;
  background: transparent;
  border: none;
  width: 32px;
  height: 32px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all 0.2s ease;
  z-index: 10;
}

.modal-close:hover {
  background: rgba(255, 255, 255, 0.15);
  transform: scale(1.05);
}

.modal-close-icon {
  font-size: 18px;
  font-weight: 800;
  line-height: 1;
  color: white;
}

.modal-loading {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 4rem 2rem;
  color: #64748b;
}

.modal-loading .spinner {
  width: 48px;
  height: 48px;
  border: 4px solid rgba(102, 126, 234, 0.2);
  border-top-color: #667eea;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
  margin-bottom: 1.5rem;
}

.modal-loading p {
  font-size: 1rem;
  font-weight: 500;
}

.modal-empty {
  text-align: center;
  padding: 4rem 2rem;
  color: #94a3b8;
}

.modal-empty svg {
  margin-bottom: 1.5rem;
  opacity: 0.4;
  color: #cbd5e1;
}

.modal-empty p {
  margin: 0;
  font-size: 1.1rem;
  font-weight: 500;
}

.transactions-list {
  padding: 2rem 2.5rem;
  max-height: calc(85vh - 140px);
  overflow-y: auto;
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.transactions-list::-webkit-scrollbar {
  width: 8px;
}

.transactions-list::-webkit-scrollbar-track {
  background: #f1f5f9;
  border-radius: 10px;
}

[data-theme="dark"] .transactions-list::-webkit-scrollbar-track {
  background: #0f172a;
}

.transactions-list::-webkit-scrollbar-thumb {
  background: #cbd5e1;
  border-radius: 10px;
}

.transactions-list::-webkit-scrollbar-thumb:hover {
  background: #94a3b8;
}

.transaction-item {
  background: #f8fafc;
  border: 2px solid #e2e8f0;
  border-radius: 12px;
  overflow: hidden;
  transition: all 0.2s;
  animation: fadeInUp 0.3s ease-out backwards;
}

@keyframes fadeInUp {
  from {
    opacity: 0;
    transform: translateY(10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.transaction-item:nth-child(1) { animation-delay: 0.05s; }
.transaction-item:nth-child(2) { animation-delay: 0.1s; }
.transaction-item:nth-child(3) { animation-delay: 0.15s; }
.transaction-item:nth-child(4) { animation-delay: 0.2s; }
.transaction-item:nth-child(5) { animation-delay: 0.25s; }

[data-theme="dark"] .transaction-item {
  background: #0f172a;
  border-color: #334155;
}

.transaction-item:hover {
  box-shadow: 0 8px 20px rgba(102, 126, 234, 0.15);
  border-color: #667eea;
  transform: translateY(-2px);
}

[data-theme="dark"] .transaction-item:hover {
  box-shadow: 0 8px 20px rgba(102, 126, 234, 0.25);
  border-color: #667eea;
}

.transaction-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.25rem 1.5rem;
  background: white;
  border-bottom: 2px solid #e2e8f0;
}

[data-theme="dark"] .transaction-header {
  background: #1e293b;
  border-bottom-color: #334155;
}

.transaction-action {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  font-weight: 700;
  text-transform: uppercase;
  font-size: 0.875rem;
  letter-spacing: 0.5px;
  padding: 0.5rem 1rem;
  border-radius: 8px;
  background: rgba(59, 130, 246, 0.1);
}

.transaction-action.borrow {
  color: #3b82f6;
  background: rgba(59, 130, 246, 0.1);
}

.transaction-action.return {
  color: #10b981;
  background: rgba(16, 185, 129, 0.1);
}

.transaction-action svg {
  flex-shrink: 0;
}

.transaction-date {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  color: #64748b;
  font-size: 0.875rem;
  font-weight: 500;
  padding: 0.5rem 0.75rem;
  background: #f1f5f9;
  border-radius: 6px;
}

[data-theme="dark"] .transaction-date {
  color: #94a3b8;
  background: #0f172a;
}

.transaction-date svg {
  flex-shrink: 0;
}

.transaction-body {
  padding: 1.5rem;
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 2rem;
}

.transaction-info {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.info-label {
  font-size: 0.75rem;
  color: #64748b;
  text-transform: uppercase;
  letter-spacing: 0.1em;
  font-weight: 700;
}

[data-theme="dark"] .info-label {
  color: #94a3b8;
}

.info-value {
  color: #1e293b;
  font-weight: 600;
  font-size: 1rem;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

[data-theme="dark"] .info-value {
  color: #f1f5f9;
}

.info-value::before {
  content: '▸';
  color: #667eea;
  font-weight: bold;
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

.error-message {
  color: #ef4444;
  font-size: 0.875rem;
  margin-top: 0.25rem;
  display: block;
}

.form-input.has-error {
  border-color: #ef4444;
}

.form-input.has-error:focus {
  box-shadow: 0 0 0 3px rgba(239, 68, 68, 0.2);
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
