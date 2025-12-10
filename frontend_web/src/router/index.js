import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '../stores/authStore'

const routes = [
  {
    path: '/',
    redirect: (to) => {
      const authStore = useAuthStore()
      return authStore.user?.role === 'Admin' ? '/dashboard' : '/kiosk'
    }
  },
  {
    path: '/login',
    name: 'Login',
    component: () => import('../views/Login.vue'),
    meta: { requiresAuth: false }
  },
  {
    path: '/kiosk',
    name: 'Kiosk',
    component: () => import('../views/Kiosk.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/kiosk/history',
    name: 'UserHistory',
    component: () => import('../views/UserHistory.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/dashboard',
    name: 'Dashboard',
    component: () => import('../views/AdminDashboard.vue'),
    meta: { requiresAuth: true, requiresAdmin: true }
  },
  {
    path: '/admin/transactions',
    name: 'TransactionHistory',
    component: () => import('../views/AdminTransactionHistory.vue'),
    meta: { requiresAuth: true, requiresAdmin: true }
  },
  {
    path: '/admin/users',
    name: 'UserManagement',
    component: () => import('../views/AdminUserManagement.vue'),
    meta: { requiresAuth: true, requiresAdmin: true }
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

router.beforeEach((to, from, next) => {
  const authStore = useAuthStore()
  
  console.log('Router guard - navigating to:', to.path)
  console.log('User:', authStore.user)
  console.log('User role:', authStore.user?.role)

  // Check authentication
  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    next('/login')
  } 
  // Check admin access - only block non-admins from admin routes
  else if (to.meta.requiresAdmin && authStore.user?.role !== 'Admin') {
    console.log('Blocking access to admin route, redirecting to kiosk')
    next('/kiosk')
  } 
  // Redirect logged-in users away from login page
  else if (to.path === '/login' && authStore.isAuthenticated) {
    if (authStore.user?.role === 'Admin') {
      next('/dashboard')
    } else {
      next('/kiosk')
    }
  } 
  // Allow navigation (admins can access all routes, users can access their routes)
  else {
    next()
  }
})

export default router
