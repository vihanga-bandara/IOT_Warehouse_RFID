<template>
  <div class="kiosk-container">
    <div class="kiosk-content">
      <h2>Scanner Ready</h2>
      <p class="instructions">Scan items with your RFID reader</p>

      <div class="cart-section">
        <h3>Current Cart ({{ cartStore.itemCount }} items)</h3>

        <div v-if="cartStore.borrowItems.length > 0" class="items-list">
          <div class="list-title">To Borrow ({{ cartStore.borrowItems.length }})</div>
          <div v-for="item in cartStore.borrowItems" :key="item.id" class="cart-item borrow">
            <span class="item-name">{{ item.name }}</span>
            <button @click="removeFromCart(item.id)" class="remove-btn">Remove</button>
          </div>
        </div>

        <div v-if="cartStore.returnItems.length > 0" class="items-list">
          <div class="list-title">To Return ({{ cartStore.returnItems.length }})</div>
          <div v-for="item in cartStore.returnItems" :key="item.id" class="cart-item return">
            <span class="item-name">{{ item.name }}</span>
            <button @click="removeFromCart(item.id)" class="remove-btn">Remove</button>
          </div>
        </div>

        <div v-if="cartStore.itemCount === 0" class="empty-cart">
          <p>Scan an RFID tag to get started</p>
        </div>
      </div>

      <div class="action-buttons">
        <button
          @click="commitTransaction"
          class="btn btn-primary"
          :disabled="cartStore.itemCount === 0 || processing"
        >
          {{ processing ? 'Processing...' : 'Confirm Transaction' }}
        </button>
        <button @click="clearCart" class="btn btn-secondary">
          Clear Cart
        </button>
        <router-link to="/kiosk/history" class="btn btn-info">
          View History
        </router-link>
      </div>

      <div v-if="message" :class="['message', message.type]">
        {{ message.text }}
      </div>
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
      }
    }

    const clearCart = async () => {
      try {
        await api.post('/session/clear')
        cartStore.clear()
        message.value = { type: 'success', text: 'Cart cleared' }
      } catch (err) {
        message.value = { type: 'error', text: 'Failed to clear cart' }
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
.kiosk-container {
  display: flex;
  justify-content: center;
  min-height: 100vh;
  background: #ecf0f1;
  padding: 1rem;
}

.kiosk-content {
  background: white;
  padding: 2rem;
  border-radius: 8px;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
  width: 100%;
  max-width: 600px;
}

.kiosk-content h2 {
  text-align: center;
  color: #2c3e50;
  margin: 0 0 0.5rem;
}

.instructions {
  text-align: center;
  color: #7f8c8d;
  margin: 0 0 2rem;
}

.cart-section {
  background: #f8f9fa;
  padding: 1.5rem;
  border-radius: 6px;
  margin-bottom: 1.5rem;
}

.cart-section h3 {
  margin: 0 0 1rem;
  color: #2c3e50;
}

.items-list {
  margin-bottom: 1rem;
}

.list-title {
  font-weight: 600;
  color: #34495e;
  margin-bottom: 0.5rem;
  font-size: 0.9rem;
  text-transform: uppercase;
}

.cart-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0.75rem;
  margin-bottom: 0.5rem;
  border-radius: 4px;
  border-left: 4px solid;
}

.cart-item.borrow {
  background: #d4edda;
  border-left-color: #28a745;
}

.cart-item.return {
  background: #fff3cd;
  border-left-color: #ffc107;
}

.item-name {
  font-weight: 500;
  color: #2c3e50;
}

.remove-btn {
  padding: 0.4rem 0.8rem;
  background: #e74c3c;
  color: white;
  border: none;
  border-radius: 3px;
  cursor: pointer;
  font-size: 0.8rem;
  transition: background 0.3s;
}

.remove-btn:hover {
  background: #c0392b;
}

.empty-cart {
  text-align: center;
  color: #95a5a6;
  padding: 2rem 0;
}

.action-buttons {
  display: grid;
  grid-template-columns: 1fr;
  gap: 0.75rem;
  margin-bottom: 1rem;
}

.btn {
  padding: 0.75rem 1rem;
  border: none;
  border-radius: 4px;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s;
  text-align: center;
  text-decoration: none;
}

.btn-primary {
  background: #27ae60;
  color: white;
}

.btn-primary:hover:not(:disabled) {
  background: #229954;
}

.btn-secondary {
  background: #95a5a6;
  color: white;
}

.btn-secondary:hover {
  background: #7f8c8d;
}

.btn-info {
  background: #3498db;
  color: white;
}

.btn-info:hover {
  background: #2980b9;
}

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.message {
  padding: 1rem;
  border-radius: 4px;
  text-align: center;
  font-weight: 500;
}

.message.success {
  background: #d4edda;
  color: #155724;
  border: 1px solid #c3e6cb;
}

.message.error {
  background: #f8d7da;
  color: #721c24;
  border: 1px solid #f5c6cb;
}
</style>
