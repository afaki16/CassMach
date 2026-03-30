<template>
  <div class="admin-ai">
    <!-- Header -->
    <div class="admin-header">
      <div>
        <h1 class="admin-title">AI Kullanım Yönetimi</h1>
        <p class="admin-subtitle">Kullanıcı sorguları, kredi harcamaları ve çözüm istatistikleri</p>
      </div>
    </div>

    <!-- Dashboard Stats -->
    <div class="stats-grid">
      <div class="stat-card">
        <div class="stat-icon stat-icon--users">
          <v-icon size="22" color="white">mdi-account-group</v-icon>
        </div>
        <div class="stat-info">
          <span class="stat-value">{{ dashboard.totalUsers }}</span>
          <span class="stat-label">Toplam Kullanıcı</span>
        </div>
      </div>
      <div class="stat-card">
        <div class="stat-icon stat-icon--questions">
          <v-icon size="22" color="white">mdi-chat-question</v-icon>
        </div>
        <div class="stat-info">
          <span class="stat-value">{{ dashboard.totalQuestions }}</span>
          <span class="stat-label">Toplam Sorgu</span>
        </div>
      </div>
      <div class="stat-card">
        <div class="stat-icon stat-icon--accepted">
          <v-icon size="22" color="white">mdi-check-circle</v-icon>
        </div>
        <div class="stat-info">
          <span class="stat-value">{{ dashboard.totalAcceptedSolutions }}</span>
          <span class="stat-label">Kabul Edilen</span>
        </div>
      </div>
      <div class="stat-card">
        <div class="stat-icon stat-icon--credits">
          <v-icon size="22" color="white">mdi-circle-multiple</v-icon>
        </div>
        <div class="stat-info">
          <span class="stat-value">{{ dashboard.totalCreditsUsed.toFixed(1) }}</span>
          <span class="stat-label">Toplam Kredi Kullanımı</span>
        </div>
      </div>
      <div class="stat-card">
        <div class="stat-icon stat-icon--cached">
          <v-icon size="22" color="white">mdi-cached</v-icon>
        </div>
        <div class="stat-info">
          <span class="stat-value">{{ dashboard.totalCachedResponses }}</span>
          <span class="stat-label">Önbellek Yanıtı</span>
        </div>
      </div>
      <div class="stat-card">
        <div class="stat-icon stat-icon--tokens">
          <v-icon size="22" color="white">mdi-chip</v-icon>
        </div>
        <div class="stat-info">
          <span class="stat-value">{{ formatNumber(dashboard.totalTokensUsed) }}</span>
          <span class="stat-label">Toplam Token</span>
        </div>
      </div>
    </div>

    <!-- Users Table -->
    <div class="users-section">
      <div class="section-header">
        <h2 class="section-title">Kullanıcı Bazlı Kullanım</h2>
        <div class="section-actions">
          <div class="search-wrap">
            <v-icon size="16" class="search-icon">mdi-magnify</v-icon>
            <input
              v-model="searchTerm"
              type="text"
              class="search-input"
              placeholder="Kullanıcı ara..."
              @input="debouncedFetch"
            />
            <button v-if="searchTerm" class="search-clear" @click="searchTerm = ''; fetchUsers()">
              <v-icon size="14">mdi-close</v-icon>
            </button>
          </div>
        </div>
      </div>

      <!-- Loading -->
      <div v-if="usersLoading" class="table-loading">
        <v-progress-circular indeterminate size="32" width="3" color="grey" />
      </div>

      <!-- Table -->
      <div v-else class="table-container">
        <table class="data-table">
          <thead>
            <tr>
              <th>Kullanıcı</th>
              <th>E-posta</th>
              <th class="text-right">Bakiye</th>
              <th class="text-right">Harcanan</th>
              <th class="text-right">Token</th>
              <th class="text-center">İşlemler</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="users.length === 0">
              <td colspan="6" class="empty-row">Kullanıcı bulunamadı</td>
            </tr>
            <tr v-for="user in users" :key="user.id" class="table-row">
              <td class="user-name">{{ user.fullName }}</td>
              <td class="user-email">{{ user.email }}</td>
              <td class="text-right">
                <span class="balance-badge" :class="{ 'balance-badge--low': user.tokenBalance < 10 }">
                  {{ user.tokenBalance.toFixed(1) }}
                </span>
              </td>
              <td class="text-right credit-used">{{ user.totalCreditsUsed.toFixed(1) }}</td>
              <td class="text-right token-count">{{ formatNumber(user.totalRawTokensUsed) }}</td>
              <td class="text-center">
                <button class="icon-btn" title="Detay" @click="openUserDetail(user.id)">
                  <v-icon size="18">mdi-eye-outline</v-icon>
                </button>
                <button class="icon-btn" title="Kredi Yükle" @click="openTopUp(user)">
                  <v-icon size="18">mdi-plus-circle-outline</v-icon>
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Pagination -->
      <div v-if="totalPages > 1" class="table-pagination">
        <button class="page-btn" :disabled="page <= 1" @click="page--; fetchUsers()">
          <v-icon size="18">mdi-chevron-left</v-icon>
        </button>
        <span class="page-info">{{ page }} / {{ totalPages }}</span>
        <button class="page-btn" :disabled="page >= totalPages" @click="page++; fetchUsers()">
          <v-icon size="18">mdi-chevron-right</v-icon>
        </button>
      </div>
    </div>

    <!-- User Detail Dialog -->
    <v-dialog v-model="detailDialog" max-width="700" scrollable>
      <v-card class="detail-card" rounded="xl">
        <v-card-title class="detail-title">
          <v-icon class="mr-2">mdi-account-details</v-icon>
          {{ userDetail?.fullName }}
          <v-spacer />
          <v-btn icon size="small" variant="text" @click="detailDialog = false">
            <v-icon>mdi-close</v-icon>
          </v-btn>
        </v-card-title>
        <v-card-text v-if="detailLoading" class="text-center py-8">
          <v-progress-circular indeterminate />
        </v-card-text>
        <v-card-text v-else-if="userDetail" class="detail-body">
          <div class="detail-stats">
            <div class="detail-stat">
              <span class="detail-stat-val">{{ userDetail.currentBalance.toFixed(1) }}</span>
              <span class="detail-stat-lbl">Bakiye</span>
            </div>
            <div class="detail-stat">
              <span class="detail-stat-val">{{ userDetail.totalCreditsUsed.toFixed(1) }}</span>
              <span class="detail-stat-lbl">Harcanan</span>
            </div>
            <div class="detail-stat">
              <span class="detail-stat-val">{{ userDetail.totalQuestions }}</span>
              <span class="detail-stat-lbl">Sorgu</span>
            </div>
            <div class="detail-stat">
              <span class="detail-stat-val">{{ userDetail.totalAcceptedSolutions }}</span>
              <span class="detail-stat-lbl">Kabul</span>
            </div>
          </div>

          <h4 class="tx-title">Son İşlemler</h4>
          <div v-if="userDetail.recentTransactions?.length" class="tx-list">
            <div v-for="(tx, i) in userDetail.recentTransactions" :key="i" class="tx-item">
              <div class="tx-left">
                <v-icon size="16" :color="tx.creditAmount < 0 ? 'error' : 'success'">
                  {{ tx.creditAmount < 0 ? 'mdi-arrow-down' : 'mdi-arrow-up' }}
                </v-icon>
                <div>
                  <span class="tx-desc">{{ tx.description }}</span>
                  <span class="tx-date">{{ formatDate(tx.createdDate) }}</span>
                </div>
              </div>
              <div class="tx-right">
                <span :class="tx.creditAmount < 0 ? 'tx-neg' : 'tx-pos'">
                  {{ tx.creditAmount < 0 ? '' : '+' }}{{ tx.creditAmount.toFixed(2) }}
                </span>
                <span class="tx-after">Bakiye: {{ tx.balanceAfter.toFixed(1) }}</span>
              </div>
            </div>
          </div>
          <div v-else class="tx-empty">Henüz işlem yok</div>
        </v-card-text>
      </v-card>
    </v-dialog>

    <!-- Top-Up Dialog -->
    <v-dialog v-model="topUpDialog" max-width="420">
      <v-card rounded="xl">
        <v-card-title>
          <v-icon class="mr-2">mdi-plus-circle</v-icon>
          Kredi Yükle — {{ topUpUser?.fullName }}
        </v-card-title>
        <v-card-text>
          <v-text-field
            v-model.number="topUpAmount"
            label="Kredi Miktarı"
            type="number"
            min="1"
            variant="outlined"
            density="compact"
            class="mb-2"
          />
          <v-text-field
            v-model="topUpDescription"
            label="Açıklama (opsiyonel)"
            variant="outlined"
            density="compact"
          />
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn variant="text" @click="topUpDialog = false">İptal</v-btn>
          <v-btn color="primary" variant="elevated" :loading="topUpLoading" @click="submitTopUp">
            Yükle
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Snackbar -->
    <v-snackbar v-model="showSnackbar" :color="snackbarColor" :timeout="3000" location="top">
      {{ snackbarText }}
    </v-snackbar>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { API_ENDPOINTS } from '~/utils/apiEndpoints'

definePageMeta({
  middleware: ['auth', 'permission'],
  permission: 'AdminPanel.Read'
})

const { get, post } = useApi()

interface DashboardData {
  totalUsers: number
  totalQuestions: number
  totalAcceptedSolutions: number
  totalCachedResponses: number
  totalTokensUsed: number
  totalCreditsUsed: number
}

interface AdminUser {
  id: number
  fullName: string
  email: string
  tokenBalance: number
  totalCreditsUsed: number
  totalRawTokensUsed: number
}

interface UserUsageDetail {
  userId: number
  fullName: string
  email: string
  currentBalance: number
  totalRawTokensUsed: number
  totalCreditsUsed: number
  totalQuestions: number
  totalAcceptedSolutions: number
  recentTransactions: Array<{
    transactionType: string
    rawTokens: number
    creditAmount: number
    balanceAfter: number
    description: string
    createdDate: string
  }>
}

const dashboard = ref<DashboardData>({
  totalUsers: 0, totalQuestions: 0, totalAcceptedSolutions: 0,
  totalCachedResponses: 0, totalTokensUsed: 0, totalCreditsUsed: 0
})

const users = ref<AdminUser[]>([])
const page = ref(1)
const totalPages = ref(0)
const searchTerm = ref('')
const usersLoading = ref(false)

const detailDialog = ref(false)
const detailLoading = ref(false)
const userDetail = ref<UserUsageDetail | null>(null)

const topUpDialog = ref(false)
const topUpUser = ref<AdminUser | null>(null)
const topUpAmount = ref(100)
const topUpDescription = ref('')
const topUpLoading = ref(false)

const showSnackbar = ref(false)
const snackbarText = ref('')
const snackbarColor = ref('success')

let debounceTimer: ReturnType<typeof setTimeout> | null = null
const debouncedFetch = () => {
  if (debounceTimer) clearTimeout(debounceTimer)
  debounceTimer = setTimeout(() => {
    page.value = 1
    fetchUsers()
  }, 400)
}

const fetchDashboard = async () => {
  try {
    const res = await get<DashboardData>(API_ENDPOINTS.ADMIN_AI.DASHBOARD)
    if (res.data) dashboard.value = res.data
  } catch { /* silent */ }
}

const fetchUsers = async () => {
  usersLoading.value = true
  try {
    const params = new URLSearchParams({ page: page.value.toString(), pageSize: '10' })
    if (searchTerm.value.trim()) params.append('searchTerm', searchTerm.value.trim())
    const res = await get<any>(`${API_ENDPOINTS.ADMIN_AI.USERS}?${params.toString()}`)
    users.value = res.data?.items ?? []
    totalPages.value = res.data?.totalPages ?? 0
  } catch {
    users.value = []
  } finally {
    usersLoading.value = false
  }
}

const openUserDetail = async (userId: number) => {
  detailDialog.value = true
  detailLoading.value = true
  userDetail.value = null
  try {
    const res = await get<UserUsageDetail>(API_ENDPOINTS.ADMIN_AI.USER_USAGE(userId))
    userDetail.value = res.data ?? null
  } catch {
    userDetail.value = null
  } finally {
    detailLoading.value = false
  }
}

const openTopUp = (user: AdminUser) => {
  topUpUser.value = user
  topUpAmount.value = 100
  topUpDescription.value = ''
  topUpDialog.value = true
}

const submitTopUp = async () => {
  if (!topUpUser.value || topUpAmount.value <= 0) return
  topUpLoading.value = true
  try {
    await post(API_ENDPOINTS.ADMIN_AI.TOPUP(topUpUser.value.id), {
      creditAmount: topUpAmount.value,
      description: topUpDescription.value || `Admin tarafından ${topUpAmount.value} kredi yüklendi`
    })
    snackbarText.value = `${topUpUser.value.fullName} hesabına ${topUpAmount.value} kredi yüklendi`
    snackbarColor.value = 'success'
    showSnackbar.value = true
    topUpDialog.value = false
    fetchUsers()
    fetchDashboard()
  } catch {
    snackbarText.value = 'Kredi yükleme başarısız oldu'
    snackbarColor.value = 'error'
    showSnackbar.value = true
  } finally {
    topUpLoading.value = false
  }
}

const formatNumber = (n: number) => {
  if (n >= 1000000) return (n / 1000000).toFixed(1) + 'M'
  if (n >= 1000) return (n / 1000).toFixed(1) + 'K'
  return n.toString()
}

const formatDate = (dateStr: string) => {
  const d = new Date(dateStr)
  return d.toLocaleDateString('tr-TR', { day: 'numeric', month: 'short', year: 'numeric', hour: '2-digit', minute: '2-digit' })
}

onMounted(() => {
  fetchDashboard()
  fetchUsers()
})

useHead({ title: 'AI Kullanım Yönetimi - CassMach' })
</script>

<style scoped>
.admin-ai {
  padding: 32px 40px;
  max-width: 1200px;
  margin: 0 auto;
}

.admin-header { margin-bottom: 28px; }

.admin-title {
  font-size: 1.5rem;
  font-weight: 800;
  color: #0f172a;
  margin: 0;
}

.admin-subtitle {
  font-size: 0.9rem;
  color: #64748b;
  margin-top: 4px;
}

/* Stats Grid */
.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(170px, 1fr));
  gap: 14px;
  margin-bottom: 32px;
}

.stat-card {
  display: flex;
  align-items: center;
  gap: 14px;
  padding: 18px 20px;
  background: white;
  border: 1px solid #e2e8f0;
  border-radius: 16px;
  box-shadow: 0 2px 8px rgba(15, 23, 42, 0.04);
}

.stat-icon {
  width: 42px;
  height: 42px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.stat-icon--users { background: linear-gradient(135deg, #3b82f6, #2563eb); }
.stat-icon--questions { background: linear-gradient(135deg, #8b5cf6, #7c3aed); }
.stat-icon--accepted { background: linear-gradient(135deg, #10b981, #059669); }
.stat-icon--credits { background: linear-gradient(135deg, #f59e0b, #d97706); }
.stat-icon--cached { background: linear-gradient(135deg, #06b6d4, #0891b2); }
.stat-icon--tokens { background: linear-gradient(135deg, #0f172a, #334155); }

.stat-info { display: flex; flex-direction: column; }

.stat-value {
  font-size: 1.3rem;
  font-weight: 800;
  color: #0f172a;
  line-height: 1.2;
}

.stat-label {
  font-size: 0.75rem;
  color: #94a3b8;
  font-weight: 500;
}

/* Users Section */
.users-section {
  background: white;
  border: 1px solid #e2e8f0;
  border-radius: 20px;
  padding: 24px;
  box-shadow: 0 4px 20px rgba(15, 23, 42, 0.04);
}

.section-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  margin-bottom: 18px;
  flex-wrap: wrap;
}

.section-title {
  font-size: 1.05rem;
  font-weight: 700;
  color: #0f172a;
  margin: 0;
}

.search-wrap {
  display: flex;
  align-items: center;
  gap: 6px;
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 10px;
  padding: 7px 10px;
  min-width: 200px;
}

.search-wrap:focus-within { border-color: #94a3b8; }
.search-icon { color: #94a3b8; flex-shrink: 0; }

.search-input {
  flex: 1;
  border: none;
  outline: none;
  background: transparent;
  font-size: 0.82rem;
  color: #334155;
  font-family: inherit;
}

.search-input::placeholder { color: #94a3b8; }

.search-clear {
  background: none;
  border: none;
  cursor: pointer;
  color: #94a3b8;
  display: flex;
  padding: 2px;
}

.table-loading {
  display: flex;
  justify-content: center;
  padding: 40px 0;
}

/* Table */
.table-container { overflow-x: auto; }

.data-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.88rem;
}

.data-table th {
  text-align: left;
  padding: 10px 14px;
  font-weight: 700;
  color: #64748b;
  font-size: 0.75rem;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  border-bottom: 2px solid #f1f5f9;
}

.data-table td {
  padding: 12px 14px;
  border-bottom: 1px solid #f8fafc;
  color: #334155;
}

.table-row:hover { background: #f8fafc; }

.user-name { font-weight: 600; color: #0f172a; }
.user-email { color: #64748b; font-size: 0.82rem; }

.balance-badge {
  display: inline-block;
  padding: 2px 10px;
  border-radius: 8px;
  font-weight: 700;
  font-size: 0.82rem;
  background: #f0fdf4;
  color: #166534;
}

.balance-badge--low { background: #fef2f2; color: #991b1b; }

.credit-used { font-weight: 600; color: #475569; }
.token-count { font-size: 0.8rem; color: #94a3b8; }

.text-right { text-align: right; }
.text-center { text-align: center; }

.empty-row {
  text-align: center;
  color: #94a3b8;
  padding: 32px !important;
}

.icon-btn {
  background: none;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  padding: 6px;
  cursor: pointer;
  color: #475569;
  margin: 0 2px;
  transition: all 0.15s;
}

.icon-btn:hover {
  background: #f1f5f9;
  border-color: #94a3b8;
}

/* Pagination */
.table-pagination {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 12px;
  margin-top: 16px;
  padding-top: 14px;
  border-top: 1px solid #f1f5f9;
}

.page-btn {
  width: 32px;
  height: 32px;
  border-radius: 8px;
  border: 1px solid #e2e8f0;
  background: white;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  color: #475569;
}

.page-btn:hover:not(:disabled) { background: #f1f5f9; }
.page-btn:disabled { opacity: 0.3; cursor: not-allowed; }

.page-info { font-size: 0.82rem; font-weight: 600; color: #64748b; }

/* Detail Dialog */
.detail-card { border-radius: 20px !important; }

.detail-title {
  font-size: 1rem;
  font-weight: 700;
  padding: 20px 24px 12px;
  border-bottom: 1px solid #f1f5f9;
}

.detail-body { padding: 20px 24px !important; }

.detail-stats {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 12px;
  margin-bottom: 24px;
}

.detail-stat {
  text-align: center;
  padding: 14px 8px;
  background: #f8fafc;
  border-radius: 12px;
}

.detail-stat-val {
  display: block;
  font-size: 1.2rem;
  font-weight: 800;
  color: #0f172a;
}

.detail-stat-lbl {
  font-size: 0.72rem;
  color: #94a3b8;
  font-weight: 500;
}

.tx-title {
  font-size: 0.9rem;
  font-weight: 700;
  color: #0f172a;
  margin-bottom: 12px;
}

.tx-list {
  display: flex;
  flex-direction: column;
  gap: 8px;
  max-height: 300px;
  overflow-y: auto;
}

.tx-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 10px 14px;
  background: #f8fafc;
  border-radius: 10px;
  gap: 12px;
}

.tx-left {
  display: flex;
  align-items: center;
  gap: 10px;
  min-width: 0;
}

.tx-desc {
  display: block;
  font-size: 0.82rem;
  font-weight: 500;
  color: #334155;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  max-width: 280px;
}

.tx-date {
  display: block;
  font-size: 0.7rem;
  color: #94a3b8;
}

.tx-right { text-align: right; flex-shrink: 0; }

.tx-neg { display: block; font-weight: 700; color: #dc2626; font-size: 0.88rem; }
.tx-pos { display: block; font-weight: 700; color: #16a34a; font-size: 0.88rem; }
.tx-after { font-size: 0.7rem; color: #94a3b8; }

.tx-empty {
  text-align: center;
  color: #94a3b8;
  padding: 20px;
  font-size: 0.85rem;
}

@media (max-width: 768px) {
  .admin-ai { padding: 20px 16px; }

  .stats-grid { grid-template-columns: repeat(2, 1fr); }

  .section-header { flex-direction: column; align-items: stretch; }

  .detail-stats { grid-template-columns: repeat(2, 1fr); }

  .data-table { font-size: 0.8rem; }
  .data-table th, .data-table td { padding: 8px 10px; }
}
</style>
