<template>
  <div class="history-page">
    <div class="history-container">
      <div class="page-header">
        <div>
          <h1>My Borrowing History</h1>
          <p class="page-subtitle">Transaction records and activity</p>
        </div>
        <router-link to="/kiosk" class="back-button">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M19 12H5M12 19l-7-7 7-7"/>
          </svg>
          Back to Scanner
        </router-link>
      </div>

      <div v-if="loading" class="loading-state">
        <div class="spinner"></div>
        <p>Loading history...</p>
      </div>

      <div v-else-if="transactions.length === 0" class="empty-state">
        <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
          <path d="M21.5 2v6h-6M2.5 22v-6h6M2 11.5a10 10 0 0 1 18.8-4.3M22 12.5a10 10 0 0 1-18.8 4.2"/>
        </svg>
        <p>No transactions yet</p>
        <small>Use the RFID scanner to start borrowing items</small>
      </div>

      <div v-else class="history-list">
        <div class="results-header">
          <span class="result-count">
            <strong>{{ transactions.length }}</strong> transaction<span v-if="transactions.length !== 1">s</span>
          </span>
        </div>

        <div v-for="tx in transactions" :key="tx.id" class="history-item" :class="tx.action.toLowerCase()">
          <div class="item-timeline">
            <span class="timeline-dot"></span>
          </div>
          <div class="item-content">
            <div class="item-header">
              <h3 class="item-name">{{ tx.itemName }}</h3>
              <span :class="['action-badge', tx.action.toLowerCase()]">
                {{ tx.action }}
              </span>
            </div>
            <div class="item-details">
              <div class="detail">
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                  <circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/>
                </svg>
                <span>{{ formatDate(tx.timestamp) }}</span>
              </div>
              <div class="detail">
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                  <path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/><polyline points="9 22 9 12 15 12 15 22"/>
                </svg>
                <span>{{ tx.deviceName }}</span>
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

export default {
  name: 'UserHistory',
  setup() {
    const transactions = ref([])
    const loading = ref(true)

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

    const formatDate = (timestamp) => {
      return new Date(timestamp).toLocaleString()
    }

    onMounted(() => {
      fetchHistory()
    })

    return {
      transactions,
      loading,
      formatDate
    }
  }
}
</script>

<style scoped>
.history-page {
  width: 100%;
  min-height: 100vh;
  background: var(--gradient-page-bg);
  padding: 2rem 1rem;
}

.history-container {
  max-width: 900px;
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

.back-button {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.75rem 1.5rem;
  background: linear-gradient(135deg, var(--primary-dark) 0%, var(--primary-light) 100%);
  color: white;
  text-decoration: none;
  border-radius: 10px;
  font-weight: 600;
  transition: all 0.3s ease;
  box-shadow: 0 4px 12px rgba(30, 144, 255, 0.3);
}

.back-button:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(30, 144, 255, 0.4);
}

@media (max-width: 600px) {
  .page-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 1rem;
  }

  .page-header h1 {
    font-size: 1.8rem;
  }

  .page-subtitle {
    font-size: 0.95rem;
  }

  .back-button {
    width: 100%;
    justify-content: center;
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

.empty-state {
  text-align: center;
  color: var(--accent-gray);
  padding: 3rem 2rem;
  background: var(--bg-secondary);
  border-radius: 16px;
  border: 2px dashed var(--border-color);
  box-shadow: 0 4px 16px rgba(0, 61, 107, 0.08);
}

.empty-state svg {
  opacity: 0.5;
  margin-bottom: 1rem;
}

.empty-state p {
  margin: 0 0 0.5rem;
  font-size: 1.1rem;
  font-weight: 600;
}

.empty-state small {
  font-size: 0.9rem;
  color: var(--accent-gray);
}

.history-list {
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

.results-header {
  background: var(--bg-primary);
  padding: 1rem 1.5rem;
  border-radius: 14px 14px 0 0;
  border-bottom: 2px solid var(--border-color);
  box-shadow: 0 2px 8px rgba(0, 61, 107, 0.06);
}

.result-count {
  color: var(--accent-gray);
  font-size: 0.9rem;
  font-weight: 600;
}

.result-count strong {
  color: var(--primary-dark);
  font-size: 1.1rem;
}

.history-list {
  display: flex;
  flex-direction: column;
  background: var(--bg-primary);
  border-radius: 14px;
  overflow: hidden;
  box-shadow: 0 4px 16px rgba(0, 61, 107, 0.08);
}

.history-item {
  display: flex;
  gap: 1.5rem;
  padding: 1.5rem;
  border-bottom: 1px solid var(--border-color);
  transition: all 0.3s ease;
  position: relative;
}

.history-item:last-child {
  border-bottom: none;
}

.history-item:hover {
  background: linear-gradient(90deg, rgba(30, 144, 255, 0.02) 0%, rgba(80, 200, 120, 0.02) 100%);
}

.item-timeline {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.5rem;
}

.timeline-dot {
  display: inline-block;
  width: 14px;
  height: 14px;
  background: var(--primary-light);
  border: 3px solid white;
  border-radius: 50%;
  box-shadow: 0 0 0 2px var(--primary-light);
  flex-shrink: 0;
  margin-top: 0.25rem;
}

.history-item.checkout .timeline-dot {
  background: var(--accent-green);
  box-shadow: 0 0 0 2px var(--accent-green);
}

.history-item.checkin .timeline-dot {
  background: var(--color-warning);
  box-shadow: 0 0 0 2px var(--color-warning);
}

.item-content {
  flex: 1;
  min-width: 0;
}

.item-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 1rem;
  margin-bottom: 0.75rem;
}

.item-name {
  color: var(--primary-dark);
  margin: 0;
  font-size: 1.1rem;
  font-weight: 700;
  word-break: break-word;
}

.action-badge {
  display: inline-block;
  padding: 0.5rem 1rem;
  border-radius: 8px;
  font-size: 0.8rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  white-space: nowrap;
  flex-shrink: 0;
}

.action-badge.checkout {
  background: linear-gradient(135deg, var(--accent-green) 0%, #45a049 100%);
  color: white;
  box-shadow: 0 2px 8px rgba(80, 200, 120, 0.3);
}

.action-badge.checkin {
  background: linear-gradient(135deg, #ffc107 0%, #ffb300 100%);
  color: white;
  box-shadow: 0 2px 8px rgba(255, 193, 7, 0.3);
}

.item-details {
  display: flex;
  gap: 1.5rem;
  flex-wrap: wrap;
}

.detail {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.9rem;
  color: var(--accent-gray);
  font-weight: 500;
}

.detail svg {
  opacity: 0.6;
  color: var(--primary-light);
  flex-shrink: 0;
}

@media (max-width: 600px) {
  .history-item {
    gap: 1rem;
    padding: 1rem;
  }

  .item-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 0.5rem;
  }

  .item-name {
    font-size: 1rem;
  }

  .action-badge {
    font-size: 0.75rem;
    padding: 0.4rem 0.8rem;
  }

  .item-details {
    gap: 1rem;
    font-size: 0.85rem;
  }

  .result-count {
    font-size: 0.85rem;
  }
}

@media (max-width: 400px) {
  .history-item {
    gap: 0.75rem;
    padding: 0.75rem;
  }

  .item-name {
    font-size: 0.95rem;
  }

  .item-details {
    flex-direction: column;
    gap: 0.5rem;
  }
}
</style>
