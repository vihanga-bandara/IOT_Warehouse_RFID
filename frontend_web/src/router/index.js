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

  // Check authentication
  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    next('/login')
  } 
  // Check admin access - only block non-admins from admin routes
  else if (to.meta.requiresAdmin && authStore.user?.role !== 'Admin') {
    console.log('Blocking access to admin route, redirecting to kiosk')
    next('/kiosk')
  }
  // Admins must select a scanner before accessing kiosk routes
  else if ((to.path === '/kiosk' || to.path === '/kiosk/history') && authStore.user?.role === 'Admin' && !authStore.scannerDeviceId) {
    // Admins without a bound scanner cannot use kiosk;
    // they should log out and log in again with a scanner name.
    next({ path: '/dashboard', query: { kioskBlocked: '1' } })
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
