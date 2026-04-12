<template>
  <div class="mb-6">
    <BreadCrumb :items="[
      { text: 'Ana Sayfa', to: '/' },
      { text: 'Makinelerim' }
    ]" />
  </div>

  <v-row>
    <!-- Sol: Katalog -->
    <v-col cols="12" md="5">
      <v-card rounded="lg" elevation="0" border>
        <v-card-title class="d-flex align-center gap-2 pa-4">
          <v-icon color="primary">mdi-format-list-bulleted</v-icon>
          <span class="text-subtitle-1 font-weight-semibold">Makine Kataloğu</span>
        </v-card-title>
        <v-divider />

        <v-card-text class="pa-3">
          <v-text-field
            v-model="catalogSearch"
            placeholder="Marka veya model ara..."
            prepend-inner-icon="mdi-magnify"
            variant="outlined"
            density="compact"
            hide-details
            class="mb-3"
            clearable
          />

          <div v-if="isCatalogLoading" class="d-flex justify-center py-6">
            <v-progress-circular indeterminate color="primary" size="32" />
          </div>

          <div v-else-if="filteredCatalog.length === 0" class="text-center py-6 text-medium-emphasis">
            <v-icon size="40" class="mb-2">mdi-robot-industrial-outline</v-icon>
            <div class="text-body-2">Katalog boş veya arama sonucu bulunamadı</div>
          </div>

          <v-list v-else density="compact" class="catalog-list">
            <v-list-item
              v-for="machine in filteredCatalog"
              :key="machine.id"
              rounded="lg"
              class="mb-1"
              :disabled="isAlreadyAdded(machine.id)"
            >
              <template #prepend>
                <v-icon color="primary" size="20">mdi-robot-industrial</v-icon>
              </template>

              <v-list-item-title class="text-body-2 font-weight-medium">
                {{ machine.brand }} {{ machine.model }}
              </v-list-item-title>

              <template #append>
                <v-chip
                  v-if="isAlreadyAdded(machine.id)"
                  size="x-small"
                  color="success"
                  variant="tonal"
                >
                  Eklendi
                </v-chip>
                <v-btn
                  v-else
                  size="small"
                  variant="tonal"
                  color="primary"
                  icon="mdi-plus"
                  :loading="addingId === machine.id"
                  @click="openAddDialog(machine)"
                />
              </template>
            </v-list-item>
          </v-list>
        </v-card-text>
      </v-card>
    </v-col>

    <!-- Sağ: Kullanıcının makineleri -->
    <v-col cols="12" md="7">
      <v-card rounded="lg" elevation="0" border>
        <v-card-title class="d-flex align-center gap-2 pa-4">
          <v-icon color="success">mdi-check-circle-outline</v-icon>
          <span class="text-subtitle-1 font-weight-semibold">Kayıtlı Makinelerim</span>
          <v-spacer />
          <v-chip size="small" color="primary" variant="tonal">
            {{ myMachines.length }} makine
          </v-chip>
        </v-card-title>
        <v-divider />

        <v-card-text class="pa-3">
          <div v-if="isMyLoading" class="d-flex justify-center py-6">
            <v-progress-circular indeterminate color="primary" size="32" />
          </div>

          <div v-else-if="myMachines.length === 0" class="text-center py-8 text-medium-emphasis">
            <v-icon size="48" class="mb-3 opacity-40">mdi-robot-industrial-outline</v-icon>
            <div class="text-body-1 font-weight-medium mb-1">Henüz makine eklenmedi</div>
            <div class="text-body-2">Sol taraftaki katalogdan makine seçerek listenize ekleyin</div>
          </div>

          <div v-else>
            <v-card
              v-for="um in myMachines"
              :key="um.id"
              variant="tonal"
              color="primary"
              rounded="lg"
              class="mb-2 pa-3"
            >
              <div class="d-flex align-center gap-3">
                <v-icon color="primary">mdi-robot-industrial</v-icon>
                <div class="flex-grow-1">
                  <div class="text-body-2 font-weight-semibold">{{ um.brand }} {{ um.model }}</div>
                  <div v-if="um.name" class="text-caption text-medium-emphasis">{{ um.name }}</div>
                </div>
                <v-btn
                  size="small"
                  variant="text"
                  color="error"
                  icon="mdi-close"
                  :loading="removingId === um.id"
                  @click="confirmRemove(um)"
                />
              </div>
            </v-card>
          </div>
        </v-card-text>
      </v-card>
    </v-col>
  </v-row>

  <!-- Add dialog: makine adı ver (opsiyonel) -->
  <v-dialog v-model="addDialog" max-width="420" persistent>
    <v-card rounded="lg">
      <v-card-title class="pa-4 d-flex align-center gap-2">
        <v-icon color="primary">mdi-robot-industrial</v-icon>
        <span>Makine Ekle</span>
      </v-card-title>
      <v-divider />
      <v-card-text class="pa-4">
        <div class="text-body-2 font-weight-medium mb-3">
          <v-icon size="16" class="mr-1">mdi-tag</v-icon>
          {{ selectedCatalogMachine?.brand }} {{ selectedCatalogMachine?.model }}
        </div>
        <v-text-field
          v-model="newMachineName"
          label="Makine Adı (opsiyonel)"
          placeholder="Fabrika 1 - Torna"
          variant="outlined"
          density="comfortable"
          hint="Birden fazla aynı model makineniz varsa ayırt etmek için isim verin"
          persistent-hint
          :maxlength="150"
        />
      </v-card-text>
      <v-card-actions class="px-4 pb-4">
        <v-spacer />
        <v-btn variant="outlined" @click="addDialog = false">İptal</v-btn>
        <v-btn color="primary" :loading="isAdding" @click="handleAdd">Ekle</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>

  <!-- Remove confirm -->
  <ConfirmDialog
    v-model="removeDialog"
    title="Makineyi Listeden Kaldır"
    :message="`'${machineToRemove?.brand} ${machineToRemove?.model}' makinesini listenizden kaldırmak istediğinizden emin misiniz?`"
    type="error"
    confirm-text="Kaldır"
    :loading="isRemoving"
    @confirm="handleRemove"
    @cancel="removeDialog = false"
  />
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import type { Machine, UserMachine } from '~/types'
import ConfirmDialog from '~/components/UI/ConfirmDialog.vue'

definePageMeta({
  middleware: ['auth', 'permission'],
  permission: 'UserMachines.Read'
})

useHead({ title: 'Makinelerim - CassMach' })

//#region Composables
const { getMachines } = useMachines()
const { getMyMachines, addMachine, removeMachine } = useUserMachines()
const { $toast } = useNuxtApp() as any
//#endregion

//#region State
const catalog = ref<Machine[]>([])
const myMachines = ref<UserMachine[]>([])
const catalogSearch = ref('')
const isCatalogLoading = ref(false)
const isMyLoading = ref(false)
const addingId = ref<number | null>(null)
const removingId = ref<number | null>(null)
const isAdding = ref(false)
const isRemoving = ref(false)

const addDialog = ref(false)
const removeDialog = ref(false)
const selectedCatalogMachine = ref<Machine | null>(null)
const machineToRemove = ref<UserMachine | null>(null)
const newMachineName = ref('')
//#endregion

//#region Computed
const filteredCatalog = computed(() => {
  const q = catalogSearch.value?.toLowerCase() ?? ''
  if (!q) return catalog.value
  return catalog.value.filter(m =>
    m.brand.toLowerCase().includes(q) || m.model.toLowerCase().includes(q)
  )
})

const isAlreadyAdded = (machineId: number) => {
  return myMachines.value.some(um => um.machineId === machineId)
}
//#endregion

//#region Actions
const openAddDialog = (machine: Machine) => {
  selectedCatalogMachine.value = machine
  newMachineName.value = ''
  addDialog.value = true
}

const handleAdd = async () => {
  if (!selectedCatalogMachine.value) return
  isAdding.value = true
  try {
    await addMachine({
      machineId: selectedCatalogMachine.value.id,
      name: newMachineName.value?.trim() || undefined
    })
    myMachines.value = await getMyMachines()
    addDialog.value = false
    $toast?.success('Makine listenize eklendi')
  } catch {
    $toast?.error('Makine eklenirken hata oluştu')
  } finally {
    isAdding.value = false
  }
}

const confirmRemove = (um: UserMachine) => {
  machineToRemove.value = um
  removeDialog.value = true
}

const handleRemove = async () => {
  if (!machineToRemove.value) return
  removingId.value = machineToRemove.value.id
  isRemoving.value = true
  try {
    await removeMachine(machineToRemove.value.id)
    myMachines.value = myMachines.value.filter(m => m.id !== machineToRemove.value!.id)
    removeDialog.value = false
    $toast?.success('Makine listeden kaldırıldı')
  } catch {
    $toast?.error('Makine kaldırılırken hata oluştu')
  } finally {
    isRemoving.value = false
    removingId.value = null
  }
}
//#endregion

//#region Lifecycle
onMounted(async () => {
  isCatalogLoading.value = true
  isMyLoading.value = true
  try {
    const [cat, mine] = await Promise.all([getMachines(), getMyMachines()])
    catalog.value = cat
    myMachines.value = mine
  } finally {
    isCatalogLoading.value = false
    isMyLoading.value = false
  }
})
//#endregion
</script>

<style scoped>
.catalog-list {
  max-height: 500px;
  overflow-y: auto;
}
</style>
