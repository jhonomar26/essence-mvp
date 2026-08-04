import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { Link as RouterLink } from 'react-router-dom';
import { Alert, Button, InputAdornment, Link, Stack, TextField, Typography } from '@mui/material';
import EmailOutlinedIcon from '@mui/icons-material/EmailOutlined';
import LockOutlinedIcon from '@mui/icons-material/LockOutlined';
import PersonOutlineIcon from '@mui/icons-material/PersonOutlineOutlined';
import { paths } from '../../../app/router/paths';
import { useRegister } from '../hooks/useRegister';

const registerSchema = z.object({
  email: z.string().email('Email inválido'),
  password: z.string().min(8, 'Mínimo 8 caracteres'),
  displayName: z.string().optional(),
});

type RegisterFormValues = z.infer<typeof registerSchema>;

export function RegisterForm() {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<RegisterFormValues>({ resolver: zodResolver(registerSchema) });
  const registerMutation = useRegister();

  const onSubmit = (values: RegisterFormValues) => registerMutation.mutate(values);

  return (
    <Stack component="form" spacing={3} onSubmit={handleSubmit(onSubmit)}>
      <Stack spacing={0.5}>
        <Typography variant="h5" component="h1">
          Crea tu cuenta
        </Typography>
        <Typography variant="body2" color="text.secondary">
          Regístrate para empezar a usar EssenceMvp
        </Typography>
      </Stack>

      <Stack spacing={2}>
        <TextField
          label="Nombre (opcional)"
          fullWidth
          slotProps={{
            input: {
              startAdornment: (
                <InputAdornment position="start">
                  <PersonOutlineIcon fontSize="small" color="action" />
                </InputAdornment>
              ),
            },
          }}
          {...register('displayName')}
        />

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

      {registerMutation.isError ? (
        <Alert severity="error">No se pudo registrar. Es posible que el email ya esté en uso.</Alert>
      ) : null}

      <Button type="submit" variant="contained" size="large" fullWidth disabled={registerMutation.isPending}>
        {registerMutation.isPending ? 'Creando cuenta...' : 'Registrarme'}
      </Button>

      <Typography variant="body2" color="text.secondary" sx={{ textAlign: 'center' }}>
        ¿Ya tienes cuenta?{' '}
        <Link component={RouterLink} to={paths.login} underline="hover" sx={{ fontWeight: 600 }}>
          Inicia sesión
        </Link>
      </Typography>
    </Stack>
  );
}
