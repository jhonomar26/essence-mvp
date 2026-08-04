import type { ReactNode } from 'react';
import { useNavigate } from 'react-router-dom';
import { AppBar, Box, Button, Container, Toolbar, Typography } from '@mui/material';
import { useAuthStore } from '../../stores/useAuthStore';
import { paths } from '../router/paths';

export function MainLayout({ children }: { children: ReactNode }) {
  const navigate = useNavigate();
  const clear = useAuthStore((state) => state.clear);

  const handleLogout = () => {
    clear();
    navigate(paths.login);
  };

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'grey.100' }}>
      <AppBar position="static" color="default" elevation={1}>
        <Toolbar>
          <Typography variant="h6" component="div" sx={{ flexGrow: 1 }}>
            EssenceMvp
          </Typography>
          <Button onClick={handleLogout}>Cerrar sesión</Button>
        </Toolbar>
      </AppBar>
      <Container sx={{ py: 4 }}>{children}</Container>
    </Box>
  );
}
