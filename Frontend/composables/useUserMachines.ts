import type { UserMachine, AddUserMachineRequest } from '~/types'
import { API_ENDPOINTS } from '~/utils/apiEndpoints'

export const useUserMachines = () => {
  const api = useApi()

  //#region GET - Kullanıcının eşleştirdiği makineler
  const getMyMachines = async () => {
    try {
      const response: any = await api.get<UserMachine[]>(API_ENDPOINTS.USER_MACHINES.LIST)
      const val = response?.value ?? response?.data ?? response
      if (Array.isArray(val)) return val
      if (Array.isArray(val?.value)) return val.value
      return []
    } catch (error) {
      console.error('Get user machines error:', error)
      throw error
    }
  }
  //#endregion

  //#region POST - Katalogdan makine ekle (eşleştir)
  const addMachine = async (data: AddUserMachineRequest) => {
    try {
      const response: any = await api.post<UserMachine>(API_ENDPOINTS.USER_MACHINES.ADD, data)
      return response?.value ?? response?.data ?? response
    } catch (error) {
      console.error('Add user machine error:', error)
      throw error
    }
  }
  //#endregion

  //#region DELETE - Listeden makine kaldır
  const removeMachine = async (id: number) => {
    try {
      await api.delete(API_ENDPOINTS.USER_MACHINES.REMOVE(id))
      return true
    } catch (error) {
      console.error('Remove user machine error:', error)
      throw error
    }
  }
  //#endregion

  return {
    getMyMachines,
    addMachine,
    removeMachine,
  }
}
