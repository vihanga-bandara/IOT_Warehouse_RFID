<template>
  <div class="admin-layout-wrapper">
    <header class="admin-shell">
      <div class="page-header">
        <div class="header-left">
          <h1 class="app-title">ToolTrackPro - Admin Dashboard</h1>
        </div>
        <div class="page-header-actions">
          <div class="account-wrapper" @click.stop>
            <button
              class="account-btn"
              title="My account"
              @click="toggleDropdown"
            >
              <span class="account-avatar">{{ userInitials }}</span>
              <span class="account-name-btn">{{ authStore.user?.name }}</span>
              <svg class="chevron-icon" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <polyline points="6 9 12 15 18 9"/>
              </svg>
            </button>
            <transition name="fade">
              <div v-if="showDropdown" class="account-dropdown">
                <div class="account-dropdown-header">
                  <div class="account-avatar-sm">{{ userInitials }}</div>
                  <div class="account-dropdown-text">
                    <div class="account-name">{{ authStore.user?.name }} {{ authStore.user?.lastname }}</div>
                    <div class="account-email">{{ authStore.user?.email }}</div>
                    <div class="account-role">{{ primaryRole }}</div>
                  </div>
                </div>
                
                <button class="dropdown-item" @click="toggleTheme">
                  <svg v-if="isDark" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <circle cx="12" cy="12" r="5"/>
                    <line x1="12" y1="1" x2="12" y2="3"/>
                    <line x1="12" y1="21" x2="12" y2="23"/>
                    <line x1="4.22" y1="4.22" x2="5.64" y2="5.64"/>
                    <line x1="18.36" y1="18.36" x2="19.78" y2="19.78"/>
                    <line x1="1" y1="12" x2="3" y2="12"/>
                    <line x1="21" y1="12" x2="23" y2="12"/>
                    <line x1="4.22" y1="19.78" x2="5.64" y2="18.36"/>
                    <line x1="18.36" y1="5.64" x2="19.78" y2="4.22"/>
                  </svg>
                  <svg v-else width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z"/>
                  </svg>
                  {{ isDark ? 'Switch to Light Mode' : 'Switch to Dark Mode' }}
                </button>

                <button class="dropdown-item" @click="openAccountModal">
                  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <path d="M12 12c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4z"/>
                    <path d="M4 20c0-2.21 3.58-4 8-4s8 1.79 8 4"/>
                  </svg>
                  My profile & password
                </button>
                <div class="dropdown-divider"></div>
                <button class="dropdown-item danger" @click="logout">
                  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"/>
                    <polyline points="16 17 21 12 16 7"/>
                    <line x1="21" y1="12" x2="9" y2="12"/>
                  </svg>
                  Logout
                </button>
              </div>
            </transition>
          </div>
        </div>
      </div>

      <nav class="admin-nav">
        <div class="nav-links">
          <router-link to="/dashboard" class="nav-link" :class="{ active: activeTab === 'dashboard' }">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/><polyline points="9 22 9 12 15 12 15 22"/>
            </svg>
            Overview
          </router-link>
          <router-link to="/admin/items" class="nav-link" :class="{ active: activeTab === 'items' }">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <rect x="3" y="4" width="18" height="16" rx="2" ry="2"/><path d="M3 10h18"/>
            </svg>
            Items
          </router-link>
          <router-link to="/admin/transactions" class="nav-link" :class="{ active: activeTab === 'transactions' }">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M3 12a9 9 0 1 0 18 0 9 9 0 0 0-18 0z"/><path d="M12 6v6l4 2"/>
            </svg>
            Transactions
          </router-link>
          <router-link to="/admin/users" class="nav-link" :class="{ active: activeTab === 'users' }">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/>
            </svg>
            Users
          </router-link>
        </div>
      </nav>
    </header>

    <main class="admin-content">
      <div class="content-header" v-if="subtitle">
        <p class="content-subtitle">{{ subtitle }}</p>
      </div>
      <slot></slot>
    </main>

    <footer class="admin-footer">
      <div class="footer-content">
        <p>&copy; {{ new Date().getFullYear() }} ToolTrackPro. All rights reserved.</p>
        <div class="footer-links">
          <a href="#">Privacy Policy</a>
          <span class="separator">•</span>
          <a href="#">Terms of Service</a>
          <span class="separator">•</span>
          <span class="version">v1.0.0</span>
        </div>
      </div>
    </footer>

    <!-- Account modal is rendered once but overlays all content -->
    <transition name="modal-fade">
      <div v-if="showAccountModal" class="modal-overlay" @click="closeAccountModal">
        <div class="form-modal account-modal" @click.stop>
          <button class="modal-close" @click="closeAccountModal" aria-label="Close">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <line x1="18" y1="6" x2="6" y2="18"/>
              <line x1="6" y1="6" x2="18" y2="18"/>
            </svg>
          </button>
          <h2>My Account</h2>

          <div v-if="accountLoading" class="account-loading">
            <div class="spinner"></div>
            <p>Loading your profile...</p>
          </div>

          <div v-else>
            <div class="account-summary">
              <div class="account-avatar-lg">{{ userInitials }}</div>
              <div class="account-summary-text">
                <div class="account-name">{{ accountProfile?.name }} {{ accountProfile?.lastname }}</div>
                <div class="account-email">{{ accountProfile?.email }}</div>
                <div class="account-roles">{{ (accountProfile?.roles || []).join(', ') || primaryRole }}</div>
              </div>
            </div>

            <div class="divider"></div>

            <form class="password-form" @submit.prevent="submitPasswordChange">
              <h3>Change Password</h3>
              <div class="form-group">
                <label for="currentPassword">Current password</label>
                <input
                  id="currentPassword"
                  v-model="passwordForm.currentPassword"
                  type="password"
                  required
                  class="form-input"
                  autocomplete="current-password"
                />
              </div>
              <div class="form-row">
                <div class="form-group">
                  <label for="newPassword">New password</label>
                  <input
                    id="newPassword"
                    v-model="passwordForm.newPassword"
                    type="password"
                    required
                    class="form-input"
                    autocomplete="new-password"
                  />
                </div>
                <div class="form-group">
                  <label for="confirmPassword">Confirm new password</label>
                  <input
                    id="confirmPassword"
                    v-model="passwordForm.confirmNewPassword"
                    type="password"
                    required
                    class="form-input"
                    autocomplete="new-password"
                  />
                </div>
              </div>

              <div v-if="passwordError" class="password-message error">{{ passwordError }}</div>
              <div v-if="passwordSuccess" class="password-message success">{{ passwordSuccess }}</div>

              <div class="password-actions">
                <button type="submit" class="btn btn-primary" :disabled="passwordSaving">
                  <span v-if="!passwordSaving">Update Password</span>
                  <span v-else>Updating...</span>
                </button>
                <button type="button" class="btn btn-secondary" @click="resetPasswordForm">Clear</button>
              </div>
            </form>
          </div>
        </div>
      </div>
    </transition>
  </div>
</template>

<script>
import { computed, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/authStore'
import { useTheme } from '../composables/useTheme'
import api from '../services/api'

export default {
  name: 'AdminShell',
  props: {
    title: {
      type: String,
      required: true
    },
    subtitle: {
      type: String,
      default: ''
    },
    activeTab: {
      type: String,
      required: true
    }
  },
  setup() {
    const router = useRouter()
    const authStore = useAuthStore()
    const { toggleTheme, isDark } = useTheme()
    const showDropdown = ref(false)
    const showAccountModal = ref(false)
    const accountLoading = ref(false)
    const accountProfile = ref(null)
    const passwordForm = ref({
      currentPassword: '',
      newPassword: '',
      confirmNewPassword: ''
    })
    const passwordSaving = ref(false)
    const passwordError = ref('')
    const passwordSuccess = ref('')

    const userInitials = computed(() => {
      const name = authStore.user?.name || ''
      const lastname = authStore.user?.lastname || ''
      const fromNames = `${name} ${lastname}`.trim()
      if (fromNames) {
        return fromNames
          .split(' ')
          .filter(Boolean)
          .slice(0, 2)
          .map(p => p[0].toUpperCase())
          .join('')
      }
      const email = authStore.user?.email || ''
      return email ? email[0].toUpperCase() : '?'
    })

    const primaryRole = computed(() => {
      return authStore.user?.role || 'User'
    })

    const toggleDropdown = () => {
      showDropdown.value = !showDropdown.value
    }

    const openAccountModal = async () => {
      showAccountModal.value = true
      showDropdown.value = false
      passwordError.value = ''
      passwordSuccess.value = ''
      accountLoading.value = true
      try {
        const response = await api.get('/auth/me')
        accountProfile.value = response.data
      } catch (err) {
        console.error('Failed to load account profile:', err)
      } finally {
        accountLoading.value = false
      }
    }

    const closeAccountModal = () => {
      showAccountModal.value = false
      resetPasswordForm()
      passwordError.value = ''
      passwordSuccess.value = ''
    }

    const resetPasswordForm = () => {
      passwordForm.value = {
        currentPassword: '',
        newPassword: '',
        confirmNewPassword: ''
      }
    }

    const submitPasswordChange = async () => {
      passwordError.value = ''
      passwordSuccess.value = ''

      if (!passwordForm.value.newPassword || passwordForm.value.newPassword.length < 6) {
        passwordError.value = 'New password must be at least 6 characters.'
        return
      }

      if (passwordForm.value.newPassword !== passwordForm.value.confirmNewPassword) {
        passwordError.value = 'New password and confirmation do not match.'
        return
      }

      passwordSaving.value = true
      try {
        await api.put('/auth/me/password', {
          currentPassword: passwordForm.value.currentPassword,
          newPassword: passwordForm.value.newPassword
        })
        passwordSuccess.value = 'Password updated successfully.'
        resetPasswordForm()
      } catch (err) {
        console.error('Failed to update password:', err)
        const message = err.response?.data?.message || 'Failed to update password. Please check your current password.'
        passwordError.value = message
      } finally {
        passwordSaving.value = false
      }
    }

    const logout = () => {
      authStore.logout()
      router.push('/login')
    }

    return {
      authStore,
      showDropdown,
      showAccountModal,
      accountLoading,
      accountProfile,
      passwordForm,
      passwordSaving,
      passwordError,
      passwordSuccess,
      userInitials,
      primaryRole,
      toggleDropdown,
      openAccountModal,
      closeAccountModal,
      resetPasswordForm,
      submitPasswordChange,
      logout,
      toggleTheme,
      isDark
    }
  }
}
</script>

<style scoped>
.admin-layout-wrapper {
  min-height: 100%;
  display: flex;
  flex-direction: column;
  width: 100%;
  max-width: 1440px;
  margin: 0 auto;
  padding: 2rem 2rem 0;
}

.admin-content {
  flex: 1;
  width: 100%;
  animation: fadeIn 0.5s ease-out;
}

@keyframes fadeIn {
  from { opacity: 0; transform: translateY(10px); }
  to { opacity: 1; transform: translateY(0); }
}

.admin-shell {
  margin-bottom: 2rem;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
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
  margin: 0;
  font-size: 2rem;
  font-weight: 800;
  letter-spacing: -0.5px;
  line-height: 1.2;
}

.page-subtitle {
  color: var(--accent-gray);
  margin: 0.25rem 0 0;
  font-size: 1rem;
  font-weight: 500;
}

.header-left {
  display: flex;
  flex-direction: row;
  align-items: center;
  gap: 1rem;
}

.brand-logo {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  color: var(--primary-light);
  font-weight: 800;
  font-size: 1.25rem;
  letter-spacing: -0.5px;
}

.account-btn {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  background: white;
  border: 1px solid var(--border-color);
  padding: 0.5rem 1rem 0.5rem 0.5rem;
  border-radius: 50px;
  cursor: pointer;
  transition: all 0.2s ease;
  box-shadow: var(--shadow-sm);
  min-width: 180px;
  justify-content: space-between;
}

.account-btn:hover {
  box-shadow: var(--shadow-md);
  transform: translateY(-1px);
  border-color: var(--primary-light);
}

.account-name-btn {
  font-weight: 600;
  color: var(--primary-dark);
  font-size: 0.9rem;
  flex: 1;
  text-align: left;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.chevron-icon {
  color: var(--accent-gray);
  transition: transform 0.2s ease;
}

.account-btn:hover .chevron-icon {
  color: var(--primary-light);
}

[data-theme="dark"] .account-btn {
  background: var(--bg-secondary);
  border-color: var(--border-color);
}

[data-theme="dark"] .account-name-btn {
  color: var(--dark-text);
}

.page-header-actions {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.account-wrapper {
  position: relative;
}



.account-avatar {
  font-size: 0.85rem;
  font-weight: 700;
  color: var(--primary-dark);
}

.account-dropdown {
  position: absolute;
  right: 0;
  margin-top: 0.5rem;
  min-width: 220px;
  background: var(--bg-secondary);
  border-radius: 12px;
  box-shadow: 0 12px 30px rgba(15, 23, 42, 0.18);
  border: 1px solid var(--border-color);
  padding: 0.5rem 0;
  z-index: 1100;
}

.account-dropdown-header {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem 0.75rem 0.4rem;
  border-bottom: 1px solid var(--border-color);
}

.account-avatar-sm {
  width: 32px;
  height: 32px;
  border-radius: 999px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, var(--primary-dark), var(--primary-light));
  color: #fff;
  font-size: 0.9rem;
  font-weight: 700;
}

.account-dropdown-text {
  display: flex;
  flex-direction: column;
  gap: 0.1rem;
}

.account-name {
  font-weight: 600;
  font-size: 0.9rem;
  color: var(--primary-dark);
}

.account-email {
  font-size: 0.8rem;
  color: var(--accent-gray);
}

.account-role {
  font-size: 0.75rem;
  color: var(--text-tertiary);
}

.dropdown-item {
  width: 100%;
  padding: 0.5rem 0.9rem;
  background: transparent;
  border: none;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.9rem;
  color: var(--dark-text);
  cursor: pointer;
  text-align: left;
  justify-content: flex-start;
  transition: background-color 0.15s ease, color 0.15s ease;
}

.dropdown-item svg {
  color: var(--accent-gray);
}

.dropdown-item:hover {
  background-color: rgba(15, 23, 42, 0.04);
}

.dropdown-item.danger {
  color: #dc3545;
  justify-content: center;
}

.dropdown-item.danger svg {
  color: #dc3545;
}

.dropdown-divider {
  height: 1px;
  background: var(--border-color);
  margin: 0.35rem 0;
}

.admin-nav {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  margin-bottom: 2rem;
  border-bottom: 2px solid rgba(30, 144, 255, 0.1);
  padding-bottom: 1rem;
}

.nav-links {
  display: flex;
  gap: 0.5rem;
}

.nav-subtitle {
  color: var(--accent-gray);
  font-size: 0.95rem;
  font-weight: 500;
  margin: 0;
  padding-left: 0.5rem;
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
  .page-header h1 {
    font-size: 1.8rem;
  }

  .page-subtitle {
    font-size: 0.95rem;
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
  inset: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 1rem;
  z-index: 1000;
}

.modal-fade-enter-active,
.modal-fade-leave-active {
  transition: opacity 0.2s ease;
}

.modal-fade-enter-from,
.modal-fade-leave-to {
  opacity: 0;
}

.account-modal {
  max-width: 540px;
}

.form-modal {
  background: var(--bg-secondary);
  padding: 2.5rem;
  border-radius: 16px;
  box-shadow: 0 20px 60px rgba(0, 61, 107, 0.25);
  width: 100%;
  position: relative;
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

.account-summary {
  display: flex;
  align-items: center;
  gap: 1rem;
  margin-bottom: 1.5rem;
}

.account-avatar-lg {
  width: 56px;
  height: 56px;
  border-radius: 999px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, var(--primary-dark), var(--primary-light));
  color: #fff;
  font-size: 1.4rem;
  font-weight: 700;
}

.account-summary-text {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.account-name {
  font-weight: 700;
  color: var(--primary-dark);
}

[data-theme="dark"] .account-name {
  color: #f1f5f9;
}

.account-email {
  font-size: 0.9rem;
  color: var(--accent-gray);
}

.account-roles {
  font-size: 0.85rem;
  color: var(--text-tertiary);
}

.divider {
  height: 1px;
  background: var(--border-color);
  margin: 1.25rem 0;
}

.password-form h3 {
  margin: 0 0 1rem;
  font-size: 1.1rem;
  font-weight: 700;
  color: var(--primary-dark);
}

[data-theme="dark"] .password-form h3 {
  color: #f1f5f9;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
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

.password-actions {
  display: flex;
  gap: 0.75rem;
  margin-top: 1rem;
}

.password-message {
  margin-top: 0.5rem;
  font-size: 0.85rem;
}

.password-message.error {
  color: #dc3545;
}

.password-message.success {
  color: var(--accent-green);
}

.admin-footer {
  margin-top: auto;
  padding: 2rem 0;
  border-top: 1px solid var(--border-color);
  background: var(--surface-card);
}

.footer-content {
  max-width: 1440px;
  margin: 0 auto;
  padding: 0 2rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
  color: var(--accent-gray);
  font-size: 0.875rem;
}

.footer-links {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.footer-links a {
  color: var(--accent-gray);
  text-decoration: none;
  transition: color 0.2s;
}

.footer-links a:hover {
  color: var(--primary-light);
}

.separator {
  color: var(--border-color);
}

.version {
  font-family: monospace;
  background: var(--bg-color);
  padding: 0.2rem 0.5rem;
  border-radius: 4px;
  font-size: 0.75rem;
}

@media (max-width: 600px) {
  .footer-content {
    flex-direction: column;
    gap: 1rem;
    text-align: center;
  }
}

.account-loading {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 2rem 0;
  gap: 0.75rem;
}
.content-header {
  margin-bottom: 1rem;
  padding-bottom: 0.5rem;
  border-bottom: 1px solid var(--border-color);
}

.content-subtitle {
  color: var(--primary-dark);
  margin: 0;
  font-size: 1.25rem;
  font-weight: 600;
  letter-spacing: -0.01em;
}

[data-theme="dark"] .app-title {
  color: var(--text-primary);
}

[data-theme="dark"] .content-subtitle {
  color: var(--text-primary);
}

</style>
