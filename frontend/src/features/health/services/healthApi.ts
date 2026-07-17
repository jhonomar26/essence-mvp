import { apiClient } from '../../../shared/api/client';
import type { HealthResult } from '../types/health';

export async function getProjectHealth(projectId: number): Promise<HealthResult> {
  const { data } = await apiClient.get<HealthResult>(`/evaluation/health/${projectId}`);
  return data;
}
