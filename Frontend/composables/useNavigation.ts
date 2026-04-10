  import type { NavigationItem } from '~/types'

  export const navigationItems: NavigationItem[] = [
    {
      title: 'AI Assistant',
      icon: 'mdi-robot-happy-outline',
      children: [
        {
          title: 'Hata Asistanı',
          icon: 'mdi-chat-processing-outline',
          to: '/error-assistant',
          permission: 'Errors.Read'
        },
        {
          title: 'Geçmiş Sorgularım',
          icon: 'mdi-history',
          to: '/error-history',
          permission: 'Errors.Read'
        },
        {
          title: 'AI Kullanım Yönetimi',
          icon: 'mdi-chart-box-outline',
          to: '/admin-ai',
          permission: 'AdminPanel.Read'
        }
      ]
    },
    {
      title: 'User Management',
      icon: 'mdi-account-group',
      children: [
        {
          title: 'Users',
          icon: 'mdi-account-multiple',
          to: '/users',
          permission: 'Users.Read'
        },
        {
          title: 'Tenants',
          icon: 'mdi-domain',
          to: '/tenants',
          roles: ['SuperAdmin']
        },
        {
          title: 'Roles',
          icon: 'mdi-shield-account',
          to: '/roles',
          permission: 'Roles.Read'
        },
        {
          title: 'Permissions',
          icon: 'mdi-key',
          to: '/permissions',
          permission: 'Permissions.Read'
        }
      ]
    },
    
    {
      title: 'Settings',
      icon: 'mdi-cog',
      to: '/settings',
      roles: ['Admin', 'SuperAdmin']
    }
  ]

  export const filterNavigationByPermissions = (
    items: NavigationItem[],
    hasPermission: (permission: string) => boolean,
    hasRole: (role: string) => boolean
  ): NavigationItem[] => {
    return items
      .map((item): NavigationItem | null => {
        // Check permission requirement
        if (item.permission && !hasPermission(item.permission)) return null
        // Check role requirement
        if (item.roles && !item.roles.some(role => hasRole(role))) return null
        // If item has children, filter recursively and return new object (do not mutate original)
        if (item.children) {
          const filteredChildren = filterNavigationByPermissions(item.children, hasPermission, hasRole)
          if (filteredChildren.length === 0) return null
          return { ...item, children: filteredChildren }
        }
        return { ...item }
      })
      .filter((item): item is NavigationItem => item !== null)
  } 