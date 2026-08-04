import { createBrowserRouter, Navigate } from 'react-router-dom';
import { LoginForm, RegisterForm } from '../../features/auth';
import { HealthDemoPage } from '../../features/health';
import { AuthLayout } from '../layout/AuthLayout';
import { MainLayout } from '../layout/MainLayout';
import { paths } from './paths';
import { ProtectedRoute } from './ProtectedRoute';

export const router = createBrowserRouter([
  {
    path: paths.home,
    element: <Navigate to={paths.healthDemo} replace />,
  },
  {
    path: paths.login,
    element: (
      <AuthLayout>
        <LoginForm />
      </AuthLayout>
    ),
  },
  {
    path: paths.register,
    element: (
      <AuthLayout>
        <RegisterForm />
      </AuthLayout>
    ),
  },
  {
    path: paths.healthDemo,
    element: (
      <ProtectedRoute>
        <MainLayout>
          <HealthDemoPage />
        </MainLayout>
      </ProtectedRoute>
    ),
  },
]);
