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

  const login = async (email, password) => {
    try {
      const response = await api.post('/auth/login', { email, password })
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
      // Scanner is no longer set at login time - it's done in a separate step
      scannerDeviceId.value = null
      scannerName.value = null
      localStorage.setItem('authToken', token.value)
      localStorage.setItem('user', JSON.stringify(user.value))
      localStorage.removeItem('scannerDeviceId')
      localStorage.removeItem('scannerName')
      api.defaults.headers.common['Authorization'] = `Bearer ${token.value}`
      return response.data
    } catch (error) {
      console.error('Login failed:', error)
      throw error
    }
  }

  const bindScanner = async (scannerNameInput) => {
    try {
      const response = await api.post('/auth/bind-scanner', { scannerName: scannerNameInput })
      scannerDeviceId.value = response.data.scannerDeviceId
      scannerName.value = response.data.scannerName
      localStorage.setItem('scannerDeviceId', response.data.scannerDeviceId)
      localStorage.setItem('scannerName', response.data.scannerName)
      return response.data
    } catch (error) {
      console.error('Failed to bind scanner:', error)
      throw error
    }
  }

  const setRfidLoginData = (data) => {
    // Used by RFID login flow where scanner is already known
    token.value = data.token
    const hasAdminRole = data.roleIds && data.roleIds.includes(1)
    user.value = {
      email: data.email,
      name: data.name,
      lastname: data.lastname,
      userId: data.userId,
      roleIds: data.roleIds,
      role: hasAdminRole ? 'Admin' : 'User'
    }
    scannerDeviceId.value = data.scannerDeviceId || null
    scannerName.value = data.scannerName || null

    localStorage.setItem('authToken', data.token)
    localStorage.setItem('user', JSON.stringify(user.value))
    if (data.scannerDeviceId) {
      localStorage.setItem('scannerDeviceId', data.scannerDeviceId)
    }
    if (data.scannerName) {
      localStorage.setItem('scannerName', data.scannerName)
    }
    api.defaults.headers.common['Authorization'] = `Bearer ${data.token}`
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
    bindScanner,
    setRfidLoginData,
    register,
    logout,
    initializeAuth
  }
})
