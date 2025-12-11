<template>
  <div class="kiosk-page">
    <div class="kiosk-container surface-card surface-card--padded">
      <div class="kiosk-header">
        <div class="header-content">
          <h1>
            <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/>
            </svg>
            My History
          </h1>
          <p class="scan-status">
            Transaction records
          </p>
        </div>
        <router-link to="/kiosk" class="logout-btn">
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M19 12H5M12 19l-7-7 7-7" />
          </svg>
          Back to Scanner
        </router-link>
      </div>

      <div v-if="loading" class="loading-state">
        <div class="spinner"></div>
        <p>Loading history...</p>
      </div>

      <div v-else-if="transactions.length === 0" class="empty-cart surface-card surface-card--padded">
        <svg width="64" height="64" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
          <path d="M21.5 2v6h-6M2.5 22v-6h6M2 11.5a10 10 0 0 1 18.8-4.3M22 12.5a10 10 0 0 1-18.8 4.2" />
        </svg>
        <p>No transactions yet</p>
        <small>Use the RFID scanner to start borrowing items</small>
      </div>

      <div v-else class="cart-display surface-card surface-card--padded" style="min-height: auto;">
        <div class="cart-header">
          <h2>Transactions</h2>
          <span class="item-badge">{{ transactions.length }} records</span>
        </div>

        <div class="cart-items">
          <div
            v-for="tx in transactions"
            :key="tx.id"
            class="cart-item"
            :class="actionClass(tx.action)"
          >
            <div class="item-content">
              <div class="item-header">
                <h3 class="item-name">{{ tx.itemName }}</h3>
                <span :class="['action-badge', actionClass(tx.action)]">
                  {{ actionLabel(tx.action) }}
                </span>
              </div>
              <div class="item-details">
                <div class="detail">
                  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <circle cx="12" cy="12" r="10" /><polyline points="12 6 12 12 16 14" />
                  </svg>
                  <span>{{ formatDate(tx.timestamp) }}</span>
                </div>
                <div class="detail">
                  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z" /><polyline points="9 22 9 12 15 12 15 22" />
                  </svg>
                  <span>{{ tx.deviceName }}</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, onMounted } from 'vue'
import api from '../services/api'
import { useTheme } from '../composables/useTheme'

export default {
  name: 'UserHistory',
  setup() {
    const transactions = ref([])
    const loading = ref(true)
    const { isDark, toggleTheme } = useTheme()

    const fetchHistory = async () => {
      try {
        const response = await api.get('/transaction/history')
        transactions.value = response.data
      } catch (err) {
        console.error('Failed to fetch history:', err)
      } finally {
        loading.value = false
      }
    }

    const formatDate = (timestamp) => new Date(timestamp).toLocaleString()

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

    onMounted(() => {
      // Force light mode
      if (isDark.value) {
        toggleTheme()
      }
      fetchHistory()
    })

    return {
      transactions,
      loading,
      formatDate,
      actionLabel,
      actionClass
    }
  }
}
</script>

<style scoped>
/* Reuse Kiosk styles for consistency */
.kiosk-page {
  width: 100%;
  min-height: 100vh;
  background: var(--gradient-page-bg);
  padding: 1rem;
  display: flex;
  align-items: center;
  justify-content: center;
}

.kiosk-container {
  background: var(--bg-primary);
  border-radius: 16px;
  box-shadow: 0 4px 16px rgba(0, 61, 107, 0.08);
  width: 100%;
  max-width: 700px;
  padding: 2rem;
  animation: slideUp 0.4s ease-out;
  display: flex;
  flex-direction: column;
  gap: 2rem;
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

.kiosk-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-bottom: 1.5rem;
  border-bottom: 2px solid var(--border-color);
}

.header-content {
  text-align: center;
  flex: 1;
}

.kiosk-header h1 {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.75rem;
  color: var(--primary-dark);
  margin: 0 0 0.5rem;
  font-size: 1.8rem;
  font-weight: 800;
}

.scan-status {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  color: var(--accent-green);
  margin: 0;
  font-weight: 600;
  font-size: 1rem;
}

.logout-btn {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.75rem 1.5rem;
  background: var(--bg-secondary);
  color: var(--accent-gray);
  border: 1px solid var(--border-color);
  border-radius: 8px;
  font-size: 0.95rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  text-decoration: none;
}

.logout-btn:hover {
  background: var(--border-color);
  color: var(--primary-dark);
  transform: translateY(-2px);
}

.cart-display {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
  min-height: 400px;
  border-radius: 16px;
  border: 2px solid var(--border-color);
}

.cart-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.cart-header h2 {
  margin: 0;
  font-size: 1.2rem;
  color: var(--primary-dark);
}

.item-badge {
  background: var(--primary-light);
  color: white;
  padding: 0.25rem 0.75rem;
  border-radius: 20px;
  font-size: 0.85rem;
  font-weight: 700;
}

.empty-cart {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 1rem;
  color: var(--accent-gray);
  min-height: 300px;
  border-radius: 16px;
  border: 2px dashed var(--border-color);
}

.empty-cart p {
  font-size: 1.2rem;
  font-weight: 600;
  margin: 0;
}

.cart-items {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 1rem;
  overflow-y: auto;
  max-height: 500px;
}

.cart-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 1rem;
  border-radius: 10px;
  border-left: 4px solid;
  transition: all 0.3s ease;
  background: var(--bg-secondary);
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
}

.cart-item.borrow {
  border-left-color: var(--accent-green);
  background: linear-gradient(135deg, rgba(80, 200, 120, 0.08) 0%, rgba(80, 200, 120, 0.04) 100%);
}

.cart-item.return {
  border-left-color: var(--color-warning);
  background: linear-gradient(135deg, rgba(255, 193, 7, 0.08) 0%, rgba(255, 193, 7, 0.04) 100%);
}

.item-content {
  flex: 1;
}

.item-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 0.5rem;
}

.item-name {
  margin: 0;
  font-size: 1rem;
  font-weight: 700;
  color: var(--primary-dark);
}

.action-badge {
  font-size: 0.75rem;
  font-weight: 700;
  text-transform: uppercase;
  padding: 0.25rem 0.5rem;
  border-radius: 4px;
}

.action-badge.borrow {
  color: var(--accent-green);
  background: rgba(80, 200, 120, 0.1);
}

.action-badge.return {
  color: var(--color-warning);
  background: rgba(255, 193, 7, 0.1);
}

.item-details {
  display: flex;
  gap: 1rem;
}

.detail {
  display: flex;
  align-items: center;
  gap: 0.4rem;
  font-size: 0.85rem;
  color: var(--accent-gray);
}

.loading-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 3rem;
}

.spinner {
  width: 40px;
  height: 40px;
  border: 3px solid var(--border-color);
  border-top-color: var(--primary-light);
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin-bottom: 1rem;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

@media (max-width: 600px) {
  .kiosk-container {
    padding: 1.5rem;
  }
  
  .kiosk-header h1 {
    font-size: 1.4rem;
  }
}
</style>
