<template>
  <div class="transactions-container">
    <div class="transactions-header">
      <h2>Transaction History</h2>
      <router-link to="/dashboard" class="back-link">← Back to Dashboard</router-link>
    </div>

    <div class="filters">
      <input
        v-model="filterAction"
        type="text"
        placeholder="Filter by action (Checkout/Checkin)..."
        class="filter-input"
      />
      <input
        v-model="filterUser"
        type="text"
        placeholder="Filter by user..."
        class="filter-input"
      />
    </div>

    <div v-if="loading" class="loading">Loading transactions...</div>

    <div v-else-if="filteredTransactions.length === 0" class="empty-state">
      <p>No transactions found</p>
    </div>

    <div v-else class="transactions-table">
      <table>
        <thead>
          <tr>
            <th>Item</th>
            <th>User</th>
            <th>Action</th>
            <th>Scanner</th>
            <th>Date/Time</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="tx in filteredTransactions" :key="tx.id">
            <td>{{ tx.itemName }}</td>
            <td>{{ tx.userName }}</td>
            <td>
              <span :class="['action-badge', tx.action.toLowerCase()]">
                {{ tx.action }}
              </span>
            </td>
            <td>{{ tx.deviceName }}</td>
            <td>{{ formatDate(tx.timestamp) }}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script>
import { ref, computed, onMounted } from 'vue'
import api from '../services/api'

export default {
  name: 'AdminTransactionHistory',
  setup() {
    const transactions = ref([])
    const loading = ref(true)
    const filterAction = ref('')
    const filterUser = ref('')

    const filteredTransactions = computed(() => {
      return transactions.value.filter(tx => {
        const actionMatch = !filterAction.value ||
          tx.action.toLowerCase().includes(filterAction.value.toLowerCase())
        const userMatch = !filterUser.value ||
          tx.userName.toLowerCase().includes(filterUser.value.toLowerCase())
        return actionMatch && userMatch
      })
    })

    const fetchTransactions = async () => {
      try {
        const response = await api.get('/transaction/all')
        transactions.value = response.data
      } catch (err) {
        console.error('Failed to fetch transactions:', err)
      } finally {
        loading.value = false
      }
    }

    const formatDate = (timestamp) => {
      return new Date(timestamp).toLocaleString()
    }

    onMounted(() => {
      fetchTransactions()
    })

    return {
      transactions,
      loading,
      filterAction,
      filterUser,
      filteredTransactions,
      formatDate
    }
  }
}
</script>

<style scoped>
.transactions-container {
  max-width: 1200px;
}

.transactions-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
}

.transactions-header h2 {
  margin: 0;
  color: #2c3e50;
}

.back-link {
  color: #3498db;
  text-decoration: none;
  transition: color 0.3s;
}

.back-link:hover {
  color: #2980b9;
}

.filters {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 1rem;
  margin-bottom: 1.5rem;
}

.filter-input {
  padding: 0.75rem;
  border: 1px solid #bdc3c7;
  border-radius: 4px;
  font-size: 0.9rem;
}

.filter-input:focus {
  outline: none;
  border-color: #3498db;
  box-shadow: 0 0 0 3px rgba(52, 152, 219, 0.1);
}

.loading {
  text-align: center;
  color: #7f8c8d;
  padding: 3rem;
}

.empty-state {
  text-align: center;
  color: #95a5a6;
  padding: 3rem;
  background: #f8f9fa;
  border-radius: 8px;
}

.transactions-table {
  overflow-x: auto;
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

table {
  width: 100%;
  border-collapse: collapse;
}

th {
  background: #f8f9fa;
  padding: 1rem;
  text-align: left;
  font-weight: 600;
  color: #2c3e50;
  border-bottom: 2px solid #ecf0f1;
}

td {
  padding: 1rem;
  border-bottom: 1px solid #ecf0f1;
  color: #34495e;
}

tr:hover {
  background: #f8f9fa;
}

.action-badge {
  display: inline-block;
  padding: 0.35rem 0.75rem;
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
</style>
