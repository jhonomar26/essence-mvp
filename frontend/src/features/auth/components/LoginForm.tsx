import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { Link as RouterLink } from 'react-router-dom';
import { Alert, Button, InputAdornment, Link, Stack, TextField, Typography } from '@mui/material';
import EmailOutlinedIcon from '@mui/icons-material/EmailOutlined';
import LockOutlinedIcon from '@mui/icons-material/LockOutlined';
import { paths } from '../../../app/router/paths';
import { useLogin } from '../hooks/useLogin';

const loginSchema = z.object({
  email: z.string().email('Email inválido'),
  password: z.string().min(1, 'La contraseña es obligatoria'),
});

type LoginFormValues = z.infer<typeof loginSchema>;

export function LoginForm() {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginFormValues>({ resolver: zodResolver(loginSchema) });
  const loginMutation = useLogin();

  const onSubmit = (values: LoginFormValues) => loginMutation.mutate(values);

  return (
    <Stack component="form" spacing={3} onSubmit={handleSubmit(onSubmit)}>
      <Stack spacing={0.5}>
        <Typography variant="h5" component="h1">
          Bienvenido de nuevo
        </Typography>
        <Typography variant="body2" color="text.secondary">
          Ingresa tus credenciales para continuar
        </Typography>
      </Stack>

      <Stack spacing={2}>
        <TextField
          label="Email"
          type="email"
          fullWidth
          error={!!errors.email}
          helperText={errors.email?.message}
          slotProps={{
            input: {
              startAdornment: (
                <InputAdornment position="start">
                  <EmailOutlinedIcon fontSize="small" color="action" />
                </InputAdornment>
              ),
            },
          }}
          {...register('email')}
        />

        <TextField
          label="Contraseña"
          type="password"
          fullWidth
          error={!!errors.password}
          helperText={errors.password?.message}
          slotProps={{
            input: {
              startAdornment: (
                <InputAdornment position="start">
                  <LockOutlinedIcon fontSize="small" color="action" />
                </InputAdornment>
              ),
            },
          }}
          {...register('password')}
        />
      </Stack>

      {loginMutation.isError ? <Alert severity="error">Credenciales inválidas.</Alert> : null}

      <Button type="submit" variant="contained" size="large" fullWidth disabled={loginMutation.isPending}>
        {loginMutation.isPending ? 'Ingresando...' : 'Ingresar'}
      </Button>

      <Typography variant="body2" color="text.secondary" sx={{ textAlign: 'center' }}>
        ¿No tienes cuenta?{' '}
        <Link component={RouterLink} to={paths.register} underline="hover" sx={{ fontWeight: 600 }}>
          Regístrate
        </Link>
      </Typography>
    </Stack>
  );
}
