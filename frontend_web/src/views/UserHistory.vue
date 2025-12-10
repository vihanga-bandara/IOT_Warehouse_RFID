<template>
  <div class="history-container">
    <div class="history-header">
      <h2>My Borrowing History</h2>
      <router-link to="/kiosk" class="back-btn">← Back to Kiosk</router-link>
    </div>

    <div v-if="loading" class="loading">Loading history...</div>

    <div v-else-if="transactions.length === 0" class="empty-state">
      <p>No transactions yet</p>
    </div>

    <div v-else class="history-list">
      <div v-for="tx in transactions" :key="tx.id" class="history-item">
        <div class="item-header">
          <span class="item-name">{{ tx.itemName }}</span>
          <span :class="['action-badge', tx.action.toLowerCase()]">
            {{ tx.action }}
          </span>
        </div>
        <div class="item-details">
          <span class="detail">{{ formatDate(tx.timestamp) }}</span>
          <span class="detail">Scanner: {{ tx.deviceName }}</span>
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
.history-container {
  max-width: 800px;
  margin: 0 auto;
}

.history-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
}

.history-header h2 {
  margin: 0;
  color: #2c3e50;
}

.back-btn {
  padding: 0.5rem 1rem;
  background: #3498db;
  color: white;
  text-decoration: none;
  border-radius: 4px;
  transition: background 0.3s;
}

.back-btn:hover {
  background: #2980b9;
}

.loading {
  text-align: center;
  color: #7f8c8d;
  padding: 2rem;
}

.empty-state {
  text-align: center;
  color: #95a5a6;
  padding: 3rem;
  background: #f8f9fa;
  border-radius: 8px;
}

.history-list {
  display: grid;
  gap: 1rem;
}

.history-item {
  background: white;
  border: 1px solid #e0e0e0;
  border-radius: 6px;
  padding: 1rem;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.item-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 0.75rem;
}

.item-name {
  font-weight: 600;
  color: #2c3e50;
  font-size: 1.05rem;
}

.action-badge {
  padding: 0.25rem 0.75rem;
  border-radius: 20px;
  font-size: 0.8rem;
  font-weight: 600;
}

.action-badge.checkout {
  background: #d4edda;
  color: #155724;
}

.action-badge.checkin {
  background: #fff3cd;
  color: #856404;
}

.item-details {
  display: flex;
  gap: 2rem;
  font-size: 0.9rem;
  color: #7f8c8d;
}

.detail {
  display: flex;
  align-items: center;
}
</style>
