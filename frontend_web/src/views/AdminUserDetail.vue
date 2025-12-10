<template>
  <div class="user-detail-page">
    <div class="user-detail-container">
      <div class="page-header">
        <div>
          <button @click="goBack" class="back-btn button-ghost">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M19 12H5M12 19l-7-7 7-7"/>
            </svg>
            Back to Users
          </button>
          <h1 v-if="user">{{ user.name }} {{ user.lastname }}</h1>
          <p v-if="user" class="page-subtitle">{{ user.email }} • {{ user.role }}</p>
        </div>
      </div>

      <nav class="admin-nav">
        <router-link to="/dashboard" class="nav-link">
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
        <router-link to="/admin/users" class="nav-link active">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/>
          </svg>
          Users
        </router-link>
      </nav>

      <div v-if="loading" class="loading-state surface-card surface-card--padded">
        <div class="spinner"></div>
        <p>Loading transactions...</p>
      </div>

      <div v-else class="transactions-section surface-card surface-card--padded">
        <div class="section-header">
          <h2>Transaction History</h2>
          <span class="transaction-count">{{ transactions.length }} total transactions</span>
        </div>

        <div v-if="transactions.length === 0" class="empty-state surface-card surface-card--padded">
          <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
            <path d="M3 12a9 9 0 1 0 18 0 9 9 0 0 0-18 0z"/><path d="M12 6v6l4 2"/>
          </svg>
          <p>No transactions found for this user.</p>
        </div>

        <div v-else class="transactions-list">
          <div v-for="transaction in transactions" :key="transaction.id" class="transaction-card surface-card surface-card--padded">
            <div class="transaction-header">
              <div class="transaction-action" :class="actionClass(transaction.action)">
                <svg v-if="actionClass(transaction.action) === 'borrow'" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                  <path d="M13 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V9z"/><polyline points="13 2 13 9 20 9"/>
                </svg>
                <svg v-else width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                  <path d="M1 4v6h6M23 20v-6h-6"/>
                </svg>
                <span>{{ actionLabel(transaction.action) }}</span>
              </div>
              <div class="transaction-date">
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                  <circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/>
                </svg>
                {{ formatDate(transaction.timestamp) }}
              </div>
            </div>
            <div class="transaction-body">
              <div class="transaction-info">
                <div class="info-item">
                  <span class="info-label">Item:</span>
                  <span class="info-value">{{ transaction.itemName }}</span>
                </div>
                <div class="info-item">
                  <span class="info-label">Scanner:</span>
                  <span class="info-value">{{ transaction.deviceName }}</span>
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
import { useRoute, useRouter } from 'vue-router'
import api from '../services/api'

export default {
  name: 'AdminUserDetail',
  setup() {
    const route = useRoute()
    const router = useRouter()
    const user = ref(null)
    const transactions = ref([])
    const loading = ref(true)
    const userId = route.params.id

    const fetchUserDetails = async () => {
      try {
        // Fetch user info
        const usersResponse = await api.get('/auth/users')
        user.value = usersResponse.data.find(u => u.id === parseInt(userId))

        // Fetch user's transactions
        const transactionsResponse = await api.get(`/auth/users/${userId}/transactions`)
        transactions.value = transactionsResponse.data
      } catch (err) {
        console.error('Failed to fetch user details:', err)
      } finally {
        loading.value = false
      }
    }

    const formatDate = (dateString) => {
      const date = new Date(dateString)
      return date.toLocaleString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
      })
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

    const goBack = () => {
      router.push('/admin/users')
    }

    onMounted(() => {
      fetchUserDetails()
    })

    return {
      user,
      transactions,
      loading,
      formatDate,
      actionLabel,
      actionClass,
      goBack
    }
  }
}
</script>

<style scoped lang="scss">
.user-detail-page {
  min-height: 100vh;
  background: var(--gradient-page-bg);
  padding: 2rem 1rem;
}

.user-detail-container {
  max-width: 1200px;
  margin: 0 auto;
}

.page-header {
  margin-bottom: 2rem;

  .back-btn {
    display: inline-flex;
    align-items: center;
    gap: 0.5rem;
    padding: 0.5rem 1rem;
    background: rgba(255, 255, 255, 0.2);
    border: none;
    border-radius: 8px;
    color: white;
    font-size: 0.875rem;
    cursor: pointer;
    transition: all 0.2s;
    margin-bottom: 1rem;

    &:hover {
      background: rgba(255, 255, 255, 0.3);
      transform: translateX(-4px);
    }

    svg {
      flex-shrink: 0;
    }
  }

  h1 {
    color: white;
    font-size: 2rem;
    margin: 0;
    font-weight: 700;
  }

  .page-subtitle {
    color: rgba(255, 255, 255, 0.9);
    margin: 0.5rem 0 0;
    font-size: 1rem;
  }
}

.admin-nav {
  display: flex;
  gap: 0.5rem;
  margin-bottom: 2rem;
  border-bottom: 2px solid rgba(30, 144, 255, 0.1);
  padding-bottom: 1rem;

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

    &:hover {
      color: var(--primary-light);
    }

    &.active {
      color: var(--primary-light);
      border-bottom-color: var(--primary-light);
      background: rgba(30, 144, 255, 0.05);
    }
  }
}

.loading-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 4rem 2rem;
  background: white;
  border-radius: 16px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);

  .spinner {
    width: 48px;
    height: 48px;
    border: 4px solid rgba(102, 126, 234, 0.2);
    border-top-color: #667eea;
    border-radius: 50%;
    animation: spin 1s linear infinite;
  }

  p {
    margin-top: 1rem;
    color: #64748b;
  }
}

.transactions-section {
  background: white;
  border-radius: 16px;
  padding: 2rem;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);

  .section-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 2rem;
    padding-bottom: 1rem;
    border-bottom: 2px solid #f1f5f9;

    h2 {
      font-size: 1.5rem;
      color: #1e293b;
      margin: 0;
    }

    .transaction-count {
      color: #64748b;
      font-size: 0.875rem;
      font-weight: 500;
    }
  }
}

.empty-state {
  text-align: center;
  padding: 4rem 2rem;
  color: #94a3b8;

  svg {
    margin-bottom: 1rem;
    opacity: 0.5;
  }

  p {
    font-size: 1rem;
    margin: 0;
  }
}

.transactions-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.transaction-card {
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 12px;
  overflow: hidden;
  transition: all 0.2s;

  &:hover {
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
    border-color: #cbd5e1;
  }

  .transaction-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 1rem 1.5rem;
    background: white;
    border-bottom: 1px solid #e2e8f0;

    .transaction-action {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      font-weight: 600;
      text-transform: capitalize;

      &.borrow {
        color: #3b82f6;
      }

      &.return {
        color: #10b981;
      }

      svg {
        flex-shrink: 0;
      }
    }

    .transaction-date {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      color: #64748b;
      font-size: 0.875rem;

      svg {
        flex-shrink: 0;
        opacity: 0.6;
      }
    }
  }

  .transaction-body {
    padding: 1rem 1.5rem;

    .transaction-info {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 1rem;

      .info-item {
        display: flex;
        flex-direction: column;
        gap: 0.25rem;

        .info-label {
          font-size: 0.75rem;
          color: #64748b;
          text-transform: uppercase;
          letter-spacing: 0.05em;
          font-weight: 600;
        }

        .info-value {
          color: #1e293b;
          font-weight: 500;
        }
      }
    }
  }
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

@media (max-width: 768px) {
  .user-detail-page {
    padding: 1rem;
  }

  .page-header h1 {
    font-size: 1.5rem;
  }

  .admin-nav {
    flex-direction: column;
  }

  .transaction-card .transaction-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 0.5rem;
  }
}
</style>
