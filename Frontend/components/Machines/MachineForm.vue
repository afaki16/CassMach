<template>
  <v-form ref="formRef" @submit.prevent="submitForm">
    <v-card flat>
      <v-card-text class="pa-4">
        <v-text-field
          v-model="formData.brand"
          label="Marka *"
          placeholder="Fanuc, Siemens, Haas..."
          :rules="[rules.required, rules.maxLength(100)]"
          variant="outlined"
          density="comfortable"
          class="mb-2"
        />

        <v-text-field
          v-model="formData.model"
          label="Model *"
          placeholder="30i-B, 840D sl, VF-2..."
          :rules="[rules.required, rules.maxLength(100)]"
          variant="outlined"
          density="comfortable"
        />
      </v-card-text>

      <v-card-actions class="px-4 pb-4">
        <v-spacer />
        <v-btn variant="outlined" @click="$emit('cancel')">İptal</v-btn>
        <v-btn color="primary" type="submit" :loading="loading">
          {{ machine ? 'Güncelle' : 'Ekle' }}
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-form>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
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

const formData = ref({
  brand: '',
  model: '',
})

const rules = {
  required: (v: any) => !!v?.trim() || 'Bu alan zorunludur',
  maxLength: (max: number) => (v: any) => !v || v.length <= max || `En fazla ${max} karakter`,
}

watch(() => props.machine, (val) => {
  if (val) {
    formData.value = {
      brand: val.brand || '',
      model: val.model || '',
    }
  } else {
    formData.value = { brand: '', model: '' }
  }
}, { immediate: true })

const submitForm = async () => {
  const { valid } = await formRef.value.validate()
  if (!valid) return
  emit('submit', {
    data: {
      brand: formData.value.brand.trim(),
      model: formData.value.model.trim(),
    }
  })
}
</script>
