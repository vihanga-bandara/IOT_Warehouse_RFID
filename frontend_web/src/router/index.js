import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '../stores/authStore'

const routes = [
  {
    path: '/',
    redirect: '/dashboard'
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

  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    next('/login')
  } else if (to.meta.requiresAdmin && authStore.user?.role !== 'Admin') {
    next('/kiosk')
  } else if (to.path === '/login' && authStore.isAuthenticated) {
    next('/dashboard')
  } else {
    next()
  }
})

export default router
