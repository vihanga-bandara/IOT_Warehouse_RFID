<template>
  <div class="kiosk-page">
    <!-- Transaction Summary Modal -->
    <transition name="modal-fade">
      <div v-if="showSummary" class="summary-overlay" @click.self="closeSummary">
        <div class="summary-modal">
          <div class="summary-header">
            <div class="success-animation">
              <svg class="checkmark" viewBox="0 0 52 52">
                <circle class="checkmark-circle" cx="26" cy="26" r="25" fill="none"/>
                <path class="checkmark-check" fill="none" d="M14.1 27.2l7.1 7.2 16.7-16.8"/>
              </svg>
            </div>
            <h2>Transaction Complete!</h2>
            <p class="summary-timestamp">{{ summaryData.timestamp }}</p>
          </div>
          
          <div class="summary-content">
            <div v-if="summaryData.borrowed.length > 0" class="summary-section borrowed">
              <div class="summary-section-header">
                <div class="section-icon borrowed-icon">
                  <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <path d="M12 5v14M5 12h14"/>
                  </svg>
                </div>
                <span>Items Borrowed</span>
                <span class="section-count">{{ summaryData.borrowed.length }}</span>
              </div>
              <div class="summary-items">
                <transition-group name="item-fly-out" appear>
                  <div v-for="(item, index) in summaryData.borrowed" :key="item.id" 
                       class="summary-item" :style="{ animationDelay: `${index * 0.1}s` }">
                    <div class="item-icon-wrapper borrowed">
                      <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/>
                        <polyline points="14 2 14 8 20 8"/>
                      </svg>
                    </div>
                    <span class="item-name">{{ item.name }}</span>
                    <div class="item-status-badge out">OUT</div>
                  </div>
                </transition-group>
              </div>
            </div>
            
            <div v-if="summaryData.returned.length > 0" class="summary-section returned">
              <div class="summary-section-header">
                <div class="section-icon returned-icon">
                  <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <polyline points="20 6 9 17 4 12"/>
                  </svg>
                </div>
                <span>Items Returned</span>
                <span class="section-count">{{ summaryData.returned.length }}</span>
              </div>
              <div class="summary-items">
                <transition-group name="item-fly-in" appear>
                  <div v-for="(item, index) in summaryData.returned" :key="item.id" 
                       class="summary-item" :style="{ animationDelay: `${index * 0.1}s` }">
                    <div class="item-icon-wrapper returned">
                      <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/>
                        <polyline points="14 2 14 8 20 8"/>
                      </svg>
                    </div>
                    <span class="item-name">{{ item.name }}</span>
                    <div class="item-status-badge in">IN</div>
                  </div>
                </transition-group>
              </div>
            </div>
          </div>
          
          <div class="summary-footer">
            <button @click="closeSummary" class="btn-back">
              <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M19 12H5M12 19l-7-7 7-7"/>
              </svg>
              Back to Scanner
            </button>
          </div>
        </div>
      </div>
    </transition>

    <div class="kiosk-container" :class="{ 'blur-bg': showSummary }">
      <!-- Header Section -->
      <header class="kiosk-header">
        <div class="header-brand">
          <div class="brand-icon">
            <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <rect x="2" y="7" width="20" height="14" rx="2" ry="2"/>
              <path d="M16 3l-4 4-4-4"/>
              <line x1="6" y1="11" x2="6" y2="11.01"/>
              <line x1="10" y1="11" x2="10" y2="11.01"/>
            </svg>
          </div>
          <div class="brand-text">
            <h1>RFID Scanner</h1>
            <div class="scan-status">
              <span class="status-dot"></span>
              <span>Ready for scanning</span>
            </div>
          </div>
        </div>
        <div v-if="authStore.scannerDeviceId" class="scanner-info">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <rect x="5" y="2" width="14" height="20" rx="2" ry="2"/>
            <line x1="12" y1="18" x2="12" y2="18.01"/>
          </svg>
          {{ authStore.scannerName || authStore.scannerDeviceId }}
        </div>
        <div class="header-actions">
          <button v-if="authStore.user?.role === 'Admin'" @click="goToAdmin" class="action-btn admin">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <rect x="3" y="3" width="7" height="7"/><rect x="14" y="3" width="7" height="7"/>
              <rect x="14" y="14" width="7" height="7"/><rect x="3" y="14" width="7" height="7"/>
            </svg>
            <span>Admin</span>
          </button>
          <button @click="handleLogout" class="action-btn logout">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"/>
              <polyline points="16 17 21 12 16 7"/><line x1="21" y1="12" x2="9" y2="12"/>
            </svg>
            <span>Logout</span>
          </button>
        </div>
      </header>

      <!-- Cart Section -->
      <section class="cart-section">
        <div class="cart-header">
          <div class="cart-title">
            <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <circle cx="9" cy="21" r="1"/><circle cx="20" cy="21" r="1"/>
              <path d="M1 1h4l2.68 13.39a2 2 0 0 0 2 1.61h9.72a2 2 0 0 0 2-1.61L23 6H6"/>
            </svg>
            <h2>Current Cart</h2>
          </div>
          <div class="cart-stats">
            <div class="stat-pill" :class="{ empty: cartStore.itemCount === 0 }">
              <span class="stat-value">{{ cartStore.itemCount }}</span>
              <span class="stat-label">{{ cartStore.itemCount === 1 ? 'item' : 'items' }}</span>
            </div>
          </div>
        </div>

        <div class="cart-body">
          <!-- Empty State -->
          <div v-if="cartStore.itemCount === 0" class="empty-state">
            <div class="empty-icon">
              <svg width="80" height="80" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1">
                <circle cx="9" cy="21" r="1"/><circle cx="20" cy="21" r="1"/>
                <path d="M1 1h4l2.68 13.39a2 2 0 0 0 2 1.61h9.72a2 2 0 0 0 2-1.61L23 6H6"/>
              </svg>
              <div class="empty-pulse"></div>
            </div>
            <h3>No items in cart</h3>
            <p>Scan RFID tags to add items to your transaction</p>
          </div>

          <!-- Items Grid -->
          <div v-else class="items-container">
            <!-- Borrow Section -->
            <div v-if="cartStore.borrowItems.length > 0" class="item-group borrow-group">
              <div class="group-header">
                <div class="group-indicator borrow"></div>
                <span class="group-title">To Borrow</span>
                <span class="group-count">{{ cartStore.borrowItems.length }}</span>
              </div>
              <transition-group name="item-list" tag="div" class="items-grid">
                <div v-for="item in cartStore.borrowItems" :key="item.id" class="item-card borrow">
                  <div class="item-icon">
                    <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                      <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/>
                      <polyline points="14 2 14 8 20 8"/>
                    </svg>
                  </div>
                  <div class="item-details">
                    <span class="item-name">{{ item.name }}</span>
                    <span class="item-action">
                      <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <path d="M12 5v14M5 12h14"/>
                      </svg>
                      Checkout
                    </span>
                  </div>
                  <button @click="removeFromCart(item.id)" class="remove-btn" title="Remove">
                    <span class="sr-only">Remove</span>
                  </button>
                </div>
              </transition-group>
            </div>

            <!-- Return Section -->
            <div v-if="cartStore.returnItems.length > 0" class="item-group return-group">
              <div class="group-header">
                <div class="group-indicator return"></div>
                <span class="group-title">To Return</span>
                <span class="group-count">{{ cartStore.returnItems.length }}</span>
              </div>
              <transition-group name="item-list" tag="div" class="items-grid">
                <div v-for="item in cartStore.returnItems" :key="item.id" class="item-card return">
                  <div class="item-icon">
                    <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                      <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/>
                      <polyline points="14 2 14 8 20 8"/>
                    </svg>
                  </div>
                  <div class="item-details">
                    <span class="item-name">{{ item.name }}</span>
                    <span class="item-action return-action">
                      <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <polyline points="20 6 9 17 4 12"/>
                      </svg>
                      Check-in
                    </span>
                  </div>
                  <button @click="removeFromCart(item.id)" class="remove-btn" title="Remove">
                    <span class="sr-only">Remove</span>
                  </button>
                </div>
              </transition-group>
            </div>
          </div>
        </div>
      </section>

      <!-- Action Bar -->
      <footer class="action-bar">
        <button
          @click="commitTransaction"
          class="btn primary-btn"
          :disabled="cartStore.itemCount === 0 || processing"
        >
          <span v-if="!processing" class="btn-content">
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <polyline points="20 6 9 17 4 12"/>
            </svg>
            Confirm Transaction
          </span>
          <span v-else class="btn-content loading">
            <span class="spinner"></span>
            Processing...
          </span>
        </button>
        
        <button @click="clearCart" class="btn secondary-btn" :disabled="cartStore.itemCount === 0">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <polyline points="3 6 5 6 21 6"/>
            <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"/>
          </svg>
          Clear
        </button>
        
        <router-link to="/kiosk/history" class="btn tertiary-btn">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/>
          </svg>
          History
        </router-link>
      </footer>

      <!-- Toast Messages -->
      <transition name="toast-slide">
        <div v-if="message" :class="['toast', message.type]">
          <div class="toast-icon">
            <svg v-if="message.type === 'success'" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/><polyline points="22 4 12 14.01 9 11.01"/>
            </svg>
            <svg v-else width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <circle cx="12" cy="12" r="10"/><line x1="15" y1="9" x2="9" y2="15"/><line x1="9" y1="9" x2="15" y2="15"/>
            </svg>
          </div>
          <span class="toast-text">{{ message.text }}</span>
        </div>
      </transition>
    </div>
  </div>
</template>

<script>
import { ref, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/authStore'
import { useCartStore } from '../stores/cartStore'
import { useTheme } from '../composables/useTheme'
import api from '../services/api'
import { initSignalR, onCartUpdated, joinScannerGroup, closeSignalR } from '../services/signalr'

export default {
  name: 'Kiosk',
  setup() {
    const router = useRouter()
    const authStore = useAuthStore()
    const cartStore = useCartStore()
    const { isDark, toggleTheme } = useTheme()
    const message = ref(null)
    const processing = ref(false)
    const showSummary = ref(false)
    const summaryData = ref({ borrowed: [], returned: [], timestamp: '' })
    let sessionRefreshInProgress = false

    const handleLogout = () => {
      cartStore.clear()
      authStore.logout()
      router.push('/login')
    }

    const goToAdmin = () => {
      router.push('/dashboard')
    }

    const removeFromCart = async (itemId) => {
      try {
        await api.delete(`/session/items/${itemId}`)
        cartStore.removeItem(itemId)
      } catch (err) {
        message.value = { type: 'error', text: 'Failed to remove item' }
        setTimeout(() => {
          message.value = null
        }, 3000)
      }
    }

    const clearCart = async () => {
      try {
        await api.post('/session/clear')
        cartStore.clear()
        message.value = { type: 'success', text: 'Cart cleared' }
        setTimeout(() => {
          message.value = null
        }, 2000)
      } catch (err) {
        message.value = { type: 'error', text: 'Failed to clear cart' }
        setTimeout(() => {
          message.value = null
        }, 3000)
      }
    }

    const refreshSessionCart = async () => {
      if (sessionRefreshInProgress) {
        return
      }
      sessionRefreshInProgress = true
      try {
        const response = await api.get('/session/current')
        cartStore.updateFromServer(response.data)
      } catch (err) {
        console.error('Unable to refresh session cart', err)
      } finally {
        sessionRefreshInProgress = false
      }
    }

    const commitTransaction = async () => {
      // Capture items before clearing for summary
      const borrowedItems = [...cartStore.borrowItems]
      const returnedItems = [...cartStore.returnItems]
      
      processing.value = true
      try {
        await api.post('/transaction/commit', {
          deviceId: authStore.scannerDeviceId,
          notes: null
        })
        
        // Prepare summary data
        summaryData.value = {
          borrowed: borrowedItems,
          returned: returnedItems,
          timestamp: new Date().toLocaleString('en-US', {
            weekday: 'short',
            year: 'numeric',
            month: 'short',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
          })
        }
        
        cartStore.clear()
        showSummary.value = true
      } catch (err) {
        message.value = { type: 'error', text: err.response?.data?.message || 'Transaction failed' }
        setTimeout(() => {
          message.value = null
        }, 4000)
      } finally {
        processing.value = false
      }
    }

    const closeSummary = () => {
      showSummary.value = false
      summaryData.value = { borrowed: [], returned: [], timestamp: '' }
    }

    onMounted(async () => {
      // Force light mode
      if (isDark.value) {
        toggleTheme()
      }
      try {
        const token = localStorage.getItem('authToken')
        await initSignalR(token)
        if (authStore.scannerDeviceId) {
          await joinScannerGroup(authStore.scannerDeviceId)
        }
        onCartUpdated((updatedCart) => {
          cartStore.updateFromServer(updatedCart)
        })
        await refreshSessionCart()
      } catch (err) {
        console.error('SignalR initialization failed:', err)
      }
    })

    onUnmounted(() => {
      closeSignalR()
    })

    return {
      authStore,
      cartStore,
      message,
      processing,
      showSummary,
      summaryData,
      handleLogout,
      goToAdmin,
      removeFromCart,
      clearCart,
      commitTransaction,
      closeSummary
    }
  }
}
</script>

<style scoped>
/* ============================================
   KIOSK PAGE - PROFESSIONAL DESIGN
   ============================================ */

.kiosk-page {
  width: 100%;
  min-height: 100vh;
  background: linear-gradient(135deg, #f0f4f8 0%, #e8f0fe 50%, #f0f4f8 100%);
  padding: 1.5rem;
  display: flex;
  align-items: center;
  justify-content: center;
}

.kiosk-container {
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(20px);
  border-radius: 24px;
  box-shadow: 
    0 4px 6px rgba(0, 61, 107, 0.04),
    0 10px 40px rgba(0, 61, 107, 0.08),
    0 0 0 1px rgba(255, 255, 255, 0.8) inset;
  width: 100%;
  max-width: 800px;
  padding: 2rem;
  animation: containerEnter 0.5s cubic-bezier(0.16, 1, 0.3, 1);
  transition: filter 0.3s ease;
}

.kiosk-container.blur-bg {
  filter: blur(4px);
  pointer-events: none;
}

@keyframes containerEnter {
  from {
    opacity: 0;
    transform: translateY(30px) scale(0.98);
  }
  to {
    opacity: 1;
    transform: translateY(0) scale(1);
  }
}

/* ============================================
   HEADER
   ============================================ */

.kiosk-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  margin-bottom: 2rem;
  padding-bottom: 1.5rem;
  border-bottom: 1px solid rgba(0, 61, 107, 0.08);
}

.header-brand {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.brand-icon {
  width: 52px;
  height: 52px;
  background: linear-gradient(135deg, var(--primary-dark) 0%, var(--primary-light) 100%);
  border-radius: 14px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  box-shadow: 0 4px 12px rgba(0, 61, 107, 0.25);
}

.brand-text h1 {
  margin: 0;
  font-size: 1.5rem;
  font-weight: 800;
  color: var(--primary-dark);
  letter-spacing: -0.5px;
}

.scan-status {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.85rem;
  color: var(--accent-green);
  font-weight: 600;
}

.status-dot {
  width: 8px;
  height: 8px;
  background: var(--accent-green);
  border-radius: 50%;
  animation: statusPulse 2s ease-in-out infinite;
  box-shadow: 0 0 8px var(--accent-green);
}

@keyframes statusPulse {
  0%, 100% { opacity: 1; transform: scale(1); }
  50% { opacity: 0.6; transform: scale(1.3); }
}

.scanner-info {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem 1rem;
  background: rgba(0, 61, 107, 0.05);
  border-radius: 20px;
  font-size: 0.8rem;
  color: var(--primary-dark);
  font-weight: 500;
}

.header-actions {
  display: flex;
  gap: 0.75rem;
}

.action-btn {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.6rem 1rem;
  border-radius: 10px;
  font-size: 0.85rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
  border: none;
}

.action-btn.admin {
  background: linear-gradient(135deg, var(--primary-light) 0%, var(--primary-dark) 100%);
  color: white;
  box-shadow: 0 2px 8px rgba(30, 144, 255, 0.3);
}

.action-btn.admin:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 16px rgba(30, 144, 255, 0.4);
}

.action-btn.logout {
  background: rgba(220, 53, 69, 0.08);
  color: #dc3545;
  border: 1px solid rgba(220, 53, 69, 0.2);
}

.action-btn.logout:hover {
  background: #dc3545;
  color: white;
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(220, 53, 69, 0.3);
}

/* ============================================
   CART SECTION
   ============================================ */

.cart-section {
  background: linear-gradient(135deg, rgba(248, 250, 252, 0.8) 0%, rgba(240, 243, 255, 0.6) 100%);
  border-radius: 20px;
  padding: 1.5rem;
  margin-bottom: 1.5rem;
  border: 1px solid rgba(0, 61, 107, 0.06);
  min-height: 320px;
}

.cart-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 1.5rem;
}

.cart-title {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  color: var(--primary-dark);
}

.cart-title svg {
  opacity: 0.7;
}

.cart-title h2 {
  margin: 0;
  font-size: 1.15rem;
  font-weight: 700;
}

.stat-pill {
  display: flex;
  align-items: center;
  gap: 0.4rem;
  padding: 0.5rem 1rem;
  background: linear-gradient(135deg, var(--primary-dark) 0%, var(--primary-light) 100%);
  border-radius: 20px;
  color: white;
  font-weight: 600;
  box-shadow: 0 2px 8px rgba(0, 61, 107, 0.2);
  transition: all 0.3s ease;
}

.stat-pill.empty {
  background: var(--bg-secondary);
  color: var(--accent-gray);
  box-shadow: none;
}

.stat-value {
  font-size: 1.1rem;
  font-weight: 800;
}

.stat-label {
  font-size: 0.8rem;
  opacity: 0.9;
}

/* Empty State */
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 3rem 1rem;
  text-align: center;
}

.empty-icon {
  position: relative;
  color: var(--accent-gray);
  opacity: 0.4;
  margin-bottom: 1.5rem;
}

.empty-pulse {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  width: 100px;
  height: 100px;
  border-radius: 50%;
  background: rgba(30, 144, 255, 0.1);
  animation: emptyPulse 3s ease-in-out infinite;
}

@keyframes emptyPulse {
  0%, 100% { transform: translate(-50%, -50%) scale(0.8); opacity: 0.3; }
  50% { transform: translate(-50%, -50%) scale(1.2); opacity: 0.1; }
}

.empty-state h3 {
  margin: 0 0 0.5rem;
  font-size: 1.2rem;
  color: var(--primary-dark);
  font-weight: 700;
}

.empty-state p {
  margin: 0;
  color: var(--accent-gray);
  font-size: 0.95rem;
}

/* Items Container */
.items-container {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
  max-height: 350px;
  overflow-y: auto;
  padding-right: 0.5rem;
}

.items-container::-webkit-scrollbar {
  width: 6px;
}

.items-container::-webkit-scrollbar-track {
  background: transparent;
}

.items-container::-webkit-scrollbar-thumb {
  background: rgba(0, 61, 107, 0.15);
  border-radius: 3px;
}

.item-group {
  background: white;
  border-radius: 16px;
  padding: 1.25rem;
  box-shadow: 0 2px 8px rgba(0, 61, 107, 0.04);
  border: 1px solid rgba(0, 61, 107, 0.06);
}

.group-header {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  margin-bottom: 1rem;
  padding-bottom: 0.75rem;
  border-bottom: 1px solid rgba(0, 61, 107, 0.06);
}

.group-indicator {
  width: 4px;
  height: 24px;
  border-radius: 2px;
}

.group-indicator.borrow {
  background: linear-gradient(180deg, var(--accent-green) 0%, #34d399 100%);
}

.group-indicator.return {
  background: linear-gradient(180deg, #ffc107 0%, #ffb300 100%);
}

.group-title {
  font-weight: 700;
  color: var(--primary-dark);
  font-size: 0.9rem;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.group-count {
  margin-left: auto;
  background: rgba(0, 61, 107, 0.08);
  padding: 0.25rem 0.75rem;
  border-radius: 12px;
  font-size: 0.8rem;
  font-weight: 700;
  color: var(--primary-dark);
}

.items-grid {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

/* Item Cards */
.item-card {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 1rem 1.25rem;
  background: linear-gradient(135deg, rgba(248, 250, 252, 0.9) 0%, rgba(255, 255, 255, 0.9) 100%);
  border-radius: 14px;
  border: 1px solid rgba(0, 61, 107, 0.08);
  transition: all 0.3s cubic-bezier(0.16, 1, 0.3, 1);
  position: relative;
  overflow: hidden;
}

.item-card::before {
  content: '';
  position: absolute;
  left: 0;
  top: 0;
  bottom: 0;
  width: 4px;
  transition: width 0.3s ease;
}

.item-card.borrow::before {
  background: linear-gradient(180deg, var(--accent-green) 0%, #34d399 100%);
}

.item-card.return::before {
  background: linear-gradient(180deg, #ffc107 0%, #ffb300 100%);
}

.item-card:hover {
  transform: translateX(4px);
  box-shadow: 0 4px 16px rgba(0, 61, 107, 0.1);
}

.item-card:hover::before {
  width: 6px;
}

.item-icon {
  width: 44px;
  height: 44px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.item-card.borrow .item-icon {
  background: linear-gradient(135deg, rgba(80, 200, 120, 0.15) 0%, rgba(52, 211, 153, 0.1) 100%);
  color: var(--accent-green);
}

.item-card.return .item-icon {
  background: linear-gradient(135deg, rgba(255, 193, 7, 0.15) 0%, rgba(255, 179, 0, 0.1) 100%);
  color: #f59e0b;
}

.item-details {
  flex: 1;
  min-width: 0;
}

.item-details .item-name {
  display: block;
  font-weight: 700;
  color: var(--primary-dark);
  font-size: 0.95rem;
  margin-bottom: 0.25rem;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.item-action {
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  font-size: 0.75rem;
  font-weight: 600;
  color: var(--accent-green);
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.item-action.return-action {
  color: #f59e0b;
}

.remove-btn {
  width: 36px;
  height: 36px;
  border-radius: 10px;
  background: rgba(0, 61, 107, 0.04);
  border: 1px solid rgba(0, 61, 107, 0.08);
  color: #2c3e50 !important;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s ease;
  flex-shrink: 0;
  background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='18' height='18' viewBox='0 0 24 24' fill='none' stroke='%232c3e50' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'%3E%3Cpolyline points='3 6 5 6 21 6'/%3E%3Cpath d='M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2'/%3E%3Cline x1='10' y1='11' x2='10' y2='17'/%3E%3Cline x1='14' y1='11' x2='14' y2='17'/%3E%3C/svg%3E");
  background-repeat: no-repeat;
  background-position: center;
  background-size: 18px 18px;
}

.remove-btn:hover {
  background: rgba(220, 53, 69, 0.1);
  border-color: rgba(220, 53, 69, 0.2);
  color: #dc3545;
  transform: scale(1.05);
}

.sr-only {
  position: absolute;
  width: 1px;
  height: 1px;
  padding: 0;
  margin: -1px;
  overflow: hidden;
  clip: rect(0, 0, 0, 0);
  white-space: nowrap;
  border: 0;
}

/* Item List Transitions */
.item-list-enter-active {
  animation: itemSlideIn 0.4s cubic-bezier(0.16, 1, 0.3, 1);
}

.item-list-leave-active {
  animation: itemSlideOut 0.3s ease-in forwards;
}

@keyframes itemSlideIn {
  from {
    opacity: 0;
    transform: translateX(-20px) scale(0.95);
  }
  to {
    opacity: 1;
    transform: translateX(0) scale(1);
  }
}

@keyframes itemSlideOut {
  to {
    opacity: 0;
    transform: translateX(20px) scale(0.95);
  }
}

/* ============================================
   ACTION BAR
   ============================================ */

.action-bar {
  display: grid;
  grid-template-columns: 2fr 1fr 1fr;
  gap: 0.75rem;
}

.btn {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.6rem;
  padding: 1rem 1.25rem;
  border: none;
  border-radius: 14px;
  font-weight: 700;
  font-size: 0.95rem;
  cursor: pointer;
  transition: all 0.3s cubic-bezier(0.16, 1, 0.3, 1);
  text-decoration: none;
  position: relative;
  overflow: hidden;
}

.btn-content {
  display: flex;
  align-items: center;
  gap: 0.6rem;
}

.btn-content.loading {
  gap: 0.75rem;
}

.primary-btn {
  background: linear-gradient(135deg, var(--accent-green) 0%, #34d399 100%);
  color: white;
  box-shadow: 0 4px 16px rgba(80, 200, 120, 0.35);
}

.primary-btn:hover:not(:disabled) {
  transform: translateY(-3px);
  box-shadow: 0 8px 24px rgba(80, 200, 120, 0.45);
}

.primary-btn:active:not(:disabled) {
  transform: translateY(-1px);
}

.primary-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
  transform: none;
}

.secondary-btn {
  background: rgba(0, 61, 107, 0.06);
  color: var(--primary-dark);
  border: 1px solid rgba(0, 61, 107, 0.1);
}

.secondary-btn:hover:not(:disabled) {
  background: rgba(0, 61, 107, 0.12);
  transform: translateY(-2px);
}

.secondary-btn:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

.tertiary-btn {
  background: linear-gradient(135deg, var(--primary-dark) 0%, var(--primary-light) 100%);
  color: white;
  box-shadow: 0 4px 16px rgba(0, 61, 107, 0.25);
}

.tertiary-btn:hover {
  transform: translateY(-3px);
  box-shadow: 0 8px 24px rgba(0, 61, 107, 0.35);
}

.spinner {
  width: 18px;
  height: 18px;
  border: 2px solid rgba(255, 255, 255, 0.25);
  border-top-color: white;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

/* ============================================
   TOAST MESSAGES
   ============================================ */

.toast {
  position: fixed;
  bottom: 2rem;
  left: 50%;
  transform: translateX(-50%);
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 1rem 1.5rem;
  border-radius: 14px;
  font-weight: 600;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.15);
  z-index: 1000;
}

.toast.success {
  background: linear-gradient(135deg, var(--accent-green) 0%, #34d399 100%);
  color: white;
}

.toast.error {
  background: linear-gradient(135deg, #ef4444 0%, #dc2626 100%);
  color: white;
}

.toast-icon {
  display: flex;
  align-items: center;
  justify-content: center;
}

.toast-slide-enter-active {
  animation: toastIn 0.4s cubic-bezier(0.16, 1, 0.3, 1);
}

.toast-slide-leave-active {
  animation: toastOut 0.3s ease-in forwards;
}

@keyframes toastIn {
  from {
    opacity: 0;
    transform: translateX(-50%) translateY(20px) scale(0.9);
  }
  to {
    opacity: 1;
    transform: translateX(-50%) translateY(0) scale(1);
  }
}

@keyframes toastOut {
  to {
    opacity: 0;
    transform: translateX(-50%) translateY(20px) scale(0.9);
  }
}

/* ============================================
   SUMMARY MODAL
   ============================================ */

.summary-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 31, 63, 0.6);
  backdrop-filter: blur(8px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 2000;
  padding: 1rem;
}

.summary-modal {
  background: white;
  border-radius: 24px;
  width: 100%;
  max-width: 500px;
  max-height: 90vh;
  overflow-y: auto;
  box-shadow: 0 24px 80px rgba(0, 0, 0, 0.25);
  animation: modalEnter 0.5s cubic-bezier(0.16, 1, 0.3, 1);
}

@keyframes modalEnter {
  from {
    opacity: 0;
    transform: scale(0.9) translateY(20px);
  }
  to {
    opacity: 1;
    transform: scale(1) translateY(0);
  }
}

.summary-header {
  text-align: center;
  padding: 2.5rem 2rem 1.5rem;
  background: linear-gradient(135deg, rgba(80, 200, 120, 0.08) 0%, rgba(52, 211, 153, 0.04) 100%);
  border-bottom: 1px solid rgba(80, 200, 120, 0.15);
}

.success-animation {
  margin-bottom: 1rem;
}

.checkmark {
  width: 80px;
  height: 80px;
  margin: 0 auto;
  display: block;
}

.checkmark-circle {
  stroke-dasharray: 166;
  stroke-dashoffset: 166;
  stroke-width: 2;
  stroke-miterlimit: 10;
  stroke: var(--accent-green);
  fill: none;
  animation: stroke 0.6s cubic-bezier(0.65, 0, 0.45, 1) forwards;
}

.checkmark-check {
  transform-origin: 50% 50%;
  stroke-dasharray: 48;
  stroke-dashoffset: 48;
  stroke: var(--accent-green);
  stroke-width: 3;
  stroke-linecap: round;
  animation: stroke 0.3s cubic-bezier(0.65, 0, 0.45, 1) 0.6s forwards;
}

@keyframes stroke {
  100% { stroke-dashoffset: 0; }
}

.summary-header h2 {
  margin: 0 0 0.5rem;
  font-size: 1.5rem;
  color: var(--primary-dark);
  font-weight: 800;
}

.summary-timestamp {
  margin: 0;
  color: var(--accent-gray);
  font-size: 0.9rem;
}

.summary-content {
  padding: 1.5rem 2rem;
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.summary-section {
  background: rgba(248, 250, 252, 0.8);
  border-radius: 16px;
  padding: 1.25rem;
}

.summary-section-header {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  margin-bottom: 1rem;
  padding-bottom: 0.75rem;
  border-bottom: 1px solid rgba(0, 61, 107, 0.08);
  font-weight: 700;
  color: var(--primary-dark);
}

.section-icon {
  width: 32px;
  height: 32px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.borrowed-icon {
  background: linear-gradient(135deg, var(--accent-green) 0%, #34d399 100%);
  color: white;
}

.returned-icon {
  background: linear-gradient(135deg, #ffc107 0%, #f59e0b 100%);
  color: white;
}

.section-count {
  margin-left: auto;
  background: rgba(0, 61, 107, 0.1);
  padding: 0.25rem 0.75rem;
  border-radius: 10px;
  font-size: 0.8rem;
}

.summary-items {
  display: flex;
  flex-direction: column;
  gap: 0.6rem;
}

.summary-item {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.75rem 1rem;
  background: white;
  border-radius: 12px;
  border: 1px solid rgba(0, 61, 107, 0.06);
  animation: itemAppear 0.5s cubic-bezier(0.16, 1, 0.3, 1) backwards;
}

.item-icon-wrapper {
  width: 32px;
  height: 32px;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.item-icon-wrapper.borrowed {
  background: rgba(80, 200, 120, 0.15);
  color: var(--accent-green);
}

.item-icon-wrapper.returned {
  background: rgba(255, 193, 7, 0.15);
  color: #f59e0b;
}

.summary-item .item-name {
  flex: 1;
  font-weight: 600;
  color: var(--primary-dark);
  font-size: 0.9rem;
}

.item-status-badge {
  padding: 0.25rem 0.6rem;
  border-radius: 6px;
  font-size: 0.7rem;
  font-weight: 800;
  letter-spacing: 0.5px;
}

.item-status-badge.out {
  background: linear-gradient(135deg, var(--accent-green) 0%, #34d399 100%);
  color: white;
}

.item-status-badge.in {
  background: linear-gradient(135deg, #ffc107 0%, #f59e0b 100%);
  color: white;
}

/* Item fly animations */
.item-fly-out-enter-active {
  animation: flyOut 0.6s cubic-bezier(0.16, 1, 0.3, 1) backwards;
}

.item-fly-in-enter-active {
  animation: flyIn 0.6s cubic-bezier(0.16, 1, 0.3, 1) backwards;
}

@keyframes flyOut {
  from {
    opacity: 0;
    transform: translateX(-30px) scale(0.8);
  }
  to {
    opacity: 1;
    transform: translateX(0) scale(1);
  }
}

@keyframes flyIn {
  from {
    opacity: 0;
    transform: translateX(30px) scale(0.8);
  }
  to {
    opacity: 1;
    transform: translateX(0) scale(1);
  }
}

@keyframes itemAppear {
  from {
    opacity: 0;
    transform: translateY(10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.summary-footer {
  padding: 1.5rem 2rem 2rem;
  display: flex;
  justify-content: center;
}

.btn-back {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 1rem 2rem;
  background: linear-gradient(135deg, var(--primary-dark) 0%, var(--primary-light) 100%);
  color: white;
  border: none;
  border-radius: 14px;
  font-size: 1rem;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.3s cubic-bezier(0.16, 1, 0.3, 1);
  box-shadow: 0 4px 16px rgba(0, 61, 107, 0.25);
}

.btn-back:hover {
  transform: translateY(-3px);
  box-shadow: 0 8px 24px rgba(0, 61, 107, 0.35);
}

/* Modal transitions */
.modal-fade-enter-active {
  animation: fadeIn 0.3s ease;
}

.modal-fade-leave-active {
  animation: fadeOut 0.3s ease forwards;
}

@keyframes fadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}

@keyframes fadeOut {
  to { opacity: 0; }
}

/* ============================================
   RESPONSIVE
   ============================================ */

@media (max-width: 700px) {
  .kiosk-page {
    padding: 0.75rem;
  }

  .kiosk-container {
    padding: 1.25rem;
    border-radius: 20px;
  }

  .kiosk-header {
    flex-wrap: wrap;
    gap: 1rem;
  }

  .scanner-info {
    order: 3;
    width: 100%;
    justify-content: center;
  }

  .header-actions {
    margin-left: auto;
  }

  .action-btn span {
    display: none;
  }

  .action-btn {
    padding: 0.6rem;
  }

  .cart-section {
    padding: 1rem;
    min-height: 280px;
  }

  .action-bar {
    grid-template-columns: 1fr 1fr;
    gap: 0.6rem;
  }

  .primary-btn {
    grid-column: 1 / -1;
  }

  .btn {
    padding: 0.85rem 1rem;
    font-size: 0.85rem;
  }

  .summary-modal {
    border-radius: 20px;
  }

  .summary-header {
    padding: 2rem 1.5rem 1.25rem;
  }

  .summary-content {
    padding: 1.25rem 1.5rem;
  }

  .summary-footer {
    padding: 1.25rem 1.5rem 1.5rem;
  }
}

@media (max-width: 400px) {
  .brand-text h1 {
    font-size: 1.2rem;
  }

  .brand-icon {
    width: 44px;
    height: 44px;
  }

  .item-card {
    padding: 0.85rem 1rem;
  }

  .item-icon {
    width: 38px;
    height: 38px;
  }
}
</style>

