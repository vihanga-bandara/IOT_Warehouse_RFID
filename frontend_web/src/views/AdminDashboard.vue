<template>
  <AdminShell
    title="Overview"
    subtitle="Warehouse Inventory Overview"
    active-tab="dashboard"
  >
    <div class="dashboard-page-content">
      <div v-if="showKioskHint" class="info-banner surface-card surface-card--compact">
        <div class="info-banner-icon">
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <circle cx="12" cy="12" r="10" />
            <line x1="12" y1="8" x2="12" y2="12" />
            <line x1="12" y1="16" x2="12.01" y2="16" />
          </svg>
        </div>
        <div class="info-banner-content">
          <p class="info-banner-title">Kiosk requires a scanner selection</p>
          <p class="info-banner-text">
            To use the kiosk as an admin, please log out and log in again with a scanner name selected. Regular users must always log in with a scanner name.
          </p>
        </div>
        <button class="info-banner-close" @click="dismissKioskHint" aria-label="Dismiss">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <line x1="18" y1="6" x2="6" y2="18" />
            <line x1="6" y1="6" x2="18" y2="18" />
          </svg>
        </button>
      </div>
      <div v-if="loading" class="loading-state surface-card surface-card--padded">
        <div class="spinner"></div>
        <p>Loading inventory...</p>
      </div>

      <div v-else>
        <div class="summary-grid">
          <div class="summary-card surface-card surface-card--compact total">
            <div class="summary-icon">
              <svg
                width="24"
                height="24"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                stroke-width="2"
              >
                <path d="M12 2v20M2 12h20" />
              </svg>
            </div>
            <div class="summary-content">
              <div class="summary-label">Total Items</div>
              <div class="summary-number">{{ totalItems }}</div>
            </div>
          </div>

          <div class="summary-card surface-card surface-card--compact borrowed clickable" @click="openBorrowedModal">
            <div class="summary-icon">
              <svg
                width="24"
                height="24"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                stroke-width="2"
              >
                <path
                  d="M13 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V9z"
                />
                <polyline points="13 2 13 9 20 9" />
              </svg>
            </div>
            <div class="summary-content">
              <div class="summary-label">Borrowed</div>
              <div class="summary-number">{{ borrowedItems.length }}</div>
              <div class="summary-hint">Click to view details</div>
            </div>
          </div>

          <div class="summary-card surface-card surface-card--compact available">
            <div class="summary-icon">
              <svg
                width="24"
                height="24"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                stroke-width="2"
              >
                <path
                  d="M21.5 2v6h-6M2.5 22v-6h6M2 11.5a10 10 0 0 1 18.8-4.3M22 12.5a10 10 0 0 1-18.8 4.2"
                />
              </svg>
            </div>
            <div class="summary-content">
              <div class="summary-label">Available</div>
              <div class="summary-number">{{ availableItems }}</div>
            </div>
          </div>
        </div>

        <div class="inventory-section surface-card surface-card--padded">
          <div class="section-header">
            <h2>Currently Borrowed Items</h2>
            <span class="item-count">{{ borrowedItems.length }} item<span v-if="borrowedItems.length !== 1">s</span></span>
          </div>

          <div
            v-if="borrowedItems.length === 0"
            class="empty-state surface-card surface-card--padded"
          >
            <svg
              width="48"
              height="48"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              stroke-width="1.5"
            >
              <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2" />
              <circle cx="12" cy="7" r="4" />
            </svg>
            <p>No items currently borrowed</p>
          </div>

          <div v-else class="items-grid">
            <div
              v-for="item in borrowedItems"
              :key="item.id"
              class="item-card surface-card surface-card--padded"
            >
              <div class="item-badge">Borrowed</div>
              <h4 class="item-name">{{ item.itemName }}</h4>
              <div class="item-details">
                <div class="detail-row">
                  <span class="detail-label">Holder:</span>
                  <span class="detail-value">{{ item.currentHolderName }}</span>
                </div>
                <div class="detail-row">
                  <span class="detail-label">Email:</span>
                  <span class="detail-value">{{ item.currentHolderEmail }}</span>
                </div>
                <div class="detail-row">
                  <span class="detail-label">RFID:</span>
                  <span class="detail-value mono">{{ item.rfidUid }}</span>
                </div>
              </div>
              <div class="item-footer">
                <small>Updated: {{ formatDate(item.lastUpdated) }}</small>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Borrowed Items Modal -->
    <transition name="modal-fade">
      <div v-if="showBorrowedModal" class="modal-overlay" @click="closeBorrowedModal">
        <div class="borrowed-modal form-modal" @click.stop>
          <button class="modal-close" @click="closeBorrowedModal" aria-label="Close">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <line x1="18" y1="6" x2="6" y2="18"/>
              <line x1="6" y1="6" x2="18" y2="18"/>
            </svg>
          </button>
          
          <h2>Borrowed Items</h2>
          <p class="modal-subtitle">View all currently borrowed items and manage overdue reminders</p>

          <div v-if="borrowedModalMessage" class="form-message" :class="borrowedModalMessageType">
            {{ borrowedModalMessage }}
          </div>

          <div v-if="borrowedDetailsLoading" class="loading-state">
            <div class="spinner"></div>
            <p>Loading borrowed items...</p>
          </div>

          <div v-else-if="borrowedDetails.length === 0" class="empty-state">
            <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
              <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2" />
              <circle cx="12" cy="7" r="4" />
            </svg>
            <p>No items currently borrowed</p>
          </div>

          <div v-else class="borrowed-items-list">
            <div 
              v-for="item in borrowedDetails" 
              :key="item.itemId" 
              class="borrowed-item-card"
              :class="{ 'is-overdue': item.isOverdue }"
            >
              <div class="item-info">
                <div class="item-header">
                  <h4>{{ item.itemName }}</h4>
                  <span v-if="item.isOverdue" class="overdue-badge">
                    Overdue {{ item.daysOverdue }} day{{ item.daysOverdue > 1 ? 's' : '' }}
                  </span>
                </div>
                <div class="item-meta">
                  <div class="meta-row">
                    <span class="meta-label">Holder:</span>
                    <span class="meta-value">{{ item.holderName }}</span>
                  </div>
                  <div class="meta-row">
                    <span class="meta-label">Email:</span>
                    <span class="meta-value">{{ item.holderEmail }}</span>
                  </div>
                  <div class="meta-row">
                    <span class="meta-label">Borrowed:</span>
                    <span class="meta-value">{{ formatDate(item.borrowedAt) }} ({{ item.daysBorrowed }} day{{ item.daysBorrowed !== 1 ? 's' : '' }} ago)</span>
                  </div>
                  <div v-if="item.reminderSent" class="meta-row reminder-sent">
                    <span class="meta-label">Reminder Sent:</span>
                    <span class="meta-value">
                      <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <polyline points="20 6 9 17 4 12"/>
                      </svg>
                      {{ formatDate(item.reminderSentAt) }}
                    </span>
                  </div>
                </div>
              </div>
              <div class="item-actions">
                <button 
                  class="btn btn-send-reminder"
                  :class="{ 'btn-resend': item.reminderSent }"
                  @click="sendReminder(item)"
                  :disabled="sendingReminder === item.itemId"
                >
                  <svg v-if="sendingReminder !== item.itemId" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"/>
                    <polyline points="22,6 12,13 2,6"/>
                  </svg>
                  <span v-if="sendingReminder === item.itemId">Sending...</span>
                  <span v-else-if="item.reminderSent">Resend Reminder</span>
                  <span v-else>Send Reminder</span>
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </transition>
  </AdminShell>
</template>

<script>
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import api from '../services/api'
import AdminShell from '../components/AdminShell.vue'

export default {
  name: 'AdminDashboard',
  components: {
    AdminShell
  },
  setup() {
    const route = useRoute()
    const router = useRouter()
    const items = ref([])
    const loading = ref(true)
    const transactions = ref([])
    const showKioskHint = ref(route.query.kioskBlocked === '1')
    
    // Borrowed modal state
    const showBorrowedModal = ref(false)
    const borrowedDetails = ref([])
    const borrowedDetailsLoading = ref(false)
    const sendingReminder = ref(null)
    const borrowedModalMessage = ref('')
    const borrowedModalMessageType = ref('')

    const latestActionByItem = computed(() => {
      const map = new Map()
      transactions.value.forEach((tx) => {
        const ts = new Date(tx.timestamp || tx.createdAt || Date.now()).getTime()
        const existing = map.get(tx.itemId)
        if (!existing || ts > existing.ts) {
          map.set(tx.itemId, {
            action: tx.action || '',
            ts
          })
        }
      })
      return map
    })

    const derivedBorrowState = (item) => {
      const latest = latestActionByItem.value.get(item.id || item.itemId)
      if (latest) {
        const a = (latest.action || '').toLowerCase()
        if (a.includes('checkin') || a.includes('return')) return false
        if (a.includes('checkout') || a.includes('borrow')) return true
      }
      const status = (item?.status || '').toString().toLowerCase()
      if (!status) return false
      if (status.includes('borrow')) return true
      if (status.includes('checkout') || status.includes('checkedout')) return true
      if (status === 'unavailable' || status === 'out') return true
      return false
    }

    const borrowedItems = computed(() =>
      items.value.filter((item) => derivedBorrowState(item))
    )

    const totalItems = computed(() => items.value.length)

    const availableItems = computed(() => {
      const availableCount = items.value.filter((item) => !derivedBorrowState(item)).length
      const fallback = Math.max(totalItems.value - borrowedItems.value.length, 0)
      return availableCount || fallback
    })

    const formatDate = (value) => {
      if (!value) return '—'
      return new Date(value).toLocaleString()
    }

    const fetchItems = async () => {
      try {
        const response = await api.get('/items')
        items.value = Array.isArray(response.data)
          ? response.data
          : response.data?.items ?? []
      } catch (err) {
        console.error('Failed to fetch items:', err)
        items.value = []
      }
    }

    const fetchTransactions = async () => {
      try {
        const response = await api.get('/transaction/all')
        const payload = response?.data?.data ?? response?.data ?? []
        const list = Array.isArray(payload) ? payload : []

        transactions.value = list.map((tx, idx) => ({
          id: tx.transactionId ?? tx.id ?? idx,
          itemId: tx.item?.itemId ?? tx.itemId,
          action: tx.action ?? '',
          timestamp: tx.timestamp ?? tx.createdAt ?? new Date().toISOString()
        }))
      } catch (err) {
        console.error('Failed to fetch transactions for dashboard:', err)
        transactions.value = []
      }
    }

    const dismissKioskHint = () => {
      showKioskHint.value = false
      if (route.query.kioskBlocked) {
        router.replace({
          path: route.path,
          query: { ...route.query, kioskBlocked: undefined }
        })
      }
    }

    const openBorrowedModal = async () => {
      showBorrowedModal.value = true
      borrowedDetailsLoading.value = true
      borrowedModalMessage.value = ''
      borrowedModalMessageType.value = ''
      try {
        const response = await api.get('/items/borrowed')
        borrowedDetails.value = response.data
      } catch (err) {
        console.error('Failed to fetch borrowed items details:', err)
        borrowedDetails.value = []
      } finally {
        borrowedDetailsLoading.value = false
      }
    }

    const closeBorrowedModal = () => {
      showBorrowedModal.value = false
      borrowedDetails.value = []
      borrowedModalMessage.value = ''
      borrowedModalMessageType.value = ''
    }

    const sendReminder = async (item) => {
      if (sendingReminder.value) return
      sendingReminder.value = item.itemId
      borrowedModalMessage.value = ''
      borrowedModalMessageType.value = ''
      try {
        await api.post(`/items/${item.itemId}/send-reminder`)
        // Update the item in the list
        const idx = borrowedDetails.value.findIndex(i => i.itemId === item.itemId)
        if (idx !== -1) {
          borrowedDetails.value[idx].reminderSent = true
          borrowedDetails.value[idx].reminderSentAt = new Date().toISOString()
        }
        borrowedModalMessageType.value = 'success'
        borrowedModalMessage.value = `Reminder sent successfully to ${item.holderEmail}.`
      } catch (err) {
        console.error('Failed to send reminder:', err)
        borrowedModalMessageType.value = 'error'
        borrowedModalMessage.value = 'Failed to send reminder email. Please try again.'
      } finally {
        sendingReminder.value = null
      }
    }

    onMounted(async () => {
      loading.value = true
      await Promise.all([fetchItems(), fetchTransactions()])
      loading.value = false
    })

    return {
      items,
      loading,
      borrowedItems,
      totalItems,
      availableItems,
      formatDate,
      showKioskHint,
      dismissKioskHint,
      // Borrowed modal
      showBorrowedModal,
      borrowedDetails,
      borrowedDetailsLoading,
      sendingReminder,
      borrowedModalMessage,
      borrowedModalMessageType,
      openBorrowedModal,
      closeBorrowedModal,
      sendReminder
    }
  }
}
</script>

<style scoped>
.dashboard-page {
  width: 100%;
  min-height: 100vh;
  background: var(--gradient-page-bg);
  padding: 2rem 1rem;
}

.dashboard-container {
  max-width: 1400px;
  margin: 0 auto;
}

.info-banner {
  margin-bottom: 1.5rem;
  padding: 1rem 1.25rem;
  border-radius: 12px;
  background: linear-gradient(135deg, rgba(255, 193, 7, 0.12) 0%, rgba(255, 193, 7, 0.06) 100%);
  border: 1px solid rgba(255, 193, 7, 0.4);
  display: flex;
  align-items: flex-start;
  gap: 0.75rem;
}

.info-banner-icon {
  margin-top: 0.1rem;
  color: #ffb300;
}

.info-banner-content {
  flex: 1;
}

.info-banner-title {
  margin: 0 0 0.25rem;
  font-weight: 700;
  color: var(--primary-dark);
}

.info-banner-text {
  margin: 0;
  font-size: 0.9rem;
  color: var(--accent-gray);
}

.info-banner-close {
  border: none;
  background: transparent;
  color: var(--accent-gray);
  cursor: pointer;
  padding: 0.25rem;
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
  to {
    transform: rotate(360deg);
  }
}

.loading-state p {
  color: var(--accent-gray);
  font-weight: 600;
}

.summary-grid {

.form-message {
  margin: 0.75rem 0 0.25rem;
  font-size: 0.9rem;
}

.form-message.error {
  color: var(--color-error);
}

.form-message.success {
  color: var(--color-success);
}
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
  gap: 1.5rem;
  margin-bottom: 2rem;
}

.summary-card {
  display: flex;
  align-items: center;
  gap: 1.5rem;
  padding: 1.5rem;
  border-left: 4px solid var(--border-color);
  transition: transform 0.2s ease, box-shadow 0.2s ease;
}

.summary-card:hover {
  transform: translateY(-4px);
  box-shadow: var(--shadow-lg);
}

.summary-card.total {
  border-left-color: var(--primary-light);
}

.summary-card.borrowed {
  border-left-color: #ff6b6b;
}

.summary-card.available {
  border-left-color: var(--accent-green);
}

.summary-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 64px;
  height: 64px;
  border-radius: 16px;
  color: white;
  flex-shrink: 0;
}

.summary-card.total .summary-icon {
  background: linear-gradient(135deg, var(--primary-dark) 0%, var(--primary-light) 100%);
  box-shadow: 0 4px 12px rgba(0, 61, 107, 0.2);
}

.summary-card.borrowed .summary-icon {
  background: linear-gradient(135deg, #ff6b6b 0%, #ff8787 100%);
  box-shadow: 0 4px 12px rgba(255, 107, 107, 0.2);
}

.summary-card.available .summary-icon {
  background: linear-gradient(135deg, var(--accent-green) 0%, #45a049 100%);
  box-shadow: 0 4px 12px rgba(76, 175, 80, 0.2);
}

.summary-content {
  flex: 1;
  display: flex;
  flex-direction: column;
  justify-content: center;
}

.summary-label {
  color: var(--accent-gray);
  font-size: 0.85rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 1px;
  margin-bottom: 0.25rem;
}

.summary-number {
  color: var(--primary-dark);
  font-size: 2.5rem;
  font-weight: 800;
  line-height: 1.1;
}

@media (max-width: 600px) {
  .summary-grid {
    grid-template-columns: 1fr;
  }

  .summary-number {
    font-size: 1.6rem;
  }
}

.inventory-section {
  animation: slideUp 0.4s ease-out;
}

@keyframes slideUp {
  from {
    opacity: 0;
    transform: translateY(20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
  padding-bottom: 1rem;
  border-bottom: 2px solid var(--border-color);
}

.section-header h2 {
  color: var(--primary-dark);
  margin: 0;
  font-size: 1.5rem;
  font-weight: 700;
}

.item-count {
  background: var(--bg-secondary);
  color: var(--text-primary);
  padding: 0.35rem 0.7rem;
  border-radius: 999px;
  font-size: 0.92rem;
  font-weight: 700;
  border: 1px solid var(--border-color);
  pointer-events: none;
  user-select: none;
}

@media (max-width: 600px) {
  .inventory-section {
    padding: 1.5rem;
  }

  .section-header h2 {
    font-size: 1.2rem;
  }
}

.empty-state {
  text-align: center;
  color: var(--accent-gray);
  padding: 3rem 2rem;
  border: 2px dashed var(--border-color);
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

.items-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 1.5rem;
}

.item-card {
  position: relative;
  overflow: hidden;
  transition: transform var(--transition-normal),
    box-shadow var(--transition-normal),
    border-color var(--transition-fast);
}

.item-card::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 4px;
  background: linear-gradient(90deg, var(--primary-dark) 0%, var(--primary-light) 100%);
}

.item-card:hover {
  border-color: var(--primary-light);
  box-shadow: var(--shadow-lg);
  transform: translateY(-2px);
}

.item-badge {
  display: inline-block;
  background: linear-gradient(135deg, #ff6b6b 0%, #ff8787 100%);
  color: white;
  padding: 0.4rem 0.8rem;
  border-radius: 6px;
  font-size: 0.75rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  margin-bottom: 0.8rem;
}

.item-name {
  color: var(--primary-dark);
  margin: 0 0 1rem;
  font-size: 1.1rem;
  font-weight: 700;
  line-height: 1.4;
}

.item-details {
  display: flex;
  flex-direction: column;
  gap: 0.6rem;
  margin-bottom: 1rem;
}

.detail-row {
  display: flex;
  gap: 0.5rem;
  font-size: 0.9rem;
}

.detail-label {
  color: var(--accent-gray);
  font-weight: 600;
  min-width: 70px;
}

.detail-value {
  color: var(--dark-text);
  font-weight: 500;
  word-break: break-word;
}

.detail-value.mono {
  font-family: 'Monaco', 'Menlo', monospace;
  font-size: 0.85rem;
  background: var(--bg-light);
  padding: 0.2rem 0.4rem;
  border-radius: 4px;
}

.item-footer {
  padding-top: 1rem;
  border-top: 1px solid var(--border-color);
  color: var(--accent-gray);
  font-size: 0.8rem;
  font-weight: 500;
}

@media (max-width: 600px) {
  .items-grid {
    grid-template-columns: 1fr;
  }
}

/* Clickable summary card */
.summary-card.clickable {
  cursor: pointer;
}

.summary-card.clickable:hover {
  transform: translateY(-6px);
  box-shadow: var(--shadow-lg), 0 0 0 2px var(--primary-light);
}

.summary-hint {
  font-size: 0.7rem;
  color: var(--accent-gray);
  margin-top: 0.25rem;
  opacity: 0.8;
}

/* Modal styles */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.6);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  padding: 1rem;
}

.borrowed-modal {
  background: var(--bg-secondary);
  border-radius: 16px;
  padding: 2rem;
  max-width: 800px;
  width: 100%;
  max-height: 80vh;
  overflow-y: auto;
  position: relative;
  box-shadow: var(--shadow-lg);
}

.borrowed-modal h2 {
  margin: 0 0 0.5rem;
  color: var(--primary-dark);
  font-size: 1.5rem;
}

.modal-subtitle {
  color: var(--accent-gray);
  margin: 0 0 1.5rem;
  font-size: 0.9rem;
}

.modal-close {
  position: absolute;
  top: 1rem;
  right: 1rem;
  background: none;
  border: none;
  color: var(--accent-gray);
  cursor: pointer;
  padding: 0.5rem;
  border-radius: 8px;
  transition: all 0.2s ease;
}

.modal-close:hover {
  background: var(--bg-light);
  color: var(--primary-dark);
}

.borrowed-items-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.borrowed-item-card {
  background: var(--bg-light);
  border: 1px solid var(--border-color);
  border-radius: 12px;
  padding: 1rem 1.25rem;
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 1rem;
  transition: all 0.2s ease;
}

.borrowed-item-card:hover {
  border-color: var(--primary-light);
  box-shadow: var(--shadow-sm);
}

.borrowed-item-card.is-overdue {
  border-left: 4px solid #ff6b6b;
  background: linear-gradient(135deg, rgba(255, 107, 107, 0.05) 0%, var(--bg-light) 100%);
}

.item-info {
  flex: 1;
}

.item-header {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  margin-bottom: 0.75rem;
}

.item-header h4 {
  margin: 0;
  color: var(--primary-dark);
  font-size: 1rem;
  font-weight: 600;
}

.overdue-badge {
  background: linear-gradient(135deg, #ff6b6b 0%, #ff8787 100%);
  color: white;
  padding: 0.25rem 0.6rem;
  border-radius: 4px;
  font-size: 0.7rem;
  font-weight: 700;
  text-transform: uppercase;
}

.item-meta {
  display: flex;
  flex-direction: column;
  gap: 0.4rem;
}

.meta-row {
  display: flex;
  gap: 0.5rem;
  font-size: 0.85rem;
}

.meta-label {
  color: var(--accent-gray);
  font-weight: 600;
  min-width: 100px;
}

.meta-value {
  color: var(--text-primary);
}

.meta-row.reminder-sent .meta-value {
  display: flex;
  align-items: center;
  gap: 0.35rem;
  color: var(--accent-green);
}

.item-actions {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.btn-send-reminder {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.6rem 1rem;
  background: linear-gradient(135deg, var(--primary-dark) 0%, var(--primary-light) 100%);
  color: white;
  border: none;
  border-radius: 8px;
  font-size: 0.85rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
  white-space: nowrap;
}

.btn-send-reminder:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0, 61, 107, 0.3);
}

.btn-send-reminder:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-send-reminder.btn-resend {
  background: linear-gradient(135deg, #ff9800 0%, #ffb74d 100%);
}

.btn-send-reminder.btn-resend:hover:not(:disabled) {
  box-shadow: 0 4px 12px rgba(255, 152, 0, 0.3);
}

/* Modal transitions */
.modal-fade-enter-active,
.modal-fade-leave-active {
  transition: opacity 0.3s ease;
}

.modal-fade-enter-from,
.modal-fade-leave-to {
  opacity: 0;
}

.modal-fade-enter-active .borrowed-modal,
.modal-fade-leave-active .borrowed-modal {
  transition: transform 0.3s ease;
}

.modal-fade-enter-from .borrowed-modal,
.modal-fade-leave-to .borrowed-modal {
  transform: scale(0.95) translateY(20px);
}

/* Loading and empty states in modal */
.borrowed-modal .loading-state,
.borrowed-modal .empty-state {
  text-align: center;
  padding: 3rem 2rem;
  color: var(--accent-gray);
}

.borrowed-modal .spinner {
  width: 40px;
  height: 40px;
  border: 3px solid var(--border-color);
  border-top-color: var(--primary-light);
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin: 0 auto 1rem;
}

@media (max-width: 600px) {
  .borrowed-item-card {
    flex-direction: column;
  }
  
  .item-actions {
    width: 100%;
  }
  
  .btn-send-reminder {
    width: 100%;
    justify-content: center;
  }
}
</style>
