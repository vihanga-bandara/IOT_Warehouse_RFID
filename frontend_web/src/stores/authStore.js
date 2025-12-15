import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import api from '../services/api'
import { useCartStore } from './cartStore'

export const useAuthStore = defineStore('auth', () => {
  const storedUser = JSON.parse(localStorage.getItem('user') || 'null')
  
  // Validate stored user has role field, otherwise clear it
  const user = ref(storedUser && storedUser.role ? storedUser : null)
  const token = ref(user.value ? localStorage.getItem('authToken') : null)
  const scannerDeviceId = ref(localStorage.getItem('scannerDeviceId') || null)
  const scannerName = ref(localStorage.getItem('scannerName') || null)
  
  // If user data is invalid, clear everything
  if (!user.value && localStorage.getItem('authToken')) {
    localStorage.removeItem('authToken')
    localStorage.removeItem('user')
  }

  const isAuthenticated = computed(() => !!token.value && !!user.value)

  const login = async (email, password, scannerNameInput) => {
    try {
      const response = await api.post('/auth/login', { email, password, scannerName: scannerNameInput })
      token.value = response.data.token
      // Map RoleIds to role name: if user has Admin role (1), they're Admin
      const hasAdminRole = response.data.roleIds && response.data.roleIds.includes(1)
      user.value = {
        email: response.data.email,
        name: response.data.name,
        lastname: response.data.lastname,
        userId: response.data.userId,
        roleIds: response.data.roleIds,
        role: hasAdminRole ? 'Admin' : 'User'
      }
      scannerDeviceId.value = response.data.scannerDeviceId || null
      scannerName.value = response.data.scannerName || null
      localStorage.setItem('authToken', token.value)
      localStorage.setItem('user', JSON.stringify(user.value))
      if (scannerDeviceId.value) {
        localStorage.setItem('scannerDeviceId', scannerDeviceId.value)
      } else {
        localStorage.removeItem('scannerDeviceId')
      }
      if (scannerName.value) {
        localStorage.setItem('scannerName', scannerName.value)
      } else {
        localStorage.removeItem('scannerName')
      }
      api.defaults.headers.common['Authorization'] = `Bearer ${token.value}`
      return response.data
    } catch (error) {
      console.error('Login failed:', error)
      throw error
    }
  }

  const register = async (email, password, name, lastname) => {
    try {
      const response = await api.post('/auth/register', {
        email,
        password,
        name,
        lastname
      })
      return response.data
    } catch (error) {
      console.error('Registration failed:', error)
      throw error
    }
  }

  const logout = () => {
    // Best-effort: clear backend session cart before dropping auth.
    // (If this fails, we still proceed with client logout.)
    const currentToken = token.value || localStorage.getItem('authToken')
    if (currentToken) {
      api.post('/session/clear', null, {
        headers: { Authorization: `Bearer ${currentToken}` }
      }).catch(() => {})
    }

    useCartStore().clear()
    user.value = null
    token.value = null
    scannerDeviceId.value = null
    scannerName.value = null
    localStorage.removeItem('authToken')
    localStorage.removeItem('user')
    localStorage.removeItem('scannerDeviceId')
    localStorage.removeItem('scannerName')
    delete api.defaults.headers.common['Authorization']
  }

  const initializeAuth = () => {
    if (token.value) {
      api.defaults.headers.common['Authorization'] = `Bearer ${token.value}`
    }
  }

  return {
    user,
    token,
    scannerDeviceId,
    scannerName,
    isAuthenticated,
    login,
    register,
    logout,
    initializeAuth
  }
})
