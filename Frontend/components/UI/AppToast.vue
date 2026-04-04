<template>
  <div class="app-toast-container">
    <TransitionGroup name="app-toast">
      <div
        v-for="item in toastStore.items"
        :key="item.id"
        :class="['app-toast-item', `app-toast-${item.type}`]"
      >
        <div class="app-toast-glow" aria-hidden="true" />
        <div class="app-toast-accent" />

        <div class="app-toast-icon">
          <v-icon :size="22">{{ iconMap[item.type] }}</v-icon>
        </div>

        <div class="app-toast-body">
          <span class="app-toast-title">{{ item.title }}</span>
          <ul v-if="item.messages.length > 1" class="app-toast-messages">
            <li v-for="(msg, i) in item.messages" :key="i">{{ msg }}</li>
          </ul>
          <p v-else class="app-toast-message">{{ item.messages[0] }}</p>
        </div>

        <button
          type="button"
          class="app-toast-close"
          :aria-label="'Kapat'"
          @click="toastStore.remove(item.id)"
        >
          <v-icon size="16">mdi-close</v-icon>
        </button>

        <div
          v-if="item.duration > 0"
          class="app-toast-progress"
          :style="{ animationDuration: `${item.duration}ms` }"
        />
      </div>
    </TransitionGroup>
  </div>
</template>

<script setup lang="ts">
import { useToastStore } from '~/stores/toast'

const toastStore = useToastStore()

const iconMap: Record<string, string> = {
  success: 'mdi-check-decagram',
  error: 'mdi-alert-octagon-outline',
  warning: 'mdi-alert-outline',
  info: 'mdi-information-outline',
}
</script>

<style>
.app-toast-container {
  position: fixed;
  top: 24px;
  right: 24px;
  z-index: 99999;
  display: flex;
  flex-direction: column;
  gap: 14px;
  pointer-events: none;
  max-width: 400px;
  width: calc(100% - 48px);
}

.app-toast-item {
  position: relative;
  display: flex;
  align-items: flex-start;
  gap: 14px;
  padding: 17px 18px 18px;
  border-radius: 16px;
  background: linear-gradient(
    145deg,
    rgba(28, 28, 38, 0.97) 0%,
    rgba(18, 18, 26, 0.94) 100%
  );
  backdrop-filter: blur(28px) saturate(1.35);
  -webkit-backdrop-filter: blur(28px) saturate(1.35);
  border: 1px solid rgba(255, 255, 255, 0.08);
  box-shadow:
    0 4px 6px rgba(0, 0, 0, 0.15),
    0 22px 48px rgba(0, 0, 0, 0.45),
    0 0 0 1px rgba(255, 255, 255, 0.04) inset;
  overflow: hidden;
  pointer-events: all;
  font-family: 'Inter', system-ui, sans-serif;
}

.app-toast-glow {
  position: absolute;
  inset: -40% -20% auto -20%;
  height: 80px;
  border-radius: 50%;
  opacity: 0.14;
  filter: blur(28px);
  pointer-events: none;
}

.app-toast-success .app-toast-glow { background: #10b981; }
.app-toast-error   .app-toast-glow { background: #ef4444; }
.app-toast-warning .app-toast-glow { background: #f59e0b; }
.app-toast-info    .app-toast-glow { background: #6366f1; }

.app-toast-accent {
  position: absolute;
  left: 0;
  top: 0;
  bottom: 0;
  width: 4px;
  border-radius: 4px 0 0 4px;
  box-shadow: 2px 0 12px rgba(0, 0, 0, 0.2);
}

.app-toast-success .app-toast-accent { background: linear-gradient(180deg, #6ee7b7 0%, #10b981 55%, #059669 100%); }
.app-toast-error   .app-toast-accent { background: linear-gradient(180deg, #fca5a5 0%, #ef4444 55%, #dc2626 100%); }
.app-toast-warning .app-toast-accent { background: linear-gradient(180deg, #fde047 0%, #f59e0b 55%, #d97706 100%); }
.app-toast-info    .app-toast-accent { background: linear-gradient(180deg, #a5b4fc 0%, #6366f1 55%, #4f46e5 100%); }

.app-toast-success {
  border-color: rgba(52, 211, 153, 0.18);
  box-shadow:
    0 4px 6px rgba(0, 0, 0, 0.12),
    0 22px 48px rgba(0, 0, 0, 0.42),
    0 0 32px -8px rgba(16, 185, 129, 0.25),
    0 0 0 1px rgba(255, 255, 255, 0.04) inset;
}
.app-toast-error {
  border-color: rgba(248, 113, 113, 0.2);
  box-shadow:
    0 4px 6px rgba(0, 0, 0, 0.12),
    0 22px 48px rgba(0, 0, 0, 0.42),
    0 0 32px -8px rgba(239, 68, 68, 0.22),
    0 0 0 1px rgba(255, 255, 255, 0.04) inset;
}
.app-toast-warning {
  border-color: rgba(251, 191, 36, 0.2);
  box-shadow:
    0 4px 6px rgba(0, 0, 0, 0.12),
    0 22px 48px rgba(0, 0, 0, 0.42),
    0 0 32px -8px rgba(245, 158, 11, 0.2),
    0 0 0 1px rgba(255, 255, 255, 0.04) inset;
}
.app-toast-info {
  border-color: rgba(129, 140, 248, 0.2);
  box-shadow:
    0 4px 6px rgba(0, 0, 0, 0.12),
    0 22px 48px rgba(0, 0, 0, 0.42),
    0 0 32px -8px rgba(99, 102, 241, 0.2),
    0 0 0 1px rgba(255, 255, 255, 0.04) inset;
}

.app-toast-icon {
  flex-shrink: 0;
  width: 40px;
  height: 40px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  position: relative;
  z-index: 1;
}

.app-toast-success .app-toast-icon {
  background: linear-gradient(160deg, rgba(52, 211, 153, 0.22), rgba(16, 185, 129, 0.08));
  color: #6ee7b7;
  box-shadow: 0 0 0 1px rgba(52, 211, 153, 0.15) inset;
}
.app-toast-error .app-toast-icon {
  background: linear-gradient(160deg, rgba(248, 113, 113, 0.22), rgba(239, 68, 68, 0.08));
  color: #fca5a5;
  box-shadow: 0 0 0 1px rgba(248, 113, 113, 0.15) inset;
}
.app-toast-warning .app-toast-icon {
  background: linear-gradient(160deg, rgba(251, 191, 36, 0.22), rgba(245, 158, 11, 0.08));
  color: #fde047;
  box-shadow: 0 0 0 1px rgba(251, 191, 36, 0.15) inset;
}
.app-toast-info .app-toast-icon {
  background: linear-gradient(160deg, rgba(129, 140, 248, 0.22), rgba(99, 102, 241, 0.08));
  color: #a5b4fc;
  box-shadow: 0 0 0 1px rgba(129, 140, 248, 0.15) inset;
}

.app-toast-body {
  flex: 1;
  min-width: 0;
  position: relative;
  z-index: 1;
}

.app-toast-title {
  display: block;
  font-size: 0.875rem;
  font-weight: 600;
  color: rgba(255, 255, 255, 0.96);
  margin-bottom: 5px;
  letter-spacing: 0.01em;
  line-height: 1.35;
}

.app-toast-message {
  font-size: 0.8125rem;
  color: rgba(255, 255, 255, 0.62);
  line-height: 1.55;
  margin: 0;
}

.app-toast-messages {
  list-style: none;
  padding: 0;
  margin: 0;
}

.app-toast-messages li {
  position: relative;
  font-size: 0.8125rem;
  color: rgba(255, 255, 255, 0.62);
  line-height: 1.55;
  padding-left: 14px;
}

.app-toast-messages li::before {
  content: '';
  position: absolute;
  left: 0;
  top: 8px;
  width: 5px;
  height: 5px;
  border-radius: 50%;
}

.app-toast-success .app-toast-messages li::before { background: rgba(52, 211, 153, 0.5); }
.app-toast-error   .app-toast-messages li::before { background: rgba(248, 113, 113, 0.5); }
.app-toast-warning .app-toast-messages li::before { background: rgba(251, 191, 36, 0.5); }
.app-toast-info    .app-toast-messages li::before { background: rgba(129, 140, 248, 0.5); }

.app-toast-close {
  flex-shrink: 0;
  width: 30px;
  height: 30px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  border: none;
  background: rgba(255, 255, 255, 0.04);
  color: rgba(255, 255, 255, 0.38);
  cursor: pointer;
  transition: background 0.2s ease, color 0.2s ease, transform 0.15s ease;
  position: relative;
  z-index: 1;
}

.app-toast-close:hover {
  background: rgba(255, 255, 255, 0.1);
  color: rgba(255, 255, 255, 0.85);
}

.app-toast-close:active {
  transform: scale(0.96);
}

.app-toast-progress {
  position: absolute;
  bottom: 0;
  left: 0;
  height: 3px;
  border-radius: 0 3px 3px 16px;
  animation: appToastShrink linear forwards;
  transform-origin: left center;
}

.app-toast-success .app-toast-progress {
  background: linear-gradient(90deg, rgba(16, 185, 129, 0.35), rgba(52, 211, 153, 0.85));
}
.app-toast-error .app-toast-progress {
  background: linear-gradient(90deg, rgba(239, 68, 68, 0.35), rgba(252, 165, 165, 0.85));
}
.app-toast-warning .app-toast-progress {
  background: linear-gradient(90deg, rgba(245, 158, 11, 0.35), rgba(253, 224, 71, 0.85));
}
.app-toast-info .app-toast-progress {
  background: linear-gradient(90deg, rgba(99, 102, 241, 0.35), rgba(165, 180, 252, 0.85));
}

@keyframes appToastShrink {
  from { width: 100%; }
  to   { width: 0%; }
}

.app-toast-enter-active {
  animation: appToastSlideIn 0.35s cubic-bezier(0.22, 1, 0.36, 1);
}

.app-toast-leave-active {
  animation: appToastSlideOut 0.3s cubic-bezier(0.4, 0, 1, 1) forwards;
}

@keyframes appToastSlideIn {
  from {
    opacity: 0;
    transform: translateX(40px) scale(0.96);
  }
  to {
    opacity: 1;
    transform: translateX(0) scale(1);
  }
}

@keyframes appToastSlideOut {
  from {
    opacity: 1;
    transform: translateX(0) scale(1);
  }
  to {
    opacity: 0;
    transform: translateX(40px) scale(0.96);
  }
}

@media (max-width: 480px) {
  .app-toast-container {
    top: 12px;
    right: 12px;
    left: 12px;
    max-width: none;
    width: auto;
  }
}
</style>
