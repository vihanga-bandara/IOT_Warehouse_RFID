import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '../stores/authStore'

const routes = [
  {
    path: '/',
    redirect: (to) => {
      const authStore = useAuthStore()
      if (!authStore.isAuthenticated) {
        return '/login'
      }
      // If authenticated but no scanner selected
      if (!authStore.scannerDeviceId) {
        return '/select-scanner'
      }
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
    path: '/verify-pin',
    name: 'PinVerification',
    component: () => import('../views/PinVerification.vue'),
    meta: { requiresAuth: false, requiresPendingMfa: true }
  },
  {
    path: '/select-scanner',
    name: 'ScannerSelect',
    component: () => import('../views/ScannerSelect.vue'),
    meta: { requiresAuth: true, requiresScanner: false }
  },
  {
    path: '/kiosk',
    name: 'Kiosk',
    component: () => import('../views/Kiosk.vue'),
    meta: { requiresAuth: true, requiresScanner: true }
  },
  {
    path: '/kiosk/history',
    name: 'UserHistory',
    component: () => import('../views/UserHistory.vue'),
    meta: { requiresAuth: true, requiresScanner: true }
  },
  {
    path: '/dashboard',
    name: 'Dashboard',
    component: () => import('../views/AdminDashboard.vue'),
    meta: { requiresAuth: true, requiresAdmin: true }
  },
  {
    path: '/admin/items',
    name: 'Items',
    component: () => import('../views/AdminItems.vue'),
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
  },
  {
    path: '/admin/users/:id',
    name: 'UserDetail',
    component: () => import('../views/AdminUserDetail.vue'),
    meta: { requiresAuth: true, requiresAdmin: true }
  },
  {
    path: '/admin/scanners',
    name: 'Scanners',
    component: () => import('../views/AdminScanners.vue'),
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
  console.log('Scanner:', authStore.scannerDeviceId)
  console.log('Pending MFA:', !!authStore.pendingMfaData)

  // Handle PIN verification route
  if (to.meta.requiresPendingMfa) {
    // Only allow access if there's pending MFA data
    if (!authStore.pendingMfaData) {
      console.log('No pending MFA, redirecting to login')
      next('/login')
      return
    }
    // Allow access to PIN verification page
    next()
    return
  }

  // If user has pending MFA and tries to go elsewhere (except login), redirect to PIN verification
  if (authStore.pendingMfaData && to.path !== '/login' && to.path !== '/verify-pin') {
    console.log('Pending MFA exists, redirecting to PIN verification')
    next('/verify-pin')
    return
  }

  // Check authentication
  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    next('/login')
    return
  }
  
  // Check admin access - only block non-admins from admin routes
  if (to.meta.requiresAdmin && authStore.user?.role !== 'Admin') {
    console.log('Blocking access to admin route, redirecting to scanner select')
    next('/select-scanner')
    return
  }

  // Check scanner requirement for kiosk routes (non-admin users)
  if (to.meta.requiresScanner && !authStore.scannerDeviceId) {
    console.log('Scanner required but not set, redirecting to scanner select')
    next('/select-scanner')
    return
  }

  // Redirect logged-in users away from login page
  if (to.path === '/login' && authStore.isAuthenticated) {
    // If no scanner selected yet, go to scanner select
    if (!authStore.scannerDeviceId) {
      next('/select-scanner')
    } else if (authStore.user?.role === 'Admin') {
      next('/dashboard')
    } else {
      next('/kiosk')
    }
    return
  }
  
  // Allow navigation
  next()
})

export default router
