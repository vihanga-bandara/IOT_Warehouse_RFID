<template>
  <AdminShell
    title="Items"
    subtitle="Browse all warehouse items and their status"
    active-tab="items"
  >
    <div class="items-page-content">
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
        <button class="button-primary" @click="clearFilters" v-if="searchTerm || statusFilter !== 'all'">
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
  </AdminShell>
</template>

<script>
import { ref, computed, onMounted } from 'vue'
import api from '../services/api'
import AdminShell from '../components/AdminShell.vue'

export default {
  name: 'AdminItems',
  components: {
    AdminShell
  },
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

        const searchable = [
          item.itemName,
          item.description,
          item.rfidUid,
          item.currentHolderName,
          item.currentHolderEmail
        ]
          .filter(Boolean)
          .map(v => v.toString().toLowerCase())

        const matchesSearch = !term || searchable.some(v => v.includes(term))

        return matchesStatus && matchesSearch
      })
    })

    const holderName = (item) => {
      if (item.currentHolderName) return item.currentHolderName
      if (item.currentHolder && (item.currentHolder.name || item.currentHolder.lastname)) {
        return `${item.currentHolder.name ?? ''} ${item.currentHolder.lastname ?? ''}`.trim()
      }
      return 'Unknown holder'
    }

    const formatDate = (value) => {
      if (!value) return '—'
      return new Date(value).toLocaleString()
    }

    const clearFilters = () => {
      searchTerm.value = ''
      statusFilter.value = 'all'
    }

    const fetchItems = async () => {
      try {
        const response = await api.get('/items')
        items.value = Array.isArray(response.data) ? response.data : (response.data?.items ?? [])
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
          userName: tx.user ? `${tx.user.name ?? ''} ${tx.user.lastname ?? ''}`.trim() || 'Unknown user' : 'Unknown user',
          timestamp: tx.timestamp ?? tx.createdAt ?? new Date().toISOString()
        }))
      } catch (err) {
        console.error('Failed to fetch transactions for items view:', err)
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
      searchTerm,
      statusFilter,
      filteredItems,
      derivedBorrowState,
      statusLabel,
      statusClass,
      holderName,
      formatDate,
      clearFilters
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

.button-primary {
  display: inline-block;
  padding: 0.6rem 1.2rem;
  border-radius: 10px;
  font-weight: 600;
  font-size: 0.95rem;
  text-align: center;
  color: white;
  background: var(--primary);
  border: none;
  cursor: pointer;
  transition: background 0.3s, transform 0.3s;
}

.button-primary:hover {
  background: var(--primary-dark);
  transform: translateY(-1px);
}

.label-primary {
  display: inline-block;
  padding: 0.3rem 0.8rem;
  border-radius: 999px;
  font-weight: 700;
  font-size: 0.8rem;
  text-transform: uppercase;
  color: var(--badge-pill-text);
  background: var(--badge-available-gradient);
}

.label-secondary {
  display: inline-block;
  padding: 0.3rem 0.8rem;
  border-radius: 999px;
  font-weight: 700;
  font-size: 0.8rem;
  text-transform: uppercase;
  color: var(--badge-pill-text);
  background: var(--badge-borrowed-gradient);
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
  color: var(--accent-gray);
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

.status-badge.available {
  background: var(--badge-available-gradient);
  color: var(--badge-available-text);
}

.status-badge.borrowed {
  background: var(--badge-borrowed-gradient);
  color: white;
}

[data-theme="dark"] .status-badge,
[data-theme="dark"] .action-badge {
  text-shadow: 0 1px 0 rgba(0,0,0,0.28);
  box-shadow: 0 6px 18px rgba(0,0,0,0.35);
}

.status-badge.neutral {
  background: var(--badge-neutral-bg);
  color: var(--badge-neutral-color);
  box-shadow: none;
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
