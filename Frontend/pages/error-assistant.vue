<template>
  <div class="ai-dashboard">
    <!-- Animated Background -->
    <div class="ai-bg">
      <div class="ai-bg-orb ai-bg-orb--1"></div>
      <div class="ai-bg-orb ai-bg-orb--2"></div>
      <div class="ai-bg-orb ai-bg-orb--3"></div>
    </div>

    <!-- Hero Section -->
    <section class="hero-section" :class="{ 'hero-section--compact': messages.length > 0 }">
      <div class="hero-greeting">
        <div class="ai-icon-wrapper">
          <v-icon size="24" color="white">mdi-robot-happy-outline</v-icon>
        </div>
        <h1 class="hero-title">
          Merhaba, <span class="hero-name">{{ authStore.user?.firstName || 'Kullanıcı' }}</span>
        </h1>
        <p class="hero-subtitle">Makine hata kodları hakkında size yardımcı olabilirim</p>

        <!-- Token Balance Badge -->
        <div v-if="tokenBalance !== null" class="token-badge">
          <v-icon size="14">mdi-circle-multiple-outline</v-icon>
          <span>{{ tokenBalance.toFixed(1) }} kredi</span>
        </div>
      </div>

      <!-- Hero Input (conversation hasn't started) -->
      <div v-if="messages.length === 0" class="ai-input-block">

        <!-- Makine Seçici -->
        <div class="machine-picker" ref="machinePickerHeroRef">
          <!-- Trigger -->
          <button
            class="machine-picker-trigger"
            :class="{ 'machine-picker-trigger--selected': !!selectedMachineId, 'machine-picker-trigger--open': machineDropdownOpen }"
            @click="machineDropdownOpen = !machineDropdownOpen"
            type="button"
          >
            <div class="machine-picker-trigger-left">
              <div class="machine-picker-icon" :class="{ 'machine-picker-icon--selected': !!selectedMachineId }">
                <v-icon size="16" :color="selectedMachineId ? 'white' : '#64748b'">mdi-robot-industrial</v-icon>
              </div>
              <div class="machine-picker-text">
                <span v-if="!selectedMachineId" class="machine-picker-placeholder">Makine seçin <span class="machine-picker-opt">(opsiyonel)</span></span>
                <span v-else class="machine-picker-value">
                  <strong>{{ selectedMachine?.brand }}</strong> {{ selectedMachine?.model }}
                  <span v-if="selectedMachine?.name" class="machine-picker-name">— {{ selectedMachine?.name }}</span>
                </span>
              </div>
            </div>
            <div class="machine-picker-trigger-right">
              <span v-if="!selectedMachineId" class="machine-picker-penalty">
                <v-icon size="11" color="#f59e0b">mdi-lightning-bolt</v-icon>2x
              </span>
              <v-icon
                size="16"
                class="machine-picker-chevron"
                :class="{ 'machine-picker-chevron--open': machineDropdownOpen }"
                color="#94a3b8"
              >mdi-chevron-down</v-icon>
            </div>
          </button>

          <!-- Dropdown -->
          <Transition name="picker-drop">
            <div v-if="machineDropdownOpen" class="machine-picker-dropdown">
              <!-- Boş durum -->
              <div v-if="machines.length === 0" class="machine-picker-empty">
                <v-icon size="28" color="#cbd5e1">mdi-robot-industrial-outline</v-icon>
                <span>Henüz makine eklenmemiş</span>
                <NuxtLink to="/my-machines" class="machine-picker-add-btn" @click="machineDropdownOpen = false">
                  <v-icon size="14">mdi-plus</v-icon> Makine Ekle
                </NuxtLink>
              </div>
              <!-- Liste -->
              <template v-else>
                <div class="machine-picker-list">
                  <!-- Seçimi kaldır -->
                  <button
                    class="machine-picker-item machine-picker-item--none"
                    :class="{ 'machine-picker-item--active': !selectedMachineId }"
                    @click="selectedMachineId = null; machineDropdownOpen = false"
                    type="button"
                  >
                    <div class="machine-picker-item-icon">
                      <v-icon size="14" color="#94a3b8">mdi-close-circle-outline</v-icon>
                    </div>
                    <span class="machine-picker-item-label">Makine seçme</span>
                    <span class="machine-picker-item-penalty">
                      <v-icon size="11" color="#f59e0b">mdi-lightning-bolt</v-icon>2x kredi
                    </span>
                  </button>
                  <!-- Makineler -->
                  <button
                    v-for="m in machines"
                    :key="m.id"
                    class="machine-picker-item"
                    :class="{ 'machine-picker-item--active': selectedMachineId === m.id }"
                    @click="selectedMachineId = m.id; machineDropdownOpen = false"
                    type="button"
                  >
                    <div class="machine-picker-item-icon" :class="{ 'machine-picker-item-icon--sel': selectedMachineId === m.id }">
                      <v-icon size="14" :color="selectedMachineId === m.id ? 'white' : '#64748b'">mdi-robot-industrial</v-icon>
                    </div>
                    <div class="machine-picker-item-info">
                      <span class="machine-picker-item-brand">{{ m.brand }} {{ m.model }}</span>
                      <span v-if="m.name" class="machine-picker-item-name">{{ m.name }}</span>
                    </div>
                    <v-icon v-if="selectedMachineId === m.id" size="16" color="#16a34a" class="machine-picker-check">mdi-check-circle</v-icon>
                  </button>
                </div>
              </template>
            </div>
          </Transition>
        </div>

        <div class="ai-input-container ai-input-container--hero">
          <div class="ai-input-wrapper ai-input-wrapper--stacked" :class="{ 'ai-input-wrapper--focused': isInputFocused }">
            <div class="ai-input-field-wrap">
              <textarea
                ref="textareaRef"
                v-model="searchQuery"
                class="ai-input"
                placeholder="Bir makine hata kodu veya sorun açıklaması yazın..."
                rows="3"
                @focus="isInputFocused = true"
                @blur="isInputFocused = false"
                @keydown.enter="onInputKeydown"
                @input="resizeTextarea"
                :disabled="isLoading"
              />
              <div ref="mirrorRef" class="ai-input-mirror" aria-hidden="true">{{ searchQuery || ' ' }}</div>
            </div>
            <div class="ai-input-actions">
              <button v-if="searchQuery" class="ai-input-clear" @click="searchQuery = ''">
                <v-icon size="18">mdi-close-circle</v-icon>
              </button>
              <button class="ai-input-send" @click="handleAsk" :disabled="!searchQuery.trim() || isLoading">
                <v-icon size="20" color="white">mdi-arrow-up</v-icon>
              </button>
            </div>
          </div>
        </div>

        <!-- Template Questions -->
        <div v-if="templateQuestions.length > 0" class="template-chips">
          <button
            v-for="tpl in templateQuestions"
            :key="tpl.query"
            class="template-chip"
            @click="searchQuery = tpl.query; selectedMachineId = tpl.machineId; $nextTick(() => textareaRef?.focus())"
          >
            <v-icon size="14" :color="tpl.color">{{ tpl.icon }}</v-icon>
            <span>{{ tpl.title }}</span>
          </button>
        </div>
        <div v-else class="template-chips">
          <NuxtLink to="/my-machines" class="template-chip template-chip--add">
            <v-icon size="14" color="#64748b">mdi-plus-circle-outline</v-icon>
            <span>Makine ekleyerek hızlı sorgula</span>
          </NuxtLink>
        </div>
      </div>

      <!-- Last Query Card (hero screen only) -->
      <div v-if="messages.length === 0 && lastQuery" class="last-query-card">
        <div class="last-query-header">
          <div class="last-query-header-left">
            <v-icon size="18" color="#334155">mdi-clock-outline</v-icon>
            <span class="last-query-label">Son Sorgunuz</span>
          </div>
          <NuxtLink to="/error-history" class="last-query-link">
            Tüm Geçmişi Gör
            <v-icon size="14">mdi-arrow-right</v-icon>
          </NuxtLink>
        </div>
        <button class="last-query-body" @click="loadConversation(lastQuery)">
          <div class="last-query-badges">
            <span v-if="lastQuery.brand" class="lq-brand">{{ lastQuery.brand }}</span>
            <span v-if="lastQuery.errorCode" class="lq-code">{{ lastQuery.errorCode }}</span>
            <v-chip
              v-if="lastQuery.isAccepted === true"
              size="x-small"
              color="success"
              variant="tonal"
              prepend-icon="mdi-check-circle"
            >Kabul Edildi</v-chip>
          </div>
          <p class="last-query-question">{{ lastQuery.userQuestion }}</p>
          <div class="last-query-meta">
            <span class="lq-date">{{ formatDate(lastQuery.createdDate) }}</span>
            <span class="lq-credits">{{ lastQuery.creditsCharged.toFixed(1) }} kredi</span>
          </div>
        </button>
      </div>
    </section>

    <!-- Chat Messages Area -->
    <section v-if="messages.length > 0" class="chat-section">
      <div ref="chatContainerRef" class="chat-messages">
        <div
          v-for="(msg, index) in messages"
          :key="index"
          class="chat-message"
          :class="{
            'chat-message--user': msg.role === 'user',
            'chat-message--assistant': msg.role === 'assistant',
            'chat-message--system': msg.role === 'system'
          }"
        >
          <!-- AI Avatar -->
          <div v-if="msg.role === 'assistant'" class="chat-avatar">
            <v-icon size="24" color="white">mdi-robot-happy-outline</v-icon>
          </div>

          <!-- System Message (retry divider, done stats, needs_info form) -->
          <div v-if="msg.role === 'system'" class="chat-system-msg">
            <div v-if="msg.meta?.type === 'retry-divider'" class="retry-divider">
              <div class="retry-divider-line"></div>
              <span class="retry-divider-text">
                <v-icon size="14">mdi-refresh</v-icon>
                Tekrar #{{ msg.meta.attempt }}
              </span>
              <div class="retry-divider-line"></div>
            </div>
            <div v-else-if="msg.meta?.type === 'needs_info'" class="needs-info-card">
              <template v-if="!msg.meta.isSubmitted">
                <div class="needs-info-header">
                  <v-icon size="16" color="#2563eb">mdi-information-outline</v-icon>
                  <span>Size daha iyi yardımcı olabilmem için aşağıdaki bilgileri paylaşır mısınız?</span>
                </div>
                <div class="needs-info-fields">
                  <div v-for="field in msg.meta.missingFields" :key="field" class="needs-info-field">
                    <label class="needs-info-label">{{ fieldLabel(field) }}</label>
                    <input
                      v-model="enrichmentForm[field]"
                      :placeholder="fieldPlaceholder(field)"
                      class="needs-info-input"
                    />
                  </div>
                </div>
                <div class="needs-info-actions">
                  <button
                    class="action-btn action-btn--retry"
                    @click="handleEnrichmentSkip(msg.meta.conversationId, msg)"
                    :disabled="isLoading"
                  >
                    <v-icon size="16">mdi-skip-next</v-icon>
                    Atla, yine de dene
                  </button>
                  <button
                    class="action-btn action-btn--accept"
                    @click="handleEnrichmentSubmit(msg.meta.conversationId, msg)"
                    :disabled="isLoading"
                  >
                    <v-icon size="16">mdi-send</v-icon>
                    Gönder ve Dene
                  </button>
                </div>
              </template>
              <template v-else>
                <div class="needs-info-submitted">
                  <v-icon size="14" color="#16a34a">mdi-check-circle</v-icon>
                  <span>Bilgiler gönderildi</span>
                </div>
              </template>
            </div>
            <div v-else-if="msg.meta?.type === 'done'" class="done-msg">
              <div v-if="msg.meta.isHistory && msg.meta.isAccepted === true" class="done-stats">
                <v-chip
                  size="x-small"
                  color="success"
                  variant="tonal"
                  prepend-icon="mdi-check-circle"
                >Kabul Edildi</v-chip>
              </div>
              <template v-if="!msg.meta.isHistory">
                <div v-if="msg.meta.remainingRetries > 0" class="done-info-text">
                  Bu çözüm işinize yaradı mı? Kabul edebilir veya farklı bir çözüm deneyebilirsiniz.
                </div>
                <div v-else class="done-info-text done-info-text--warning">
                  <v-icon size="16" color="#b45309">mdi-alert-circle-outline</v-icon>
                  Maksimum deneme hakkınız doldu. Çözüm işinize yaradıysa kabul edin, aksi halde destek talebi oluşturabilirsiniz.
                </div>
                <div class="done-actions">
                  <button
                    v-if="msg.meta.remainingRetries > 0"
                    class="action-btn action-btn--retry"
                    @click="handleRetry(msg.meta.conversationId)"
                    :disabled="isLoading"
                  >
                    <v-icon size="16">mdi-refresh</v-icon>
                    Tekrar Dene
                  </button>
                  <button
                    v-if="msg.meta.remainingRetries <= 0"
                    class="action-btn action-btn--not-satisfied"
                    @click="handleNotSatisfied(msg.meta.conversationId)"
                    :disabled="isLoading"
                  >
                    <v-icon size="16">mdi-emoticon-sad-outline</v-icon>
                    Yeterli Olmadı
                  </button>
                  <button
                    class="action-btn action-btn--accept"
                    @click="handleAccept(msg.meta.conversationId, msg.meta.attempt)"
                    :disabled="isLoading"
                  >
                    <v-icon size="16">mdi-check</v-icon>
                    Kabul Et
                  </button>
                </div>
              </template>
            </div>
          </div>

          <!-- Chat Bubble -->
          <div v-else class="chat-bubble">
            <div class="chat-text" v-html="formatMessage(msg.content)"></div>
          </div>
        </div>

        <!-- Loading Indicator (only before first chunk arrives) -->
        <div v-if="isLoading && currentStreamingIndex === -1" class="chat-message chat-message--assistant">
          <div class="chat-avatar">
            <v-icon size="24" color="white">mdi-robot-happy-outline</v-icon>
          </div>
          <div class="chat-bubble chat-bubble--loading">
            <div class="typing-dots">
              <span></span><span></span><span></span>
            </div>
          </div>
        </div>
      </div>

      <!-- Chat modu makine seçici -->
      <div class="machine-picker machine-picker--chat" ref="machinePickerChatRef">
        <button
          class="machine-picker-trigger"
          :class="{ 'machine-picker-trigger--selected': !!selectedMachineId, 'machine-picker-trigger--open': machineDropdownOpen }"
          @click="machineDropdownOpen = !machineDropdownOpen"
          type="button"
        >
          <div class="machine-picker-trigger-left">
            <div class="machine-picker-icon" :class="{ 'machine-picker-icon--selected': !!selectedMachineId }">
              <v-icon size="15" :color="selectedMachineId ? 'white' : '#64748b'">mdi-robot-industrial</v-icon>
            </div>
            <div class="machine-picker-text">
              <span v-if="!selectedMachineId" class="machine-picker-placeholder">Makine seçin</span>
              <span v-else class="machine-picker-value">
                <strong>{{ selectedMachine?.brand }}</strong> {{ selectedMachine?.model }}
              </span>
            </div>
          </div>
          <div class="machine-picker-trigger-right">
            <span v-if="!selectedMachineId" class="machine-picker-penalty">
              <v-icon size="11" color="#f59e0b">mdi-lightning-bolt</v-icon>2x
            </span>
            <v-icon
              size="16"
              class="machine-picker-chevron"
              :class="{ 'machine-picker-chevron--open': machineDropdownOpen }"
              color="#94a3b8"
            >mdi-chevron-down</v-icon>
          </div>
        </button>

        <Transition name="picker-drop">
          <div v-if="machineDropdownOpen" class="machine-picker-dropdown machine-picker-dropdown--up">
            <div v-if="machines.length === 0" class="machine-picker-empty">
              <v-icon size="24" color="#cbd5e1">mdi-robot-industrial-outline</v-icon>
              <span>Makine eklenmemiş</span>
              <NuxtLink to="/machines" class="machine-picker-add-btn" @click="machineDropdownOpen = false">
                <v-icon size="13">mdi-plus</v-icon> Ekle
              </NuxtLink>
            </div>
            <template v-else>
              <div class="machine-picker-list">
                <button
                  class="machine-picker-item machine-picker-item--none"
                  :class="{ 'machine-picker-item--active': !selectedMachineId }"
                  @click="selectedMachineId = null; machineDropdownOpen = false"
                  type="button"
                >
                  <div class="machine-picker-item-icon">
                    <v-icon size="13" color="#94a3b8">mdi-close-circle-outline</v-icon>
                  </div>
                  <span class="machine-picker-item-label">Seçme</span>
                  <span class="machine-picker-item-penalty">
                    <v-icon size="10" color="#f59e0b">mdi-lightning-bolt</v-icon>2x
                  </span>
                </button>
                <button
                  v-for="m in machines"
                  :key="m.id"
                  class="machine-picker-item"
                  :class="{ 'machine-picker-item--active': selectedMachineId === m.id }"
                  @click="selectedMachineId = m.id; machineDropdownOpen = false"
                  type="button"
                >
                  <div class="machine-picker-item-icon" :class="{ 'machine-picker-item-icon--sel': selectedMachineId === m.id }">
                    <v-icon size="13" :color="selectedMachineId === m.id ? 'white' : '#64748b'">mdi-robot-industrial</v-icon>
                  </div>
                  <div class="machine-picker-item-info">
                    <span class="machine-picker-item-brand">{{ m.brand }} {{ m.model }}</span>
                    <span v-if="m.name" class="machine-picker-item-name">{{ m.name }}</span>
                  </div>
                  <v-icon v-if="selectedMachineId === m.id" size="15" color="#16a34a" class="machine-picker-check">mdi-check-circle</v-icon>
                </button>
              </div>
            </template>
          </div>
        </Transition>
      </div>

      <!-- Chat Input (conversation mode) -->
      <div class="ai-input-container ai-input-container--full">
        <div class="ai-input-wrapper ai-input-wrapper--full ai-input-wrapper--stacked" :class="{ 'ai-input-wrapper--focused': isInputFocused }">
          <div class="ai-input-field-wrap">
            <textarea
              ref="textareaRef"
              v-model="searchQuery"
              class="ai-input"
              placeholder="Başka bir hata kodu sorun..."
              rows="3"
              @focus="isInputFocused = true"
              @blur="isInputFocused = false"
              @keydown.enter="onInputKeydown"
              @input="resizeTextarea"
              :disabled="isLoading"
            />
            <div ref="mirrorRef" class="ai-input-mirror" aria-hidden="true">{{ searchQuery || ' ' }}</div>
          </div>
          <div class="ai-input-actions">
            <button v-if="searchQuery" class="ai-input-clear" @click="searchQuery = ''">
              <v-icon size="18">mdi-close-circle</v-icon>
            </button>
            <button class="ai-input-send" @click="handleAsk" :disabled="!searchQuery.trim() || isLoading">
              <v-icon size="20" color="white">mdi-arrow-up</v-icon>
            </button>
          </div>
        </div>
      </div>
    </section>

    <!-- Footer -->
    <div class="ai-footer">
      <p>Zeror AI — Makine Hata Kodu Asistanı</p>
    </div>

  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, nextTick, watch, onMounted, onBeforeUnmount } from 'vue'
import { onClickOutside } from '@vueuse/core'
import { useAuthStore } from '~/stores/auth'
import { API_ENDPOINTS } from '~/utils/apiEndpoints'
import type { UserMachine } from '~/types'

definePageMeta({
  middleware: ['auth', 'permission'],
  permission: 'Errors.Read'
})

const authStore = useAuthStore()
const config = useRuntimeConfig()

interface ChatMessage {
  role: 'user' | 'assistant' | 'system'
  content: string
  meta?: Record<string, any>
}

const searchQuery = ref('')
const isInputFocused = ref(false)
const messages = ref<ChatMessage[]>([])
const isLoading = ref(false)
const tokenBalance = ref<number | null>(null)
const chatContainerRef = ref<HTMLElement | null>(null)
const textareaRef = ref<HTMLTextAreaElement | null>(null)
const mirrorRef = ref<HTMLElement | null>(null)
const currentStreamText = ref('')
const currentStreamingIndex = ref(-1)
const currentRetryConversationId = ref<string | null>(null)
const activeConversationId = ref<string | null>(null)
const enrichmentForm = reactive<Record<string, string>>({ brand: '', model: '', errorCode: '', symptom: '' })

const templateQuestions = computed(() =>
  machines.value.slice(0, 4).map(m => ({
    icon: 'mdi-robot-industrial',
    title: m.name ? m.name : `${m.brand} ${m.model}`,
    color: 'primary',
    query: `${m.brand} ${m.model} `,
    machineId: m.id
  }))
)

const { get, post, patch } = useApi()
const toast = useToast()
const { getMyMachines } = useUserMachines()

// Makine seçimi state
const machines = ref<UserMachine[]>([])
const selectedMachineId = ref<number | null>(null)
const selectedMachine = computed(() => machines.value.find(m => m.id === selectedMachineId.value) ?? null)
const machineDropdownOpen = ref(false)
const machinePickerHeroRef = ref<HTMLElement | null>(null)
const machinePickerChatRef = ref<HTMLElement | null>(null)
onClickOutside(machinePickerHeroRef, () => { machineDropdownOpen.value = false })
onClickOutside(machinePickerChatRef, () => { machineDropdownOpen.value = false })

const fetchMachines = async () => {
  try {
    machines.value = await getMyMachines()
  } catch {
    machines.value = []
  }
}

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

const lastQuery = ref<HistoryItem | null>(null)
const conversationLoading = ref(false)

const fetchLastQuery = async () => {
  try {
    const params = new URLSearchParams({ page: '1', pageSize: '1' })
    const res = await get<any>(`${API_ENDPOINTS.ERRORS.HISTORY}?${params.toString()}`)
    const items = res.data?.items ?? []
    lastQuery.value = items.length > 0 ? items[0] : null
  } catch {
    lastQuery.value = null
  }
}

const loadConversation = async (item: HistoryItem) => {
  conversationLoading.value = true
  try {
    const res = await get<any>(API_ENDPOINTS.ERRORS.CONVERSATION(item.conversationId))
    const attempts = res.data ?? []
    if (attempts.length === 0) return

    const rebuilt: ChatMessage[] = []
    const first = attempts[0]

    rebuilt.push({ role: 'user', content: first.userQuestion })

    for (const attempt of attempts) {
      if (attempt.attemptNumber > 1) {
        rebuilt.push({
          role: 'system',
          content: '',
          meta: { type: 'retry-divider', attempt: attempt.attemptNumber }
        })
      }
      rebuilt.push({ role: 'assistant', content: attempt.aiResponse || '' })
    }

    messages.value = rebuilt
    scrollToBottom()
  } catch {
    toast.error('Konuşma yüklenirken hata oluştu.', { title: 'Yüklenemedi' })
  } finally {
    conversationLoading.value = false
  }
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

const startNewConversation = () => {
  messages.value = []
  currentStreamText.value = ''
  activeConversationId.value = null
  nextTick(() => textareaRef.value?.focus())
}

const fetchBalance = async () => {
  try {
    const res = await get<{ balance: number }>(API_ENDPOINTS.ERRORS.BALANCE)
    tokenBalance.value = res.data?.balance ?? null
  } catch {
    // silent
  }
}

const escapeHtml = (raw: string) =>
  raw
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>')
    .replace(/`([^`]+)`/g, '<code>$1</code>')

const isTableRow = (line: string) => /^\s*\|.+\|\s*$/.test(line)
const isSeparatorRow = (line: string) => /^\s*\|[\s\-|:]+\|\s*$/.test(line)

const parseTableRow = (line: string) =>
  line.split('|').slice(1, -1).map(cell => escapeHtml(cell.trim()))

const flushTable = (tableLines: string[], html: string[]) => {
  if (tableLines.length === 0) return
  html.push('<div class="md-table-wrap"><table class="md-table">')
  let headerDone = false
  for (const tLine of tableLines) {
    if (isSeparatorRow(tLine)) { headerDone = true; continue }
    const cells = parseTableRow(tLine)
    if (!headerDone) {
      html.push('<thead><tr>' + cells.map(c => `<th>${c}</th>`).join('') + '</tr></thead><tbody>')
      headerDone = true
    } else {
      html.push('<tr>' + cells.map(c => `<td>${c}</td>`).join('') + '</tr>')
    }
  }
  html.push('</tbody></table></div>')
  tableLines.length = 0
}

const formatMessage = (text: string) => {
  if (!text) return ''

  const lines = text.split('\n')
  const html: string[] = []
  let inList = false
  const tableBuffer: string[] = []

  for (const rawLine of lines) {
    // Tablo satırı
    if (isTableRow(rawLine)) {
      if (inList) { html.push('</ul>'); inList = false }
      tableBuffer.push(rawLine)
      continue
    }

    // Tablo bitti, flush et
    if (tableBuffer.length > 0) {
      flushTable(tableBuffer, html)
    }

    const line = escapeHtml(rawLine)

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

  if (tableBuffer.length > 0) flushTable(tableBuffer, html)
  if (inList) html.push('</ul>')

  return html.join('')
}

const scrollToBottom = () => {
  nextTick(() => {
    if (chatContainerRef.value) {
      chatContainerRef.value.scrollTop = chatContainerRef.value.scrollHeight
    }
  })
}

const onInputKeydown = (e: KeyboardEvent) => {
  if (e.key === 'Enter' && !e.shiftKey) {
    e.preventDefault()
    handleAsk()
  }
}

const TEXTAREA_MIN_HEIGHT = 72
const TEXTAREA_MAX_HEIGHT = 240

const resizeTextarea = () => {
  nextTick(() => {
    const textarea = textareaRef.value
    const mirror = mirrorRef.value
    if (!textarea || !mirror) return
    mirror.style.width = textarea.clientWidth + 'px'
    mirror.textContent = textarea.value || ' '
    const contentHeight = mirror.getBoundingClientRect().height
    const h = Math.max(TEXTAREA_MIN_HEIGHT, Math.min(contentHeight, TEXTAREA_MAX_HEIGHT))
    textarea.style.height = h + 'px'
  })
}

watch(searchQuery, resizeTextarea)

const streamSSE = async (url: string, body?: any) => {
  const token = authStore.accessToken
  const fullUrl = `${config.public.apiBase}${url}`

  const response = await fetch(fullUrl, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    },
    ...(body ? { body: JSON.stringify(body) } : {})
  })

  if (!response.ok) {
    throw new Error(`HTTP ${response.status}`)
  }

  const reader = response.body?.getReader()
  if (!reader) throw new Error('No response body')

  const decoder = new TextDecoder()
  let buffer = ''

  while (true) {
    const { done, value } = await reader.read()
    if (done) break

    buffer += decoder.decode(value, { stream: true })
    const parts = buffer.split('\n\n')
    buffer = parts.pop() || ''

    for (const part of parts) {
      const line = part.trim()
      if (line.startsWith('data: ')) {
        try {
          const event = JSON.parse(line.slice(6))
          handleSSEEvent(event)
        } catch {
          // skip malformed events
        }
      }
    }
  }

  if (buffer.trim().startsWith('data: ')) {
    try {
      const event = JSON.parse(buffer.trim().slice(6))
      handleSSEEvent(event)
    } catch {
      // skip
    }
  }
}

const handleSSEEvent = (event: any) => {
  switch (event.type) {
    case 'parse':
      break

    case 'needs_info':
      enrichmentForm.brand = ''
      enrichmentForm.model = ''
      enrichmentForm.errorCode = ''
      enrichmentForm.symptom = ''
      messages.value.push({
        role: 'system',
        content: '',
        meta: {
          type: 'needs_info',
          missingFields: event.missingFields,
          conversationId: currentRetryConversationId.value,
          isSubmitted: false
        }
      })
      scrollToBottom()
      break

    case 'chunk':
      currentStreamText.value += event.text
      if (currentStreamingIndex.value >= 0 && currentStreamingIndex.value < messages.value.length) {
        messages.value[currentStreamingIndex.value].content = currentStreamText.value
      } else {
        messages.value.push({ role: 'assistant', content: currentStreamText.value })
        currentStreamingIndex.value = messages.value.length - 1
      }
      scrollToBottom()
      break

    case 'done':
      tokenBalance.value = event.remainingBalance
      activeConversationId.value = event.conversationId
      messages.value.push({
        role: 'system',
        content: '',
        meta: {
          type: 'done',
          conversationId: event.conversationId,
          attempt: event.attempt,
          creditsCharged: event.creditsCharged,
          remainingBalance: event.remainingBalance,
          remainingRetries: event.remainingRetries
        }
      })
      scrollToBottom()
      break

    case 'error':
      messages.value.push({
        role: 'assistant',
        content: event.message
      })
      scrollToBottom()
      break
  }
}

const handleAsk = async () => {
  const query = searchQuery.value.trim()
  if (!query || query.length < 3) return

  messages.value.push({ role: 'user', content: query })
  searchQuery.value = ''
  currentStreamText.value = ''
  currentStreamingIndex.value = -1
  scrollToBottom()

  isLoading.value = true
  try {
    // Aktif konuşma varsa yeni mesajı devam olarak route et
    if (activeConversationId.value) {
      currentRetryConversationId.value = activeConversationId.value
      await streamSSE(
        API_ENDPOINTS.ERRORS.RETRY(activeConversationId.value),
        { continuationQuestion: query }
      )
    } else {
      const body: Record<string, any> = { question: query }
      if (selectedMachineId.value) body.machineId = selectedMachineId.value
      await streamSSE(API_ENDPOINTS.ERRORS.ASK, body)
    }
  } catch (error: any) {
    messages.value.push({
      role: 'assistant',
      content: 'Bağlantı hatası oluştu. Lütfen tekrar deneyin.'
    })
  } finally {
    isLoading.value = false
    scrollToBottom()
    nextTick(() => textareaRef.value?.focus())
  }
}

const handleRetry = async (conversationId: string) => {
  currentRetryConversationId.value = conversationId
  currentStreamText.value = ''
  currentStreamingIndex.value = -1
  isLoading.value = true
  try {
    await streamSSE(API_ENDPOINTS.ERRORS.RETRY(conversationId))
  } catch (error: any) {
    messages.value.push({
      role: 'assistant',
      content: 'Tekrar denemede hata oluştu.'
    })
  } finally {
    isLoading.value = false
    scrollToBottom()
  }
}

const handleEnrichmentSubmit = async (conversationId: string, msg: ChatMessage) => {
  msg.meta!.isSubmitted = true
  const body: Record<string, any> = {}
  if (enrichmentForm.brand) body.brand = enrichmentForm.brand
  if (enrichmentForm.model) body.model = enrichmentForm.model
  if (enrichmentForm.errorCode) body.errorCode = enrichmentForm.errorCode
  if (enrichmentForm.symptom) body.symptom = enrichmentForm.symptom

  currentStreamText.value = ''
  currentStreamingIndex.value = -1
  isLoading.value = true
  try {
    await streamSSE(API_ENDPOINTS.ERRORS.RETRY(conversationId), body)
  } catch {
    messages.value.push({ role: 'assistant', content: 'Tekrar denemede hata oluştu.' })
  } finally {
    isLoading.value = false
    scrollToBottom()
  }
}

const handleEnrichmentSkip = async (conversationId: string, msg: ChatMessage) => {
  msg.meta!.isSubmitted = true
  currentStreamText.value = ''
  currentStreamingIndex.value = -1
  isLoading.value = true
  try {
    await streamSSE(API_ENDPOINTS.ERRORS.RETRY(conversationId), { skipEnrichment: true })
  } catch {
    messages.value.push({ role: 'assistant', content: 'Tekrar denemede hata oluştu.' })
  } finally {
    isLoading.value = false
    scrollToBottom()
  }
}

const fieldLabel = (field: string) => {
  const labels: Record<string, string> = {
    brand: 'Marka',
    model: 'Model',
    errorCode: 'Hata Kodu',
    symptom: 'Belirti / Semptom'
  }
  return labels[field] ?? field
}

const fieldPlaceholder = (field: string) => {
  const placeholders: Record<string, string> = {
    brand: 'örn. Siemens, Fanuc, Lenze...',
    model: 'örn. 840D, 0i-F, 8400...',
    errorCode: 'örn. F30001, Alarm 414...',
    symptom: 'örn. Kırmızı ışık yanıyor, ekran donuyor...'
  }
  return placeholders[field] ?? ''
}

const handleNotSatisfied = async (conversationId: string) => {
  toast.warning('Geri bildiriminiz kaydedildi. Destek ekibimiz sizinle iletişime geçecektir.', {
    title: 'Teşekkürler',
  })
}

const handleAccept = async (conversationId: string, attemptNumber: number) => {
  try {
    await patch(API_ENDPOINTS.ERRORS.ACCEPT(conversationId), { attemptNumber })
    toast.success('Çözümünüz kaydedildi.', { title: 'Kabul edildi' })
  } catch {
    toast.error('Kabul işlemi tamamlanamadı. Lütfen tekrar deneyin.', { title: 'Hata' })
  }
}

const setBodyOverflow = (hidden: boolean) => {
  if (import.meta.client) {
    document.documentElement.style.overflow = hidden ? 'hidden' : ''
    document.body.style.overflow = hidden ? 'hidden' : ''
  }
}

watch(() => messages.value.length, (len) => {
  setBodyOverflow(len === 0)
}, { immediate: true })

onMounted(() => {
  fetchBalance()
  fetchLastQuery()
  fetchMachines()
  resizeTextarea()
  window.addEventListener('resize', resizeTextarea)
  nextTick(() => textareaRef.value?.focus())
})

onBeforeUnmount(() => {
  setBodyOverflow(false)
  window.removeEventListener('resize', resizeTextarea)
})

useHead({ title: 'Hata Asistanı - CassMach' })
</script>

<style scoped>
.ai-dashboard {
  position: relative;
  min-height: 100vh;
  background: #f1f5f9;
  padding: 32px 40px 60px;
  overflow: visible;
}

/* Animated Background Orbs */
.ai-bg {
  position: fixed;
  inset: 0;
  pointer-events: none;
  z-index: 0;
  overflow: hidden;
}

.ai-bg-orb {
  position: absolute;
  border-radius: 50%;
  filter: blur(120px);
  opacity: 0.12;
  animation: orbFloat 20s ease-in-out infinite;
}

.ai-bg-orb--1 {
  width: 500px;
  height: 500px;
  background: #334155;
  top: -120px;
  right: -100px;
}

.ai-bg-orb--2 {
  width: 400px;
  height: 400px;
  background: #0f172a;
  bottom: -80px;
  left: -60px;
  animation-delay: -7s;
}

.ai-bg-orb--3 {
  width: 300px;
  height: 300px;
  background: #475569;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  animation-delay: -14s;
}

@keyframes orbFloat {
  0%, 100% { transform: translate(0, 0) scale(1); }
  25% { transform: translate(30px, -40px) scale(1.1); }
  50% { transform: translate(-20px, 20px) scale(0.95); }
  75% { transform: translate(15px, 30px) scale(1.05); }
}

/* Hero */
.hero-section {
  position: relative;
  z-index: 1;
  text-align: center;
  padding: 40px 0 36px;
}

.hero-greeting {
  margin-bottom: 32px;
}

.ai-icon-wrapper {
  width: 48px;
  height: 48px;
  margin: 0 auto 14px;
  background: linear-gradient(180deg, #0f172a 0%, #334155 100%);
  border-radius: 18px;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 8px 32px rgba(15, 23, 42, 0.25);
  animation: iconPulse 3s ease-in-out infinite;
}

@keyframes iconPulse {
  0%, 100% { box-shadow: 0 8px 32px rgba(15, 23, 42, 0.25); }
  50% { box-shadow: 0 8px 48px rgba(15, 23, 42, 0.4); }
}

.hero-title {
  font-size: 1.75rem;
  font-weight: 800;
  color: #0f172a;
  letter-spacing: -0.03em;
  margin: 0;
  line-height: 1.2;
}

.hero-name {
  background: linear-gradient(135deg, #0f172a, #475569);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.hero-subtitle {
  font-size: 0.95rem;
  color: #64748b;
  margin-top: 6px;
  font-weight: 400;
}

.token-badge {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  margin-top: 12px;
  padding: 6px 14px;
  background: white;
  border: 1px solid #e2e8f0;
  border-radius: 20px;
  font-size: 0.8rem;
  font-weight: 600;
  color: #475569;
  box-shadow: 0 2px 8px rgba(15, 23, 42, 0.04);
}

.hero-section--compact {
  padding: 20px 0 16px;
}

.hero-section--compact .hero-greeting { margin-bottom: 16px; }
.hero-section--compact .ai-icon-wrapper { width: 40px; height: 40px; margin-bottom: 8px; }
.hero-section--compact .hero-title { font-size: 1.35rem; }
.hero-section--compact .hero-subtitle { font-size: 0.85rem; }

/* Template Chips */
.template-chips {
  display: flex;
  flex-wrap: wrap;
  justify-content: center;
  gap: 8px;
  margin-top: 16px;
}

.template-chip {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 8px 14px;
  background: white;
  border: 1px solid #e2e8f0;
  border-radius: 12px;
  font-size: 0.8rem;
  font-weight: 500;
  color: #475569;
  cursor: pointer;
  font-family: inherit;
  transition: all 0.2s;
}

.template-chip:hover {
  border-color: #94a3b8;
  background: #f8fafc;
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(15, 23, 42, 0.06);
}

.template-chip--add {
  text-decoration: none;
  border-style: dashed;
  color: #64748b;
}

.template-chip--add:hover {
  border-color: #6366f1;
  color: #6366f1;
}

/* Chat Section */
.chat-section {
  position: relative;
  z-index: 1;
  width: 100%;
  max-width: 100%;
  margin: 0 0 24px;
  display: flex;
  flex-direction: column;
}

.chat-messages {
  flex: 1;
  overflow: visible;
  padding: 24px 0 16px;
  min-height: 200px;
}

.chat-message {
  display: flex;
  gap: 12px;
  margin-bottom: 20px;
  align-items: flex-start;
  width: 100%;
  animation: messageSlide 0.3s ease-out;
}

@keyframes messageSlide {
  from { opacity: 0; transform: translateY(8px); }
  to { opacity: 1; transform: translateY(0); }
}

.chat-message--user {
  flex-direction: row;
  justify-content: flex-end;
}

.chat-message--system {
  justify-content: center;
}

.chat-avatar {
  width: 40px;
  height: 40px;
  border-radius: 12px;
  background: linear-gradient(180deg, #0f172a 0%, #334155 100%);
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.chat-bubble {
  max-width: 75%;
  padding: 14px 18px;
  border-radius: 16px;
  background: white;
  border: 1px solid #e2e8f0;
}

.chat-message--user .chat-bubble {
  background: linear-gradient(135deg, #0f172a 0%, #334155 100%);
  border-color: transparent;
  margin-left: auto;
}

.chat-message--user .chat-text { color: white; }

.chat-text {
  margin: 0;
  font-size: 0.95rem;
  line-height: 1.6;
  color: #334155;
  word-break: break-word;
}

.chat-text :deep(.md-h1) {
  font-size: 1.15rem;
  font-weight: 800;
  color: #0f172a;
  margin: 16px 0 8px;
  padding-bottom: 6px;
  border-bottom: 1px solid #f1f5f9;
}

.chat-text :deep(.md-h2) {
  font-size: 1rem;
  font-weight: 700;
  color: #1e293b;
  margin: 14px 0 6px;
}

.chat-text :deep(.md-h3) {
  font-size: 0.92rem;
  font-weight: 700;
  color: #334155;
  margin: 10px 0 4px;
}

.chat-text :deep(.md-h4) {
  font-size: 0.88rem;
  font-weight: 700;
  color: #475569;
  margin: 8px 0 4px;
}

.chat-text :deep(.md-list) {
  margin: 4px 0 8px 0;
  padding-left: 20px;
  list-style: disc;
}

.chat-text :deep(.md-list li) {
  margin-bottom: 2px;
  font-size: 0.92rem;
}

.chat-text :deep(.md-step) {
  font-weight: 600;
  margin: 8px 0 2px;
  font-size: 0.92rem;
  color: #1e293b;
}

.chat-text :deep(.md-p) {
  margin: 2px 0;
}

.chat-text :deep(.md-table-wrap) {
  overflow-x: auto;
  margin: 8px 0;
  border-radius: 8px;
  border: 1px solid #e2e8f0;
}

.chat-text :deep(.md-table) {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.88rem;
}

.chat-text :deep(.md-table th) {
  background: #f1f5f9;
  padding: 8px 12px;
  text-align: left;
  font-weight: 600;
  color: #475569;
  border-bottom: 2px solid #e2e8f0;
  white-space: nowrap;
}

.chat-text :deep(.md-table td) {
  padding: 7px 12px;
  color: #1e293b;
  border-bottom: 1px solid #f1f5f9;
  vertical-align: top;
}

.chat-text :deep(.md-table tr:last-child td) {
  border-bottom: none;
}

.chat-text :deep(.md-table tr:hover td) {
  background: #f8fafc;
}

.chat-text :deep(code) {
  background: #f1f5f9;
  padding: 1px 5px;
  border-radius: 4px;
  font-size: 0.85rem;
  font-family: 'SF Mono', 'Fira Code', monospace;
  color: #0f172a;
}

.chat-message--user .chat-text :deep(.md-h1),
.chat-message--user .chat-text :deep(.md-h2),
.chat-message--user .chat-text :deep(.md-h3),
.chat-message--user .chat-text :deep(.md-h4),
.chat-message--user .chat-text :deep(.md-step) {
  color: white;
  border-color: rgba(255, 255, 255, 0.2);
}

.chat-message--user .chat-text :deep(code) {
  background: rgba(255, 255, 255, 0.15);
  color: white;
}

.chat-bubble--loading { padding: 18px 24px; }

.typing-dots {
  display: flex;
  gap: 6px;
  align-items: center;
}

.typing-dots span {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: #94a3b8;
  animation: typingBounce 1.4s ease-in-out infinite both;
}

.typing-dots span:nth-child(2) { animation-delay: 0.2s; }
.typing-dots span:nth-child(3) { animation-delay: 0.4s; }

@keyframes typingBounce {
  0%, 80%, 100% { transform: scale(0.8); opacity: 0.5; }
  40% { transform: scale(1.2); opacity: 1; }
}

/* System Messages */
.chat-system-msg {
  width: 100%;
  max-width: 700px;
  margin: 0 auto;
}

/* Retry Divider */
.retry-divider {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 4px 0;
}

.retry-divider-line {
  flex: 1;
  height: 1px;
  background: #e2e8f0;
}

.retry-divider-text {
  display: flex;
  align-items: center;
  gap: 4px;
  font-size: 0.75rem;
  font-weight: 600;
  color: #94a3b8;
  white-space: nowrap;
}

.needs-info-card {
  background: white;
  border: 1px solid #bfdbfe;
  border-left: 4px solid #2563eb;
  border-radius: 12px;
  padding: 16px 20px;
  max-width: 480px;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.needs-info-header {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 13px;
  color: #1e40af;
  font-weight: 500;
  line-height: 1.4;
}

.needs-info-fields {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.needs-info-field {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.needs-info-label {
  font-size: 11px;
  font-weight: 600;
  color: #64748b;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.needs-info-input {
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  padding: 8px 12px;
  font-size: 13px;
  color: #1e293b;
  background: #f8fafc;
  outline: none;
  transition: border-color 0.15s;
}

.needs-info-input:focus {
  border-color: #2563eb;
  background: white;
}

.needs-info-input::placeholder {
  color: #94a3b8;
}

.needs-info-actions {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}

.needs-info-submitted {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 12px;
  color: #16a34a;
  font-weight: 500;
}

.done-msg {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 12px 20px;
  background: white;
  border: 1px solid #e2e8f0;
  border-radius: 14px;
  box-shadow: 0 2px 8px rgba(15, 23, 42, 0.04);
}

.done-info-text {
  font-size: 0.8rem;
  color: #64748b;
  margin-bottom: 8px;
  line-height: 1.4;
  text-align: center;
}

.done-info-text--warning {
  display: flex;
  align-items: center;
  gap: 6px;
  color: #92400e;
  background: #fffbeb;
  border: 1px solid #fde68a;
  border-radius: 10px;
  padding: 8px 12px;
  text-align: left;
}

.done-stats {
  display: flex;
  flex-wrap: wrap;
  gap: 16px;
  margin-bottom: 12px;
}

.done-stat {
  display: flex;
  align-items: center;
  gap: 5px;
  font-size: 0.8rem;
  color: #64748b;
  font-weight: 500;
}

.done-actions {
  display: flex;
  gap: 8px;
}

.action-btn {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 8px 16px;
  border-radius: 10px;
  font-size: 0.8rem;
  font-weight: 600;
  cursor: pointer;
  font-family: inherit;
  border: 1px solid;
  transition: all 0.2s;
}

.action-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.action-btn--retry {
  background: #f8fafc;
  border-color: #e2e8f0;
  color: #475569;
}

.action-btn--retry:hover:not(:disabled) {
  background: #f1f5f9;
  border-color: #94a3b8;
}

.action-btn--not-satisfied {
  background: #fffbeb;
  border-color: #fde68a;
  color: #92400e;
}

.action-btn--not-satisfied:hover:not(:disabled) {
  background: #fef3c7;
  border-color: #fbbf24;
}

.action-btn--accept {
  background: linear-gradient(180deg, #0f172a 0%, #334155 100%);
  border-color: transparent;
  color: white;
}

.action-btn--accept:hover:not(:disabled) {
  box-shadow: 0 4px 16px rgba(15, 23, 42, 0.3);
  transform: translateY(-1px);
}

/* AI Input */
.ai-input-block {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 10px;
}

.ai-input-container {
  width: 95%;
  max-width: 900px;
  margin: 0 auto;
}

.ai-input-wrapper {
  display: flex;
  align-items: flex-end;
  gap: 12px;
  background: white;
  border: 2px solid #e2e8f0;
  border-radius: 16px;
  padding: 16px 20px 16px 24px;
  min-height: 64px;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  box-shadow: 0 4px 24px rgba(15, 23, 42, 0.04);
}

.ai-input-wrapper--stacked {
  flex-direction: column;
  align-items: stretch;
  padding: 16px 20px 12px;
}

.ai-input-field-wrap {
  position: relative;
  width: 100%;
}

.ai-input-mirror {
  position: absolute;
  top: 0; left: 0; right: 0;
  visibility: hidden;
  pointer-events: none;
  white-space: pre-wrap;
  word-wrap: break-word;
  font: inherit;
  font-size: 1rem;
  line-height: 1.5;
  padding: 0; margin: 0;
  min-height: 72px;
  box-sizing: border-box;
}

.ai-input-wrapper--stacked .ai-input {
  width: 100%;
  min-height: 72px;
  padding: 0;
  margin-bottom: 8px;
  overflow-y: auto;
  display: block;
}

.ai-input-actions {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 8px;
}

.ai-input-wrapper--focused {
  border-color: #334155;
  box-shadow: 0 4px 32px rgba(15, 23, 42, 0.1);
}

.ai-input {
  flex: 1;
  border: none;
  outline: none;
  font-size: 1rem;
  color: #0f172a;
  background: transparent;
  padding: 8px 0;
  min-height: 28px;
  font-family: inherit;
  resize: none;
  overflow-y: auto;
}

.ai-input::placeholder { color: #94a3b8; }

.ai-input-clear {
  background: none;
  border: none;
  cursor: pointer;
  color: #94a3b8;
  padding: 4px;
  display: flex;
  align-items: center;
  transition: color 0.2s;
}

.ai-input-clear:hover { color: #475569; }

.ai-input-send {
  width: 40px;
  height: 40px;
  border-radius: 12px;
  background: linear-gradient(180deg, #0f172a 0%, #334155 100%);
  border: none;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  transition: all 0.2s;
}

.ai-input-send:hover:not(:disabled) {
  transform: scale(1.05);
  box-shadow: 0 4px 16px rgba(15, 23, 42, 0.3);
}

.ai-input-send:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

.ai-input-wrapper--full {
  width: 100%;
  max-width: none;
}

.chat-section .ai-input-container {
  position: sticky;
  bottom: 0;
  padding: 12px 0 24px;
  background: transparent;
  z-index: 10;
}

.chat-section .ai-input-container--full {
  width: 100%;
  max-width: none;
}

.chat-section .ai-input-wrapper {
  margin: 0;
  background: #f8fafc;
  border-color: #e2e8f0;
}

/* Last Query Card */
.last-query-card {
  position: relative;
  z-index: 1;
  width: 95%;
  max-width: 600px;
  margin: 24px auto 0;
  background: white;
  border: 1px solid #e2e8f0;
  border-radius: 16px;
  padding: 16px 20px;
  box-shadow: 0 2px 12px rgba(15, 23, 42, 0.04);
}

.last-query-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 10px;
  padding-bottom: 8px;
  border-bottom: 1px solid #f1f5f9;
}

.last-query-header-left {
  display: flex;
  align-items: center;
  gap: 6px;
}

.last-query-label {
  font-size: 0.82rem;
  font-weight: 700;
  color: #334155;
}

.last-query-link {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  font-size: 0.78rem;
  font-weight: 600;
  color: #64748b;
  text-decoration: none;
  transition: color 0.2s;
}

.last-query-link:hover { color: #0f172a; }

.last-query-body {
  width: 100%;
  background: transparent;
  border: none;
  cursor: pointer;
  font-family: inherit;
  text-align: left;
  padding: 4px 0;
  transition: opacity 0.15s;
}

.last-query-body:hover { opacity: 0.8; }

.last-query-badges {
  display: flex;
  align-items: center;
  gap: 6px;
  margin-bottom: 6px;
  flex-wrap: wrap;
}

.lq-brand {
  font-size: 0.75rem;
  font-weight: 700;
  color: #0f172a;
  background: #f1f5f9;
  padding: 2px 8px;
  border-radius: 6px;
}

.lq-code {
  font-size: 0.75rem;
  font-weight: 600;
  color: #475569;
  font-family: 'SF Mono', 'Fira Code', monospace;
}

.last-query-question {
  margin: 0 0 6px;
  font-size: 0.85rem;
  font-weight: 500;
  color: #1e293b;
  line-height: 1.4;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.last-query-meta {
  display: flex;
  align-items: center;
  gap: 12px;
}

.lq-date {
  font-size: 0.72rem;
  color: #94a3b8;
}

.lq-credits {
  font-size: 0.72rem;
  color: #64748b;
  font-weight: 600;
}

/* Footer */
.ai-footer {
  position: relative;
  z-index: 1;
  text-align: center;
  padding-top: 16px;
}

.ai-footer p {
  font-size: 0.8rem;
  color: #94a3b8;
  margin: 0;
}

/* Responsive */
@media (max-width: 768px) {
  .ai-dashboard {
    padding: 20px 16px 40px;
  }

  .hero-title {
    font-size: 1.5rem;
  }

  .template-chips {
    flex-direction: column;
    align-items: stretch;
    padding: 0 16px;
  }

  .done-stats {
    flex-direction: column;
    gap: 8px;
  }

  .done-actions {
    flex-direction: column;
  }

  .chat-bubble {
    max-width: 90%;
  }

  .last-query-card {
    width: 100%;
  }
}

/* ── Makine Picker ── */
.machine-picker {
  position: relative;
  margin-bottom: 10px;
}

.machine-picker--chat {
  margin-bottom: 8px;
}

/* Trigger button */
.machine-picker-trigger {
  width: 100%;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
  padding: 10px 14px;
  background: white;
  border: 1.5px solid #e2e8f0;
  border-radius: 12px;
  cursor: pointer;
  transition: border-color 0.18s, box-shadow 0.18s;
  font-family: inherit;
  box-shadow: 0 1px 4px rgba(15,23,42,0.04);
}

.machine-picker-trigger:hover {
  border-color: #cbd5e1;
  box-shadow: 0 2px 8px rgba(15,23,42,0.07);
}

.machine-picker-trigger--open {
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59,130,246,0.12);
}

.machine-picker-trigger--selected {
  border-color: #86efac;
  background: #f0fdf4;
}

.machine-picker-trigger--selected.machine-picker-trigger--open {
  border-color: #22c55e;
  box-shadow: 0 0 0 3px rgba(34,197,94,0.12);
}

.machine-picker-trigger-left {
  display: flex;
  align-items: center;
  gap: 10px;
  min-width: 0;
}

.machine-picker-icon {
  width: 28px;
  height: 28px;
  border-radius: 8px;
  background: #f1f5f9;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  transition: background 0.18s;
}

.machine-picker-icon--selected {
  background: #16a34a;
}

.machine-picker-text {
  min-width: 0;
  text-align: left;
}

.machine-picker-placeholder {
  font-size: 0.875rem;
  color: #94a3b8;
}

.machine-picker-opt {
  font-size: 0.78rem;
  color: #cbd5e1;
}

.machine-picker-value {
  font-size: 0.875rem;
  color: #166534;
}

.machine-picker-name {
  color: #4ade80;
  font-size: 0.8rem;
}

.machine-picker-trigger-right {
  display: flex;
  align-items: center;
  gap: 6px;
  flex-shrink: 0;
}

.machine-picker-penalty {
  display: flex;
  align-items: center;
  gap: 2px;
  font-size: 0.72rem;
  font-weight: 600;
  color: #d97706;
  background: #fef3c7;
  border-radius: 5px;
  padding: 2px 6px;
}

.machine-picker-chevron {
  transition: transform 0.2s;
}

.machine-picker-chevron--open {
  transform: rotate(180deg);
}

/* Dropdown */
.machine-picker-dropdown {
  position: absolute;
  top: calc(100% + 6px);
  left: 0;
  right: 0;
  background: white;
  border: 1.5px solid #e2e8f0;
  border-radius: 14px;
  box-shadow: 0 8px 24px rgba(15,23,42,0.12), 0 2px 8px rgba(15,23,42,0.06);
  z-index: 100;
  overflow: hidden;
}

.machine-picker-dropdown--up {
  top: auto;
  bottom: calc(100% + 6px);
}

.machine-picker-list {
  max-height: 220px;
  overflow-y: auto;
  padding: 6px;
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.machine-picker-list::-webkit-scrollbar { width: 4px; }
.machine-picker-list::-webkit-scrollbar-track { background: transparent; }
.machine-picker-list::-webkit-scrollbar-thumb { background: #e2e8f0; border-radius: 2px; }

.machine-picker-item {
  width: 100%;
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 8px 10px;
  border: none;
  border-radius: 9px;
  background: none;
  cursor: pointer;
  transition: background 0.14s;
  text-align: left;
  font-family: inherit;
}

.machine-picker-item:hover {
  background: #f8fafc;
}

.machine-picker-item--active {
  background: #f0fdf4;
}

.machine-picker-item--none .machine-picker-item-label {
  color: #94a3b8;
  font-size: 0.83rem;
  flex: 1;
}

.machine-picker-item-icon {
  width: 26px;
  height: 26px;
  border-radius: 7px;
  background: #f1f5f9;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  transition: background 0.14s;
}

.machine-picker-item-icon--sel {
  background: #16a34a;
}

.machine-picker-item-info {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 1px;
}

.machine-picker-item-brand {
  font-size: 0.855rem;
  font-weight: 600;
  color: #1e293b;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.machine-picker-item-name {
  font-size: 0.77rem;
  color: #94a3b8;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.machine-picker-item-penalty {
  display: flex;
  align-items: center;
  gap: 1px;
  font-size: 0.72rem;
  font-weight: 600;
  color: #d97706;
  background: #fef3c7;
  border-radius: 4px;
  padding: 1px 5px;
  flex-shrink: 0;
}

.machine-picker-check {
  flex-shrink: 0;
}

.machine-picker-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 6px;
  padding: 20px 16px;
  color: #94a3b8;
  font-size: 0.83rem;
}

.machine-picker-add-btn {
  display: flex;
  align-items: center;
  gap: 3px;
  font-size: 0.8rem;
  color: white;
  background: #2563eb;
  border-radius: 7px;
  padding: 5px 12px;
  text-decoration: none;
  transition: background 0.14s;
}

.machine-picker-add-btn:hover {
  background: #1d4ed8;
}

/* Dropdown açılma animasyonu */
.picker-drop-enter-active,
.picker-drop-leave-active {
  transition: opacity 0.15s, transform 0.15s;
}

.picker-drop-enter-from,
.picker-drop-leave-to {
  opacity: 0;
  transform: translateY(-6px) scale(0.98);
}

.machine-picker-dropdown--up.picker-drop-enter-from,
.machine-picker-dropdown--up.picker-drop-leave-to {
  transform: translateY(6px) scale(0.98);
}
</style>
