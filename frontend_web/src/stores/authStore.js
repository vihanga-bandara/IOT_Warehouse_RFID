import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import api from '../services/api'

export const useAuthStore = defineStore('auth', () => {
  const storedUser = JSON.parse(localStorage.getItem('user') || 'null')
  
  // Validate stored user has role field, otherwise clear it
  const user = ref(storedUser && storedUser.role ? storedUser : null)
  const token = ref(user.value ? localStorage.getItem('authToken') : null)
  
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
      localStorage.setItem('authToken', token.value)
      localStorage.setItem('user', JSON.stringify(user.value))
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
    user.value = null
    token.value = null
    localStorage.removeItem('authToken')
    localStorage.removeItem('user')
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
    isAuthenticated,
    login,
    register,
    logout,
    initializeAuth
  }
})
