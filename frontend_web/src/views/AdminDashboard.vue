<template>
  <AdminShell
    title="Overview"
    subtitle="Warehouse Inventory Overview"
    active-tab="dashboard"
  >
    <div class="dashboard-page-content">
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

          <div class="summary-card surface-card surface-card--compact borrowed">
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
            <span class="item-count">{{ borrowedItems.length }} items</span>
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
  </AdminShell>
</template>

<script>
import { ref, computed, onMounted } from 'vue'
import api from '../services/api'
import AdminShell from '../components/AdminShell.vue'

export default {
  name: 'AdminDashboard',
  components: {
    AdminShell
  },
  setup() {
    const items = ref([])
    const loading = ref(true)
    const transactions = ref([])

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
      formatDate
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
  background: linear-gradient(135deg, var(--primary-light) 0%, var(--accent-green) 100%);
  color: white;
  padding: 0.4rem 0.8rem;
  border-radius: 20px;
  font-size: 0.85rem;
  font-weight: 600;
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
</style>
