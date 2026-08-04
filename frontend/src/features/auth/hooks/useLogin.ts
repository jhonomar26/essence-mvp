import { useMutation } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { paths } from '../../../app/router/paths';
import { useAuthStore } from '../../../stores/useAuthStore';
import { login } from '../services/authApi';
import type { LoginPayload } from '../types/auth';

export function useLogin() {
  const navigate = useNavigate();
  const setSession = useAuthStore((state) => state.setSession);

  return useMutation({
    mutationFn: (payload: LoginPayload) => login(payload),
    onSuccess: (data) => {
      setSession(data.token, data.user);
      navigate(paths.healthDemo);
    },
  });
}
