<template>
  <div class="mb-6">
    <BreadCrumb :items="[
      { text: 'Ana Sayfa', to: '/' },
      { text: 'Makine Kataloğu' }
    ]" />
  </div>

  <BaseDataTable
    :items="items"
    :columns="tableColumns"
    title="Makine Kataloğu"
    toolbar-icon="mdi-robot-industrial"
    search-placeholder="Marka veya model ara..."
    add-button-text="Makine Ekle"
    :loading="isLoading"
    loading-text="Makineler yükleniyor..."
    empty-title="Henüz makine eklenmedi"
    :show-add-button="true"
    :show-actions="true"
    :show-view-button="false"
    :show-edit-button="true"
    :show-delete-button="true"
    @add="openCreateDialog"
    @edit="openEditDialog"
    @delete="openDeleteDialog"
    @search="handleSearch"
    @refresh="refreshData"
  />

  <!-- Create/Edit Drawer -->
  <ResizableDrawer
    v-model="dialogs.create"
    :title="isEditMode ? 'Makine Düzenle' : 'Makine Ekle'"
    icon="mdi-robot-industrial"
    :default-width="420"
    :min-width="350"
  >
    <MachineForm
      :machine="selectedItem"
      :loading="isLoading"
      @submit="handleSubmit"
      @cancel="closeCreateDialog"
    />
  </ResizableDrawer>

  <!-- Delete Confirm -->
  <ConfirmDialog
    v-model="dialogs.delete"
    title="Makineyi Sil"
    :message="`'${itemToDelete?.brand} ${itemToDelete?.model}' makinesini katalogdan silmek istediğinizden emin misiniz?`"
    type="error"
    confirm-text="Sil"
    :loading="isDeleting"
    @confirm="confirmDelete"
    @cancel="closeDeleteDialog"
  />
</template>

<script setup lang="ts">
import { onMounted } from 'vue'
import BaseDataTable from '~/components/UI/BaseDataTable.vue'
import MachineForm from '~/components/Machines/MachineForm.vue'
import ConfirmDialog from '~/components/UI/ConfirmDialog.vue'

//#region Page Metadata
definePageMeta({
  middleware: ['auth', 'permission'],
  permission: 'Machines.Read'
})

useHead({ title: 'Makine Kataloğu - CassMach' })
//#endregion

//#region DataTable Columns
const tableColumns = [
  { label: 'Marka', key: 'brand', sortable: true, filterable: true, filterType: 'text', width: '200px' },
  { label: 'Model', key: 'model', sortable: true, filterable: true, filterType: 'text', width: '200px' },
]
//#endregion

//#region Composables
const { getMachines, createMachine, updateMachine, deleteMachine } = useMachines()

const {
  items,
  isLoading,
  isDeleting,
  dialogs,
  selectedItem,
  itemToDelete,
  isEditMode,
  openCreateDialog,
  openEditDialog,
  openDeleteDialog,
  closeCreateDialog,
  closeDeleteDialog,
  handleSubmit,
  confirmDelete,
  handleSearch,
  loadItemsData,
  refreshData
} = useCrudOperations({
  loadItems: getMachines,
  createItem: createMachine,
  updateItem: (id: number, data: any) => updateMachine(id, data),
  deleteItem: (id: number) => deleteMachine(id),
  itemName: 'Makine'
})
//#endregion

//#region Lifecycle
onMounted(async () => {
  await loadItemsData()
})
//#endregion
</script>
