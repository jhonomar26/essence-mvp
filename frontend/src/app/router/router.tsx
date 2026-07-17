import { createBrowserRouter } from 'react-router-dom';
import { MainLayout } from '../layout/MainLayout';
import { paths } from './paths';

export const router = createBrowserRouter([
  {
    path: paths.home,
    element: (
      <MainLayout>
        <></>
      </MainLayout>
    ),
  },
]);
