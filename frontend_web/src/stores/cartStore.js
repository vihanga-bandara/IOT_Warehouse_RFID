import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export const useCartStore = defineStore('cart', () => {
  const items = ref([])

  const itemCount = computed(() => items.value.length)

  const borrowItems = computed(() =>
    items.value.filter(item => ['Borrow', 'Checkout'].includes(item.action))
  )

  const returnItems = computed(() =>
    items.value.filter(item => ['Return', 'Checkin'].includes(item.action))
  )

  const addItem = (item) => {
    const exists = items.value.find(i => i.id === item.id)
    if (!exists) {
      items.value.push(item)
    }
  }

  const removeItem = (itemId) => {
    items.value = items.value.filter(i => i.id !== itemId)
  }

  const clear = () => {
    items.value = []
  }

  const updateFromServer = (payload) => {
    // Backend emits a SessionCartDto over SignalR:
    // { userId, sessionStarted, items: [{ itemId, itemName, action, rfidUid, scannedAt }, ...] }
    // Normalize it to the frontend item shape used by Kiosk.vue:
    // { id, name, action, rfidUid, scannedAt }

    const serverItems = Array.isArray(payload)
      ? payload
      : (payload && Array.isArray(payload.items) ? payload.items : [])

    items.value = serverItems
      .map((i) => {
        // Already-normalized items
        if (i && (i.id !== undefined || i.name !== undefined)) {
          return i
        }

        return {
          id: i?.itemId,
          name: i?.itemName,
          action: i?.action,
          rfidUid: i?.rfidUid,
          scannedAt: i?.scannedAt
        }
      })
      // Drop any malformed entries
      .filter((i) => i && i.id !== undefined)
  }

  return {
    items,
    itemCount,
    borrowItems,
    returnItems,
    addItem,
    removeItem,
    clear,
    updateFromServer
  }
})
