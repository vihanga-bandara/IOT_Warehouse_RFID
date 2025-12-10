<template>
  <div class="kiosk-page">
    <div class="kiosk-container">
      <div class="kiosk-header">
        <h1>
          <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/><polyline points="9 22 9 12 15 12 15 22"/>
          </svg>
          RFID Scanner
        </h1>
        <p class="scan-status">
          <span class="pulse"></span>
          Ready for scanning
        </p>
      </div>

      <div class="cart-display">
        <div class="cart-header">
          <h2>Current Cart</h2>
          <span class="item-badge">{{ cartStore.itemCount }} items</span>
        </div>

        <div v-if="cartStore.itemCount === 0" class="empty-cart">
          <svg width="64" height="64" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
            <circle cx="9" cy="21" r="1"/><circle cx="20" cy="21" r="1"/><path d="M1 1h4l2.68 13.39a2 2 0 0 0 2 1.61h9.72a2 2 0 0 0 2-1.61L23 6H6"/>
          </svg>
          <p>Scan RFID tags to add items</p>
          <small>Use your RFID scanner to begin</small>
        </div>

        <div v-else class="cart-items">
          <div v-if="cartStore.borrowItems.length > 0" class="items-section">
            <div class="section-header">
              <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M13 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V9z"/><polyline points="13 2 13 9 20 9"/>
              </svg>
              <span>To Borrow</span>
              <span class="count">{{ cartStore.borrowItems.length }}</span>
            </div>
            <div class="items-list">
              <div v-for="item in cartStore.borrowItems" :key="item.id" class="cart-item borrow">
                <span class="item-name">{{ item.name }}</span>
                <button @click="removeFromCart(item.id)" class="remove-btn" title="Remove item">
                  <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
                  </svg>
                </button>
              </div>
            </div>
          </div>

          <div v-if="cartStore.returnItems.length > 0" class="items-section">
            <div class="section-header">
              <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M1 4v6h6M23 20v-6h-6"/>
              </svg>
              <span>To Return</span>
              <span class="count">{{ cartStore.returnItems.length }}</span>
            </div>
            <div class="items-list">
              <div v-for="item in cartStore.returnItems" :key="item.id" class="cart-item return">
                <span class="item-name">{{ item.name }}</span>
                <button @click="removeFromCart(item.id)" class="remove-btn" title="Remove item">
                  <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
                  </svg>
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="action-buttons">
        <button
          @click="commitTransaction"
          class="btn btn-confirm"
          :disabled="cartStore.itemCount === 0 || processing"
        >
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <polyline points="20 6 9 17 4 12"/>
          </svg>
          <span v-if="!processing">Confirm Transaction</span>
          <span v-else class="spinner-text">
            <span class="spinner"></span>
            Processing...
          </span>
        </button>
        <button @click="clearCart" class="btn btn-secondary" :disabled="cartStore.itemCount === 0">
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <polyline points="3 6 5 6 21 6"/><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"/>
          </svg>
          Clear Cart
        </button>
        <router-link to="/kiosk/history" class="btn btn-history">
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/>
          </svg>
          View History
        </router-link>
      </div>

      <transition name="message-fade">
        <div v-if="message" :class="['message', message.type]">
          <div class="message-content">
            <svg v-if="message.type === 'success'" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/><polyline points="22 4 12 14.01 9 11.01"/>
            </svg>
            <svg v-else width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <circle cx="12" cy="12" r="10"/><line x1="15" y1="9" x2="9" y2="15"/><line x1="9" y1="9" x2="15" y2="15"/>
            </svg>
            {{ message.text }}
          </div>
        </div>
      </transition>
    </div>
  </div>
</template>

<script>
import { ref, onMounted, onUnmounted } from 'vue'
import { useCartStore } from '../stores/cartStore'
import api from '../services/api'
import { initSignalR, onCartUpdated, closeSignalR } from '../services/signalr'

export default {
  name: 'Kiosk',
  setup() {
    const cartStore = useCartStore()
    const message = ref(null)
    const processing = ref(false)

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

    const commitTransaction = async () => {
      processing.value = true
      try {
        await api.post('/transaction/commit')
        cartStore.clear()
        message.value = { type: 'success', text: 'Transaction completed successfully!' }
        setTimeout(() => {
          message.value = null
        }, 3000)
      } catch (err) {
        message.value = { type: 'error', text: err.response?.data?.message || 'Transaction failed' }
        setTimeout(() => {
          message.value = null
        }, 4000)
      } finally {
        processing.value = false
      }
    }

    onMounted(async () => {
      try {
        const token = localStorage.getItem('authToken')
        await initSignalR(token)
        onCartUpdated((updatedCart) => {
          cartStore.updateFromServer(updatedCart)
        })
      } catch (err) {
        console.error('SignalR initialization failed:', err)
      }
    })

    onUnmounted(() => {
      closeSignalR()
    })

    return {
      cartStore,
      message,
      processing,
      removeFromCart,
      clearCart,
      commitTransaction
    }
  }
}
</script>

<style scoped>
.kiosk-page {
  width: 100%;
  min-height: 100vh;
  background: linear-gradient(135deg, #f8f9fa 0%, #f0f3ff 100%);
  padding: 1rem;
  display: flex;
  align-items: center;
  justify-content: center;
}

.kiosk-container {
  background: white;
  border-radius: 16px;
  box-shadow: 0 4px 16px rgba(0, 61, 107, 0.08);
  width: 100%;
  max-width: 700px;
  padding: 2rem;
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

.kiosk-header {
  text-align: center;
  margin-bottom: 2rem;
  padding-bottom: 1.5rem;
  border-bottom: 2px solid var(--border-color);
}

.kiosk-header h1 {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.75rem;
  color: var(--primary-dark);
  margin: 0 0 0.5rem;
  font-size: 1.8rem;
  font-weight: 800;
}

.scan-status {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  color: var(--accent-green);
  margin: 0;
  font-weight: 600;
  font-size: 1rem;
}

.pulse {
  width: 12px;
  height: 12px;
  background: var(--accent-green);
  border-radius: 50%;
  animation: pulse 2s ease-in-out infinite;
}

@keyframes pulse {
  0%, 100% {
    opacity: 1;
    transform: scale(1);
  }
  50% {
    opacity: 0.5;
    transform: scale(1.2);
  }
}

.cart-display {
  background: linear-gradient(135deg, rgba(0, 61, 107, 0.02) 0%, rgba(80, 200, 120, 0.02) 100%);
  border: 2px solid var(--border-color);
  border-radius: 14px;
  padding: 1.5rem;
  margin-bottom: 2rem;
  min-height: 300px;
  display: flex;
  flex-direction: column;
}

.cart-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;
  padding-bottom: 0.75rem;
  border-bottom: 2px solid var(--border-color);
}

.cart-header h2 {
  color: var(--primary-dark);
  margin: 0;
  font-size: 1.2rem;
  font-weight: 700;
}

.item-badge {
  background: linear-gradient(135deg, var(--primary-light) 0%, var(--accent-green) 100%);
  color: white;
  padding: 0.5rem 1rem;
  border-radius: 20px;
  font-size: 0.85rem;
  font-weight: 700;
}

.empty-cart {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  flex: 1;
  color: var(--accent-gray);
  text-align: center;
}

.empty-cart svg {
  opacity: 0.4;
  margin-bottom: 1rem;
}

.empty-cart p {
  margin: 0 0 0.5rem;
  font-size: 1.1rem;
  font-weight: 600;
  color: var(--primary-dark);
}

.empty-cart small {
  font-size: 0.9rem;
  color: var(--accent-gray);
}

.cart-items {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
  overflow-y: auto;
  max-height: 400px;
  -webkit-overflow-scrolling: touch;
}

.items-section {
  display: flex;
  flex-direction: column;
  gap: 0.8rem;
}

.section-header {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  color: var(--primary-dark);
  font-weight: 700;
  font-size: 0.95rem;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.section-header svg {
  opacity: 0.7;
}

.count {
  margin-left: auto;
  background: var(--border-color);
  padding: 0.2rem 0.6rem;
  border-radius: 4px;
  font-size: 0.8rem;
  font-weight: 600;
}

.items-list {
  display: flex;
  flex-direction: column;
  gap: 0.6rem;
}

.cart-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 1rem;
  border-radius: 10px;
  border-left: 4px solid;
  transition: all 0.3s ease;
  background: white;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
}

.cart-item.borrow {
  border-left-color: var(--accent-green);
  background: linear-gradient(135deg, rgba(80, 200, 120, 0.08) 0%, rgba(80, 200, 120, 0.04) 100%);
}

.cart-item.return {
  border-left-color: #ffc107;
  background: linear-gradient(135deg, rgba(255, 193, 7, 0.08) 0%, rgba(255, 193, 7, 0.04) 100%);
}

.cart-item:hover {
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

.item-name {
  flex: 1;
  font-weight: 600;
  color: var(--dark-text);
  font-size: 1rem;
}

.remove-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 40px;
  height: 40px;
  background: #ff6b6b;
  color: white;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  font-size: 1rem;
  transition: all 0.3s ease;
  flex-shrink: 0;
}

.remove-btn:hover {
  background: #ff5252;
  transform: scale(1.05);
}

.remove-btn:active {
  transform: scale(0.95);
}

@media (max-width: 600px) {
  .kiosk-container {
    padding: 1.5rem;
  }

  .kiosk-header h1 {
    font-size: 1.4rem;
  }

  .cart-display {
    min-height: 250px;
    padding: 1rem;
  }

  .cart-items {
    max-height: 350px;
  }

  .item-name {
    font-size: 0.95rem;
  }

  .remove-btn {
    width: 36px;
    height: 36px;
  }
}

.action-buttons {
  display: grid;
  grid-template-columns: 2fr 1fr 1.5fr;
  gap: 0.75rem;
  margin-bottom: 1.5rem;
}

.btn {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  padding: 1rem;
  border: none;
  border-radius: 10px;
  font-weight: 700;
  font-size: 1rem;
  cursor: pointer;
  transition: all 0.3s ease;
  text-decoration: none;
  min-height: 50px;
  flex-shrink: 0;
}

.btn-confirm {
  background: linear-gradient(135deg, var(--accent-green) 0%, #45a049 100%);
  color: white;
  box-shadow: 0 4px 12px rgba(80, 200, 120, 0.3);
}

.btn-confirm:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(80, 200, 120, 0.4);
}

.btn-confirm:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.spinner-text {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.spinner {
  width: 16px;
  height: 16px;
  border: 2px solid rgba(255, 255, 255, 0.3);
  border-top-color: white;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.btn-secondary {
  background: var(--border-color);
  color: var(--dark-text);
}

.btn-secondary:hover:not(:disabled) {
  background: #d0d5dd;
  color: var(--primary-dark);
}

.btn-secondary:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.btn-history {
  background: linear-gradient(135deg, var(--primary-dark) 0%, var(--primary-light) 100%);
  color: white;
  box-shadow: 0 4px 12px rgba(30, 144, 255, 0.3);
}

.btn-history:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(30, 144, 255, 0.4);
}

@media (max-width: 600px) {
  .action-buttons {
    grid-template-columns: 1fr;
    gap: 0.6rem;
  }

  .btn {
    padding: 0.8rem;
    font-size: 0.9rem;
    min-height: 44px;
  }
}

.message {
  padding: 1rem;
  border-radius: 10px;
  text-align: center;
  font-weight: 600;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.75rem;
  animation: slideDown 0.3s ease-out;
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

.message-content {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  flex: 1;
}

.message.success {
  background: linear-gradient(135deg, rgba(80, 200, 120, 0.1) 0%, rgba(80, 200, 120, 0.05) 100%);
  color: #2d5f3f;
  border: 2px solid var(--accent-green);
}

.message.success svg {
  color: var(--accent-green);
}

.message.error {
  background: linear-gradient(135deg, rgba(255, 107, 107, 0.1) 0%, rgba(255, 107, 107, 0.05) 100%);
  color: #7d2a2a;
  border: 2px solid #ff6b6b;
}

.message.error svg {
  color: #ff6b6b;
}

.message-fade-enter-active,
.message-fade-leave-active {
  transition: all 0.3s ease;
}

.message-fade-enter-from,
.message-fade-leave-to {
  opacity: 0;
  transform: translateY(-20px);
}
</style>
