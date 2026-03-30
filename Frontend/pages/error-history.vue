<template>
  <div class="history-page">
    <div class="history-page-bg">
      <div class="history-bg-orb history-bg-orb--1"></div>
      <div class="history-bg-orb history-bg-orb--2"></div>
    </div>

    <div class="history-page-content">
      <!-- Page Header -->
      <div class="page-header">
        <div class="page-header-left">
          <div class="page-header-icon">
            <v-icon size="22" color="white">mdi-history</v-icon>
          </div>
          <div>
            <h1 class="page-title">Geçmiş Sorgularım</h1>
            <p class="page-subtitle">Tüm AI asistan konuşmalarınız</p>
          </div>
        </div>
        <div class="page-header-right">
          <NuxtLink to="/error-assistant" class="header-action-btn">
            <v-icon size="16">mdi-plus</v-icon>
            Yeni Sorgu
          </NuxtLink>
        </div>
      </div>

      <!-- Search & Filters -->
      <div class="search-bar">
        <div class="search-input-wrap">
          <v-icon size="18" class="search-icon">mdi-magnify</v-icon>
          <input
            v-model="historySearch"
            type="text"
            class="search-input"
            placeholder="Marka, hata kodu veya soru ile ara..."
            @input="debouncedFetchHistory"
          />
          <button v-if="historySearch" class="search-clear" @click="historySearch = ''; fetchHistory()">
            <v-icon size="16">mdi-close</v-icon>
          </button>
        </div>
        <span v-if="historyTotal > 0" class="result-count">{{ historyTotal }} sonuç</span>
      </div>

      <!-- Loading -->
      <div v-if="historyLoading && historyItems.length === 0" class="state-empty">
        <v-progress-circular indeterminate size="32" width="2" color="grey" />
        <span>Yükleniyor...</span>
      </div>

      <!-- Empty -->
      <div v-else-if="historyItems.length === 0" class="state-empty">
        <v-icon size="48" color="grey-lighten-1">mdi-chat-remove-outline</v-icon>
        <span class="state-empty-title">{{ historySearch ? 'Sonuç bulunamadı' : 'Henüz sorgu geçmişiniz yok' }}</span>
        <p v-if="!historySearch" class="state-empty-subtitle">
          <NuxtLink to="/error-assistant">Hata Asistanı</NuxtLink>'ndan ilk sorunuzu sorun.
        </p>
      </div>

      <!-- History List -->
      <div v-else class="history-grid">
        <div
          v-for="item in historyItems"
          :key="item.id"
          class="history-card"
          :class="{ 'history-card--active': selectedConversationId === item.conversationId }"
          @click="selectConversation(item)"
        >
          <div class="card-top">
            <div class="card-badges">
              <span v-if="item.brand" class="card-brand">{{ item.brand }}</span>
              <span v-if="item.errorCode" class="card-code">{{ item.errorCode }}</span>
              <v-chip
                v-if="item.isAccepted === true"
                size="x-small"
                color="success"
                variant="tonal"
                prepend-icon="mdi-check-circle"
              >Kabul Edildi</v-chip>
              <v-chip
                v-else-if="item.fromCache"
                size="x-small"
                color="info"
                variant="tonal"
                prepend-icon="mdi-cached"
              >Önbellek</v-chip>
            </div>
            <span class="card-date">{{ formatDate(item.createdDate) }}</span>
          </div>

          <p class="card-question">{{ item.userQuestion }}</p>

          <div v-if="item.aiResponse" class="card-preview">
            {{ truncateText(item.aiResponse, 120) }}
          </div>

          <div class="card-footer">
            <span class="card-attempt">
              <v-icon size="12">mdi-refresh</v-icon>
              {{ item.attemptNumber }} deneme
            </span>
            <span class="card-credits">{{ item.creditsCharged.toFixed(1) }} kredi</span>
          </div>
        </div>
      </div>

      <!-- Pagination -->
      <div v-if="historyTotalPages > 1" class="pagination">
        <button
          class="pagination-btn"
          :disabled="historyPage <= 1"
          @click="historyPage--; fetchHistory()"
        >
          <v-icon size="18">mdi-chevron-left</v-icon>
        </button>
        <span class="pagination-info">{{ historyPage }} / {{ historyTotalPages }}</span>
        <button
          class="pagination-btn"
          :disabled="historyPage >= historyTotalPages"
          @click="historyPage++; fetchHistory()"
        >
          <v-icon size="18">mdi-chevron-right</v-icon>
        </button>
      </div>
    </div>

    <!-- Conversation Detail Dialog -->
    <v-dialog v-model="showConversation" max-width="800" scrollable>
      <v-card class="conversation-dialog" rounded="xl">
        <v-card-title class="dialog-title">
          <div class="dialog-title-left">
            <div class="dialog-icon">
              <v-icon size="20" color="white">mdi-chat-processing-outline</v-icon>
            </div>
            <div>
              <span class="dialog-brand">{{ conversationMeta.brand }}</span>
              <span v-if="conversationMeta.errorCode" class="dialog-code">{{ conversationMeta.errorCode }}</span>
            </div>
          </div>
          <v-btn icon="mdi-close" variant="text" size="small" @click="showConversation = false" />
        </v-card-title>

        <v-divider />

        <v-card-text class="dialog-body">
          <div v-if="conversationLoading" class="dialog-loading">
            <v-progress-circular indeterminate size="28" width="2" />
          </div>
          <div v-else class="conversation-messages">
            <div
              v-for="(msg, i) in conversationMessages"
              :key="i"
              class="conv-msg"
              :class="{
                'conv-msg--user': msg.role === 'user',
                'conv-msg--assistant': msg.role === 'assistant',
                'conv-msg--divider': msg.role === 'divider'
              }"
            >
              <div v-if="msg.role === 'divider'" class="conv-divider">
                <div class="conv-divider-line"></div>
                <span class="conv-divider-text">
                  <v-icon size="14">mdi-refresh</v-icon>
                  Deneme #{{ msg.attempt }}
                </span>
                <div class="conv-divider-line"></div>
              </div>

              <template v-else>
                <div v-if="msg.role === 'assistant'" class="conv-avatar">
                  <v-icon size="18" color="white">mdi-robot-happy-outline</v-icon>
                </div>
                <div class="conv-bubble" :class="{ 'conv-bubble--user': msg.role === 'user' }">
                  <div class="conv-text" v-html="msg.role === 'assistant' ? formatMessage(msg.content) : msg.content"></div>
                </div>
              </template>
            </div>
          </div>
        </v-card-text>
      </v-card>
    </v-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useAuthStore } from '~/stores/auth'
import { API_ENDPOINTS } from '~/utils/apiEndpoints'

definePageMeta({
  middleware: ['auth', 'permission'],
  permission: 'Errors.Read'
})

const authStore = useAuthStore()
const { get } = useApi()

interface HistoryItem {
  id: number
  conversationId: string
  brand: string
  model: string
  errorCode: string
  userQuestion: string
  aiResponse: string
  attemptNumber: number
  isAccepted: boolean | null
  fromCache: boolean
  creditsCharged: number
  createdDate: string
}

interface ConversationMessage {
  role: 'user' | 'assistant' | 'divider'
  content: string
  attempt?: number
}

const historyItems = ref<HistoryItem[]>([])
const historyPage = ref(1)
const historyTotalPages = ref(0)
const historyTotal = ref(0)
const historySearch = ref('')
const historyLoading = ref(false)

const showConversation = ref(false)
const conversationLoading = ref(false)
const conversationMessages = ref<ConversationMessage[]>([])
const conversationMeta = ref({ brand: '', errorCode: '', model: '' })
const selectedConversationId = ref<string | null>(null)

let debounceTimer: ReturnType<typeof setTimeout> | null = null
const debouncedFetchHistory = () => {
  if (debounceTimer) clearTimeout(debounceTimer)
  debounceTimer = setTimeout(() => {
    historyPage.value = 1
    fetchHistory()
  }, 400)
}

const fetchHistory = async () => {
  historyLoading.value = true
  try {
    const params = new URLSearchParams({
      page: historyPage.value.toString(),
      pageSize: '12'
    })
    if (historySearch.value.trim()) {
      params.append('searchTerm', historySearch.value.trim())
    }
    const res = await get<any>(`${API_ENDPOINTS.ERRORS.HISTORY}?${params.toString()}`)
    historyItems.value = res.data?.items ?? []
    historyTotalPages.value = res.data?.totalPages ?? 0
    historyTotal.value = res.data?.totalCount ?? 0
  } catch {
    historyItems.value = []
  } finally {
    historyLoading.value = false
  }
}

const selectConversation = async (item: HistoryItem) => {
  selectedConversationId.value = item.conversationId
  conversationMeta.value = { brand: item.brand, errorCode: item.errorCode, model: item.model }
  showConversation.value = true
  conversationLoading.value = true
  conversationMessages.value = []

  try {
    const res = await get<any>(API_ENDPOINTS.ERRORS.CONVERSATION(item.conversationId))
    const attempts = res.data ?? []
    if (attempts.length === 0) return

    const msgs: ConversationMessage[] = []
    const first = attempts[0]

    msgs.push({ role: 'user', content: first.userQuestion })

    for (const attempt of attempts) {
      if (attempt.attemptNumber > 1) {
        msgs.push({ role: 'divider', content: '', attempt: attempt.attemptNumber })
      }
      msgs.push({ role: 'assistant', content: attempt.aiResponse || '' })
    }

    conversationMessages.value = msgs
  } catch {
    conversationMessages.value = [{ role: 'assistant', content: 'Konuşma yüklenirken hata oluştu.' }]
  } finally {
    conversationLoading.value = false
  }
}

const truncateText = (text: string, maxLen: number) => {
  if (!text) return ''
  const clean = text.replace(/[#*`\-_]/g, '').replace(/\n+/g, ' ').trim()
  return clean.length > maxLen ? clean.substring(0, maxLen) + '...' : clean
}

const formatDate = (dateStr: string) => {
  const d = new Date(dateStr)
  const now = new Date()
  const diffMs = now.getTime() - d.getTime()
  const diffMin = Math.floor(diffMs / 60000)
  if (diffMin < 1) return 'Az önce'
  if (diffMin < 60) return `${diffMin} dk önce`
  const diffHours = Math.floor(diffMin / 60)
  if (diffHours < 24) return `${diffHours} saat önce`
  const diffDays = Math.floor(diffHours / 24)
  if (diffDays < 7) return `${diffDays} gün önce`
  return d.toLocaleDateString('tr-TR', { day: 'numeric', month: 'short', year: 'numeric' })
}

const formatMessage = (text: string) => {
  if (!text) return ''
  const lines = text.split('\n')
  const html: string[] = []
  let inList = false

  for (const rawLine of lines) {
    const line = rawLine
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>')
      .replace(/`([^`]+)`/g, '<code>$1</code>')

    if (/^#{4,}\s+(.+)/.test(line)) {
      if (inList) { html.push('</ul>'); inList = false }
      html.push(`<h5 class="md-h4">${line.replace(/^#{4,}\s+/, '')}</h5>`)
    } else if (/^###\s+(.+)/.test(line)) {
      if (inList) { html.push('</ul>'); inList = false }
      html.push(`<h4 class="md-h3">${line.replace(/^###\s+/, '')}</h4>`)
    } else if (/^##\s+(.+)/.test(line)) {
      if (inList) { html.push('</ul>'); inList = false }
      html.push(`<h3 class="md-h2">${line.replace(/^##\s+/, '')}</h3>`)
    } else if (/^#\s+(.+)/.test(line)) {
      if (inList) { html.push('</ul>'); inList = false }
      html.push(`<h2 class="md-h1">${line.replace(/^#\s+/, '')}</h2>`)
    } else if (/^\s*[-*]\s+(.+)/.test(line)) {
      if (!inList) { html.push('<ul class="md-list">'); inList = true }
      html.push(`<li>${line.replace(/^\s*[-*]\s+/, '')}</li>`)
    } else if (/^\s*\d+\.\s+(.+)/.test(line)) {
      if (inList) { html.push('</ul>'); inList = false }
      html.push(`<div class="md-step">${line}</div>`)
    } else if (line.trim() === '') {
      if (inList) { html.push('</ul>'); inList = false }
      html.push('<br>')
    } else {
      if (inList) { html.push('</ul>'); inList = false }
      html.push(`<p class="md-p">${line}</p>`)
    }
  }
  if (inList) html.push('</ul>')
  return html.join('')
}

onMounted(() => {
  fetchHistory()
})

useHead({ title: 'Geçmiş Sorgularım - CassMach' })
</script>

<style scoped>
.history-page {
  position: relative;
  min-height: 100vh;
  background: #f1f5f9;
  padding: 32px 40px 60px;
}

.history-page-bg {
  position: fixed;
  inset: 0;
  pointer-events: none;
  z-index: 0;
  overflow: hidden;
}

.history-bg-orb {
  position: absolute;
  border-radius: 50%;
  filter: blur(120px);
  opacity: 0.1;
}

.history-bg-orb--1 {
  width: 400px;
  height: 400px;
  background: #334155;
  top: -100px;
  right: -80px;
}

.history-bg-orb--2 {
  width: 350px;
  height: 350px;
  background: #0f172a;
  bottom: -60px;
  left: -40px;
}

.history-page-content {
  position: relative;
  z-index: 1;
  max-width: 1000px;
  margin: 0 auto;
}

/* Page Header */
.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 24px;
  gap: 16px;
  flex-wrap: wrap;
}

.page-header-left {
  display: flex;
  align-items: center;
  gap: 14px;
}

.page-header-icon {
  width: 44px;
  height: 44px;
  border-radius: 14px;
  background: linear-gradient(180deg, #0f172a 0%, #334155 100%);
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 4px 16px rgba(15, 23, 42, 0.2);
}

.page-title {
  font-size: 1.4rem;
  font-weight: 800;
  color: #0f172a;
  margin: 0;
  line-height: 1.2;
}

.page-subtitle {
  font-size: 0.85rem;
  color: #64748b;
  margin: 2px 0 0;
}

.header-action-btn {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 10px 18px;
  background: linear-gradient(180deg, #0f172a 0%, #334155 100%);
  border: none;
  border-radius: 12px;
  font-size: 0.82rem;
  font-weight: 600;
  color: white;
  cursor: pointer;
  font-family: inherit;
  text-decoration: none;
  transition: all 0.2s;
}

.header-action-btn:hover {
  box-shadow: 0 4px 16px rgba(15, 23, 42, 0.3);
  transform: translateY(-1px);
}

/* Search Bar */
.search-bar {
  display: flex;
  align-items: center;
  gap: 14px;
  margin-bottom: 20px;
}

.search-input-wrap {
  flex: 1;
  display: flex;
  align-items: center;
  gap: 8px;
  background: white;
  border: 1px solid #e2e8f0;
  border-radius: 12px;
  padding: 10px 14px;
  transition: border-color 0.2s, box-shadow 0.2s;
}

.search-input-wrap:focus-within {
  border-color: #334155;
  box-shadow: 0 2px 12px rgba(15, 23, 42, 0.08);
}

.search-icon { color: #94a3b8; flex-shrink: 0; }

.search-input {
  flex: 1;
  border: none;
  outline: none;
  background: transparent;
  font-size: 0.88rem;
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
  align-items: center;
  padding: 2px;
  transition: color 0.2s;
}

.search-clear:hover { color: #475569; }

.result-count {
  font-size: 0.8rem;
  font-weight: 600;
  color: #64748b;
  white-space: nowrap;
}

/* States */
.state-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 10px;
  padding: 60px 0;
  color: #94a3b8;
}

.state-empty-title {
  font-size: 0.95rem;
  font-weight: 600;
  color: #64748b;
}

.state-empty-subtitle {
  font-size: 0.85rem;
  color: #94a3b8;
  margin: 0;
}

.state-empty-subtitle a {
  color: #334155;
  font-weight: 600;
  text-decoration: none;
}

.state-empty-subtitle a:hover { text-decoration: underline; }

/* History Grid */
.history-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 12px;
}

.history-card {
  background: white;
  border: 1px solid #e2e8f0;
  border-radius: 14px;
  padding: 16px;
  cursor: pointer;
  transition: all 0.2s;
}

.history-card:hover {
  border-color: #cbd5e1;
  box-shadow: 0 4px 16px rgba(15, 23, 42, 0.06);
  transform: translateY(-1px);
}

.history-card--active {
  border-color: #334155;
  box-shadow: 0 4px 16px rgba(15, 23, 42, 0.12);
}

.card-top {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
  margin-bottom: 8px;
}

.card-badges {
  display: flex;
  align-items: center;
  gap: 6px;
  flex-wrap: wrap;
}

.card-brand {
  font-size: 0.75rem;
  font-weight: 700;
  color: #0f172a;
  background: #f1f5f9;
  padding: 2px 8px;
  border-radius: 6px;
}

.card-code {
  font-size: 0.75rem;
  font-weight: 600;
  color: #475569;
  font-family: 'SF Mono', 'Fira Code', monospace;
}

.card-date {
  font-size: 0.7rem;
  color: #94a3b8;
  white-space: nowrap;
}

.card-question {
  margin: 0 0 8px;
  font-size: 0.85rem;
  font-weight: 600;
  color: #1e293b;
  line-height: 1.4;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.card-preview {
  font-size: 0.78rem;
  color: #64748b;
  line-height: 1.5;
  margin-bottom: 10px;
  display: -webkit-box;
  -webkit-line-clamp: 3;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.card-footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding-top: 8px;
  border-top: 1px solid #f1f5f9;
}

.card-attempt {
  display: flex;
  align-items: center;
  gap: 4px;
  font-size: 0.72rem;
  color: #94a3b8;
  font-weight: 500;
}

.card-credits {
  font-size: 0.72rem;
  color: #64748b;
  font-weight: 600;
}

/* Pagination */
.pagination {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 12px;
  margin-top: 24px;
}

.pagination-btn {
  width: 36px;
  height: 36px;
  border-radius: 10px;
  border: 1px solid #e2e8f0;
  background: white;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  color: #475569;
  transition: all 0.15s;
}

.pagination-btn:hover:not(:disabled) {
  background: #f1f5f9;
  border-color: #94a3b8;
}

.pagination-btn:disabled {
  opacity: 0.3;
  cursor: not-allowed;
}

.pagination-info {
  font-size: 0.82rem;
  font-weight: 600;
  color: #64748b;
}

/* Conversation Dialog */
.conversation-dialog {
  border-radius: 20px !important;
}

.dialog-title {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px 20px;
}

.dialog-title-left {
  display: flex;
  align-items: center;
  gap: 12px;
}

.dialog-icon {
  width: 36px;
  height: 36px;
  border-radius: 10px;
  background: linear-gradient(180deg, #0f172a 0%, #334155 100%);
  display: flex;
  align-items: center;
  justify-content: center;
}

.dialog-brand {
  font-size: 0.95rem;
  font-weight: 700;
  color: #0f172a;
  margin-right: 8px;
}

.dialog-code {
  font-size: 0.85rem;
  font-weight: 600;
  color: #475569;
  font-family: 'SF Mono', 'Fira Code', monospace;
}

.dialog-body {
  padding: 20px !important;
  max-height: 65vh;
  overflow-y: auto;
}

.dialog-loading {
  display: flex;
  justify-content: center;
  padding: 40px 0;
}

/* Conversation Messages */
.conversation-messages {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.conv-msg {
  display: flex;
  gap: 10px;
  align-items: flex-start;
}

.conv-msg--user {
  justify-content: flex-end;
}

.conv-msg--divider {
  justify-content: center;
}

.conv-avatar {
  width: 32px;
  height: 32px;
  border-radius: 10px;
  background: linear-gradient(180deg, #0f172a 0%, #334155 100%);
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.conv-bubble {
  max-width: 85%;
  padding: 12px 16px;
  border-radius: 14px;
  background: #f8fafc;
  border: 1px solid #e2e8f0;
}

.conv-bubble--user {
  background: linear-gradient(135deg, #0f172a 0%, #334155 100%);
  border-color: transparent;
  color: white;
}

.conv-text {
  font-size: 0.88rem;
  line-height: 1.6;
  color: #334155;
  word-break: break-word;
}

.conv-bubble--user .conv-text { color: white; }

.conv-divider {
  display: flex;
  align-items: center;
  gap: 10px;
  width: 100%;
  padding: 4px 0;
}

.conv-divider-line {
  flex: 1;
  height: 1px;
  background: #e2e8f0;
}

.conv-divider-text {
  display: flex;
  align-items: center;
  gap: 4px;
  font-size: 0.72rem;
  font-weight: 600;
  color: #94a3b8;
  white-space: nowrap;
}

/* Markdown inside dialog */
.conv-text :deep(.md-h1) {
  font-size: 1.05rem;
  font-weight: 800;
  color: #0f172a;
  margin: 12px 0 6px;
  padding-bottom: 4px;
  border-bottom: 1px solid #f1f5f9;
}

.conv-text :deep(.md-h2) {
  font-size: 0.95rem;
  font-weight: 700;
  color: #1e293b;
  margin: 10px 0 4px;
}

.conv-text :deep(.md-h3) {
  font-size: 0.88rem;
  font-weight: 700;
  color: #334155;
  margin: 8px 0 4px;
}

.conv-text :deep(.md-h4) {
  font-size: 0.84rem;
  font-weight: 700;
  color: #475569;
  margin: 6px 0 3px;
}

.conv-text :deep(.md-list) {
  margin: 4px 0 8px 0;
  padding-left: 18px;
  list-style: disc;
}

.conv-text :deep(.md-list li) {
  margin-bottom: 2px;
  font-size: 0.85rem;
}

.conv-text :deep(.md-step) {
  font-weight: 600;
  margin: 6px 0 2px;
  font-size: 0.85rem;
  color: #1e293b;
}

.conv-text :deep(.md-p) { margin: 2px 0; }

.conv-text :deep(code) {
  background: #f1f5f9;
  padding: 1px 5px;
  border-radius: 4px;
  font-size: 0.8rem;
  font-family: 'SF Mono', 'Fira Code', monospace;
  color: #0f172a;
}

/* Responsive */
@media (max-width: 768px) {
  .history-page { padding: 20px 16px 40px; }
  .page-header { flex-direction: column; align-items: flex-start; }
  .history-grid { grid-template-columns: 1fr; }
  .search-bar { flex-direction: column; align-items: stretch; }
  .result-count { text-align: right; }
}
</style>
