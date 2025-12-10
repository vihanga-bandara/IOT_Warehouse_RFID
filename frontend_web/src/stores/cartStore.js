import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export const useCartStore = defineStore('cart', () => {
  const items = ref([])

  const itemCount = computed(() => items.value.length)

  const borrowItems = computed(() => 
    items.value.filter(item => item.action === 'Borrow')
  )

  const returnItems = computed(() =>
    items.value.filter(item => item.action === 'Checkin')
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

  const updateFromServer = (serverItems) => {
    items.value = serverItems
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
