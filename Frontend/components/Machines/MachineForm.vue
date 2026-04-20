<template>
  <!-- Tab Navigation -->
  <div class="tabs-container">
    <button
      v-for="tab in tabs"
      :key="tab.value"
      :class="['tab-item', { active: currentTab === tab.value }]"
      @click="currentTab = tab.value"
      :disabled="loading"
    >
      <div class="tab-icon-wrapper">
        <v-icon :icon="tab.icon" size="22" />
      </div>
      <span class="tab-label">{{ tab.label }}</span>
    </button>
  </div>

  <v-form ref="formRef">
    <v-window v-model="currentTab" class="mt-6">
      <!-- Tab: Makine Bilgileri -->
      <v-window-item value="info">
        <v-container>
          <v-row>
            <v-col cols="12">
              <v-text-field
                v-model="formData.brand"
                label="Marka"
                placeholder="Fanuc, Siemens, Haas..."
                variant="outlined"
                :disabled="loading"
                prepend-inner-icon="mdi-factory"
                density="comfortable"
                hide-details="auto"
                :rules="[rules.required, rules.maxLength(100)]"
                class="modern-input"
              />
            </v-col>
          </v-row>

          <v-row>
            <v-col cols="12">
              <v-text-field
                v-model="formData.model"
                label="Model"
                placeholder="30i-B, 840D sl, VF-2..."
                variant="outlined"
                :disabled="loading"
                prepend-inner-icon="mdi-robot-industrial"
                density="comfortable"
                hide-details="auto"
                :rules="[rules.required, rules.maxLength(100)]"
                class="modern-input"
              />
            </v-col>
          </v-row>

        </v-container>
      </v-window-item>
    </v-window>

    <!-- Actions -->
    <div class="form-actions">
      <v-btn
        variant="outlined"
        size="large"
        @click="$emit('cancel')"
        :disabled="loading"
        class="btn-gradient-dark"
      >
        İptal
      </v-btn>

      <v-btn
        size="large"
        :loading="loading"
        :disabled="loading"
        @click="handleSubmit"
        class="btn-gradient-primary"
      >
        {{ machine ? 'Güncelle' : 'Kaydet' }}
      </v-btn>
    </div>
  </v-form>
</template>

<script setup lang="ts">
import { ref, reactive, watch } from 'vue'
import type { Machine } from '~/types'

const props = defineProps<{
  machine?: Machine | null
  loading?: boolean
}>()

const emit = defineEmits<{
  submit: [data: any]
  cancel: []
}>()

const formRef = ref()
const currentTab = ref('info')

const tabs = [
  { value: 'info', label: 'Makine Bilgileri', icon: 'mdi-robot-industrial' }
]

const rules = {
  required: (v: any) => !!v?.trim() || 'Bu alan zorunludur',
  maxLength: (max: number) => (v: any) => !v || v.length <= max || `En fazla ${max} karakter`,
}

const formData = reactive({
  brand: '',
  model: '',
})

watch(() => props.machine, (val) => {
  if (val) {
    Object.assign(formData, {
      brand: val.brand || '',
      model: val.model || '',
    })
  } else {
    Object.assign(formData, {
      brand: '',
      model: '',
    })
  }
}, { immediate: true })

const handleSubmit = async () => {
  const { valid } = await formRef.value?.validate()
  if (!valid) return

  emit('submit', {
    brand: formData.brand.trim(),
    model: formData.model.trim(),
  })
}
</script>

