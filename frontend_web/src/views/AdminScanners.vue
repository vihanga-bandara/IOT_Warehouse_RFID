<template>
  <AdminShell
    title="Scanners"
    subtitle="Manage RFID scanners and their configuration"
    active-tab="scanners"
  >
    <div class="scanners-page-content">
      <div v-if="loading" class="loading-state">
        <div class="spinner"></div>
        <p>Loading scanners...</p>
      </div>

      <div v-else-if="scanners.length === 0" class="empty-state">
        <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
          <rect x="3" y="4" width="18" height="16" rx="2" ry="2" />
          <path d="M3 10h18" />
        </svg>
        <p>No scanners have been configured yet.</p>
      </div>

      <div v-else class="scanners-section">
        <div class="filters-bar surface-card surface-card--padded">
          <div class="filter-group">
            <label for="scanner-filter">Filter scanners</label>
            <input
              id="scanner-filter"
              v-model="scannerFilter"
              type="text"
              placeholder="Search by name, device ID, or status"
              class="filter-input"
            />
          </div>
          <div class="filter-actions">
            <button
              v-if="scannerFilter"
              class="clear-filter-btn button-ghost"
              @click="scannerFilter = ''"
            >
              Clear
            </button>
            <button class="button-primary" @click="openCreate">
              + Add scanner
            </button>
          </div>
        </div>

        <div class="results-info">
          Showing {{ paginatedScanners.length }} of {{ filteredScanners.length }} scanner<span v-if="filteredScanners.length !== 1">s</span>
        </div>

        <div class="scanners-grid">
          <div
            v-for="scanner in paginatedScanners"
            :key="scanner.scannerId"
            class="user-card scanner-card"
          >
            <div class="scanner-badges">
              <span
                class="scanner-badge"
                :class="(scanner.status || 'Active').toLowerCase() === 'active' ? 'active' : 'inactive'"
              >
                {{ (scanner.status || 'Active').toUpperCase() }}
              </span>
            </div>
            <div class="scanner-card-header">
              <div>
                <h3 class="scanner-name">
                  <span class="scanner-name-text">{{ scanner.name }}</span>
                  <button
                    type="button"
                    class="copy-name-btn"
                    @click="copyName(scanner)"
                    :title="copied[scanner.scannerId] ? 'Copied' : 'Copy scanner name'"
                    :aria-label="copied[scanner.scannerId] ? 'Copied' : 'Copy scanner name'"
                  >
                    <svg v-if="!copied[scanner.scannerId]" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                      <rect x="9" y="9" width="13" height="13" rx="2" ry="2" />
                      <path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1"/>
                    </svg>
                    <svg v-else width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                      <polyline points="20 6 9 17 4 12" />
                    </svg>
                  </button>
                </h3>
                <div class="scanner-device mono">{{ scanner.deviceId }}</div>
              </div>
            </div>
            <div class="scanner-card-footer">
              <button class="edit-scanner-btn" @click="openEdit(scanner)">
                Edit
              </button>
            </div>
          </div>
        </div>

        <!-- Pagination Controls -->
        <div class="pagination-controls" v-if="totalPages > 1">
          <button 
            class="pagination-btn" 
            :disabled="currentPage === 1"
            @click="currentPage = 1"
            title="First page"
          >
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <polyline points="11 17 6 12 11 7"/><polyline points="18 17 13 12 18 7"/>
            </svg>
          </button>
          <button 
            class="pagination-btn" 
            :disabled="currentPage === 1"
            @click="currentPage--"
            title="Previous page"
          >
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <polyline points="15 18 9 12 15 6"/>
            </svg>
          </button>
          
          <div class="pagination-info">
            Page {{ currentPage }} of {{ totalPages }}
          </div>
          
          <button 
            class="pagination-btn" 
            :disabled="currentPage === totalPages"
            @click="currentPage++"
            title="Next page"
          >
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <polyline points="9 18 15 12 9 6"/>
            </svg>
          </button>
          <button 
            class="pagination-btn" 
            :disabled="currentPage === totalPages"
            @click="currentPage = totalPages"
            title="Last page"
          >
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <polyline points="13 17 18 12 13 7"/><polyline points="6 17 11 12 6 7"/>
            </svg>
          </button>
          
          <select v-model="itemsPerPage" class="page-size-select">
            <option :value="10">10 / page</option>
            <option :value="25">25 / page</option>
            <option :value="50">50 / page</option>
          </select>
        </div>
      </div>

      <transition name="modal-fade">
        <div v-if="showModal" class="modal-overlay" @click="closeModal">
          <div class="form-modal" @click.stop>
            <button class="modal-close" @click="closeModal" aria-label="Close">
              <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <line x1="18" y1="6" x2="6" y2="18" />
                <line x1="6" y1="6" x2="18" y2="18" />
              </svg>
            </button>
            <h2>{{ isEditing ? 'Edit scanner' : 'Add scanner' }}</h2>

            <form @submit.prevent="submitForm" class="register-form scanner-form">
              <div class="form-group">
                <label for="scanner-name">Name</label>
                <input
                  id="scanner-name"
                  v-model.trim="form.name"
                  type="text"
                  required
                  class="form-input"
                  :class="{ 'has-error': errors.name }"
                  placeholder="e.g. Lab Kiosk 1"
                />
                <p v-if="errors.name" class="field-error">{{ errors.name }}</p>
              </div>

              <div class="form-group">
                <label for="scanner-device">Device ID</label>
                <input
                  id="scanner-device"
                  v-model.trim="form.deviceId"
                  type="text"
                  required
                  class="form-input mono"
                  :class="{ 'has-error': errors.deviceId }"
                  placeholder="Exact DeviceId sent by the scanner"
                />
                <p v-if="errors.deviceId" class="field-error">{{ errors.deviceId }}</p>
              </div>

              <div class="form-group">
                <label for="scanner-status">Status</label>
                <select
                  id="scanner-status"
                  v-model="form.status"
                  class="form-input"
                >
                  <option value="Active">Active</option>
                  <option value="Inactive">Inactive</option>
                </select>
              </div>

              <div v-if="formError" class="form-message error">{{ formError }}</div>
              <div v-if="formSuccess" class="form-message success">{{ formSuccess }}</div>

              <div class="form-actions">
                <button
                  type="submit"
                  class="btn btn-primary"
                  :disabled="submitting"
                >
                  <span v-if="!submitting">{{ isEditing ? 'Save changes' : 'Create scanner' }}</span>
                  <span v-else>Saving...</span>
                </button>
                <button type="button" class="btn btn-secondary" @click="closeModal">
                  Cancel
                </button>
              </div>
            </form>
          </div>
        </div>
      </transition>
    </div>
  </AdminShell>
</template>

<script>
import { ref, reactive, computed, onMounted } from 'vue'
import AdminShell from '../components/AdminShell.vue'
import api from '../services/api'

export default {
  name: 'AdminScanners',
  components: { AdminShell },
  setup() {
    const scanners = ref([])
    const loading = ref(true)
    const scannerFilter = ref('')
    const showModal = ref(false)
    const isEditing = ref(false)
    const submitting = ref(false)
    const currentId = ref(null)
    const currentPage = ref(1)
    const itemsPerPage = ref(25)

    const form = reactive({
      name: '',
      deviceId: '',
      status: 'Active'
    })

    const errors = reactive({
      name: '',
      deviceId: ''
    })

    const formError = ref('')
    const formSuccess = ref('')

    const resetForm = () => {
      form.name = ''
      form.deviceId = ''
      form.status = 'Active'
      errors.name = ''
      errors.deviceId = ''
      formError.value = ''
      formSuccess.value = ''
      currentId.value = null
      isEditing.value = false
    }

    const copied = reactive({})

    const copyName = async (scanner) => {
      try {
        const text = scanner?.name || ''
        if (!text) return
        if (navigator.clipboard && navigator.clipboard.writeText) {
          await navigator.clipboard.writeText(text)
        } else {
          const textarea = document.createElement('textarea')
          textarea.value = text
          textarea.setAttribute('readonly', '')
          textarea.style.position = 'absolute'
          textarea.style.left = '-9999px'
          document.body.appendChild(textarea)
          textarea.select()
          document.execCommand('copy')
          document.body.removeChild(textarea)
        }
        copied[scanner.scannerId] = true
        setTimeout(() => {
          copied[scanner.scannerId] = false
        }, 1400)
      } catch (err) {
        console.error('Failed to copy scanner name', err)
      }
    }

    const validate = () => {
      errors.name = ''
      errors.deviceId = ''
      formError.value = ''

      if (!form.name || form.name.trim().length < 3) {
        errors.name = 'Name is required and should be at least 3 characters.'
      }

      if (!form.deviceId || form.deviceId.trim().length < 3) {
        errors.deviceId = 'Device ID is required and should be at least 3 characters.'
      }

      return !errors.name && !errors.deviceId
    }

    const fetchScanners = async () => {
      loading.value = true
      try {
        const response = await api.get('/scanners')
        scanners.value = Array.isArray(response.data) ? response.data : []
      } catch (err) {
        console.error('Failed to fetch scanners', err)
        scanners.value = []
      } finally {
        loading.value = false
      }
    }

    const filteredScanners = computed(() => {
      const term = scannerFilter.value.trim().toLowerCase()
      if (!term) return scanners.value

      return scanners.value.filter((scanner) => {
        const name = (scanner.name || '').toLowerCase()
        const deviceId = (scanner.deviceId || '').toLowerCase()
        const status = (scanner.status || 'Active').toLowerCase()

        return (
          name.includes(term) ||
          deviceId.includes(term) ||
          status.includes(term)
        )
      })
    })

    const totalPages = computed(() => Math.ceil(filteredScanners.value.length / itemsPerPage.value) || 1)
    
    const paginatedScanners = computed(() => {
      const start = (currentPage.value - 1) * itemsPerPage.value
      const end = start + itemsPerPage.value
      return filteredScanners.value.slice(start, end)
    })

    const openCreate = () => {
      resetForm()
      showModal.value = true
    }

    const openEdit = (scanner) => {
      resetForm()
      isEditing.value = true
      currentId.value = scanner.scannerId
      form.name = scanner.name
      form.deviceId = scanner.deviceId
      form.status = scanner.status || 'Active'
      showModal.value = true
    }

    const closeModal = () => {
      showModal.value = false
    }

    const submitForm = async () => {
      if (!validate()) return

      submitting.value = true
      formError.value = ''
      formSuccess.value = ''

      try {
        const payload = {
          name: form.name.trim(),
          deviceId: form.deviceId.trim(),
          status: form.status
        }

        if (isEditing.value && currentId.value != null) {
          await api.put(`/scanners/${currentId.value}`, payload)
          formSuccess.value = 'Scanner updated successfully.'
        } else {
          await api.post('/scanners', payload)
          formSuccess.value = 'Scanner created successfully.'
        }

        await fetchScanners()
        setTimeout(() => {
          showModal.value = false
        }, 400)
      } catch (err) {
        console.error('Failed to save scanner', err)
        const message = err?.response?.data?.message || 'Failed to save scanner. Please try again.'
        formError.value = message
      } finally {
        submitting.value = false
      }
    }

    onMounted(() => {
      fetchScanners()
    })

    return {
      scanners,
      loading,
      showModal,
      isEditing,
      submitting,
      scannerFilter,
      form,
      errors,
      formError,
      formSuccess,
      filteredScanners,
      paginatedScanners,
      currentPage,
      itemsPerPage,
      totalPages,
      openCreate,
      openEdit,
      closeModal,
      submitForm,
      copied,
      copyName
    }
  }
}
</script>

<style>
.scanners-page-content {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
  width: 100%;
}

.scanners-section .filters-bar {
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
  gap: 1rem;
  margin-bottom: 1.5rem;
}

.scanners-section .filter-group {
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
  flex: 1;
}

.scanners-section .filter-actions {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.scanners-section {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.results-info {
  font-size: 0.85rem;
  color: var(--text-muted);
  margin-bottom: 1rem;
}

.scanners-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
  gap: 1.5rem;
}

.scanner-card {
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  background: var(--card-bg, #ffffff);
  border: 2px solid var(--border-color);
  border-radius: 12px;
  padding: 1.25rem;
  position: relative;
  overflow: hidden;
  box-shadow: 0 2px 8px rgba(0, 61, 107, 0.06);
  transition: all 0.22s ease;
}

.scanner-card::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 4px;
  background: linear-gradient(90deg, var(--primary-dark) 0%, var(--primary-light) 100%);
}

.scanner-card-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 1rem;
}

.scanner-badges {
  display: flex;
  gap: 0.5rem;
  margin-bottom: 0.8rem;
  flex-wrap: wrap;
}
.scanner-badge {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  padding: 0.35rem 0.85rem;
  border-radius: 999px;
  font-size: 0.75rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.4px;
  color: var(--badge-pill-text);
  background: var(--badge-neutral-bg);
  border: none;
  box-shadow: 0 6px 18px rgba(0, 0, 0, 0.12);
  transition: transform 0.2s ease;
}
.scanner-badge.active {
  background: var(--badge-active-gradient);
  color: #064e3b;
  box-shadow: 0 8px 20px var(--badge-active-glow);
}
.scanner-badge.inactive {
  background: var(--badge-inactive-gradient);
}
.scanner-badge:hover {
  transform: translateY(-1px);
}

.scanner-card-footer {
  display: flex;
  gap: 0.75rem;
  align-items: center;
  justify-content: flex-start;
  margin-top: 0.75rem;
}

.edit-scanner-btn {
  background: transparent;
  color: var(--primary-dark);
  border: 1px solid var(--border-color);
  padding: 0.35rem 0.7rem;
  border-radius: 8px;
  font-size: 0.9rem;
  font-weight: 600;
  cursor: pointer;
  min-width: 64px;
}

.scanner-card-footer .primary-action-btn {
  flex: 1 1 auto;
}

.scanner-name {
  margin: 0 0 0.25rem;
  font-size: 1.1rem;
  font-weight: 600;
  color: var(--primary-dark);
  display: flex;
  align-items: center;
  gap: 0.4rem;
}
.scanner-name-text {
  flex: 1;
}
.copy-name-btn {
  border: 1px solid var(--border-color);
  background: transparent;
  border-radius: 999px;
  padding: 0.15rem 0.4rem;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  color: var(--text-muted);
  transition: all 0.2s ease;
}
.copy-name-btn:hover {
  border-color: var(--primary-light);
  color: var(--primary-light);
}
.copy-name-btn svg {
  display: block;
}

.scanner-card:hover {
  border-color: var(--primary-light);
  box-shadow: 0 8px 24px rgba(30, 144, 255, 0.15);
  transform: translateY(-2px);
}

[data-theme="dark"] .scanner-card {
  background: var(--bg-secondary);
  border-color: var(--border-color);
  box-shadow: 0 10px 30px rgba(0,0,0,0.55);
}

[data-theme="dark"] .scanner-card::before {
  background: linear-gradient(90deg, var(--primary-dark) 0%, var(--primary-light) 100%);
}

[data-theme="dark"] .scanner-name {
  color: var(--text-primary);
}

[data-theme="dark"] .scanner-device {
  color: var(--text-tertiary);
}

[data-theme="dark"] .copy-name-btn {
  border-color: rgba(255,255,255,0.06);
  color: var(--text-secondary);
}

[data-theme="dark"] .scanner-card-footer {
  border-top: 1px solid rgba(255,255,255,0.02);
}

.add-scanner-btn {
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

.add-scanner-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(30, 144, 255, 0.4);
}

/* Shared modal styling (mirrors user management view) */
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

.scanner-form {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.field-error {
  margin-top: 0.25rem;
  color: #f97373;
  font-size: 0.8rem;
}

.form-message {
  margin-top: 0.5rem;
  font-size: 0.9rem;
}

.form-message.error {
  color: #f97373;
}

.form-message.success {
  color: #22c55e;
}

.form-actions {
  display: flex;
  gap: 0.75rem;
  justify-content: flex-end;
  margin-top: 0.5rem;
}

[data-theme='dark'] .table-section {
  background-color: var(--surface-elevated);
}

[data-theme='dark'] .form-modal {
  background-color: var(--surface-elevated);
}

[data-theme='dark'] .form-input {
  background-color: var(--form-bg);
  border-color: var(--border-subtle);
  color: var(--text-primary);
}

.form-input.has-error {
  border-color: #f97373;
}

/* Pagination Controls */
.pagination-controls {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  padding: 1.5rem 0;
  margin-top: 1rem;
}

.pagination-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 36px;
  height: 36px;
  border: 1px solid var(--border-color);
  border-radius: 8px;
  background: var(--bg-secondary);
  color: var(--text-primary);
  cursor: pointer;
  transition: all 0.2s ease;
}

.pagination-btn:hover:not(:disabled) {
  background: var(--primary-light);
  color: white;
  border-color: var(--primary-light);
}

.pagination-btn:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

.pagination-info {
  padding: 0 1rem;
  font-size: 0.9rem;
  font-weight: 500;
  color: var(--text-secondary);
}

.page-size-select {
  margin-left: 1rem;
  padding: 0.5rem 0.75rem;
  border: 1px solid var(--border-color);
  border-radius: 8px;
  background: var(--bg-secondary);
  color: var(--text-primary);
  font-size: 0.85rem;
  cursor: pointer;
}

.page-size-select:focus {
  outline: none;
  border-color: var(--primary-light);
}

@media (max-width: 600px) {
  .pagination-controls {
    flex-wrap: wrap;
    gap: 0.75rem;
  }
  
  .page-size-select {
    margin-left: 0;
    margin-top: 0.5rem;
  }
}
</style>
