<template>
  <div class="dashboard-container">
    <div class="dashboard-header">
      <h2>Admin Dashboard</h2>
      <nav class="admin-nav">
        <router-link to="/dashboard" class="nav-link active">Dashboard</router-link>
        <router-link to="/admin/transactions" class="nav-link">Transactions</router-link>
        <router-link to="/admin/users" class="nav-link">Users</router-link>
      </nav>
    </div>

    <div v-if="loading" class="loading">Loading inventory...</div>

    <div v-else class="inventory-section">
      <h3>Currently Borrowed Items</h3>

      <div v-if="borrowedItems.length === 0" class="empty-state">
        <p>No items currently borrowed</p>
      </div>

      <div v-else class="items-grid">
        <div v-for="item in borrowedItems" :key="item.id" class="item-card">
          <div class="item-status borrowed">Borrowed</div>
          <h4>{{ item.itemName }}</h4>
          <p><strong>Holder:</strong> {{ item.currentHolderName }}</p>
          <p><strong>Email:</strong> {{ item.currentHolderEmail }}</p>
          <p><strong>RFID:</strong> {{ item.rfidUid }}</p>
          <div class="item-footer">
            <small>Last Updated: {{ formatDate(item.lastUpdated) }}</small>
          </div>
        </div>
      </div>
    </div>

    <div class="inventory-summary">
      <div class="summary-card">
        <div class="summary-number">{{ totalItems }}</div>
        <div class="summary-label">Total Items</div>
      </div>
      <div class="summary-card">
        <div class="summary-number">{{ borrowedItems.length }}</div>
        <div class="summary-label">Borrowed</div>
      </div>
      <div class="summary-card">
        <div class="summary-number">{{ availableItems }}</div>
        <div class="summary-label">Available</div>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, onMounted, computed } from 'vue'
import api from '../services/api'

export default {
  name: 'AdminDashboard',
  setup() {
    const items = ref([])
    const loading = ref(true)

    const borrowedItems = computed(() =>
      items.value.filter(item => item.status === 'Borrowed')
    )

    const totalItems = computed(() => items.value.length)

    const availableItems = computed(() =>
      items.value.filter(item => item.status === 'Available').length
    )

    const fetchItems = async () => {
      try {
        const response = await api.get('/items')
        items.value = response.data
      } catch (err) {
        console.error('Failed to fetch items:', err)
      } finally {
        loading.value = false
      }
    }

    const formatDate = (timestamp) => {
      return new Date(timestamp).toLocaleDateString()
    }

    onMounted(() => {
      fetchItems()
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
.dashboard-container {
  max-width: 1200px;
}

.dashboard-header {
  margin-bottom: 2rem;
}

.dashboard-header h2 {
  margin: 0 0 1rem;
  color: #2c3e50;
}

.admin-nav {
  display: flex;
  gap: 1rem;
  border-bottom: 2px solid #ecf0f1;
}

.nav-link {
  padding: 0.75rem 1.5rem;
  color: #7f8c8d;
  text-decoration: none;
  border-bottom: 3px solid transparent;
  transition: all 0.3s;
}

.nav-link:hover {
  color: #2c3e50;
}

.nav-link.active {
  color: #3498db;
  border-bottom-color: #3498db;
}

.loading {
  text-align: center;
  color: #7f8c8d;
  padding: 3rem;
}

.inventory-section {
  margin-bottom: 2rem;
}

.inventory-section h3 {
  color: #2c3e50;
  margin-bottom: 1.5rem;
}

.empty-state {
  text-align: center;
  color: #95a5a6;
  padding: 2rem;
  background: #f8f9fa;
  border-radius: 8px;
}

.items-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 1.5rem;
  margin-bottom: 2rem;
}

.item-card {
  background: white;
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  padding: 1.5rem;
  position: relative;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.item-status {
  position: absolute;
  top: 1rem;
  right: 1rem;
  padding: 0.35rem 0.75rem;
  border-radius: 20px;
  font-size: 0.75rem;
  font-weight: 600;
  background: #fff3cd;
  color: #856404;
}

.item-status.borrowed {
  background: #f8d7da;
  color: #721c24;
}

.item-card h4 {
  margin: 0 0 1rem;
  color: #2c3e50;
}

.item-card p {
  margin: 0.5rem 0;
  color: #34495e;
  font-size: 0.9rem;
}

.item-footer {
  margin-top: 1rem;
  padding-top: 1rem;
  border-top: 1px solid #ecf0f1;
  color: #95a5a6;
  font-size: 0.8rem;
}

.inventory-summary {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1rem;
}

.summary-card {
  background: white;
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  padding: 1.5rem;
  text-align: center;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.summary-number {
  font-size: 2.5rem;
  font-weight: 700;
  color: #3498db;
  margin-bottom: 0.5rem;
}

.summary-label {
  color: #7f8c8d;
  font-weight: 600;
  text-transform: uppercase;
  font-size: 0.85rem;
}
</style>
