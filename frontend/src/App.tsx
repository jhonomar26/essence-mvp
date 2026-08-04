import { useEffect } from 'react';
import { RouterProvider } from 'react-router-dom';
import { router } from './app/router/router';
import { getMe } from './features/auth';
import { useAuthStore } from './stores/useAuthStore';

function App() {
  useEffect(() => {
    const { token, clear } = useAuthStore.getState();
    if (!token) return;
    getMe().catch(() => clear());
  }, []);

  return <RouterProvider router={router} />;
}

export default App;
