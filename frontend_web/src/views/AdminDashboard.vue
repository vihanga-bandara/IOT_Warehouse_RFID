<template>
  <div class="dashboard-page">
    <div class="dashboard-container">
      <div class="page-header">
        <div>
          <h1>Admin Dashboard</h1>
          <p class="page-subtitle">Warehouse Inventory Overview</p>
        </div>
        <button @click="handleLogout" class="logout-btn" title="Logout">
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"/>
            <polyline points="16 17 21 12 16 7"/>
            <line x1="21" y1="12" x2="9" y2="12"/>
          </svg>
          Logout
        </button>
      </div>

      <nav class="admin-nav">
        <router-link to="/dashboard" class="nav-link active">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/><polyline points="9 22 9 12 15 12 15 22"/>
          </svg>
          Dashboard
        </router-link>
        <router-link to="/admin/items" class="nav-link">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <rect x="3" y="4" width="18" height="16" rx="2" ry="2"/><path d="M3 10h18"/>
          </svg>
          Items
        </router-link>
        <router-link to="/admin/transactions" class="nav-link">
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

      <div v-if="loading" class="loading-state">
        <div class="spinner"></div>
        <p>Loading inventory...</p>
      </div>

      <div v-else>
        <div class="summary-grid">
          <div class="summary-card surface-card surface-card--compact total">
            <div class="summary-icon">
              <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M12 2v20M2 12h20"/>
              </svg>
            </div>
            <div class="summary-content">
              <div class="summary-label">Total Items</div>
              <div class="summary-number">{{ totalItems }}</div>
            </div>
          </div>

          <div class="summary-card surface-card surface-card--compact borrowed">
            <div class="summary-icon">
              <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M13 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V9z"/><polyline points="13 2 13 9 20 9"/>
              </svg>
            </div>
            <div class="summary-content">
              <div class="summary-label">Borrowed</div>
              <div class="summary-number">{{ borrowedItems.length }}</div>
            </div>
          </div>

          <div class="summary-card surface-card surface-card--compact available">
            <div class="summary-icon">
              <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M21.5 2v6h-6M2.5 22v-6h6M2 11.5a10 10 0 0 1 18.8-4.3M22 12.5a10 10 0 0 1-18.8 4.2"/>
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

          <div v-if="borrowedItems.length === 0" class="empty-state surface-card surface-card--padded">
            <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
              <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/>
            </svg>
            <p>No items currently borrowed</p>
          </div>

          <div v-else class="items-grid">
            <div v-for="item in borrowedItems" :key="item.id" class="item-card surface-card surface-card--padded">
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
  </div>
</template>

<script>
import { ref, onMounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/authStore'
import api from '../services/api'

export default {
  name: 'AdminDashboard',
  setup() {
    const router = useRouter()
    const authStore = useAuthStore()
    const items = ref([])
    const loading = ref(true)
    const transactions = ref([])

    const isBorrowed = (item) => {
      const status = (item?.status || '').toString().toLowerCase()
      if (!status) return false
      if (status.includes('borrow')) return true
      if (status.includes('checkout') || status.includes('checkedout')) return true
      if (status === 'unavailable' || status === 'out') return true
      return false
    }

    const latestActionByItem = computed(() => {
      const map = new Map()
      transactions.value.forEach(tx => {
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
      return isBorrowed(item)
    }

    const borrowedItems = computed(() =>
      items.value.filter(item => derivedBorrowState(item))
    )

    const totalItems = computed(() => items.value.length)

    const availableItems = computed(() => {
      const availableCount = items.value.filter(item => !derivedBorrowState(item)).length
      const fallback = Math.max(totalItems.value - borrowedItems.value.length, 0)
      return availableCount || fallback
    })

    const fetchItems = async () => {
      try {
        const response = await api.get('/items')
        items.value = response.data
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
          idx
        }))
      } catch (err) {
        console.error('Failed to fetch transactions:', err)
      }
    }

    const handleLogout = () => {
      authStore.logout()
      router.push('/login')
    }

    const formatDate = (timestamp) => {
      if (!timestamp) return 'Unknown'
      return new Date(timestamp).toLocaleDateString()
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
      handleLogout
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

.logout-btn {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.75rem 1.5rem;
  background: rgba(220, 53, 69, 0.1);
  color: #dc3545;
  border: 1px solid rgba(220, 53, 69, 0.3);
  border-radius: 8px;
  font-size: 0.95rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
}

.logout-btn:hover {
  background: #dc3545;
  color: white;
  border-color: #dc3545;
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(220, 53, 69, 0.3);
}

.logout-btn svg {
  transition: transform 0.3s ease;
}

.logout-btn:hover svg {
  transform: translateX(2px);
}

@media (max-width: 600px) {
  .page-header h1 {
    font-size: 1.8rem;
  }
  
  .page-subtitle {
    font-size: 0.95rem;
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

.summary-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
  gap: 1.5rem;
  margin-bottom: 2rem;
}

.summary-card {
  display: flex;
  align-items: center;
  gap: 1rem;
  border-left: 4px solid var(--border-color);
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
  width: 60px;
  height: 60px;
  border-radius: 12px;
  color: white;
  flex-shrink: 0;
}

.summary-card.total .summary-icon {
  background: linear-gradient(135deg, var(--primary-dark) 0%, var(--primary-light) 100%);
}

.summary-card.borrowed .summary-icon {
  background: linear-gradient(135deg, #ff6b6b 0%, #ff8787 100%);
}

.summary-card.available .summary-icon {
  background: linear-gradient(135deg, var(--accent-green) 0%, #45a049 100%);
}

.summary-content {
  flex: 1;
}

.summary-label {
  color: var(--accent-gray);
  font-size: 0.85rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  margin-bottom: 0.4rem;
}

.summary-number {
  color: var(--primary-dark);
  font-size: 2rem;
  font-weight: 800;
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
  transition: transform var(--transition-normal), box-shadow var(--transition-normal), border-color var(--transition-fast);
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

.item-card h4 {
  margin: 0 0 1rem;
  color: var(--dark-text);
}

.item-card p {
  margin: 0.5rem 0;
  color: var(--text-secondary);
  font-size: 0.9rem;
}
</style>
