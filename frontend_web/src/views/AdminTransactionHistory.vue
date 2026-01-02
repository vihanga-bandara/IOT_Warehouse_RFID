<template>
  <AdminShell
    title="Transaction History"
    subtitle="View all warehouse transactions"
    active-tab="transactions"
  >
    <div class="transactions-page-content">
      <div class="filters-bar surface-card surface-card--padded">
        <div class="filter-group">
          <label for="filter-action">Filter by action</label>
          <select
            id="filter-action"
            v-model="filterAction"
            class="filter-input"
          >
            <option value="">All Actions</option>
            <option value="borrowed">Borrowed</option>
            <option value="returned">Returned</option>
          </select>
        </div>
        <div class="filter-group">
          <label for="filter-user">Filter by user</label>
          <input
            id="filter-user"
            v-model="filterUser"
            type="text"
            placeholder="Search by name or email"
            class="filter-input"
          />
        </div>
        <button class="button-primary" @click="clearFilters">Clear</button>
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
          Showing {{ paginatedTransactions.length }} of {{ filteredTransactions.length }} transaction<span v-if="filteredTransactions.length !== 1">s</span>
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
              <tr v-for="tx in paginatedTransactions" :key="tx.id" class="transaction-row">
                <td class="item-cell">{{ tx.itemName }}</td>
                <td class="user-cell">{{ tx.userName }}</td>
                <td class="action-cell">
                  <span :class="['action-badge', actionClass(tx.action)]">
                    {{ actionLabel(tx.action) }}
                  </span>
                </td>
                <td class="scanner-cell">{{ tx.deviceName }}</td>
                <td class="date-cell">{{ formatDate(tx.timestamp) }}</td>
              </tr>
            </tbody>
          </table>
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
            <option :value="100">100 / page</option>
          </select>
        </div>
      </div>
    </div>
  </AdminShell>
</template>

<script>
import { ref, computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import api from '../services/api'
import AdminShell from '../components/AdminShell.vue'

export default {
  name: 'AdminTransactionHistory',
  components: {
    AdminShell
  },
  setup() {
    const route = useRoute()
    const transactions = ref([])
    const loading = ref(true)
    const filterAction = ref('')
    const filterUser = ref('')
    const userIdFilter = ref(route.query.userId ? parseInt(route.query.userId) : null)
    const currentPage = ref(1)
    const itemsPerPage = ref(25)

    const filteredTransactions = computed(() => {
      const actionTerm = filterAction.value.toLowerCase()
      const userTerm = filterUser.value.trim().toLowerCase()

      return transactions.value.filter(tx => {
        let actionMatch = true
        if (actionTerm) {
          const a = (tx.action || '').toLowerCase()
          if (actionTerm === 'borrowed') {
            actionMatch = a.includes('checkout') || a.includes('borrow')
          } else if (actionTerm === 'returned') {
            actionMatch = a.includes('checkin') || a.includes('return')
          } else {
            actionMatch = a.includes(actionTerm)
          }
        }
        
        const userMatch = !userTerm || tx.userName.toLowerCase().includes(userTerm)
        const userIdMatch = !userIdFilter.value || tx.userId === userIdFilter.value
        return actionMatch && userMatch && userIdMatch
      })
    })

    const totalPages = computed(() => Math.ceil(filteredTransactions.value.length / itemsPerPage.value) || 1)
    
    const paginatedTransactions = computed(() => {
      const start = (currentPage.value - 1) * itemsPerPage.value
      const end = start + itemsPerPage.value
      return filteredTransactions.value.slice(start, end)
    })

    const fetchTransactions = async () => {
      try {
        const response = await api.get('/transaction/all')
        const payload = response?.data?.data ?? response?.data ?? []
        const list = Array.isArray(payload) ? payload : []

        transactions.value = list.map((tx, idx) => ({
          id: tx.transactionId ?? tx.id ?? idx,
          itemName: tx.item?.itemName ?? 'Unknown item',
          userId: tx.user?.id ?? tx.userId ?? null,
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

    const clearFilters = () => {
      filterAction.value = ''
      filterUser.value = ''
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

    onMounted(() => {
      fetchTransactions()
    })

    return {
      transactions,
      loading,
      filterAction,
      filterUser,
      filteredTransactions,
      paginatedTransactions,
      currentPage,
      itemsPerPage,
      totalPages,
      formatDate,
      clearFilters,
      actionLabel,
      actionClass
    }
  }
}
</script>

<style>
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
  padding: 0.4rem 0.8rem;
  border-radius: 6px;
  font-size: 0.75rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  color: white;
  background: var(--badge-neutral-bg);
  border: none;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
  transition: transform 0.2s ease, box-shadow 0.18s ease;
}

.action-badge.borrow {
  background: var(--badge-borrowed-gradient);
  color: white;
}

.action-badge.return {
  background: var(--badge-available-gradient);
  color: var(--badge-available-text);
}

[data-theme="dark"] .status-badge,
[data-theme="dark"] .action-badge {
  text-shadow: 0 1px 0 rgba(0,0,0,0.28);
  box-shadow: 0 6px 18px rgba(0,0,0,0.35);
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

/* Pagination Controls */
.pagination-controls {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  padding: 1rem 1.5rem;
  background: linear-gradient(135deg, rgba(0, 61, 107, 0.02) 0%, rgba(80, 200, 120, 0.02) 100%);
  border-top: 1px solid var(--border-color);
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
