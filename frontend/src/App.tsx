import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { getProjectHealth } from './features/health';

function App() {
  const [projectId, setProjectId] = useState(1);

  const { data, error, isLoading, isFetching } = useQuery({
    queryKey: ['project-health', projectId],
    queryFn: () => getProjectHealth(projectId),
  });

  return (
    <div className="mx-auto max-w-xl p-8">
      <h1 className="text-2xl font-semibold">EssenceMvp — Frontend scaffold</h1>
      <p className="mt-2 text-sm text-gray-500">
        Prueba de conexión: <code>GET /evaluation/health/{'{projectId}'}</code>
      </p>

      <label className="mt-6 block text-sm font-medium">
        Project ID
        <input
          type="number"
          className="mt-1 block w-full rounded border px-3 py-2"
          value={projectId}
          onChange={(e) => setProjectId(Number(e.target.value))}
        />
      </label>

      {isLoading || isFetching ? <p className="mt-4">Cargando...</p> : null}
      {error ? (
        <p className="mt-4 text-red-600">Error: {(error as Error).message}</p>
      ) : null}
      {data ? (
        <pre className="mt-4 overflow-auto rounded bg-gray-100 p-4 text-left text-sm">
          {JSON.stringify(data, null, 2)}
        </pre>
      ) : null}
    </div>
  );
}

export default App;
