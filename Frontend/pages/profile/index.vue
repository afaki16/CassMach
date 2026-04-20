<template>
  <v-container class="py-8" max-width="600">
    <div class="page-header mb-6">
      <h1 class="page-title">Profil</h1>
      <p class="page-subtitle">Hesap ayarlarınızı yönetin</p>
    </div>

    <v-card class="modern-card" rounded="xl">
      <v-card-title class="pa-6 pb-2">
        <div class="d-flex align-center gap-3">
          <v-icon icon="mdi-lock-reset" color="primary" size="24" />
          <span class="text-h6 font-weight-semibold">Şifre Değiştir</span>
        </div>
      </v-card-title>

      <v-card-text class="pa-6 pt-4">
        <v-form ref="formRef" @submit.prevent="handleSubmit">
          <v-text-field
            v-model="form.currentPassword"
            label="Mevcut Şifre"
            :type="showCurrent ? 'text' : 'password'"
            variant="outlined"
            density="comfortable"
            class="modern-input mb-3"
            prepend-inner-icon="mdi-lock-outline"
            :append-inner-icon="showCurrent ? 'mdi-eye-off' : 'mdi-eye'"
            @click:append-inner="showCurrent = !showCurrent"
            :rules="[rules.required]"
            hide-details="auto"
            :disabled="loading"
          />

          <v-text-field
            v-model="form.newPassword"
            label="Yeni Şifre"
            :type="showNew ? 'text' : 'password'"
            variant="outlined"
            density="comfortable"
            class="modern-input mb-3"
            prepend-inner-icon="mdi-lock-plus-outline"
            :append-inner-icon="showNew ? 'mdi-eye-off' : 'mdi-eye'"
            @click:append-inner="showNew = !showNew"
            :rules="[rules.required, rules.password]"
            hide-details="auto"
            :disabled="loading"
          />

          <v-text-field
            v-model="form.confirmPassword"
            label="Yeni Şifre (Tekrar)"
            :type="showConfirm ? 'text' : 'password'"
            variant="outlined"
            density="comfortable"
            class="modern-input"
            prepend-inner-icon="mdi-lock-check-outline"
            :append-inner-icon="showConfirm ? 'mdi-eye-off' : 'mdi-eye'"
            @click:append-inner="showConfirm = !showConfirm"
            :rules="[rules.required, rules.confirmPassword(form.newPassword)]"
            hide-details="auto"
            :disabled="loading"
          />

          <div class="d-flex justify-end mt-6">
            <v-btn
              type="submit"
              size="large"
              :loading="loading"
              :disabled="loading"
              class="btn-gradient-primary"
            >
              Şifreyi Güncelle
            </v-btn>
          </div>
        </v-form>
      </v-card-text>
    </v-card>
  </v-container>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue'
import { useAuth } from '~/composables/useAuth'
import { useValidators } from '~/composables/useValidators'
import { useToastStore } from '~/stores/toast'

const { changePassword } = useAuth()
const { validationRules } = useValidators()
const toastStore = useToastStore()

const formRef = ref()
const loading = ref(false)
const showCurrent = ref(false)
const showNew = ref(false)
const showConfirm = ref(false)

const form = reactive({
  currentPassword: '',
  newPassword: '',
  confirmPassword: '',
})

const rules = {
  required: validationRules.required,
  password: validationRules.password,
  confirmPassword: validationRules.confirmPassword,
}

const handleSubmit = async () => {
  const { valid } = await formRef.value?.validate()
  if (!valid) return

  loading.value = true
  try {
    await changePassword(form.currentPassword, form.newPassword)
    toastStore.add('success', 'Şifreniz başarıyla güncellendi')
    formRef.value?.reset()
  } catch (error: any) {
    const message = error?.response?.data?.errors?.[0]?.message
      ?? error?.message
      ?? 'Şifre güncellenirken bir hata oluştu'
    toastStore.add('error', message)
  } finally {
    loading.value = false
  }
}
</script>
