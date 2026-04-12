import type { Machine, CreateMachineRequest, UpdateMachineRequest } from '~/types'
import { API_ENDPOINTS } from '~/utils/apiEndpoints'

export const useMachines = () => {
  const api = useApi()

  //#region GET - Kullanıcının makineleri
  const getMachines = async () => {
    try {
      const response: any = await api.get<Machine[]>(API_ENDPOINTS.MACHINES.LIST)
      const val = response?.value ?? response?.data ?? response
      if (Array.isArray(val)) return val
      if (Array.isArray(val?.value)) return val.value
      return []
    } catch (error) {
      console.error('Get machines error:', error)
      throw error
    }
  }
  //#endregion

  //#region POST - Makine ekle
  const createMachine = async (data: CreateMachineRequest) => {
    try {
      const response: any = await api.post<Machine>(API_ENDPOINTS.MACHINES.CREATE, data)
      return response?.value ?? response?.data ?? response
    } catch (error) {
      console.error('Create machine error:', error)
      throw error
    }
  }
  //#endregion

  //#region PUT - Makine güncelle
  const updateMachine = async (id: number, data: UpdateMachineRequest) => {
    try {
      const response: any = await api.put<Machine>(API_ENDPOINTS.MACHINES.UPDATE(id), data)
      return response?.value ?? response?.data ?? response
    } catch (error) {
      console.error('Update machine error:', error)
      throw error
    }
  }
  //#endregion

  //#region DELETE - Makine sil
  const deleteMachine = async (id: number) => {
    try {
      await api.delete(API_ENDPOINTS.MACHINES.DELETE(id))
      return true
    } catch (error) {
      console.error('Delete machine error:', error)
      throw error
    }
  }
  //#endregion

  return {
    getMachines,
    createMachine,
    updateMachine,
    deleteMachine,
  }
}
