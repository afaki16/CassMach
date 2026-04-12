// Global makine kataloğu (admin yönetir)
export interface Machine {
  id: number
  brand: string
  model: string
  createdDate: string
}

export interface CreateMachineRequest {
  brand: string
  model: string
}

export interface UpdateMachineRequest {
  brand: string
  model: string
}

// Kullanıcının eşleştirdiği makineler
export interface UserMachine {
  id: number
  machineId: number
  brand: string
  model: string
  name?: string
  createdDate: string
}

export interface AddUserMachineRequest {
  machineId: number
  name?: string
}
