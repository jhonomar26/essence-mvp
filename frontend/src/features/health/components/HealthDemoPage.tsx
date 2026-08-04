import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Alert, Box, CircularProgress, Paper, TextField, Typography } from '@mui/material';
import { getProjectHealth } from '../services/healthApi';

export function HealthDemoPage() {
  const [projectId, setProjectId] = useState(1);

  const { data, error, isLoading, isFetching } = useQuery({
    queryKey: ['project-health', projectId],
    queryFn: () => getProjectHealth(projectId),
  });

  return (
    <Box sx={{ maxWidth: 600, mx: 'auto' }}>
      <Typography variant="h5" component="h1" sx={{ fontWeight: 600 }}>
        EssenceMvp — Frontend scaffold
      </Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
        Prueba de conexión: <code>GET /evaluation/health/{'{projectId}'}</code>
      </Typography>

      <TextField
        label="Project ID"
        type="number"
        fullWidth
        sx={{ mt: 3 }}
        value={projectId}
        onChange={(e) => setProjectId(Number(e.target.value))}
      />

      {isLoading || isFetching ? (
        <Box sx={{ mt: 3, display: 'flex', justifyContent: 'center' }}>
          <CircularProgress size={24} />
        </Box>
      ) : null}

      {error ? (
        <Alert severity="error" sx={{ mt: 3 }}>
          Error: {(error as Error).message}
        </Alert>
      ) : null}

      {data ? (
        <Paper
          variant="outlined"
          component="pre"
          sx={{ mt: 3, p: 2, overflow: 'auto', fontSize: 14, fontFamily: 'monospace' }}
        >
          {JSON.stringify(data, null, 2)}
        </Paper>
      ) : null}
    </Box>
  );
}
