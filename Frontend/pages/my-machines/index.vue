<template>
  <!-- Breadcrumb -->
  <div class="mb-6">
    <BreadCrumb :items="[
      { text: 'Ana Sayfa', to: '/' },
      { text: 'Makinelerim' }
    ]" />
  </div>

  <!-- Page Header -->
  <div class="page-header mb-6">
    <div class="page-header-left">
      <div class="page-header-icon">
        <v-icon size="28" color="white">mdi-robot-industrial</v-icon>
      </div>
      <div>
        <h1 class="page-title">Makinelerim</h1>
        <p class="page-subtitle">Katalogdan makine seçerek listenizi oluşturun</p>
      </div>
    </div>
    <div class="page-header-stats">
      <div class="stat-card">
        <span class="stat-number">{{ myMachines.length }}</span>
        <span class="stat-label">Kayıtlı</span>
      </div>
      <div class="stat-card">
        <span class="stat-number">{{ brandGroups.length }}</span>
        <span class="stat-label">Marka</span>
      </div>
      <div class="stat-card">
        <span class="stat-number">{{ catalog.length }}</span>
        <span class="stat-label">Model</span>
      </div>
    </div>
  </div>

  <!-- Main Layout -->
  <div class="machines-layout">

    <!-- Sol: Katalog (Split View) -->
    <div class="panel catalog-panel">
      <div class="panel-header">
        <div class="panel-header-left">
          <div class="panel-icon catalog-icon">
            <v-icon size="20" color="white">mdi-format-list-bulleted</v-icon>
          </div>
          <div>
            <h2 class="panel-title">Makine Kataloğu</h2>
            <p class="panel-subtitle" v-if="selectedBrand">
              {{ selectedBrand }} · {{ filteredModels.length }} model
            </p>
            <p class="panel-subtitle" v-else>
              Marka seçin
            </p>
          </div>
        </div>
      </div>

      <!-- Split View Body -->
      <div class="catalog-body" v-if="!isCatalogLoading">

        <!-- Sol: Marka Şeridi -->
        <div class="brands-sidebar">
          <div class="brands-search">
            <v-text-field
              v-model="brandSearch"
              placeholder="Marka ara..."
              prepend-inner-icon="mdi-magnify"
              variant="outlined"
              density="compact"
              hide-details
              clearable
              class="modern-input"
            />
          </div>
          <div class="brands-list">
            <div
              v-if="filteredBrandGroups.length === 0"
              class="brands-empty"
            >
              Sonuç yok
            </div>
            <button
              v-for="bg in filteredBrandGroups"
              :key="bg.brand"
              class="brand-item"
              :class="{ active: selectedBrand === bg.brand }"
              @click="selectBrand(bg.brand)"
            >
              <div class="brand-item-avatar">
                {{ bg.brand.charAt(0).toUpperCase() }}
              </div>
              <div class="brand-item-info">
                <span class="brand-item-name">{{ bg.brand }}</span>
                <span class="brand-item-meta">{{ bg.models.length }} model</span>
              </div>
              <div class="brand-item-badges">
                <span
                  v-if="addedCountByBrand(bg.brand) > 0"
                  class="brand-added-dot"
                  :title="`${addedCountByBrand(bg.brand)} model eklendi`"
                >
                  {{ addedCountByBrand(bg.brand) }}
                </span>
                <v-icon v-if="selectedBrand === bg.brand" size="14" color="white">
                  mdi-chevron-right
                </v-icon>
              </div>
            </button>
          </div>
        </div>

        <!-- Sağ: Model Listesi -->
        <div class="models-area">
          <!-- Seçim yapılmamış -->
          <div v-if="!selectedBrand" class="models-placeholder">
            <v-icon size="40" class="mb-3">mdi-cursor-pointer</v-icon>
            <p class="empty-title">Bir marka seçin</p>
            <p class="empty-text">Sol listeden marka seçerek modellerini görüntüleyin</p>
          </div>

          <template v-else>
            <!-- Model Arama -->
            <div class="models-search">
              <v-text-field
                v-model="modelSearch"
                :placeholder="`${selectedBrand} modellerinde ara...`"
                prepend-inner-icon="mdi-magnify"
                variant="outlined"
                density="compact"
                hide-details
                clearable
                class="modern-input"
              />
              <div class="models-meta">
                <span class="models-count">
                  {{ filteredModels.length }} / {{ currentBrandModels.length }} model
                </span>
                <span
                  v-if="addedCountByBrand(selectedBrand) > 0"
                  class="models-added-count"
                >
                  <v-icon size="12">mdi-check-circle</v-icon>
                  {{ addedCountByBrand(selectedBrand) }} ekli
                </span>
              </div>
            </div>

            <!-- Model Öğeleri -->
            <div v-if="filteredModels.length === 0" class="models-empty">
              <v-icon size="32" class="mb-2">mdi-magnify-remove-outline</v-icon>
              <p>Model bulunamadı</p>
            </div>

            <div v-else class="models-list">
              <div
                v-for="machine in paginatedModels"
                :key="machine.id"
                class="model-item"
                :class="{ 'model-item--added': isAlreadyAdded(machine.id) }"
              >
                <div class="model-item-icon">
                  <v-icon size="14" :color="isAlreadyAdded(machine.id) ? 'success' : 'primary'">
                    mdi-cog
                  </v-icon>
                </div>
                <span class="model-item-name">{{ machine.model }}</span>
                <div class="model-item-action">
                  <v-chip v-if="isAlreadyAdded(machine.id)" size="x-small" color="success" variant="tonal">
                    <v-icon start size="10">mdi-check</v-icon>
                    Ekli
                  </v-chip>
                  <button
                    v-else
                    class="add-btn"
                    :disabled="addingId === machine.id"
                    @click="openAddDialog(machine)"
                  >
                    <v-progress-circular v-if="addingId === machine.id" indeterminate size="12" width="2" color="white" />
                    <v-icon v-else size="15">mdi-plus</v-icon>
                  </button>
                </div>
              </div>
            </div>

            <!-- Sayfalama -->
            <div v-if="totalPages > 1" class="models-pagination">
              <button
                class="page-btn"
                :disabled="modelPage === 1"
                @click="modelPage--"
              >
                <v-icon size="16">mdi-chevron-left</v-icon>
              </button>
              <span class="page-info">{{ modelPage }} / {{ totalPages }}</span>
              <button
                class="page-btn"
                :disabled="modelPage === totalPages"
                @click="modelPage++"
              >
                <v-icon size="16">mdi-chevron-right</v-icon>
              </button>
            </div>
          </template>
        </div>
      </div>

      <!-- Catalog Loading -->
      <div v-else class="panel-loading">
        <v-progress-circular indeterminate color="primary" size="36" width="3" />
        <span class="loading-text">Katalog yükleniyor...</span>
      </div>
    </div>

    <!-- Sağ: Kayıtlı Makinelerim -->
    <div class="panel my-panel">
      <div class="panel-header">
        <div class="panel-header-left">
          <div class="panel-icon my-icon">
            <v-icon size="20" color="white">mdi-check-circle</v-icon>
          </div>
          <div>
            <h2 class="panel-title">Kayıtlı Makinelerim</h2>
            <p class="panel-subtitle">{{ myMachines.length }} makine kayıtlı</p>
          </div>
        </div>
      </div>

      <div v-if="isMyLoading" class="panel-loading">
        <v-progress-circular indeterminate color="primary" size="36" width="3" />
        <span class="loading-text">Makineler yükleniyor...</span>
      </div>

      <div v-else-if="myMachines.length === 0" class="panel-empty my-empty">
        <div class="empty-icon-wrapper">
          <v-icon size="48" color="primary">mdi-robot-industrial-outline</v-icon>
        </div>
        <p class="empty-title">Henüz makine eklenmedi</p>
        <p class="empty-text">Sol taraftaki katalogdan marka ve model seçerek listenize ekleyin</p>
      </div>

      <div v-else class="my-machines-grid">
        <div v-for="um in myMachines" :key="um.id" class="machine-card">
          <div class="machine-card-icon">
            <v-icon size="24" color="white">mdi-robot-industrial</v-icon>
          </div>
          <div class="machine-card-info">
            <span class="machine-card-name">{{ um.brand }} {{ um.model }}</span>
            <span v-if="um.name" class="machine-card-alias">
              <v-icon size="12" class="mr-1">mdi-tag-outline</v-icon>
              {{ um.name }}
            </span>
          </div>
          <button class="remove-btn" :disabled="removingId === um.id" @click="confirmRemove(um)">
            <v-progress-circular v-if="removingId === um.id" indeterminate size="14" width="2" color="error" />
            <v-icon v-else size="18">mdi-close</v-icon>
          </button>
        </div>
      </div>
    </div>
  </div>

  <!-- Add Dialog -->
  <v-dialog v-model="addDialog" max-width="460" persistent>
    <v-card class="add-dialog-card" rounded="xl">
      <div class="dialog-header">
        <div class="dialog-header-icon">
          <v-icon size="28" color="white">mdi-robot-industrial</v-icon>
        </div>
        <div>
          <h3 class="dialog-title">Makine Ekle</h3>
          <p class="dialog-subtitle">{{ selectedCatalogMachine?.brand }} · {{ selectedCatalogMachine?.model }}</p>
        </div>
        <button class="dialog-close" @click="addDialog = false">
          <v-icon size="20">mdi-close</v-icon>
        </button>
      </div>
      <div class="dialog-body">
        <p class="dialog-hint">
          <v-icon size="16" class="mr-1" color="primary">mdi-information-outline</v-icon>
          Birden fazla aynı model makineniz varsa ayırt etmek için özel bir isim verebilirsiniz.
        </p>
        <v-text-field
          v-model="newMachineName"
          label="Makine Adı (opsiyonel)"
          placeholder="Örn: Fabrika 1 - Torna"
          variant="outlined"
          density="comfortable"
          prepend-inner-icon="mdi-tag-outline"
          hide-details="auto"
          :maxlength="150"
          class="modern-input"
        />
      </div>
      <div class="dialog-actions">
        <v-btn variant="outlined" size="large" class="btn-gradient-dark" :disabled="isAdding" @click="addDialog = false">İptal</v-btn>
        <v-btn size="large" class="btn-gradient-primary" :loading="isAdding" @click="handleAdd">
          <v-icon start>mdi-plus</v-icon>Listeye Ekle
        </v-btn>
      </div>
    </v-card>
  </v-dialog>

  <!-- Remove Confirm -->
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
import { ref, computed, watch, onMounted } from 'vue'
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

const brandSearch = ref('')
const modelSearch = ref('')
const selectedBrand = ref<string | null>(null)
const modelPage = ref(1)
const modelsPerPage = 20

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

//#region Computed — Brand Groups
const brandGroups = computed(() => {
  const map = new Map<string, Machine[]>()
  for (const m of catalog.value) {
    if (!map.has(m.brand)) map.set(m.brand, [])
    map.get(m.brand)!.push(m)
  }
  return Array.from(map.entries())
    .map(([brand, models]) => ({ brand, models }))
    .sort((a, b) => a.brand.localeCompare(b.brand))
})

const filteredBrandGroups = computed(() => {
  const q = brandSearch.value?.toLowerCase().trim()
  if (!q) return brandGroups.value
  return brandGroups.value.filter(bg => bg.brand.toLowerCase().includes(q))
})

const currentBrandModels = computed(() => {
  if (!selectedBrand.value) return []
  return brandGroups.value.find(bg => bg.brand === selectedBrand.value)?.models ?? []
})

const filteredModels = computed(() => {
  const q = modelSearch.value?.toLowerCase().trim()
  if (!q) return currentBrandModels.value
  return currentBrandModels.value.filter(m => m.model.toLowerCase().includes(q))
})

const totalPages = computed(() => Math.ceil(filteredModels.value.length / modelsPerPage))

const paginatedModels = computed(() => {
  const start = (modelPage.value - 1) * modelsPerPage
  return filteredModels.value.slice(start, start + modelsPerPage)
})
//#endregion

//#region Helpers
const isAlreadyAdded = (machineId: number) =>
  myMachines.value.some(um => um.machineId === machineId)

const addedCountByBrand = (brand: string) =>
  myMachines.value.filter(um => um.brand === brand).length

const selectBrand = (brand: string) => {
  selectedBrand.value = brand
  modelSearch.value = ''
  modelPage.value = 1
}
//#endregion

// Arama değişince sayfayı sıfırla
watch(modelSearch, () => { modelPage.value = 1 })

//#region Actions
const openAddDialog = (machine: Machine) => {
  selectedCatalogMachine.value = machine
  newMachineName.value = ''
  addDialog.value = true
}

const handleAdd = async () => {
  if (!selectedCatalogMachine.value) return
  isAdding.value = true
  addingId.value = selectedCatalogMachine.value.id
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
    addingId.value = null
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
    // İlk markayı otomatik seç
    if (brandGroups.value.length > 0) {
      selectedBrand.value = brandGroups.value[0].brand
    }
  } finally {
    isCatalogLoading.value = false
    isMyLoading.value = false
  }
})
//#endregion
</script>

<style scoped>
/* ── Page Header ─────────────────────────────── */
.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  flex-wrap: wrap;
}
.page-header-left { display: flex; align-items: center; gap: 16px; }
.page-header-icon {
  width: 52px; height: 52px;
  background: var(--theme-gradient, linear-gradient(135deg, #4338ca 0%, #2563eb 100%));
  border-radius: 14px;
  display: flex; align-items: center; justify-content: center;
  box-shadow: 0 8px 16px rgba(37,99,235,0.25);
  flex-shrink: 0;
}
.page-title { font-size: 1.5rem; font-weight: 700; color: #0f172a; margin: 0; line-height: 1.2; }
.page-subtitle { font-size: 0.875rem; color: #64748b; margin: 4px 0 0; }
.page-header-stats { display: flex; gap: 12px; }
.stat-card {
  display: flex; flex-direction: column; align-items: center;
  background: white; border: 1px solid #e2e8f0; border-radius: 12px;
  padding: 12px 20px; min-width: 72px;
  box-shadow: 0 1px 3px rgba(0,0,0,0.05);
}
.stat-number { font-size: 1.5rem; font-weight: 700; color: var(--theme-primary, #2563eb); line-height: 1; }
.stat-label { font-size: 0.7rem; color: #94a3b8; margin-top: 4px; text-transform: uppercase; letter-spacing: 0.5px; font-weight: 500; }

/* ── Layout ──────────────────────────────────── */
.machines-layout {
  display: grid;
  grid-template-columns: 1fr 1.3fr;
  gap: 20px;
  align-items: start;
}

/* ── Panel ───────────────────────────────────── */
.panel {
  background: white;
  border: 1px solid #e2e8f0;
  border-radius: 16px;
  overflow: hidden;
  box-shadow: 0 1px 3px rgba(0,0,0,0.05);
}
.panel-header {
  display: flex; align-items: center; justify-content: space-between;
  padding: 16px 20px;
  border-bottom: 1px solid #f1f5f9;
}
.panel-header-left { display: flex; align-items: center; gap: 12px; }
.panel-icon {
  width: 38px; height: 38px; border-radius: 10px;
  display: flex; align-items: center; justify-content: center; flex-shrink: 0;
}
.catalog-icon {
  background: linear-gradient(135deg, #6366f1 0%, #4f46e5 100%);
  box-shadow: 0 4px 8px rgba(99,102,241,0.25);
}
.my-icon {
  background: linear-gradient(135deg, #10b981 0%, #059669 100%);
  box-shadow: 0 4px 8px rgba(16,185,129,0.25);
}
.panel-title { font-size: 0.95rem; font-weight: 600; color: #0f172a; margin: 0; }
.panel-subtitle { font-size: 0.75rem; color: #94a3b8; margin: 2px 0 0; }

/* ── Loading / Empty ─────────────────────────── */
.panel-loading {
  display: flex; flex-direction: column; align-items: center;
  gap: 12px; padding: 60px 24px;
}
.loading-text { font-size: 0.875rem; color: #94a3b8; }

.panel-empty {
  display: flex; flex-direction: column; align-items: center;
  padding: 48px 24px; text-align: center; color: #94a3b8;
}
.my-empty { padding: 56px 24px; }
.empty-icon-wrapper {
  width: 72px; height: 72px;
  background: linear-gradient(135deg, rgba(99,102,241,0.08) 0%, rgba(37,99,235,0.08) 100%);
  border-radius: 50%;
  display: flex; align-items: center; justify-content: center;
  margin-bottom: 16px;
}
.empty-title { font-size: 0.95rem; font-weight: 600; color: #475569; margin: 0 0 6px; }
.empty-text { font-size: 0.8rem; color: #94a3b8; margin: 0; max-width: 220px; line-height: 1.5; }

/* ── Catalog Split View ──────────────────────── */
.catalog-body {
  display: flex;
  height: 560px;         /* Sabit yükseklik — sayfa hiç uzamaz */
}

/* Marka Şeridi */
.brands-sidebar {
  width: 175px;
  flex-shrink: 0;
  border-right: 1px solid #f1f5f9;
  display: flex;
  flex-direction: column;
  background: #fafafa;
}

.brands-search {
  padding: 10px 10px 6px;
  border-bottom: 1px solid #f1f5f9;
}

.brands-list {
  flex: 1;
  overflow-y: auto;
  padding: 6px;
}
.brands-list::-webkit-scrollbar { width: 3px; }
.brands-list::-webkit-scrollbar-track { background: transparent; }
.brands-list::-webkit-scrollbar-thumb { background: #e2e8f0; border-radius: 2px; }

.brands-empty {
  padding: 20px 8px;
  text-align: center;
  font-size: 0.75rem;
  color: #94a3b8;
}

.brand-item {
  width: 100%;
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 8px;
  border-radius: 8px;
  border: none;
  background: transparent;
  cursor: pointer;
  text-align: left;
  transition: all 0.15s ease;
  margin-bottom: 2px;
}
.brand-item:hover { background: #eff6ff; }
.brand-item.active {
  background: var(--theme-gradient, linear-gradient(135deg, #4338ca 0%, #2563eb 100%));
}

.brand-item-avatar {
  width: 26px; height: 26px;
  border-radius: 7px;
  background: rgba(99,102,241,0.12);
  color: #4f46e5;
  font-size: 0.75rem;
  font-weight: 700;
  display: flex; align-items: center; justify-content: center;
  flex-shrink: 0;
  transition: all 0.15s ease;
}
.brand-item.active .brand-item-avatar {
  background: rgba(255,255,255,0.2);
  color: white;
}

.brand-item-info { flex: 1; min-width: 0; }
.brand-item-name {
  display: block;
  font-size: 0.8rem;
  font-weight: 600;
  color: #1e293b;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  transition: color 0.15s ease;
}
.brand-item.active .brand-item-name { color: white; }

.brand-item-meta {
  display: block;
  font-size: 0.65rem;
  color: #94a3b8;
  transition: color 0.15s ease;
}
.brand-item.active .brand-item-meta { color: rgba(255,255,255,0.7); }

.brand-item-badges {
  display: flex;
  align-items: center;
  gap: 3px;
  flex-shrink: 0;
}
.brand-added-dot {
  min-width: 18px;
  height: 18px;
  padding: 0 4px;
  border-radius: 9px;
  background: #10b981;
  color: white;
  font-size: 0.6rem;
  font-weight: 700;
  display: flex; align-items: center; justify-content: center;
}

/* Model Alanı */
.models-area {
  flex: 1;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.models-placeholder {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  color: #94a3b8;
  text-align: center;
  padding: 24px;
}

.models-search {
  padding: 10px 12px 6px;
  border-bottom: 1px solid #f1f5f9;
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.models-meta {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 2px;
}
.models-count { font-size: 0.7rem; color: #94a3b8; font-weight: 500; }
.models-added-count {
  font-size: 0.7rem;
  color: #10b981;
  font-weight: 600;
  display: flex;
  align-items: center;
  gap: 3px;
}

.models-empty {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  color: #94a3b8;
  font-size: 0.8rem;
  gap: 4px;
}

.models-list {
  flex: 1;
  overflow-y: auto;
  padding: 6px 8px;
}
.models-list::-webkit-scrollbar { width: 4px; }
.models-list::-webkit-scrollbar-track { background: transparent; }
.models-list::-webkit-scrollbar-thumb { background: #e2e8f0; border-radius: 2px; }

.model-item {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 7px 10px;
  border-radius: 8px;
  border-left: 2px solid transparent;
  transition: all 0.15s ease;
  margin-bottom: 1px;
}
.model-item:hover:not(.model-item--added) {
  background: #f8faff;
  border-left-color: var(--theme-primary, #2563eb);
}
.model-item--added {
  background: #f0fdf4;
  border-left-color: #10b981;
}

.model-item-icon {
  width: 24px; height: 24px;
  background: #f1f5f9;
  border-radius: 6px;
  display: flex; align-items: center; justify-content: center;
  flex-shrink: 0;
}
.model-item--added .model-item-icon { background: #dcfce7; }

.model-item-name {
  flex: 1;
  font-size: 0.82rem;
  font-weight: 500;
  color: #334155;
}
.model-item--added .model-item-name { color: #166534; }

.model-item-action { flex-shrink: 0; }

.add-btn {
  width: 24px; height: 24px;
  border-radius: 6px;
  background: var(--theme-gradient, linear-gradient(135deg, #4338ca 0%, #2563eb 100%));
  border: none; cursor: pointer;
  display: flex; align-items: center; justify-content: center;
  color: white;
  transition: all 0.2s ease;
  box-shadow: 0 2px 6px rgba(37,99,235,0.3);
}
.add-btn:hover:not(:disabled) { transform: scale(1.15); box-shadow: 0 4px 10px rgba(37,99,235,0.4); }
.add-btn:disabled { opacity: 0.6; cursor: not-allowed; }

/* Sayfalama */
.models-pagination {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  padding: 8px 12px;
  border-top: 1px solid #f1f5f9;
  background: #fafafa;
}
.page-btn {
  width: 28px; height: 28px;
  border-radius: 7px;
  border: 1px solid #e2e8f0;
  background: white;
  display: flex; align-items: center; justify-content: center;
  cursor: pointer;
  color: #475569;
  transition: all 0.15s ease;
}
.page-btn:hover:not(:disabled) { background: #eff6ff; border-color: var(--theme-primary, #2563eb); color: var(--theme-primary, #2563eb); }
.page-btn:disabled { opacity: 0.35; cursor: not-allowed; }
.page-info { font-size: 0.78rem; font-weight: 600; color: #475569; min-width: 50px; text-align: center; }

/* ── My Machine Cards ────────────────────────── */
.my-machines-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(210px, 1fr));
  gap: 10px;
  padding: 14px 16px 16px;
}
.machine-card {
  display: flex; align-items: center; gap: 12px;
  padding: 12px;
  background: linear-gradient(135deg, #f8faff 0%, #f0f4ff 100%);
  border: 1.5px solid #dbeafe;
  border-radius: 12px;
  transition: all 0.2s ease;
}
.machine-card:hover {
  border-color: var(--theme-primary, #2563eb);
  box-shadow: 0 4px 12px rgba(37,99,235,0.1);
  transform: translateY(-1px);
}
.machine-card-icon {
  width: 38px; height: 38px;
  background: var(--theme-gradient, linear-gradient(135deg, #4338ca 0%, #2563eb 100%));
  border-radius: 10px;
  display: flex; align-items: center; justify-content: center;
  flex-shrink: 0;
  box-shadow: 0 4px 8px rgba(37,99,235,0.2);
}
.machine-card-info { flex: 1; min-width: 0; display: flex; flex-direction: column; gap: 3px; }
.machine-card-name {
  font-size: 0.82rem; font-weight: 600; color: #1e293b;
  white-space: nowrap; overflow: hidden; text-overflow: ellipsis;
}
.machine-card-alias { font-size: 0.72rem; color: #64748b; display: flex; align-items: center; }
.remove-btn {
  width: 26px; height: 26px;
  border-radius: 7px;
  background: transparent;
  border: 1.5px solid #fca5a5;
  cursor: pointer;
  display: flex; align-items: center; justify-content: center;
  color: #ef4444;
  transition: all 0.2s ease;
  flex-shrink: 0;
}
.remove-btn:hover:not(:disabled) { background: #fef2f2; border-color: #ef4444; }
.remove-btn:disabled { opacity: 0.6; cursor: not-allowed; }

/* ── Add Dialog ──────────────────────────────── */
.add-dialog-card { overflow: hidden; }
.dialog-header {
  display: flex; align-items: center; gap: 14px;
  padding: 20px 24px;
  background: var(--theme-gradient, linear-gradient(135deg, #4338ca 0%, #2563eb 100%));
}
.dialog-header-icon {
  width: 48px; height: 48px;
  background: rgba(255,255,255,0.2); border-radius: 12px;
  display: flex; align-items: center; justify-content: center; flex-shrink: 0;
}
.dialog-title { font-size: 1.1rem; font-weight: 700; margin: 0; color: white; }
.dialog-subtitle { font-size: 0.8rem; margin: 3px 0 0; color: rgba(255,255,255,0.8); }
.dialog-close {
  margin-left: auto;
  background: rgba(255,255,255,0.15); border: none; border-radius: 8px;
  width: 32px; height: 32px;
  display: flex; align-items: center; justify-content: center;
  cursor: pointer; color: white; transition: background 0.2s ease; flex-shrink: 0;
}
.dialog-close:hover { background: rgba(255,255,255,0.25); }
.dialog-body { padding: 24px 24px 16px; display: flex; flex-direction: column; gap: 16px; }
.dialog-hint {
  font-size: 0.8rem; color: #64748b; margin: 0;
  display: flex; align-items: flex-start; gap: 4px;
  background: #f8fafc; border-radius: 8px; padding: 10px 12px;
  border: 1px solid #e2e8f0;
}
.dialog-actions { display: flex; justify-content: flex-end; gap: 10px; padding: 0 24px 24px; }

/* ── Responsive ──────────────────────────────── */
@media (max-width: 960px) {
  .machines-layout { grid-template-columns: 1fr; }
}

@media (max-width: 600px) {
  .page-header { flex-direction: column; align-items: flex-start; }
  .page-header-stats { width: 100%; }
  .stat-card { flex: 1; }
  .catalog-body { height: 460px; }
  .brands-sidebar { width: 130px; }
  .my-machines-grid { grid-template-columns: 1fr; }
}
</style>
