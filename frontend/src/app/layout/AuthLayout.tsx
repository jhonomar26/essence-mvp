import type { ReactNode } from 'react';
import { Box, Paper, Stack, Typography } from '@mui/material';

export function AuthLayout({ children }: { children: ReactNode }) {
  return (
    <Box
      sx={{
        minHeight: '100vh',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        p: 2,
        background: 'linear-gradient(135deg, #12309C 0%, #1E4DE0 45%, #5C82F0 100%)',
      }}
    >
      <Stack spacing={3} sx={{ width: '100%', maxWidth: 420, alignItems: 'center' }}>
        <Stack spacing={0.5} sx={{ alignItems: 'center' }}>
          <Box
            sx={{
              width: 48,
              height: 48,
              borderRadius: '14px',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              bgcolor: 'rgba(255, 255, 255, 0.15)',
              border: '1px solid rgba(255, 255, 255, 0.25)',
            }}
          >
            <Typography sx={{ color: 'common.white', fontWeight: 700, fontSize: 20 }}>E</Typography>
          </Box>
          <Typography variant="h6" sx={{ color: 'common.white', fontWeight: 700 }}>
            EssenceMvp
          </Typography>
        </Stack>

        <Paper
          elevation={0}
          sx={{
            width: '100%',
            p: { xs: 3, sm: 4.5 },
            borderRadius: 4,
            boxShadow: '0 24px 48px -12px rgba(30, 27, 46, 0.35)',
          }}
        >
          {children}
        </Paper>
      </Stack>
    </Box>
  );
}
