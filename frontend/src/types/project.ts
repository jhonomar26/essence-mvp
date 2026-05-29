export type HealthStatus = 'green' | 'yellow' | 'red'

export interface ProjectSummary {
  id: number
  name: string
  phase?: string
  createdAt: string
  overallHealth: HealthStatus
}

export interface AlphaProgress {
  alphaId: number
  alphaName: string
  areaOfConcern: string
  currentStateNumber: number
  maxStateNumber: number
  currentStateName: string
  progress: number
}

export interface ProjectDetail {
  project: {
    id: number
    name: string
    description?: string
    phase?: string
    overallHealth: HealthStatus
    healthScore: number
    healthClassification: string
    averageProgress: number
    progressDispersion: number
    alphaProgress: AlphaProgress[]
  }
  alphaChecklists: unknown[]
  recentSnapshots: unknown[]
}

export interface CreateProjectDto {
  name: string
  description?: string
  phase?: string
}
