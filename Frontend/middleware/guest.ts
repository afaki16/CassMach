export default defineNuxtRouteMiddleware(async () => {
  const authStore = useAuthStore()
  
  // Initialize auth state (await - cookie/localStorage senkronizasyonu için)
  if (!authStore.isAuthenticated) {
    await authStore.initializeAuth()
  }

  // If user is authenticated, redirect to error-assistant
  if (authStore.isAuthenticated) {
    return navigateTo('/error-assistant')
  }
}) 