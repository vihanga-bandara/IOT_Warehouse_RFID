<template>
  <div class="transactions-page">
    <div class="transactions-container">
      <div class="page-header">
        <div>
          <h1>Transaction History</h1>
          <p class="page-subtitle">View all warehouse transactions</p>
        </div>
        <router-link to="/dashboard" class="back-button">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M19 12H5M12 19l-7-7 7-7"/>
          </svg>
          Back to Dashboard
        </router-link>
      </div>

      <nav class="admin-nav">
        <router-link to="/dashboard" class="nav-link">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/><polyline points="9 22 9 12 15 12 15 22"/>
          </svg>
          Dashboard
        </router-link>
        <router-link to="/admin/transactions" class="nav-link active">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M3 12a9 9 0 1 0 18 0 9 9 0 0 0-18 0z"/><path d="M12 6v6l4 2"/>
          </svg>
          Transactions
        </router-link>
        <router-link to="/admin/users" class="nav-link">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/>
          </svg>
          Users
        </router-link>
      </nav>

      <div class="filters-section">
        <div class="filter-group">
          <label for="filter-action">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <circle cx="12" cy="12" r="1"/><circle cx="19" cy="12" r="1"/><circle cx="5" cy="12" r="1"/>
            </svg>
            Filter by Action
          </label>
          <input
            id="filter-action"
            v-model="filterAction"
            type="text"
            placeholder="Checkout or Checkin..."
            class="filter-input"
          />
        </div>
        <div class="filter-group">
          <label for="filter-user">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/>
            </svg>
            Filter by User
          </label>
          <input
            id="filter-user"
            v-model="filterUser"
            type="text"
            placeholder="Search user..."
            class="filter-input"
          />
        </div>
      </div>

      <div v-if="loading" class="loading-state">
        <div class="spinner"></div>
        <p>Loading transactions...</p>
      </div>

      <div v-else-if="filteredTransactions.length === 0" class="empty-state">
        <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
          <path d="M13 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V9z"/><polyline points="13 2 13 9 20 9"/>
        </svg>
        <p>No transactions found</p>
      </div>

      <div v-else class="transactions-table-section">
        <div class="results-info">
          Showing {{ filteredTransactions.length }} transaction<span v-if="filteredTransactions.length !== 1">s</span>
        </div>
        <div class="table-wrapper">
          <table class="transactions-table">
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
              <tr v-for="tx in filteredTransactions" :key="tx.id" class="transaction-row">
                <td class="item-cell">{{ tx.itemName }}</td>
                <td class="user-cell">{{ tx.userName }}</td>
                <td class="action-cell">
                  <span :class="['action-badge', tx.action.toLowerCase()]">
                    {{ tx.action }}
                  </span>
                </td>
                <td class="scanner-cell">{{ tx.deviceName }}</td>
                <td class="date-cell">{{ formatDate(tx.timestamp) }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import api from '../services/api'

export default {
  name: 'AdminTransactionHistory',
  setup() {
    const route = useRoute()
    const transactions = ref([])
    const loading = ref(true)
    const filterAction = ref('')
    const filterUser = ref('')
    const userIdFilter = ref(route.query.userId ? parseInt(route.query.userId) : null)

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
        const payload = response?.data?.data ?? response?.data ?? []
        const list = Array.isArray(payload) ? payload : []

        transactions.value = list.map((tx, idx) => ({
          id: tx.transactionId ?? tx.id ?? idx,
          itemName: tx.item?.itemName ?? 'Unknown item',
          userName: tx.user ? `${tx.user.name ?? ''} ${tx.user.lastname ?? ''}`.trim() || 'Unknown user' : 'Unknown user',
          action: tx.action ?? '',
          deviceName: tx.scanner?.name ?? tx.scanner?.deviceId ?? 'N/A',
          timestamp: tx.timestamp ?? tx.createdAt ?? new Date().toISOString()
        }))
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
.transactions-page {
  width: 100%;
  min-height: 100vh;
  background: var(--gradient-page-bg);
  padding: 2rem 1rem;
}

.transactions-container {
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
  
  .back-button {
    padding: 0.6rem 1rem;
    font-size: 0.85rem;
  }
}

.filters-section {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 1rem;
  margin-bottom: 2rem;
}

.filter-group {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.filter-group label {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  color: var(--dark-text);
  font-weight: 600;
  font-size: 0.9rem;
}

.filter-group label svg {
  opacity: 0.7;
  color: var(--primary-light);
}

.filter-input {
  padding: 0.9rem 1rem;
  border: 2px solid var(--border-color);
  border-radius: 10px;
  font-size: 1rem;
  transition: all 0.3s ease;
  background: var(--bg-primary);
  color: var(--dark-text);
  box-sizing: border-box;
}

.filter-input:focus {
  outline: none;
  border-color: var(--primary-light);
  background: #fafbfc;
  box-shadow: 0 0 0 4px rgba(30, 144, 255, 0.1);
}

.filter-input::placeholder {
  color: var(--text-tertiary);
}

@media (max-width: 600px) {
  .filter-input {
    padding: 0.85rem 0.9rem;
    font-size: 16px;
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
  margin: 0;
  font-size: 1.1rem;
  font-weight: 600;
}

.transactions-table-section {
  background: var(--bg-secondary);
  border-radius: 16px;
  overflow: hidden;
  box-shadow: 0 4px 16px rgba(0, 61, 107, 0.08);
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

.results-info {
  padding: 1rem 1.5rem;
  background: linear-gradient(135deg, rgba(0, 61, 107, 0.02) 0%, rgba(80, 200, 120, 0.02) 100%);
  color: var(--accent-gray);
  font-size: 0.9rem;
  font-weight: 600;
  border-bottom: 1px solid var(--border-color);
}

.table-wrapper {
  overflow-x: auto;
  -webkit-overflow-scrolling: touch;
}

.transactions-table {
  width: 100%;
  border-collapse: collapse;
}

.transactions-table thead {
  background: linear-gradient(135deg, rgba(0, 61, 107, 0.05) 0%, rgba(30, 144, 255, 0.05) 100%);
  border-bottom: 2px solid var(--primary-light);
  position: sticky;
  top: 0;
}

.transactions-table th {
  padding: 1rem 1.5rem;
  text-align: left;
  font-weight: 700;
  color: var(--primary-dark);
  font-size: 0.9rem;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.transactions-table td {
  padding: 1rem 1.5rem;
  border-bottom: 1px solid var(--border-color);
  color: var(--dark-text);
  font-size: 0.95rem;
}

.transaction-row {
  transition: all 0.3s ease;
}

.transaction-row:hover {
  background: linear-gradient(90deg, rgba(30, 144, 255, 0.02) 0%, rgba(80, 200, 120, 0.02) 100%);
}

.item-cell {
  font-weight: 600;
  color: var(--primary-dark);
}

.action-cell {
  text-align: center;
}

.action-badge {
  display: inline-block;
  padding: 0.5rem 1rem;
  border-radius: 8px;
  font-size: 0.8rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  transition: all 0.3s ease;
}

.action-badge.checkout {
  background: linear-gradient(135deg, var(--accent-green) 0%, #45a049 100%);
  color: white;
  box-shadow: 0 2px 8px rgba(80, 200, 120, 0.3);
}

.action-badge.checkin {
  background: linear-gradient(135deg, #ffb300 0%, #ff9800 100%);
  color: white;
  box-shadow: 0 2px 8px rgba(255, 193, 7, 0.3);
}

.date-cell {
  font-size: 0.9rem;
  color: var(--accent-gray);
}

@media (max-width: 800px) {
  .transactions-table th,
  .transactions-table td {
    padding: 0.75rem 1rem;
    font-size: 0.85rem;
  }
  
  .action-badge {
    padding: 0.35rem 0.75rem;
    font-size: 0.75rem;
  }
}

@media (max-width: 600px) {
  .table-wrapper {
    overflow-x: auto;
    border-radius: 0 0 16px 16px;
  }
  
  .transactions-table th,
  .transactions-table td {
    padding: 0.6rem 0.8rem;
    font-size: 0.75rem;
  }
  
  .transactions-table th {
    font-size: 0.8rem;
  }
}
</style>
