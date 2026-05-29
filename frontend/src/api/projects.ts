import api from './client'
import type { ProjectSummary, ProjectDetail, CreateProjectDto } from '../types/project'

export const getProjects = () =>
  api.get<ProjectSummary[]>('/projects')

export const getProject = (id: number) =>
  api.get<ProjectDetail>(`/projects/${id}`)

export const createProject = (data: CreateProjectDto) =>
  api.post('/projects', data)

export const deleteProject = (id: number) =>
  api.delete(`/projects/${id}`)
