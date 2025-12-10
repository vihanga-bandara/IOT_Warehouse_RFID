<template>
  <div class="items-page">
    <div class="items-container">
      <div class="page-header">
        <div>
          <h1>Items</h1>
          <p class="page-subtitle">Browse all warehouse items and their status</p>
        </div>
        <router-link to="/dashboard" class="back-button">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M19 12H5M12 19l-7-7 7-7" />
          </svg>
          Back to Dashboard
        </router-link>
      </div>

      <nav class="admin-nav">
        <router-link to="/dashboard" class="nav-link">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z" />
            <polyline points="9 22 9 12 15 12 15 22" />
          </svg>
          Dashboard
        </router-link>
        <router-link to="/admin/items" class="nav-link active">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <rect x="3" y="4" width="18" height="16" rx="2" ry="2" />
            <path d="M3 10h18" />
          </svg>
          Items
        </router-link>
        <router-link to="/admin/transactions" class="nav-link">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M3 12a9 9 0 1 0 18 0 9 9 0 0 0-18 0z" />
            <path d="M12 6v6l4 2" />
          </svg>
          Transactions
        </router-link>
        <router-link to="/admin/users" class="nav-link">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2" />
            <circle cx="9" cy="7" r="4" />
            <path d="M23 21v-2a4 4 0 0 0-3-3.87" />
            <path d="M16 3.13a4 4 0 0 1 0 7.75" />
          </svg>
          Users
        </router-link>
      </nav>

      <div class="filters-bar surface-card surface-card--padded">
        <div class="filter-group">
          <label for="search-term">Search</label>
          <input
            id="search-term"
            v-model="searchTerm"
            type="text"
            placeholder="Search by name, RFID, or holder"
            class="filter-input"
          />
        </div>
        <div class="filter-group">
          <label for="status-filter">Status</label>
          <select id="status-filter" v-model="statusFilter" class="filter-input">
            <option value="all">All</option>
            <option value="available">Available</option>
            <option value="borrowed">Borrowed</option>
          </select>
        </div>
        <button class="clear-filter-btn button-ghost" @click="clearFilters" v-if="searchTerm || statusFilter !== 'all'">
          Clear
        </button>
      </div>

      <div v-if="loading" class="loading-state">
        <div class="spinner"></div>
        <p>Loading items...</p>
      </div>

      <div v-else-if="filteredItems.length === 0" class="empty-state">
        <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
          <rect x="3" y="4" width="18" height="16" rx="2" ry="2" />
          <path d="M3 10h18" />
        </svg>
        <p>No items match your filters</p>
      </div>

      <div v-else class="items-table-section">
        <div class="results-info">
          Showing {{ filteredItems.length }} item<span v-if="filteredItems.length !== 1">s</span>
        </div>
        <div class="table-wrapper">
          <table class="items-table">
            <thead>
              <tr>
                <th>Item</th>
                <th>Status</th>
                <th>Holder</th>
                <th>RFID</th>
                <th>Last Updated</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="item in filteredItems" :key="item.id" class="item-row">
                <td class="item-name-cell">
                  <div class="item-title">{{ item.itemName }}</div>
                  <div class="item-subtitle" v-if="item.description">{{ item.description }}</div>
                </td>
                <td class="status-cell">
                  <span :class="['status-badge', statusClass(item)]">{{ statusLabel(item) }}</span>
                </td>
                <td class="holder-cell">
                  <div v-if="derivedBorrowState(item)" class="holder-info">
                    <div class="holder-name">{{ holderName(item) }}</div>
                    <div class="holder-email" v-if="item.currentHolderEmail">{{ item.currentHolderEmail }}</div>
                  </div>
                  <div v-else class="holder-info muted">Not borrowed</div>
                </td>
                <td class="rfid-cell mono">{{ item.rfidUid || '—' }}</td>
                <td class="date-cell">{{ formatDate(item.lastUpdated) }}</td>
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
import api from '../services/api'

export default {
  name: 'AdminItems',
  setup() {
    const items = ref([])
    const loading = ref(true)
    const transactions = ref([])
    const searchTerm = ref('')
    const statusFilter = ref('all')

    const latestActionByItem = computed(() => {
      const map = new Map()
      transactions.value.forEach(tx => {
        const ts = new Date(tx.timestamp || tx.createdAt || Date.now()).getTime()
        const existing = map.get(tx.itemId)
        if (!existing || ts > existing.ts) {
          map.set(tx.itemId, {
            action: tx.action || '',
            userName: tx.userName || tx.user?.name || '',
            timestamp: tx.timestamp || tx.createdAt,
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

    const statusLabel = (item) => {
      const latest = latestActionByItem.value.get(item.id || item.itemId)
      if (latest) {
        const a = (latest.action || '').toLowerCase()
        if (a.includes('checkin') || a.includes('return')) return 'Available'
        if (a.includes('checkout') || a.includes('borrow')) return 'Borrowed'
      }
      const normalized = (item?.status || '').toString().toLowerCase()
      if (normalized.includes('borrow')) return 'Borrowed'
      if (normalized.includes('available')) return 'Available'
      return 'Unknown'
    }

    const statusClass = (item) => {
      const label = statusLabel(item)
      if (label === 'Borrowed') return 'borrowed'
      if (label === 'Available') return 'available'
      return 'neutral'
    }

    const filteredItems = computed(() => {
      const term = searchTerm.value.trim().toLowerCase()
      return items.value.filter(item => {
        const matchesStatus =
          statusFilter.value === 'all' ||
          (statusFilter.value === 'borrowed' && derivedBorrowState(item)) ||
          (statusFilter.value === 'available' && !derivedBorrowState(item))

        const matchesSearch = !term || [
          item.itemName,
          item.description,
          item.rfidUid,
          item.currentHolderName,
          item.currentHolderEmail,
          latestActionByItem.value.get(item.id || item.itemId)?.userName
        ]
          .filter(Boolean)
          .some(field => field.toString().toLowerCase().includes(term))

        return matchesStatus && matchesSearch
      })
    })

    const fetchItems = async () => {
      try {
        const response = await api.get('/items')
        items.value = response.data || []
      } catch (err) {
        console.error('Failed to fetch items:', err)
      }
    }

    const fetchTransactions = async () => {
      try {
        const response = await api.get('/transaction/all')
        const payload = response?.data?.data ?? response?.data ?? []
        const list = Array.isArray(payload) ? payload : []
        transactions.value = list.map((tx, idx) => ({
          itemId: tx.item?.id ?? tx.itemId,
          action: tx.action || '',
          timestamp: tx.timestamp || tx.createdAt || new Date().toISOString(),
          userName: tx.user ? `${tx.user.name ?? ''} ${tx.user.lastname ?? ''}`.trim() || tx.user.email || 'Unknown user' : tx.userName,
          idx
        }))
      } catch (err) {
        console.error('Failed to fetch transactions:', err)
      }
    }

    const clearFilters = () => {
      searchTerm.value = ''
      statusFilter.value = 'all'
    }

    const formatDate = (timestamp) => {
      if (!timestamp) return 'Unknown'
      return new Date(timestamp).toLocaleString()
    }

    const holderName = (item) => {
      const latest = latestActionByItem.value.get(item.id || item.itemId)
      if (latest && derivedBorrowState(item)) return latest.userName || 'Unknown user'
      if (derivedBorrowState(item)) return item.currentHolderName || 'Unknown user'
      return 'Not borrowed'
    }

    onMounted(async () => {
      loading.value = true
      await Promise.all([fetchItems(), fetchTransactions()])
      loading.value = false
    })

    return {
      items,
      loading,
      searchTerm,
      statusFilter,
      filteredItems,
      statusLabel,
      statusClass,
      formatDate,
      clearFilters,
      derivedBorrowState,
      holderName
    }
  }
}
</script>

<style scoped>
.items-page {
  width: 100%;
  min-height: 100vh;
  background: var(--gradient-page-bg);
  padding: 2rem 1rem;
}

.items-container {
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
  font-size: 1.05rem;
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

.loading-state,
.empty-state {
  text-align: center;
  padding: 3rem;
  background: white;
  border-radius: 16px;
  border: 1px solid var(--border-subtle);
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.05);
}

.loading-state .spinner {
  width: 48px;
  height: 48px;
  border: 4px solid rgba(59, 130, 246, 0.2);
  border-top-color: var(--primary-dark);
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin: 0 auto 1rem;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

.items-table-section {
  background: var(--bg-secondary);
  border-radius: 16px;
  overflow: hidden;
  box-shadow: 0 4px 16px rgba(0, 61, 107, 0.08);
  animation: slideUp 0.4s ease-out;
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

.items-table {
  width: 100%;
  border-collapse: collapse;
}

.items-table thead {
  background: linear-gradient(135deg, rgba(0, 61, 107, 0.05) 0%, rgba(30, 144, 255, 0.05) 100%);
  border-bottom: 2px solid var(--primary-light);
  position: sticky;
  top: 0;
}

.items-table th {
  padding: 1rem 1.5rem;
  text-align: left;
  font-weight: 700;
  color: var(--primary-dark);
  font-size: 0.9rem;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.items-table td {
  padding: 1rem 1.5rem;
  border-bottom: 1px solid var(--border-color);
  color: var(--dark-text);
  font-size: 0.95rem;
}

.items-table tbody tr:last-child td {
  border-bottom: none;
}

.item-row {
  transition: all 0.3s ease;
}

.item-row:hover {
  background: linear-gradient(90deg, rgba(30, 144, 255, 0.02) 0%, rgba(80, 200, 120, 0.02) 100%);
}

.item-name-cell .item-title {
  font-weight: 700;
  color: var(--primary-dark);
  margin-bottom: 0.3rem;
}

.item-name-cell .item-subtitle {
  color: var(--accent-gray);
  font-size: 0.9rem;
}

.status-cell {
  width: 140px;
}

.status-badge {
  display: inline-flex;
  align-items: center;
  padding: 0.35rem 0.7rem;
  border-radius: 999px;
  font-weight: 700;
  font-size: 0.85rem;
  letter-spacing: 0.3px;
  text-transform: uppercase;
  border: 1px solid transparent;
}

.status-badge.available {
  background: rgba(16, 185, 129, 0.12);
  color: #0b9c72;
  border-color: rgba(16, 185, 129, 0.25);
}

.status-badge.borrowed {
  background: rgba(245, 158, 11, 0.16);
  color: #c37300;
  border-color: rgba(245, 158, 11, 0.3);
}

.status-badge.neutral {
  background: rgba(107, 114, 128, 0.16);
  color: #374151;
  border-color: rgba(107, 114, 128, 0.3);
}

.holder-cell .holder-info {
  display: flex;
  flex-direction: column;
  gap: 0.2rem;
}

.holder-cell .holder-name {
  font-weight: 700;
  color: var(--primary-dark);
}

.holder-cell .holder-email {
  color: var(--accent-gray);
  font-size: 0.9rem;
}

.holder-cell .muted {
  color: var(--accent-gray);
  font-style: italic;
}

.rfid-cell {
  font-weight: 700;
}

.date-cell {
  color: var(--accent-gray);
  font-size: 0.95rem;
}

.mono {
  font-family: 'SFMono-Regular', Consolas, 'Liberation Mono', Menlo, monospace;
}

@media (max-width: 900px) {
  .items-table thead {
    display: none;
  }

  .items-table tbody tr {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 0.75rem 1rem;
    padding: 1.25rem 1rem;
  }

  .items-table td {
    padding: 0;
  }

  .status-cell,
  .holder-cell,
  .rfid-cell,
  .date-cell {
    display: flex;
    align-items: center;
    gap: 0.5rem;
  }

  .status-cell::before { content: 'Status'; font-weight: 700; color: var(--accent-gray); width: 90px; }
  .holder-cell::before { content: 'Holder'; font-weight: 700; color: var(--accent-gray); width: 90px; }
  .rfid-cell::before { content: 'RFID'; font-weight: 700; color: var(--accent-gray); width: 90px; }
  .date-cell::before { content: 'Updated'; font-weight: 700; color: var(--accent-gray); width: 90px; }
}
</style>
