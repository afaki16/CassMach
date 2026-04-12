export interface Machine {
  id: number
  userId: number
  brand: string
  model: string
  name?: string
  createdDate: string
}

export interface CreateMachineRequest {
  brand: string
  model: string
  name?: string
}

export interface UpdateMachineRequest {
  brand: string
  model: string
  name?: string
}
