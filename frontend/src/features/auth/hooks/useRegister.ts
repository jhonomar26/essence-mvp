import { useMutation } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { paths } from '../../../app/router/paths';
import { useAuthStore } from '../../../stores/useAuthStore';
import { register } from '../services/authApi';
import type { RegisterPayload } from '../types/auth';

export function useRegister() {
  const navigate = useNavigate();
  const setSession = useAuthStore((state) => state.setSession);

  return useMutation({
    mutationFn: (payload: RegisterPayload) => register(payload),
    onSuccess: (data) => {
      setSession(data.token, data.user);
      navigate(paths.healthDemo);
    },
  });
}
