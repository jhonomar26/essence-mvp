export type AlphaDetail = {
  alphaId: number;
  alphaName: string;
  currentStateNumber: number;
  maxStateNumber: number;
  progress: number;
};

export type HealthResult = {
  healthScore: number;
  classification: string;
  averageProgress: number;
  progressDispersion: number;
  alphaDetails: AlphaDetail[];
};
