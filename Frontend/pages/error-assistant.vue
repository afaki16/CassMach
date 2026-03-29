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
        <div class="template-chips">
          <button
            v-for="tpl in templateQuestions"
            :key="tpl.query"
            class="template-chip"
            @click="searchQuery = tpl.query"
          >
            <v-icon size="14" :color="tpl.color">{{ tpl.icon }}</v-icon>
            <span>{{ tpl.title }}</span>
          </button>
        </div>
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

          <!-- System Message (parse result, errors) -->
          <div v-if="msg.role === 'system'" class="chat-system-msg">
            <div v-if="msg.meta?.type === 'parse'" class="parse-result">
              <v-icon size="16" color="success">mdi-check-circle</v-icon>
              <span>
                <strong>{{ msg.meta.brand }}</strong> — Hata Kodu: <strong>{{ msg.meta.errorCode }}</strong>
                <span v-if="msg.meta.model"> ({{ msg.meta.model }})</span>
              </span>
            </div>
            <div v-else-if="msg.meta?.type === 'error'" class="error-msg">
              <v-icon size="16" color="error">mdi-alert-circle</v-icon>
              <span>{{ msg.content }}</span>
            </div>
            <div v-else-if="msg.meta?.type === 'done'" class="done-msg">
              <div class="done-stats">
                <span class="done-stat">
                  <v-icon size="14">mdi-circle-multiple-outline</v-icon>
                  {{ msg.meta.creditsCharged?.toFixed(2) }} kredi kullanıldı
                </span>
                <span class="done-stat">
                  <v-icon size="14">mdi-wallet-outline</v-icon>
                  Kalan: {{ msg.meta.remainingBalance?.toFixed(1) }}
                </span>
                <span class="done-stat">
                  <v-icon size="14">mdi-refresh</v-icon>
                  {{ msg.meta.remainingRetries }} deneme hakkı
                </span>
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
                  class="action-btn action-btn--accept"
                  @click="handleAccept(msg.meta.conversationId, msg.meta.attempt)"
                  :disabled="isLoading"
                >
                  <v-icon size="16">mdi-check</v-icon>
                  Kabul Et
                </button>
              </div>
            </div>
          </div>

          <!-- Chat Bubble -->
          <div v-else class="chat-bubble">
            <p class="chat-text" v-html="formatMessage(msg.content)"></p>
          </div>
        </div>

        <!-- Loading Indicator -->
        <div v-if="isLoading" class="chat-message chat-message--assistant">
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
      <p>CassMach AI — Makine Hata Kodu Asistanı</p>
    </div>

    <!-- Accept Success Snackbar -->
    <v-snackbar v-model="showSnackbar" :color="snackbarColor" :timeout="3000" location="top">
      {{ snackbarText }}
    </v-snackbar>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, nextTick, watch, onMounted, onBeforeUnmount } from 'vue'
import { useAuthStore } from '~/stores/auth'
import { API_ENDPOINTS } from '~/utils/apiEndpoints'

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

const showSnackbar = ref(false)
const snackbarText = ref('')
const snackbarColor = ref('success')

const templateQuestions = [
  { icon: 'mdi-alert-circle-outline', title: 'Fanuc Alarm 414', color: 'red', query: 'Fanuc alarm 414 nedir ve nasıl çözülür?' },
  { icon: 'mdi-wrench-outline', title: 'Siemens F30001', color: 'blue', query: 'Siemens Sinumerik F30001 hata kodu çözümü' },
  { icon: 'mdi-cog-outline', title: 'Haas Alarm 108', color: 'amber', query: 'Haas CNC alarm 108 ne anlama gelir?' },
  { icon: 'mdi-chart-line', title: 'Mitsubishi E61', color: 'purple', query: 'Mitsubishi CNC E61 servo alarm nasıl giderilir?' },
]

const { get, post, patch } = useApi()

const fetchBalance = async () => {
  try {
    const res = await get<{ balance: number }>(API_ENDPOINTS.ERRORS.BALANCE)
    tokenBalance.value = res.data?.balance ?? null
  } catch {
    // silent
  }
}

const formatMessage = (text: string) => {
  if (!text) return ''
  return text
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>')
    .replace(/\n/g, '<br>')
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
      messages.value.push({
        role: 'system',
        content: '',
        meta: { type: 'parse', brand: event.brand, errorCode: event.errorCode, model: event.model }
      })
      scrollToBottom()
      break

    case 'chunk':
      currentStreamText.value += event.text
      const lastAssistant = messages.value.findLast(m => m.role === 'assistant')
      if (lastAssistant) {
        lastAssistant.content = currentStreamText.value
      } else {
        messages.value.push({ role: 'assistant', content: currentStreamText.value })
      }
      scrollToBottom()
      break

    case 'done':
      tokenBalance.value = event.remainingBalance
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
        role: 'system',
        content: event.message,
        meta: { type: 'error' }
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
  scrollToBottom()

  isLoading.value = true
  try {
    await streamSSE(API_ENDPOINTS.ERRORS.ASK, { question: query })
  } catch (error: any) {
    messages.value.push({
      role: 'system',
      content: 'Bağlantı hatası oluştu. Lütfen tekrar deneyin.',
      meta: { type: 'error' }
    })
  } finally {
    isLoading.value = false
    scrollToBottom()
    nextTick(() => textareaRef.value?.focus())
  }
}

const handleRetry = async (conversationId: string) => {
  currentStreamText.value = ''
  isLoading.value = true
  try {
    await streamSSE(API_ENDPOINTS.ERRORS.RETRY(conversationId))
  } catch (error: any) {
    messages.value.push({
      role: 'system',
      content: 'Tekrar denemede hata oluştu.',
      meta: { type: 'error' }
    })
  } finally {
    isLoading.value = false
    scrollToBottom()
  }
}

const handleAccept = async (conversationId: string, attemptNumber: number) => {
  try {
    await patch(API_ENDPOINTS.ERRORS.ACCEPT(conversationId), { attemptNumber })
    snackbarText.value = 'Çözüm kabul edildi!'
    snackbarColor.value = 'success'
    showSnackbar.value = true
  } catch {
    snackbarText.value = 'Kabul işlemi başarısız oldu.'
    snackbarColor.value = 'error'
    showSnackbar.value = true
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

.parse-result {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px 16px;
  background: #f0fdf4;
  border: 1px solid #bbf7d0;
  border-radius: 12px;
  font-size: 0.85rem;
  color: #166534;
}

.error-msg {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px 16px;
  background: #fef2f2;
  border: 1px solid #fecaca;
  border-radius: 12px;
  font-size: 0.85rem;
  color: #991b1b;
}

.done-msg {
  padding: 14px 18px;
  background: white;
  border: 1px solid #e2e8f0;
  border-radius: 14px;
  box-shadow: 0 2px 8px rgba(15, 23, 42, 0.04);
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
  gap: 10px;
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
}
</style>
